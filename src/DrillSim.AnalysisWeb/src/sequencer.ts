import type { WorkspaceMode } from './components/AnalysisWorkspace'
import type { AnalysisResult, FieldPackage, Scenario, ScenarioResources } from './types'
import { operatorLifecycleLabel } from './operator'

export const tasks = [
  { id: 'evidence', label: 'Evidence', group: 'evidence', mode: 'logs', description: 'Inspect the loaded evidence inventory, curve coverage and provenance.' },
  { id: 'qc', label: 'Units & QC', group: 'evidence', mode: 'logs', description: 'Review source units, depth references, null flags and missing values without editing source evidence.' },
  { id: 'position', label: 'Position', group: 'evidence', mode: 'map', description: 'Inspect coordinate diagnostics and select a well to locate its evidence.' },
  { id: 'correlation', label: 'Correlation', group: 'evidence', mode: 'section', description: 'Inspect multiple bore formation bounds and save explicit correlation/control annotations on a hypothesis revision.' },
  { id: 'criteria', label: 'Rock criteria', group: 'interpret', mode: 'crossplot', description: 'Edit validated rock cutoffs, then explicitly apply a new analysis configuration.' },
  { id: 'pay', label: 'Pay intervals', group: 'interpret', mode: 'logs', description: 'Reconcile per-bore rock and fluid interval thickness with the ranking proxy.' },
  { id: 'search', label: 'Search area', group: 'interpret', mode: 'map', description: 'Inspect the full candidate grid and apply supported search settings.' },
  { id: 'exclusions', label: 'Exclusions', group: 'interpret', mode: 'map', description: 'Inspect exclusions around located, logged screening controls; not all known field wells are covered.' },
  { id: 'model', label: 'Reservoir model', group: 'prospect', mode: 'trajectory', description: 'Inspect visible-evidence geometry; view-only mesh settings do not recalculate rankings.' },
  { id: 'uncertainty', label: 'Uncertainty', group: 'prospect', mode: 'crossplot', description: 'Inspect uncalibrated target quantiles and quantitative uncertainty components.' },
  { id: 'targets', label: 'Targets', group: 'prospect', mode: 'map', description: 'Sort the retained shortlist and inspect score contributions.' },
  { id: 'challenge', label: 'Challenge', group: 'challenge', mode: 'section', description: 'Attach cited critiques to an exact saved revision and review versioned dispositions; AI runs only on explicit request.' },
  { id: 'alternatives', label: 'Alternatives', group: 'challenge', mode: 'trajectory', description: 'Save named hypotheses and create spatial, conservative or custom branches using actual stored-evidence reanalysis.' },
  { id: 'compare', label: 'Compare hypotheses', group: 'challenge', mode: 'crossplot', description: 'Compare 2–8 exact saved revisions, inspect scope differences and record a decision in a saved rationale.' },
  { id: 'bundle', label: 'Evidence bundle', group: 'challenge', mode: 'trajectory', description: 'Download a current, fingerprinted analysis decision with evidence citations and your rationale.' },
  { id: 'prediction', label: 'Prediction', group: 'interpret', mode: 'trajectory', description: 'Create a scenario, review an editable forecast, save and seal it, then explicitly approve the prediction.' },
  { id: 'simulation', label: 'Simulation', group: 'prospect', mode: 'trajectory', description: 'Configure and prepare the simulator, start a run, and review completion before production.' },
  { id: 'new-evidence', label: 'New evidence', group: 'evidence', mode: 'logs', description: 'Inspect a published reveal and production history; publishing remains an operator action.' },
  { id: 'evaluation', label: 'Evaluation', group: 'challenge', mode: 'crossplot', description: 'Inspect published aggregate metrics after reveal, without exposing hidden truth.' },
] as const satisfies ReadonlyArray<{ id: string; label: string; group: string; mode: WorkspaceMode; description: string }>

export type TaskId = typeof tasks[number]['id']
export type LifecycleTaskId = Extract<TaskId, 'prediction' | 'simulation' | 'new-evidence' | 'evaluation'>
export const taskById = Object.fromEntries(tasks.map((task) => [task.id, task])) as Record<TaskId, typeof tasks[number]>
export const isLifecycleTask = (id: TaskId): id is LifecycleTaskId =>
  ['prediction', 'simulation', 'new-evidence', 'evaluation'].includes(id)

export function evidenceScopeKey(pkg: FieldPackage, reservoir: string, scenario?: Scenario) {
  return JSON.stringify([pkg.fieldId, reservoir, scenario?.scenarioId ?? null, scenario?.asOfUtc ?? null, pkg.sha256])
}

export function lifecycleStatus(id: LifecycleTaskId, scenario: Scenario | undefined, resources: ScenarioResources): string {
  if (!scenario) return 'Create or select scenario'
  if (resources.loading || resources.scenarioId !== scenario.scenarioId) return 'Loading ledger'
  if (resources.error) return 'Ledger unavailable'
  const { prediction, reveal, scorecard } = resources
  if (id === 'prediction') return prediction?.approval ? 'Approved' : prediction?.seal ? 'Sealed' : prediction ? 'Draft' : 'No draft'
  if (id === 'simulation') {
    if (resources.operator?.scenarioId === scenario.scenarioId && resources.operator.enabled) return operatorLifecycleLabel(id, resources.operator)
    if (scenario.status === 'Cancelled') return 'Cancelled'
    if (scenario.status === 'Failed') return 'Failed'
    if (['ReadyToReveal', 'Revealed', 'Scored', 'PublishFailed'].includes(scenario.status)) return 'Finished'
    if (['Queued', 'Drilling', 'Surveying', 'Logging', 'Producing'].includes(scenario.status)) return 'Running'
    if (scenario.status === 'CompletionDesigned') return 'Completion approval required'
    if (!prediction?.seal) return 'Sealed prediction required'
    if (!prediction.approval) return 'Approval required'
    if (scenario.status === 'WorldBound') return 'Bound · operator start'
    return 'Prepare simulator'
  }
  if (id === 'new-evidence') {
    if (reveal?.status === 'Revealed') return 'Revealed'
    if (resources.operator?.scenarioId === scenario.scenarioId && resources.operator.enabled) {
      const label = operatorLifecycleLabel(id, resources.operator)
      return label === 'Revealed' ? 'Reveal receipt pending' : label
    }
    if (scenario.status === 'PublishFailed') return 'Publication failed'
    if (scenario.status === 'ReadyToReveal') return 'Ready to publish'
    if (['Revealed', 'Scored'].includes(scenario.status)) return 'Reveal receipt unavailable'
    return 'Simulation incomplete'
  }
  if (scorecard) return 'Scored'
  if (resources.operator?.scenarioId === scenario.scenarioId && resources.operator.enabled) {
    const label = operatorLifecycleLabel('evaluation', resources.operator)
    return label === 'Scored' ? 'Scorecard pending' : label
  }
  return reveal?.status === 'Revealed' ? 'Scorecard pending' : 'Reveal required'
}

export function taskOutputStatus(id: TaskId, analysis: AnalysisResult, stale: boolean, scenario?: Scenario, resources?: ScenarioResources) {
  if (isLifecycleTask(id)) return lifecycleStatus(id, scenario, resources ?? { loading: false, error: '' })
  if (['correlation', 'alternatives', 'compare'].includes(id)) return 'Saved revision tools'
  if (id === 'challenge') return 'Cited review tools'
  if (id === 'bundle') return stale ? 'Stale · apply settings' : 'Export controls'
  if (['evidence', 'qc', 'position'].includes(id)) return 'Inspect evidence'
  if (stale) return 'Previous result'
  if (id === 'targets' || id === 'uncertainty') return analysis.ranking.length ? 'Result present' : 'No targets'
  return 'Inspect output'
}
