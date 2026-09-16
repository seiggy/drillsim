import { configurationErrors, configurationKey } from './analysisConfiguration'
import { visibleEvidenceIds } from './evidence'
import { getOperatorSession } from './operator'
import type { AnalysisConfiguration, AnalysisResult, FieldPackage, RankedCandidate, Scenario } from './types'

export interface HypothesisScope {
  fieldId: string
  reservoirName: string
  scenarioId: string | null
  asOfUtc: string | null
}
export interface HypothesisReference { hypothesisId: string; revision: number }
export interface HypothesisControlNote { evidenceId: string; note: string }
export interface HypothesisInput {
  configuration: AnalysisConfiguration
  packageSha256: string
  analysisSha256: string
  selectedCandidateId: string
  rationale: string
  correlationNotes?: string
  controlNotes?: HypothesisControlNote[]
}
export interface HypothesisSummary extends HypothesisReference {
  name: string
  scope: HypothesisScope
  packageSha256: string
  configurationSha256: string
  analysisSha256: string
  selectedCandidateId: string
  snapshotSha256: string
  createdUtc: string
}
export interface HypothesisRevision extends HypothesisReference {
  schemaVersion: string
  name: string
  scope: HypothesisScope
  input: HypothesisInput
  branchedFrom?: HypothesisReference
  createdUtc: string
  package: FieldPackage
  analysis: AnalysisResult
  snapshotSha256: string
}
export interface HypothesisSelection { scope: HypothesisScope; reference: HypothesisReference }
export type Disposition = 'open' | 'accepted' | 'rejected' | 'deferred'
export interface HypothesisChallenge {
  schemaVersion: string
  challengeId: string
  version: number
  hypothesis: HypothesisReference
  hypothesisSnapshotSha256: string
  analysisSha256: string
  summary: string
  createdBy: string
  lastModifiedBy: string
  createdUtc: string
  modifiedUtc: string
  objections: Array<{
    objectionId: string
    text: string
    citedEvidenceIds: string[]
    disposition: Disposition
    dispositionReason?: string
    dispositionActor?: string
  }>
  sha256: string
}
export interface HypothesisComparison {
  schemaVersion: string
  baseline: HypothesisReference
  entries: Array<{
    revision: HypothesisSummary
    configuration: AnalysisConfiguration
    selectedCandidate: RankedCandidate
    rationale: string
    correlationNotes?: string | null
    controlNotes?: HypothesisControlNote[] | null
    likeForLikeEvidence: boolean
    scopeDifferences: string[]
    differences: Array<{ field: string; baselineValue: unknown; value: unknown }>
  }>
  comparisonSha256: string
}
export interface HypothesisAttempt {
  key: string
  path: string
  method: 'POST' | 'PUT'
  body: object
  expectedVersion?: number
  kind: 'revision' | 'challenge'
  label: string
}
export const hypothesisScope = (pkg: FieldPackage, reservoirName: string, scenario?: Scenario): HypothesisScope => ({
  fieldId: pkg.fieldId, reservoirName, scenarioId: scenario?.scenarioId ?? null, asOfUtc: scenario?.asOfUtc ?? null,
})
export const hypothesisScopeKey = (scope: HypothesisScope) => {
  if (!scope || typeof scope.fieldId !== 'string' || !scope.fieldId || typeof scope.reservoirName !== 'string' ||
      !scope.reservoirName.trim() || !Object.hasOwn(scope, 'scenarioId') || !Object.hasOwn(scope, 'asOfUtc') ||
      (scope.scenarioId !== null && (typeof scope.scenarioId !== 'string' || !scope.scenarioId)) ||
      (scope.asOfUtc !== null && (typeof scope.asOfUtc !== 'string' || !Number.isFinite(Date.parse(scope.asOfUtc)))) ||
      Boolean(scope.scenarioId) !== Boolean(scope.asOfUtc)) throw new Error('Supply the full field/reservoir scope and either both scenario/clock values or explicit null/null.')
  return JSON.stringify([scope.fieldId, scope.reservoirName.trim().toUpperCase(), scope.scenarioId,
    scope.asOfUtc ? new Date(scope.asOfUtc).toISOString() : null])
}
export const hypothesisSelectionKey = (selection: HypothesisSelection) =>
  JSON.stringify([hypothesisScopeKey(selection.scope), selection.reference.hypothesisId, selection.reference.revision])
export const referenceOf = (revision: HypothesisReference): HypothesisReference =>
  ({ hypothesisId: revision.hypothesisId, revision: revision.revision })
export const selectionOf = (revision: HypothesisRevision | HypothesisSummary): HypothesisSelection =>
  ({ scope: revision.scope, reference: referenceOf(revision) })
export const revisionPath = (reference: HypothesisReference) =>
  `/${encodeURIComponent(reference.hypothesisId)}/revisions/${reference.revision}`

export function hypothesisQuery(scope: HypothesisScope, page?: { limit: number; offset: number }) {
  hypothesisScopeKey(scope)
  const query = new URLSearchParams({ fieldId: scope.fieldId, reservoir: scope.reservoirName })
  if (scope.scenarioId && scope.asOfUtc) { query.set('scenarioId', scope.scenarioId); query.set('asOf', scope.asOfUtc) }
  if (page) { query.set('limit', String(page.limit)); query.set('offset', String(page.offset)) }
  return query
}

export class HypothesisApiError extends Error {
  constructor(public status: number, message: string) { super(message) }
}
async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const response = await fetch(`/analysis-api/api/hypotheses${path}`, { ...options, credentials: 'same-origin', cache: 'no-store' })
  if (!response.ok) {
    let detail = 'The hypothesis request was not confirmed. Local text is retained; inspect the saved revision before retrying.'
    try {
      const problem = await response.json() as { title?: string; detail?: string }
      detail = [problem.title, problem.detail].filter(Boolean).join(' ') || detail
    } catch { /* Never render server HTML as a successful artifact. */ }
    throw new HypothesisApiError(response.status, `${response.status}: ${detail}`)
  }
  return response.json() as Promise<T>
}
export const listHypotheses = (scope: HypothesisScope, offset = 0, id?: string, signal?: AbortSignal) =>
  request<HypothesisSummary[]>(`${id ? `/${encodeURIComponent(id)}/revisions` : ''}?${hypothesisQuery(scope, { limit: 20, offset })}`, { signal })
export async function getHypothesis(selection: HypothesisSelection, signal?: AbortSignal) {
  const value = await request<HypothesisRevision>(`${revisionPath(selection.reference)}?${hypothesisQuery(selection.scope)}`, { signal })
  verifyHypothesis(value, selection)
  return value
}
export async function getLatestHypothesis(scope: HypothesisScope, id: string, signal?: AbortSignal) {
  const value = await request<HypothesisRevision>(`/${encodeURIComponent(id)}?${hypothesisQuery(scope)}`, { signal })
  verifyHypothesis(value, { scope, reference: { hypothesisId: id, revision: value.revision } })
  return value
}
export function verifyHypothesis(value: HypothesisRevision, selection: HypothesisSelection) {
  if (value.schemaVersion !== 'hypothesis-revision-v1' || hypothesisScopeKey(value.scope) !== hypothesisScopeKey(selection.scope) ||
      value.hypothesisId !== selection.reference.hypothesisId || value.revision !== selection.reference.revision ||
      !/^[a-f0-9]{64}$/.test(value.snapshotSha256) || value.package.fieldId !== value.scope.fieldId ||
      value.analysis.fieldId !== value.scope.fieldId || value.package.sha256 !== value.input.packageSha256 ||
      value.analysis.analysisSha256 !== value.input.analysisSha256 || value.analysis.packageSha256 !== value.package.sha256 ||
      !value.analysis.configuration || configurationKey(value.analysis.configuration) !== configurationKey(value.input.configuration)) {
    throw new Error('Saved snapshot identity or evidence/configuration fingerprints do not match. No live output has been replaced.')
  }
}
export const analyzeHypothesis = (selection: HypothesisSelection, configuration: AnalysisConfiguration, signal?: AbortSignal) =>
  request<AnalysisResult>(`${revisionPath(selection.reference)}/analysis`, {
    method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ scope: selection.scope, configuration }), signal,
  })
export const compareHypotheses = (revisions: HypothesisSelection[], signal?: AbortSignal) =>
  request<HypothesisComparison>('/compare', {
    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ revisions }), signal,
  })
export const listHypothesisChallenges = (selection: HypothesisSelection, offset = 0, signal?: AbortSignal) =>
  request<HypothesisChallenge[]>(`${revisionPath(selection.reference)}/challenges?${hypothesisQuery(selection.scope, { limit: 20, offset })}`, { signal })
export async function getHypothesisChallenge(selection: HypothesisSelection, id: string, version?: number, signal?: AbortSignal) {
  const query = hypothesisQuery(selection.scope)
  if (version !== undefined) query.set('version', String(version))
  const value = await request<HypothesisChallenge>(`${revisionPath(selection.reference)}/challenges/${encodeURIComponent(id)}?${query}`, { signal })
  if (value.challengeId !== id || value.hypothesis.hypothesisId !== selection.reference.hypothesisId ||
      value.hypothesis.revision !== selection.reference.revision || (version !== undefined && value.version !== version)) {
    throw new Error('Challenge identity/version mismatch. Local dispositions are retained.')
  }
  return value
}
export async function writeHypothesis(attempt: HypothesisAttempt): Promise<HypothesisRevision | HypothesisChallenge> {
  const session = await getOperatorSession()
  if (!session.enabled || !session.csrfRequestToken || session.csrfHeaderName !== 'X-DrillSim-CSRF') {
    throw new Error(session.reason || 'An enabled local operator session is required to save hypothesis artifacts.')
  }
  if (!/^[\x21-\x7e]{1,128}$/.test(attempt.key)) throw new Error('A stable action key is required.')
  const value = await request<HypothesisRevision | HypothesisChallenge>(attempt.path, {
    method: attempt.method,
    headers: {
      'Content-Type': 'application/json', [session.csrfHeaderName]: session.csrfRequestToken, 'Idempotency-Key': attempt.key,
      ...(attempt.expectedVersion !== undefined ? { 'If-Match': `"${attempt.expectedVersion}"` } : {}),
    },
    body: JSON.stringify(attempt.body),
  })
  if (attempt.kind === 'revision') {
    if (!('snapshotSha256' in value)) throw new Error('The server did not return a saved hypothesis revision.')
    const body = attempt.body as { scope: HypothesisScope; input: HypothesisInput }
    verifyHypothesis(value, { scope: body.scope, reference: referenceOf(value) })
    if (value.input.analysisSha256 !== body.input.analysisSha256 || value.input.selectedCandidateId !== body.input.selectedCandidateId) {
      throw new Error('The saved hypothesis did not confirm this analysis and target.')
    }
  } else {
    const match = attempt.path.match(/^\/([^/]+)\/revisions\/(\d+)\/challenges(?:\/([^/]+)\/dispositions)?$/)
    if (!('challengeId' in value) || !value.sha256 || !match ||
        value.hypothesis.hypothesisId !== match[1] || value.hypothesis.revision !== Number(match[2]) ||
        (match[3] && (value.challengeId !== match[3] || value.version !== (attempt.expectedVersion ?? 0) + 1))) {
      throw new Error('The server did not confirm the expected challenge identity/version.')
    }
    const input = attempt.body as { analysisSha256?: string }
    if (input.analysisSha256 && value.analysisSha256 !== input.analysisSha256) throw new Error('Saved challenge belongs to a different analysis fingerprint.')
  }
  return value
}

export function eligibleCandidates(analysis: AnalysisResult): RankedCandidate[] {
  return (analysis.candidateGrid ?? []).flatMap((cell) => cell.status === 'eligible' && cell.prediction ? [cell.prediction] : [])
}
export function branchPreset(source: HypothesisRevision, preset: 'spatial' | 'conservative' | 'custom') {
  const config = { ...source.input.configuration }
  if (preset === 'conservative') {
    config.porosityCutoff = Math.min(1, config.porosityCutoff + .02)
    config.permeabilityCutoffM2 = Math.min(1e-8, config.permeabilityCutoffM2 * 2)
  }
  return config
}
export function branchTarget(source: HypothesisRevision, analysis: AnalysisResult, preset: string) {
  const candidates = eligibleCandidates(analysis)
  if (preset !== 'spatial') return analysis.ranking[0] ?? candidates[0]
  const original = eligibleCandidates(source.analysis).find((candidate) => candidate.candidateId === source.input.selectedCandidateId)
  if (!original) throw new Error('The saved source target is absent from its eligible grid.')
  return candidates.filter((candidate) => Math.hypot(candidate.eastingM - original.eastingM, candidate.northingM - original.northingM) > 1)
    .sort((a, b) => Math.hypot(b.eastingM - original.eastingM, b.northingM - original.northingM) -
      Math.hypot(a.eastingM - original.eastingM, a.northingM - original.northingM))[0]
}
export function hypothesisInput(pkg: FieldPackage, analysis: AnalysisResult, selectedCandidateId: string, rationale: string,
  correlationNotes: string, controlNotes: HypothesisControlNote[]): HypothesisInput {
  if (!analysis.configuration || configurationErrors(analysis.configuration).length || !analysis.analysisSha256 ||
      analysis.packageSha256 !== pkg.sha256 || !eligibleCandidates(analysis).some((candidate) => candidate.candidateId === selectedCandidateId)) {
    throw new Error('A matching fingerprinted analysis and eligible target are required.')
  }
  if (!rationale.trim() || rationale.length > 10000) throw new Error('Enter a rationale of 1–10000 characters.')
  if (correlationNotes.length > 4000 || controlNotes.length > 32) throw new Error('Correlation notes allow 4000 characters and at most 32 control notes.')
  const visible = visibleEvidenceIds(pkg)
  if (controlNotes.some((note) => !visible.has(note.evidenceId) || !note.note.trim() || note.note.length > 2000)) {
    throw new Error('Control notes require a visible evidence ID and 1–2000 characters. Remove empty notes before saving.')
  }
  return {
    configuration: analysis.configuration, packageSha256: pkg.sha256, analysisSha256: analysis.analysisSha256,
    selectedCandidateId, rationale: rationale.trim(),
    ...(correlationNotes.trim() ? { correlationNotes: correlationNotes.trim() } : {}),
    ...(controlNotes.length ? { controlNotes } : {}),
  }
}

export function validateObjections(pkg: FieldPackage, objections: Array<{ text: string; citedEvidenceIds: string[] }>) {
  const ids = visibleEvidenceIds(pkg)
  if (objections.length < 1 || objections.length > 32) throw new Error('A challenge requires 1–32 objections.')
  for (const objection of objections) {
    if (!objection.text.trim() || objection.text.length > 4000 || !objection.citedEvidenceIds.length ||
        objection.citedEvidenceIds.length > 32 || new Set(objection.citedEvidenceIds).size !== objection.citedEvidenceIds.length ||
        objection.citedEvidenceIds.some((id) => !ids.has(id))) throw new Error('Each objection requires text and 1–32 unique citations from this exact saved package.')
  }
}
