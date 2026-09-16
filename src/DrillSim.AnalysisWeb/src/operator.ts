import { setupActorError, setupSettingsError, validateSimulatorSetup, type SimulatorResolution, type SimulatorSetup } from './simulatorSetup'

export interface OperatorSession {
  enabled: boolean
  reason?: string | null
  csrfRequestToken?: string | null
  csrfHeaderName: string
  auditLabelLimitation: string
}
export interface OperatorAvailability { enabled: boolean; reason?: string | null }
export type OperatorAction = 'approve-prediction' | 'setup' | 'start' | 'cancel' | 'resume' | 'approve-completion' | 'recover-publication' | 'correct-score' | 'publish' | 'score'
export interface ScoreCorrectionReview {
  scenarioId: string
  runId: string
  correctionEnabled: boolean
  reason: string
  rejectedScorecardId: string | null
  rejectedScorecardSha256: string | null
  scoringInputSha256: string | null
  correctionVersion: 'absolute-error-bounds-v2' | null
  metricCount: number
  invalidMetricCount: number
  changedMetricCount: number
  reviewedCorrectionHash: string | null
  correctedScorecardId: string | null
  correctedScorecardSha256: string | null
  correctedInputSha256: string | null
}
interface ScoreCorrectionResult {
  scenarioId: string
  runId: string
  outcome: 'score-corrected'
  status: 'AwaitingDependency'
  rejectedScorecardId: string
  rejectedScorecardSha256: string
  scoringInputSha256: string
  correctedScorecardId: string
  correctedScorecardSha256: string
  correctedInputSha256: string
  correctionVersion: 'absolute-error-bounds-v2'
  reviewedCorrectionHash: string
  auditId: string
}
export interface PublicationRecoveryReview {
  scenarioId: string
  runId: string
  recoveryEnabled: boolean
  reason: string
  reviewedPublicationHash: string | null
  stagedManifestSha256: string | null
  publicationPlanSha256: string | null
  operationCount: number
  verifiedOperationCount: number
  pendingOperationCount: number
  completedStageCount: number
}
interface PublicationRecoveryResult {
  scenarioId: string
  runId: string
  outcome: 'publication-recovered'
  previousStatus: 'Failed'
  status: 'PublishFailed'
  reviewedPublicationHash: string
  auditId: string
}
export interface OperatorOpening {
  reservoirName: string
  type: string
  topMdM: number
  baseMdM: number
  wellboreRadiusM: number
  skin: number
  efficiency: number
  uncertaintyM: number
}
export interface OperatorView {
  enabled: boolean
  reason?: string | null
  scenarioId: string
  scenarioStatus?: string | null
  prediction: { sealed: boolean; approved: boolean; sealHash?: string | null }
  preflight: { ready: boolean; reason?: string | null; worldBound: boolean }
  run?: {
    runId: string
    status: string
    currentStage?: string | null
    stages: Array<{ stage: string; name: string; status: string; attemptCount: number }>
    progressPercent: number
    failureReason?: string | null
  } | null
  completion: {
    available: boolean
    approvalEnabled: boolean
    reason: string
    openings: OperatorOpening[]
    openingsHash?: string | null
    status?: string | null
    runId?: string | null
    scenarioId?: string | null
  }
  publicationRecovery?: PublicationRecoveryReview | null
  scoreCorrection?: ScoreCorrectionReview | null
  actions: {
    approvePrediction: OperatorAvailability
    start: OperatorAvailability
    cancel: OperatorAvailability
    resume: OperatorAvailability
    approveCompletion: OperatorAvailability
    recoverPublication?: OperatorAvailability
    correctScore?: OperatorAvailability
    publish: OperatorAvailability
    score: OperatorAvailability
  }
}
export interface OperatorActionResult {
  scenarioId: string
  runId?: string | null
  outcome: string
  reason?: string | null
  revealSucceeded: boolean
  scoringStatus?: string | null
}
export interface OperatorAttempt {
  version: 1
  scenarioId: string
  runId?: string
  action: OperatorAction
  key: string
  actor: string
  reviewedSealHash?: string
  profileId?: string
  resolution?: SimulatorResolution
  realizationSeed?: number
  reviewedOpeningHash?: string
  reviewedPublicationHash?: string
  reviewedCorrectionHash?: string
  reason?: string
}
const actions: OperatorAction[] = ['approve-prediction', 'setup', 'start', 'cancel', 'resume', 'approve-completion', 'recover-publication', 'correct-score', 'publish', 'score']
export const operatorAttemptKey = (scenarioId: string) => `drillsim-operator-attempt-v1:${scenarioId}`
export const operatorOutcomeKey = (scenarioId: string) => `drillsim-operator-outcome-v1:${scenarioId}`
export const operatorActorKey = (scenarioId: string) => `drillsim-operator-label-v1:${scenarioId}`
export const operatorAuditError = (actor: string) =>
  !actor.trim() || actor.trim().length > 200 || /[\u0000-\u001f\u007f]/.test(actor)
    ? 'Enter an operator name or initials without control characters (maximum 200 characters).' : ''
export const completionAuditError = (actor: string) =>
  operatorAuditError(actor) || (actor.trim().length > 100 || /[^\x20-\x7e]/.test(actor)
    ? 'Completion approval requires a local audit label of at most 100 visible ASCII characters.' : '')
export const recoveryAuditError = (actor: string) =>
  !/^[\x20-\x7e]{1,100}$/.test(actor.trim())
    ? 'This reviewed action requires a local audit label of 1–100 visible ASCII characters.' : ''
export const recoveryReasonError = (reason: string) =>
  !/^[\x20-\x7e]{1,500}$/.test(reason.trim())
    ? 'Enter a reason of 1–500 visible ASCII characters explaining what was corrected.' : ''

export function scoreCorrectionKey(view: OperatorView) {
  return JSON.stringify([view.scoreCorrection?.scenarioId, view.scoreCorrection?.runId,
    view.scoreCorrection?.reviewedCorrectionHash])
}

export function scoreCorrectionBlock(view: OperatorView, scenarioId: string) {
  const review = view.scoreCorrection
  if (!view.enabled || !review?.correctionEnabled || !view.actions.correctScore?.enabled) {
    return review?.reason || view.actions.correctScore?.reason || 'No eligible rejected scorecard correction is available.'
  }
  if (view.scenarioId !== scenarioId || review.scenarioId !== scenarioId || !view.run ||
      view.run.runId !== review.runId || view.run.status !== 'Failed' || view.run.currentStage !== 'S9Score') {
    return 'Correction requires the matching failed S9 run. Refresh and inspect its current state.'
  }
  if (review.correctionVersion !== 'absolute-error-bounds-v2' ||
      [review.rejectedScorecardSha256, review.scoringInputSha256, review.reviewedCorrectionHash,
        review.correctedScorecardSha256, review.correctedInputSha256].some(hash => !hash || !/^[a-f0-9]{64}$/i.test(hash)) ||
      !review.rejectedScorecardId || !review.correctedScorecardId || review.rejectedScorecardId === review.correctedScorecardId) {
    return 'The correction version or exact artifact fingerprints are unsupported or incomplete.'
  }
  if ([review.metricCount, review.invalidMetricCount, review.changedMetricCount].some(count => !Number.isSafeInteger(count) || count < 1) ||
      review.invalidMetricCount > review.changedMetricCount || review.changedMetricCount > review.metricCount) {
    return 'The reviewed correction counts are inconsistent. Correction remains blocked.'
  }
  return ''
}

export function retainScoreCorrectionReview(key: string, view: OperatorView) {
  return !scoreCorrectionBlock(view, view.scenarioId) && key === scoreCorrectionKey(view) ? key : ''
}

export function scoreCorrectionRetryBlock(view: OperatorView | undefined, attempt: OperatorAttempt | undefined) {
  if (attempt?.action !== 'correct-score') return ''
  if (!view?.enabled || view.scenarioId !== attempt.scenarioId || view.run?.runId !== attempt.runId) {
    return 'Refresh this scenario and run before replaying its saved correction attempt.'
  }
  if (view.scoreCorrection?.correctionEnabled && view.scoreCorrection.reviewedCorrectionHash !== attempt.reviewedCorrectionHash) {
    return 'The reviewed correction changed. Inspect current state and retire this attempt before reviewing a new correction; its saved hash will not be replaced.'
  }
  return ''
}

export function publicationRecoveryKey(view: OperatorView) {
  return JSON.stringify([view.publicationRecovery?.scenarioId, view.publicationRecovery?.runId,
    view.publicationRecovery?.reviewedPublicationHash])
}

export function publicationRecoveryBlock(view: OperatorView, scenarioId: string) {
  const review = view.publicationRecovery
  if (!view.enabled || !review || !review.recoveryEnabled || !view.actions.recoverPublication?.enabled) {
    return review?.reason || view.actions.recoverPublication?.reason || 'No recoverable staged publication is available.'
  }
  if (view.scenarioId !== scenarioId || review.scenarioId !== scenarioId || !view.run || review.runId !== view.run.runId ||
      view.run.status !== 'Failed' || view.run.currentStage !== 'S8PublishReveal') {
    return 'Recovery requires the matching failed S8 run. Refresh and inspect its current state.'
  }
  if ([review.reviewedPublicationHash, review.stagedManifestSha256, review.publicationPlanSha256]
    .some(hash => !hash || !/^[a-f0-9]{64}$/i.test(hash))) {
    return 'The staged recovery fingerprints are incomplete. Recovery remains blocked.'
  }
  if ([review.operationCount, review.verifiedOperationCount, review.pendingOperationCount, review.completedStageCount]
    .some(count => !Number.isSafeInteger(count) || count < 0) || review.operationCount === 0 ||
      review.verifiedOperationCount + review.pendingOperationCount !== review.operationCount || review.completedStageCount !== 8) {
    return 'The staged operation counts or completed prerequisites are inconsistent. Recovery remains blocked.'
  }
  return ''
}

export function retainPublicationRecoveryReview(key: string, view: OperatorView) {
  return !publicationRecoveryBlock(view, view.scenarioId) && key === publicationRecoveryKey(view) ? key : ''
}

export function publicationRecoveryRetryBlock(view: OperatorView | undefined, attempt: OperatorAttempt | undefined) {
  if (attempt?.action !== 'recover-publication') return ''
  if (!view?.enabled || view.scenarioId !== attempt.scenarioId || view.run?.runId !== attempt.runId) {
    return 'Refresh this scenario and run before replaying its saved recovery attempt.'
  }
  if (view.publicationRecovery?.recoveryEnabled &&
      view.publicationRecovery.reviewedPublicationHash !== attempt.reviewedPublicationHash) {
    return 'The staged recovery fingerprint changed. Inspect current state and retire this attempt before reviewing a new recovery; its saved hash will not be replaced.'
  }
  return ''
}

export function completionReviewKey(view: OperatorView) {
  return JSON.stringify([view.completion.scenarioId, view.completion.runId, view.completion.openingsHash])
}

export function retainCompletionReview(reviewedKey: string, refreshed: OperatorView) {
  return refreshed.completion.available && reviewedKey === completionReviewKey(refreshed) ? reviewedKey : ''
}

export function completionApprovalBlock(view: OperatorView, scenarioId: string, reservoirName: string) {
  const review = view.completion
  if (!view.enabled || !review.available) return review.reason || 'No observable completion review is available. Refresh after the S6 pause.'
  if (view.scenarioId !== scenarioId || review.scenarioId !== scenarioId || !view.run || review.runId !== view.run.runId) {
    return 'The completion review does not match this scenario and current run. Refresh before reviewing.'
  }
  if (!review.openingsHash || !/^[a-f0-9]{64}$/i.test(review.openingsHash)) return 'A valid openings fingerprint is required. Approval remains blocked.'
  if (!review.openings.length || review.openings.some((opening) =>
    opening.reservoirName !== reservoirName || typeof opening.type !== 'string' || !opening.type ||
    [opening.topMdM, opening.baseMdM, opening.wellboreRadiusM, opening.skin, opening.efficiency, opening.uncertaintyM]
      .some((value) => typeof value !== 'number' || !Number.isFinite(value)))) {
    return 'Every opening must have the selected reservoir, a type and all finite numeric review fields. Incomplete review cannot be approved.'
  }
  if (review.status !== 'Draft' || !review.approvalEnabled || !view.actions.approveCompletion.enabled ||
      view.run.status !== 'AwaitingApproval' || view.run.currentStage !== 'S6DesignCompletion') {
    return review.reason || 'Completion approval requires the matching draft at the S6 approval pause.'
  }
  return ''
}

export function completionRetryBlock(view: OperatorView | undefined, attempt: OperatorAttempt | undefined) {
  if (attempt?.action !== 'approve-completion') return ''
  if (!view?.completion.available || view.scenarioId !== attempt.scenarioId ||
      view.completion.scenarioId !== attempt.scenarioId || view.run?.runId !== attempt.runId ||
      view.completion.runId !== attempt.runId || view.completion.openingsHash !== attempt.reviewedOpeningHash) {
    return 'The current completion review is unavailable or differs from this attempt. After inspecting current state, retire the old retry record and explicitly review the new openings; the old hash will never be replaced automatically.'
  }
  return ''
}

export function readOperatorAttempt(text: string, scenarioId: string): OperatorAttempt {
  const value = JSON.parse(text) as OperatorAttempt
  if (!value || value.version !== 1 || value.scenarioId !== scenarioId || !actions.includes(value.action) ||
      typeof value.key !== 'string' || !/^[\x21-\x7e]{1,128}$/.test(value.key) ||
      typeof value.actor !== 'string' || operatorAuditError(value.actor) ||
      (value.runId !== undefined && typeof value.runId !== 'string') ||
      (!['approve-prediction', 'setup', 'start'].includes(value.action) && !value.runId) ||
      (['approve-prediction', 'setup'].includes(value.action) && (typeof value.reviewedSealHash !== 'string' || !/^[a-f0-9]{64}$/i.test(value.reviewedSealHash))) ||
      (value.action === 'setup' && (setupActorError(value.actor) || value.actor !== value.actor.trim() ||
        setupSettingsError(value) || value.runId !== undefined)) ||
      (value.action === 'approve-completion' && (typeof value.reviewedOpeningHash !== 'string' ||
        !/^[a-f0-9]{64}$/i.test(value.reviewedOpeningHash) || completionAuditError(value.actor))) ||
      (value.action === 'recover-publication' && (typeof value.reviewedPublicationHash !== 'string' ||
        !/^[a-f0-9]{64}$/i.test(value.reviewedPublicationHash) || recoveryAuditError(value.actor) ||
        value.actor !== value.actor.trim() || typeof value.reason !== 'string' ||
        recoveryReasonError(value.reason) || value.reason !== value.reason.trim())) ||
      (value.action === 'correct-score' && (typeof value.reviewedCorrectionHash !== 'string' ||
        !/^[a-f0-9]{64}$/i.test(value.reviewedCorrectionHash) || recoveryAuditError(value.actor) ||
        value.actor !== value.actor.trim() || typeof value.reason !== 'string' ||
        recoveryReasonError(value.reason) || value.reason !== value.reason.trim()))) {
    throw new Error('The saved action attempt is unreadable or belongs to another scenario. No request was sent. Inspect the operator state before removing the local retry record.')
  }
  return value
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const response = await fetch(`/analysis-api/api/operator${path}`, {
    ...options, credentials: 'same-origin', cache: 'no-store',
  })
  if (!response.ok) {
    let detail = response.status === 404
      ? 'The local operator endpoint or requested scenario/run was not found. Refresh operator status and verify server integration.'
      : 'Refresh operator status before retrying the same attempt. No rollback is implied.'
    try {
      const problem = await response.json() as { title?: string; detail?: string }
      detail = [problem.title, problem.detail].filter(Boolean).join(' ') || detail
    } catch { /* Only curated problem details are displayed, never upstream response HTML. */ }
    throw new Error(`${response.status}: ${detail}`)
  }
  return response.json() as Promise<T>
}

let sessionRequest: Promise<OperatorSession> | undefined
export async function getOperatorSession(signal?: AbortSignal) {
  signal?.throwIfAborted()
  // Initial cookie issuance must finish once, even when a component unmounts mid-request.
  sessionRequest ??= request<OperatorSession>('/session').finally(() => { sessionRequest = undefined })
  const session = await sessionRequest
  signal?.throwIfAborted()
  return session
}
export async function getOperatorView(scenarioId: string, signal?: AbortSignal) {
  const view = await request<OperatorView>(`/scenarios/${encodeURIComponent(scenarioId)}`, { signal })
  if (view.scenarioId !== scenarioId) throw new Error('Operator status belongs to another scenario. Refresh before acting.')
  return view
}
export async function getOperatorSetup(scenarioId: string, signal?: AbortSignal) {
  const setup = await request<SimulatorSetup>(`/scenarios/${encodeURIComponent(scenarioId)}/setup`, { signal })
  return validateSimulatorSetup(setup, scenarioId)
}

export async function sendOperatorAttempt(session: OperatorSession, attempt: OperatorAttempt) {
  readOperatorAttempt(JSON.stringify(attempt), attempt.scenarioId)
  if (!session.enabled || !session.csrfRequestToken || session.csrfHeaderName !== 'X-DrillSim-CSRF') {
    throw new Error(session.reason || 'A supported local operator session and antiforgery token are required. Refresh operator status.')
  }
  const base = `/scenarios/${encodeURIComponent(attempt.scenarioId)}`
  const path = attempt.action === 'approve-prediction' ? `${base}/approve-prediction`
    : attempt.action === 'setup' ? `${base}/setup`
    : attempt.action === 'start' ? `${base}/runs`
    : `${base}/runs/${encodeURIComponent(attempt.runId!)}/${attempt.action}`
  const options: RequestInit = {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      [session.csrfHeaderName]: session.csrfRequestToken,
      'Idempotency-Key': attempt.key,
    },
    body: JSON.stringify({
      actor: attempt.actor,
      ...(attempt.action === 'approve-prediction' ? { reviewedSealHash: attempt.reviewedSealHash } : {}),
      ...(attempt.action === 'setup' ? { profileId: attempt.profileId, resolution: attempt.resolution,
        realizationSeed: attempt.realizationSeed, reviewedSealHash: attempt.reviewedSealHash } : {}),
      ...(attempt.action === 'approve-completion' ? { reviewedOpeningHash: attempt.reviewedOpeningHash } : {}),
      ...(attempt.action === 'recover-publication' ? { reviewedPublicationHash: attempt.reviewedPublicationHash, reason: attempt.reason } : {}),
      ...(attempt.action === 'correct-score' ? { reviewedCorrectionHash: attempt.reviewedCorrectionHash, reason: attempt.reason } : {}),
    }),
  }
  let result: OperatorActionResult
  if (attempt.action === 'recover-publication') {
    const recovered = await request<PublicationRecoveryResult>(path, options)
    if (recovered.outcome !== 'publication-recovered' || recovered.previousStatus !== 'Failed' ||
        recovered.status !== 'PublishFailed' || recovered.reviewedPublicationHash !== attempt.reviewedPublicationHash ||
        typeof recovered.auditId !== 'string' || !recovered.auditId) {
      throw new Error('The recovery response does not confirm the reviewed transition. Keep the saved attempt and inspect current state.')
    }
    result = { ...recovered, revealSucceeded: false,
      reason: `Recovery recorded (${recovered.auditId}). Staged data is retained. Review and publish separately with a new attempt.` }
  } else if (attempt.action === 'correct-score') {
    const corrected = await request<ScoreCorrectionResult>(path, options)
    if (corrected.outcome !== 'score-corrected' || corrected.status !== 'AwaitingDependency' ||
        corrected.correctionVersion !== 'absolute-error-bounds-v2' || corrected.reviewedCorrectionHash !== attempt.reviewedCorrectionHash ||
        !corrected.auditId || !corrected.rejectedScorecardId || !corrected.correctedScorecardId ||
        corrected.rejectedScorecardId === corrected.correctedScorecardId ||
        [corrected.rejectedScorecardSha256, corrected.scoringInputSha256, corrected.correctedScorecardSha256,
          corrected.correctedInputSha256].some(hash => typeof hash !== 'string' || !/^[a-f0-9]{64}$/i.test(hash))) {
      throw new Error('The correction response does not confirm the reviewed artifacts. Keep the saved attempt and inspect current state.')
    }
    result = { ...corrected, revealSucceeded: true, scoringStatus: 'pending',
      reason: `Correction recorded (${corrected.auditId}); rejected scorecard ${corrected.rejectedScorecardId} is retained. Publish evaluation separately with a new attempt.` }
  } else {
    result = await request<OperatorActionResult>(path, options)
  }
  if (attempt.action === 'setup' && (result.outcome !== 'simulation-prepared' || result.revealSucceeded !== false || result.runId)) {
    throw new Error('The response did not confirm simulator preparation without starting a run. Keep the saved attempt and refresh setup before retrying.')
  }
  if (result.scenarioId !== attempt.scenarioId || (attempt.runId && result.runId !== attempt.runId)) {
    throw new Error('The action response belongs to another scenario or run. No success is assumed; refresh operator status.')
  }
  return result
}

export const operatorAttemptConfirmed = (result: OperatorActionResult) =>
  ['approved', 'simulation-prepared', 'started', 'cancelled', 'resumed', 'completion-approved', 'publication-recovered', 'score-corrected', 'scored'].includes(result.outcome)
export const operatorAttemptFailed = (result: OperatorActionResult) =>
  ['publication-failed', 'scoring-failed'].includes(result.outcome)

export function operatorLifecycleLabel(taskId: 'simulation' | 'new-evidence' | 'evaluation', view: OperatorView) {
  if (!view.enabled) return 'Operator unavailable'
  if (taskId === 'simulation') {
    if (view.run) {
      if (view.run.status === 'AwaitingApproval') return 'Completion approval required'
      return view.run.status
    }
    if (!view.prediction.sealed) return 'Sealed prediction required'
    if (!view.prediction.approved) return 'Approval required'
    return view.preflight.ready ? 'Ready to start' : !view.preflight.worldBound ? 'Prepare simulator' : 'Setup check required'
  }
  if (taskId === 'new-evidence') {
    if (view.run?.status === 'Scored' || view.run?.status === 'Revealed') return 'Revealed'
    if (view.actions.recoverPublication?.enabled) return 'Recovery review required'
    if (view.actions.publish.enabled) return 'Ready to publish'
    return view.run?.status === 'PublishFailed' ? 'Publication failed' : 'Simulation incomplete'
  }
  if (view.run?.status === 'Scored') return 'Scored'
  if (view.actions.correctScore?.enabled) return 'Score correction review'
  if (view.run?.currentStage === 'S9Score') return view.run.status === 'AwaitingDependency' ? 'Scoring blocked' : 'Scoring'
  return view.actions.score.enabled ? 'Evaluation available' : 'Reveal required'
}
