import { configurationErrors, configurationKey } from './analysisConfiguration'
import { idOf, nameOf } from './data'
import { visibleEvidenceIds } from './evidence'
import { hypothesisScopeKey } from './hypotheses'
import type { HypothesisControlNote, HypothesisReference, HypothesisScope } from './hypotheses'
import { getOperatorSession } from './operator'
import type { AnalysisConfiguration, FieldPackage } from './types'

export interface FormationDraftNotes {
  name: string
  rationale: string
  correlationNotes: string
  controlNotes: HypothesisControlNote[]
}
export interface FormationDraftRequest {
  scope: HypothesisScope
  configuration: AnalysisConfiguration
  packageSha256: string
  analysisSha256: string
  selectedCandidateId: string | null
  savedHypothesis: HypothesisReference | null
  snapshotSha256: string | null
  notes: FormationDraftNotes
}
export interface FormationDraftResult {
  version: 'formation-interpretation-v1'
  promptVersion: 'formation-interpretation-prompt-v2'
  scope: HypothesisScope
  packageSha256: string
  analysisSha256: string
  configurationSha256: string
  selectedCandidateId: string | null
  savedHypothesis: HypothesisReference | null
  snapshotSha256: string | null
  generatedAt: string
  draft: {
    name: string
    rationale: string
    correlationNotes: string
    citedEvidenceIds: string[]
    limitations: string[]
  }
}
export interface FormationAgentStatus { configured: boolean; reason: string | null }

export function formationContextKey(request: FormationDraftRequest) {
  return JSON.stringify([hypothesisScopeKey(request.scope), configurationKey(request.configuration),
    request.packageSha256, request.analysisSha256, request.selectedCandidateId,
    request.savedHypothesis?.hypothesisId ?? null, request.savedHypothesis?.revision ?? null, request.snapshotSha256])
}

export function formationNotesKey(notes: FormationDraftNotes) {
  return JSON.stringify([notes.name, notes.rationale, notes.correlationNotes,
    notes.controlNotes.map(note => [note.evidenceId, note.note])])
}

export function formationDraftBlock(request: FormationDraftRequest, pkg: FieldPackage) {
  if (configurationErrors(request.configuration).length) return 'Apply valid analysis settings before drafting an interpretation.'
  if (request.packageSha256 !== pkg.sha256 || request.scope.fieldId !== pkg.fieldId ||
      !/^[a-f0-9]{64}$/.test(request.packageSha256) || !/^[a-f0-9]{64}$/.test(request.analysisSha256)) {
    return 'The displayed evidence and analysis do not match. Refresh the field before drafting.'
  }
  if (Boolean(request.savedHypothesis) !== Boolean(request.snapshotSha256)) return 'Load the saved revision again before drafting.'
  if (request.notes.name.length > 120 || request.notes.rationale.length > 10000 || request.notes.correlationNotes.length > 4000 ||
      request.notes.controlNotes.length > 32 || request.notes.controlNotes.some(note => note.note.length > 2000 ||
        !visibleEvidenceIds(pkg).has(note.evidenceId))) return 'Check the length and evidence references of the current notes before drafting.'
  return ''
}

export function formationNotesText(result: FormationDraftResult) {
  return ['AI-assisted interpretation', result.draft.correlationNotes,
    ...(result.draft.limitations.length ? ['Checks before using this interpretation:', ...result.draft.limitations.map(item => `- ${item}`)] : []),
    'Evidence references:', ...result.draft.citedEvidenceIds].join('\n\n')
}

export function verifyFormationDraft(value: FormationDraftResult, request: FormationDraftRequest,
  pkg: FieldPackage, configurationSha256: string) {
  if (!value || value.version !== 'formation-interpretation-v1' || value.promptVersion !== 'formation-interpretation-prompt-v2' ||
      hypothesisScopeKey(value.scope) !== hypothesisScopeKey(request.scope) ||
      value.packageSha256 !== request.packageSha256 || value.analysisSha256 !== request.analysisSha256 ||
      value.configurationSha256 !== configurationSha256 || value.selectedCandidateId !== request.selectedCandidateId ||
      (value.savedHypothesis?.hypothesisId ?? null) !== (request.savedHypothesis?.hypothesisId ?? null) ||
      (value.savedHypothesis?.revision ?? null) !== (request.savedHypothesis?.revision ?? null) ||
      (value.snapshotSha256 ?? null) !== request.snapshotSha256 || !Number.isFinite(Date.parse(value.generatedAt))) {
    throw new Error('The AI draft does not match the displayed evidence and analysis. No notes were changed.')
  }
  const draft = value.draft
  const validText = (text: unknown, max: number) => typeof text === 'string' && Boolean(text.trim()) && text.length <= max
  if (!draft || !validText(draft.name, 120) || !validText(draft.rationale, 10000) ||
      !validText(draft.correlationNotes, 2000) || !Array.isArray(draft.citedEvidenceIds) ||
      draft.citedEvidenceIds.length < 1 || draft.citedEvidenceIds.length > 16 ||
      new Set(draft.citedEvidenceIds).size !== draft.citedEvidenceIds.length ||
      draft.citedEvidenceIds.some(id => typeof id !== 'string' || !visibleEvidenceIds(pkg).has(id)) ||
      !Array.isArray(draft.limitations) || draft.limitations.length < 1 || draft.limitations.length > 4 ||
      draft.limitations.some(item => !validText(item, 180)) || formationNotesText(value).length > 4000) {
    throw new Error('The AI draft is incomplete or has unsupported evidence references. No notes were changed.')
  }
}

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`/analysis-api/api/formation-interpretation${path}`, {
    ...options, credentials: 'same-origin', cache: 'no-store',
  })
  if (!response.ok) {
    let message = 'The interpretation could not be drafted. Your notes are unchanged. Try again when the service is available.'
    try {
      const problem = await response.json() as { detail?: unknown; title?: unknown }
      if (typeof problem.detail === 'string') message = problem.detail
      else if (typeof problem.title === 'string') message = problem.title
    } catch { /* Do not display raw server responses as interpretation text. */ }
    throw new Error(`${response.status}: ${message}`)
  }
  return response.json() as Promise<T>
}

export async function getFormationAgentStatus(signal?: AbortSignal) {
  const status = await request<FormationAgentStatus>('/status', { signal })
  if (typeof status.configured !== 'boolean' || status.reason !== null && typeof status.reason !== 'string') {
    throw new Error('AI availability could not be verified. Refresh AI status before trying again.')
  }
  return status
}

export async function draftFormationInterpretation(body: FormationDraftRequest, pkg: FieldPackage,
  configurationSha256: string, signal: AbortSignal) {
  const block = formationDraftBlock(body, pkg)
  if (block) throw new Error(block)
  formationContextKey(body)
  const session = await getOperatorSession(signal)
  if (!session.enabled || !session.csrfRequestToken || session.csrfHeaderName !== 'X-DrillSim-CSRF') {
    throw new Error(session.reason || 'A local operator session is required to request an AI draft.')
  }
  signal.throwIfAborted()
  const value = await request<FormationDraftResult>('', {
    method: 'POST', signal,
    headers: { 'Content-Type': 'application/json', [session.csrfHeaderName]: session.csrfRequestToken,
      'Idempotency-Key': crypto.randomUUID() },
    body: JSON.stringify(body),
  })
  signal.throwIfAborted()
  verifyFormationDraft(value, body, pkg, configurationSha256)
  return value
}

export function formationEvidenceLabels(pkg: FieldPackage) {
  return new Map<string, string>([
    [`field:${pkg.fieldId}`, `Field · ${nameOf(pkg.field)}`],
    ...([['well', pkg.wells], ['wellbore', pkg.wellBores], ['geology', pkg.geologicalProperties],
      ['trajectory', pkg.trajectories], ['cluster', pkg.clusters], ['architecture', pkg.wellBoreArchitectures]] as const)
      .flatMap(([kind, records]) => records.map(record => [`${kind}:${idOf(record)}`, `${kind} · ${nameOf(record)}`] as const)),
  ])
}
