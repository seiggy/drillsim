import { tasks, taskOutputStatus, type TaskId } from '../sequencer'
import type { AnalysisResult, Scenario, ScenarioResources } from '../types'

export function HypothesisSequencer({ active, visited, onSelect, analysis, stale, scenario, resources }: {
  active: TaskId
  visited: ReadonlySet<TaskId>
  onSelect: (id: TaskId) => void
  analysis: AnalysisResult
  stale: boolean
  scenario?: Scenario
  resources: ScenarioResources
}) {
  return (
    <nav className="sequencer" aria-label="Hypothesis tasks">
      <div className="sequence-head">
        <div>
          <strong>Hypothesis sequence</strong>
          <span>Evidence → interpretation → alternatives → decision / simulation</span>
        </div>
        <span className="tempo">Visited ≠ reviewed</span>
      </div>
      <div className="sequence-rail" tabIndex={0} role="region" aria-label="Scrollable hypothesis task keys">
        <div className="step-row" style={{ '--step-count': tasks.length } as React.CSSProperties}>
          {tasks.map((task, index) => {
            const status = taskOutputStatus(task.id, analysis, stale, scenario, resources)
            return <button
              key={task.id}
              data-tutorial={`task-${task.id}`}
              className={`step-key ${task.group}`}
              aria-pressed={active === task.id}
              aria-controls="task-workspace"
              data-state={active === task.id ? 'active' : visited.has(task.id) ? 'visited' : 'unvisited'}
              title={task.description}
              aria-label={`${task.label}. ${task.description} ${status}. ${visited.has(task.id) ? 'Visited; not a review acknowledgement.' : 'Not visited.'}`}
              onClick={() => onSelect(task.id)}
            >
              <span>{String(index + 1).padStart(2, '0')} · {visited.has(task.id) ? 'visited' : 'open'}</span>
              <strong>{task.label}</strong>
              <em>{status}</em>
            </button>
          })}
          <span className="playhead" style={{ '--step': tasks.findIndex((task) => task.id === active) } as React.CSSProperties} aria-hidden="true" />
        </div>
      </div>
    </nav>
  )
}
