import type { Scenario } from './types'

export type Screen = 'field' | 'prospects' | 'forecast' | 'drill' | 'results' | 'settings'

const forecastStates = ['Draft', 'Armed', 'PredictionDrafted', 'PredictionSealed']

/** Where an existing run belongs in the five-step flow. */
export function runStage(run: Scenario): Screen {
  if (forecastStates.includes(run.status)) return 'forecast'
  if (['Revealed', 'Scored'].includes(run.status)) return 'results'
  return 'drill'
}

/** Plain status; `runStatus` is the simulator run state when known (scenario status alone hides failures). */
export function runStageLabel(run: Scenario, runStatus?: string) {
  if (run.status === 'Scored') return 'results ready'
  if (run.status === 'Revealed') return 'awaiting evaluation'
  if (runStatus === 'Failed' || run.status === 'Failed') return 'drilling failed'
  if (runStatus === 'Cancelled' || run.status === 'Cancelled') return 'cancelled'
  if (run.status === 'Draft' || run.status === 'Armed') return 'forecast not started'
  if (run.status === 'PredictionDrafted') return 'forecast in progress'
  if (run.status === 'PredictionSealed') return 'forecast locked, needs approval'
  if (run.status === 'HumanApproved' || run.status === 'WorldBound') return runStatus ? 'drilling' : 'ready to drill'
  return 'drilling'
}
