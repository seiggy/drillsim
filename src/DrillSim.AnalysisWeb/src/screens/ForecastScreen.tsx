import { useEffect, useMemo, useRef, useState } from 'react'
import { applyAnalysis, createScenario, getFieldPackage, getPrediction, getPredictionCapabilities, getScenario, savePrediction, sealPrediction } from '../api'
import { evidenceLabels, readableEvidence } from '../agentApi'
import { applyForecastDraft, draftForecast, getForecastAgentStatus, type ForecastDraftResult } from '../forecastDraft'
import { getOperatorSession, getOperatorView, operatorAttemptConfirmed, sendOperatorAttempt } from '../operator'
import { parsePrediction, predictionTemplate } from '../prediction'
import { createPredictionBinding, predictionSourceBlock, validatePredictionSource, type PredictionSourceContext } from '../predictionBinding'
import { fillDemoForecast, fillFromReservoirModel, readPredictionForm, type PredictionFormDocument } from '../predictionExamples'
import { getReservoirColumns, modelPay, type ReservoirModelReadout } from '../reservoirModel'
import { insideCoverage, type SimulatorCoverage } from '../prospects'
import { payLabel } from '../analysisConfiguration'
import { AssistantContext } from '../assistantContext'
import { prepareScenarioCreation } from '../scenarioCreation'
import type { AppSettings } from '../settings'
import { metres } from '../structure'
import type { FieldPackage, PredictionRecord, RankedCandidate, Scenario } from '../types'

type Quantile = { p90: number | null; p50: number | null; p10: number | null }

function QuantileInputs({ label, value, onChange, disabled }: { label: string; value: Quantile; onChange: (next: Quantile) => void; disabled?: boolean }) {
  return <fieldset className="quantile-inputs" disabled={disabled}><legend>{label}</legend>
    {(['p90', 'p50', 'p10'] as const).map(key => <label key={key}>{key === 'p90' ? 'Shallow / low' : key === 'p50' ? 'Best estimate' : 'Deep / high'}
      <input type="number" step="any" value={value[key] ?? ''} onChange={event => onChange({ ...value, [key]: event.target.value === '' ? null : event.target.valueAsNumber })} /></label>)}
  </fieldset>
}

export function PredictionSummary({ prediction }: { prediction: PredictionRecord }) {
  const body = prediction.body
  const formation = body.formations[0]
  const td = body.proposedWellPath.at(-1)?.trueVerticalDepthM
  const year5 = body.productionForecasts.find(item => item.year === 5)
  return <dl className="summary-grid">
    <div><dt>Expected {payLabel(prediction.body.analysisBinding?.configuration)}</dt><dd>{metres(body.expectedPaydirtM.p50)}</dd><small>{metres(body.expectedPaydirtM.p90)} – {metres(body.expectedPaydirtM.p10)}</small></div>
    <div><dt>Reservoir top</dt><dd>{metres(formation?.topTrueVerticalDepthM.p50)}</dd><small>{metres(formation?.topTrueVerticalDepthM.p90)} – {metres(formation?.topTrueVerticalDepthM.p10)}</small></div>
    <div><dt>Reservoir base</dt><dd>{metres(formation?.baseTrueVerticalDepthM.p50)}</dd><small>{metres(formation?.baseTrueVerticalDepthM.p90)} – {metres(formation?.baseTrueVerticalDepthM.p10)}</small></div>
    <div><dt>Total depth</dt><dd>{metres(td)}</dd><small>Vertical well</small></div>
    <div><dt>Fluids</dt><dd>{body.fluidClasses.join(', ')}</dd><small>{body.contactPredictions.map(c => `${c.contactType} ${metres(c.trueVerticalDepthM.p50)}`).join(' · ') || 'No contacts'}</small></div>
    <div><dt>5-year gas · oil</dt><dd>{year5 ? `${(year5.gasM3 / 1e6).toFixed(1)} M m³` : '—'}</dd><small>{year5 ? `${Math.round(year5.oilM3).toLocaleString()} m³ oil` : ''}</small></div>
  </dl>
}

export function ForecastScreen({ run, pkg, reservoir, candidate, coverage, settings, onRunChanged, onProspects, onDrill }: {
  run?: Scenario
  pkg: FieldPackage
  reservoir: string
  candidate?: RankedCandidate
  coverage?: SimulatorCoverage
  settings: AppSettings
  onRunChanged: (run: Scenario, runStatus?: string) => void
  onProspects: () => void
  onDrill: () => void
}) {
  const [context, setContext] = useState<PredictionSourceContext>()
  const [prediction, setPrediction] = useState<PredictionRecord>()
  const [text, setText] = useState('')
  const [busy, setBusy] = useState('')
  const [error, setError] = useState('')
  const [ai, setAi] = useState<ForecastDraftResult>()
  const [aiStatus, setAiStatus] = useState<{ configured: boolean; reason: string | null }>()
  const [aiElapsed, setAiElapsed] = useState(0)
  const [modelReadout, setModelReadout] = useState<ReservoirModelReadout>()
  const created = useRef(false)
  const aiRequest = useRef<AbortController | undefined>(undefined)
  const doc = useMemo(() => text ? readPredictionForm(text) : undefined, [text])
  const locked = Boolean(prediction?.seal && prediction.approval)
  const draftKey = run ? `drillsim-forecast-draft-v2:${run.scenarioId}` : ''

  // Start a run for the chosen target. A ref guard keeps strict-mode remounts from creating two runs.
  useEffect(() => {
    if (run || !candidate || created.current) return
    created.current = true
    setBusy('Starting a new run for this target…')
    void (async () => {
      try {
        const rank = candidate.rank || 1
        const attempt = await prepareScenarioCreation({
          sourceFieldId: pkg.fieldId, reservoirName: reservoir, configuration: settings.analysis,
          name: `Target #${rank} · ${new Date().toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })}`,
          asOfUtc: new Date().toISOString(),
          purpose: `Forecast and drill target #${rank} in the blind simulator.`,
          assumptions: 'Uses the field data available now. One vertical well at the selected target.',
        })
        onRunChanged(await createScenario(attempt.payload))
      } catch (reason) { created.current = false; setError(`Could not start a run: ${String(reason)} Go back to Prospects and try again.`); setBusy('') }
    })()
  }, [run, candidate])

  useEffect(() => {
    if (!run) return
    const controller = new AbortController()
    setBusy('Preparing the evidence frozen at the start of this run…'); setError('')
    void (async () => {
      try {
        const scope = { scenarioId: run.scenarioId, asOf: run.initialAsOfUtc }
        const [capabilities, existing, frozenPkg] = await Promise.all([
          getPredictionCapabilities(controller.signal), getPrediction(run.scenarioId, controller.signal),
          getFieldPackage(run.sourceFieldId, scope, controller.signal)])
        const configuration = existing?.body.analysisBinding?.configuration ?? settings.analysis
        const frozen = await applyAnalysis(run.sourceFieldId, run.reservoirName, configuration, scope, controller.signal)
        const targetId = existing?.body.candidateId ?? candidate?.candidateId
        const target = frozen.candidateGrid?.find(cell => cell.status === 'eligible' && cell.candidateId === targetId)?.prediction ?? undefined
        if (controller.signal.aborted) return
        if (!target) throw new Error('This run has no target yet. Choose one on Prospects.')
        const next = { scenario: run, fieldPackage: frozenPkg, analysis: frozen, candidate: { ...target, rank: candidate?.candidateId === target.candidateId ? candidate.rank : target.rank }, capabilities }
        setContext(next); setPrediction(existing)
        void getReservoirColumns(run.sourceFieldId, run.reservoirName, configuration,
          [{ id: target.candidateId, eastingM: target.eastingM, northingM: target.northingM }], scope, controller.signal)
          .then(setModelReadout).catch(() => setModelReadout(undefined))
        const stored = draftKey ? sessionStorage.getItem(draftKey) : null
        setText(existing ? JSON.stringify(existing.body, null, 2) : stored ?? predictionTemplate(run, frozenPkg, target, createPredictionBinding(next)))
        setBusy('')
      } catch (reason) { if (!controller.signal.aborted) { setError(String(reason)); setBusy('') } }
    })()
    void getForecastAgentStatus(controller.signal).then(setAiStatus).catch(() => setAiStatus({ configured: false, reason: 'AI status unavailable' }))
    return () => controller.abort()
  }, [run?.scenarioId])

  useEffect(() => { if (draftKey && text && !locked) sessionStorage.setItem(draftKey, text) }, [text])
  useEffect(() => {
    if (!aiRequest.current) return
    const started = Date.now()
    const timer = window.setInterval(() => setAiElapsed(Math.round((Date.now() - started) / 1000)), 1000)
    return () => window.clearInterval(timer)
  }, [busy])

  function edit(update: (body: PredictionFormDocument) => void) {
    if (!doc || locked) return
    const next = structuredClone(doc)
    update(next)
    setText(JSON.stringify(next, null, 2)); setError('')
  }

  async function askAi() {
    if (!context) return
    const controller = new AbortController()
    aiRequest.current = controller
    setBusy('ai'); setAiElapsed(0); setError('')
    try {
      const result = await draftForecast(context, controller.signal)
      const labels = evidenceLabels(context.fieldPackage)
      const body = JSON.parse(applyForecastDraft(context, result)) as PredictionFormDocument
      body.rationale = readableEvidence(body.rationale, labels)
      body.uncertaintyAssumptions = body.uncertaintyAssumptions.map(item => readableEvidence(item, labels))
      setAi({ ...result, draft: { ...result.draft, rationale: readableEvidence(result.draft.rationale, labels),
        limitations: result.draft.limitations.map(item => readableEvidence(item, labels)) } })
      setText(JSON.stringify(body, null, 2))
    } catch (reason) { if (!controller.signal.aborted) setError(`The AI could not draft a forecast. ${reason instanceof Error ? reason.message : String(reason)} You can fill the values yourself.`) }
    finally { aiRequest.current = undefined; if (!controller.signal.aborted) setBusy('') }
  }

  async function lock() {
    if (!run || !context || locked) return
    const td = doc?.proposedWellPath.at(-1)?.trueVerticalDepthM
    if (coverage && typeof td === 'number' && td > coverage.maxTrueVerticalDepthM) {
      setError(`Total depth ${metres(td)} is deeper than the simulator models (${metres(coverage.maxTrueVerticalDepthM)}). Reduce it before locking.`)
      return
    }
    let body
    try { body = parsePrediction(text); validatePredictionSource(body, context, 'seal') }
    catch (reason) { setError(`The forecast is not complete yet: ${reason instanceof Error ? reason.message : String(reason)}`); return }
    if (!window.confirm('Lock this forecast? It becomes the sealed prediction for this run and can no longer be edited. You can then drill the well.')) return
    setError('')
    try {
      let record = prediction
      if (!record?.seal) {
        setBusy('Saving the forecast…')
        record = await savePrediction(run.scenarioId, body, record?.revision ?? null)
        setBusy('Sealing the forecast…')
        record = await sealPrediction(run.scenarioId, record.revision, crypto.randomUUID())
      }
      setBusy('Approving the forecast…')
      const session = await getOperatorSession()
      const view = await getOperatorView(run.scenarioId)
      if (!view.prediction.approved) {
        const result = await sendOperatorAttempt(session, { version: 1, scenarioId: run.scenarioId, action: 'approve-prediction',
          key: crypto.randomUUID(), actor: settings.operatorName.trim(), reviewedSealHash: record.seal!.sha256 })
        if (!operatorAttemptConfirmed(result)) throw new Error(result.reason ?? 'Approval was not confirmed.')
      }
      const saved = await getPrediction(run.scenarioId)
      setPrediction(saved); sessionStorage.removeItem(draftKey)
      onRunChanged(await getScenario(run.scenarioId))
      setBusy('')
      onDrill()
    } catch (reason) { setError(String(reason)); setBusy('') }
  }

  if (!run && !candidate) return <section className="screen-empty"><h2>Choose a target first</h2>
    <p>Pick one of the best targets on Prospects, then forecast it here.</p><button type="button" className="primary" onClick={onProspects}>Go to Prospects</button></section>

  const target = context?.candidate
  const sourceProblem = context ? predictionSourceBlock(context) : undefined
  const outside = target && !insideCoverage(target, coverage)
  const formation = doc?.formations[0]
  const tdStation = doc?.proposedWellPath.at(-1)
  const modelColumn = modelReadout?.columns.find(item => item.id === target?.candidateId)
  const pays = payLabel(context?.analysis.configuration)
  return <section className="screen-grid" aria-labelledby="forecast-title">
    <AssistantContext id="forecast" description="Forecast screen: the forecast for the selected target as currently entered (P90 low, P50 best, P10 high)" value={doc && {
      locked, targetRank: target?.rank, payMeasure: pays,
      screeningPayP90P50P10M: target && [target.p90NetPayM, target.p50NetPayM, target.p10NetPayM].map(v => +v.toFixed(1)),
      reservoirModelAtTarget: modelColumn && { topTvdM: modelColumn.topTvdM, baseTvdM: modelColumn.baseTvdM, goodRockM: modelColumn.netRockM, gasM: modelColumn.gasM, oilM: modelColumn.oilM, waterM: modelColumn.waterM },
      expectedPayM: doc.expectedPaydirtM, reservoirTopTvdM: formation?.topTrueVerticalDepthM, reservoirBaseTvdM: formation?.baseTrueVerticalDepthM,
      totalDepthM: tdStation?.trueVerticalDepthM, fluids: doc.fluidClasses, contacts: doc.contactPredictions,
      cumulativeProductionSurfaceM3: doc.productionForecasts, rationale: doc.rationale.slice(0, 1200),
      simulatorMaxDepthM: coverage?.maxTrueVerticalDepthM, aiDraftLimitations: ai?.draft.limitations,
    }} />
    <header className="screen-head">
      <div>
        <h1 id="forecast-title">{locked ? 'Locked forecast' : 'What will this well find?'}</h1>
        <p>{locked ? 'This forecast is sealed. Drill the well to see how close it was.'
          : <>Forecast a vertical well at target #{target?.rank ?? candidate?.rank ?? '–'}. Ask the AI to draft it from nearby wells, adjust anything you disagree with,
            then lock it. Once locked it can’t change, so the simulator result is a fair test.</>}</p>
      </div>
      {locked ? <button type="button" className="primary" onClick={onDrill}>Drill this well →</button>
        : <button type="button" className="primary" disabled={!doc || Boolean(busy) || Boolean(outside) || Boolean(sourceProblem)} onClick={() => void lock()}>Lock forecast</button>}
    </header>

    {busy && busy !== 'ai' && <p className="status-line" role="status"><span className="spinner" aria-hidden="true" />{busy}</p>}
    {error && <p className="app-error" role="alert">{error}</p>}
    {outside && <p className="app-error" role="alert">This target is outside the area the simulator can drill. Pick another target on Prospects.</p>}
    {sourceProblem && !busy && <p className="app-error" role="alert">{sourceProblem}</p>}

    {locked && prediction && <section className="panel"><PredictionSummary prediction={prediction} />
      <details className="plain-details"><summary>Reasoning and assumptions</summary><p>{prediction.body.rationale}</p>
        <ul>{prediction.body.uncertaintyAssumptions.map((item, index) => <li key={index}>{item}</li>)}</ul></details></section>}

    {!locked && doc && target && <div className="split forecast-split">
      <section className="panel forecast-editor" aria-label="Forecast values">
        <div className="fixed-values">
          <div><span>Target</span><strong>#{target.rank}</strong></div>
          <div><span>Screening estimate of {pays} (no AI)</span><strong>{metres(target.p50NetPayM)}</strong><small>{metres(target.p90NetPayM)} – {metres(target.p10NetPayM)}</small></div>
          {modelColumn && <div><span>Reservoir model (no AI)</span><strong>{metres(modelPay(modelColumn, context.analysis.configuration))}</strong><small>{metres(modelColumn.gasM + modelColumn.oilM)} above the contact · top {metres(modelColumn.topTvdM)}</small></div>}
        </div>
        <QuantileInputs label={`Your ${pays} forecast · metres of good rock${context.analysis.configuration?.targetFluid ? ` holding ${context.analysis.configuration.targetFluid.toLowerCase()}` : ''}`} value={doc.expectedPaydirtM}
          onChange={next => edit(body => { body.expectedPaydirtM = next })} />
        {formation && <>
          <QuantileInputs label={`${formation.formationName} top · metres below surface`} value={formation.topTrueVerticalDepthM}
            onChange={next => edit(body => { body.formations[0].topTrueVerticalDepthM = next })} />
          <QuantileInputs label={`${formation.formationName} base · metres below surface`} value={formation.baseTrueVerticalDepthM}
            onChange={next => edit(body => { body.formations[0].baseTrueVerticalDepthM = next })} />
        </>}
        <label className="inline-field">Total depth to drill (m)<input type="number" step="any" value={tdStation?.trueVerticalDepthM ?? ''}
          onChange={event => edit(body => {
            const value = event.target.value === '' ? null : event.target.valueAsNumber
            const last = body.proposedWellPath[body.proposedWellPath.length - 1]
            body.proposedWellPath[0] = { ...body.proposedWellPath[0], measuredDepthM: 0, trueVerticalDepthM: 0 }
            last.measuredDepthM = value; last.trueVerticalDepthM = value
          })} />
          {coverage && <small>The simulator models down to {metres(coverage.maxTrueVerticalDepthM)}.</small>}</label>
        <fieldset className="fluid-inputs"><legend>Fluids you expect in the reservoir</legend>
          {(['Oil', 'Gas', 'Water'] as const).map(fluid => <label key={fluid}><input type="checkbox" checked={doc.fluidClasses.includes(fluid)}
            onChange={event => edit(body => { body.fluidClasses = event.target.checked ? [...body.fluidClasses, fluid] : body.fluidClasses.filter(item => item !== fluid) })} />{fluid}</label>)}
        </fieldset>
        <fieldset className="production-inputs"><legend>Total production after 1, 3 and 5 years (m³ at surface)</legend>
          <table><thead><tr><th>Year</th><th>Oil</th><th>Gas</th><th>Water</th></tr></thead>
            <tbody>{doc.productionForecasts.map((row, index) => <tr key={index}><th>{row.year}</th>
              {(['oilM3', 'gasM3', 'waterM3'] as const).map(key => <td key={key}><input type="number" step="any" aria-label={`Year ${row.year} ${key.replace('M3', '')} m³`}
                value={row[key] ?? ''} onChange={event => edit(body => { body.productionForecasts[index][key] = event.target.value === '' ? null : event.target.valueAsNumber })} /></td>)}
            </tr>)}</tbody></table>
        </fieldset>
        <label className="inline-field">Why you expect this<textarea rows={3} value={doc.rationale} onChange={event => edit(body => { body.rationale = event.target.value })} /></label>
        <details className="plain-details"><summary>Contacts, assumptions and evidence ({doc.contactPredictions.length} contacts)</summary>
          {doc.contactPredictions.map((contact, index) => <div key={index} className="contact-row">
            <QuantileInputs label={`${contact.contactType} depth (m)`} value={contact.trueVerticalDepthM}
              onChange={next => edit(body => { body.contactPredictions[index].trueVerticalDepthM = next })} />
            <button type="button" onClick={() => edit(body => { body.contactPredictions.splice(index, 1) })}>Remove {contact.contactType}</button></div>)}
          <label className="inline-field">Assumptions (one per line)<textarea rows={4} value={doc.uncertaintyAssumptions.join('\n')}
            onChange={event => edit(body => { body.uncertaintyAssumptions = event.target.value.split('\n') })} /></label>
          <p>{doc.citedEvidenceIds.length} wells and records are cited as evidence.</p>
        </details>
      </section>

      <section className="ai-panel" aria-labelledby="ai-forecast-title" aria-busy={busy === 'ai'}>
        <header><h2 id="ai-forecast-title">AI forecast</h2>
          <p>The AI estimates the reservoir depth here from nearby wells, checks their logs for fluids, then fills in the form. You can change any value before locking.</p></header>
        {busy === 'ai' ? <div className="ai-progress" role="status"><span className="spinner" aria-hidden="true" />
          <div><strong>Drafting the forecast… {aiElapsed}s</strong><small>Usually 60–120 seconds.</small></div>
          <button type="button" onClick={() => { aiRequest.current?.abort(); aiRequest.current = undefined; setBusy('') }}>Cancel</button></div>
          : <div className="button-row">
            <button type="button" className="primary" disabled={!aiStatus?.configured || Boolean(busy)} onClick={() => void askAi()}>{ai ? 'Draft again with AI' : 'Draft forecast with AI'}</button>
            <button type="button" disabled={Boolean(busy)} onClick={() => { try { setText(modelReadout?.columns.length
              ? fillFromReservoirModel(text, context!, modelReadout, coverage?.maxTrueVerticalDepthM) : fillDemoForecast(text, context!)) } catch (reason) { setError(String(reason)) } }}>
              {modelReadout?.columns.length ? 'Fill from reservoir model' : 'Fill example values'}</button>
          </div>}
        {aiStatus && !aiStatus.configured && <p className="empty-note">AI is not connected. Fill the values yourself or start from the reservoir model.</p>}
        {ai && <div className="ai-result">
          <p className="ai-headline">Filled in from {ai.structureEstimate.controlCount} nearby wells.</p>
          <p>{pays[0].toUpperCase() + pays.slice(1)}: AI forecasts {metres(ai.draft.expectedPaydirtM.p50)} ({metres(ai.draft.expectedPaydirtM.p90)} – {metres(ai.draft.expectedPaydirtM.p10)})
            against the screening estimate of {metres(target.p50NetPayM)}.</p>
          {ai.structureEstimate.topTrueVerticalDepthM && <p>Nearby wells put the reservoir top at about {metres(ai.structureEstimate.topTrueVerticalDepthM.p50)} here
            ({metres(ai.structureEstimate.topTrueVerticalDepthM.p90)} – {metres(ai.structureEstimate.topTrueVerticalDepthM.p10)}).</p>}
          <h3>Watch out for</h3><ul>{ai.draft.limitations.map((item, index) => <li key={index}>{item}</li>)}</ul>
          <details className="plain-details"><summary>Full AI reasoning</summary><p>{ai.draft.rationale}</p></details>
        </div>}
      </section>
    </div>}
  </section>
}
