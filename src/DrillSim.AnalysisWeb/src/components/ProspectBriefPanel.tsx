import { useEffect, useRef, useState } from 'react'
import type { WellView } from './AnalysisWorkspace'
import { evidenceLabels } from '../agentApi'
import { draftProspectBrief, getProspectBriefStatus, recommendationLabel, type ProspectBrief } from '../prospects'
import type { AnalysisConfiguration, AnalysisResult, FieldPackage, RankedCandidate } from '../types'

export function ProspectBriefPanel({ pkg, analysis, reservoir, configuration, candidate, wells }: {
  pkg: FieldPackage
  analysis: AnalysisResult
  reservoir: string
  configuration: AnalysisConfiguration
  candidate: RankedCandidate
  wells: WellView[]
}) {
  const [status, setStatus] = useState<{ configured: boolean; reason: string | null }>()
  const [running, setRunning] = useState(false)
  const [elapsed, setElapsed] = useState(0)
  const [error, setError] = useState('')
  const [result, setResult] = useState<ProspectBrief>()
  const request = useRef<AbortController | undefined>(undefined)

  useEffect(() => {
    const controller = new AbortController()
    void getProspectBriefStatus(controller.signal).then(value => { if (!controller.signal.aborted) setStatus(value) })
    return () => { controller.abort(); request.current?.abort() }
  }, [])
  useEffect(() => {
    if (!running) return
    const started = Date.now()
    const timer = window.setInterval(() => setElapsed(Math.round((Date.now() - started) / 1000)), 1000)
    return () => window.clearInterval(timer)
  }, [running])

  async function ask() {
    const controller = new AbortController()
    request.current = controller
    setRunning(true); setElapsed(0); setError(''); setResult(undefined)
    try { setResult(await draftProspectBrief(pkg, analysis, reservoir, configuration, candidate, controller.signal)) }
    catch (reason) { if (!controller.signal.aborted) setError(reason instanceof Error ? reason.message : String(reason)) }
    finally { if (!controller.signal.aborted) setRunning(false) }
  }

  const labels = evidenceLabels(pkg)
  const source = (id: string) => {
    const well = wells.find(item => `well:${item.wellId}` === id || `wellbore:${item.wellBoreId}` === id)
    return well?.name ?? labels.get(id) ?? 'field record'
  }
  const sources = (ids: string[]) => [...new Set(ids.map(source))].join(', ')

  return <section className="ai-panel" aria-labelledby="brief-title" aria-busy={running}>
    <header>
      <h2 id="brief-title">AI second opinion</h2>
      <p>The AI reads the same wells and logs, then explains why this target scores well and what could make it wrong. It can't change the numbers.</p>
    </header>
    {!result && !running && <button type="button" className="primary" disabled={!status?.configured} onClick={() => void ask()}>
      Explain target #{candidate.rank} and its risks</button>}
    {status && !status.configured && <p className="empty-note">AI is not connected ({status.reason ?? 'no model configured'}). The numbers on the left still work without it.</p>}
    {running && <div className="ai-progress" role="status">
      <span className="spinner" aria-hidden="true" />
      <div><strong>Reading the wells and logs… {elapsed}s</strong><small>This usually takes 30–90 seconds.</small></div>
      <button type="button" onClick={() => { request.current?.abort(); setRunning(false) }}>Cancel</button>
    </div>}
    {error && <div className="ai-error" role="alert"><strong>The AI could not answer.</strong><p>{error}</p>
      <button type="button" onClick={() => void ask()}>Try again</button></div>}
    {result && <div className="ai-result" aria-live="polite">
      <p className={`verdict-chip ${result.brief.recommendation}`}>{recommendationLabel[result.brief.recommendation]}</p>
      <p className="ai-headline">{result.brief.headline}</p>
      <h3>Why it looks good</h3>
      <ul>{result.brief.reasons.map((item, index) => <li key={index}>{item.text}<small>Based on {sources(item.citedEvidenceIds)}</small></li>)}</ul>
      <h3>What could make it wrong</h3>
      <ul>{result.brief.risks.map((item, index) => <li key={index}><span className={`severity ${item.severity}`}>{item.severity}</span>{item.text}
        <small>Based on {sources(item.citedEvidenceIds)}</small></li>)}</ul>
      {result.brief.nextChecks.length > 0 && <><h3>Check before drilling</h3><ul>{result.brief.nextChecks.map((item, index) => <li key={index}>{item}</li>)}</ul></>}
      <button type="button" onClick={() => void ask()}>Ask again</button>
    </div>}
  </section>
}
