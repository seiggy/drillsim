import type { FieldSummary, Scenario } from './types'

export interface SelectionOption {
  value: string
  name: string
  badge: string
  description: string
  tone?: 'complete' | 'attention'
}

const statusDescriptions: Record<string, string> = {
  Draft: 'Scenario created; no prediction has been saved.',
  Armed: 'Scenario prepared for a prediction.',
  PredictionDrafted: 'A prediction is saved and can still be edited.',
  PredictionSealed: 'Prediction locked; waiting for approval.',
  HumanApproved: 'Prediction approved for simulation.',
  WorldBound: 'Reservoir linked; ready for a simulation run.',
  Queued: 'Waiting to start.',
  Drilling: 'Drilling simulation in progress.',
  Surveying: 'Generating survey measurements.',
  Logging: 'Generating well logs.',
  CompletionDesigned: 'Completion design ready for review.',
  Producing: 'Simulating production.',
  ReadyToReveal: 'Simulated results are ready to publish.',
  Revealed: 'Simulated well data is available.',
  Scored: 'Evaluation complete.',
  Cancelled: 'The run was cancelled.',
  Failed: 'The run stopped with an error; inspect its status.',
  PublishFailed: 'Publication needs attention.',
  Reset: 'Scenario reset.',
}
const spaced = (text: string) => text.replace(/([a-z])([A-Z])/g, '$1 $2').replace(/[-_]/g, ' ')
export const scenarioStatusLabel = (status: string) => ({
  HumanApproved: 'Prediction approved',
  WorldBound: 'Simulator prepared',
  PredictionDrafted: 'Prediction saved',
  PredictionSealed: 'Prediction sealed',
  ReadyToReveal: 'Ready to publish',
  Revealed: 'Results published',
  Scored: 'Evaluation complete',
  CompletionDesigned: 'Completion review',
  AwaitingApproval: 'Awaiting completion review',
  AwaitingDependency: 'Waiting for next step',
  PublishFailed: 'Publication needs attention',
  'simulation-prepared': 'Simulator prepared',
  approved: 'Prediction approved',
  started: 'Simulation started',
  'completion-approved': 'Completion approved',
  scored: 'Evaluation complete',
}[status] ?? spaced(status).replace(/^./, letter => letter.toUpperCase()))
const scenarioDate = (scenario: Scenario) => new Date(scenario.createdUtc).toLocaleDateString(undefined, {
  year: 'numeric', month: 'short', day: 'numeric',
})

export function scenarioOptions(scenarios: Scenario[]): SelectionOption[] {
  return [
    { value: '', name: 'Live evidence', badge: 'Browse', description: 'Explore the field’s data without selecting a simulation run.' },
    ...scenarios.map(scenario => ({
      value: scenario.scenarioId,
      name: scenario.seedLabel,
      badge: spaced(scenario.status),
      description: `${statusDescriptions[scenario.status] ?? 'Inspect this scenario for details.'} Created ${scenarioDate(scenario)} · ${scenario.scenarioId.slice(0, 8)}.${scenario.clonedFieldId ? ' Includes simulated well results.' : ''}`,
      tone: scenario.status === 'Scored' ? 'complete' as const
        : ['Failed', 'PublishFailed', 'Cancelled'].includes(scenario.status) ? 'attention' as const : undefined,
    })),
  ]
}

export function fieldOptions(fields: FieldSummary[], scenarios: Scenario[]): SelectionOption[] {
  return fields.map(field => {
    const simulation = scenarios.find(scenario => scenario.clonedFieldId === field.id)
    const original = scenarios.some(scenario => scenario.sourceFieldId === field.id)
    const duplicates = fields.filter(other => other.name === field.name).length > 1
    return {
      value: field.id, name: field.name,
      badge: simulation ? 'Simulated results' : original ? 'Original data' : 'Dataset',
      description: simulation
        ? `${scenarioDate(simulation)} · ${simulation.scenarioId.slice(0, 8)} · ${spaced(simulation.status)} · ${simulation.seedLabel}`
        : original ? 'Field data before the simulated wells were added.'
          : duplicates ? `${field.description || 'Field dataset'} · ID ${field.id}` : field.description || 'Field dataset',
      tone: simulation?.status === 'Scored' ? 'complete' as const : undefined,
    }
  })
}
