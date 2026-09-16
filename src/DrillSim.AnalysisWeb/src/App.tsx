import { useEffect, useMemo, useRef, useState } from 'react'
import {
  getAgentStatus,
  getAnalysis,
  getEvidenceVisibility,
  getFieldPackage,
  getFields,
  getMapsStatus,
  getPrediction,
  getProduction,
  getReveal,
  getScenario,
  getScenarios,
  getScorecard,
  applyAnalysis,
} from './api'
import { AiFieldNote } from './components/AiFieldNote'
import { EvidenceMatrix } from './components/EvidenceMatrix'
import { FieldStage } from './components/FieldStage'
import { HypothesisSequencer } from './components/HypothesisSequencer'
import { ScenarioContextBar } from './components/ScenarioWorkspace'
import { reservoirOptionsOf } from './data'
import { analysisIsStale, configurationErrors, configurationKey, defaultConfiguration, verifyAppliedAnalysis } from './analysisConfiguration'
import { evidenceScopeKey, isLifecycleTask, taskById, type TaskId } from './sequencer'
import type {
  AgentStatus,
  AnalysisResult,
  AnalysisConfiguration,
  FieldPackage,
  FieldSummary,
  MapsStatus,
  PredictionRecord,
  RankedCandidate,
  Scenario,
  ScenarioResources,
} from './types'
import { useFieldAgent } from './useFieldAgent'
import { GuidedTutorial } from './components/GuidedTutorial'
import demoGuideUrl from '../../../docs/index.html?url'
import { fieldOptions, scenarioOptions } from './selection'
import { StatusSelect } from './components/StatusSelect'
import { ScenarioCreation } from './components/ScenarioCreation'
import type { PredictionPreparationState } from './predictionBinding'

const emptyScenarioResources: ScenarioResources = { loading: false, error: '' }

function preferredField(items: FieldSummary[]) {
  return (
    items.find((field) => field.name.includes('TROLL FORCE-SODIR')) ??
    items.find((field) => field.name.includes('FORCE-SODIR 15/9')) ??
    items.find((field) => field.name.includes('SYNTH-northstar')) ??
    items[0]
  )?.id ?? ''
}

export default function App() {
  const [fields, setFields] = useState<FieldSummary[]>([])
  const [fieldId, setFieldId] = useState('')
  const [scenarios, setScenarios] = useState<Scenario[]>([])
  const [scenarioId, setScenarioId] = useState('')
  const [scenarioResources, setScenarioResources] = useState<ScenarioResources>(emptyScenarioResources)
  const [fieldPackage, setFieldPackage] = useState<FieldPackage>()
  const [reservoirs, setReservoirs] = useState<Array<{ name: string; controls: number }>>([])
  const [reservoir, setReservoir] = useState('')
  const [analysis, setAnalysis] = useState<AnalysisResult>()
  const [agentStatus, setAgentStatus] = useState<AgentStatus>()
  const [mapsStatus, setMapsStatus] = useState<MapsStatus>()
  const [activeTask, setActiveTask] = useState<TaskId>('targets')
  const [visitedTasks, setVisitedTasks] = useState<{ scope: string; ids: ReadonlySet<TaskId> }>({ scope: '', ids: new Set() })
  const [configurationDraft, setConfigurationDraft] = useState<AnalysisConfiguration>({ ...defaultConfiguration })
  const [applying, setApplying] = useState(false)
  const [applyError, setApplyError] = useState('')
  const [applyMessage, setApplyMessage] = useState('')
  const [refreshKey, setRefreshKey] = useState(0)
  const [agentContext, setAgentContext] = useState('')
  const analysisRequest = useRef<AbortController | undefined>(undefined)
  const liveContext = useRef('')
  const [selectedCandidate, setSelectedCandidate] = useState<RankedCandidate>()
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [refreshingScenario, setRefreshingScenario] = useState(false)
  const [tutorialOpen, setTutorialOpen] = useState(false)
  const [creatingScenario, setCreatingScenario] = useState(false)
  const [creationNotice, setCreationNotice] = useState('')
  const [preparation, setPreparation] = useState<PredictionPreparationState>()
  const scenarioRead = useRef<AbortController | undefined>(undefined)
  const selectedScenarioRef = useRef(scenarioId)
  selectedScenarioRef.current = scenarioId
  const initialScenarioSettings = useRef<{ scenarioId: string; configuration: AnalysisConfiguration } | undefined>(undefined)
  const agent = useFieldAgent()
  const selectedScenario = useMemo(
    () => scenarios.find((scenario) => scenario.scenarioId === scenarioId),
    [scenarios, scenarioId],
  )
  const fieldScenarios = useMemo(
    () => scenarios.filter((scenario) => scenario.sourceFieldId === fieldId),
    [scenarios, fieldId],
  )
  const usingClone = Boolean(
    selectedScenario?.clonedFieldId &&
    (selectedScenario.status === 'Revealed' || selectedScenario.status === 'Scored'),
  )
  liveContext.current = JSON.stringify([fieldId, reservoir, scenarioId, selectedScenario?.asOfUtc, fieldPackage?.sha256])
  const visitedScope = fieldPackage ? evidenceScopeKey(fieldPackage, reservoir, selectedScenario) : ''
  const stale = Boolean(fieldPackage && analysis && analysisIsStale(analysis, fieldPackage, reservoir, configurationDraft))
  const configuredAnalysis = Boolean(analysis &&
    configurationKey(analysis.configuration ?? defaultConfiguration) !== configurationKey(defaultConfiguration))
  const agentBlock = applying || stale
    ? 'Apply or restore analysis settings before requesting an AI note.'
    : configuredAnalysis ? 'AI tools are not bound to this configured revision in this client. Export the bundle for review; apply defaults before using these default-analysis AI tools.' : undefined

  useEffect(() => {
    setVisitedTasks({ scope: visitedScope, ids: new Set([activeTask]) })
  }, [visitedScope])
  useEffect(() => () => { analysisRequest.current?.abort(); scenarioRead.current?.abort() }, [])
  useEffect(() => {
    scenarioRead.current?.abort()
    setRefreshingScenario(false)
  }, [scenarioId])

  useEffect(() => {
    const controller = new AbortController()
    Promise.all([
      getFields(controller.signal),
      getScenarios(controller.signal),
      getAgentStatus(controller.signal),
      getMapsStatus(controller.signal),
    ])
      .then(([items, nextScenarios, status, nextMapsStatus]) => {
        setFields(items)
        setScenarios(nextScenarios)
        setAgentStatus(status)
        setMapsStatus(nextMapsStatus)
        setFieldId(preferredField(items))
      })
      .catch((reason) => {
        setError(reason instanceof Error ? reason.message : 'Unable to load instrument controls.')
        setLoading(false)
      })
    return () => controller.abort()
  }, [])

  useEffect(() => {
    if (!selectedScenario) {
      setScenarioResources(emptyScenarioResources)
      return
    }
    const controller = new AbortController()
    setScenarioResources(current => ({
      ...(current.scenarioId === selectedScenario.scenarioId ? current : {}),
      scenarioId: selectedScenario.scenarioId, loading: true, error: '',
    }))
    Promise.all([
      getEvidenceVisibility(selectedScenario.scenarioId, selectedScenario.initialAsOfUtc, controller.signal),
      getEvidenceVisibility(selectedScenario.scenarioId, selectedScenario.asOfUtc, controller.signal),
      getPrediction(selectedScenario.scenarioId, controller.signal),
      getReveal(selectedScenario.scenarioId, controller.signal),
      getProduction(selectedScenario.scenarioId, controller.signal),
      getScorecard(selectedScenario.scenarioId, controller.signal),
    ])
      .then(([initial, current, prediction, reveal, production, scorecard]) => {
        if (controller.signal.aborted) return
        setScenarioResources(existing => ({
          ...(existing.scenarioId === selectedScenario.scenarioId ? existing : {}),
          scenarioId: selectedScenario.scenarioId,
          loading: false,
          error: '',
          evidence: { initial, current },
          prediction,
          reveal,
          production,
          scorecard,
        }))
      })
      .catch((reason) => {
        if (!controller.signal.aborted) {
          setScenarioResources({
            scenarioId: selectedScenario.scenarioId,
            loading: false,
            error: reason instanceof Error ? reason.message : 'Unable to load scenario ledger.',
          })
        }
      })
    return () => controller.abort()
  }, [selectedScenario])

  useEffect(() => {
    if (!fieldId || scenarioId && !selectedScenario) return
    const controller = new AbortController()
    analysisRequest.current?.abort()
    setApplying(false)
    setApplyError('')
    setApplyMessage('')
    const packageFieldId = usingClone ? selectedScenario!.clonedFieldId! : selectedScenario?.sourceFieldId ?? fieldId
    const scope = selectedScenario
      ? { scenarioId: selectedScenario.scenarioId, asOf: selectedScenario.asOfUtc }
      : undefined
    setLoading(true)
    setError('')
    setFieldPackage(undefined)
    setAnalysis(undefined)
    setSelectedCandidate(undefined)
    agent.clear()
    setAgentContext('')
    getFieldPackage(packageFieldId, scope, controller.signal)
      .then((nextPackage) => {
        const nextReservoirs = reservoirOptionsOf(nextPackage.geologicalProperties)
        const scenarioReservoir = selectedScenario?.reservoirName
        const nextReservoir = scenarioReservoir ?? (
          nextReservoirs.find((item) => item.name.toLowerCase().includes('sognefjord')) ??
          nextReservoirs[0]
        )?.name
        if (!nextReservoir) throw new Error('No formation intersects at least four logged wellbores in this field.')
        const controls = nextReservoirs.find((item) =>
          item.name.localeCompare(nextReservoir, undefined, { sensitivity: 'accent' }) === 0)?.controls ?? 0
        return getAnalysis(packageFieldId, nextReservoir, scope, controller.signal)
          .then((nextAnalysis) => ({
            nextPackage,
            nextReservoirs: scenarioReservoir ? [{ name: nextReservoir, controls }] : nextReservoirs,
            nextReservoir,
            nextAnalysis,
          }))
      })
      .then(({ nextPackage, nextReservoirs, nextReservoir, nextAnalysis }) => {
        if (controller.signal.aborted) return
        setFieldPackage(nextPackage)
        setReservoirs(nextReservoirs)
        setReservoir(nextReservoir)
        setAnalysis(nextAnalysis)
        const initialSettings = initialScenarioSettings.current
        setConfigurationDraft({ ...(initialSettings && initialSettings.scenarioId === selectedScenario?.scenarioId
          ? initialSettings.configuration : nextAnalysis.configuration ?? defaultConfiguration) })
        if (initialSettings && initialSettings.scenarioId === selectedScenario?.scenarioId) initialScenarioSettings.current = undefined
        setSelectedCandidate(nextAnalysis.ranking[0])
      })
      .catch((reason) => {
        if (!controller.signal.aborted) {
          setError(reason instanceof Error ? reason.message : 'Unable to assemble field package.')
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false)
      })
    return () => controller.abort()
  }, [
    fieldId,
    scenarioId,
    selectedScenario?.scenarioId,
    selectedScenario?.sourceFieldId,
    selectedScenario?.clonedFieldId,
    selectedScenario?.asOfUtc,
    selectedScenario?.reservoirName,
    usingClone,
    refreshKey,
  ])

  useEffect(() => {
    if (selectedScenario || !fieldId || !reservoir || fieldPackage?.fieldId !== fieldId ||
        analysis?.fieldId === fieldId && analysis.reservoirName?.toLowerCase() === reservoir.toLowerCase()) return
    const controller = new AbortController()
    analysisRequest.current?.abort()
    setApplying(false)
    setApplyError('')
    setApplyMessage('')
    setLoading(true)
    setError('')
    getAnalysis(fieldId, reservoir, undefined, controller.signal)
      .then((nextAnalysis) => {
        if (controller.signal.aborted) return
        setAnalysis(nextAnalysis)
        setConfigurationDraft({ ...(nextAnalysis.configuration ?? defaultConfiguration) })
        setSelectedCandidate(nextAnalysis.ranking[0])
        agent.clear()
        setAgentContext('')
      })
      .catch((reason) => {
        if (!controller.signal.aborted) {
          setError(reason instanceof Error ? reason.message : 'Unable to analyze reservoir.')
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false)
      })
    return () => controller.abort()
  }, [fieldId, fieldPackage?.fieldId, reservoir, analysis?.fieldId, analysis?.reservoirName, selectedScenario])

  async function runAgent(prompt: string) {
    if (!fieldPackage || !selectedCandidate || agentBlock) return
    setAgentContext(`Note requested for ${selectedCandidate.candidateId} · package ${fieldPackage.sha256.slice(0, 16)} · result ${analysis?.analysisSha256?.slice(0, 16) || 'legacy default'}. Changing tasks does not re-run or save this note.`)
    await agent.run(prompt, fieldPackage.fieldId, fieldPackage.sha256, {
      activeTaskId: activeTask,
      activeTask: taskById[activeTask].label,
      selectedReservoir: reservoir,
      selectedCandidate,
      sourceFieldId: selectedScenario?.sourceFieldId ?? fieldId,
    }, selectedScenario ? {
      scenarioId: selectedScenario.scenarioId,
      asOfUtc: selectedScenario.asOfUtc,
    } : undefined)
  }

  function selectTask(id: TaskId) {
    setVisitedTasks((current) => ({ scope: visitedScope, ids: new Set([...(current.scope === visitedScope ? current.ids : []), id]) }))
    setActiveTask(id)
    if (isLifecycleTask(id) && !scenarioId) setCreatingScenario(true)
  }

  async function applyConfiguration() {
    if (!fieldPackage || !analysis || applying) return
    const errors = configurationErrors(configurationDraft)
    if (errors.length) { setApplyError(errors.join(' ')); return }
    const requested = { ...configurationDraft }
    const context = liveContext.current
    const controller = new AbortController()
    analysisRequest.current?.abort()
    analysisRequest.current = controller
    setApplying(true)
    setApplyError('')
    setApplyMessage('')
    try {
      const next = await applyAnalysis(fieldPackage.fieldId, reservoir, requested,
        selectedScenario ? { scenarioId: selectedScenario.scenarioId, asOf: selectedScenario.asOfUtc } : undefined,
        controller.signal)
      if (controller.signal.aborted || liveContext.current !== context) return
      verifyAppliedAnalysis(next, fieldPackage, reservoir, requested)
      setAnalysis(next)
      setConfigurationDraft({ ...next.configuration! })
      setSelectedCandidate(next.ranking.find((candidate) => candidate.candidateId === selectedCandidate?.candidateId) ?? next.ranking[0])
      setApplyMessage(`Applied result ${next.analysisSha256!.slice(0, 16)}. ${next.ranking.length} retained targets from ${next.candidateGrid!.length} grid cells.`)
      agent.clear()
      setAgentContext('')
    } catch (reason) {
      if (!controller.signal.aborted && liveContext.current === context) setApplyError(reason instanceof Error ? reason.message : String(reason))
    } finally {
      if (!controller.signal.aborted && liveContext.current === context) setApplying(false)
    }
  }

  function changeField(nextFieldId: string) {
    setCreatingScenario(false)
    setCreationNotice('')
    setScenarioId('')
    setFieldId(nextFieldId)
  }

  function changeScenario(nextScenarioId: string) {
    setCreatingScenario(false)
    setCreationNotice('')
    setScenarioId(nextScenarioId)
    const next = scenarios.find((scenario) => scenario.scenarioId === nextScenarioId)
    if (next) {
      setFieldId(next.sourceFieldId)
      setReservoir(next.reservoirName)
    }
  }

  function beginScenario() {
    setCreatingScenario(true)
    setCreationNotice('')
    selectTask('prediction')
  }

  function scenarioCreated(next: Scenario, configuration: AnalysisConfiguration) {
    initialScenarioSettings.current = { scenarioId: next.scenarioId, configuration: { ...configuration } }
    setScenarios(current => [...current.filter(item => item.scenarioId !== next.scenarioId), next])
    setScenarioId(next.scenarioId)
    setFieldId(next.sourceFieldId)
    setReservoir(next.reservoirName)
    setCreatingScenario(false)
    setCreationNotice(`Opened ${next.seedLabel}. Identical creation requests reuse this scenario; existing results are never reset.`)
    setActiveTask(['Scored', 'Revealed'].includes(next.status) ? 'new-evidence' : 'prediction')
  }

  function predictionSaved(prediction: PredictionRecord) {
    setScenarioResources((current) => current.scenarioId === prediction.scenarioId
      ? { ...current, prediction } : current)
    setScenarios((current) => current.map((scenario) => scenario.scenarioId === prediction.scenarioId
      ? {
        ...scenario,
        status: ['Draft', 'Armed', 'PredictionDrafted', 'PredictionSealed'].includes(scenario.status)
          ? prediction.approval ? 'HumanApproved' : prediction.seal ? 'PredictionSealed' : 'PredictionDrafted'
          : scenario.status,
        modifiedUtc: prediction.modifiedUtc,
      } : scenario))
  }

  async function refreshScenario() {
    if (!selectedScenario) return
    const id = selectedScenario.scenarioId
    const controller = new AbortController()
    scenarioRead.current?.abort()
    scenarioRead.current = controller
    setRefreshingScenario(true)
    try {
      const next = await getScenario(id, controller.signal)
      if (controller.signal.aborted || selectedScenarioRef.current !== id) return
      setScenarios((current) => current.map((scenario) =>
        scenario.scenarioId === next.scenarioId ? next : scenario))
    } catch (reason) {
      if (!controller.signal.aborted && selectedScenarioRef.current === id) setScenarioResources((current) => ({
        ...current,
        error: reason instanceof Error ? reason.message : 'Unable to refresh scenario state.',
      }))
    } finally {
      if (!controller.signal.aborted && selectedScenarioRef.current === id) setRefreshingScenario(false)
    }
  }

  const sourceFieldName = fields.find((field) => field.id === fieldId)?.name ?? fieldId
  let originalFieldId = selectedScenario?.sourceFieldId ?? fieldId
  const seenOrigins = new Set<string>()
  while (originalFieldId && !seenOrigins.has(originalFieldId)) {
    seenOrigins.add(originalFieldId)
    const origin = scenarios.find(item => item.clonedFieldId === originalFieldId)?.sourceFieldId
    if (!origin) break
    originalFieldId = origin
  }
  const originalFieldName = fields.find(field => field.id === originalFieldId)?.name ?? sourceFieldName

  return (
    <>
    <main className={`portal ${tutorialOpen ? 'has-tutorial' : ''}`}>
      <header className="top-panel" data-tutorial="scope">
        <div className="product-lockup"><span>DS</span><div><strong>DrillSim</strong><small>Hypothesis Sequencer</small></div></div>
        <StatusSelect label="Field" value={fieldId} options={fieldOptions(fields, scenarios)} onChange={changeField} disabled={loading || applying} />
        <label><span>RESERVOIR</span><select value={reservoir} onChange={(event) => setReservoir(event.target.value)} disabled={loading || applying || Boolean(selectedScenario)}>{reservoirs.map((item) => <option key={item.name} value={item.name}>{item.name} · {item.controls} bores</option>)}</select></label>
        <StatusSelect label="Scenario" value={scenarioId} options={scenarioOptions(fieldScenarios)} onChange={changeScenario} disabled={loading || applying} />
        <div className="system-readout"><span className="lamp on">SERVICES</span><span className={`lamp ${mapsStatus?.configured ? 'on' : ''}`}>MAP</span><span className={`lamp ${agentStatus?.configured ? 'on' : ''}`}>AG-UI</span><code>{loading ? 'SYNC' : fieldPackage?.sha256.slice(0, 10) ?? 'NO PACKAGE'}</code></div>
      </header>
      <div className="workspace-help" role="group" aria-label="Learning resources">
        <a href={demoGuideUrl} target="_blank" rel="noreferrer">Demo & architecture guide</a>
        <button type="button" aria-expanded={tutorialOpen} onClick={() => setTutorialOpen(value => !value)}>Guided tutorial</button>
        <button type="button" data-tutorial="new-simulation" disabled={!originalFieldId || !reservoir || loading || applying || Boolean(error)} onClick={beginScenario}>New simulation</button>
      </div>
      {creationNotice && <p className="creation-notice" role="status">{creationNotice}</p>}
      {creatingScenario && <ScenarioCreation key={`${originalFieldId}:${reservoir}`} sourceFieldId={originalFieldId}
        sourceFieldName={originalFieldName} reservoirName={selectedScenario?.reservoirName ?? reservoir}
        configuration={analysis?.configuration ?? defaultConfiguration} onCreated={scenarioCreated} onClose={() => setCreatingScenario(false)} />}
      {selectedScenario && <ScenarioContextBar
        scenario={selectedScenario}
        sourceFieldName={sourceFieldName}
        usingClone={usingClone}
        refreshing={refreshingScenario}
        onRefresh={() => void refreshScenario()} />}
      {error && <div className="portal-error" role="alert"><strong>Signal lost</strong><span>{error}</span></div>}
      {fieldPackage && analysis ? (
        <>
          <HypothesisSequencer active={activeTask} visited={visitedTasks.scope === visitedScope ? visitedTasks.ids : new Set()}
            onSelect={selectTask} analysis={analysis} stale={stale} scenario={selectedScenario} resources={scenarioResources} />
          <FieldStage
            fieldPackage={fieldPackage}
            analysis={analysis}
            reservoirName={reservoir}
            selectedCandidate={selectedCandidate}
            onCandidate={setSelectedCandidate}
            activeTask={activeTask}
            mapsStatus={mapsStatus}
            scenario={selectedScenario}
            scenarioResources={scenarioResources}
            onPredictionSaved={predictionSaved}
            controls={{ draft: configurationDraft, applying, error: applyError, message: applyMessage, stale,
              onDraft: (draft) => { setConfigurationDraft(draft); setApplyError(''); setApplyMessage('') },
              onApply: () => void applyConfiguration() }}
            onRefresh={() => setRefreshKey((key) => key + 1)}
            onOperatorView={(view) => setScenarioResources((current) => current.scenarioId === scenarioId ? { ...current, operator: view } : current)}
            onOperatorSetup={(setup) => setScenarioResources(current => current.scenarioId === scenarioId ? { ...current, operatorSetup: setup } : current)}
            onPredictionPrepared={setPreparation}
            onNavigate={selectTask}
            onNewSimulation={beginScenario}
            onOperatorChanged={() => void refreshScenario()}
            challenge={<AiFieldNote status={agentStatus} state={agent.state} candidate={selectedCandidate}
              actions={['challenge']} blockReason={agentBlock} context={agentContext} onRun={(prompt) => void runAgent(prompt)} />}
          />
          <div className="lower-bay">
            <EvidenceMatrix fieldPackage={fieldPackage} />
            {activeTask !== 'challenge' && <AiFieldNote status={agentStatus} state={agent.state} candidate={selectedCandidate}
              blockReason={agentBlock} context={agentContext} onRun={(prompt) => void runAgent(prompt)} />}
          </div>
        </>
      ) : (
        <section className="loading-stage" role="status" aria-live="polite">
          {loading ? <div className="loading-calibration"><strong>CALIBRATING FIELD SIGNALS</strong><span /><span /><span /></div> : <strong>NO FIELD PACKAGE</strong>}
        </section>
      )}
    </main>
    <GuidedTutorial open={tutorialOpen} onClose={() => setTutorialOpen(false)} onNavigate={selectTask}
      guideUrl={demoGuideUrl}
      context={{ fieldId, packageFieldId: fieldPackage?.fieldId, reservoir, scenarioId, scenarioStatus: selectedScenario?.status,
        scenario: selectedScenario, resources: scenarioResources, preparation, creatingScenario,
        activeTask, loading, applying, error: error || applyError, stale, draft: configurationDraft,
        applied: analysis?.configuration, analysisSha256: analysis?.analysisSha256, gridCount: analysis?.candidateGrid?.length }} />
    </>
  )
}
