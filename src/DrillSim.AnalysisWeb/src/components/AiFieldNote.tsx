import type { AgentRunState, AgentStatus, RankedCandidate } from '../types'

const actions = [
  { id: 'challenge', label: 'Challenge ranking', prompt: 'Challenge the preferred ranking using contradictory or missing evidence' },
  { id: 'alternate', label: 'Discuss alternative', prompt: 'Discuss a materially different geologic interpretation. This is a field note only, not a saved or recalculated alternative.' },
  { id: 'audit', label: 'Audit evidence', prompt: 'Audit provenance, data gaps, and uncertainty that could invalidate this recommendation' },
]

export function AiFieldNote({
  status,
  state,
  candidate,
  onRun,
  actions: actionIds = ['alternate', 'audit'],
  blockReason,
  context,
}: {
  status?: AgentStatus
  state: AgentRunState
  candidate?: RankedCandidate
  onRun: (prompt: string) => void
  actions?: string[]
  blockReason?: string
  context?: string
}) {
  return (
    <section className="ai-field-note" aria-labelledby="ai-note-title">
      <header>
        <div><h2 id="ai-note-title">AI field note</h2><p>Runs only on request in the labeled live context. To persist a critique, review it and attach cited objections to an exact saved hypothesis in Challenge; notes are never saved automatically.</p></div>
        <span className={`agent-lamp ${status?.configured ? 'ready' : ''}`}>{status?.configured ? 'MODEL READY' : 'MODEL OFFLINE'}</span>
      </header>
      <div className="ai-actions">
        {actions.filter((action) => actionIds.includes(action.id)).map((action) => (
          <button key={action.id} onClick={() => onRun(action.prompt)} disabled={!status?.configured || !candidate || state.running || Boolean(blockReason)}>
            {action.label}
          </button>
        ))}
      </div>
      {blockReason && <p>{blockReason}</p>}
      {context && <p className="ai-context">{context}</p>}
      <div className="field-note-output" aria-live="polite">
        {state.running && !state.output && <p>Agent is tracing the selected hypothesis…</p>}
        {state.output && <p>{state.output}</p>}
        {state.error && <p className="error">{state.error}</p>}
        {!state.running && !state.output && !state.error && (
          <p>{status?.configured ? 'Select a target, then ask the model to challenge or extend the interpretation.' : 'Set AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_DEPLOYMENT_NAME to activate this layer.'}</p>
        )}
      </div>
    </section>
  )
}
