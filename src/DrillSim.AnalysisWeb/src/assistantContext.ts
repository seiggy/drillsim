import { useEffect, useSyncExternalStore } from 'react'

/** One AG-UI context item: a plain-language description and a JSON value of what the page is showing. */
export interface AssistantContextItem { description: string; value: string }

const entries = new Map<string, AssistantContextItem>()
const listeners = new Set<() => void>()
let snapshot: AssistantContextItem[] = []

function publish() {
  snapshot = [...entries.values()]
  listeners.forEach(listener => listener())
}

/** Exposes a compact, page-visible summary to the assistant while the calling component is mounted. */
export function useAssistantContext(id: string, description: string, value: unknown) {
  const json = value === undefined ? undefined : JSON.stringify(value)
  useEffect(() => {
    if (json === undefined) return
    entries.set(id, { description, value: json })
    publish()
    return () => { entries.delete(id); publish() }
  }, [id, description, json])
}

export const assistantContext = () => snapshot

/** Render-anywhere form of useAssistantContext, for screens that compute their summary after early returns. */
export function AssistantContext({ id, description, value }: { id: string; description: string; value: unknown }) {
  useAssistantContext(id, description, value)
  return null
}

export function useAssistantContextSnapshot() {
  return useSyncExternalStore(listener => { listeners.add(listener); return () => listeners.delete(listener) }, () => snapshot)
}
