import { useEffect, useRef, useState } from 'react'
import { downloadJson } from '../analysisConfiguration'
import {
  compareHypotheses, getHypothesis, getLatestHypothesis, hypothesisScopeKey, hypothesisSelectionKey,
  listHypotheses, selectionOf,
} from '../hypotheses'
import type { HypothesisComparison, HypothesisRevision, HypothesisScope, HypothesisSelection, HypothesisSummary } from '../hypotheses'
import type { AnalysisResult, FieldPackage } from '../types'
import type { TaskId } from '../sequencer'
import { HypothesisRevisionEditor, HypothesisBranchEditor } from './HypothesisRevisionEditor'
import { HypothesisChallenges } from './HypothesisChallenges'
import { DraftWarning, HypothesisTable, useHypothesisWrites, useTextDraft } from './HypothesisSupport'
import { AnalysisSettingsSummary, hypothesisFieldLabel, RecordedValue, SavedArtifactSummary, SavedHypothesisInput } from './HypothesisPresentation'

export const isHypothesisTask = (id: TaskId) => ['alternatives', 'compare', 'challenge', 'correlation'].includes(id)

function parseSelections(raw: string): HypothesisSelection[] {
  const value = JSON.parse(raw) as HypothesisSelection[]
  if (!Array.isArray(value) || value.length > 8 || value.some((item) => !item?.scope || !item.reference ||
    typeof item.scope.fieldId !== 'string' || typeof item.scope.reservoirName !== 'string' ||
    typeof item.reference.hypothesisId !== 'string' || !Number.isInteger(item.reference.revision) || item.reference.revision < 1)) {
    throw new Error('Comparison selection must contain at most eight explicit scopes and exact references.')
  }
  if (new Set(value.map(hypothesisSelectionKey)).size !== value.length) throw new Error('Comparison references must be unique.')
  return value
}

export function HypothesisWorkbench({ taskId, pkg, analysis, scope, candidateId, stale, ai }: {
  taskId: TaskId; pkg: FieldPackage; analysis: AnalysisResult; scope: HypothesisScope; candidateId: string; stale: boolean; ai: React.ReactNode
}) {
  const { writes, panel } = useHypothesisWrites(hypothesisScopeKey(scope))
  const [items, setItems] = useState<HypothesisSummary[]>([])
  const [offset, setOffset] = useState(0)
  const [revisionListId, setRevisionListId] = useState('')
  const [selected, setSelected] = useState<HypothesisRevision>()
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [comparison, setComparison] = useState<HypothesisComparison>()
  const [comparedKey, setComparedKey] = useState('')
  const [comparing, setComparing] = useState(false)
  const [referenceId, setReferenceId] = useState('')
  const [referenceVersion, setReferenceVersion] = useState('')
  const [editorSource, setEditorSource] = useState<'live' | 'saved'>('live')
  const selectionDraft = useTextDraft(`hypothesis-comparison-v1:${hypothesisScopeKey(scope)}`, { selections: '[]' })
  const reader = useRef<AbortController | undefined>(undefined)
  const generation = useRef(0)
  let choices: HypothesisSelection[] = []
  let selectionError = selectionDraft.error
  try { choices = parseSelections(selectionDraft.value.selections) }
  catch (reason) { selectionError = String(reason) }

  async function refresh(id = revisionListId, start = offset) {
    reader.current?.abort()
    const controller = new AbortController()
    reader.current = controller
    setLoading(true); setError('')
    try {
      const rows = await listHypotheses(scope, start, id || undefined, controller.signal)
      if (controller.signal.aborted) return
      if (rows.some((row) => hypothesisScopeKey(row.scope) !== hypothesisScopeKey(scope))) throw new Error('List returned artifacts outside the requested scope.')
      setItems(rows); setOffset(start); setRevisionListId(id)
    } catch (reason) { if (!controller.signal.aborted) setError(String(reason)) }
    finally { if (!controller.signal.aborted) setLoading(false) }
  }
  useEffect(() => {
    void refresh('', 0)
    return () => { reader.current?.abort(); generation.current++ }
  }, [hypothesisScopeKey(scope)])
  async function load(selection: HypothesisSelection, latest = false) {
    const ticket = ++generation.current
    setError('')
    try {
      const revision = latest
        ? await getLatestHypothesis(selection.scope, selection.reference.hypothesisId)
        : await getHypothesis(selection)
      if (generation.current !== ticket) return
      setSelected(revision); setEditorSource('saved')
    } catch (reason) { if (generation.current === ticket) setError(String(reason)) }
  }
  function saved(value: HypothesisRevision) {
    setSelected(value)
    setEditorSource('saved')
    void refresh('', 0)
  }
  function addComparison(selection: HypothesisSelection) {
    if (selectionError) return
    const next = [...choices]
    if (!next.some((item) => hypothesisSelectionKey(item) === hypothesisSelectionKey(selection))) next.push(selection)
    if (next.length > 8) { setError('Compare at most eight exact revisions.'); return }
    selectionDraft.set('selections', JSON.stringify(next, null, 2))
  }
  async function compare() {
    if (choices.length < 2 || choices.length > 8 || selectionError) return
    const ticket = ++generation.current
    const exactChoices = structuredClone(choices)
    setComparing(true); setError('')
    try {
      const value = await compareHypotheses(exactChoices)
      if (generation.current !== ticket) return
      if (value.entries.length !== exactChoices.length || value.entries.some((entry) =>
        !exactChoices.some((choice) => hypothesisSelectionKey(choice) === hypothesisSelectionKey(selectionOf(entry.revision))))) throw new Error('Comparison returned different references than selected.')
      setComparison(value)
      setComparedKey(JSON.stringify(exactChoices))
    } catch (reason) { if (generation.current === ticket) setError(String(reason)) }
    finally { setComparing(false) }
  }
  const editSaved = editorSource === 'saved' ? selected : undefined
  const visible = isHypothesisTask(taskId)
  return <div className="hypothesis-workbench" hidden={!visible}>
    {visible && <>
      <div className="task-actions">
        <button type="button" disabled={loading} onClick={() => void refresh()}>Refresh saved hypothesis list</button>
        <button type="button" disabled={loading} onClick={() => void refresh('', 0)}>List named hypotheses</button>
        <button type="button" onClick={() => setEditorSource('live')}>Edit current live hypothesis</button>
        {selected && <button type="button" onClick={() => setEditorSource('saved')}>Edit loaded snapshot notes</button>}
      </div>
      <p>Scope: {scope.fieldId} / {scope.reservoirName} / {scope.scenarioId ?? 'live'} / {scope.asOfUtc ?? 'live service time'}.
        Lists and reads never overwrite local text or comparison references. Saves require the local operator session, native CSRF and a stable action key.</p>
      {error && <p className="task-errors" role="alert">{error}</p>}
      {panel}
      {writes.receipt && <SavedArtifactSummary artifact={writes.receipt} />}
      <HypothesisTable label="Scoped saved hypotheses and exact revisions">
        <caption>{revisionListId ? `Revision history for ${revisionListId}` : 'Latest listed revision of each named hypothesis'} · loading is not selection</caption>
        <thead><tr><th scope="col">Name / revision</th><th scope="col">Saved evidence and result</th><th scope="col">Read</th><th scope="col">Compare</th></tr></thead>
        <tbody>{items.map((item) => <tr key={`${item.hypothesisId}:${item.revision}`}>
          <th scope="row">{item.name} · r{item.revision}<br /><code>{item.hypothesisId}</code></th>
          <td>Package <code>{item.packageSha256.slice(0, 16)}</code><br />Result <code>{item.analysisSha256.slice(0, 16)}</code></td>
          <td><button type="button" onClick={() => void load(selectionOf(item))}>Load exact r{item.revision}</button>
            <button type="button" disabled={loading} onClick={() => void refresh(item.hypothesisId, 0)}>List revision history</button></td>
          <td><button type="button" onClick={() => addComparison(selectionOf(item))}>Add exact revision to comparison</button></td>
        </tr>)}</tbody>
      </HypothesisTable>
      {!loading && !items.length && <p>No saved hypotheses on this page. Save the named current analysis below to begin; visiting is not a saved revision.</p>}
      <div className="task-actions">
        <button type="button" disabled={loading || offset === 0} onClick={() => void refresh(revisionListId, Math.max(0, offset - 20))}>Previous revisions</button>
        <span>Offset {offset} · 20 per page</span><button type="button" disabled={loading || items.length < 20} onClick={() => void refresh(revisionListId, offset + 20)}>Next revisions</button>
      </div>
      <details><summary>Load an explicit saved reference in this scope</summary>
        <div className="task-controls"><label>Hypothesis ID<input value={referenceId} onChange={(event) => setReferenceId(event.target.value)} /></label>
          <label>Exact revision<input type="number" min={1} max={100000} value={referenceVersion} onChange={(event) => setReferenceVersion(event.target.value)} /></label>
          <button type="button" disabled={!referenceId || !Number.isInteger(Number(referenceVersion)) || Number(referenceVersion) < 1}
            onClick={() => void load({ scope, reference: { hypothesisId: referenceId, revision: Number(referenceVersion) } })}>Load exact reference</button>
        </div>
      </details>
      {selected && <section className="hypothesis-snapshot">
        <h3>Loaded snapshot · {selected.name} · r{selected.revision}</h3>
        <p className="task-notice">Historical snapshot, not current live evidence. Map, 2D/3D and live target readouts below remain on the current package.
          Loading this revision never changes that package, its configuration, or any sealed prediction.</p>
        <p>Snapshot <code>{selected.snapshotSha256}</code> · package <code>{selected.package.sha256}</code> · result <code>{selected.analysis.analysisSha256}</code>.</p>
        <p>Recorded target: {selected.input.selectedCandidateId}. Rationale: {selected.input.rationale}</p>
        <div className="task-actions">
          <button type="button" onClick={() => void load(selectionOf(selected), true)}>Inspect latest saved revision</button>
          <button type="button" onClick={() => addComparison(selectionOf(selected))}>Compare loaded exact revision</button>
          <button type="button" onClick={() => downloadJson(selected, `hypothesis-${selected.hypothesisId}-r${selected.revision}.json`)}>Download exact snapshot JSON</button>
        </div>
        <details><summary>Saved configuration, annotations and target</summary><SavedHypothesisInput input={selected.input} /></details>
      </section>}
      {taskId === 'compare' && <section>
        <h3>Compare 2–8 exact saved revisions</h3>
        <DraftWarning error={selectionError} raw={selectionDraft.raw} />
        <p>Selections do not advance when a latest revision changes. To compare across evidence scopes, edit each explicit scope/reference below; server-returned scope differences are shown, not normalized away.</p>
        <label className="rationale-field">Comparison scopes and exact references · JSON<textarea aria-label="Comparison scopes and exact references · JSON" rows={8} spellCheck={false}
          value={selectionDraft.value.selections} onChange={(event) => selectionDraft.set('selections', event.target.value)} /></label>
        <div className="task-actions"><button type="button" disabled={comparing || Boolean(selectionError) || choices.length < 2 || choices.length > 8} onClick={() => void compare()}>
          {comparing ? 'Comparing saved revisions…' : 'Compare selected exact revisions'}</button></div>
        {comparison && <ComparisonOutput comparison={comparison} stale={comparedKey !== JSON.stringify(choices)} onLoad={(selection) => void load(selection)} />}
      </section>}
      {(taskId === 'alternatives' || taskId === 'correlation' || taskId === 'compare') && <HypothesisRevisionEditor
        key={`editor:${editSaved?.snapshotSha256 ?? `live:${pkg.sha256}:${analysis.analysisSha256}:${candidateId}`}`}
        selected={editSaved} pkg={editSaved?.package ?? pkg} analysis={editSaved?.analysis ?? analysis} scope={editSaved?.scope ?? scope}
        candidateId={editSaved?.input.selectedCandidateId ?? candidateId} livePackage={pkg} liveScope={scope} stale={stale}
        writes={writes} onSaved={saved} correlation={taskId === 'correlation'} />}
      {taskId === 'alternatives' && (selected
        ? <HypothesisBranchEditor key={`branch:${selected.snapshotSha256}`} source={selected} writes={writes} onSaved={saved} />
        : <p className="task-notice">Save or load an exact revision before creating a spatial, conservative or custom branch.</p>)}
      {taskId === 'challenge' && (selected
        ? <HypothesisChallenges key={`challenge:${selected.snapshotSha256}`} source={selected} writes={writes} ai={ai} />
        : <><p className="task-notice">Load or save an exact hypothesis revision in Alternatives before attaching a challenge. The existing AI action below is a transient live-context note, not a saved objection.</p>{ai}</>)}
    </>}
  </div>
}

const scopeDifferenceLabels: Record<string, string> = {
  'different-field': 'Different field', 'different-reservoir': 'Different reservoir', 'different-scenario': 'Different scenario',
  'different-as-of': 'Different evidence time', 'different-visible-package': 'Different visible package',
}

export function ComparisonOutput({ comparison, stale, onLoad }: {
  comparison: HypothesisComparison; stale: boolean; onLoad: (selection: HypothesisSelection) => void
}) {
  return <>
    <p role="status">{stale ? 'Comparison selections changed; this is the prior comparison output.' : 'Comparison matches the selected exact references.'}
      {' '}Fingerprint <code>{comparison.comparisonSha256}</code>.</p>
    <p>Exact baseline: <code>{comparison.baseline.hypothesisId}</code> r{comparison.baseline.revision}.
      Baseline follows the server's canonical ordering; it is not a recorded preference.</p>
    <div className="task-actions"><button type="button" onClick={() => downloadJson(comparison, `hypothesis-comparison-${comparison.comparisonSha256}.json`)}>Download comparison JSON</button></div>
    <HypothesisTable label="Exact hypothesis comparison results">
      <thead><tr><th scope="col">Saved revision</th><th scope="col">Evidence comparability</th><th scope="col">Assumptions</th><th scope="col">Target / P90–P50–P10 m</th><th scope="col">Rationale / preference annotation</th></tr></thead>
      <tbody>{comparison.entries.map((entry) => <tr key={`${entry.revision.hypothesisId}:${entry.revision.revision}`}>
        <th scope="row">{entry.revision.name} · r{entry.revision.revision}<br /><button type="button" onClick={() => onLoad(selectionOf(entry.revision))}>Load for decision rationale</button></th>
        <td>{entry.likeForLikeEvidence ? 'Matching evidence scope' : 'NOT LIKE-FOR-LIKE'}
          {entry.scopeDifferences.length > 0 && <ul>{entry.scopeDifferences.map((reason, index) => <li key={index}>{
            Object.hasOwn(scopeDifferenceLabels, reason) ? scopeDifferenceLabels[reason] : `Additional recorded scope difference: ${reason}`}</li>)}</ul>}
          <details><summary>Evidence scope and package</summary>
            <RecordedValue field="scope" value={entry.revision.scope} downloadLabel="Download comparison JSON" />
            <p>Package fingerprint<br /><code>{entry.revision.packageSha256}</code></p>
          </details>
        </td>
        <td><AnalysisSettingsSummary configuration={entry.configuration} downloadLabel="Download comparison JSON" compact /></td>
        <td>{entry.selectedCandidate.candidateId}<br />{entry.selectedCandidate.p90NetPayM} / {entry.selectedCandidate.p50NetPayM} / {entry.selectedCandidate.p10NetPayM}
          <br />Uncalibrated rock proxy, not hydrocarbon pay.</td>
        <td><strong>Rationale</strong><p style={{ whiteSpace: 'pre-wrap' }}>{entry.rationale}</p>
          <strong>Correlation notes</strong><p style={{ whiteSpace: 'pre-wrap' }}>{entry.correlationNotes || 'No correlation note'}</p>
          <strong>Control notes</strong>{entry.controlNotes?.length ? <ul>{entry.controlNotes.map((note, index) => <li key={index}>
            <span style={{ whiteSpace: 'pre-wrap' }}>{note.note}</span><br /><small>Evidence reference: <code>{note.evidenceId}</code></small>
          </li>)}</ul> : <p>No control notes recorded.</p>}
        </td>
      </tr>)}</tbody>
    </HypothesisTable>
    {comparison.entries.map((entry) => <details key={`${entry.revision.hypothesisId}:${entry.revision.revision}`}>
      <summary>{entry.revision.name} r{entry.revision.revision} · {entry.differences.length} differences from exact baseline</summary>
      <p>Full recorded values are in Download comparison JSON, in this revision's differences. Field references below identify each value.</p>
      <HypothesisTable label={`Differences for ${entry.revision.name}`}><thead><tr><th scope="col">Field</th><th scope="col">Baseline value</th><th scope="col">Compared value</th></tr></thead>
        <tbody>{entry.differences.map((difference) => <tr key={difference.field}><th scope="row">{hypothesisFieldLabel(difference.field)}
          <br /><small><code>{difference.field}</code></small></th>
          <td><RecordedValue field={difference.field} value={difference.baselineValue} downloadLabel="Download comparison JSON" /></td>
          <td><RecordedValue field={difference.field} value={difference.value} downloadLabel="Download comparison JSON" /></td></tr>)}</tbody>
      </HypothesisTable>
    </details>)}
    <p>No preferred flag is invented. Load the preferred exact revision, explain the decision and compared references in its rationale editor, then explicitly save a revision or annotation branch below.</p>
  </>
}
