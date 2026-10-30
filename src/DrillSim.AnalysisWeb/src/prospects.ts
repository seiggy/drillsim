import { agentRequest } from './agentApi'
import { getOperatorSession } from './operator'
import type { AnalysisConfiguration, AnalysisResult, FieldPackage, RankedCandidate } from './types'
import type { FormationStructureEstimate } from './structure'

export interface SimulatorCoverage {
  method: string
  minEastingM: number
  maxEastingM: number
  minNorthingM: number
  maxNorthingM: number
  maxTrueVerticalDepthM: number
  controlCount: number
  marginM: number
  limitation: string
}

export async function getSimulatorCoverage(fieldId: string, reservoir: string, scope?: { scenarioId: string; asOf: string }, signal?: AbortSignal) {
  const query = new URLSearchParams({ reservoir, ...(scope ? { scenarioId: scope.scenarioId, asOf: scope.asOf } : {}) })
  const response = await fetch(`/analysis-api/api/fields/${encodeURIComponent(fieldId)}/simulator-coverage?${query}`, { signal })
  if (!response.ok) return undefined
  const value = await response.json() as SimulatorCoverage
  return [value.minEastingM, value.maxEastingM, value.minNorthingM, value.maxNorthingM, value.maxTrueVerticalDepthM].every(Number.isFinite)
    ? value : undefined
}

export const insideCoverage = (candidate: Pick<RankedCandidate, 'eastingM' | 'northingM'>, coverage?: SimulatorCoverage) =>
  !coverage || candidate.eastingM >= coverage.minEastingM && candidate.eastingM <= coverage.maxEastingM &&
    candidate.northingM >= coverage.minNorthingM && candidate.northingM <= coverage.maxNorthingM

export type BriefRecommendation = 'Drill' | 'DrillWithCaution' | 'NotYet'
export interface ProspectBrief {
  version: 'prospect-brief-v1'
  candidateId: string
  analysisSha256: string
  generatedAt: string
  structureEstimate: FormationStructureEstimate
  brief: {
    headline: string
    recommendation: BriefRecommendation
    reasons: Array<{ text: string; citedEvidenceIds: string[] }>
    risks: Array<{ text: string; severity: 'High' | 'Medium' | 'Low'; citedEvidenceIds: string[] }>
    nextChecks: string[]
  }
}

export const recommendationLabel: Record<BriefRecommendation, string> = {
  Drill: 'Worth drilling', DrillWithCaution: 'Drill with caution', NotYet: 'Not yet — check first',
}

export async function getProspectBriefStatus(signal?: AbortSignal) {
  try { return await agentRequest<{ configured: boolean; reason: string | null }>('/analysis-api/api/prospect-briefs/status', { signal }) }
  catch (reason) { return { configured: false, reason: reason instanceof Error ? reason.message : String(reason) } }
}

export async function draftProspectBrief(pkg: FieldPackage, analysis: AnalysisResult, reservoir: string,
  configuration: AnalysisConfiguration, candidate: RankedCandidate, signal: AbortSignal) {
  const session = await getOperatorSession(signal)
  if (!session.enabled || !session.csrfRequestToken) throw new Error(session.reason || 'The local AI service is not available.')
  const value = await agentRequest<ProspectBrief>('/analysis-api/api/prospect-briefs', {
    method: 'POST', signal,
    headers: { 'Content-Type': 'application/json', [session.csrfHeaderName]: session.csrfRequestToken, 'Idempotency-Key': crypto.randomUUID() },
    body: JSON.stringify({ fieldId: pkg.fieldId, reservoirName: reservoir, scenarioId: null, asOfUtc: null, configuration,
      packageSha256: pkg.sha256, analysisSha256: analysis.analysisSha256, candidateId: candidate.candidateId }),
  })
  if (value.version !== 'prospect-brief-v1' || value.candidateId !== candidate.candidateId || !value.brief?.headline ||
      !Array.isArray(value.brief.reasons) || !Array.isArray(value.brief.risks)) {
    throw new Error('The AI returned an incomplete explanation. Nothing was changed; try again.')
  }
  return value
}
