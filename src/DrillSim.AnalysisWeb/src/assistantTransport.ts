import { HttpAgent } from '@ag-ui/client'
import type { ChatTransport, UIMessage, UIMessageChunk } from 'ai'
import { assistantContext } from './assistantContext'

export const assistantUrl = '/analysis-api/agui/assistant'

type AguiEvent = { type: string; [key: string]: unknown }

function textOf(message: UIMessage) {
  return message.parts.flatMap(part => part.type === 'text' ? [part.text] : []).join('\n')
}

/**
 * AI SDK ChatTransport that talks AG-UI: each send posts the conversation plus the current page context items to the
 * assistant endpoint and maps AG-UI events (text messages, tool calls, errors) onto UI message chunks.
 */
export class AguiChatTransport implements ChatTransport<UIMessage> {
  constructor(private readonly url = assistantUrl) {}

  async sendMessages({ chatId, messages, abortSignal }: Parameters<ChatTransport<UIMessage>['sendMessages']>[0]): Promise<ReadableStream<UIMessageChunk>> {
    const agent = new HttpAgent({ url: this.url })
    const input = {
      threadId: chatId,
      runId: crypto.randomUUID(),
      state: {},
      tools: [],
      forwardedProps: {},
      context: assistantContext(),
      messages: messages.map(message => ({ id: message.id, role: message.role === 'assistant' ? 'assistant' as const : 'user' as const, content: textOf(message) }))
        .filter(message => message.content.trim()),
    }
    return new ReadableStream<UIMessageChunk>({
      start: controller => {
        const openText = new Set<string>()
        const toolArgs = new Map<string, { name: string; args: string }>()
        let started = false, closed = false
        const emit = (chunk: UIMessageChunk) => { if (!closed) controller.enqueue(chunk) }
        const begin = () => { if (!started) { started = true; emit({ type: 'start' }) } }
        const finish = (error?: string) => {
          if (closed) return
          for (const id of openText) emit({ type: 'text-end', id })
          openText.clear()
          if (error) emit({ type: 'error', errorText: error })
          else emit({ type: 'finish' })
          closed = true
          controller.close()
        }
        const subscription = agent.run(input as Parameters<HttpAgent['run']>[0]).subscribe({
          next: (raw: unknown) => {
            const event = raw as AguiEvent
            switch (event.type) {
              case 'RUN_STARTED': begin(); break
              case 'TEXT_MESSAGE_START': begin(); openText.add(String(event.messageId)); emit({ type: 'text-start', id: String(event.messageId) }); break
              case 'TEXT_MESSAGE_CONTENT': case 'TEXT_MESSAGE_CHUNK': {
                const id = String(event.messageId)
                begin()
                if (!openText.has(id)) { openText.add(id); emit({ type: 'text-start', id }) }
                if (event.delta) emit({ type: 'text-delta', id, delta: String(event.delta) })
                break
              }
              case 'TEXT_MESSAGE_END': { const id = String(event.messageId); if (openText.delete(id)) emit({ type: 'text-end', id }); break }
              case 'TOOL_CALL_START': begin(); toolArgs.set(String(event.toolCallId), { name: String(event.toolCallName), args: '' }); break
              case 'TOOL_CALL_ARGS': { const call = toolArgs.get(String(event.toolCallId)); if (call) call.args += String(event.delta ?? ''); break }
              case 'TOOL_CALL_END': {
                const call = toolArgs.get(String(event.toolCallId))
                if (!call) break
                let parsed: unknown = call.args
                try { parsed = call.args ? JSON.parse(call.args) : {} } catch { /* keep raw text */ }
                emit({ type: 'tool-input-available', toolCallId: String(event.toolCallId), toolName: call.name, input: parsed, dynamic: true, providerExecuted: true })
                break
              }
              case 'TOOL_CALL_RESULT':
                emit({ type: 'tool-output-available', toolCallId: String(event.toolCallId), output: event.content, dynamic: true, providerExecuted: true })
                break
              case 'RUN_ERROR': finish(String(event.message ?? 'The assistant run failed.')); break
              case 'RUN_FINISHED': finish(); break
            }
          },
          error: (error: unknown) => finish(error instanceof Error ? error.message : String(error)),
          complete: () => finish(),
        })
        abortSignal?.addEventListener('abort', () => { subscription.unsubscribe(); agent.abortRun?.(); finish() }, { once: true })
      },
    })
  }

  async reconnectToStream() { return null }
}
