import { useEffect, useRef, useState } from 'react'
import { createScenario } from '../api'
import {
  prepareScenarioCreation, readScenarioCreationAttempt, scenarioCreationError, scenarioCreationKey,
  verifyScenarioCreationAttempt, type ScenarioCreationAttempt,
} from '../scenarioCreation'
import type { AnalysisConfiguration, Scenario } from '../types'
import './SimulationForms.css'

export function ScenarioCreation({ sourceFieldId, sourceFieldName, reservoirName, configuration, onCreated, onClose }: {
  sourceFieldId: string
  sourceFieldName: string
  reservoirName: string
  configuration: AnalysisConfiguration
  onCreated: (scenario: Scenario, configuration: AnalysisConfiguration) => void
  onClose: () => void
}) {
  const storageKey = scenarioCreationKey(sourceFieldId, reservoirName)
  const [initial] = useState(() => {
    try {
      const text = sessionStorage.getItem(storageKey)
      return { attempt: text ? readScenarioCreationAttempt(text, sourceFieldId, reservoirName) : undefined, error: '' }
    } catch (reason) { return { attempt: undefined, error: String(reason) } }
  })
  const [pending, setPending] = useState(initial.attempt)
  const [name, setName] = useState(initial.attempt?.payload.seedLabel ?? `Guided simulation ${new Date().toISOString().slice(0, 19).replace('T', ' ')} UTC`)
  const [asOfUtc, setAsOfUtc] = useState(initial.attempt?.payload.asOfUtc ?? new Date().toISOString())
  const [purpose, setPurpose] = useState(initial.attempt?.reviewed.purpose ?? 'Compare a pre-drill forecast with a new synthetic well and five years of production.')
  const [assumptions, setAssumptions] = useState(initial.attempt?.reviewed.assumptions ??
    'Use the original field measurements available at the UTC cutoff. Review an eligible fixed-location target. Editable depth, formation, fluid and production examples are demonstration estimates, not calibrated predictions.')
  const [reviewed, setReviewed] = useState(false)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState(initial.error)
  const active = useRef(true)
  useEffect(() => { active.current = true; return () => { active.current = false } }, [])
  const input = { sourceFieldId, reservoirName, name, asOfUtc, purpose, assumptions, configuration }
  const invalid = scenarioCreationError(input)
  const settings = pending?.reviewed.initialAnalysisSettings ?? configuration
  const locked = busy || Boolean(pending || initial.error)

  async function submit() {
    if (busy || initial.error || !reviewed || !pending && invalid) return
    setBusy(true)
    setError('')
    let attempt: ScenarioCreationAttempt | undefined = pending
    let retained = false
    try {
      attempt ??= await prepareScenarioCreation(input)
      if (!active.current) return
      await verifyScenarioCreationAttempt(attempt)
      if (!active.current) return
      sessionStorage.setItem(storageKey, JSON.stringify(attempt))
      retained = true
      setPending(attempt)
      const scenario = await createScenario(attempt.payload)
      if (!active.current) return
      sessionStorage.removeItem(storageKey)
      onCreated(scenario, attempt.reviewed.initialAnalysisSettings)
    } catch (reason) {
      if (active.current) setError(`${String(reason)} ${retained
        ? 'Keep this request unchanged. After inspecting the scenario list, retry this same creation request; an uncertain response does not mean creation was rolled back.'
        : 'No request was sent. Local retry storage and browser SHA-256 must be available before creation.'}`)
    } finally { if (active.current) { setBusy(false); setReviewed(false) } }
  }

  return <section className="simulation-form scenario-creation" data-tutorial="scenario-create" aria-labelledby="scenario-create-title">
    <header className="simulation-form-heading">
      <div><h2 id="scenario-create-title">Create a new simulation</h2><p>Original field: <strong>{sourceFieldName}</strong> · {reservoirName}.
        Existing scenarios and their results stay unchanged.</p></div>
      <button type="button" onClick={onClose}>Close setup</button>
    </header>
    <form onSubmit={event => { event.preventDefault(); void submit() }}>
      <fieldset disabled={locked}>
        <legend>Scenario purpose and evidence</legend>
        <div className="simulation-form-grid">
          <label>Scenario name<input data-tutorial="scenario-name" type="text" maxLength={100} required value={name}
            onChange={event => { setName(event.target.value); setReviewed(false) }} /></label>
          <label>Evidence cutoff (UTC)<input type="text" required spellCheck={false} value={asOfUtc}
            aria-describedby="scenario-cutoff-help" onChange={event => { setAsOfUtc(event.target.value); setReviewed(false) }} /></label>
        </div>
        <p id="scenario-cutoff-help">Only evidence available at this time can inform the prediction. Include Z for UTC, for example 2026-09-16T12:00:00Z.
          The scenario name is a label, not a secret seed.</p>
        <label>Simulation purpose<textarea required rows={2} value={purpose} onChange={event => { setPurpose(event.target.value); setReviewed(false) }} /></label>
        <label>Reviewed scenario assumptions<textarea required rows={3} value={assumptions} onChange={event => { setAssumptions(event.target.value); setReviewed(false) }} /></label>
      </fieldset>
      <p>Initial analysis settings: porosity ≥ {settings.porosityCutoff}, permeability ≥ {settings.permeabilityCutoffM2} m²,
        {' '}{settings.gridPointsPerAxis} × {settings.gridPointsPerAxis} search grid, {settings.wellExclusionRadiusM} m exclusion radius,
        {' '}{settings.idwNeighborCount} neighbors. You can explicitly apply different settings before preparing the prediction.</p>
      <p>Next: analyze the earlier measurements, choose a target and review an editable demo forecast. Save, Seal, Approve and Start remain separate actions.</p>
      {pending && <p className="operator-notice" role="status">A creation request is retained. Its name, UTC cutoff and assumptions are locked for an exact retry.
        An identical request reuses the same scenario; it never starts another run.</p>}
      {error && <p className="form-error" role="alert">{error}</p>}
      {!pending && invalid && <p className="form-error" role="status">{invalid}</p>}
      <label className="simulation-check"><input type="checkbox" checked={reviewed} disabled={busy || Boolean(initial.error || !pending && invalid)}
        onChange={event => setReviewed(event.target.checked)} />I reviewed the original field, UTC cutoff, purpose and assumptions for this new simulation.</label>
      <button type="submit" data-tutorial="create-scenario" disabled={busy || !reviewed || Boolean(initial.error || !pending && invalid)}>
        {busy ? 'Creating scenario…' : pending ? 'Retry same scenario creation' : 'Create scenario'}
      </button>
      <details><summary>Assumptions fingerprint and retry details</summary>
        <p>The browser fingerprints a versioned purpose, assumption and settings record with SHA-256; no hash entry is required.
          The retained request contains no credentials. Closing setup retains an unresolved request for this field and reservoir.</p>
        {pending && <><code>{pending.payload.assumptionsSha256}</code><pre>{JSON.stringify(pending.reviewed, null, 2)}</pre></>}
      </details>
    </form>
  </section>
}
