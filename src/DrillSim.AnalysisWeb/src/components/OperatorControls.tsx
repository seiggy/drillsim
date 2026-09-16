import { useEffect, useRef, useState } from 'react'
import {
  getOperatorSession, getOperatorView, getOperatorSetup, operatorActorKey, operatorAttemptConfirmed, operatorAttemptFailed, operatorAttemptKey,
  operatorAuditError, operatorOutcomeKey, readOperatorAttempt, sendOperatorAttempt,
  completionApprovalBlock, completionAuditError, completionReviewKey, completionRetryBlock, retainCompletionReview,
  publicationRecoveryBlock, publicationRecoveryKey, publicationRecoveryRetryBlock, retainPublicationRecoveryReview,
  recoveryAuditError, recoveryReasonError,
  scoreCorrectionBlock, scoreCorrectionKey, scoreCorrectionRetryBlock, retainScoreCorrectionReview,
} from '../operator'
import type { OperatorAction, OperatorActionResult, OperatorAttempt, OperatorSession, OperatorView } from '../operator'
import type { LifecycleTaskId } from '../sequencer'
import type { Scenario, ScenarioResources } from '../types'
import { OperatorCompletionReview } from './OperatorCompletionReview'
import { OperatorPublicationRecovery } from './OperatorPublicationRecovery'
import { OperatorScoreCorrection } from './OperatorScoreCorrection'
import { SimulatorSetup } from './SimulatorSetup'
import { setupRetryBlock, simulatorSetupBlock, type SimulatorSettings, type SimulatorSetup as SetupView } from '../simulatorSetup'
import { scenarioStatusLabel } from '../selection'

type LocalOperatorResult = OperatorActionResult & { clientAttemptKey?: string }

export function OperatorControls({ scenario, resources, taskId, onView, onSetup, onArtifactsChanged, onNavigate }: {
  scenario: Scenario
  resources: ScenarioResources
  taskId: LifecycleTaskId
  onView: (view: OperatorView | undefined) => void
  onSetup?: (setup: SetupView | undefined) => void
  onArtifactsChanged: () => void
  onNavigate?: (task: LifecycleTaskId) => void
}) {
  const [initial] = useState(() => {
    try {
      const text = sessionStorage.getItem(operatorAttemptKey(scenario.scenarioId))
      return { attempt: text ? readOperatorAttempt(text, scenario.scenarioId) : undefined, error: '' }
    } catch (error) { return { attempt: undefined, error: String(error) } }
  })
  const [session, setSession] = useState<OperatorSession>()
  const [view, setView] = useState<OperatorView>()
  const [setup, setSetup] = useState<SetupView>()
  const [setupError, setSetupError] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [storageError, setStorageError] = useState(initial.error)
  const [actor, setActor] = useState(() => {
    if (initial.attempt) return initial.attempt.actor
    try { return sessionStorage.getItem(operatorActorKey(scenario.scenarioId)) ?? '' }
    catch { return '' }
  })
  const [pending, setPending] = useState<OperatorAttempt | undefined>(initial.attempt)
  const [busy, setBusy] = useState(false)
  const [result, setResult] = useState<LocalOperatorResult | undefined>(() => {
    try {
      const text = sessionStorage.getItem(operatorOutcomeKey(scenario.scenarioId))
      const value = text ? JSON.parse(text) as LocalOperatorResult : undefined
      return value?.scenarioId === scenario.scenarioId && typeof value.outcome === 'string' &&
        typeof value.revealSucceeded === 'boolean' ? value : undefined
    } catch { return undefined }
  })
  const [reviewedSeal, setReviewedSeal] = useState('')
  const [reviewedPublication, setReviewedPublication] = useState('')
  const [reviewedCompletion, setReviewedCompletion] = useState<{ key: string; openingsHash: string }>()
  const [recoveryReason, setRecoveryReason] = useState(initial.attempt?.action === 'recover-publication' ? initial.attempt.reason ?? '' : '')
  const [reviewedRecovery, setReviewedRecovery] = useState<{ key: string; hash: string }>()
  const [correctionReason, setCorrectionReason] = useState(initial.attempt?.action === 'correct-score' ? initial.attempt.reason ?? '' : '')
  const [reviewedCorrection, setReviewedCorrection] = useState<{ key: string; hash: string }>()
  const [readAttemptKey, setReadAttemptKey] = useState('')
  const [reviewedAttemptKey, setReviewedAttemptKey] = useState('')
  const active = useRef(true)
  const pendingRef = useRef(pending)
  const readController = useRef<AbortController | undefined>(undefined)
  const lastArtifactState = useRef('')
  pendingRef.current = pending
  const callbacks = useRef({ onView, onSetup, onArtifactsChanged })
  callbacks.current = { onView, onSetup, onArtifactsChanged }

  async function refresh() {
    const controller = new AbortController()
    readController.current?.abort()
    readController.current = controller
    const attemptAtRead = pendingRef.current?.key
    setLoading(true)
    setReviewedAttemptKey('')
    try {
      const wantsSetup = taskId === 'simulation' || pendingRef.current?.action === 'setup'
      const [nextSession, nextView, nextSetup] = await Promise.all([
        getOperatorSession(controller.signal),
        getOperatorView(scenario.scenarioId, controller.signal),
        wantsSetup ? getOperatorSetup(scenario.scenarioId, controller.signal)
          .then(value => ({ value, error: '' })).catch(reason => ({ value: undefined, error: String(reason) })) : undefined,
      ])
      if (!active.current || controller.signal.aborted) return
      setSession(nextSession)
      setView(nextView)
      if (nextSetup) {
        setSetup(nextSetup.value)
        setSetupError(nextSetup.error)
        callbacks.current.onSetup?.(nextSetup.value)
      }
      setReviewedCompletion((reviewed) => reviewed && retainCompletionReview(reviewed.key, nextView) ? reviewed : undefined)
      setReviewedRecovery(reviewed => reviewed && retainPublicationRecoveryReview(reviewed.key, nextView) ? reviewed : undefined)
      setReviewedCorrection(reviewed => reviewed && retainScoreCorrectionReview(reviewed.key, nextView) ? reviewed : undefined)
      callbacks.current.onView(nextView)
      const artifactState = JSON.stringify([nextView.scenarioStatus, nextView.prediction, nextView.run?.status,
        nextView.run?.currentStage, nextView.completion.status])
      if (lastArtifactState.current && lastArtifactState.current !== artifactState) callbacks.current.onArtifactsChanged()
      lastArtifactState.current = artifactState
      if (nextView.enabled && attemptAtRead && attemptAtRead === pendingRef.current?.key) setReadAttemptKey(attemptAtRead)
      setError('')
    } catch (reason) {
      if (active.current && !controller.signal.aborted) {
        setSession(undefined)
        setView(undefined)
        callbacks.current.onView(undefined)
        setError(reason instanceof Error ? reason.message : String(reason))
      }
    } finally {
      if (active.current && !controller.signal.aborted) setLoading(false)
    }
  }

  useEffect(() => {
    active.current = true
    void refresh()
    return () => { active.current = false; readController.current?.abort() }
  }, [scenario.scenarioId, scenario.modifiedUtc, scenario.status, taskId,
    resources.prediction?.revision, resources.prediction?.seal?.sha256, resources.prediction?.approval?.sealedSha256])

  useEffect(() => {
    if (loading || busy || !view?.enabled || !['Queued', 'Running'].includes(view.run?.status ?? '')) return
    const timer = window.setTimeout(() => void refresh(), 3000)
    return () => window.clearTimeout(timer)
  }, [loading, busy, view, scenario.scenarioId])

  const ready = Boolean(session?.enabled && session.csrfRequestToken && session.csrfHeaderName === 'X-DrillSim-CSRF' &&
    view?.enabled && view.scenarioId === scenario.scenarioId)
  const auditError = operatorAuditError(actor)
  const blocked = !ready || loading || busy || Boolean(pending || storageError || auditError)
  const sealHash = view?.prediction.sealHash ?? ''
  const matchingSeal = Boolean(sealHash && resources.prediction?.seal?.sha256 === sealHash)
  const publicationKey = JSON.stringify([scenario.scenarioId, scenario.asOfUtc, view?.run?.runId])
  const definitiveFailure = Boolean(pending && result?.clientAttemptKey === pending.key && operatorAttemptFailed(result))
  const retryReason = completionRetryBlock(view, pending) || publicationRecoveryRetryBlock(view, pending) || scoreCorrectionRetryBlock(view, pending) || setupRetryBlock(view, setup, pending)
  const published = resources.reveal?.status === 'Revealed' || ['Revealed', 'Scored'].includes(view?.scenarioStatus ?? scenario.status) ||
    ['Revealed', 'Scored'].includes(view?.run?.status ?? '')
  const scored = Boolean(resources.scorecard)

  async function perform(attempt: OperatorAttempt) {
    if (!session || busy || loading || !ready) return
    const retryBlock = completionRetryBlock(view, attempt) || publicationRecoveryRetryBlock(view, attempt) || scoreCorrectionRetryBlock(view, attempt) || setupRetryBlock(view, setup, attempt)
    if (retryBlock) { setError(retryBlock); return }
    setBusy(true)
    setResult(undefined)
    setError('')
    setReviewedSeal('')
    setReviewedPublication('')
    setReviewedCompletion(undefined)
    setReviewedRecovery(undefined)
    setReviewedCorrection(undefined)
    setReviewedAttemptKey('')
    try {
      sessionStorage.setItem(operatorAttemptKey(scenario.scenarioId), JSON.stringify(attempt))
    } catch (reason) {
      setStorageError(`Action not sent: its stable retry key could not be saved locally. ${String(reason)}`)
      setBusy(false)
      return
    }
    setPending(attempt)
    pendingRef.current = attempt
    try {
      const outcome = await sendOperatorAttempt(session, attempt)
      const localOutcome = { ...outcome, clientAttemptKey: attempt.key }
      try { sessionStorage.setItem(operatorOutcomeKey(scenario.scenarioId), JSON.stringify(localOutcome)) }
      catch { /* The stable attempt record is retained until confirmation even if response-history storage fails. */ }
      if (operatorAttemptConfirmed(outcome)) {
        try {
          sessionStorage.removeItem(operatorAttemptKey(scenario.scenarioId))
          pendingRef.current = undefined
          if (active.current) { setPending(undefined); setStorageError('') }
        } catch (reason) {
          if (active.current) setStorageError(`The server confirmed this action, but the local retry record could not be removed. ${String(reason)}`)
        }
      }
      if (active.current) setResult(localOutcome)
    } catch (reason) {
      if (active.current) setError(`${reason instanceof Error ? reason.message : String(reason)} The attempt key is retained; no rollback is assumed.`)
    } finally {
      if (active.current) {
        setBusy(false)
        void refresh()
        callbacks.current.onArtifactsChanged()
      }
    }
  }

  function start(action: OperatorAction, settings?: SimulatorSettings) {
    if (blocked || !view) return
    const availability = action === 'approve-prediction' ? view.actions.approvePrediction
      : action === 'approve-completion' ? view.actions.approveCompletion
      : action === 'recover-publication' ? view.actions.recoverPublication
      : action === 'correct-score' ? view.actions.correctScore
      : action === 'setup' ? { enabled: setup?.available } : view.actions[action]
    if (!availability?.enabled || (!['approve-prediction', 'setup', 'start'].includes(action) && !view.run)) return
    if (action === 'approve-prediction' && (!matchingSeal || reviewedSeal !== sealHash)) return
    if (action === 'publish' && reviewedPublication !== publicationKey) return
    if (action === 'setup' && (!settings || simulatorSetupBlock(setup, view, scenario.scenarioId, actor, settings))) return
    if (action === 'approve-completion' && (completionApprovalBlock(view, scenario.scenarioId, scenario.reservoirName) ||
      completionAuditError(actor) || reviewedCompletion?.key !== completionReviewKey(view))) return
    if (action === 'recover-publication' && (publicationRecoveryBlock(view, scenario.scenarioId) ||
      recoveryAuditError(actor) || recoveryReasonError(recoveryReason) || reviewedRecovery?.key !== publicationRecoveryKey(view))) return
    if (action === 'correct-score' && (scoreCorrectionBlock(view, scenario.scenarioId) ||
      recoveryAuditError(actor) || recoveryReasonError(correctionReason) || reviewedCorrection?.key !== scoreCorrectionKey(view))) return
    if (action === 'cancel' && !window.confirm('Cancel this simulation run? This does not undo evidence that was already published.')) return
    const attempt: OperatorAttempt = {
      version: 1, scenarioId: scenario.scenarioId, action, key: crypto.randomUUID(), actor: actor.trim(),
      ...(!['approve-prediction', 'setup', 'start'].includes(action) && view.run ? { runId: view.run.runId } : {}),
      ...(action === 'approve-prediction' ? { reviewedSealHash: sealHash } : {}),
      ...(action === 'setup' ? { ...settings!, reviewedSealHash: setup!.reviewedSealHash! } : {}),
      ...(action === 'approve-completion' ? { reviewedOpeningHash: reviewedCompletion!.openingsHash } : {}),
      ...(action === 'recover-publication' ? { reviewedPublicationHash: reviewedRecovery!.hash, reason: recoveryReason.trim() } : {}),
      ...(action === 'correct-score' ? { reviewedCorrectionHash: reviewedCorrection!.hash, reason: correctionReason.trim() } : {}),
    }
    void perform(attempt)
  }

  function changeActor(value: string) {
    setActor(value)
    setReviewedSeal('')
    setReviewedPublication('')
    setReviewedCompletion(undefined)
    setReviewedRecovery(undefined)
    setReviewedCorrection(undefined)
    try { sessionStorage.setItem(operatorActorKey(scenario.scenarioId), value) }
    catch { /* The actual action still requires durable attempt storage before it is sent. */ }
  }

  function retireAttempt() {
    if (busy || loading || !view?.enabled || (pending && reviewedAttemptKey !== pending.key)) return
    if (!window.confirm('Remove only the local retry record after checking the current run and ledger? This does not cancel, roll back, or repeat any server action.')) return
    try {
      sessionStorage.removeItem(operatorAttemptKey(scenario.scenarioId))
      setPending(undefined)
      pendingRef.current = undefined
      setStorageError('')
      setError('')
      setReviewedAttemptKey('')
    } catch (reason) { setStorageError(String(reason)) }
  }

  return <section className="operator-controls" aria-labelledby="operator-title">
    <header className="operator-heading">
      <div><h3 id="operator-title">{taskId === 'prediction' ? 'Approve the saved prediction' : 'Simulation operator'}</h3>
        <p>{published ? 'This scenario has published results. Inspect them below; a different simulation needs a new scenario.'
          : 'Review each step, then choose its action. Opening a task only reads status.'}</p></div>
      <button type="button" disabled={loading || busy} onClick={() => void refresh()}>{loading ? 'Reading operator state…' : 'Refresh operator status'}</button>
    </header>
    <details><summary>Local operator and audit details</summary><p>{session?.auditLabelLimitation || 'Operator labels annotate this local development workflow; credentials remain on the server.'}</p></details>
    {error && <p className="operator-error" role="alert">{error}</p>}
    {storageError && <p className="operator-error" role="alert">{storageError}</p>}
    {!loading && !ready && <p className="operator-notice">{session?.reason || view?.reason || 'Local operator controls are unavailable. Configure and enable the facade, then refresh status. No command has been sent.'}</p>}
    {result && <p className="operator-result" role="status">
      Last command response for this scenario: <strong>{scenarioStatusLabel(result.outcome)}</strong>. {result.reason}
      {result.revealSucceeded && <> Reveal succeeded and remains published. Evaluation: {result.scoringStatus ?? 'Not confirmed'}.</>}
    </p>}
    {view && <>
      <dl className="dense-readout">
        <div><dt>SCENARIO</dt><dd>{scenarioStatusLabel(view.scenarioStatus ?? scenario.status)}</dd></div>
        <div><dt>SEALED / APPROVED</dt><dd>{view.prediction.sealed ? 'Sealed' : 'No seal'} / {view.prediction.approved ? 'Approved' : 'Not approved'}</dd></div>
        {!view.run && !published && <div><dt>NEXT STEP</dt><dd>{!view.prediction.sealed ? 'Save and seal prediction' : !view.prediction.approved ? 'Approve prediction'
          : !view.preflight.worldBound ? 'Prepare simulator' : view.preflight.ready ? 'Start simulation' : 'Review setup'}</dd></div>}
        <div><dt>RUN</dt><dd>{view.run ? scenarioStatusLabel(view.run.status) : 'Not started'}</dd></div>
      </dl>
      {!view.run && !published && view.prediction.approved && view.preflight.worldBound && view.preflight.reason &&
        <p className="operator-notice">{view.preflight.reason}</p>}
      {view.run && <details data-tutorial="simulation-progress" open={taskId === 'simulation'}>
        <summary>Run {view.run.runId} · {view.run.progressPercent}% checkpoints complete</summary>
        {view.run.failureReason && <p className="operator-notice">{view.run.failureReason}</p>}
        <div className="scenario-table-wrap" tabIndex={0} role="region" aria-label="Public simulation checkpoint progress">
          <table className="scenario-table"><thead><tr><th scope="col">Stage</th><th scope="col">Checkpoint</th><th scope="col">Status</th><th scope="col">Attempts</th></tr></thead>
            <tbody>{view.run.stages.map((stage) => <tr key={stage.stage}><th scope="row">{stage.stage}</th><td>{stage.name}</td><td>{scenarioStatusLabel(stage.status)}</td><td>{stage.attemptCount}</td></tr>)}</tbody>
          </table>
        </div>
      </details>}
    </>}
    {pending && <div className="operator-pending">
      <h4>{definitiveFailure ? 'Failed' : 'Unresolved'} {pending.action} attempt</h4>
      <p>Key <code>{pending.key}</code> is retained for this scenario. Refresh and inspect current status before retrying the same request or ending this local retry record. Neither action implies rollback.</p>
      {definitiveFailure && <p>The server saved a definitive failure for this key. Replaying it returns the same saved outcome.
        After inspecting current state, retire this local retry record before submitting a new explicit attempt with a new key.
        {result?.revealSucceeded && ' The reveal remains published; use Evaluation for the new scoring attempt, not another publication.'}</p>}
      {retryReason && <p className="operator-notice">{retryReason}</p>}
      <label className="operator-check"><input type="checkbox" disabled={busy || loading || !view?.enabled || readAttemptKey !== pending.key}
        checked={reviewedAttemptKey === pending.key} onChange={(event) => setReviewedAttemptKey(event.target.checked ? pending.key : '')} />
        I checked current operator state before retrying or retiring this attempt.
      </label>
      <div className="operator-actions">
        <button type="button" disabled={!ready || busy || loading || definitiveFailure || Boolean(retryReason) || reviewedAttemptKey !== pending.key || Boolean(storageError)} onClick={() => void perform(pending)}>Retry same attempt</button>
        <button type="button" disabled={busy || loading || reviewedAttemptKey !== pending.key} onClick={retireAttempt}>Retire local retry record</button>
      </div>
    </div>}
    {!pending && storageError && view?.enabled && <button type="button" disabled={busy || loading} onClick={retireAttempt}>Remove unreadable local retry record after review</button>}
    {ready && view && <>
      {(!published || taskId === 'evaluation' && !scored || pending) && <div data-tutorial="operator-label">
        <label className="operator-actor">Operator name or initials
          <input type="text" maxLength={100} value={actor} disabled={busy || Boolean(pending)} autoComplete="off" onChange={(event) => changeActor(event.target.value)} />
        </label>
        <button type="button" disabled={busy || Boolean(pending)} onClick={() => changeActor('Demo operator')}>Use demo operator label</button>
        <p>Use your chosen review label, or explicitly fill “Demo operator” for a demonstration.</p>
        {auditError && <p>{auditError}</p>}
      </div>}
      {(taskId === 'simulation' || taskId === 'prediction') && <>
        {!published && !view.run && !view.prediction.approved && <section data-tutorial="approve-prediction">
          <p>Review the saved ledger in Prediction before approving. Seal under review: <code>{sealHash || 'Not sealed yet'}</code>.</p>
          {taskId === 'simulation' && onNavigate && <button type="button" onClick={() => onNavigate('prediction')}>Review prediction</button>}
          {!matchingSeal && <p className="operator-notice">The displayed prediction ledger does not match this operator seal. Refresh the scenario before approving.</p>}
          <label className="operator-check"><input type="checkbox" data-tutorial="review-prediction-approval" disabled={blocked || !matchingSeal || !view.actions.approvePrediction.enabled}
            checked={Boolean(sealHash) && reviewedSeal === sealHash} onChange={(event) => setReviewedSeal(event.target.checked ? sealHash : '')} />
            I reviewed this exact immutable prediction seal and approve it for the local simulation workflow.
          </label>
          <div className="operator-action-row"><button type="button" disabled={blocked || !view.actions.approvePrediction.enabled || !matchingSeal || reviewedSeal !== sealHash}
            onClick={() => start('approve-prediction')}>Approve reviewed prediction</button><span>{view.actions.approvePrediction.reason}</span></div>
        </section>}
        {view.prediction.approved && taskId === 'prediction' && <p data-tutorial="prediction-approved" role="status">
          Prediction approved. {onNavigate && <button type="button" onClick={() => onNavigate('simulation')}>Continue to simulator setup</button>}
        </p>}
        {taskId === 'simulation' && <>
          {!view.run && !published && <SimulatorSetup setup={setup} view={view} actor={actor} blocked={blocked}
            editingBlocked={busy || loading || Boolean(pending)} pending={pending} error={setupError} onPrepare={settings => start('setup', settings)} />}
          {!view.run && !published && <div className="operator-action-row" data-tutorial="start-simulation"><button type="button" disabled={blocked || !view.actions.start.enabled}
            onClick={() => start('start')}>Start simulation</button><span>{view.preflight.worldBound ? view.actions.start.reason : 'Prepare the simulator after approving the prediction.'}</span></div>}
          {view.actions.cancel.enabled && view.run && <div className="operator-action-row"><button type="button" disabled={blocked} onClick={() => start('cancel')}>Cancel run</button><span>{view.actions.cancel.reason}</span></div>}
          {view.actions.resume.enabled && view.run && <div className="operator-action-row"><button type="button" disabled={blocked} onClick={() => start('resume')}>Resume permitted checkpoint</button><span>{view.actions.resume.reason}</span></div>}
          {(view.completion.available || view.run?.currentStage === 'S6DesignCompletion') && <OperatorCompletionReview view={view} scenario={scenario} actor={actor} blocked={blocked}
            reviewedKey={reviewedCompletion?.key ?? ''}
            onReview={(key) => setReviewedCompletion(key === completionReviewKey(view) && view.completion.openingsHash
              ? { key, openingsHash: view.completion.openingsHash } : undefined)}
            onApprove={() => start('approve-completion')} />}
          {view.actions.publish.enabled && onNavigate && <button type="button" onClick={() => onNavigate('new-evidence')}>Continue to publication</button>}
        </>}
      </>}
      {taskId === 'new-evidence' && <>
        <OperatorPublicationRecovery view={view} scenarioId={scenario.scenarioId} actor={actor} reason={recoveryReason}
          blocked={blocked} reviewedKey={reviewedRecovery?.key ?? ''}
          onReason={reason => { setRecoveryReason(reason); setReviewedRecovery(undefined) }}
          onReview={key => setReviewedRecovery(key === publicationRecoveryKey(view) && view.publicationRecovery?.reviewedPublicationHash
            ? { key, hash: view.publicationRecovery.reviewedPublicationHash } : undefined)}
          onRecover={() => start('recover-publication')} />
        {!published ? <section data-tutorial="publish-simulation"><p>Publishing adds synthetic observations to a separate results field and advances the evidence clock from {scenario.asOfUtc} to the reveal time.
          It does not rewrite the sealed prediction. This command also requests evaluation after a successful reveal; scoring failure does not undo publication.</p>
        <label className="operator-check"><input type="checkbox" data-tutorial="review-publication" disabled={blocked || !view.actions.publish.enabled || !view.run}
          checked={reviewedPublication === publicationKey} onChange={(event) => setReviewedPublication(event.target.checked ? publicationKey : '')} />
          I reviewed this run and intend to publish its new evidence and advance the public clock.
        </label>
        <div className="operator-action-row"><button type="button" disabled={blocked || !view.actions.publish.enabled || !view.run || reviewedPublication !== publicationKey}
          onClick={() => start('publish')}>Publish reveal and evaluate</button><span>{view.actions.publish.reason}</span></div>
        </section> : <p data-tutorial="published-simulation">New measurements have been published. Inspect the evidence and production below.
          {onNavigate && <button type="button" onClick={() => onNavigate('evaluation')}>Inspect evaluation</button>}</p>}
      </>}
      {taskId === 'evaluation' && <>
        <OperatorScoreCorrection view={view} scenarioId={scenario.scenarioId} actor={actor} reason={correctionReason}
          blocked={blocked} reviewedKey={reviewedCorrection?.key ?? ''}
          onReason={reason => { setCorrectionReason(reason); setReviewedCorrection(undefined) }}
          onReview={key => setReviewedCorrection(key === scoreCorrectionKey(view) && view.scoreCorrection?.reviewedCorrectionHash
            ? { key, hash: view.scoreCorrection.reviewedCorrectionHash } : undefined)}
          onCorrect={() => start('correct-score')} />
        {!scored && <div className="operator-action-row" data-tutorial="evaluate-simulation"><button type="button" disabled={blocked || !view.actions.score.enabled || !view.run}
          onClick={() => start('score')}>Run / retry evaluation</button><span>{view.actions.score.reason}</span></div>}
      </>}
    </>}
    {busy && <p role="status">Waiting for the operator facade. Navigating away does not undo a command; its stable attempt key is retained until confirmation.</p>}
  </section>
}
