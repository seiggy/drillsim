import { useRef, useState } from 'react'
import { HttpAgent, type AgentSubscriber, type Message } from '@ag-ui/client'
import type { AgentRunState } from './types'

export function useFieldAgent() {
  const [state, setState] = useState<AgentRunState>({ running: false, output: '', error: '' })
  const activeAgent = useRef<HttpAgent | undefined>(undefined)
  const generation = useRef(0)

  async function run(
    action: string,
    fieldId: string,
    packageSha256: string,
    context: Record<string, unknown>,
    scenario?: { scenarioId: string; asOfUtc: string },
  ) {
    activeAgent.current?.abortRun()
    const runGeneration = ++generation.current
    const agent = new HttpAgent({
      url: scenario ? '/analysis-api/agui/scenario' : '/analysis-api/agui',
      threadId: crypto.randomUUID(),
      headers: scenario ? { 'X-DrillSim-Scenario-Id': scenario.scenarioId } : undefined,
    })
    activeAgent.current = agent
    const message: Message = {
      id: crypto.randomUUID(),
      role: 'user',
      content: `${action}. Analyze reservoir ${String(context.selectedReservoir ?? 'unspecified')} in field ${fieldId}, using every intersecting wellbore in the package and distinguishing logged controls from interval-only evidence. Return an evidence-cited field note, not conversational filler.`,
    }
    agent.setMessages([message])
    setState({ running: true, output: '', error: '' })
    const subscriber: AgentSubscriber = {
      onTextMessageContentEvent: ({ textMessageBuffer }) => {
        if (generation.current === runGeneration) {
          setState({ running: true, output: textMessageBuffer, error: '' })
        }
      },
    }
    try {
      const result = await agent.runAgent({
        context: [
          { description: 'Selected field package', value: JSON.stringify({ fieldId, packageSha256 }) },
          { description: 'Active interpretation controls', value: JSON.stringify(context) },
          ...(scenario ? [{
            description: 'Blind scenario clock',
            value: JSON.stringify({ scenarioId: scenario.scenarioId, asOfUtc: scenario.asOfUtc }),
          }] : []),
        ],
      }, subscriber)
      const response = [...result.newMessages].reverse().find((item) => item.role === 'assistant')
      if (generation.current === runGeneration) {
        activeAgent.current = undefined
        setState({
          running: false,
          output: response?.role === 'assistant' && typeof response.content === 'string' ? response.content : '',
          error: '',
        })
      }
    } catch (error) {
      if (generation.current === runGeneration) {
        activeAgent.current = undefined
        setState({ running: false, output: '', error: error instanceof Error ? error.message : 'Agent run failed.' })
      }
    }
  }

  function clear() {
    generation.current++
    activeAgent.current?.abortRun()
    activeAgent.current = undefined
    setState({ running: false, output: '', error: '' })
  }

  return { state, run, clear }
}
