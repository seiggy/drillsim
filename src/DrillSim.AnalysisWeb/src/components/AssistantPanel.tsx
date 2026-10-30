import { useChat } from '@ai-sdk/react'
import type { UIMessage } from 'ai'
import { Fragment, useEffect, useMemo, useRef, useState, type ReactNode } from 'react'
import { useAssistantContextSnapshot } from '../assistantContext'
import { AguiChatTransport } from '../assistantTransport'
import { AssistantChart, parseChartSpec } from './AssistantChart'
import type { Screen } from '../workflow'

const suggestions: Record<string, string[]> = {
  field: ['Which wells matter most for this reservoir?', 'What does net pay mean here?'],
  prospects: ['Why is target #1 ranked first?', 'How do targets #1 and #2 differ?'],
  forecast: ['Is this forecast reasonable?', 'What could make this well disappoint?'],
  drill: ['What happens when I drill this well?'],
  results: ['Why did the forecast miss?', 'Explain this result to a manager in three sentences.'],
  settings: ['What do the rock cutoffs change?'],
}

const toolLabels: Record<string, string> = {
  get_reservoir_model_column: 'Read the reservoir model',
  list_logged_wells: 'Listed the logged wells',
  skill: 'Opened the DrillSim glossary',
}

/** Minimal, injection-safe markdown: paragraphs, bullet and numbered lists, **bold** and `code`. */
function inline(text: string): ReactNode[] {
  return text.split(/(\*\*[^*]+\*\*|`[^`]+`)/g).map((piece, index) =>
    piece.startsWith('**') && piece.endsWith('**') && piece.length > 4 ? <strong key={index}>{piece.slice(2, -2)}</strong>
      : piece.startsWith('`') && piece.endsWith('`') && piece.length > 2 ? <code key={index}>{piece.slice(1, -1)}</code>
      : <Fragment key={index}>{piece}</Fragment>)
}

const tableSeparator = /^\s*\|?\s*:?-+:?\s*(\|\s*:?-+:?\s*)*\|?\s*$/
const numberCell = /^[-+−~≈]?\s*[\d.,]+\s*(%|[a-zA-Z³²/]{1,8})?$/
const maximumTableRows = 60
function cells(line: string) {
  return line.trim().replace(/^\|/, '').replace(/(?<!\\)\|$/, '').split(/(?<!\\)\|/).map(cell => cell.trim().replace(/\\\|/g, '|'))
}

/** GitHub-style pipe table; column alignment comes from the separator row, or numbers are right-aligned. */
function Table({ header, separator, rows }: { header: string[]; separator: string[]; rows: string[][] }) {
  const align = header.map((_, column) => {
    const rule = separator[column] ?? ''
    if (rule.endsWith(':')) return rule.startsWith(':') ? 'center' : 'num'
    if (rule.startsWith(':')) return ''
    return rows.length && rows.every(row => !row[column] || numberCell.test(row[column])) ? 'num' : ''
  })
  return <div className="assistant-table" tabIndex={0}><table>
    <thead><tr>{header.map((cell, column) => <th key={column} className={align[column] || undefined}>{inline(cell)}</th>)}</tr></thead>
    <tbody>{rows.slice(0, maximumTableRows).map((row, index) => <tr key={index}>{header.map((_, column) =>
      <td key={column} className={align[column] || undefined}>{inline(row[column] ?? '')}</td>)}</tr>)}</tbody>
  </table>{rows.length > maximumTableRows && <p className="assistant-tool">Showing the first {maximumTableRows} of {rows.length} rows.</p>}</div>
}

export function Markdown({ text }: { text: string }) {
  const blocks: ReactNode[] = []
  let list: { ordered: boolean; items: string[] } | undefined
  const flush = () => {
    if (!list) return
    const items = list.items.map((item, index) => <li key={index}>{inline(item)}</li>)
    blocks.push(list.ordered ? <ol key={blocks.length}>{items}</ol> : <ul key={blocks.length}>{items}</ul>)
    list = undefined
  }
  const lines = text.split('\n')
  for (let index = 0; index < lines.length; index++) {
    const line = lines[index]
    if (line.includes('|') && tableSeparator.test(lines[index + 1] ?? '') && lines[index + 1].includes('-')) {
      flush()
      const header = cells(line), separator = cells(lines[index + 1]), rows: string[][] = []
      index += 2
      while (index < lines.length && lines[index].includes('|') && lines[index].trim()) rows.push(cells(lines[index++]))
      index--
      blocks.push(<Table key={blocks.length} header={header} separator={separator} rows={rows} />)
      continue
    }
    const bullet = line.match(/^\s*[-*]\s+(.*)$/), numbered = line.match(/^\s*\d+[.)]\s+(.*)$/)
    if (bullet || numbered) {
      const ordered = Boolean(numbered)
      if (list && list.ordered !== ordered) flush()
      list ??= { ordered, items: [] }
      list.items.push((bullet ?? numbered)![1])
    } else if (!line.trim()) flush()
    else {
      flush()
      const heading = line.match(/^#{1,4}\s+(.*)$/)
      blocks.push(heading ? <p key={blocks.length} className="assistant-heading">{inline(heading[1])}</p> : <p key={blocks.length}>{inline(line)}</p>)
    }
  }
  flush()
  return <>{blocks}</>
}

/** The server refuses invalid charts with {"shown":false,...}; the model then retries, so the refused call draws nothing. */
function rejected(output: unknown) {
  let value = output
  if (typeof value === 'string') try { value = JSON.parse(value) } catch { return false }
  return typeof value === 'object' && value !== null && (value as { shown?: unknown }).shown === false
}

function MessageBody({ message }: { message: UIMessage }) {
  const parts = message.parts
  return <>{parts.map((part, index) => {
    if (part.type === 'text') return message.role === 'user' ? <p key={index}>{part.text}</p> : <Markdown key={index} text={part.text} />
    if (part.type !== 'dynamic-tool') return null
    if (part.toolName === 'show_chart') {
      if (part.state !== 'output-available') return <p key={index} className="assistant-tool">… Drawing a chart</p>
      if (rejected(part.output)) return null
      const spec = parseChartSpec(part.input)
      return typeof spec === 'string' ? <p key={index} className="assistant-tool">Couldn’t draw a chart: {spec}</p> : <AssistantChart key={index} spec={spec} />
    }
    const previous = parts[index - 1]
    if (previous?.type === 'dynamic-tool' && previous.toolName === part.toolName) return null
    let end = index
    while (parts[end + 1]?.type === 'dynamic-tool' && (parts[end + 1] as typeof part).toolName === part.toolName) end++
    const run = parts.slice(index, end + 1) as (typeof part)[]
    const done = run.every(item => item.state === 'output-available')
    return <p key={index} className="assistant-tool">
      {done ? '✓' : '…'} {toolLabels[part.toolName] ?? `Used ${part.toolName}`}{run.length > 1 ? ` ×${run.length}` : ''}</p>
  })}</>
}

export function AssistantPanel({ open, screen, onClose }: { open: boolean; screen: Screen; onClose: () => void }) {
  const transport = useMemo(() => new AguiChatTransport(), [])
  const { messages, sendMessage, status, stop, error, setMessages, clearError } = useChat({ transport })
  const context = useAssistantContextSnapshot()
  const [draft, setDraft] = useState('')
  const [available, setAvailable] = useState<{ configured: boolean; reason: string | null }>()
  const input = useRef<HTMLTextAreaElement>(null)
  const log = useRef<HTMLDivElement>(null)
  const busy = status === 'submitted' || status === 'streaming'

  useEffect(() => {
    if (!open || available) return
    fetch('/analysis-api/api/assistant/status').then(response => response.ok ? response.json() : Promise.reject(new Error(String(response.status))))
      .then(setAvailable).catch(() => setAvailable({ configured: false, reason: 'The assistant service could not be reached.' }))
  }, [open, available])
  useEffect(() => { if (open) input.current?.focus() }, [open])
  useEffect(() => { log.current?.scrollTo({ top: log.current.scrollHeight }) }, [messages, status])

  function send(text: string) {
    const question = text.trim()
    if (!question || busy) return
    clearError()
    void sendMessage({ text: question })
    setDraft('')
  }

  return <aside id="assistant-panel" className={`assistant-panel ${open ? 'open' : ''}`} aria-label="DrillSim assistant" aria-hidden={!open} inert={!open}
    onKeyDown={event => { if (event.key === 'Escape') onClose() }}>
    <header className="assistant-head">
      <div><h2>Ask DrillSim</h2><p>Answers from what’s on your screen.</p></div>
      <div className="assistant-actions">
        {messages.length > 0 && <button type="button" onClick={() => { stop(); setMessages([]); clearError() }}>New chat</button>}
        <button type="button" aria-label="Close assistant" onClick={onClose}>✕</button>
      </div>
    </header>
    <p className="assistant-sees" title={context.map(item => item.description).join('\n')}>
      Sees: {context.length ? context.map(item => item.description.split(':')[0]).join(' · ') : 'nothing yet'}</p>

    <div className="assistant-log" ref={log} aria-live="polite">
      {!messages.length && <div className="assistant-empty">
        <p>Ask about the wells, targets, forecast or results you’re looking at. The assistant reads this screen’s numbers and can look up the reservoir model.</p>
        <div className="assistant-suggestions">{(suggestions[screen] ?? suggestions.field).map(item =>
          <button key={item} type="button" disabled={busy || available?.configured === false} onClick={() => send(item)}>{item}</button>)}</div>
      </div>}
      {messages.map(message => <div key={message.id} className={`assistant-message ${message.role}`}><MessageBody message={message} /></div>)}
      {status === 'submitted' && <p className="assistant-thinking" role="status"><span className="spinner" aria-hidden="true" />Thinking…</p>}
      {error && <div className="assistant-error" role="alert"><p>The assistant couldn’t answer: {error.message}</p>
        <button type="button" onClick={() => { const last = [...messages].reverse().find(item => item.role === 'user'); clearError(); if (last) { setMessages(messages.slice(0, messages.lastIndexOf(last))); send(last.parts.flatMap(part => part.type === 'text' ? [part.text] : []).join('\n')) } }}>Try again</button></div>}
      {available?.configured === false && <p className="assistant-error" role="alert">{available.reason ?? 'The assistant is not available.'}</p>}
    </div>

    <form className="assistant-compose" onSubmit={event => { event.preventDefault(); send(draft) }}>
      <textarea ref={input} rows={2} value={draft} placeholder="Ask about this screen…" aria-label="Message the assistant"
        onChange={event => setDraft(event.target.value)}
        onKeyDown={event => { if (event.key === 'Enter' && !event.shiftKey) { event.preventDefault(); send(draft) } }} />
      {busy ? <button type="button" onClick={() => stop()}>Stop</button>
        : <button type="submit" className="primary" disabled={!draft.trim() || available?.configured === false}>Send</button>}
    </form>
    <p className="assistant-note">Synthetic demo data. AI answers can be wrong; check the numbers on screen.</p>
  </aside>
}
