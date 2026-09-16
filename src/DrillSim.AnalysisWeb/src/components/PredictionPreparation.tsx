import { useEffect, useRef, useState } from 'react'
import { applyAnalysis, getFieldPackage, getPredictionCapabilities } from '../api'
import { configurationKey, verifyAppliedAnalysis } from '../analysisConfiguration'
import { isInitialPredictionPackage, predictionCapabilityBlock, predictionSourceBlock } from '../predictionBinding'
import type { PredictionPreparationState } from '../predictionBinding'
import type { AnalysisResult, FieldPackage, PredictionCapabilities, PredictionRecord, RankedCandidate, Scenario } from '../types'
import { PredictionEditor } from './PredictionEditor'

export function PredictionPreparation({ scenario, fieldPackage, analysis, candidate, prediction, onSaved, handoffBlock, onPrepared }: {
  scenario: Scenario; fieldPackage: FieldPackage; analysis: AnalysisResult; candidate?: RankedCandidate
  prediction?: PredictionRecord; onSaved: (value: PredictionRecord) => void; handoffBlock?: string
  onPrepared?: (state: PredictionPreparationState) => void
}) {
  const [capabilities, setCapabilities] = useState<PredictionCapabilities>()
  const [capabilityError, setCapabilityError] = useState('')
  const [loadingCapabilities, setLoadingCapabilities] = useState(true)
  const [refresh, setRefresh] = useState(0)
  const [source, setSource] = useState<{ identity: string; pkg: FieldPackage; analysis: AnalysisResult }>()
  const [selection, setSelection] = useState({ identity: '', candidateId: '' })
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [forecastReady, setForecastReady] = useState(false)
  const preparedCallback = useRef(onPrepared)
  preparedCallback.current = onPrepared
  const sourceRequest = useRef<AbortController | undefined>(undefined)
  const sourceIdentity = JSON.stringify([scenario.scenarioId, scenario.sourceFieldId, scenario.initialAsOfUtc,
    analysis.configuration ? configurationKey(analysis.configuration) : null])
  const identity = useRef(sourceIdentity)
  identity.current = sourceIdentity
  useEffect(() => {
    const controller = new AbortController()
    setLoadingCapabilities(true)
    setCapabilityError('')
    getPredictionCapabilities(controller.signal).then(value => {
      if (controller.signal.aborted) return
      setCapabilities(value)
      if (!value) setCapabilityError('The prediction-capabilities endpoint is absent. Configured handoff cannot be advertised; existing historical records remain readable.')
    }).catch(reason => {
      if (!controller.signal.aborted) { setCapabilities(undefined); setCapabilityError(`Prediction capabilities could not be read: ${String(reason)}`) }
    }).finally(() => { if (!controller.signal.aborted) setLoadingCapabilities(false) })
    return () => controller.abort()
  }, [refresh])
  useEffect(() => {
    sourceRequest.current?.abort()
    setBusy(false)
    return () => sourceRequest.current?.abort()
  }, [sourceIdentity])
  const currentIsSource = isInitialPredictionPackage(scenario, fieldPackage)
  const cached = source?.identity === sourceIdentity ? source : undefined
  const effectivePackage = cached?.pkg ?? fieldPackage
  const effectiveAnalysis = cached?.analysis ?? analysis
  const targetIdentity = JSON.stringify([sourceIdentity, effectivePackage.sha256, effectiveAnalysis.analysisSha256])
  const eligible = effectiveAnalysis.candidateGrid?.flatMap(cell => cell.status === 'eligible' && cell.prediction ? [cell.prediction] : []) ?? []
  const candidateId = selection.identity === targetIdentity ? selection.candidateId
    : prediction?.body.candidateId ?? (currentIsSource && !cached && prediction ? candidate?.candidateId ?? '' : '')
  const selected = eligible.find(item => item.candidateId === candidateId)
  const context = { scenario, fieldPackage: effectivePackage, analysis: effectiveAnalysis, candidate: selected, capabilities }
  const problem = predictionSourceBlock(context)
  const sourceProblem = predictionSourceBlock(context, false)
  useEffect(() => {
    if (sourceProblem || busy || handoffBlock || !cached && !prediction?.body.analysisBinding) return
    preparedCallback.current?.({
      scenarioId: scenario.scenarioId, sourceFieldId: scenario.sourceFieldId, asOfUtc: scenario.initialAsOfUtc,
      configurationKey: configurationKey(effectiveAnalysis.configuration!), analysisSha256: effectiveAnalysis.analysisSha256!,
      packageSha256: effectivePackage.sha256, candidateId: selected?.candidateId, forecastReady,
    })
  }, [sourceProblem, busy, handoffBlock, sourceIdentity, cached, prediction?.body.analysisBinding, effectiveAnalysis.analysisSha256, effectivePackage.sha256, selected?.candidateId, forecastReady])
  async function loadSource() {
    if (!analysis.configuration || predictionCapabilityBlock(capabilities) || busy) return
    const expectedIdentity = sourceIdentity
    const config = { ...analysis.configuration }
    const controller = new AbortController()
    sourceRequest.current?.abort(); sourceRequest.current = controller
    setBusy(true); setError('')
    try {
      const scope = { scenarioId: scenario.scenarioId, asOf: scenario.initialAsOfUtc }
      const pkg = await getFieldPackage(scenario.sourceFieldId, scope, controller.signal)
      const result = await applyAnalysis(scenario.sourceFieldId, scenario.reservoirName, config, scope, controller.signal)
      if (controller.signal.aborted || identity.current !== expectedIdentity) return
      verifyAppliedAnalysis(result, pkg, scenario.reservoirName, config)
      if (!isInitialPredictionPackage(scenario, pkg)) throw new Error('The source API did not return the frozen initial scenario package.')
      setSource({ identity: expectedIdentity, pkg, analysis: result })
      setSelection({ identity: JSON.stringify([expectedIdentity, pkg.sha256, result.analysisSha256]), candidateId: '' })
    } catch (reason) { if (!controller.signal.aborted) setError(String(reason)) }
    finally { if (!controller.signal.aborted) setBusy(false) }
  }
  return <>
    <section className="prediction-editor" data-tutorial="prediction-source">
      <h3>Analyze the pre-drill evidence</h3>
      <p>Use the original field at the scenario’s evidence cutoff: {scenario.initialAsOfUtc}.
        Analyze with your applied settings, then choose an eligible target.</p>
      {loadingCapabilities && <p role="status">Reading supported binding and geometry versions…</p>}
      {capabilityError && <p className="prediction-error" role="alert">{capabilityError}</p>}
      {capabilities && <details>
        <summary>Supported analysis, geometry and baseline versions</summary>
        <p>{capabilities.sourceScope}</p><p>{capabilities.targetRule}</p><p>{capabilities.limitation}</p>
        <p>Configured save: {capabilities.configuredSaveSupported ? 'Supported' : 'Unavailable'} · configured seal: {capabilities.configuredSealSupported ? 'Supported' : 'Unavailable'}.
          {' '}Minimum located controls: {capabilities.minimumLocatedControls}. Binding: <code>{capabilities.bindingVersion}</code>.</p>
        <p>{capabilities.fourNeighborBaselineRule}</p>
        <details><summary>Baseline versions and compatibility</summary>
          <p>Versioned point-screening bindings freeze configured v3 baselines. Nearest-well and field-mean baselines use the configuration; FourNeighborIDW always uses four controls independently of the main neighbor count; rank 1 uses the main configuration.
            Legacy no/null-binding records retain their original v2 behavior and bytes. Loading an old draft does not upgrade it.</p>
          <ul>{capabilities.baselineModelVersions.map(version => <li key={version}><code>{version}</code></li>)}</ul>
        </details>
      </details>}
      {problem && <p className="prediction-error" role="status">{problem}</p>}
      {error && <p className="prediction-error" role="alert">{error}</p>}
      <div className="prediction-actions">
        <button type="button" disabled={loadingCapabilities || busy} onClick={() => setRefresh(value => value + 1)}>Refresh handoff capabilities</button>
        <button type="button" data-tutorial="analyze-frozen-source" disabled={Boolean(predictionCapabilityBlock(capabilities)) || !analysis.configuration || busy || Boolean(handoffBlock)}
          onClick={() => void loadSource()}>{busy ? 'Analyzing frozen source…' : 'Analyze frozen initial source with applied settings'}</button>
      </div>
      <p>Applied settings are copied exactly. This calculation does not change the displayed field or overwrite your forecast.</p>
      <details><summary>Frozen analysis fingerprints</summary><p>Preparation result: <code>{effectiveAnalysis.analysisSha256 ?? 'Unavailable'}</code> · configuration <code>{effectiveAnalysis.configurationSha256 ?? 'Unavailable'}</code>
        {' '}· package <code>{effectivePackage.sha256}</code>. These identify the template/rebuild source, not an automatic change to an existing draft.</p>
      </details>
      {isInitialPredictionPackage(scenario, effectivePackage) && <label className="rationale-field">Frozen-source eligible target · template/rebuild source<select
        data-tutorial="prediction-target"
        aria-label="Frozen-source eligible target · template/rebuild source" value={candidateId}
        disabled={busy || Boolean(handoffBlock)} onChange={event => setSelection({ identity: targetIdentity, candidateId: event.target.value })}>
        <option value="">Select an eligible full-grid candidate</option>{eligible.map(item =>
          <option key={item.candidateId} value={item.candidateId}>{item.candidateId} · P50 {item.p50NetPayM.toFixed(2)} m rock proxy</option>)}
      </select></label>}
      {selected && <p>Selected preparation point: E {selected.eastingM} m / N {selected.northingM} m · P90/P50/P10 {selected.p90NetPayM} / {selected.p50NetPayM} / {selected.p10NetPayM} m qualifying-rock proxy.</p>}
    </section>
    {!loadingCapabilities && <PredictionEditor key={scenario.scenarioId} scenario={scenario} fieldPackage={effectivePackage}
      prediction={prediction} candidate={selected} sourceContext={context} onSaved={onSaved}
      onDraftState={setForecastReady}
      handoffBlock={busy ? 'Wait for the frozen-source calculation to finish.' : handoffBlock} />}
  </>
}
