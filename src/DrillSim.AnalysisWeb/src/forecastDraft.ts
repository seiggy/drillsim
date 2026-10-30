import { visibleEvidenceIds } from './evidence'
import { agentRequest, type AgentStatus as FormationAgentStatus } from './agentApi'
import { getOperatorSession } from './operator'
import { predictionTemplate } from './prediction'
import { createPredictionBinding, predictionSourceBlock, type PredictionSourceContext } from './predictionBinding'
import type {
  AnalysisConfiguration, FluidContactPrediction, PredictionBody, ProductionForecast, QuantileValues,
} from './types'

export const aiForecastVersion = 'forecast-draft-v1'
export const aiForecastAssumption = `AI-DRAFTED (${aiForecastVersion}): reviewed by a person before use; depth, fluid, contact and production values are uncalibrated model estimates, not truth.`

export interface ForecastDraftRequest {
  scenarioId: string
  candidateId: string
  configuration: AnalysisConfiguration
  packageSha256: string
  analysisSha256: string
}
export interface StructureControl {
  geologyEvidenceId: string
  wellEvidenceId: string | null
  distanceM: number
  topTrueVerticalDepthM: number
  baseTrueVerticalDepthM: number
  depthBasis: string
}
export interface ForecastDraft {
  totalDepthM: number
  expectedPaydirtM: QuantileValues
  formationTopTrueVerticalDepthM: QuantileValues
  formationBaseTrueVerticalDepthM: QuantileValues
  fluidClasses: PredictionBody['fluidClasses']
  contactPredictions: FluidContactPrediction[]
  productionForecasts: ProductionForecast[]
  uncertaintyAssumptions: string[]
  citedEvidenceIds: string[]
  rationale: string
  limitations: string[]
}
export interface ForecastDraftResult {
  version: typeof aiForecastVersion
  promptVersion: 'forecast-draft-prompt-v3'
  scenarioId: string
  candidateId: string
  packageSha256: string
  analysisSha256: string
  configurationSha256: string
  generatedAt: string
  structureEstimate: {
    method: string
    controlCount: number
    topTrueVerticalDepthM: QuantileValues | null
    baseTrueVerticalDepthM: QuantileValues | null
    grossThicknessM: number | null
    nearestControls: StructureControl[]
    limitations: string[]
  }
  draft: ForecastDraft
}

const base = '/analysis-api/api/forecast-drafts'

export function forecastRequest(context: PredictionSourceContext): ForecastDraftRequest | undefined {
  const { scenario, fieldPackage, analysis, candidate } = context
  if (!candidate || !analysis.configuration || !analysis.analysisSha256 || predictionSourceBlock(context)) return undefined
  return { scenarioId: scenario.scenarioId, candidateId: candidate.candidateId, configuration: analysis.configuration,
    packageSha256: fieldPackage.sha256, analysisSha256: analysis.analysisSha256 }
}

export const forecastRequestKey = (request?: ForecastDraftRequest) => JSON.stringify(request ?? null)

export async function getForecastAgentStatus(signal?: AbortSignal) {
  const status = await agentRequest<FormationAgentStatus>(`${base}/status`, { signal })
  if (typeof status.configured !== 'boolean') throw new Error('AI availability could not be verified. Refresh AI status before trying again.')
  return status
}

const quantiles = (q: unknown): q is QuantileValues => {
  const value = q as QuantileValues | null
  return Boolean(value) && [value!.p90, value!.p50, value!.p10].every(Number.isFinite) &&
    value!.p90 >= 0 && value!.p90 <= value!.p50 && value!.p50 <= value!.p10
}

export function verifyForecastDraft(value: ForecastDraftResult, request: ForecastDraftRequest, context: PredictionSourceContext) {
  if (!value || value.version !== aiForecastVersion || value.promptVersion !== 'forecast-draft-prompt-v3' ||
      value.scenarioId !== request.scenarioId || value.candidateId !== request.candidateId ||
      value.packageSha256 !== request.packageSha256 || value.analysisSha256 !== request.analysisSha256 ||
      value.configurationSha256 !== context.analysis.configurationSha256 || !Number.isFinite(Date.parse(value.generatedAt))) {
    throw new Error('The AI forecast does not match the displayed scenario, target and analysis. Nothing was changed.')
  }
  const draft = value.draft
  const visible = visibleEvidenceIds(context.fieldPackage)
  const years = draft?.productionForecasts?.map(item => item.year).sort((a, b) => a - b)
  if (!draft || !quantiles(draft.expectedPaydirtM) || !quantiles(draft.formationTopTrueVerticalDepthM) || !quantiles(draft.formationBaseTrueVerticalDepthM) ||
      !Number.isFinite(draft.totalDepthM) || draft.totalDepthM < draft.formationBaseTrueVerticalDepthM.p10 ||
      !Array.isArray(draft.fluidClasses) || !draft.fluidClasses.length || draft.fluidClasses.some(f => !['Oil', 'Gas', 'Water'].includes(f)) ||
      !Array.isArray(draft.contactPredictions) || draft.contactPredictions.some(c => !['GOC', 'GWC', 'OWC'].includes(c.contactType) || !quantiles(c.trueVerticalDepthM)) ||
      JSON.stringify(years) !== '[1,3,5]' ||
      draft.productionForecasts.some(p => [p.oilM3, p.gasM3, p.waterM3].some(v => !Number.isFinite(v) || v < 0)) ||
      !Array.isArray(draft.citedEvidenceIds) || !draft.citedEvidenceIds.length || draft.citedEvidenceIds.some(id => !visible.has(id)) ||
      typeof draft.rationale !== 'string' || !draft.rationale.trim() ||
      !Array.isArray(draft.uncertaintyAssumptions) || !Array.isArray(draft.limitations)) {
    throw new Error('The AI forecast is incomplete or cites evidence outside the frozen package. Nothing was changed.')
  }
}

export async function draftForecast(context: PredictionSourceContext, signal: AbortSignal) {
  const request = forecastRequest(context)
  if (!request) throw new Error(predictionSourceBlock(context) || 'Select an eligible target from the frozen-source analysis first.')
  const session = await getOperatorSession(signal)
  if (!session.enabled || !session.csrfRequestToken || session.csrfHeaderName !== 'X-DrillSim-CSRF') {
    throw new Error(session.reason || 'A local operator session is required to request an AI forecast.')
  }
  signal.throwIfAborted()
  const value = await agentRequest<ForecastDraftResult>(base, {
    method: 'POST', signal,
    headers: { 'Content-Type': 'application/json', [session.csrfHeaderName]: session.csrfRequestToken,
      'Idempotency-Key': crypto.randomUUID() },
    body: JSON.stringify(request),
  })
  signal.throwIfAborted()
  verifyForecastDraft(value, request, context)
  return value
}

/** Builds a complete bound prediction document from a reviewed AI forecast at the selected vertical target. */
export function applyForecastDraft(context: PredictionSourceContext, result: ForecastDraftResult): string {
  const binding = createPredictionBinding(context)
  if (binding.target.candidateId !== result.candidateId || binding.analysisSha256 !== result.analysisSha256) {
    throw new Error('The selected target or analysis changed after drafting. Generate a new AI forecast.')
  }
  const body = JSON.parse(predictionTemplate(context.scenario, context.fieldPackage, context.candidate, binding)) as PredictionBody
  const { draft } = result
  const { eastingM, northingM } = binding.target
  body.proposedWellPath = [
    { measuredDepthM: 0, trueVerticalDepthM: 0, eastingM, northingM },
    { measuredDepthM: draft.totalDepthM, trueVerticalDepthM: draft.totalDepthM, eastingM, northingM },
  ]
  body.formations = [{ formationName: context.scenario.reservoirName,
    topTrueVerticalDepthM: { ...draft.formationTopTrueVerticalDepthM },
    baseTrueVerticalDepthM: { ...draft.formationBaseTrueVerticalDepthM } }]
  body.expectedPaydirtM = { ...draft.expectedPaydirtM }
  body.fluidClasses = [...draft.fluidClasses]
  body.contactPredictions = draft.contactPredictions.map(c => ({ contactType: c.contactType, trueVerticalDepthM: { ...c.trueVerticalDepthM } }))
  body.productionForecasts = [...draft.productionForecasts].sort((a, b) => a.year - b.year)
    .map(({ year, oilM3, gasM3, waterM3 }) => ({ year, oilM3, gasM3, waterM3 }))
  body.uncertaintyAssumptions = [aiForecastAssumption,
    'Expected pay is a forecast of qualifying rock (porosity and permeability cutoffs), starting from the screening estimate; uncalibrated, not reserves.',
    ...draft.uncertaintyAssumptions]
  body.citedEvidenceIds = [...new Set([...body.citedEvidenceIds, ...draft.citedEvidenceIds])]
  body.rationale = draft.rationale
  return JSON.stringify(body, null, 2)
}
