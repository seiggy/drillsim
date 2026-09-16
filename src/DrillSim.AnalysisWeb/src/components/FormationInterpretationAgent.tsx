import { useEffect, useRef, useState } from 'react'
import {
  draftFormationInterpretation, formationContextKey, formationDraftBlock, formationEvidenceLabels,
  formationNotesKey, getFormationAgentStatus,
} from '../formationInterpretation'
import type { FormationAgentStatus, FormationDraftRequest, FormationDraftResult } from '../formationInterpretation'
import type { FieldPackage } from '../types'
import { JsonDownload } from './JsonDownload'
import './FormationInterpretationAgent.css'

export function FormationInterpretationAgent({ request, pkg, configurationSha256, blocked, onUse }: {
  request: FormationDraftRequest
  pkg: FieldPackage
  configurationSha256: string
  blocked: boolean
  onUse: (draft: FormationDraftResult) => void
}) {
  const [status, setStatus] = useState<FormationAgentStatus>()
  const [statusError, setStatusError] = useState('')
  const [refresh, setRefresh] = useState(0)
  const [running, setRunning] = useState(false)
  const [error, setError] = useState('')
  const [result, setResult] = useState<{ value: FormationDraftResult; context: string }>()
  const [reviewed, setReviewed] = useState('')
  const [message, setMessage] = useState('')
  const inFlight = useRef<AbortController | undefined>(undefined)
  const key = formationContextKey(request)
  const currentKey = useRef(key)
  currentKey.current = key
  const notesKey = formationNotesKey(request.notes)
  const blockReason = blocked ? 'Apply pending settings or resolve the current save before drafting.' : formationDraftBlock(request, pkg)
  const currentResult = result?.context === key ? result.value : undefined
  const reviewKey = JSON.stringify([currentResult?.generatedAt, key, notesKey])
  const replacing = Boolean(request.notes.name.trim() || request.notes.rationale.trim() || request.notes.correlationNotes.trim())
  const labels = formationEvidenceLabels(pkg)

  useEffect(() => {
    const controller = new AbortController()
    setStatus(undefined); setStatusError('')
    getFormationAgentStatus(controller.signal).then(value => {
      if (!controller.signal.aborted) setStatus(value)
    }).catch(reason => { if (!controller.signal.aborted) setStatusError(reason instanceof Error ? reason.message : String(reason)) })
    return () => controller.abort()
  }, [refresh])
  useEffect(() => {
    setRunning(false); setError(''); setMessage(''); setReviewed('')
    return () => { inFlight.current?.abort(); inFlight.current = undefined }
  }, [key])

  async function generate() {
    if (!status?.configured || blockReason || inFlight.current) return
    const controller = new AbortController()
    inFlight.current = controller
    const captured = structuredClone(request)
    setRunning(true); setError(''); setMessage(''); setReviewed(''); setResult(undefined)
    try {
      const draft = await draftFormationInterpretation(captured, pkg, configurationSha256, controller.signal)
      if (!controller.signal.aborted && currentKey.current === key) setResult({ value: draft, context: key })
    } catch (reason) {
      if (!controller.signal.aborted && currentKey.current === key) setError(reason instanceof Error ? reason.message : String(reason))
    } finally {
      if (inFlight.current === controller) {
        inFlight.current = undefined
        setRunning(false)
      }
    }
  }

  return <section className="formation-agent" aria-labelledby="formation-agent-title" data-tutorial="formation-ai">
    <h4 id="formation-agent-title">Draft a formation interpretation</h4>
    <p>Ask AI to compare the formations and suggest a name, target rationale and correlation notes.
      It starts with a brief, then uses read-only tools to analyze or inspect more evidence and your notes as needed.</p>
    <div className="task-actions">
      <button type="button" disabled={!status?.configured || Boolean(blockReason) || running} onClick={() => void generate()}>
        {running ? 'Drafting interpretation…' : 'Draft interpretation with AI'}
      </button>
      {running && <button type="button" onClick={() => {
        inFlight.current?.abort(); inFlight.current = undefined; setRunning(false)
        setMessage('Drafting cancelled. Your notes are unchanged.')
      }}>Cancel drafting</button>}
      {!running && <button type="button" onClick={() => setRefresh(value => value + 1)}>Refresh AI status</button>}
    </div>
    {!status && !statusError && <p role="status">Checking AI availability…</p>}
    {statusError && <p role="alert">{statusError}</p>}
    {status && !status.configured && <p>{status.reason || 'AI is not configured. You can still write and save notes yourself.'}</p>}
    {blockReason && <p>{blockReason}</p>}
    {running && <p role="status">Reading the formation evidence and drafting notes. This may take a moment; the request uses the configured AI service.</p>}
    {error && <p className="task-errors" role="alert">{error}</p>}
    {message && <p role="status">{message}</p>}
    {result && !currentResult && <p>The field, target or settings changed. Generate a new draft for this analysis.</p>}
    {currentResult && <div className="formation-agent-draft">
      <h4>Review the AI draft</h4>
      <dl className="formation-agent-text">
        <dt>Suggested name</dt><dd>{currentResult.draft.name}</dd>
        <dt>Reason for the target</dt><dd>{currentResult.draft.rationale}</dd>
        <dt>Correlation notes</dt><dd>{currentResult.draft.correlationNotes}</dd>
      </dl>
      {currentResult.draft.limitations.length > 0 && <>
        <h5>Checks before using this interpretation</h5>
        <ul>{currentResult.draft.limitations.map((item, index) => <li key={index}>{item}</li>)}</ul>
      </>}
      <details><summary>Supporting evidence ({currentResult.draft.citedEvidenceIds.length})</summary>
        <ul>{currentResult.draft.citedEvidenceIds.map(id => <li key={id}>{labels.get(id) ?? id}<br /><code>{id}</code></li>)}</ul>
      </details>
      <label className="operator-check"><input type="checkbox" checked={reviewed === reviewKey}
        disabled={running || Boolean(blockReason)}
        onChange={event => setReviewed(event.target.checked ? reviewKey : '')} />
        {replacing ? 'I reviewed this draft and want to replace my current reason and correlation notes.'
          : 'I reviewed the interpretation, supporting evidence and limitations.'}
      </label>
      <div className="task-actions">
        <button type="button" disabled={running || Boolean(blockReason) || reviewed !== reviewKey} onClick={() => {
          if (reviewed !== reviewKey || blockReason) return
          onUse(currentResult); setReviewed(''); setMessage('Draft added to the form. Edit it as needed, then save the hypothesis.')
        }}>Use this draft</button>
        <JsonDownload value={currentResult} filename="formation-interpretation-draft.json">Download AI draft JSON</JsonDownload>
      </div>
      <p>Using the draft fills this form and retains its evidence references. An existing hypothesis name is kept.
        Saving the hypothesis is a separate step.</p>
    </div>}
  </section>
}
