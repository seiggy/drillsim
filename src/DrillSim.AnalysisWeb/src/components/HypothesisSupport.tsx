import { useEffect, useRef, useState } from 'react'
import { downloadJson } from '../analysisConfiguration'
import { HypothesisApiError, writeHypothesis } from '../hypotheses'
import type { HypothesisAttempt, HypothesisChallenge, HypothesisRevision } from '../hypotheses'
import { RecordedValue } from './HypothesisPresentation'

export function readTextDraft<T extends Record<string, string>>(text: string, initial: T): T {
  const parsed: unknown = JSON.parse(text)
  if (!parsed || typeof parsed !== 'object' || Array.isArray(parsed) ||
      Object.keys(initial).some((key) => !(key in parsed) || typeof (parsed as Record<string, unknown>)[key] !== 'string')) {
    throw new Error('The local text draft is unreadable. Download its raw backup before removing it; no saved revision was substituted.')
  }
  return parsed as T
}
export function useTextDraft<T extends Record<string, string>>(key: string, initial: T) {
  const [loaded] = useState(() => {
    let raw = ''
    try {
      raw = sessionStorage.getItem(key) ?? ''
      return { value: raw ? readTextDraft(raw, initial) : initial, error: '', raw }
    } catch (error) { return { value: initial, error: String(error), raw } }
  })
  const [value, setValue] = useState(loaded.value)
  const [error, setError] = useState(loaded.error)
  const [raw, setRaw] = useState(loaded.raw)
  function replace(next: T) {
    setValue(next)
    const text = JSON.stringify(next)
    setRaw(text)
    try { sessionStorage.setItem(key, text); setError('') }
    catch (reason) { setError(`Local backup failed; keep this page open or download the text. ${String(reason)}`) }
  }
  return { value, error, raw, replace, set: (field: keyof T, text: string) => replace({ ...value, [field]: text }) }
}

export function DraftWarning({ error, raw }: { error: string; raw: string }) {
  return error ? <div className="task-errors" role="alert"><p>{error}</p>
    <button type="button" onClick={() => downloadJson({ rawLocalDraft: raw }, 'hypothesis-local-draft.json')}>Download local backup</button>
  </div> : null
}
export function HypothesisTable({ label, children }: { label: string; children: React.ReactNode }) {
  return <div className="task-table-scroll" tabIndex={0} role="region" aria-label={label}><table className="scenario-table">{children}</table></div>
}

export interface HypothesisWrites {
  busy: boolean
  blocked: boolean
  pending?: HypothesisAttempt
  error: string
  message: string
  receipt?: HypothesisRevision | HypothesisChallenge
  send: (attempt: Omit<HypothesisAttempt, 'key'>) => Promise<HypothesisRevision | HypothesisChallenge | undefined>
}
export function parseHypothesisAttempt(raw: string): HypothesisAttempt {
  const attempt = JSON.parse(raw) as HypothesisAttempt
  if (!attempt || !['POST', 'PUT'].includes(attempt.method) || !['revision', 'challenge'].includes(attempt.kind) ||
      typeof attempt.path !== 'string' || !/^(|\/branches|\/[a-zA-Z0-9-]+\/revisions(?:\/\d+\/challenges(?:\/[a-zA-Z0-9-]+\/dispositions)?)?)$/.test(attempt.path) ||
      typeof attempt.key !== 'string' || !/^[\x21-\x7e]{1,128}$/.test(attempt.key) ||
      !attempt.body || typeof attempt.body !== 'object' || typeof attempt.label !== 'string' ||
      (attempt.expectedVersion !== undefined && (!Number.isInteger(attempt.expectedVersion) || attempt.expectedVersion < 1))) {
    throw new Error('Saved hypothesis action is invalid; inspect server history before retiring the local action record.')
  }
  return attempt
}
export function PendingHypothesisAttempt({ attempt }: { attempt: HypothesisAttempt }) {
  return <details><summary>Captured request and stable attempt key</summary>
    <p><strong>{attempt.label}</strong></p>
    <dl>
      <div><dt>Stable attempt key</dt><dd><code>{attempt.key}</code></dd></div>
      <div><dt>Expected saved version</dt><dd>{attempt.expectedVersion ?? 'New artifact'}</dd></div>
      <div><dt>Request</dt><dd>{attempt.method} <code>/api/hypotheses{attempt.path}</code></dd></div>
    </dl>
    <RecordedValue field="body" value={attempt.body} downloadLabel="Download captured request JSON" />
    <button type="button" onClick={() => downloadJson(attempt, `hypothesis-attempt-${attempt.key}.json`)}>Download captured request JSON</button>
  </details>
}
export function useHypothesisWrites(scopeKey: string) {
  const storageKey = `hypothesis-pending-v1:${scopeKey}`
  const [loaded] = useState(() => {
    try {
      const text = sessionStorage.getItem(storageKey)
      return { pending: text ? parseHypothesisAttempt(text) : undefined, error: '' }
    } catch (error) { return { pending: undefined, error: String(error) } }
  })
  const [pending, setPending] = useState(loaded.pending)
  const [error, setError] = useState(loaded.error)
  const [busy, setBusy] = useState(false)
  const [message, setMessage] = useState('')
  const [receipt, setReceipt] = useState<HypothesisRevision | HypothesisChallenge>()
  const [reviewed, setReviewed] = useState(false)
  const [conflict, setConflict] = useState(false)
  const [storageBlocked, setStorageBlocked] = useState(Boolean(loaded.error))
  const active = useRef(true)
  const sending = useRef(false)
  useEffect(() => { active.current = true; return () => { active.current = false } }, [])
  async function perform(attempt: HypothesisAttempt) {
    if (sending.current) return
    sending.current = true
    setBusy(true)
    setError('')
    setReviewed(false)
    setConflict(false)
    try {
      sessionStorage.setItem(storageKey, JSON.stringify(attempt))
      setPending(attempt)
      const value = await writeHypothesis(attempt)
      if (active.current) {
        setReceipt(value)
        setMessage(`${attempt.label} confirmed. ${'revision' in value ? `Saved revision ${value.revision}` : `Challenge version ${value.version}`}.`)
      }
      try {
        sessionStorage.removeItem(storageKey)
        if (active.current) setPending(undefined)
      } catch (reason) {
        if (active.current) setError(`Server save confirmed, but the local action record could not be cleared. Inspect the confirmed artifact and retire the local record. ${String(reason)}`)
      }
      return value
    } catch (reason) {
      if (active.current) {
        setError(`${String(reason)} Your text is retained. No automatic rebase, replacement, or retry was performed.`)
        setConflict(reason instanceof HypothesisApiError && [400, 409, 428].includes(reason.status))
      }
    } finally {
      sending.current = false
      if (active.current) setBusy(false)
    }
  }
  const writes: HypothesisWrites = {
    busy, blocked: Boolean(pending || storageBlocked), pending, error, message, receipt,
    send: (attempt) => pending || busy || storageBlocked ? Promise.resolve(undefined) : perform({ ...attempt, key: crypto.randomUUID() }),
  }
  const panel = <div className="hypothesis-write-status">
    {error && <p className="task-errors" role="alert">{error}</p>}
    {message && <p role="status">{message}</p>}
    {(pending || storageBlocked) && <div className="task-notice">
      <p>{pending?.label ?? 'Unreadable saved action'} · an unresolved local attempt blocks new saves.
        Read the saved revision/challenge history below. A conflict needs explicit reconciliation and a new attempt; uncertain transport/token errors reuse the same captured request.</p>
      {pending && <PendingHypothesisAttempt attempt={pending} />}
      <label className="operator-check"><input type="checkbox" checked={reviewed} disabled={busy} onChange={(event) => setReviewed(event.target.checked)} />
        I inspected saved server history before retrying or retiring this local attempt.
      </label>
      <div className="task-actions">
        {pending && <button type="button" disabled={busy || !reviewed || conflict} onClick={() => void perform(pending)}>Retry captured request</button>}
        <button type="button" disabled={busy || !reviewed} onClick={() => {
          if (!window.confirm('Retire only this local retry record after inspecting server history? No server artifact is deleted or rolled back. Your editor text is kept.')) return
          try { sessionStorage.removeItem(storageKey); setPending(undefined); setError(''); setStorageBlocked(false); setReviewed(false); setConflict(false) }
          catch (reason) { setError(String(reason)) }
        }}>Retire local action record</button>
      </div>
    </div>}
    {busy && <p role="status">Saving through the antiforgery-protected local workflow… Navigation never cancels or rolls back a submitted save.</p>}
  </div>
  return { writes, panel }
}
