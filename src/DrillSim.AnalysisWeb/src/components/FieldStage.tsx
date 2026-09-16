import { useEffect, useMemo, useState } from 'react'
import { buildWellViews, AnalysisWorkspace, type WorkspaceMode } from './AnalysisWorkspace'
import type { AnalysisResult, FieldPackage, MapsStatus, PredictionRecord, RankedCandidate, Scenario, ScenarioResources } from '../types'
import { ScenarioSequenceWorkspace } from './ScenarioWorkspace'
import { evidenceScopeKey, isLifecycleTask, taskById, type TaskId } from '../sequencer'
import { predictionHandoffBlock } from '../analysisConfiguration'
import { TaskWorkspace, type AnalysisControls } from './TaskWorkspace'
import type { OperatorView } from '../operator'
import { hypothesisScope, hypothesisScopeKey } from '../hypotheses'
import { HypothesisWorkbench, isHypothesisTask } from './HypothesisWorkbench'
import type { PredictionPreparationState } from '../predictionBinding'
import type { SimulatorSetup } from '../simulatorSetup'

export function FieldStage({
  fieldPackage,
  analysis,
  reservoirName,
  selectedCandidate,
  onCandidate,
  activeTask,
  mapsStatus,
  scenario,
  scenarioResources,
  onPredictionSaved,
  controls,
  challenge,
  onRefresh,
  onOperatorView,
  onOperatorChanged,
  onOperatorSetup,
  onPredictionPrepared,
  onNavigate,
  onNewSimulation,
}: {
  fieldPackage: FieldPackage
  analysis: AnalysisResult
  reservoirName: string
  selectedCandidate?: RankedCandidate
  onCandidate: (candidate: RankedCandidate) => void
  activeTask: TaskId
  mapsStatus?: MapsStatus
  scenario?: Scenario
  scenarioResources: ScenarioResources
  onPredictionSaved: (prediction: PredictionRecord) => void
  controls: AnalysisControls
  challenge: React.ReactNode
  onRefresh: () => void
  onOperatorView: (view: OperatorView | undefined) => void
  onOperatorChanged: () => void
  onOperatorSetup?: (setup: SimulatorSetup | undefined) => void
  onPredictionPrepared?: (state: PredictionPreparationState) => void
  onNavigate?: (task: TaskId) => void
  onNewSimulation?: () => void
}) {
  const wells = useMemo(
    () => buildWellViews(fieldPackage, analysis.wellSummaries, analysis.reservoirName ?? reservoirName, analysis.configuration),
    [fieldPackage, analysis.wellSummaries, analysis.reservoirName, reservoirName, analysis.configuration],
  )
  const initialNeighborId = selectedCandidate?.neighborEvidenceIds[0]?.replace('well:', '')
  const initialWellId = wells.find((well) => well.wellId === initialNeighborId)?.id ?? wells[0]?.id ?? ''
  const [selectedWellId, setSelectedWellId] = useState(initialWellId)
  const [mode, setMode] = useState<WorkspaceMode>(taskById[activeTask].mode)
  const [rationale, setRationale] = useState({ scope: '', text: '' })
  const rationaleScope = JSON.stringify([evidenceScopeKey(fieldPackage, reservoirName, scenario), analysis.analysisSha256, selectedCandidate?.candidateId])
  const selectedWell = wells.find((well) => well.id === selectedWellId) ?? wells[0]
  const scenarioTask = isLifecycleTask(activeTask)
  const savedScope = hypothesisScope(fieldPackage, reservoirName, scenario)

  useEffect(() => { if (!isLifecycleTask(activeTask)) setMode(taskById[activeTask].mode) }, [activeTask])
  useEffect(() => {
    const neighbors = selectedCandidate?.neighborEvidenceIds.map((id) => id.replace('well:', '')) ?? []
    if (!neighbors.includes(selectedWell?.wellId ?? '') && neighbors[0]) {
      const bore = wells.find((well) => well.wellId === neighbors[0])
      if (bore) setSelectedWellId(bore.id)
    }
  }, [fieldPackage.fieldId, selectedCandidate?.candidateId])

  return (
    <section className="field-stage" aria-labelledby="stage-title">
      <header>
        <div>
          <h2 id="stage-title">{taskById[activeTask].label}</h2>
          <p>{taskById[activeTask].description}</p>
        </div>
        <div className="stage-readout">
          <span>LOGGED CONTROLS</span><strong>{analysis.wellSummaries.length}</strong>
          <span>RANKED TARGETS</span><strong>{analysis.ranking.length}</strong>
        </div>
      </header>
      <div id="task-workspace" data-tutorial="task-workspace" className={`task-workspace ${scenarioTask ? 'lifecycle-workspace' : ''}`}>
        {!scenarioTask && <>
          <p className={`analysis-revision ${controls.stale ? 'stale' : ''}`} role="status">
            {controls.applying ? 'Recalculating · previous output retained' : controls.stale ? 'Previous result · pending settings or evidence differ' : 'Result settings in use'}
            {' '}· {analysis.reservoirName} · {analysis.configuration?.version ?? 'Legacy defaults'}
            {' '}· result <code>{analysis.analysisSha256?.slice(0, 16) || 'fingerprint unavailable'}</code>
          </p>
          {!isHypothesisTask(activeTask) && <TaskWorkspace taskId={activeTask} pkg={fieldPackage} analysis={analysis} wells={wells}
            selectedWellId={selectedWell?.id ?? ''} onWell={setSelectedWellId} candidate={selectedCandidate} onCandidate={onCandidate}
            controls={controls} scenario={scenario} onRefresh={onRefresh}
            rationale={rationale.scope === rationaleScope ? rationale.text : ''}
            onRationale={(text) => setRationale({ scope: rationaleScope, text })} />}
        </>}
        <HypothesisWorkbench key={hypothesisScopeKey(savedScope)} taskId={activeTask} pkg={fieldPackage} analysis={analysis}
          scope={savedScope} candidateId={selectedCandidate?.candidateId ?? ''} stale={controls.stale || controls.applying}
          ai={challenge} />
        {scenarioTask && <ScenarioSequenceWorkspace taskId={activeTask} scenario={scenario} resources={scenarioResources} fieldPackage={fieldPackage} analysis={analysis}
          candidate={selectedCandidate} onPredictionSaved={onPredictionSaved}
          onOperatorView={onOperatorView} onOperatorChanged={onOperatorChanged}
          onOperatorSetup={onOperatorSetup} onPredictionPrepared={onPredictionPrepared} onNavigate={onNavigate} onNewSimulation={onNewSimulation}
          handoffBlock={predictionHandoffBlock(analysis, controls.stale, controls.applying)} />}
      </div>
      <div className="stage-canvas" hidden={scenarioTask}>
          <AnalysisWorkspace mode={mode} onMode={setMode} fieldPackage={fieldPackage} wells={wells}
            candidates={analysis.ranking} selectedCandidate={selectedCandidate} selectedWellId={selectedWell?.id ?? ''}
            mapsStatus={mapsStatus} onCandidate={onCandidate} onWell={setSelectedWellId}
            proposedPath={scenarioResources.prediction?.body.proposedWellPath} />
        <aside className="target-readout" data-tutorial="target-readout" aria-live="polite">
          <span>SELECTED TARGET</span>
          <strong>{selectedCandidate ? `#${selectedCandidate.rank}` : '—'}</strong>
          <label className="candidate-select">
            <span>RANKED TARGET</span>
            <select
              value={selectedCandidate?.candidateId ?? ''}
              disabled={!analysis.ranking.length}
              onChange={(event) => {
                const candidate = analysis.ranking.find((item) => item.candidateId === event.target.value)
                if (candidate) onCandidate(candidate)
              }}
            >
              {!analysis.ranking.length && <option value="">No retained targets</option>}
              {analysis.ranking.map((candidate) => (
                <option key={candidate.candidateId} value={candidate.candidateId}>
                  #{candidate.rank} · P50 {candidate.p50NetPayM.toFixed(1)} m
                </option>
              ))}
            </select>
          </label>
          <dl>
            <div><dt>P90</dt><dd>{selectedCandidate?.p90NetPayM.toFixed(1) ?? '—'} m</dd></div>
            <div><dt>P50</dt><dd>{selectedCandidate?.p50NetPayM.toFixed(1) ?? '—'} m</dd></div>
            <div><dt>P10</dt><dd>{selectedCandidate?.p10NetPayM.toFixed(1) ?? '—'} m</dd></div>
            <div><dt>PORO</dt><dd>{selectedCandidate ? `${(selectedCandidate.meanPayPorosity * 100).toFixed(1)}%` : '—'}</dd></div>
            <div><dt>PERM</dt><dd>{selectedCandidate?.meanPayPermeabilityMd.toFixed(1) ?? '—'} mD</dd></div>
            <div><dt>UNC.</dt><dd>{selectedCandidate ? `${(selectedCandidate.relativeUncertainty * 100).toFixed(0)}%` : '—'}</dd></div>
          </dl>
          <p className="overlay-note">Uncalibrated qualifying-rock proxy. Not fluid-conditioned hydrocarbon pay or reserves.</p>
          <div className="target-evidence">
            <span>NEIGHBOR CONTROLS</span>
            {selectedCandidate?.neighborEvidenceIds.map((evidenceId) => {
              const wellId = evidenceId.replace('well:', '')
              const well = wells.find((item) => item.wellId === wellId)
              if (!well) return null
              const distance = Math.hypot(
                (well.summary?.eastingM ?? 0) - (selectedCandidate?.eastingM ?? 0),
                (well.summary?.northingM ?? 0) - (selectedCandidate?.northingM ?? 0),
              )
              return (
                <button key={wellId} className={well.id === selectedWell?.id ? 'selected' : ''} onClick={() => setSelectedWellId(well.id)} title={evidenceId}>
                  <strong>{well.name}</strong>
                  <span>{(distance / 1000).toFixed(1)} km · {well.summary?.netPayThicknessM.toFixed(0) ?? '—'} m rock proxy</span>
                </button>
              )
            })}
          </div>
          <div className="selected-well-readout">
            <span>ACTIVE WELL</span>
            <strong>{selectedWell?.name ?? '—'}</strong>
            <small>{selectedWell?.reservoir ? `${selectedWell.reservoir.top.toFixed(0)}–${selectedWell.reservoir.base.toFixed(0)} m MD` : 'No qualifying interval'}</small>
            {selectedWell?.contacts.map((contact) => (
              <small key={`${contact.type}:${contact.depth}`}>{contact.type.replace(/Contact$/, '')} · {contact.depth.toFixed(0)} m MD</small>
            ))}
          </div>
          {scenarioResources.prediction && (
            <p className="overlay-note">Proposed path is shown in 2D and 3D local-grid views. Map overlay is withheld because no public grid-to-WGS84 transform is available.</p>
          )}
        </aside>
      </div>
    </section>
  )
}
