import { useEffect, useRef, useState } from 'react'
import { getPrediction, savePrediction, sealPrediction } from '../api'
import { parsePrediction, predictionDraftKey, predictionSealAttemptKey, predictionTemplate, predictionText, readLocalPredictionDraft, readSealActionKey } from '../prediction'
import type { LocalPredictionDraft } from '../prediction'
import type { FieldPackage, PredictionRecord, RankedCandidate, Scenario } from '../types'
import { configurationKey, defaultConfiguration, downloadJson } from '../analysisConfiguration'
import {
  createPredictionBinding, isInitialPredictionPackage, predictionBindingVersion, predictionSourceBlock,
  rebuildPredictionBinding, validatePredictionSource,
} from '../predictionBinding'
import type { PredictionSourceContext } from '../predictionBinding'
import { demoForecastAssumption, fillDemoForecast, readPredictionForm } from '../predictionExamples'
import { PredictionForecastForm } from './PredictionForecastForm'

export function PredictionEditor({ scenario, fieldPackage, prediction, candidate, onSaved, handoffBlock, sourceContext, onDraftState }: {
  scenario: Scenario
  fieldPackage: FieldPackage
  prediction?: PredictionRecord
  candidate?: RankedCandidate
  onSaved: (prediction: PredictionRecord) => void
  handoffBlock?: string
  sourceContext?: PredictionSourceContext
  onDraftState?: (ready: boolean) => void
}) {
  const key = predictionDraftKey(scenario.scenarioId)
  const [initial] = useState(() => {
    const canBind = sourceContext && !handoffBlock && !predictionSourceBlock(sourceContext)
    const binding = canBind ? createPredictionBinding(sourceContext) : undefined
    const configured = sourceContext?.analysis.configuration &&
      configurationKey(sourceContext.analysis.configuration) !== configurationKey(defaultConfiguration)
    const fallback: LocalPredictionDraft = {
      text: prediction ? predictionText(prediction.body) : predictionTemplate(scenario, fieldPackage,
        handoffBlock || configured && !binding ? undefined : candidate, binding),
      baseRevision: prediction?.revision ?? null,
      ...(prediction?.body.analysisBinding || binding ? { requiresBinding: true } : {}),
    }
    let cached: string | null = null
    try {
      cached = sessionStorage.getItem(key)
      return { draft: cached ? readLocalPredictionDraft(cached) : fallback, error: '' }
    } catch (error) {
      return { draft: cached ? { ...fallback, text: cached } : fallback, error: `Local draft could not be restored. Its raw contents are retained for download: ${String(error)}` }
    }
  })
  const [draft, setDraft] = useState(initial.draft)
  const [saved, setSaved] = useState(prediction)
  const [error, setError] = useState('')
  const [storageError, setStorageError] = useState(initial.error)
  const [busy, setBusy] = useState(false)
  const [reviewed, setReviewed] = useState(false)
  const [message, setMessage] = useState('')
  const active = useRef(true)
  const draftCallback = useRef(onDraftState)
  draftCallback.current = onDraftState
  useEffect(() => { active.current = true; return () => { active.current = false } }, [])

  const immutable = Boolean(saved?.seal || saved?.approval) || !['Draft', 'Armed', 'PredictionDrafted'].includes(scenario.status)
  const localDiffers = !saved || draft.text !== predictionText(saved.body)
  const dirty = !immutable && localDiffers
  const matchingScope = isInitialPredictionPackage(scenario, fieldPackage)
  let requiresRebuild = false
  let documentBinding = false
  try {
    const parsed = JSON.parse(draft.text) as { analysisBinding?: { version?: string } | null }
    documentBinding = Boolean(parsed?.analysisBinding)
    requiresRebuild = Boolean(parsed?.analysisBinding && parsed.analysisBinding.version !== predictionBindingVersion)
  } catch { /* Incomplete JSON remains editable and is rejected on explicit save. */ }
  const bindingOrigin = Boolean(draft.requiresBinding || saved?.body.analysisBinding || documentBinding)
  let sourceError = ''
  let sealSourceError = ''
  if (!immutable) {
    try {
      const body = parsePrediction(draft.text)
      if (bindingOrigin && !body.analysisBinding) throw new Error('This draft originated with an analysis binding. Rebuild explicitly; stripping metadata cannot relabel it as legacy.')
      if (sourceContext) validatePredictionSource(body, sourceContext)
      else if (body.analysisBinding) throw new Error('A verified frozen-source context is required for a bound prediction.')
    } catch (reason) { sourceError = reason instanceof Error ? reason.message : String(reason) }
    if (saved && sourceContext) {
      try { validatePredictionSource(saved.body, sourceContext, 'seal') }
      catch (reason) { sealSourceError = String(reason) }
    }
  }
  const rebuildBlock = !sourceContext ? 'A verified source analysis is required to build a binding.'
    : handoffBlock || predictionSourceBlock(sourceContext)
  const canSeal = !immutable && Boolean(saved) && !dirty && !busy && !error && !storageError &&
    matchingScope && !handoffBlock && !sourceError && !sealSourceError && draft.baseRevision === saved?.revision && saved?.body.fieldPackageSha256 === fieldPackage.sha256
  const draftReady = !immutable && !busy && !sourceError && !handoffBlock && matchingScope && !requiresRebuild
  useEffect(() => { draftCallback.current?.(draftReady) }, [draftReady, draft.text])

  useEffect(() => {
    if (!dirty) return
    const warn = (event: BeforeUnloadEvent) => event.preventDefault()
    window.addEventListener('beforeunload', warn)
    return () => window.removeEventListener('beforeunload', warn)
  }, [dirty])

  function edit(next: LocalPredictionDraft) {
    const retained = { ...next, ...(bindingOrigin || next.requiresBinding ? { requiresBinding: true } : {}) }
    setDraft(retained)
    setReviewed(false)
    setMessage('')
    try { sessionStorage.setItem(key, JSON.stringify(retained)); setStorageError('') }
    catch (error) { setStorageError(`Local backup failed. Keep this page open or download the draft: ${String(error)}`) }
  }
  function accept(record: PredictionRecord) {
    setSaved(record)
    setDraft({ text: predictionText(record.body), baseRevision: record.revision, ...(record.body.analysisBinding ? { requiresBinding: true } : {}) })
    setReviewed(false)
    try { sessionStorage.removeItem(key); setStorageError('') }
    catch (error) { setStorageError(`Saved on the server, but local backup cleanup failed: ${String(error)}`) }
    onSaved(record)
  }
  async function save() {
    if (handoffBlock || immutable || requiresRebuild || sourceError) return
    setError('')
    setBusy(true)
    setReviewed(false)
    try {
      const body = parsePrediction(draft.text)
      if (bindingOrigin && !body.analysisBinding) throw new Error('Do not strip the existing analysis binding. Use the explicit rebuild action.')
      if (sourceContext) validatePredictionSource(body, sourceContext)
      else if (body.analysisBinding) throw new Error('A verified frozen-source context is required for configured save.')
      if (body.fieldPackageSha256 !== fieldPackage.sha256) throw new Error('Use the current scenario package hash shown below; the draft cites a different package.')
      const record = await savePrediction(scenario.scenarioId, body, draft.baseRevision)
      if (active.current) { accept(record); setMessage(`Saved revision ${record.revision}. Review the saved ledger before sealing.`) }
    } catch (error) {
      if (active.current) setError(`${error instanceof Error ? error.message : String(error)} Your text is retained. Load the latest saved draft to resolve conflicts; nothing was overwritten automatically.`)
    } finally { if (active.current) setBusy(false) }
  }
  async function seal() {
    if (!canSeal || !reviewed || !saved) return
    setBusy(true)
    setError('')
    setReviewed(false)
    try {
      if (sourceContext) validatePredictionSource(saved.body, sourceContext, 'seal')
      const sealKey = predictionSealAttemptKey(scenario.scenarioId, saved.revision)
      const cached = sessionStorage.getItem(sealKey)
      const actionKey = cached ? readSealActionKey(cached) : crypto.randomUUID()
      sessionStorage.setItem(sealKey, actionKey)
      const record = await sealPrediction(scenario.scenarioId, saved.revision, actionKey)
      if (active.current) accept(record)
      let cleanupWarning = ''
      try { sessionStorage.removeItem(sealKey) }
      catch (reason) { cleanupWarning = ` The confirmed seal's local retry key could not be removed: ${String(reason)}` }
      if (active.current) setMessage(`Prediction sealed. Human approval is still required before execution.${cleanupWarning}`)
    } catch (error) {
      if (active.current) setError(`${String(error)} Reload and review the saved draft before trying again. A saved attempt key is reused for this exact revision; no retry is automatic and no rollback is assumed.`)
    } finally { if (active.current) setBusy(false) }
  }
  async function reload() {
    if (localDiffers && !window.confirm('Replace your local edits with the latest saved draft? Download them first if you need to keep a copy.')) return
    setBusy(true)
    setError('')
    setReviewed(false)
    try {
      const record = await getPrediction(scenario.scenarioId)
      if (active.current) {
        if (record) accept(record)
        else setError('No saved draft exists. Your local text is retained.')
      }
    } catch (error) { if (active.current) setError(String(error)) }
    finally { if (active.current) setBusy(false) }
  }
  function download() {
    const url = URL.createObjectURL(new Blob([draft.text], { type: 'application/json' }))
    const link = document.createElement('a')
    link.href = url
    link.download = `prediction-${scenario.scenarioId}.json`
    link.click()
    URL.revokeObjectURL(url)
  }
  function rebuild() {
    if (!sourceContext || rebuildBlock || immutable || busy) return
    if (!window.confirm('Explicitly rebuild candidate, pay, package and binding metadata from the selected frozen-source result? All existing path stations and other human inputs are retained, not flattened or invented. Download your old JSON first if needed.')) return
    try {
      const text = rebuildPredictionBinding(draft.text, sourceContext)
      edit({ ...draft, text, requiresBinding: true })
      setError('')
      setMessage('Binding explicitly rebuilt. Review all retained human inputs; directional coordinates and stale citations are not repaired automatically.')
    } catch (reason) { setError(String(reason)) }
  }
  function newPointTemplate() {
    if (!sourceContext || rebuildBlock || immutable || busy) return
    if (!window.confirm('Start a NEW point-screening template at the selected frozen-source target? This replaces local path/forecast text with blank human inputs; it does not convert or flatten an edited directional path. Download the current JSON first if needed. The saved server revision is unchanged until Save.')) return
    try {
      const binding = createPredictionBinding(sourceContext)
      edit({ ...draft, text: predictionTemplate(scenario, sourceContext.fieldPackage, sourceContext.candidate, binding), requiresBinding: true })
      setError('')
      setMessage('New point-screening template created locally. Depths, formation/contact/fluid and production forecasts still require explicit human inputs.')
    } catch (reason) { setError(String(reason)) }
  }
  function useExamples() {
    if (!sourceContext || rebuildBlock || immutable || busy || requiresRebuild) return
    try {
      const text = fillDemoForecast(draft.text, sourceContext)
      edit({ ...draft, text, requiresBinding: true })
      setError('')
      setMessage('Editable demo examples filled only blank forecast fields. Existing values and path stations were retained. Review every estimate before Save draft.')
    } catch (reason) { setError(String(reason)) }
  }

  return <section className="prediction-editor" aria-labelledby="prediction-editor-title">
    <header>
      <h3 id="prediction-editor-title">{immutable ? 'Prediction document' : 'Draft prediction'}</h3>
      {immutable
        ? <p>Review the saved prediction in the ledger below. Download its exact document for a separate copy.</p>
        : <p>Complete the forecast below, save for validation, then review the saved ledger.
          Seal freezes that revision; Approve prediction and Start simulation are separate actions.</p>}
    </header>
    {error && <p className="prediction-error" role="alert">{error}</p>}
    {storageError && <p className="prediction-error" role="alert">{storageError}</p>}
    {handoffBlock && !immutable && <p className="prediction-error" role="status">{handoffBlock} Existing saved and sealed records remain unchanged.</p>}
    {requiresRebuild && !immutable && <p className="prediction-error" role="status">This unversioned or unsupported bound draft needs an explicit rebuild. Its metadata is preserved; saving/sealing cannot silently upgrade or strip it.</p>}
    {immutable && saved && <div className="prediction-help">
      <p><strong>Saved revision {saved.revision} · {saved.approval ? 'Approved' : saved.seal ? 'Sealed' : 'Read only'}</strong>.
        This historical record is unchanged by current settings or local unsaved text.</p>
      <p>Scenario reference: <code>{saved.scenarioId}</code>{saved.seal && <> · seal fingerprint: <code>{saved.seal.sha256}</code></>}</p>
      {saved.seal && !saved.approval && <a href="#operator-title">Review and approve this prediction</a>}
    </div>}
    {immutable && !saved && <p className="prediction-error" role="status">No saved prediction record is loaded. Load the latest saved draft to inspect it; any local backup remains available for download.</p>}
    {immutable && saved && localDiffers && <p>A separate local backup differs from this immutable record. Download local JSON to preserve it; loading the saved draft will request confirmation before replacing that backup.</p>}
    {!matchingScope && !immutable && <p className="prediction-error" role="alert">The current source package is not ready. Refresh the scenario before saving.</p>}
    <details className="prediction-help">
      <summary>Prediction contract and units</summary>
      <p>The candidate template copies only the selected ranking's pay quantiles, local easting/northing, and cited controls.
        Supply missing inputs yourself, or explicitly fill the editable demonstration examples.
        MD/TVD are metres, positive-down; horizontal positions are field-local metres, not WGS84.</p>
      <p>The ranking quantiles are a qualifying-rock proxy, not fluid-conditioned hydrocarbon pay. Review that distinction before authoring expected paydirt and fluid assumptions.</p>
      <p>Versioned binding v1 is point-screening only: every station must retain the selected target's exact easting/northing.
        Directional edits are blocked, never silently flattened. Depths, formations, fluids, contacts and production forecasts remain human inputs.</p>
      <p>Supply at least two increasing-MD stations, formation top/base P90 ≤ P50 ≤ P10,
        expected-pay quantiles, fluidClasses (Oil/Gas/Water), optional contactPredictions (GOC/GWC/OWC with depth quantiles),
        cumulative oil/gas/water forecasts in m³ for years 1/3/5, uncertainty assumptions, visible evidence IDs, and rationale.</p>
    </details>
    {!immutable && <>
      <div className="prediction-demo" data-tutorial="demo-forecast">
        <h4>Editable demo forecast</h4>
        <p>Fill blank depth, formation, fluid and cumulative-production fields with clearly labeled examples.
          These are demonstration estimates, not evidence-derived truth or calibration. Your existing values are kept.</p>
        <button type="button" disabled={busy || Boolean(rebuildBlock) || requiresRebuild} onClick={useExamples}>Use editable demo forecast</button>
        <button type="button" disabled={busy || Boolean(rebuildBlock)} onClick={newPointTemplate}>Start new point-screening template</button>
        {readPredictionForm(draft.text)?.uncertaintyAssumptions.includes(demoForecastAssumption) &&
          <p className="demo-estimate-label">Demonstration estimates are present. Review and edit them below before saving.</p>}
      </div>
      <PredictionForecastForm text={draft.text} disabled={busy || requiresRebuild}
        onChange={text => { setError(''); edit({ ...draft, text }) }} />
      <details className="prediction-advanced">
        <summary>Advanced · prediction JSON and binding</summary>
        <label htmlFor="prediction-json">Prediction JSON · {draft.baseRevision === null ? 'new draft' : `based on revision ${draft.baseRevision}`}</label>
        <textarea id="prediction-json" value={draft.text} readOnly={busy || requiresRebuild} spellCheck={false} rows={22}
          onChange={(event) => { setError(''); edit({ ...draft, text: event.target.value }) }} />
        <button type="button" disabled={busy || Boolean(rebuildBlock)} onClick={rebuild}>Rebuild binding from selected frozen target</button>
        <p>Changing the target does not rewrite this document. Rebuild retains all path stations and human inputs.
          Use a new point template only when you explicitly intend to replace local geometry and forecasts.</p>
      </details>
    </>}
    <p className="prediction-package">{immutable && saved ? 'Saved prediction package' : matchingScope ? 'Frozen initial source package' : 'Displayed package (not a valid frozen source)'}
      {' '}<code>{immutable && saved ? saved.body.fieldPackageSha256 : fieldPackage.sha256}</code></p>
    <div className="prediction-actions">
      {!immutable && <button type="button" data-tutorial="save-prediction" disabled={busy || !matchingScope || Boolean(handoffBlock || sourceError) || requiresRebuild} onClick={() => void save()}>Save draft</button>}
      <button type="button" disabled={busy} onClick={() => void reload()}>Load latest saved draft</button>
      <button type="button" onClick={download}>Download local JSON</button>
      {immutable && saved && <button type="button" onClick={() => downloadJson(saved.body, `prediction-${scenario.scenarioId}-r${saved.revision}.json`)}>Download immutable saved JSON</button>}
    </div>
    {!immutable && rebuildBlock && <p>{rebuildBlock}</p>}
    {!immutable && sourceError && <p className="prediction-error" role="status">{sourceError}</p>}
    {!immutable && sealSourceError && <p className="prediction-error" role="status">Seal prerequisite: {sealSourceError}</p>}
    <p role="status">{busy ? 'Waiting for the ledger…' : message || (dirty ? 'Local edits are not saved to the server.' : saved ? `Saved revision ${saved.revision}.` : 'No saved prediction record is loaded.')}</p>
    {!immutable && <div className="prediction-seal" data-tutorial="seal-prediction">
      <label><input type="checkbox" data-tutorial="review-prediction-seal" checked={reviewed} disabled={!canSeal} onChange={(event) => setReviewed(event.target.checked)} />
        I reviewed saved revision {saved?.revision ?? '—'} and its assumptions. Sealing is permanent.
      </label>
      <button type="button" disabled={!canSeal || !reviewed} onClick={() => void seal()}>Seal reviewed revision</button>
      <details><summary>Seal and retry safeguards</summary><p>Sealing requires the local operator session. A stable key is retained for this exact revision before the request; an uncertain response is never retried automatically.</p></details>
      {!canSeal && <p>Save and resolve any errors before sealing. Unsaved or stale revisions cannot be sealed here.</p>}
    </div>}
  </section>
}
