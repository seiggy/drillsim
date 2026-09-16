import { useMemo, useState } from 'react'
import { configurationErrors, createEvidenceBundle, defaultConfiguration, downloadJson, screeningControlLimitation } from '../analysisConfiguration'
import { classificationLabel, read } from '../data'
import { columnTotals, curveInventory, describe, record } from '../evidence'
import { fluidLabel, gapLabel, qualityLabel } from '../fluidColumn'
import { positionAtMd } from '../reservoirGeometry'
import type { TaskId } from '../sequencer'
import type { AnalysisConfiguration, AnalysisResult, FieldPackage, RankedCandidate, Scenario } from '../types'
import type { WellView } from './AnalysisWorkspace'
import { DataSources } from './DataSources'
import { JsonDownload } from './JsonDownload'

const number = (value: number | null | undefined, digits = 1) =>
  value == null || !Number.isFinite(value) ? 'Unavailable' : value.toLocaleString(undefined, { maximumFractionDigits: digits })

function Table({ label, children }: { label: string; children: React.ReactNode }) {
  return <div className="task-table-scroll" tabIndex={0} role="region" aria-label={label}>
    <table className="scenario-table">{children}</table>
  </div>
}
function Notice({ children }: { children: React.ReactNode }) {
  return <p className="task-notice">{children}</p>
}

export interface AnalysisControls {
  draft: AnalysisConfiguration
  applying: boolean
  error: string
  message: string
  stale: boolean
  onDraft: (config: AnalysisConfiguration) => void
  onApply: () => void
}

function ConfigurationForm({ taskId, analysis, controls }: { taskId: TaskId; analysis: AnalysisResult; controls: AnalysisControls }) {
  const { draft, applying, error, message, stale, onDraft, onApply } = controls
  const errors = configurationErrors(draft)
  const fields: Array<{ key: keyof Omit<AnalysisConfiguration, 'version'>; label: string; min: number; max: number; step: string; task: TaskId }> = [
    { key: 'porosityCutoff', label: 'Minimum porosity · fraction', min: 0, max: 1, step: 'any', task: 'criteria' },
    { key: 'permeabilityCutoffM2', label: 'Minimum permeability · m²', min: 0, max: 1e-8, step: 'any', task: 'criteria' },
    { key: 'gridPointsPerAxis', label: 'Grid points per axis · 2–51', min: 2, max: 51, step: '1', task: 'search' },
    { key: 'wellExclusionRadiusM', label: 'Screening-control exclusion radius · m', min: 0, max: 100000, step: 'any', task: 'exclusions' },
    { key: 'idwNeighborCount', label: 'Ranking IDW neighbors · 1–32', min: 1, max: 32, step: '1', task: 'model' },
  ]
  return <form className="analysis-settings" data-tutorial="analysis-settings" onSubmit={(event) => { event.preventDefault(); if (!errors.length && !applying) onApply() }}>
    <fieldset disabled={applying}>
      <legend>Analysis settings · {draft.version}</legend>
      <div className="task-controls">
        {fields.filter((field) => field.task === taskId).map((field) => <label key={field.key}>
          {field.label}
          <input type="number" required min={field.min} max={field.max} step={field.step}
            data-tutorial={`setting-${field.key}`}
            value={Number.isFinite(draft[field.key]) ? draft[field.key] : ''}
            onChange={(event) => onDraft({ ...draft, [field.key]: event.target.value === '' ? Number.NaN : Number(event.target.value) })} />
        </label>)}
      </div>
      <p>Pending settings across all tasks: porosity {number(draft.porosityCutoff, 3)}, permeability {Number.isFinite(draft.permeabilityCutoffM2) ? draft.permeabilityCutoffM2.toExponential(4) : 'missing'} m²;
        {' '}{number(draft.gridPointsPerAxis, 0)} × {number(draft.gridPointsPerAxis, 0)} grid; {number(draft.wellExclusionRadiusM)} m control screening; {number(draft.idwNeighborCount, 0)} neighbors.</p>
      <div className="task-actions">
        <button type="submit" disabled={Boolean(errors.length)}>{applying ? 'Applying analysis…' : 'Apply analysis settings'}</button>
        <button type="button" onClick={() => onDraft({ ...(analysis.configuration ?? defaultConfiguration) })}>Restore result settings</button>
        <button type="button" onClick={() => onDraft({ ...defaultConfiguration })}>Load defaults into form</button>
      </div>
    </fieldset>
    {errors.length > 0 && <ul className="task-errors" role="alert">{errors.map((item) => <li key={item}>{item}</li>)}</ul>}
    {error && <p className="task-errors" role="alert">{error} Previous results have not been relabeled. Correct settings or refresh evidence, then retry Apply.</p>}
    <p role="status">{applying ? 'Calculating against the visible package…' : stale ? 'Pending edits: displayed results still use their prior applied settings.' : message || 'The form matches this result. Apply is an explicit recalculation, never an AI request.'}</p>
  </form>
}

function EvidenceInventory({ pkg, wells, onWell, onRefresh, scenario }: {
  pkg: FieldPackage; wells: WellView[]; onWell: (id: string) => void; onRefresh: () => void; scenario?: Scenario
}) {
  const curves = useMemo(() => curveInventory(pkg), [pkg])
  const loggedBores = new Set(curves.map((curve) => curve.boreId))
  return <>
    <div className="task-actions"><button type="button" onClick={onRefresh}>Refresh visible evidence</button></div>
    <p>{scenario ? `Scenario ${scenario.seedLabel}, as of ${scenario.asOfUtc}.` : 'Live service snapshot; no scenario clock restriction.'}
      {' '}This is a loaded inventory, not a raw-data import pipeline.</p>
    <dl className="dense-readout">
      <div><dt>PARENT WELLS / BORES</dt><dd>{pkg.wells.length} / {pkg.wellBores.length}</dd></div>
      <div><dt>BORES WITH CURVES</dt><dd>{loggedBores.size}</dd></div>
      <div><dt>CURVES / VALUES</dt><dd>{curves.length} / {curves.reduce((sum, curve) => sum + curve.count, 0).toLocaleString()}</dd></div>
      <div><dt>USABLE / MISSING VALUES</dt><dd>{curves.reduce((sum, curve) => sum + curve.usable, 0).toLocaleString()} / {curves.reduce((sum, curve) => sum + curve.missing, 0).toLocaleString()}</dd></div>
    </dl>
    <Table label="Curve coverage by bore">
      <caption>All visible log runs · curve counts do not imply reservoir coverage</caption>
      <thead><tr><th scope="col">Bore</th><th scope="col">Runs</th><th scope="col">Curves</th><th scope="col">Usable / total values</th><th scope="col">Source</th></tr></thead>
      <tbody>{wells.map((well) => {
        const local = curves.filter((curve) => curve.boreId === well.id)
        return <tr key={well.id}><th scope="row"><button type="button" onClick={() => onWell(well.id)}>{well.name}</button></th>
          <td>{new Set(local.map((curve) => curve.run)).size}</td><td>{local.length}</td>
          <td>{local.reduce((sum, curve) => sum + curve.usable, 0)} / {local.reduce((sum, curve) => sum + curve.count, 0)}</td><td>{well.sourceLabel}</td></tr>
      })}</tbody>
    </Table>
    <DataSources pkg={pkg} onWell={onWell} />
    <Notice>{pkg.dataGaps.length ? `${pkg.dataGaps.length} package data gaps are reported below.` : 'No service gaps were reported by this package. This is not a completeness certification.'}</Notice>
    {pkg.dataGaps.length > 0 && <ul>{pkg.dataGaps.map((gap, index) => <li key={index}>{gap}</li>)}</ul>}
    <p>Snapshot generated {pkg.generatedAt} · package <code>{pkg.sha256}</code></p>
  </>
}

function QualityWorkspace({ pkg, wells, onWell }: { pkg: FieldPackage; wells: WellView[]; onWell: (id: string) => void }) {
  const curves = useMemo(() => curveInventory(pkg), [pkg])
  return <>
    <Notice>Missing, non-finite, null-flagged and missing/invalid/bad-hole values are counted as missing, never zero.
      Flags can overlap; missingness is their union. This inspector performs no source corrections or saved QC acknowledgement.</Notice>
    <p>Units are reported as supplied. The visual rock/fluid tracks accept PHIE/SW/SO/SG in fraction or m³/m³ and PERM in m².
      Bounded formation columns require positive-down measured depth in metres. All runs are inventoried; the current visual column reads the first run.</p>
    {curves.length ? <Table label="Curve units, null flags and missingness">
      <caption>Curve-level QC · inspect raw metadata for source transforms and depth references</caption>
      <thead><tr><th scope="col">Bore / curve</th><th scope="col">Source → canonical unit</th><th scope="col">Depth reference / unit / positive-down</th><th scope="col">Missing / total</th><th scope="col">Null flags</th><th scope="col">Quality-excluded</th><th scope="col">Valid zeros</th><th scope="col">Details</th></tr></thead>
      <tbody>{curves.map((curve) => <tr key={curve.id}>
        <th scope="row"><button type="button" onClick={() => onWell(curve.boreId)}>{wells.find((well) => well.id === curve.boreId)?.name ?? curve.boreId}</button><br />{curve.mnemonic}</th>
        <td>{curve.sourceUnit} → {curve.canonicalUnit}</td>
        <td>{curve.depthReference} / {curve.depthUnit} / {curve.depthPositiveDown}<br />Datum: {curve.depthDatum}</td>
        <td>{curve.missing} / {curve.count} ({curve.count ? (100 * curve.missing / curve.count).toFixed(1) : '—'}%)</td>
        <td>{curve.nullFlagged}</td><td>{curve.qualityFlagged}</td><td>{curve.zeroValues}</td>
        <td className="curve-details-cell">{curve.lengthMismatch && <strong>Depth/value length mismatch. </strong>}
          <details><summary>Curve details</summary>
            <dl className="metadata-readout">
              <dt>Original curve name</dt><dd>{curve.sourceMnemonic}</dd>
              <dt>Log run</dt><dd>{curve.runName}</dd>
              <dt>Tool</dt><dd>{curve.tool}</dd>
              <dt>Data type</dt><dd>{classificationLabel(read(curve.curveMetadata, 'Classification', 'classification'))}</dd>
              <dt>Original missing-value code</dt><dd>{describe(read(curve.curveMetadata, 'OriginalNullValue', 'originalNullValue'))}</dd>
              <dt>Geology record</dt><dd><code>{curve.geologyEvidenceId}</code></dd>
            </dl>
            <JsonDownload value={curve.curveMetadata} filename={`curve-${curve.id.replace(/:/g, '-')}-metadata.json`}>
              Download curve metadata JSON
            </JsonDownload>
          </details></td>
      </tr>)}</tbody>
    </Table> : <Notice>No log curves are published in this package. Missing curves are not zero measurements.</Notice>}
  </>
}

function PositionWorkspace({ pkg, wells, selectedWellId, onWell }: { pkg: FieldPackage; wells: WellView[]; selectedWellId: string; onWell: (id: string) => void }) {
  const reference = record(read(pkg.field, 'ReferencePoint', 'referencePoint'))
  const coordinates = [
    ['Northing · m', read(reference, 'RiemannianNorth', 'riemannianNorth', 'X', 'x')],
    ['Easting · m', read(reference, 'RiemannianEast', 'riemannianEast', 'Y', 'y')],
    ['Vertical depth · m', read(reference, 'TVD', 'tvd', 'Z', 'z')],
    ['Latitude · radians', read(reference, 'Latitude', 'latitude')],
    ['Longitude · radians', read(reference, 'Longitude', 'longitude')],
  ] as const
  return <>
    <Notice>Surface markers and downhole positions are different evidence. No survey is replaced with a wellhead or guessed coordinate transform.
      Screening buffers are not anti-collision certification. Select a bore to synchronize its lens.</Notice>
    <details><summary>Field coordinate reference</summary>
      {!reference ? <p>No field reference point has been supplied.</p> : <>
        <p>Coordinates supplied for the field reference point. A common depth datum must be verified separately.</p>
        <dl className="metadata-readout">{coordinates.map(([label, value]) =>
          <div key={label}><dt>{label}</dt><dd>{typeof value === 'number' && Number.isFinite(value) ? String(value)
            : value == null ? 'Not supplied' : 'Invalid coordinate value'}</dd></div>)}</dl>
      </>}
      <JsonDownload value={read(pkg.field, 'ReferencePoint', 'referencePoint') ?? null} filename={`field-${pkg.fieldId}-reference.json`}>
        Download coordinate metadata JSON
      </JsonDownload>
    </details>
    <Table label="Position diagnostics by wellbore">
      <caption>Formation midpoint located only when covered by a validated survey · local metres</caption>
      <thead><tr><th scope="col">Bore</th><th scope="col">Validated stations</th><th scope="col">Formation midpoint E / N / TVD</th><th scope="col">Diagnostics</th></tr></thead>
      <tbody>{wells.map((well) => {
        const bounds = well.formationRanges[0]
        const point = bounds ? positionAtMd(well.stations, (bounds.top + bounds.base) / 2) : undefined
        return <tr key={well.id} data-selected={well.id === selectedWellId}><th scope="row"><button type="button" aria-pressed={well.id === selectedWellId} onClick={() => onWell(well.id)}>{well.name}</button></th>
          <td>{well.stations.length}</td><td>{point ? `${number(point.east)} / ${number(point.north)} / ${number(point.tvd)}` : 'No covering location'}</td>
          <td>{well.geometryWarnings.length ? well.geometryWarnings.join(' ') : 'No coordinate rejection reported by the current visual method.'}</td></tr>
      })}</tbody>
    </Table>
  </>
}

function PayWorkspace({ wells, analysis, selectedWellId, onWell }: { wells: WellView[]; analysis: AnalysisResult; selectedWellId: string; onWell: (id: string) => void }) {
  const selected = wells.find((well) => well.id === selectedWellId)
  return <>
    <Notice>Rock quality is not hydrocarbon pay. API “netPay” is a qualifying-rock screening proxy; water-bearing rock can qualify.
      The separate column calculation sums sample-supported MD intervals and reports fluid class independently. Oil/gas-classified rock is not economic pay, reserves or a production forecast.</Notice>
    <Table label="Per-bore interval thickness">
      <caption>Visual column · m MD · select a bore to see its contributing intervals below</caption>
      <thead><tr><th scope="col">Bore</th><th scope="col">Gross</th><th scope="col">Qualifying rock</th><th scope="col">Below cutoff</th><th scope="col">Unknown quality</th><th scope="col">Qualifying oil/gas-classified</th><th scope="col">Qualifying water</th><th scope="col">Qualifying unknown fluid</th></tr></thead>
      <tbody>{wells.map((well) => {
        const totals = columnTotals(well)
        return <tr key={well.id} data-selected={well.id === selectedWellId}><th scope="row"><button type="button" aria-pressed={well.id === selectedWellId} onClick={() => onWell(well.id)}>{well.name}</button></th>
          {(['gross', 'qualifying', 'belowCutoff', 'unknownQuality', 'qualifyingOilGas', 'qualifyingWater', 'qualifyingUnknownFluid'] as const).map((key) => <td key={key}>{number(totals?.[key])}</td>)}</tr>
      })}</tbody>
    </Table>
    <p>Gross = qualifying + below-cutoff + unknown quality. Qualifying = oil/gas-classified + water + unknown fluid.
      Column coverage uses sample midpoints and leaves large gaps unknown; it is not the API sample-thickness algorithm. Distinct formation occurrences remain separate.</p>
    {selected && <Table label="Contributing intervals in selected bore">
      <caption>{selected.name} · {selected.column.spacingM ? `${number(selected.column.spacingM, 2)} m nominal spacing` : 'Sampling coverage unavailable'}</caption>
      <thead><tr><th scope="col">Top–base m MD</th><th scope="col">Thickness m</th><th scope="col">Rock quality</th><th scope="col">Fluid</th><th scope="col">Reason</th></tr></thead>
      <tbody>{selected.column.intervals.map((interval) => <tr key={interval.top}>
        <th scope="row">{number(interval.top)}–{number(interval.base)}</th><td>{number(interval.base - interval.top)}</td><td>{qualityLabel(interval.quality)}</td><td>{fluidLabel(interval.fluid)}</td><td>{gapLabel(interval.reason)}</td>
      </tr>)}</tbody>
    </Table>}
    <details><summary>API calculation by geology record</summary>
      <p>{analysis.methodology?.netPayThicknessMethod ?? 'The service has not supplied its calculation method.'} Pressure rule: {analysis.methodology?.pressureDifferentialRule ?? 'Not supplied'}.</p>
      <Table label="API qualifying-rock calculations">
        <thead><tr><th scope="col">Well evidence</th><th scope="col">Geology evidence</th><th scope="col">Qualifying samples</th><th scope="col">Rock proxy m</th><th scope="col">Mean porosity</th><th scope="col">Mean permeability m²</th></tr></thead>
        <tbody>{analysis.wellSummaries.flatMap((well) => (well.geologicalResults ?? []).map((result) =>
          <tr key={result.evidenceId}><th scope="row"><code>{well.wellEvidenceId}</code></th><td><code>{result.evidenceId}</code></td><td>{result.qualifyingSampleCount}</td><td>{number(result.netPayThicknessM)}</td><td>{number(result.meanPayPorosity, 4)}</td><td>{result.meanPayPermeabilityM2.toExponential(4)}</td></tr>))}</tbody>
      </Table>
      {!analysis.wellSummaries.some((well) => well.geologicalResults?.length) && <Notice>No per-record API calculations are published. The parent-well aggregate is not copied onto every bore.</Notice>}
    </details>
  </>
}

function GridWorkspace({ analysis }: { analysis: AnalysisResult }) {
  const [status, setStatus] = useState('')
  const [page, setPage] = useState(0)
  const points = analysis.candidateGrid ?? []
  const counts = points.reduce<Record<string, number>>((result, point) => ({ ...result, [point.status]: (result[point.status] ?? 0) + 1 }), {})
  const filtered = points.filter((point) => !status || point.status === status)
  const currentPage = Math.min(page, Math.max(0, Math.ceil(filtered.length / 100) - 1))
  const bounds = analysis.candidateGridBounds
  if (!points.length) return <Notice>No full candidate-grid output exists in this result. Apply settings to request a versioned grid; no cells are invented.</Notice>
  return <>
    <p>{points.length} candidate cells before screening. {Object.entries(counts).map(([key, count]) => `${key}: ${count}`).join(' · ')}.</p>
    <Notice>{screeningControlLimitation}</Notice>
    <p>{bounds ? `Computed local-grid bounds: E ${number(bounds.minEastingM)}–${number(bounds.maxEastingM)} m; N ${number(bounds.minNorthingM)}–${number(bounds.maxNorthingM)} m.` : 'Grid bounds not supplied.'}
      {' '}Custom boundary/extent editing is not supported; the API derives this bounded search area from evidence. The map lens shows the retained shortlist; the full grid and exclusions are tabulated here.</p>
    <div className="task-controls"><label>Cell status<select value={status} onChange={(event) => { setStatus(event.target.value); setPage(0) }}>
      <option value="">All statuses</option>{Object.keys(counts).map((key) => <option key={key}>{key}</option>)}
    </select></label></div>
    <Table label="Full candidate grid and exclusion reasons">
      <thead><tr><th scope="col">Cell</th><th scope="col">E / N · m</th><th scope="col">Status</th><th scope="col">Reasons</th><th scope="col">Nearest located screening control · m</th><th scope="col">P50 rock proxy · m</th></tr></thead>
      <tbody>{filtered.slice(currentPage * 100, (currentPage + 1) * 100).map((point) => <tr key={point.candidateId}>
        <th scope="row">{point.candidateId}</th><td>{number(point.eastingM)} / {number(point.northingM)}</td><td>{point.status}</td>
        <td>{point.reasons.join(' · ') || 'No reason supplied'}</td><td>{number(point.nearestWellDistanceM)}</td><td>{number(point.prediction?.p50NetPayM)}</td>
      </tr>)}</tbody>
    </Table>
    <div className="task-actions">
      <button type="button" disabled={currentPage === 0} onClick={() => setPage(currentPage - 1)}>Previous cells</button>
      <span>Page {currentPage + 1} of {Math.max(1, Math.ceil(filtered.length / 100))} · {filtered.length} matching cells</span>
      <button type="button" disabled={(currentPage + 1) * 100 >= filtered.length} onClick={() => setPage(currentPage + 1)}>Next cells</button>
    </div>
  </>
}

function TargetDetails({ candidate, analysis }: { candidate?: RankedCandidate; analysis: AnalysisResult }) {
  if (!candidate) return <Notice>No retained target is available. Inspect grid reasons and input evidence before applying different settings.</Notice>
  const score = candidate.scoreComponents
  return <>
    <h4>Score explanation · {candidate.candidateId}</h4>
    <p>{analysis.methodology?.scoreFormula ?? 'Score formula was not included in this result.'}</p>
    {score ? <dl className="dense-readout">
      <div><dt>P50 ROCK PROXY × POROSITY</dt><dd>{number(score.p50NetPayM)} × {number(score.porosityFactor, 4)}</dd></div>
      <div><dt>PERM · mD / LOG FACTOR</dt><dd>{number(score.permeabilityMd)} / {number(score.permeabilityLogFactor, 4)}</dd></div>
      <div><dt>RAW SCORE / UNCERTAINTY DIVISOR</dt><dd>{number(score.unpenalizedScore, 4)} / {number(score.uncertaintyDivisor, 4)}</dd></div>
      <div><dt>FINAL SCORE</dt><dd>{number(candidate.score, 4)}</dd></div>
    </dl> : <Notice>Quantitative score components were not published for this target. Apply analysis settings to request the current contract.</Notice>}
    <p>Screening score is not economic value; fluid/economic contributions are not modeled.</p>
  </>
}

function TargetsWorkspace({ analysis, candidate, onCandidate }: { analysis: AnalysisResult; candidate?: RankedCandidate; onCandidate: (candidate: RankedCandidate) => void }) {
  const [sort, setSort] = useState<'rank' | 'p50NetPayM' | 'score' | 'relativeUncertainty' | 'nearestWellDistanceM'>('rank')
  const [ascending, setAscending] = useState(true)
  const ranked = [...analysis.ranking].sort((left, right) => (left[sort] - right[sort]) * (ascending ? 1 : -1))
  return <>
    <div className="task-controls"><label>Sort shortlist<select value={sort} onChange={(event) => setSort(event.target.value as typeof sort)}>
      <option value="rank">API rank</option><option value="p50NetPayM">P50 rock proxy</option><option value="score">Score</option><option value="relativeUncertainty">Relative uncertainty</option><option value="nearestWellDistanceM">Nearest located screening control</option>
    </select></label><label>Order<select value={ascending ? 'ascending' : 'descending'} onChange={(event) => setAscending(event.target.value === 'ascending')}>
      <option value="ascending">Ascending</option><option value="descending">Descending</option>
    </select></label></div>
    <Notice>{screeningControlLimitation}</Notice>
    <Table label="Ranked target shortlist">
      <caption>Retained targets · uncalibrated P90 / P50 / P10 rock-screening thickness, not hydrocarbon reserves</caption>
      <thead><tr><th scope="col">API rank / target</th><th scope="col">P90 · m</th><th scope="col">P50 · m</th><th scope="col">P10 · m</th><th scope="col">Nearest located screening control · m</th><th scope="col">Relative uncertainty</th><th scope="col">Score</th></tr></thead>
      <tbody>{ranked.map((item) => <tr key={item.candidateId} data-selected={item.candidateId === candidate?.candidateId}>
        <th scope="row"><button type="button" aria-pressed={item.candidateId === candidate?.candidateId} onClick={() => onCandidate(item)}>#{item.rank} · {item.candidateId}</button></th>
        <td>{number(item.p90NetPayM)}</td><td>{number(item.p50NetPayM)}</td><td>{number(item.p10NetPayM)}</td><td>{number(item.nearestWellDistanceM)}</td><td>{number(item.relativeUncertainty * 100)}%</td><td>{number(item.score, 4)}</td>
      </tr>)}</tbody>
    </Table>
    <TargetDetails candidate={candidate} analysis={analysis} />
  </>
}

function UncertaintyWorkspace({ candidate, analysis }: { candidate?: RankedCandidate; analysis: AnalysisResult }) {
  if (!candidate) return <Notice>Select a retained target in Targets. No target quantiles are available.</Notice>
  const components = candidate.uncertaintyComponents
  return <>
    <Notice>Uncalibrated screening uncertainty. P90 is the low estimate; P10 is the high estimate.
      These ranges are not empirically calibrated coverage probabilities, a reserve distribution or a sensitivity study.</Notice>
    <dl className="quantile-strip">
      <div><dt>P90 ROCK PROXY</dt><dd>{number(candidate.p90NetPayM)} m</dd></div>
      <div><dt>P50 ROCK PROXY</dt><dd>{number(candidate.p50NetPayM)} m</dd></div>
      <div><dt>P10 ROCK PROXY</dt><dd>{number(candidate.p10NetPayM)} m</dd></div>
    </dl>
    <p>{analysis.methodology?.uncertaintyMethod ?? 'The uncertainty method was not supplied.'}</p>
    {components ? <Table label="Uncertainty components">
      <thead><tr><th scope="col">Component</th><th scope="col">Published value</th></tr></thead>
      <tbody>
        <tr><th scope="row">Neighbor disagreement variance</th><td>{number(components.disagreementVarianceM2, 4)} m²</td></tr>
        <tr><th scope="row">Weighted control distance</th><td>{number(components.weightedDistanceM)} m</td></tr>
        <tr><th scope="row">Grid diagonal</th><td>{number(components.gridDiagonalM)} m</td></tr>
        <tr><th scope="row">Distance sigma</th><td>{number(components.distanceSigmaM, 4)} m</td></tr>
        <tr><th scope="row">Total sigma</th><td>{number(components.sigmaNetPayM, 4)} m</td></tr>
        <tr><th scope="row">Quantile z-score</th><td>{number(components.quantileZScore, 6)}</td></tr>
        <tr><th scope="row">Calibrated</th><td>{components.calibrated ? 'Reported calibrated by service' : 'No'}</td></tr>
      </tbody>
    </Table> : <Notice>Component breakdown is unavailable in this result. Apply settings to request the current version.</Notice>}
    <p>Missing-evidence effects are not separately quantified. Inspect Units & QC and the data gaps below; changing criteria manually is not a persisted sensitivity analysis.</p>
    <ul>{analysis.dataGaps.map((gap, index) => <li key={index}>{gap}</li>)}</ul>
  </>
}

export function TaskWorkspace({ taskId, pkg, analysis, wells, selectedWellId, onWell, candidate, onCandidate, controls, scenario, onRefresh, rationale, onRationale }: {
  taskId: TaskId; pkg: FieldPackage; analysis: AnalysisResult; wells: WellView[]; selectedWellId: string; onWell: (id: string) => void
  candidate?: RankedCandidate; onCandidate: (candidate: RankedCandidate) => void; controls: AnalysisControls
  scenario?: Scenario; onRefresh: () => void; rationale: string; onRationale: (value: string) => void
}) {
  const [exportError, setExportError] = useState('')
  if (taskId === 'evidence') return <EvidenceInventory pkg={pkg} wells={wells} onWell={onWell} onRefresh={onRefresh} scenario={scenario} />
  if (taskId === 'qc') return <QualityWorkspace pkg={pkg} wells={wells} onWell={onWell} />
  if (taskId === 'position') return <PositionWorkspace pkg={pkg} wells={wells} selectedWellId={selectedWellId} onWell={onWell} />
  if (taskId === 'pay') return <PayWorkspace wells={wells} analysis={analysis} selectedWellId={selectedWellId} onWell={onWell} />
  if (taskId === 'targets') return <TargetsWorkspace analysis={analysis} candidate={candidate} onCandidate={onCandidate} />
  if (taskId === 'uncertainty') return <UncertaintyWorkspace analysis={analysis} candidate={candidate} />
  if (['criteria', 'search', 'exclusions', 'model'].includes(taskId)) return <>
    <ConfigurationForm taskId={taskId} analysis={analysis} controls={controls} />
    {taskId === 'criteria' && <>
      <Notice>These are rock-screening criteria. They do not establish fluid presence or economic hydrocarbon pay.
        Apply updates API calculations and the rock thresholds in the visual lenses; unsaved form values do not change the plots.</Notice>
      <p>Pressure rule: {analysis.methodology?.pressureDifferentialRule ?? 'Not supplied'}. Thickness method: {analysis.methodology?.netPayThicknessMethod ?? 'Not supplied'}.</p>
    </>}
    {['search', 'exclusions'].includes(taskId) && <GridWorkspace analysis={analysis} />}
    {taskId === 'exclusions' && <Notice>This screening radius is not a lease constraint, all-well separation check or drilling safety clearance.</Notice>}
    {taskId === 'model' && <Notice>Ranking IDW neighbors above is an analytical setting requiring Apply.
      The 3D lens uses a separate finite-radius, triangulated visible-interval method. Its mesh search radius, grid resolution, rotation, tilt and cutting are view-only geometry settings and do not recalculate API rankings or dirty this analysis.</Notice>}
  </>
  if (taskId === 'bundle') {
    let bundle: ReturnType<typeof createEvidenceBundle> | undefined
    let blocker = ''
    try { bundle = createEvidenceBundle(pkg, analysis, candidate, analysis.reservoirName ?? '', controls.draft, rationale, scenario) }
    catch (error) { blocker = error instanceof Error ? error.message : String(error) }
    return <>
      <Notice>A bundle is a local decision export. It does not save a hypothesis, seal a prediction, publish simulator evidence or contain hidden truth.
        Only the current matching public package and actual result fingerprints can be bundled.</Notice>
      <label className="rationale-field">Decision rationale (included in JSON)<textarea rows={4} value={rationale} onChange={(event) => { onRationale(event.target.value); setExportError('') }} /></label>
      <dl className="dense-readout">
        <div><dt>PACKAGE</dt><dd><code>{pkg.sha256}</code></dd></div>
        <div><dt>CONFIGURATION</dt><dd><code>{analysis.configurationSha256 || 'Unavailable'}</code></dd></div>
        <div><dt>ANALYSIS RESULT</dt><dd><code>{analysis.analysisSha256 || 'Unavailable'}</code></dd></div>
        <div><dt>TARGET / CITATIONS</dt><dd>{candidate?.candidateId ?? 'None'} / {bundle?.evidence.citedEvidenceIds.length ?? 'Not ready'}</dd></div>
      </dl>
      <div className="task-actions"><button type="button" disabled={!bundle || controls.applying || controls.stale} onClick={() => {
        if (!bundle) return
        try { downloadJson(bundle, `analysis-${pkg.fieldId}-${analysis.analysisSha256?.slice(0, 12)}.json`); setExportError('') }
        catch (error) { setExportError(String(error)) }
      }}>Download evidence bundle JSON</button></div>
      {blocker && <p role="status">{blocker}</p>}
      {exportError && <p role="alert">{exportError}</p>}
      {bundle && <details><summary>What the download contains</summary>
        <ul>
          <li>The selected field and reservoir{scenario ? ', scenario and simulated date' : ''}.</li>
          <li>Record counts and {bundle.evidence.citedEvidenceIds.length} supporting evidence references.</li>
          <li>Applied rock cutoffs, grid size, screening radius and neighbor count.</li>
          <li>The selected target, estimated thickness, uncertainty and score components.</li>
          <li>Your rationale, method descriptions and reported data limitations.</li>
        </ul>
      </details>}
    </>
  }
  return null
}
