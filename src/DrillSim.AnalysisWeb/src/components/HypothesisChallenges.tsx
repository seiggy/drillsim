import { useEffect, useMemo, useRef, useState } from 'react'
import { downloadJson } from '../analysisConfiguration'
import { visibleEvidenceIds } from '../evidence'
import {
  getHypothesisChallenge, listHypothesisChallenges, revisionPath, selectionOf, validateObjections,
} from '../hypotheses'
import type { Disposition, HypothesisChallenge, HypothesisRevision } from '../hypotheses'
import { DraftWarning, HypothesisTable, useTextDraft } from './HypothesisSupport'
import type { HypothesisWrites } from './HypothesisSupport'

interface LocalObjection { text: string; citedEvidenceIds: string[] }
function parseObjections(raw: string): LocalObjection[] {
  const parsed: unknown = JSON.parse(raw)
  if (!Array.isArray(parsed) || parsed.some((item) => !item || typeof item.text !== 'string' ||
      !Array.isArray(item.citedEvidenceIds) || item.citedEvidenceIds.some((id: unknown) => typeof id !== 'string'))) {
    throw new Error('Objection draft is unreadable. Download its local backup before editing.')
  }
  return parsed
}
export function HypothesisChallenges({ source, writes, ai }: {
  source: HypothesisRevision; writes: HypothesisWrites; ai: React.ReactNode
}) {
  const selection = selectionOf(source)
  const draft = useTextDraft(`hypothesis-challenge-v1:${source.snapshotSha256}`, { summary: '', actor: '', objections: JSON.stringify([{ text: '', citedEvidenceIds: [] }]) })
  const [error, setError] = useState('')
  const [items, setItems] = useState<HypothesisChallenge[]>([])
  const [offset, setOffset] = useState(0)
  const [loading, setLoading] = useState(false)
  const [selected, setSelected] = useState<HypothesisChallenge>()
  const [historyVersion, setHistoryVersion] = useState('')
  const controller = useRef<AbortController | undefined>(undefined)
  const generation = useRef(0)
  const active = useRef(true)
  const ids = useMemo(() => [...visibleEvidenceIds(source.package)].sort(), [source.package])
  let objections: LocalObjection[] = []
  let draftError = draft.error
  try { objections = parseObjections(draft.value.objections) }
  catch (reason) { draftError = String(reason) }
  async function refresh(nextOffset = offset) {
    controller.current?.abort()
    const current = new AbortController()
    controller.current = current
    setLoading(true); setError('')
    try {
      const values = await listHypothesisChallenges(selection, nextOffset, current.signal)
      if (current.signal.aborted) return
      if (values.some((value) => value.hypothesisSnapshotSha256 !== source.snapshotSha256 || value.analysisSha256 !== source.input.analysisSha256)) throw new Error('Challenge list is not bound to this exact saved snapshot.')
      setItems(values); setOffset(nextOffset)
    } catch (reason) { if (!current.signal.aborted) setError(String(reason)) }
    finally { if (!current.signal.aborted) setLoading(false) }
  }
  useEffect(() => {
    active.current = true
    void refresh(0)
    return () => { active.current = false; controller.current?.abort(); generation.current++ }
  }, [source.snapshotSha256])
  function editObjection(index: number, update: Partial<LocalObjection>) {
    draft.set('objections', JSON.stringify(objections.map((item, i) => i === index ? { ...item, ...update } : item)))
  }
  async function save() {
    if (writes.busy || writes.blocked || draftError) return
    setError('')
    try {
      if (!draft.value.summary.trim() || draft.value.summary.length > 4000 || !draft.value.actor.trim() || draft.value.actor.length > 200) throw new Error('Enter a summary (1–4000 characters) and local audit label (1–200 characters).')
      validateObjections(source.package, objections)
      const value = await writes.send({
        path: `${revisionPath(selection.reference)}/challenges`, method: 'POST', kind: 'challenge', label: 'Save revision-bound challenge',
        body: { scope: source.scope, analysisSha256: source.input.analysisSha256, summary: draft.value.summary, actor: draft.value.actor, objections },
      })
      if (active.current && value && 'challengeId' in value) {
        setSelected(value)
        void refresh(0)
      }
    } catch (reason) { setError(String(reason)) }
  }
  async function inspect(id: string, version?: number) {
    const request = ++generation.current
    setError('')
    try {
      const value = await getHypothesisChallenge(selection, id, version)
      if (request !== generation.current) return
      if (value.hypothesisSnapshotSha256 !== source.snapshotSha256 || value.analysisSha256 !== source.input.analysisSha256) throw new Error('Challenge is bound to different evidence.')
      setSelected(value)
    } catch (reason) { if (request === generation.current) setError(String(reason)) }
  }
  return <section className="hypothesis-editor">
    <h3>Challenge {source.name} · revision {source.revision}</h3>
    <p>New objections attach only to snapshot <code>{source.snapshotSha256}</code> and analysis <code>{source.input.analysisSha256}</code>.
      The manual editor below is independent of AI notes. AI output is not saved or cited automatically; verify its statements against this exact visible package before attaching them.</p>
    {ai}
    <DraftWarning error={draftError} raw={draft.raw} />
    {error && <p className="task-errors" role="alert">{error}</p>}
    <label className="rationale-field">Challenge summary<textarea aria-label="Challenge summary" rows={3} maxLength={4000} value={draft.value.summary} onChange={(event) => draft.set('summary', event.target.value)} /></label>
    <label className="rationale-field">Challenge audit label · not authentication<input maxLength={200} value={draft.value.actor} onChange={(event) => draft.set('actor', event.target.value)} /></label>
    {objections.map((objection, index) => <fieldset className="hypothesis-objection" key={index}>
      <legend>Objection {index + 1}</legend>
      <label className="rationale-field">Objection text<textarea aria-label="Objection text" rows={3} maxLength={4000} value={objection.text} onChange={(event) => editObjection(index, { text: event.target.value })} /></label>
      <label className="rationale-field">Cited evidence · select 1–32 visible IDs<select aria-label="Cited evidence · select 1–32 visible IDs" multiple size={5} value={objection.citedEvidenceIds}
        onChange={(event) => editObjection(index, { citedEvidenceIds: [...event.target.selectedOptions].map((option) => option.value) })}>
        {ids.map((id) => <option key={id}>{id}</option>)}
      </select></label>
      <p>{objection.citedEvidenceIds.length} citations selected. Use Ctrl/Command or Shift for multiple selections.</p>
      <button type="button" disabled={objections.length === 1} onClick={() => draft.set('objections', JSON.stringify(objections.filter((_, i) => i !== index)))}>Remove local objection</button>
    </fieldset>)}
    <div className="task-actions">
      <button type="button" disabled={objections.length >= 32 || Boolean(draftError)} onClick={() => draft.set('objections', JSON.stringify([...objections, { text: '', citedEvidenceIds: [] }]))}>Add objection</button>
      <button type="button" disabled={writes.busy || Boolean(writes.blocked || draftError)} onClick={() => void save()}>Save challenge to exact revision</button>
      <button type="button" onClick={() => downloadJson(draft.value, 'challenge-local-text.json')}>Download challenge text</button>
    </div>
    <h4>Saved challenge records</h4>
    <div className="task-actions"><button type="button" disabled={loading} onClick={() => void refresh()}>Refresh saved challenges</button></div>
    <HypothesisTable label="Saved exact-revision challenges"><thead><tr><th scope="col">Summary</th><th scope="col">Latest listed version</th><th scope="col">Author</th><th scope="col">Inspect</th></tr></thead>
      <tbody>{items.map((item) => <tr key={item.challengeId}><th scope="row">{item.summary}</th><td>{item.version}</td><td>{item.createdBy}</td><td>
        <button type="button" onClick={() => void inspect(item.challengeId, item.version)}>Open listed version {item.version}</button>
      </td></tr>)}</tbody></HypothesisTable>
    {!loading && !items.length && <p>No challenge records in this page. Local objections above are not saved records.</p>}
    <div className="task-actions"><button type="button" disabled={loading || offset === 0} onClick={() => void refresh(Math.max(0, offset - 20))}>Previous challenges</button>
      <span>Offset {offset} · 20 per page</span><button type="button" disabled={loading || items.length < 20} onClick={() => void refresh(offset + 20)}>Next challenges</button></div>
    {selected && <>
      <div className="task-controls"><label>Exact challenge history version<input type="number" min={1} max={100000} value={historyVersion} onChange={(event) => setHistoryVersion(event.target.value)} /></label>
        <button type="button" disabled={!Number.isInteger(Number(historyVersion)) || Number(historyVersion) < 1} onClick={() => void inspect(selected.challengeId, Number(historyVersion))}>Inspect historical challenge version</button>
        <button type="button" onClick={() => void inspect(selected.challengeId)}>Inspect latest challenge version</button></div>
      <p>Opening another version preserves this version's unsaved disposition text separately. “Latest” never silently rebases edits.</p>
      <ChallengeDispositions key={`${selected.challengeId}:${selected.version}`} source={source} challenge={selected} writes={writes} onSaved={(value) => { setSelected(value); void refresh(0) }} />
    </>}
  </section>
}

export function ChallengeDispositions({ source, challenge, writes, onSaved }: {
  source: HypothesisRevision; challenge: HypothesisChallenge; writes: HypothesisWrites; onSaved: (value: HypothesisChallenge) => void
}) {
  const initial = Object.fromEntries(challenge.objections.flatMap((item) => [[`${item.objectionId}:state`, item.disposition], [`${item.objectionId}:reason`, item.dispositionReason ?? '']]))
  const draft = useTextDraft<Record<string, string>>(`hypothesis-dispositions-v1:${challenge.challengeId}:${challenge.version}`, { actor: '', ...initial })
  const [error, setError] = useState('')
  const active = useRef(true)
  useEffect(() => { active.current = true; return () => { active.current = false } }, [])
  async function save() {
    if (writes.busy || writes.blocked || draft.error) return
    setError('')
    try {
      if (!draft.value.actor.trim() || draft.value.actor.length > 200) throw new Error('Enter a local audit label of 1–200 characters.')
      const dispositions = challenge.objections.flatMap((item) => {
        const disposition = draft.value[`${item.objectionId}:state`] as Disposition
        const reason = draft.value[`${item.objectionId}:reason`]
        if (disposition === item.disposition && reason === (item.dispositionReason ?? '')) return []
        if (!['open', 'accepted', 'rejected', 'deferred'].includes(disposition) || !reason.trim() || reason.length > 2000) throw new Error('Every changed disposition requires a supported state and reason of 1–2000 characters.')
        return [{ objectionId: item.objectionId, disposition, reason }]
      })
      if (!dispositions.length) throw new Error('No disposition changes to save.')
      const value = await writes.send({ method: 'PUT', kind: 'challenge', expectedVersion: challenge.version, label: 'Save reviewed dispositions',
        path: `${revisionPath(source)}/challenges/${challenge.challengeId}/dispositions`,
        body: { scope: source.scope, actor: draft.value.actor, dispositions } })
      if (active.current && value && 'challengeId' in value) onSaved(value)
    } catch (reason) { setError(String(reason)) }
  }
  return <section className="hypothesis-dispositions">
    <h4>Disposition review · challenge version {challenge.version}</h4>
    <p>{challenge.summary} · snapshot <code>{challenge.hypothesisSnapshotSha256}</code>. Saves use <code>If-Match: "{challenge.version}"</code> and create a new challenge version.</p>
    <DraftWarning error={draft.error} raw={draft.raw} />
    {error && <p className="task-errors" role="alert">{error}</p>}
    <label className="rationale-field">Disposition audit label<input maxLength={200} value={draft.value.actor} onChange={(event) => draft.set('actor', event.target.value)} /></label>
    {challenge.objections.map((objection) => <fieldset className="hypothesis-objection" key={objection.objectionId}>
      <legend>{objection.text}</legend>
      <p>Citations: {objection.citedEvidenceIds.join(' · ')}</p>
      <p>Saved state: {objection.disposition}. {objection.dispositionActor ? `Disposition by ${objection.dispositionActor}.` : ''}</p>
      <label className="rationale-field">Disposition<select aria-label="Disposition" value={draft.value[`${objection.objectionId}:state`]} onChange={(event) => draft.set(`${objection.objectionId}:state`, event.target.value)}>
        {['open', 'accepted', 'rejected', 'deferred'].map((status) => <option key={status}>{status}</option>)}
      </select></label>
      <label className="rationale-field">Disposition reason<textarea aria-label="Disposition reason" rows={2} maxLength={2000} value={draft.value[`${objection.objectionId}:reason`]} onChange={(event) => draft.set(`${objection.objectionId}:reason`, event.target.value)} /></label>
    </fieldset>)}
    <div className="task-actions"><button type="button" disabled={writes.busy || Boolean(writes.blocked || draft.error)} onClick={() => void save()}>Save disposition changes</button>
      <button type="button" onClick={() => downloadJson(draft.value, 'disposition-local-text.json')}>Download local dispositions</button></div>
  </section>
}
