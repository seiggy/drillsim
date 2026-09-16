import type {
  AgentStatus,
  AnalysisResult,
  AnalysisConfiguration,
  EvidenceVisibilitySummary,
  FieldPackage,
  FieldSummary,
  JsonRecord,
  MapsStatus,
  PredictionRecord,
  PredictionBody,
  PredictionCapabilities,
  PublicProductionSeriesMetadata,
  PublicScorecard,
  RevealReceipt,
  Scenario,
} from './types'
import { getOperatorSession } from './operator'
import { verifyCreatedScenario, type ScenarioCreationPayload } from './scenarioCreation'

const apiRoot = '/analysis-api'

async function responseError(response: Response) {
  let message = 'The request could not be completed. Try again.'
  const raw = (await response.text()).trim()
  try {
    const problem: unknown = JSON.parse(raw)
    if (problem && typeof problem === 'object') {
      const detail = 'detail' in problem ? problem.detail : undefined
      const title = 'title' in problem ? problem.title : undefined
      const supplied = typeof detail === 'string' && detail.trim() ? detail : typeof title === 'string' ? title : ''
      if (supplied) message = supplied
    }
  } catch {
    if (response.status < 500 && raw && raw.length <= 1000 && !/[<>{}\[\]]/.test(raw)) message = raw
  }
  return new Error(`${response.status}: ${message}`)
}

async function getJson<T>(path: string, signal?: AbortSignal): Promise<T> {
  const response = await fetch(`${apiRoot}${path}`, { signal })
  if (!response.ok) throw await responseError(response)
  return response.json() as Promise<T>
}

async function getOptionalJson<T>(path: string, signal?: AbortSignal): Promise<T | undefined> {
  const response = await fetch(`${apiRoot}${path}`, { signal })
  if (response.status === 404) return undefined
  if (!response.ok) throw await responseError(response)
  return response.json() as Promise<T>
}

function scopeQuery(scope?: { scenarioId: string; asOf: string }) {
  if (!scope) return ''
  const query = new URLSearchParams({ scenarioId: scope.scenarioId, asOf: scope.asOf })
  return query.toString()
}

function read(record: JsonRecord | undefined, ...keys: string[]) {
  return keys.map((key) => record?.[key]).find((value) => value !== undefined)
}

export async function getFields(signal?: AbortSignal): Promise<FieldSummary[]> {
  const records = await getJson<JsonRecord[]>('/api/fields', signal)
  return records.map((record) => {
    const meta = read(record, 'MetaInfo', 'metaInfo') as JsonRecord | undefined
    return {
      id: String(read(meta, 'ID', 'id') ?? ''),
      name: String(read(record, 'Name', 'name') ?? 'Unnamed field'),
      description: String(read(record, 'Description', 'description') ?? ''),
    }
  }).filter((field) => field.id).sort((left, right) => left.name.localeCompare(right.name))
}

export const getFieldPackage = (
  fieldId: string,
  scope?: { scenarioId: string; asOf: string },
  signal?: AbortSignal,
) => {
  const query = scopeQuery(scope)
  return getJson<FieldPackage>(`/api/fields/${fieldId}/package${query ? `?${query}` : ''}`, signal)
}

export const getAnalysis = (
  fieldId: string,
  reservoir: string,
  scope?: { scenarioId: string; asOf: string },
  signal?: AbortSignal,
) => {
  const query = new URLSearchParams({ reservoir })
  const scoped = scopeQuery(scope)
  if (scoped) new URLSearchParams(scoped).forEach((value, key) => query.set(key, value))
  return getJson<AnalysisResult>(`/api/fields/${fieldId}/analysis?${query}`, signal)
}

export async function applyAnalysis(
  fieldId: string,
  reservoir: string,
  configuration: AnalysisConfiguration,
  scope?: { scenarioId: string; asOf: string },
  signal?: AbortSignal,
): Promise<AnalysisResult> {
  const response = await fetch(`${apiRoot}/api/fields/${encodeURIComponent(fieldId)}/analysis`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ ...scope, reservoir, configuration }),
    signal,
  })
  if (!response.ok) throw await responseError(response)
  return response.json() as Promise<AnalysisResult>
}

export const getScenarios = (signal?: AbortSignal) =>
  getJson<Scenario[]>('/api/scenarios', signal)

export const getScenario = (scenarioId: string, signal?: AbortSignal) =>
  getJson<Scenario>(`/api/scenarios/${scenarioId}`, signal)

export async function createScenario(payload: ScenarioCreationPayload): Promise<Scenario> {
  const response = await fetch(`${apiRoot}/api/scenarios`, {
    method: 'POST', credentials: 'same-origin',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  if (!response.ok) throw await responseError(response)
  const scenario = await response.json() as Scenario
  verifyCreatedScenario(scenario, payload)
  return scenario
}

export const getEvidenceVisibility = (scenarioId: string, asOf: string, signal?: AbortSignal) =>
  getJson<EvidenceVisibilitySummary>(
    `/api/scenarios/${scenarioId}/evidence-visibility?asOf=${encodeURIComponent(asOf)}`,
    signal,
  )

export const getPrediction = (scenarioId: string, signal?: AbortSignal) =>
  getOptionalJson<PredictionRecord>(`/api/scenarios/${scenarioId}/prediction`, signal)

export async function getPredictionCapabilities(signal?: AbortSignal) {
  const value = await getOptionalJson<PredictionCapabilities>('/api/analysis/prediction-capabilities', signal)
  if (!value) return undefined
  if (['version', 'bindingVersion', 'analysisModelVersion', 'configurationVersion', 'baselineBindingVersion',
    'sourceScope', 'targetRule', 'fourNeighborBaselineRule', 'limitation'].some(key => typeof value[key as keyof PredictionCapabilities] !== 'string') ||
    typeof value.configuredSaveSupported !== 'boolean' || typeof value.configuredSealSupported !== 'boolean' ||
    typeof value.legacyNullBindingSupported !== 'boolean' || !Number.isInteger(value.minimumLocatedControls) ||
    !Array.isArray(value.baselineModelVersions) || value.baselineModelVersions.some(version => typeof version !== 'string')) {
    throw new Error('Prediction-capabilities response is incomplete or invalid. Configured handoff remains unavailable.')
  }
  return value
}

async function writePrediction(scenarioId: string, body: PredictionBody | undefined, revision: number | null, protectedHeaders?: Record<string, string>) {
  const response = await fetch(`${apiRoot}/api/scenarios/${scenarioId}/prediction${body ? '' : '/seal'}`, {
    method: body ? 'PUT' : 'POST',
    headers: {
      ...(body ? { 'Content-Type': 'application/json' } : {}),
      ...(revision !== null ? { 'If-Match': `"${revision}"` } : {}),
      ...protectedHeaders,
    },
    credentials: 'same-origin',
    body: body ? JSON.stringify(body) : undefined,
  })
  if (!response.ok) throw await responseError(response)
  const saved: PredictionRecord = await response.json()
  if (saved.scenarioId !== scenarioId) throw new Error('Server returned a prediction for a different scenario. Reload the ledger.')
  return saved
}

export const savePrediction = (scenarioId: string, body: PredictionBody, revision: number | null) =>
  writePrediction(scenarioId, body, revision)

export async function sealPrediction(scenarioId: string, revision: number, actionKey: string) {
  if (typeof actionKey !== 'string' || !/^[\x21-\x7e]{1,128}$/.test(actionKey)) throw new Error('A stable, bounded seal attempt key is required before sending a request.')
  const session = await getOperatorSession()
  if (!session.enabled || !session.csrfRequestToken || session.csrfHeaderName !== 'X-DrillSim-CSRF') {
    throw new Error(session.reason || 'Sealing requires an enabled local operator session and antiforgery token. Refresh operator status.')
  }
  const saved = await writePrediction(scenarioId, undefined, revision, {
    [session.csrfHeaderName]: session.csrfRequestToken,
    'Idempotency-Key': actionKey,
  })
  if (!saved.seal) throw new Error('The server did not confirm a sealed prediction. Reload the ledger; no seal is assumed.')
  return saved
}

export const getReveal = (scenarioId: string, signal?: AbortSignal) =>
  getOptionalJson<RevealReceipt>(`/api/scenarios/${scenarioId}/reveal`, signal)

export const getProduction = (scenarioId: string, signal?: AbortSignal) =>
  getOptionalJson<PublicProductionSeriesMetadata>(`/api/scenarios/${scenarioId}/production`, signal)

export const getScorecard = (scenarioId: string, signal?: AbortSignal) =>
  getOptionalJson<PublicScorecard>(`/api/scenarios/${scenarioId}/scorecard`, signal)

export const getAgentStatus = (signal?: AbortSignal) =>
  getJson<AgentStatus>('/api/agent/status', signal)

export const getMapsStatus = (signal?: AbortSignal) =>
  getJson<MapsStatus>('/api/maps/status', signal)
