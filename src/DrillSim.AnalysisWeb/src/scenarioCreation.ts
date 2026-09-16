import { configurationErrors } from './analysisConfiguration'
import type { AnalysisConfiguration, Scenario } from './types'

export interface ScenarioCreationInput {
  sourceFieldId: string
  reservoirName: string
  name: string
  asOfUtc: string
  purpose: string
  assumptions: string
  configuration: AnalysisConfiguration
}

export interface ScenarioCreationPayload {
  sourceFieldId: string
  reservoirName: string
  asOfUtc: string
  seedLabel: string
  assumptionsSha256: string
}

export interface ScenarioCreationAttempt {
  version: 1
  payload: ScenarioCreationPayload
  reviewed: {
    version: 'guided-simulation-assumptions-v1'
    purpose: string
    assumptions: string
    forecastPolicy: 'editable-demonstration-estimates'
    geometry: 'point-screening-v1'
    initialAnalysisSettings: AnalysisConfiguration
  }
}

export const scenarioCreationKey = (fieldId: string, reservoir: string) =>
  `drillsim-scenario-creation-v1:${fieldId}:${reservoir}`

export function scenarioCreationError(input: ScenarioCreationInput) {
  if (!input.sourceFieldId || !input.reservoirName.trim()) return 'Choose an original field and reservoir before creating a simulation.'
  if (!input.name.trim() || input.name.trim().length > 100 || /[\u0000-\u001f\u007f]/.test(input.name)) {
    return 'Enter a scenario name of 1–100 characters without control characters.'
  }
  if (!/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}(:\d{2}(\.\d{1,7})?)?(Z|\+00:00)$/.test(input.asOfUtc) ||
      !Number.isFinite(Date.parse(input.asOfUtc))) return 'Enter a valid evidence cutoff in UTC, for example 2026-09-16T12:00:00Z.'
  if (new Date(input.asOfUtc).toISOString().slice(0, 16) !== input.asOfUtc.slice(0, 16)) return 'The evidence cutoff contains an invalid calendar date or time.'
  if (!input.purpose.trim() || !input.assumptions.trim()) return 'Describe the purpose and assumptions you are reviewing.'
  return configurationErrors(input.configuration).join(' ')
}

async function fingerprint(value: ScenarioCreationAttempt['reviewed']) {
  const digest = await crypto.subtle.digest('SHA-256', new TextEncoder().encode(JSON.stringify(value)))
  return Array.from(new Uint8Array(digest), byte => byte.toString(16).padStart(2, '0')).join('')
}

export async function prepareScenarioCreation(input: ScenarioCreationInput): Promise<ScenarioCreationAttempt> {
  const error = scenarioCreationError(input)
  if (error) throw new Error(error)
  const reviewed: ScenarioCreationAttempt['reviewed'] = {
    version: 'guided-simulation-assumptions-v1',
    purpose: input.purpose.trim(),
    assumptions: input.assumptions.trim(),
    forecastPolicy: 'editable-demonstration-estimates',
    geometry: 'point-screening-v1',
    initialAnalysisSettings: {
      version: input.configuration.version,
      porosityCutoff: input.configuration.porosityCutoff,
      permeabilityCutoffM2: input.configuration.permeabilityCutoffM2,
      wellExclusionRadiusM: input.configuration.wellExclusionRadiusM,
      gridPointsPerAxis: input.configuration.gridPointsPerAxis,
      idwNeighborCount: input.configuration.idwNeighborCount,
    },
  }
  return {
    version: 1, reviewed,
    payload: {
      sourceFieldId: input.sourceFieldId, reservoirName: input.reservoirName.trim(),
      asOfUtc: new Date(input.asOfUtc).toISOString(), seedLabel: input.name.trim(),
      assumptionsSha256: await fingerprint(reviewed),
    },
  }
}

export function readScenarioCreationAttempt(text: string, fieldId: string, reservoir: string): ScenarioCreationAttempt {
  const value = JSON.parse(text) as ScenarioCreationAttempt
  if (!value || value.version !== 1 || value.payload?.sourceFieldId !== fieldId || value.payload?.reservoirName !== reservoir ||
      value.reviewed?.version !== 'guided-simulation-assumptions-v1' ||
      value.reviewed.forecastPolicy !== 'editable-demonstration-estimates' || value.reviewed.geometry !== 'point-screening-v1' ||
      typeof value.payload.seedLabel !== 'string' || typeof value.payload.asOfUtc !== 'string' ||
      typeof value.reviewed.purpose !== 'string' || typeof value.reviewed.assumptions !== 'string' ||
      !value.reviewed.initialAnalysisSettings || !/^[a-f0-9]{64}$/.test(value.payload.assumptionsSha256) ||
      scenarioCreationError({
        sourceFieldId: fieldId, reservoirName: reservoir, name: value.payload.seedLabel, asOfUtc: value.payload.asOfUtc,
        purpose: value.reviewed.purpose, assumptions: value.reviewed.assumptions, configuration: value.reviewed.initialAnalysisSettings,
      })) throw new Error('The retained creation request is unreadable. No request was sent; preserve it and inspect the scenario list before removing it.')
  return value
}

export async function verifyScenarioCreationAttempt(attempt: ScenarioCreationAttempt) {
  readScenarioCreationAttempt(JSON.stringify(attempt), attempt.payload.sourceFieldId, attempt.payload.reservoirName)
  if (await fingerprint(attempt.reviewed) !== attempt.payload.assumptionsSha256) {
    throw new Error('The retained assumptions do not match their fingerprint. No creation request was sent.')
  }
}

export function verifyCreatedScenario(scenario: Scenario, payload: ScenarioCreationPayload) {
  if (!scenario.scenarioId || scenario.sourceFieldId !== payload.sourceFieldId || scenario.reservoirName !== payload.reservoirName ||
      scenario.seedLabel !== payload.seedLabel || scenario.assumptionsSha256 !== payload.assumptionsSha256 ||
      Date.parse(scenario.initialAsOfUtc) !== Date.parse(payload.asOfUtc)) {
    throw new Error('The response does not match this scenario request. Keep the retained request and inspect the scenario list before retrying.')
  }
}
