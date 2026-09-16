import { useEffect, useMemo, useRef, useState } from 'react'
import { configurationErrors, configurationKey, downloadJson, verifyAppliedAnalysis } from '../analysisConfiguration'
import { idOf, nameOf } from '../data'
import { visibleEvidenceIds } from '../evidence'
import {
  analyzeHypothesis, branchPreset, branchTarget, eligibleCandidates, hypothesisInput, hypothesisScopeKey,
  referenceOf, selectionOf,
} from '../hypotheses'
import type { HypothesisControlNote, HypothesisRevision, HypothesisScope } from '../hypotheses'
import type { AnalysisConfiguration, AnalysisResult, FieldPackage } from '../types'
import { buildWellViews } from './AnalysisWorkspace'
import { DraftWarning, HypothesisTable, useTextDraft } from './HypothesisSupport'
import type { HypothesisWrites } from './HypothesisSupport'
import { RecordedValue } from './HypothesisPresentation'
import { FormationInterpretationAgent } from './FormationInterpretationAgent'
import { formationNotesText } from '../formationInterpretation'

export const exampleCorrelationNote = 'Check whether differences in formation depth reflect geological structure or different depth references. Compare wells with logs against wells with formation intervals only before choosing a drilling target.'

export function parseControlNotes(text: string): HypothesisControlNote[] {
  const value: unknown = JSON.parse(text)
  if (!Array.isArray(value) || value.some((item) => !item || typeof item !== 'object' ||
    typeof item.evidenceId !== 'string' || typeof item.note !== 'string')) throw new Error('Control notes must be evidenceId/note records.')
  return value
}

export function HypothesisRevisionEditor({ pkg, analysis, scope, candidateId, selected, livePackage, liveScope, stale, writes, onSaved, correlation }: {
  pkg: FieldPackage; analysis: AnalysisResult; scope: HypothesisScope; candidateId: string
  selected?: HypothesisRevision; livePackage: FieldPackage; liveScope: HypothesisScope; stale: boolean
  writes: HypothesisWrites; onSaved: (value: HypothesisRevision) => void; correlation: boolean
}) {
  const identity = selected ? `${selected.hypothesisId}:${selected.revision}:${selected.snapshotSha256}` : `live:${pkg.sha256}:${analysis.analysisSha256}:${candidateId}`
  const draft = useTextDraft(`hypothesis-notes-v1:${hypothesisScopeKey(scope)}:${identity}`, {
    name: selected?.name ?? '', rationale: selected?.input.rationale ?? '',
    correlation: selected?.input.correlationNotes ?? '', controls: JSON.stringify(selected?.input.controlNotes ?? []),
  })
  const [error, setError] = useState('')
  const [controlId, setControlId] = useState('')
  const active = useRef(true)
  useEffect(() => { active.current = true; return () => { active.current = false } }, [])
  const controls = useMemo(() => {
    try { return { notes: parseControlNotes(draft.value.controls), error: '' } }
    catch (reason) { return { notes: [], error: String(reason) } }
  }, [draft.value.controls])
  const wells = useMemo(() => buildWellViews(pkg, analysis.wellSummaries, scope.reservoirName, analysis.configuration), [pkg, analysis, scope.reservoirName])
  const ids = useMemo(() => [...visibleEvidenceIds(pkg)].sort(), [pkg])
  const differsFromLive = pkg.sha256 !== livePackage.sha256 || hypothesisScopeKey(scope) !== hypothesisScopeKey(liveScope)
  const disabled = writes.busy || Boolean(writes.blocked || draft.error || controls.error) || (!selected && stale)
  const target = eligibleCandidates(analysis).find((item) => item.candidateId === candidateId)
  function editControl(evidenceId: string, note: string) {
    draft.set('controls', JSON.stringify(controls.notes.map((item) => item.evidenceId === evidenceId ? { ...item, note } : item)))
  }
  async function save(kind: 'create' | 'revise' | 'branch') {
    if (disabled || (kind === 'revise' && differsFromLive)) return
    setError('')
    try {
      if (!draft.value.name.trim() || draft.value.name.length > 120) throw new Error('Enter a hypothesis name of 1–120 characters.')
      const input = hypothesisInput(pkg, analysis, candidateId, draft.value.rationale, draft.value.correlation, controls.notes)
      const attempt = kind === 'revise' && selected
        ? { path: `/${selected.hypothesisId}/revisions`, body: { scope, input }, expectedVersion: selected.revision }
        : kind === 'branch' && selected
          ? { path: '/branches', body: { name: draft.value.name, scope, source: referenceOf(selected), input } }
          : { path: '', body: { name: draft.value.name, scope, input } }
      const result = await writes.send({ ...attempt, method: 'POST', kind: 'revision', label: kind === 'revise' ? 'Save hypothesis revision' : kind === 'branch' ? 'Save annotation branch' : 'Save named hypothesis' })
      if (active.current && result && 'snapshotSha256' in result) onSaved(result)
    } catch (reason) { setError(String(reason)) }
  }
  return <section className="hypothesis-editor" aria-labelledby="hypothesis-editor-title">
    <h3 id="hypothesis-editor-title">{selected ? `Notes for ${selected.name} · revision ${selected.revision}`
      : correlation ? 'Formation interpretation' : 'Save this interpretation'}</h3>
    <p>Save your reasoning with the selected target, analysis settings and field data.
      You can review these notes when comparing alternatives or revisiting the decision.</p>
    {selected && <p>These notes belong to the saved revision shown above. Saving a new revision keeps the earlier version.</p>}
    <DraftWarning error={draft.error} raw={draft.raw} />
    {(error || controls.error) && <p className="task-errors" role="alert">{error || controls.error}</p>}
    {correlation && analysis.configuration && analysis.analysisSha256 && analysis.configurationSha256 &&
      <FormationInterpretationAgent pkg={pkg} configurationSha256={analysis.configurationSha256} blocked={disabled}
        request={{ scope, configuration: analysis.configuration, packageSha256: pkg.sha256,
          analysisSha256: analysis.analysisSha256, selectedCandidateId: target?.candidateId ?? null,
          savedHypothesis: selected ? referenceOf(selected) : null, snapshotSha256: selected?.snapshotSha256 ?? null,
          notes: { name: draft.value.name, rationale: draft.value.rationale, correlationNotes: draft.value.correlation, controlNotes: controls.notes } }}
        onUse={value => draft.replace({ ...draft.value, name: draft.value.name.trim() ? draft.value.name : value.draft.name,
          rationale: value.draft.rationale, correlation: formationNotesText(value) })} />}
    <label className="rationale-field">{selected ? 'New branch name' : 'Hypothesis name'}<input maxLength={120} value={draft.value.name} onChange={(event) => draft.set('name', event.target.value)} /></label>
    {selected && <p>A new revision keeps the existing hypothesis name. The name field above names a new branch only.</p>}
    <p>Selected eligible target: <strong>{target?.candidateId ?? 'None available'}</strong> · P50 qualifying-rock proxy {target?.p50NetPayM.toFixed(2) ?? 'Unavailable'} m.</p>
    <label className="rationale-field">Reason for choosing this target<textarea aria-label="Reason for choosing this target" rows={4} maxLength={10000} value={draft.value.rationale} onChange={(event) => draft.set('rationale', event.target.value)} /></label>
    <p>Explain why this target is worth investigating. If you compared alternatives, name the versions and what influenced your choice.</p>
    <section data-tutorial="correlation-notes" aria-label="Formation interpretation notes">
      <label className="rationale-field">Correlation notes<textarea aria-label="Correlation notes" rows={3} maxLength={4000} value={draft.value.correlation} onChange={(event) => draft.set('correlation', event.target.value)} /></label>
      <p>Record how you relate the formations between wells and what still needs checking. These notes appear in saved comparisons;
        they do not change calculations. “Draft interpretation with AI” reads these notes when you request a draft.</p>
      <details>
        <summary>Example correlation note</summary>
        <p>{exampleCorrelationNote}</p>
        <button type="button" disabled={disabled || Boolean(draft.value.correlation.trim())}
          onClick={() => { if (!disabled && !draft.value.correlation.trim()) draft.set('correlation', exampleCorrelationNote) }}>Use example note</button>
        {draft.value.correlation.trim() && <p>Your note is kept. Clear the field first if you want to use the example instead.</p>}
      </details>
    </section>
    {correlation && <HypothesisTable label="Multi-bore formation and control annotation table">
      <caption>{selected ? 'Saved data' : 'Current field data'} · compare formation intervals and choose a wellbore for a specific note</caption>
      <thead><tr><th scope="col">Bore / control</th><th scope="col">Formation MD ranges · m</th><th scope="col">Located survey stations</th><th scope="col">Sample rows</th><th scope="col">Evidence / annotation</th></tr></thead>
      <tbody>{wells.map((well) => <tr key={well.id}>
        <th scope="row">{well.name}</th><td>{well.formationRanges.map((range) => `${range.top}–${range.base}`).join('; ') || 'No bounded interval'}</td>
        <td>{well.stations.length}</td><td>{well.samples.length}</td><td>
          <button type="button" onClick={() => setControlId(`wellbore:${well.wellBoreId}`)}>Select bore for note</button>
          <code>{well.wellBoreId}</code><br />{well.sourceLabel}
        </td>
      </tr>)}</tbody>
    </HypothesisTable>}
    <div className="task-controls"><label>Visible evidence for a control note<select aria-label="Visible evidence for a control note" value={controlId} onChange={(event) => setControlId(event.target.value)}>
      <option value="">Select an evidence record</option>{ids.map((id) => <option key={id}>{id}</option>)}
    </select></label><button type="button" disabled={!ids.includes(controlId) || controls.notes.length >= 32 || controls.notes.some((note) => note.evidenceId === controlId)}
      onClick={() => draft.set('controls', JSON.stringify([...controls.notes, { evidenceId: controlId, note: '' }]))}>Add control note</button></div>
    {controls.notes.map((note) => <div className="hypothesis-control-note" key={note.evidenceId}>
      <label className="rationale-field">Control note · {note.evidenceId}<textarea aria-label={`Control note · ${note.evidenceId}`} rows={2} maxLength={2000} value={note.note} onChange={(event) => editControl(note.evidenceId, event.target.value)} /></label>
      <button type="button" onClick={() => draft.set('controls', JSON.stringify(controls.notes.filter((item) => item.evidenceId !== note.evidenceId)))}>Remove local note</button>
    </div>)}
    <div className="task-actions">
      {!selected && <button type="button" disabled={disabled || !target} onClick={() => void save('create')}>Save hypothesis</button>}
      {selected && <>
        <button type="button" disabled={disabled || differsFromLive || !target} onClick={() => void save('revise')}>Save notes as revision {selected.revision + 1}</button>
        <button type="button" disabled={disabled || !target} onClick={() => void save('branch')}>Save notes as named snapshot branch</button>
      </>}
      <button type="button" onClick={() => downloadJson(draft.value, 'hypothesis-unsaved-notes.json')}>Download local text</button>
    </div>
    {differsFromLive && <p className="task-notice">The loaded snapshot differs from the current package/scope. Live revision saves are blocked to avoid relabeling historical evidence.
      A named annotation branch keeps this exact stored evidence. It does not make historical output current.</p>}
    {!selected && stale && <p className="task-notice">Apply or restore the pending analysis settings before capturing the current hypothesis.</p>}
    {selected && <p>Revision saves use <code>If-Match: "{selected.revision}"</code>. If someone saved a newer revision, inspect it explicitly and reconcile your retained text; no latest read silently rebases this editor.</p>}
    <InputReferences scope={scope} pkg={pkg} analysis={analysis} candidateId={candidateId} />
  </section>
}

function InputReferences({ scope, pkg, analysis, candidateId }: {
  scope: HypothesisScope; pkg: FieldPackage; analysis: AnalysisResult; candidateId: string
}) {
  const references = {
    scope, packageSha256: pkg.sha256, analysisSha256: analysis.analysisSha256, configurationSha256: analysis.configurationSha256,
    selectedCandidateId: candidateId, geology: pkg.geologicalProperties.map(idOf),
  }
  return <details><summary>Input fingerprints and source IDs</summary>
    <RecordedValue field="scope" value={scope} downloadLabel="Download input references JSON" />
    <dl>
      <div><dt>Package fingerprint</dt><dd><code>{pkg.sha256}</code></dd></div>
      <div><dt>Result fingerprint</dt><dd><code>{analysis.analysisSha256 || 'Not recorded'}</code></dd></div>
      <div><dt>Settings fingerprint</dt><dd><code>{analysis.configurationSha256 || 'Not recorded'}</code></dd></div>
      <div><dt>Selected target reference</dt><dd><code>{candidateId || 'Not selected'}</code></dd></div>
    </dl>
    <p>{pkg.geologicalProperties.length} geology records in this package.</p>
    {pkg.geologicalProperties.length > 0 && <HypothesisTable label="Input geology references">
      <thead><tr><th scope="col">Geology record</th><th scope="col">Exact reference</th></tr></thead>
      <tbody>{pkg.geologicalProperties.map((record, index) => <tr key={`${idOf(record)}:${index}`}>
        <th scope="row">{nameOf(record) || `Geology record ${index + 1}`}</th><td><code>{idOf(record) || 'Not recorded'}</code></td>
      </tr>)}</tbody>
    </HypothesisTable>}
    <button type="button" onClick={() => downloadJson(references, 'hypothesis-input-references.json')}>Download input references JSON</button>
  </details>
}

export function HypothesisBranchEditor({ source, writes, onSaved }: {
  source: HypothesisRevision; writes: HypothesisWrites; onSaved: (value: HypothesisRevision) => void
}) {
  const draft = useTextDraft(`hypothesis-branch-v1:${source.snapshotSha256}`, {
    name: '', rationale: '', preset: 'spatial', configuration: JSON.stringify(branchPreset(source, 'spatial')),
  })
  const [result, setResult] = useState<AnalysisResult>()
  const [candidateId, setCandidateId] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const active = useRef(true)
  const controller = useRef<AbortController | undefined>(undefined)
  useEffect(() => { active.current = true; return () => { active.current = false; controller.current?.abort() } }, [])
  let configuration: AnalysisConfiguration | undefined
  try { configuration = JSON.parse(draft.value.configuration) as AnalysisConfiguration } catch { /* The JSON text remains editable below. */ }
  const configurationProblems = configuration ? configurationErrors(configuration) : ['Configuration JSON could not be read.']
  const resultStale = !result?.configuration || !configuration || configurationKey(result.configuration) !== configurationKey(configuration)
  const candidates = result ? eligibleCandidates(result) : []
  async function reanalyze() {
    if (!configuration || configurationProblems.length) return
    setBusy(true); setError('')
    const request = new AbortController()
    controller.current?.abort()
    controller.current = request
    try {
      const analysis = await analyzeHypothesis(selectionOf(source), configuration, request.signal)
      if (request.signal.aborted || !active.current) return
      verifyAppliedAnalysis(analysis, source.package, source.scope.reservoirName, configuration)
      setResult(analysis)
      setCandidateId(branchTarget(source, analysis, draft.value.preset)?.candidateId ?? '')
    } catch (reason) { if (active.current && !request.signal.aborted) setError(String(reason)) }
    finally { if (active.current && !request.signal.aborted) setBusy(false) }
  }
  async function save() {
    if (!result || resultStale || busy) return
    setError('')
    try {
      if (!draft.value.name.trim() || draft.value.name.length > 120) throw new Error('Enter a branch name of 1–120 characters.')
      const input = hypothesisInput(source.package, result, candidateId, draft.value.rationale, source.input.correlationNotes ?? '', source.input.controlNotes ?? [])
      const value = await writes.send({ path: '/branches', method: 'POST', kind: 'revision', label: 'Save named alternative branch',
        body: { name: draft.value.name, scope: source.scope, source: referenceOf(source), input } })
      if (active.current && value && 'snapshotSha256' in value) onSaved(value)
    } catch (reason) { setError(String(reason)) }
  }
  return <section className="hypothesis-editor" data-tutorial="hypothesis-branch">
    <h3>Branch exact revision {source.revision} · {source.name}</h3>
    <p>Presets are editable assumptions, not generated geology. Reanalysis runs the real API against the source revision's stored package, never current or hidden evidence.
      Any number of named branches may be saved; the backend list is paginated.</p>
    <DraftWarning error={draft.error} raw={draft.raw} />
    {error && <p className="task-errors" role="alert">{error}</p>}
    <div className="task-controls"><label>Branch preset<select aria-label="Branch preset" value={draft.value.preset} disabled={busy} onChange={(event) => {
      const preset = event.target.value as 'spatial' | 'conservative' | 'custom'
      draft.replace({ ...draft.value, preset, configuration: JSON.stringify(branchPreset(source, preset)) })
      setResult(undefined); setCandidateId('')
    }}><option value="spatial">Spatial · farthest eligible cell</option><option value="conservative">Conservative · higher rock cutoffs</option><option value="custom">Custom settings</option></select></label></div>
    <p>Spatial retains source configuration and selects the farthest eligible cell from the source target after recalculation.
      Conservative raises minimum porosity by 0.02 and doubles permeability cutoff (within supported bounds), then selects rank 1. These are presets, not calibrated conservatism or geologic independence.</p>
    <div className="task-controls">{configuration && (['porosityCutoff', 'permeabilityCutoffM2', 'wellExclusionRadiusM', 'gridPointsPerAxis', 'idwNeighborCount'] as const).map((key) =>
      <label key={key}>{key}<input type="number" step="any" disabled={busy} value={Number.isFinite(configuration[key]) ? configuration[key] : ''}
        onChange={(event) => draft.set('configuration', JSON.stringify({ ...configuration, [key]: event.target.value === '' ? null : Number(event.target.value) }))} /></label>)}</div>
    {configurationProblems.length > 0 && <p className="task-errors" role="alert">{configurationProblems.join(' ')}</p>}
    <div className="task-actions"><button type="button" disabled={busy || Boolean(draft.error) || configurationProblems.length > 0} onClick={() => void reanalyze()}>
      {busy ? 'Reanalyzing stored evidence…' : 'Reanalyze stored evidence'}</button></div>
    {result && <><p role="status">{resultStale ? 'Settings changed: prior branch output is stale.' : 'Stored-evidence reanalysis available.'}
      {' '}{result.candidateGrid?.length ?? 0} grid cells; {candidates.length} eligible; result <code>{result.analysisSha256}</code>.</p>
      <label className="rationale-field">Branch eligible target<select aria-label="Branch eligible target" value={candidateId} disabled={resultStale} onChange={(event) => setCandidateId(event.target.value)}>
        <option value="">Select an eligible target</option>{candidates.map((candidate) => <option key={candidate.candidateId} value={candidate.candidateId}>{candidate.candidateId} · P50 {candidate.p50NetPayM.toFixed(2)} m rock proxy</option>)}
      </select></label>
      {!candidates.length && <p className="task-notice">No eligible targets. Inspect the real exclusion reasons; no alternative can be saved from missing output.</p>}
      <BranchGridOutput key={result.analysisSha256} result={result} candidateId={candidateId} />
    </>}
    <label className="rationale-field">New alternative name<input maxLength={120} value={draft.value.name} onChange={(event) => draft.set('name', event.target.value)} /></label>
    <label className="rationale-field">Alternative rationale<textarea aria-label="Alternative rationale" rows={3} maxLength={10000} value={draft.value.rationale} onChange={(event) => draft.set('rationale', event.target.value)} /></label>
    <div className="task-actions"><button type="button" disabled={busy || writes.busy || Boolean(writes.blocked || draft.error) || resultStale || !candidateId} onClick={() => void save()}>Save named alternative branch</button>
      <button type="button" onClick={() => downloadJson(draft.value, 'alternative-local-text.json')}>Download branch text</button></div>
  </section>
}

const gridStatusLabels: Record<string, string> = { eligible: 'Eligible', excluded: 'Excluded', unsupported: 'Unsupported' }
const gridReasonLabels: Record<string, string> = {
  'within-well-exclusion-radius': 'Within screening-control exclusion radius',
  'insufficient-located-well-controls': 'Too few located screening controls',
}
const gridStatusLabel = (status: string) => Object.hasOwn(gridStatusLabels, status) ? gridStatusLabels[status] : `Unrecognized status: ${status}`
const gridReasonLabel = (reason: string) => Object.hasOwn(gridReasonLabels, reason) ? gridReasonLabels[reason] : `Additional recorded reason: ${reason}`

export function BranchGridOutput({ result, candidateId }: { result: AnalysisResult; candidateId: string }) {
  const [page, setPage] = useState(0)
  const grid = result.candidateGrid ?? []
  const statuses = new Map<string, number>()
  const reasons = new Map<string, number>()
  grid.forEach(cell => {
    statuses.set(cell.status, (statuses.get(cell.status) ?? 0) + 1)
    cell.reasons.forEach(reason => reasons.set(reason, (reasons.get(reason) ?? 0) + 1))
  })
  const start = page * 20
  return <details><summary>Reanalysis grid/status output</summary>
    <p>{grid.length} grid cells. P90/P50/P10 are uncalibrated qualifying-rock estimates, not hydrocarbon pay.</p>
    <ul>{[...statuses].map(([status, count]) => <li key={status}>{gridStatusLabel(status)}: {count}</li>)}</ul>
    {reasons.size > 0 && <><h4>Recorded reasons</h4>
      <ul>{[...reasons].map(([reason, count]) => <li key={reason}>{gridReasonLabel(reason)}: {count} cells</li>)}</ul>
    </>}
    {grid.length ? <>
      <HypothesisTable label="Reanalysis grid cells">
        <caption>Cells {start + 1}–{Math.min(start + 20, grid.length)} of {grid.length} · field-local metres</caption>
        <thead><tr><th scope="col">Cell / target</th><th scope="col">Easting / northing · m</th><th scope="col">Status / reasons</th>
          <th scope="col">Nearest screening control · m</th><th scope="col">P90 / P50 / P10 · m rock proxy</th></tr></thead>
        <tbody>{grid.slice(start, start + 20).map((cell, index) => <tr key={cell.candidateId} data-selected={cell.candidateId === candidateId}>
          <th scope="row">Cell {start + index + 1}{cell.candidateId === candidateId ? ' · selected' : ''}
            <br /><small><code>{cell.candidateId}</code></small></th>
          <td>{cell.eastingM} / {cell.northingM}</td>
          <td>{gridStatusLabel(cell.status)}{cell.reasons.length
            ? <ul>{cell.reasons.map((reason, i) => <li key={i}>{gridReasonLabel(reason)}</li>)}</ul> : <p>No reasons recorded.</p>}</td>
          <td>{cell.nearestWellDistanceM ?? 'Unavailable'}</td>
          <td>{cell.prediction ? `${cell.prediction.p90NetPayM} / ${cell.prediction.p50NetPayM} / ${cell.prediction.p10NetPayM}` : 'Unavailable'}</td>
        </tr>)}</tbody>
      </HypothesisTable>
      <div className="task-actions">
        <button type="button" disabled={page === 0} onClick={() => setPage(value => value - 1)}>Previous grid cells</button>
        <span>Page {page + 1} of {Math.ceil(grid.length / 20)} · 20 cells per page</span>
        <button type="button" disabled={start + 20 >= grid.length} onClick={() => setPage(value => value + 1)}>Next grid cells</button>
      </div>
    </> : <p>{result.candidateGrid ? 'The returned grid is empty.' : 'Grid output was not returned.'}</p>}
    <p>Download the full grid for exact coordinates, prediction details and any additional recorded fields.</p>
    {result.candidateGrid && <button type="button" onClick={() => downloadJson(result.candidateGrid, `hypothesis-grid-${result.analysisSha256}.json`)}>Download full grid JSON</button>}
  </details>
}
