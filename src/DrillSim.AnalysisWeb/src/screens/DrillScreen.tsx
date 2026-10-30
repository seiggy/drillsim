import { useEffect, useRef, useState } from 'react'
import { getScenario } from '../api'
import {
  getOperatorSession, getOperatorSetup, getOperatorView, operatorAttemptConfirmed, sendOperatorAttempt,
  type OperatorAttempt, type OperatorView,
} from '../operator'
import type { AppSettings } from '../settings'
import type { Scenario } from '../types'

const stageNames: Record<string, string> = {
  S0BindWorld: 'Load the hidden reservoir model',
  S1MaterializePlan: 'Plan the well',
  S2ExecuteDrilling: 'Drill the well',
  S3GenerateSurvey: 'Survey the well path',
  S4SampleGeology: 'Sample the rock',
  S5GenerateLogs: 'Log the well',
  S6DesignCompletion: 'Design the completion',
  S7RunProduction: 'Produce for five years',
  S8PublishReveal: 'Publish the new well data',
  S9Score: 'Score the forecast',
}
const wait = (ms: number) => new Promise(resolve => window.setTimeout(resolve, ms))

function explainFailure(reason?: string | null) {
  const text = reason ?? 'The simulator stopped without a reason.'
  if (/coverage|outside/i.test(text)) return { text: 'The well was planned outside the area the simulator models, so drilling never started.', fix: 'Choose a target inside the drillable area on Prospects and forecast it again.' }
  return { text, fix: 'Start a new run from Prospects. If it happens again, check the simulator services in the Aspire dashboard.' }
}

export function DrillScreen({ run, settings, onRunChanged, onResults, onProspects }: {
  run: Scenario
  settings: AppSettings
  onRunChanged: (run: Scenario, runStatus?: string) => void
  onResults: () => void
  onProspects: () => void
}) {
  const [view, setView] = useState<OperatorView>()
  const [activity, setActivity] = useState('')
  const [error, setError] = useState('')
  const [driving, setDriving] = useState(false)
  const alive = useRef(true)
  const driver = useRef(false)

  async function refresh() {
    const next = await getOperatorView(run.scenarioId)
    if (alive.current) setView(next)
    return next
  }

  useEffect(() => {
    alive.current = true
    void refresh().catch(reason => setError(String(reason)))
    return () => { alive.current = false }
  }, [run.scenarioId])

  async function send(next: OperatorView, action: OperatorAttempt['action'], extra: Partial<OperatorAttempt> = {}) {
    const session = await getOperatorSession()
    const result = await sendOperatorAttempt(session, { version: 1, scenarioId: run.scenarioId, action, key: crypto.randomUUID(),
      actor: settings.operatorName.trim(), ...(next.run && !['setup', 'start', 'approve-prediction'].includes(action) ? { runId: next.run.runId } : {}), ...extra })
    if (!operatorAttemptConfirmed(result) && !result.outcome.endsWith('-pending')) {
      throw new Error(result.reason ?? `The simulator did not confirm "${action}".`)
    }
  }

  async function drive() {
    if (driver.current) return
    driver.current = true
    setDriving(true); setError('')
    let idle = 0
    let reported = ''
    try {
      while (alive.current) {
        const next = await refresh()
        const status = next.run?.status
        if (status && status !== reported) { reported = status; onRunChanged(run, status) }
        if (status === 'Scored') { setActivity(''); onRunChanged(await getScenario(run.scenarioId), status); onResults(); return }
        if (status === 'Failed' || status === 'Cancelled') { onRunChanged(await getScenario(run.scenarioId), status); setActivity(''); return }
        if (!next.prediction.approved) throw new Error('This forecast is not locked yet. Lock it on the Forecast step first.')
        if (!next.run) {
          if (!next.preflight.worldBound) {
            setActivity('Preparing the hidden reservoir model…')
            const setup = await getOperatorSetup(run.scenarioId)
            const profileId = setup.defaults.profileId ?? setup.profiles[0]?.profileId
            if (!setup.available || !profileId || !setup.reviewedSealHash) throw new Error(setup.reason ?? 'No simulator model is available for this field.')
            await send(next, 'setup', { profileId, resolution: settings.simulatorResolution, realizationSeed: setup.defaults.realizationSeed, reviewedSealHash: setup.reviewedSealHash })
          } else if (next.actions.start.enabled) {
            setActivity('Starting the drilling run…')
            await send(next, 'start')
          } else throw new Error(next.preflight.reason ?? next.actions.start.reason ?? 'The run cannot start yet.')
          continue
        }
        if (next.actions.approveCompletion.enabled && next.completion.openingsHash) {
          if (!settings.autoApproveCompletion) { setActivity(''); return }
          setActivity('Approving the completion design…')
          await send(next, 'approve-completion', { reviewedOpeningHash: next.completion.openingsHash })
        } else if (next.actions.publish.enabled) {
          setActivity('Publishing the new well data…')
          await send(next, 'publish')
        } else if (next.actions.score.enabled) {
          setActivity('Scoring the forecast against the hidden truth…')
          await send(next, 'score')
        } else if (next.actions.resume.enabled) {
          setActivity('Resuming the run…')
          await send(next, 'resume')
        } else {
          setActivity(`${stageNames[next.run.currentStage ?? ''] ?? 'Simulating'}…`)
          idle = ['Queued', 'Running'].includes(status ?? '') ? 0 : idle + 1
          if (idle > 40) throw new Error(next.actions.publish.reason ?? next.completion.reason ?? 'The run stopped making progress.')
          await wait(3000)
        }
      }
    } catch (reason) {
      const text = reason instanceof Error ? reason.message : String(reason)
      if (alive.current) {
        setError(/^409\b/.test(text) ? 'The simulator refused this step because the run is not in the state it expected. Click Try again; if it repeats, start a new run from Prospects.' : text)
        setActivity('')
      }
    } finally { driver.current = false; if (alive.current) setDriving(false) }
  }

  // Resume following a run that is already underway when the screen opens.
  useEffect(() => {
    if (view?.run && ['Queued', 'Running', 'AwaitingDependency'].includes(view.run.status) && !driver.current) void drive()
  }, [view?.run?.runId])

  const stages = view?.run?.stages ?? Object.keys(stageNames).map(stage => ({ stage, name: stageNames[stage], status: 'Pending', attemptCount: 0 }))
  const failed = view?.run && ['Failed', 'Cancelled'].includes(view.run.status) ? explainFailure(view.run.failureReason) : undefined
  const scored = view?.run?.status === 'Scored'
  const awaitingCompletion = Boolean(view?.actions.approveCompletion.enabled && !settings.autoApproveCompletion && !driving)

  return <section className="screen-grid" aria-labelledby="drill-title">
    <header className="screen-head">
      <div>
        <h1 id="drill-title">Drill it in the simulator</h1>
        <p>The simulator drills your locked well into a hidden reservoir you have never seen, logs it, produces it for five years,
          then compares the result with your forecast.</p>
      </div>
      {scored ? <button type="button" className="primary" onClick={onResults}>See the results →</button>
        : failed ? <button type="button" className="primary" onClick={onProspects}>Choose another target</button>
        : <button type="button" className="primary" disabled={driving || !view?.prediction.approved} onClick={() => void drive()}>
          {driving ? 'Drilling…' : view?.run ? 'Continue the run' : 'Drill this well'}</button>}
    </header>

    {!view?.prediction.approved && view && <p className="app-error" role="alert">Lock the forecast on the Forecast step before drilling.</p>}
    {activity && <p className="status-line" role="status"><span className="spinner" aria-hidden="true" />{activity}</p>}
    {error && <div className="app-error" role="alert"><strong>The run stopped.</strong> {error}
      <div className="button-row"><button type="button" onClick={() => void drive()}>Try again</button></div></div>}
    {failed && <div className="app-error" role="alert"><strong>Drilling failed.</strong> {failed.text}<p>{failed.fix}</p></div>}

    <ol className="stage-track" aria-label="Simulation progress">
      {stages.map(stage => {
        const state = stage.status === 'Completed' ? 'done' : stage.status === 'Failed' ? 'failed'
          : view?.run?.currentStage === stage.stage ? 'active' : 'pending'
        return <li key={stage.stage} className={state}>
          <span className="stage-dot" aria-hidden="true" />
          <span>{stageNames[stage.stage] ?? stage.name}</span>
          <small>{state === 'done' ? 'Done' : state === 'failed' ? 'Failed' : state === 'active' ? (view?.run?.status === 'AwaitingApproval' ? 'Waiting for approval' : 'In progress') : 'Waiting'}</small>
        </li>
      })}
    </ol>

    {awaitingCompletion && view && <section className="panel">
      <h2>Approve the completion design</h2>
      <p>The simulator chose where to open the well to the reservoir. Approve to start production.</p>
      <table className="plain-table"><thead><tr><th>Interval (m along the well)</th><th>Type</th><th>Efficiency</th></tr></thead>
        <tbody>{view.completion.openings.map((opening, index) => <tr key={index}>
          <td>{opening.topMdM.toFixed(0)} – {opening.baseMdM.toFixed(0)}</td><td>{opening.type}</td><td>{(opening.efficiency * 100).toFixed(0)}%</td></tr>)}</tbody></table>
      <button type="button" className="primary" onClick={() => { void (async () => {
        try { await send(view, 'approve-completion', { reviewedOpeningHash: view.completion.openingsHash! }); void drive() }
        catch (reason) { setError(String(reason)) }
      })() }}>Approve and continue</button>
    </section>}
    <p className="fine-print">Run {run.seedLabel}. Simulator settings and automatic approvals are in Settings.</p>
  </section>
}
