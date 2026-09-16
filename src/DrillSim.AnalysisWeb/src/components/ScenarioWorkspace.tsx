import type {
  AnalysisResult,
  FieldPackage,
  EvidenceVisibilitySummary,
  PredictionRecord,
  PublicScorecard,
  QuantileValues,
  RankedCandidate,
  Scenario,
  ScenarioResources,
  ScorecardMetric,
} from '../types'
import { ProductionPane } from './ProductionPane'
import { PredictionPreparation } from './PredictionPreparation'
import { lifecycleStatus, type LifecycleTaskId } from '../sequencer'
import { OperatorControls } from './OperatorControls'
import type { OperatorView } from '../operator'
import type { SimulatorSetup } from '../simulatorSetup'
import type { PredictionPreparationState } from '../predictionBinding'
import { PredictionEditor } from './PredictionEditor'
import { scenarioStatusLabel } from '../selection'
import './SimulationForms.css'

const scoreBases: ScorecardMetric['basis'][] = [
  'HiddenTruth',
  'RevealedObservation',
  'ObservationGap',
  'Baseline',
]

const formatUtc = (value: string) => new Intl.DateTimeFormat(undefined, {
  dateStyle: 'medium',
  timeStyle: 'short',
  timeZone: 'UTC',
}).format(new Date(value)) + ' UTC'

const shortHash = (value?: string) => value ? `${value.slice(0, 12)}…` : '—'

const sentence = (value: string) => value
  .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
  .replace(/[-_]/g, ' ')

const formatMetric = (value?: number | null) => value == null
  ? '—'
  : new Intl.NumberFormat(undefined, { maximumFractionDigits: 3 }).format(value)

function Quantiles({ values, unit = 'm' }: { values?: QuantileValues; unit?: string }) {
  return (
    <dl className="quantile-strip">
      {(['p90', 'p50', 'p10'] as const).map((key) => (
        <div key={key}><dt>{key.toUpperCase()}</dt><dd>{values ? `${formatMetric(values[key])} ${unit}` : '—'}</dd></div>
      ))}
    </dl>
  )
}

export function ScenarioContextBar({
  scenario,
  sourceFieldName,
  usingClone,
  refreshing,
  onRefresh,
}: {
  scenario: Scenario
  sourceFieldName: string
  usingClone: boolean
  refreshing: boolean
  onRefresh: () => void
}) {
  return (
    <section className="scenario-context" aria-label="Scenario clock and provenance">
      <div className="scenario-context-primary">
        <strong>{scenario.seedLabel}</strong>
        <span className="scenario-status">{scenarioStatusLabel(scenario.status)}</span>
        <span>{usingClone ? 'SIMULATED RESULTS' : 'ORIGINAL EVIDENCE'}</span>
        <span>{sourceFieldName}</span>
        <button type="button" onClick={onRefresh} disabled={refreshing}>
          {refreshing ? 'REFRESHING' : 'REFRESH'}
        </button>
      </div>
      <dl>
        <div><dt>INITIAL AS OF</dt><dd>{formatUtc(scenario.initialAsOfUtc)}</dd></div>
        <div><dt>CURRENT AS OF</dt><dd>{formatUtc(scenario.asOfUtc)}</dd></div>
        <div><dt>MODEL VERSIONS</dt><dd>{scenario.worldModelVersion} · {scenario.observationModelVersion} · {scenario.scoringModelVersion}</dd></div>
        <div><dt>ASSUMPTIONS</dt><dd><code title={scenario.assumptionsSha256}>{shortHash(scenario.assumptionsSha256)}</code></dd></div>
      </dl>
    </section>
  )
}

function ScenarioEmpty({ title, children }: { title: string; children: React.ReactNode }) {
  return <div className="scenario-empty"><strong>{title}</strong><p>{children}</p></div>
}

function ScenarioLoading() {
  return (
    <div className="scenario-loading" role="status" aria-label="Loading scenario ledger">
      <span /><span /><span /><span />
    </div>
  )
}

export function PredictionPane({ prediction, scenario }: { prediction?: PredictionRecord; scenario: Scenario }) {
  if (!prediction) {
    return (
      <ScenarioEmpty title="NO PREDICTION DRAFT">
        Complete the prediction above and save it to display the server-validated review here.
        Human approval and execution remain separate from sealing.
      </ScenarioEmpty>
    )
  }
  const body = prediction.body
  const first = body.proposedWellPath[0]
  const last = body.proposedWellPath.at(-1)
  const immutable = Boolean(prediction.seal || prediction.approval)
  return (
    <div className="scenario-pane prediction-pane" data-tutorial="prediction-ledger">
      <header className="scenario-pane-head">
        <div><h3>Prediction ledger · {body.candidateId}</h3><p>{body.rationale}</p></div>
        <div className="ledger-state"><strong>{immutable ? 'READ ONLY' : 'DRAFT SNAPSHOT'}</strong><span>Revision {prediction.revision}</span></div>
      </header>

      <section className="instrument-section pay-summary" aria-labelledby="pay-title">
        <div><h4 id="pay-title">Expected paydirt</h4><p>Prediction quantiles, not reserves.</p></div>
        <Quantiles values={body.expectedPaydirtM} />
      </section>

      <section className="instrument-section" aria-labelledby="path-title">
        <div><h4 id="path-title">Proposed path</h4><p>Local field-grid coordinates; overlaid only in matching 2D and 3D views.</p></div>
        <dl className="dense-readout">
          <div><dt>STATIONS</dt><dd>{body.proposedWellPath.length}</dd></div>
          <div><dt>FINAL MD / TVD</dt><dd>{last ? `${last.measuredDepthM.toFixed(0)} / ${last.trueVerticalDepthM.toFixed(0)} m` : '—'}</dd></div>
          <div><dt>SURFACE E / N</dt><dd>{first ? `${first.eastingM.toFixed(0)} / ${first.northingM.toFixed(0)} m` : '—'}</dd></div>
          <div><dt>BOTTOM E / N</dt><dd>{last ? `${last.eastingM.toFixed(0)} / ${last.northingM.toFixed(0)} m` : '—'}</dd></div>
        </dl>
      </section>

      <section className="instrument-section" aria-labelledby="formation-title">
        <div><h4 id="formation-title">Formation forecast</h4><p>Top, base, and fluids remain quantified uncertainty.</p></div>
        <div className="scenario-table-wrap">
          <table className="scenario-table">
            <thead><tr><th>Formation</th><th>Top P90 / P50 / P10</th><th>Base P90 / P50 / P10</th></tr></thead>
            <tbody>{body.formations.map((formation) => (
              <tr key={formation.formationName}>
                <th>{formation.formationName}</th>
                <td>{formation.topTrueVerticalDepthM.p90.toFixed(0)} / {formation.topTrueVerticalDepthM.p50.toFixed(0)} / {formation.topTrueVerticalDepthM.p10.toFixed(0)} m TVD</td>
                <td>{formation.baseTrueVerticalDepthM.p90.toFixed(0)} / {formation.baseTrueVerticalDepthM.p50.toFixed(0)} / {formation.baseTrueVerticalDepthM.p10.toFixed(0)} m TVD</td>
              </tr>
            ))}</tbody>
          </table>
        </div>
        <div className="prediction-detail-row">
          <div><strong>FLUIDS</strong><span>{body.fluidClasses.join(' · ') || 'No fluid classes published'}</span></div>
          <div><strong>CONTACTS</strong><span>{body.contactPredictions.map((contact) => `${contact.contactType} ${contact.trueVerticalDepthM.p50.toFixed(0)} m P50`).join(' · ') || 'No contacts published'}</span></div>
        </div>
      </section>

      <section className="instrument-section" aria-labelledby="forecast-title">
        <div><h4 id="forecast-title">Forecast checkpoints</h4><p>Prediction-year aggregates from the sealed ledger.</p></div>
        <div className="scenario-table-wrap">
          <table className="scenario-table">
            <thead><tr><th>Year</th><th>Oil m³</th><th>Gas m³</th><th>Water m³</th></tr></thead>
            <tbody>{body.productionForecasts.map((forecast) => (
              <tr key={forecast.year}><th>{forecast.year}</th><td>{formatMetric(forecast.oilM3)}</td><td>{formatMetric(forecast.gasM3)}</td><td>{formatMetric(forecast.waterM3)}</td></tr>
            ))}</tbody>
          </table>
        </div>
      </section>

      <section className="instrument-section" aria-labelledby="integrity-title">
        <div><h4 id="integrity-title">Integrity record</h4><p>{body.citedEvidenceIds.length} cited evidence records · package <code>{shortHash(body.fieldPackageSha256)}</code></p></div>
        <dl className="dense-readout">
          <div><dt>SEALED</dt><dd>{prediction.seal ? formatUtc(prediction.seal.sealedUtc) : 'Not sealed'}</dd></div>
          <div><dt>SEAL HASH</dt><dd><code>{shortHash(prediction.seal?.sha256)}</code></dd></div>
          <div><dt>APPROVED BY</dt><dd>{prediction.approval?.actor ?? 'Not approved'}</dd></div>
          <div><dt>APPROVED</dt><dd>{prediction.approval ? formatUtc(prediction.approval.approvedUtc) : '—'}</dd></div>
        </dl>
        {body.analysisBinding && <p className="boundary-note">Original prediction binding: <code>{body.analysisBinding.version ?? 'unversioned'}</code> · configuration <code>{body.analysisBinding.configurationSha256}</code> · result <code>{body.analysisBinding.analysisSha256}</code>.
          {' '}Source clock: {body.analysisBinding.asOfUtc ?? 'Not recorded in this older binding'}. Current form settings do not change this record.</p>}
        {immutable && <p className="boundary-note">This prediction is immutable. Use New simulation to review a different forecast without changing this record.</p>}
      </section>
      <section className="instrument-section" aria-labelledby="assumptions-title">
        <div><h4 id="assumptions-title">Uncertainty assumptions</h4><p>Review these alongside the quantified forecasts before sealing.</p></div>
        <ul>{body.uncertaintyAssumptions.map((assumption, index) => <li key={index}>{assumption}</li>)}</ul>
      </section>
      <details className="prediction-evidence">
        <summary>Cited evidence and full package identity ({body.citedEvidenceIds.length})</summary>
        <code>{body.fieldPackageSha256}</code>
        <ul>{body.citedEvidenceIds.map((id) => <li key={id}><code>{id}</code></li>)}</ul>
      </details>

      <section className="instrument-section baseline-section" aria-labelledby="baseline-title">
        <div><h4 id="baseline-title">Baselines</h4><p>{prediction.baselines.length} frozen comparison snapshot{prediction.baselines.length === 1 ? '' : 's'}.</p></div>
        {prediction.baselines.length ? (
          <div className="scenario-table-wrap">
            <table className="scenario-table">
              <thead><tr><th>Kind / stored model version</th><th>Candidate</th><th>Expected pay P90 / P50 / P10</th><th>Evidence</th><th>Stored binding / neighbors</th><th>Limitation</th></tr></thead>
              <tbody>{prediction.baselines.map((baseline) => (
                <tr key={baseline.baselineId}>
                  <th>{sentence(baseline.kind)}<br /><code>{baseline.modelVersion}</code></th>
                  <td>{baseline.candidateId}</td>
                  <td>{baseline.expectedPaydirtM ? `${baseline.expectedPaydirtM.p90.toFixed(1)} / ${baseline.expectedPaydirtM.p50.toFixed(1)} / ${baseline.expectedPaydirtM.p10.toFixed(1)} m` : 'Unavailable'}</td>
                  <td>{baseline.contributingEvidenceIds.length}</td>
                  <td>{baseline.analysisBinding
                    ? <><code>{baseline.analysisBinding.version}</code> · configuration IDW setting: {baseline.analysisBinding.configuration.idwNeighborCount}<br /><code>{baseline.analysisBinding.configurationSha256}</code></>
                    : 'Legacy unbound snapshot'}</td>
                  <td>{baseline.limitation}</td>
                </tr>
              ))}</tbody>
            </table>
          </div>
        ) : <p className="boundary-note">Baselines are created with the seal and are not yet available.</p>}
      </section>
      <footer className="scenario-foot">Scenario {scenario.seedLabel} · modified {formatUtc(prediction.modifiedUtc)}</footer>
    </div>
  )
}

function ExecutePane({ scenario, prediction, resources }: { scenario: Scenario; prediction?: PredictionRecord; resources: ScenarioResources }) {
  return (
    <div className="scenario-pane">
      <header className="scenario-pane-head">
        <div><h3>Execution state · {scenarioStatusLabel(scenario.status)}</h3><p>Checkpoint progress and the bound decision.</p></div>
        <div className="ledger-state"><strong>{lifecycleStatus('simulation', scenario, resources)}</strong><span>{formatUtc(scenario.asOfUtc)}</span></div>
      </header>
      <section className="instrument-section">
        <div><h4>Bound decision</h4><p>{prediction ? `${prediction.body.candidateId} · revision ${prediction.revision}` : 'No prediction ledger is published.'}</p></div>
        <dl className="dense-readout">
          <div><dt>SEAL</dt><dd><code>{shortHash(prediction?.seal?.sha256)}</code></dd></div>
          <div><dt>APPROVAL</dt><dd>{prediction?.approval ? `${prediction.approval.actor} · ${formatUtc(prediction.approval.approvedUtc)}` : 'Not approved'}</dd></div>
          <div><dt>SCENARIO STATE</dt><dd>{scenarioStatusLabel(scenario.status)}</dd></div>
          <div><dt>CLOCK</dt><dd>{formatUtc(scenario.asOfUtc)}</dd></div>
        </dl>
      </section>
      {!resources.operator?.enabled && !['Revealed', 'Scored'].includes(scenario.status) && <ScenarioEmpty title="LOCAL OPERATOR SETUP REQUIRED">
        {!prediction?.seal ? 'Save and seal a reviewed prediction in Prediction first. ' : !prediction.approval ? 'The sealed prediction requires human approval. ' : ''}
        Local operator controls require the server facade and a valid antiforgery session. Progress is not inferred from timestamps. Refresh operator status above after setup.
      </ScenarioEmpty>}
    </div>
  )
}

function visibilityValue(summary: EvidenceVisibilitySummary, kind: string, status: 'Visible' | 'Hidden') {
  return summary.counts.find((count) => count.recordKind === kind && count.status === status)?.count ?? 0
}

function VisibilityTable({ initial, current }: {
  initial: EvidenceVisibilitySummary
  current: EvidenceVisibilitySummary
}) {
  const kinds = [...new Set([...initial.counts, ...current.counts].map((count) => count.recordKind))]
  return (
    <div className="scenario-table-wrap">
      <table className="scenario-table visibility-table">
        <thead><tr><th>Record kind</th><th>T0 visible</th><th>T0 hidden</th><th>Current visible</th><th>Current hidden</th><th>Visible Δ</th></tr></thead>
        <tbody>{kinds.map((kind) => {
          const initialVisible = visibilityValue(initial, kind, 'Visible')
          const currentVisible = visibilityValue(current, kind, 'Visible')
          return (
            <tr key={kind}>
              <th>{sentence(kind)}</th>
              <td>{initialVisible}</td>
              <td>{visibilityValue(initial, kind, 'Hidden')}</td>
              <td>{currentVisible}</td>
              <td>{visibilityValue(current, kind, 'Hidden')}</td>
              <td>{currentVisible - initialVisible >= 0 ? '+' : ''}{currentVisible - initialVisible}</td>
            </tr>
          )
        })}</tbody>
      </table>
    </div>
  )
}

function RevealPane({ scenario, resources, fieldPackage }: { scenario: Scenario; resources: ScenarioResources; fieldPackage: FieldPackage }) {
  const published = resources.reveal?.status === 'Revealed'
  return (
    <div className="scenario-pane" data-tutorial="revealed-evidence">
      <header className="scenario-pane-head">
        <div><h3>Evidence reveal</h3><p>T0 inventory compared with the current public evidence clock. Hidden identifiers are never displayed.</p></div>
        <div className="ledger-state"><strong>{resources.reveal ? resources.reveal.status.toUpperCase() : 'NOT REVEALED'}</strong><span>{scenario.clonedFieldId ? 'Simulated results available' : 'Original evidence only'}</span></div>
      </header>
      {resources.evidence && (
        <section className="instrument-section">
          <div><h4>Visibility by record kind</h4><p>Original evidence compared with the current simulated-results field · counts only</p></div>
          <VisibilityTable initial={resources.evidence.initial} current={resources.evidence.current} />
        </section>
      )}
      {published && resources.reveal ? (
        <section className="instrument-section">
          <div><h4>Reveal receipt</h4><p>{resources.reveal.evidenceCount} public evidence records released.</p></div>
          <dl className="dense-readout">
            <div><dt>RESULTS FIELD ID</dt><dd><code>{resources.reveal.clonedFieldId}</code></dd></div>
            <div><dt>REVEALED AT</dt><dd>{formatUtc(resources.reveal.asOfUtc)}</dd></div>
            <div><dt>MANIFEST</dt><dd><code>{shortHash(resources.reveal.manifestSha256)}</code></dd></div>
            <div><dt>REVEAL ID</dt><dd><code>{resources.reveal.revealId}</code></dd></div>
          </dl>
        </section>
      ) : <ScenarioEmpty title={lifecycleStatus('new-evidence', scenario, resources)}>
        {scenario.status === 'ReadyToReveal'
          ? 'The simulation is ready. Review and publish it above to expose synthetic observations in a separate results field.'
          : 'A completed simulation and verified publication are required before a reveal receipt can be inspected.'}
        {' '}Use the local operator controls above when enabled. Opening this task does not publish evidence. Refresh the scenario after publication if its receipt has not appeared.
      </ScenarioEmpty>}
      {resources.production && resources.reveal && published ? (
        <ProductionPane key={resources.production.seriesId} fieldPackage={fieldPackage} scenario={scenario}
          reveal={resources.reveal} metadata={resources.production} prediction={resources.prediction} />
      ) : <ScenarioEmpty title="PRODUCTION METADATA PENDING">No public production-series metadata is available.</ScenarioEmpty>}
    </div>
  )
}

function MetricRow({ metric, headline }: { metric: ScorecardMetric; headline: boolean }) {
  return (
    <tr className={headline ? 'headline-metric' : undefined}>
      <th>{sentence(metric.name)}</th>
      <td><span className={`metric-status ${metric.status.toLowerCase()}`}>{metric.status}</span></td>
      <td>{metric.status === 'Scored' ? `${formatMetric(metric.value)} ${metric.unit}` : 'Unavailable'}</td>
      <td>{metric.lowerBound != null && metric.upperBound != null ? `${formatMetric(metric.lowerBound)}–${formatMetric(metric.upperBound)} ${metric.unit ?? ''}` : '—'}</td>
      <td>{metric.limitation ?? '—'}</td>
    </tr>
  )
}

function ScorePane({ scorecard }: { scorecard?: PublicScorecard }) {
  if (!scorecard) {
    return <ScenarioEmpty title="NO PUBLIC SCORECARD">Scoring has not published aggregate metrics. No unavailable value is inferred.</ScenarioEmpty>
  }
  const headlineName = scorecard.headlineMetric
  const expectedPay = scorecard.metrics.find((metric) =>
    metric.name.toLowerCase().includes('expected') && metric.name.toLowerCase().includes('pay') && metric.name.toLowerCase().includes('error'))
  const headline = expectedPay ?? scorecard.metrics.find((metric) => metric.name === headlineName)
  const rankingRegret = scorecard.metrics.find((metric) => metric.name.toLowerCase().includes('ranking') && metric.name.toLowerCase().includes('regret'))
  return (
    <div className="scenario-pane score-pane" data-tutorial="simulation-scorecard">
      <header className="scenario-pane-head">
        <div><h3>Aggregate scorecard</h3><p>No raw hidden truth or unavailable values are exposed.</p></div>
        <div className="ledger-state"><strong>SCORED</strong><span>{formatUtc(scorecard.createdValidTimeUtc)}</span></div>
      </header>
      {headline && (
        <section className="instrument-section score-headline">
          <div><h4>{sentence(headline.name)}</h4><p>Headline expected-pay error</p></div>
          <strong>{formatMetric(headline.value)} <small>{headline.unit}</small></strong>
          <span>{headline.status}{headline.lowerBound != null && headline.upperBound != null ? ` · bounds ${formatMetric(headline.lowerBound)}–${formatMetric(headline.upperBound)} ${headline.unit ?? ''}` : ''}</span>
        </section>
      )}
      {scoreBases.map((basis) => {
        const metrics = scorecard.metrics.filter((metric) => metric.basis === basis)
        if (!metrics.length) return null
        return (
          <section className="instrument-section" key={basis}>
            <div><h4>{sentence(basis)}</h4><p>Aggregate metrics only.</p></div>
            <div className="scenario-table-wrap">
              <table className="scenario-table">
                <thead><tr><th>Metric</th><th>Status</th><th>Value</th><th>Bounds</th><th>Limitation</th></tr></thead>
                <tbody>{metrics.map((metric) => <MetricRow key={`${basis}:${metric.name}`} metric={metric} headline={metric === headline} />)}</tbody>
              </table>
            </div>
          </section>
        )
      })}
      <p className="boundary-note">
        <strong>Ranking regret limitation:</strong>{' '}
        {rankingRegret?.status === 'Unavailable'
          ? rankingRegret.limitation
          : 'Only the published aggregate metric is shown; candidate-level hidden truth and inferred unavailable values remain inaccessible.'}
      </p>
      {scorecard.limitation && <p className="boundary-note">{scorecard.limitation}</p>}
      <footer className="scenario-foot">Model {scorecard.scoringModelVersion} · input <code>{shortHash(scorecard.inputSha256)}</code> · scorecard <code>{shortHash(scorecard.contentSha256)}</code></footer>
    </div>
  )
}

export function ScenarioSequenceWorkspace({
  taskId,
  scenario,
  resources,
  fieldPackage,
  analysis,
  candidate,
  onPredictionSaved,
  handoffBlock,
  onOperatorView,
  onOperatorChanged,
  onOperatorSetup,
  onPredictionPrepared,
  onNavigate,
  onNewSimulation,
}: {
  taskId: LifecycleTaskId
  scenario?: Scenario
  resources: ScenarioResources
  fieldPackage: FieldPackage
  analysis: AnalysisResult
  candidate?: RankedCandidate
  onPredictionSaved: (prediction: PredictionRecord) => void
  handoffBlock?: string
  onOperatorView: (view: OperatorView | undefined) => void
  onOperatorChanged: () => void
  onOperatorSetup?: (setup: SimulatorSetup | undefined) => void
  onPredictionPrepared?: (state: PredictionPreparationState) => void
  onNavigate?: (task: LifecycleTaskId) => void
  onNewSimulation?: () => void
}) {
  if (!scenario) {
    return <div className="scenario-empty"><strong>Start a new simulation</strong>
      <p>Use the setup form above to name a scenario and freeze the original evidence. Or choose an existing scenario in the header to resume its next step.</p>
      {onNewSimulation && <button type="button" onClick={onNewSimulation}>Open new simulation setup</button>}
    </div>
  }
  if (resources.scenarioId !== scenario.scenarioId || resources.loading && !resources.prediction && !resources.operator) return <ScenarioLoading />
  if (resources.error) return <div className="scenario-error" role="alert"><strong>SCENARIO SIGNAL LOST</strong><span>{resources.error}</span></div>
  let pane: React.ReactNode
  if (taskId === 'prediction') pane = <>
    {resources.prediction?.seal || resources.prediction?.approval || !['Draft', 'Armed', 'PredictionDrafted'].includes(scenario.status)
      ? <PredictionEditor key={scenario.scenarioId} scenario={scenario} fieldPackage={fieldPackage} prediction={resources.prediction} onSaved={onPredictionSaved} />
      : <PredictionPreparation key={scenario.scenarioId} scenario={scenario} fieldPackage={fieldPackage} analysis={analysis}
        prediction={resources.prediction} candidate={candidate} onSaved={onPredictionSaved} handoffBlock={handoffBlock} onPrepared={onPredictionPrepared} />}
    <PredictionPane prediction={resources.prediction} scenario={scenario} />
  </>
  else if (taskId === 'simulation') pane = <ExecutePane scenario={scenario} prediction={resources.prediction} resources={resources} />
  else if (taskId === 'new-evidence') pane = <RevealPane scenario={scenario} resources={resources} fieldPackage={fieldPackage} />
  else if (!resources.scorecard) pane = <ScenarioEmpty title={lifecycleStatus('evaluation', scenario, resources)}>
    {resources.reveal?.status === 'Revealed'
      ? 'No aggregate scorecard has been published. Use the local operator evaluation action when enabled; refresh the scenario after publication.'
      : 'Publish a verified reveal through the operator workflow before prediction-versus-outcome evaluation is available.'}
  </ScenarioEmpty>
  else pane = <ScorePane scorecard={resources.scorecard} />
  return <>
    {onNavigate && <nav className="simulation-journey" aria-label="Simulation workflow">
      {(['prediction', 'simulation', 'new-evidence', 'evaluation'] as const).map((id, index) => <button type="button" key={id}
        aria-current={id === taskId ? 'step' : undefined} onClick={() => onNavigate(id)}>
        {index + 1}. {['Prediction & approval', 'Setup & run', 'Publish & inspect', 'Evaluation'][index]}
        <span>{id === 'prediction' && resources.prediction?.approval ? 'Approved' : lifecycleStatus(id, scenario, resources)}</span>
      </button>)}
    </nav>}
    {['Revealed', 'Scored'].includes(scenario.status) && <section className="simulation-complete" data-tutorial="simulation-results">
      <h3>{scenario.status === 'Scored' ? 'Simulation and evaluation complete' : 'Simulation results published'}</h3>
      <p>This scenario is preserved as a completed result, not a run to restart. Inspect its measurements and scorecard, or create a new simulation from the original field.</p>
      <div className="task-actions">
        {onNavigate && <><button type="button" onClick={() => onNavigate('new-evidence')}>Inspect new evidence</button>
          <button type="button" onClick={() => onNavigate('evaluation')}>Inspect scorecard</button></>}
        {onNewSimulation && <button type="button" data-tutorial="new-simulation-from-results" onClick={onNewSimulation}>New simulation</button>}
      </div>
    </section>}
    {resources.loading && <p role="status">Refreshing the scenario ledger…</p>}
    {taskId === 'prediction' && pane}
    <OperatorControls key={scenario.scenarioId} scenario={scenario} resources={resources} taskId={taskId}
      onView={onOperatorView} onSetup={onOperatorSetup} onArtifactsChanged={onOperatorChanged} onNavigate={onNavigate} />
    {taskId !== 'prediction' && pane}
  </>
}
