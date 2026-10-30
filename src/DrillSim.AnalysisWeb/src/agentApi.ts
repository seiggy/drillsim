import { idOf, nameOf } from './data'
import type { FieldPackage } from './types'

export interface AgentStatus { configured: boolean; reason: string | null }

/** Fetches an AI endpoint and turns problem responses into readable errors without echoing raw server output. */
export async function agentRequest<T>(url: string, options?: RequestInit): Promise<T> {
  const response = await fetch(url, { ...options, credentials: 'same-origin', cache: 'no-store' })
  if (!response.ok) {
    let message = 'The AI could not answer. Nothing was changed. Try again when the service is available.'
    try {
      const problem = await response.json() as { detail?: unknown; title?: unknown }
      if (typeof problem.detail === 'string') message = problem.detail
      else if (typeof problem.title === 'string') message = problem.title
    } catch { /* Do not display raw server responses. */ }
    throw new Error(response.status === 429 ? `The AI service is at capacity. ${message}` : message)
  }
  return response.json() as Promise<T>
}

export function evidenceLabels(pkg: FieldPackage) {
  return new Map<string, string>([
    [`field:${pkg.fieldId}`, nameOf(pkg.field)],
    ...([['well', pkg.wells], ['wellbore', pkg.wellBores], ['geology', pkg.geologicalProperties],
      ['trajectory', pkg.trajectories], ['cluster', pkg.clusters], ['architecture', pkg.wellBoreArchitectures]] as const)
      .flatMap(([kind, records]) => records.map(record => [`${kind}:${idOf(record)}`, nameOf(record)] as const)),
  ])
}

/** Replaces inline evidence IDs (kind:uuid) with record names; the IDs stay in the citation list. */
export const readableEvidence = (text: string, labels: Map<string, string>) =>
  text.replace(/\b(?:field|cluster|well|wellbore|architecture|trajectory|geology):[0-9a-f]{8}-[0-9a-f-]{27}\b/gi, id => labels.get(id) ?? 'a field record')
