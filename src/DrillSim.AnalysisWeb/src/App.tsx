import { useEffect, useMemo, useRef, useState } from 'react'
import { applyAnalysis, getFieldPackage, getFields, getMapsStatus, getScenarios } from './api'
import { getOperatorView } from './operator'
import { reservoirOptionsOf } from './data'
import { verifyAppliedAnalysis, withTargetFluid } from './analysisConfiguration'
import { getSimulatorCoverage, type SimulatorCoverage } from './prospects'
import { getReservoirModel, type StaticReservoirModel } from './reservoirModel'
import { useAssistantContext } from './assistantContext'
import { AssistantPanel } from './components/AssistantPanel'
import { isAutomatedRun, loadSettings, saveSettings, type AppSettings } from './settings'
import { runStage, runStageLabel, type Screen } from './workflow'
import type { AnalysisResult, FieldPackage, FieldSummary, MapsStatus, RankedCandidate, Scenario } from './types'
import { FieldScreen } from './screens/FieldScreen'
import { ProspectsScreen, shortlist } from './screens/ProspectsScreen'
import { ForecastScreen } from './screens/ForecastScreen'
import { DrillScreen } from './screens/DrillScreen'
import { ResultsScreen } from './screens/ResultsScreen'
import { SettingsScreen } from './screens/SettingsScreen'
import { GuidedTour } from './components/GuidedTour'
import demoGuideUrl from '../../../docs/index.html?url'
import './app.css'

const steps: Array<{ id: Exclude<Screen, 'settings'>; label: string; question: string }> = [
  { id: 'field', label: 'Field', question: 'What do we know?' },
  { id: 'prospects', label: 'Prospects', question: 'Where should we drill?' },
  { id: 'forecast', label: 'Forecast', question: 'What will we find?' },
  { id: 'drill', label: 'Drill', question: 'Drill it in the simulator' },
  { id: 'results', label: 'Results', question: 'How did we do?' },
]

function preferredField(items: FieldSummary[], scenarios: Scenario[]) {
  const originals = items.filter(field => !scenarios.some(scenario => scenario.clonedFieldId === field.id))
  return (originals.find(field => field.name.includes('TROLL FORCE-SODIR')) ?? originals[0] ?? items[0])?.id ?? ''
}

export default function App() {
  const [settings, setSettings] = useState<AppSettings>(loadSettings)
  const [screen, setScreen] = useState<Screen>('field')
  const [fields, setFields] = useState<FieldSummary[]>([])
  const [scenarios, setScenarios] = useState<Scenario[]>([])
  const [mapsStatus, setMapsStatus] = useState<MapsStatus>()
  const [fieldId, setFieldId] = useState('')
  const [reservoirs, setReservoirs] = useState<Array<{ name: string; controls: number }>>([])
  const [reservoir, setReservoir] = useState('')
  const [pkg, setPkg] = useState<FieldPackage>()
  const [analysis, setAnalysis] = useState<AnalysisResult>()
  const [coverage, setCoverage] = useState<SimulatorCoverage>()
  const [reservoirModel, setReservoirModel] = useState<{ model?: StaticReservoirModel; loading?: boolean; error?: string }>({})
  const [candidate, setCandidate] = useState<RankedCandidate>()
  const [runId, setRunId] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [tourOpen, setTourOpen] = useState(false)
  const [assistantOpen, setAssistantOpen] = useState(false)
  const [runStates, setRunStates] = useState<Record<string, string>>({})
  const analysisRequest = useRef<AbortController | undefined>(undefined)

  const fieldScenarios = useMemo(() => scenarios
    .filter(item => item.sourceFieldId === fieldId && (!settings.hideAutomatedRuns || !isAutomatedRun(item.seedLabel)))
    .sort((a, b) => b.createdUtc.localeCompare(a.createdUtc)), [scenarios, fieldId, settings.hideAutomatedRuns])
  const run = scenarios.find(item => item.scenarioId === runId)
  const originalFields = fields.filter(field => !scenarios.some(scenario => scenario.clonedFieldId === field.id))

  useEffect(() => {
    const controller = new AbortController()
    Promise.all([getFields(controller.signal), getScenarios(controller.signal), getMapsStatus(controller.signal)])
      .then(([items, runs, maps]) => {
        setFields(items); setScenarios(runs); setMapsStatus(maps); setFieldId(preferredField(items, runs))
      })
      .catch(reason => { if (!controller.signal.aborted) { setError(String(reason)); setLoading(false) } })
    return () => controller.abort()
  }, [])

  useEffect(() => {
    const controller = new AbortController()
    const active = fieldScenarios.filter(item => ['HumanApproved', 'WorldBound', 'Queued', 'Drilling', 'Surveying', 'Logging',
      'CompletionDesigned', 'Producing', 'ReadyToReveal'].includes(item.status) && !(item.scenarioId in runStates))
    Promise.all(active.map(item => getOperatorView(item.scenarioId, controller.signal)
      .then(view => [item.scenarioId, view.run?.status ?? ''] as const).catch(() => [item.scenarioId, ''] as const)))
      .then(entries => { if (!controller.signal.aborted && entries.length) setRunStates(current => ({ ...current, ...Object.fromEntries(entries) })) })
    return () => controller.abort()
  }, [fieldScenarios])

  useEffect(() => {
    if (!fieldId) return
    const controller = new AbortController()
    setLoading(true); setError(''); setPkg(undefined); setAnalysis(undefined); setCandidate(undefined); setCoverage(undefined)
    getFieldPackage(fieldId, undefined, controller.signal).then(next => {
      if (controller.signal.aborted) return
      const options = reservoirOptionsOf(next.geologicalProperties)
      setPkg(next); setReservoirs(options)
      setReservoir(current => options.some(item => item.name === current) ? current
        : (options.find(item => /sognefjord/i.test(item.name)) ?? options[0])?.name ?? '')
    }).catch(reason => { if (!controller.signal.aborted) { setError(String(reason)); setLoading(false) } })
    return () => controller.abort()
  }, [fieldId])

  useEffect(() => {
    if (!pkg || !reservoir) return
    analysisRequest.current?.abort()
    const controller = new AbortController()
    analysisRequest.current = controller
    setLoading(true); setError('')
    Promise.all([
      applyAnalysis(pkg.fieldId, reservoir, settings.analysis, undefined, controller.signal),
      getSimulatorCoverage(pkg.fieldId, reservoir, undefined, controller.signal).catch(() => undefined),
    ]).then(([next, nextCoverage]) => {
      if (controller.signal.aborted) return
      verifyAppliedAnalysis(next, pkg, reservoir, settings.analysis)
      setAnalysis(next); setCoverage(nextCoverage)
      const { targets } = shortlist(next, nextCoverage)
      setCandidate(current => targets.find(item => item.candidateId === current?.candidateId) ?? targets[0])
      setReservoirModel({ loading: true })
      getReservoirModel(pkg.fieldId, reservoir, settings.analysis,
        targets.map(item => ({ id: item.candidateId, eastingM: item.eastingM, northingM: item.northingM })), undefined, controller.signal)
        .then(model => { if (!controller.signal.aborted) setReservoirModel({ model }) })
        .catch(reason => { if (!controller.signal.aborted) setReservoirModel({ error: reason instanceof Error ? reason.message : String(reason) }) })
    }).catch(reason => { if (!controller.signal.aborted) setError(String(reason)) })
      .finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [pkg, reservoir, settings.analysis])

  function updateSettings(next: AppSettings) { setSettings(next); saveSettings(next) }

  function openRun(id: string) {
    setRunId(id)
    const next = scenarios.find(item => item.scenarioId === id)
    if (!next) return
    if (next.sourceFieldId !== fieldId) setFieldId(next.sourceFieldId)
    if (next.reservoirName !== reservoir) setReservoir(next.reservoirName)
    setScreen(runStage(next))
  }

  function runChanged(next: Scenario, runStatus?: string) {
    if (runStatus !== undefined) setRunStates(current => ({ ...current, [next.scenarioId]: runStatus }))
    setScenarios(current => [...current.filter(item => item.scenarioId !== next.scenarioId), next])
    setRunId(next.scenarioId)
  }

  const needsRun = (id: Screen) => ['forecast', 'drill', 'results'].includes(id) && !run
  const content = !pkg || !analysis
    ? <section className="screen-loading" role="status">{error ? <><strong>The field could not be loaded.</strong><span>{error}</span></> : <strong>Loading the field…</strong>}</section>
    : screen === 'field' ? <FieldScreen pkg={pkg} analysis={analysis} reservoir={reservoir} mapsStatus={mapsStatus} coverage={coverage} reservoirModel={reservoirModel} onNext={() => setScreen('prospects')} />
    : screen === 'prospects' ? <ProspectsScreen pkg={pkg} analysis={analysis} reservoir={reservoir} mapsStatus={mapsStatus}
        coverage={coverage} candidate={candidate} onCandidate={setCandidate} settings={settings} reservoirModel={reservoirModel}
        onTargetFluid={fluid => updateSettings({ ...settings, analysis: withTargetFluid(settings.analysis, fluid) })}
        onForecast={() => { setRunId(''); setScreen('forecast') }} />
    : screen === 'forecast' ? <ForecastScreen key={run?.scenarioId ?? 'new'} run={run} pkg={pkg} reservoir={reservoir}
        candidate={candidate} coverage={coverage} settings={settings} onRunChanged={runChanged}
        onProspects={() => setScreen('prospects')} onDrill={() => setScreen('drill')} />
    : screen === 'drill' && run ? <DrillScreen key={run.scenarioId} run={run} settings={settings} onRunChanged={runChanged}
        onResults={() => setScreen('results')} onProspects={() => setScreen('prospects')} />
    : screen === 'results' && run ? <ResultsScreen key={run.scenarioId} run={run} onProspects={() => setScreen('prospects')} />
    : screen === 'settings' ? <SettingsScreen settings={settings} analysis={analysis} onChange={updateSettings} />
    : <section className="screen-empty"><h2>Choose a target first</h2><p>Forecasts, drilling and results belong to a run. Pick a target on Prospects, or open an earlier run from the Runs menu.</p>
        <button type="button" className="primary" onClick={() => setScreen('prospects')}>Go to Prospects</button></section>

  const fieldName = String(pkg?.field.Name ?? pkg?.field.name ?? '')
  useAssistantContext('app', 'Your selections: where the user is in DrillSim and the active field, reservoir, run and target', {
    screen, field: fieldName || undefined, fieldId: pkg?.fieldId, reservoir, targetHydrocarbon: settings.analysis.targetFluid ?? 'Any',
    rockCutoffs: { minimumPorosity: settings.analysis.porosityCutoff, minimumPermeabilityMd: settings.analysis.permeabilityCutoffM2 / 9.869233e-16 },
    run: run ? { label: run.seedLabel, stage: runStageLabel(run, runStates[run.scenarioId]) } : null,
    selectedTarget: candidate ? { rank: candidate.rank, eastingM: Math.round(candidate.eastingM), northingM: Math.round(candidate.northingM),
      expectedPayP50M: +candidate.p50NetPayM.toFixed(1), score: +candidate.score.toFixed(1) } : null,
    reservoirModelContacts: reservoirModel.model?.contacts.map(contact => ({ type: contact.type, tvdM: contact.trueVerticalDepthM })),
  })

  return <>
    <div className={`app ${tourOpen ? 'with-tour' : ''} ${assistantOpen ? 'with-assistant' : ''}`}>
      <header className="app-header">
        <div className="brand"><span aria-hidden="true">DS</span><strong>DrillSim</strong></div>
        <label>Field<select value={fieldId} disabled={loading && !pkg} onChange={event => { setRunId(''); setFieldId(event.target.value); setScreen('field') }}>
          {originalFields.map(field => <option key={field.id} value={field.id}>{field.name}</option>)}
        </select></label>
        <label>Reservoir<select value={reservoir} disabled={!reservoirs.length} onChange={event => { setRunId(''); setReservoir(event.target.value) }}>
          {reservoirs.map(item => <option key={item.name} value={item.name}>{item.name}</option>)}
        </select></label>
        <label>Run<select value={runId} onChange={event => event.target.value ? openRun(event.target.value) : setRunId('')}>
          <option value="">No run selected</option>
          {([['In progress', (label: string) => !/failed|cancelled|results ready/.test(label)], ['Finished', (label: string) => label === 'results ready'],
            ['Failed or cancelled', (label: string) => /failed|cancelled/.test(label)]] as const).map(([group, test]) => {
            const items = fieldScenarios.filter(item => test(runStageLabel(item, runStates[item.scenarioId])))
            return items.length ? <optgroup key={group} label={group}>{items.map(item => <option key={item.scenarioId} value={item.scenarioId}>
              {item.seedLabel} · {runStageLabel(item, runStates[item.scenarioId])}</option>)}</optgroup> : null
          })}
        </select></label>
        <nav className="header-links" aria-label="Help and settings">
          <button type="button" className="assistant-toggle" aria-pressed={assistantOpen} aria-controls="assistant-panel" onClick={() => { setAssistantOpen(open => !open); setTourOpen(false) }}>Ask DrillSim</button>
          <button type="button" aria-pressed={tourOpen} onClick={() => { setTourOpen(open => !open); setAssistantOpen(false) }}>Tour</button>
          <a href={demoGuideUrl} target="_blank" rel="noreferrer">Demo guide</a>
          <button type="button" aria-pressed={screen === 'settings'} onClick={() => setScreen('settings')}>Settings</button>
        </nav>
      </header>
      <nav className="steps" aria-label="Workflow">
        <ol>{steps.map((step, index) => <li key={step.id}>
          <button type="button" data-step={step.id} aria-current={screen === step.id ? 'step' : undefined} onClick={() => setScreen(step.id)}>
            <span className="step-number">{index + 1}</span>
            <span><strong>{step.label}</strong><small>{needsRun(step.id) ? 'Needs a target' : step.question}</small></span>
          </button>
        </li>)}</ol>
        {run && <p className="run-chip">Run: <strong>{run.seedLabel}</strong> · {runStageLabel(run, runStates[run.scenarioId])}</p>}
      </nav>
      {error && pkg && <p className="app-error" role="alert">{error}</p>}
      <main className="screen">{content}</main>
    </div>
    <AssistantPanel open={assistantOpen} screen={screen} onClose={() => setAssistantOpen(false)} />
    <GuidedTour open={tourOpen} screen={screen} onNavigate={setScreen} onClose={() => setTourOpen(false)} />
  </>
}
