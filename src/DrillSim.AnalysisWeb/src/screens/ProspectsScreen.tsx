import { useMemo, useState } from 'react'
import { AnalysisWorkspace, buildWellViews, type WorkspaceMode } from '../components/AnalysisWorkspace'
import { ProspectBriefPanel } from '../components/ProspectBriefPanel'
import { insideCoverage, type SimulatorCoverage } from '../prospects'
import { contactCode, contactLabel, type StaticReservoirModel } from '../reservoirModel'
import type { AppSettings } from '../settings'
import { metres } from '../structure'
import type { AnalysisResult, FieldPackage, MapsStatus, QuantileValues, RankedCandidate, TargetFluid } from '../types'
import { payLabel } from '../analysisConfiguration'
import { useAssistantContext } from '../assistantContext'

export function shortlist(analysis: AnalysisResult, coverage?: SimulatorCoverage) {
  const eligible = (analysis.candidateGrid ?? []).flatMap(cell => cell.status === 'eligible' && cell.prediction ? [cell.prediction] : [])
    // With a target fluid, points the model puts entirely outside that fluid have no pay to forecast.
    .filter(item => item.targetFluidFraction !== 0)
  const sorted = (eligible.length ? eligible : analysis.ranking).slice().sort((a, b) => b.score - a.score)
  const drillable = sorted.filter(item => insideCoverage(item, coverage))
  return {
    targets: drillable.slice(0, 5).map((item, index) => ({ ...item, rank: index + 1 })),
    hiddenAbove: sorted.indexOf(drillable[0]) > 0 ? sorted.indexOf(drillable[0]) : 0,
    outside: sorted.length - drillable.length,
    scored: sorted.length,
  }
}

export function RangeBar({ value, max, unit = 'm' }: { value: QuantileValues; max: number; unit?: string }) {
  const pct = (number: number) => `${Math.max(0, Math.min(100, number / max * 100))}%`
  return <div className="range-bar" role="img" aria-label={`Low case ${value.p90.toFixed(0)} ${unit}, best estimate ${value.p50.toFixed(0)} ${unit}, high case ${value.p10.toFixed(0)} ${unit}`}>
    <span className="range-span" style={{ left: pct(value.p90), width: `calc(${pct(value.p10)} - ${pct(value.p90)})` }} />
    <span className="range-mid" style={{ left: pct(value.p50) }} />
  </div>
}

const pay = (candidate: RankedCandidate): QuantileValues => ({ p90: candidate.p90NetPayM, p50: candidate.p50NetPayM, p10: candidate.p10NetPayM })

export function ProspectsScreen({ pkg, analysis, reservoir, mapsStatus, coverage, candidate, onCandidate, settings, reservoirModel, onTargetFluid, onForecast }: {
  pkg: FieldPackage
  analysis: AnalysisResult
  reservoir: string
  mapsStatus?: MapsStatus
  coverage?: SimulatorCoverage
  candidate?: RankedCandidate
  onCandidate: (candidate: RankedCandidate) => void
  settings: AppSettings
  reservoirModel?: { model?: StaticReservoirModel; loading?: boolean; error?: string }
  onTargetFluid: (fluid?: TargetFluid) => void
  onForecast: () => void
}) {
  const pays = payLabel(analysis.configuration)
  const { targets, hiddenAbove, outside, scored } = useMemo(() => shortlist(analysis, coverage), [analysis, coverage])
  const wells = useMemo(() => buildWellViews(pkg, analysis.wellSummaries, reservoir, analysis.configuration),
    [pkg, analysis.wellSummaries, reservoir, analysis.configuration])
  const selected = targets.find(item => item.candidateId === candidate?.candidateId) ?? targets[0]
  const [mode, setMode] = useState<WorkspaceMode>('map')
  const [wellId, setWellId] = useState('')
  const referenceIds = selected?.neighborEvidenceIds.map(id => id.replace(/^well:/, '')) ?? []
  const referenceWells = referenceIds.flatMap(id => {
    const bores = wells.filter(well => well.wellId === id)
    return bores.length ? [bores.find(well => well.summary) ?? bores[0]] : []
  })
  const shownWell = wells.find(well => well.id === wellId && referenceIds.includes(well.wellId)) ?? referenceWells[0]
  const shownDistance = shownWell?.summary && selected
    ? Math.hypot(shownWell.summary.eastingM - selected.eastingM, shownWell.summary.northingM - selected.northingM) : undefined
  const maxPay = Math.max(1, ...targets.map(item => item.p10NetPayM))
  const nameOf = (evidenceId: string) => wells.find(well => `well:${well.wellId}` === evidenceId)
  const neighbours = selected?.neighborEvidenceIds.map(id => {
    const well = nameOf(id)
    return { id, name: well?.name ?? id.slice(5, 13), pay: well?.summary?.netPayThicknessM,
      distance: well?.summary ? Math.hypot(well.summary.eastingM - selected.eastingM, well.summary.northingM - selected.northingM) : undefined }
  }) ?? []
  const columnOf = (id?: string) => reservoirModel?.model?.targets.find(column => column.id === id)
  const column = columnOf(selected?.candidateId)
  const contact = reservoirModel?.model?.contacts.find(item => item.type !== 'GasOilContact') ?? reservoirModel?.model?.contacts[0]
  const hydrocarbons = (item: { gasM: number; oilM: number }) =>
    [item.gasM >= .5 && `${metres(item.gasM)} gas`, item.oilM >= .5 && `${metres(item.oilM)} oil`].filter(Boolean).join(' + ') || 'no hydrocarbons'
  useAssistantContext('prospects', 'Prospects screen: the five best drillable targets as shown, ranked by score', {
    scoreFormula: 'expected pay x porosity x log10(1 + permeability mD) / (1 + relative uncertainty)',
    payMeasure: pays,
    targets: targets.map(item => ({ rank: item.rank, payP90P50P10M: [item.p90NetPayM, item.p50NetPayM, item.p10NetPayM].map(v => +v.toFixed(1)),
      score: +item.score.toFixed(1), porosity: +item.meanPayPorosity.toFixed(3), permeabilityMd: Math.round(item.meanPayPermeabilityMd),
      relativeUncertainty: +item.relativeUncertainty.toFixed(2), nearestLoggedWellKm: +(item.nearestWellDistanceM / 1000).toFixed(2),
      reservoirModel: columnOf(item.candidateId) && { topTvdM: columnOf(item.candidateId)!.topTvdM, grossM: columnOf(item.candidateId)!.grossM,
        goodRockM: columnOf(item.candidateId)!.netRockM, gasM: columnOf(item.candidateId)!.gasM, oilM: columnOf(item.candidateId)!.oilM } })),
    selectedRank: selected?.rank, referenceWells: neighbours.map(item => ({ name: item.name, distanceKm: item.distance && +(item.distance / 1000).toFixed(2), payM: item.pay })),
    hiddenOutsideSimulatorArea: hiddenAbove, mapLens: mode,
  })
  return <section className="screen-grid" aria-labelledby="prospects-title">
    <header className="screen-head">
      <div>
        <h1 id="prospects-title">Where should we drill?</h1>
        <p>Each point between the wells gets a score: expected {pays} multiplied by rock quality (porosity and permeability),
          then reduced for uncertainty. A target with less pay can rank higher if its rock flows better.
          These are the {targets.length} best-scoring points the simulator can drill.</p>
        <div className="fluid-switch" role="group" aria-label="Target hydrocarbon">
          <span>Looking for</span>
          {([[undefined, 'Any hydrocarbon'], ['Oil', 'Oil'], ['Gas', 'Gas']] as const).map(([fluid, label]) =>
            <button key={label} type="button" aria-pressed={settings.analysis.targetFluid === fluid} onClick={() => onTargetFluid(fluid)}>{label}</button>)}
          {settings.analysis.targetFluid && <small>Only good rock holding {settings.analysis.targetFluid.toLowerCase()} counts, using the reservoir model's fluid contacts.</small>}
        </div>
      </div>
      <button type="button" className="primary" disabled={!selected} onClick={() => { if (selected) { onCandidate(selected); onForecast() } }}>
        Forecast target #{selected?.rank ?? '–'} →</button>
    </header>

    <div className="split prospects-split">
      <div className="target-list" role="list" aria-label="Best drilling targets">
        {targets.map(item => <button key={item.candidateId} type="button" role="listitem" className="target-card"
          aria-pressed={item.candidateId === selected?.candidateId} onClick={() => onCandidate(item)}>
          <span className="target-rank">#{item.rank}</span>
          <span className="target-body">
            <span className="target-headline"><strong>Expected {pays} {metres(item.p50NetPayM)}</strong>
              <span className="target-score" title="Net pay × porosity × log₁₀(1 + permeability) ÷ (1 + uncertainty)">Score {item.score.toFixed(1)}</span></span>
            <RangeBar value={pay(item)} max={maxPay} />
            <small>Low {metres(item.p90NetPayM)} · high {metres(item.p10NetPayM)}</small>
            <small className="target-factors">Rock {(item.meanPayPorosity * 100).toFixed(0)}% porosity · {item.meanPayPermeabilityMd.toFixed(0)} mD ·
              ±{(item.relativeUncertainty * 100).toFixed(0)}% uncertainty · {(item.nearestWellDistanceM / 1000).toFixed(1)} km to nearest logged well</small>
            {columnOf(item.candidateId) && <small className="target-model">Reservoir model: {hydrocarbons(columnOf(item.candidateId)!)} in good rock</small>}
          </span>
        </button>)}
        {!targets.length && <p className="empty-note">No drillable target passes the current cutoffs. Loosen the rock cutoffs in Settings.</p>}
        {(hiddenAbove > 0 || outside > 0) && <p className="fine-print">{hiddenAbove > 0 ? `${hiddenAbove} higher-scoring point${hiddenAbove === 1 ? ' is' : 's are'} outside the area the simulator can drill and ${hiddenAbove === 1 ? 'is' : 'are'} not shown. ` : ''}
          {outside} of {scored} scored points fall outside that area.</p>}
      </div>

      <div className="lens-panel">
        <p className="lens-context" role="note">
          {mode === 'map'
            ? <><span className="legend-dot target" aria-hidden="true" /> Proposed well (drill here)
              <span className="legend-dot reference" aria-hidden="true" /> Existing wells it is estimated from
              <span className="legend-dot candidate" aria-hidden="true" /> Other targets</>
            : <>Showing <strong>{shownWell?.name ?? 'a reference well'}</strong>, an existing well{shownDistance !== undefined ? ` ${(shownDistance / 1000).toFixed(1)} km` : ''} from
              target #{selected?.rank ?? '–'}. The proposed well has no data yet; predicting it is the Forecast step.</>}
        </p>
        <AnalysisWorkspace mode={mode} onMode={setMode} fieldPackage={pkg} wells={wells} candidates={targets}
          selectedCandidate={selected} selectedWellId={shownWell?.id ?? wells[0]?.id ?? ''}
          referenceWellIds={referenceIds} wellLabel="REFERENCE WELL"
          onWell={setWellId} onCandidate={onCandidate} mapsStatus={mapsStatus} reservoirModel={reservoirModel} />
      </div>
    </div>

    {selected && <div className="split detail-split">
      <section className="explain-panel" aria-labelledby="why-title">
        <h2 id="why-title">Target #{selected.rank} in numbers</h2>
        <dl className="mini-facts">
          <div><dt>{pays[0].toUpperCase() + pays.slice(1)} (low · best · high)</dt><dd>{selected.p90NetPayM.toFixed(0)} · {selected.p50NetPayM.toFixed(0)} · {selected.p10NetPayM.toFixed(0)} m</dd></div>
          <div><dt>Porosity</dt><dd>{(selected.meanPayPorosity * 100).toFixed(0)}%</dd></div>
          <div><dt>Permeability</dt><dd>{selected.meanPayPermeabilityMd.toFixed(0)} mD</dd></div>
          <div><dt>Uncertainty</dt><dd>±{(selected.relativeUncertainty * 100).toFixed(0)}%</dd></div>
        </dl>
        <h3>Built from these wells</h3>
        <ul className="neighbour-list">{neighbours.map(item => <li key={item.id}>
          <strong>{item.name}</strong><span>{item.distance !== undefined ? `${(item.distance / 1000).toFixed(1)} km away` : ''}</span>
          <span>{metres(item.pay)} {pays}</span></li>)}</ul>
        <h3>What the reservoir model says here</h3>
        {column ? <>
          <dl className="mini-facts">
            <div><dt>Reservoir top</dt><dd>{metres(column.topTvdM)}</dd></div>
            <div><dt>Thickness</dt><dd>{metres(column.grossM)}</dd></div>
            <div><dt>Good rock</dt><dd>{metres(column.netRockM)}</dd></div>
            <div><dt>Gas · oil</dt><dd>{metres(column.gasM)} · {metres(column.oilM)}</dd></div>
          </dl>
          <p className="fine-print">{column.fluidAtTop === 'water'
            ? <>The reservoir top here sits below the {contact ? `${contactLabel[contact.type].toLowerCase()} at ${metres(contact.trueVerticalDepthM)}` : 'fluid contact'}, so the model expects water only. </>
            : <>Of {metres(column.netRockM)} good rock, {hydrocarbons(column)} sits above the {contact ? `${contactLabel[contact.type].toLowerCase()} (${contactCode[contact.type]}) at ${metres(contact.trueVerticalDepthM)}` : 'fluid contact'};
              {' '}the remaining {metres(column.waterM)} is expected to hold water. </>}
            {analysis.configuration?.targetFluid ? `The ranking already counts only ${analysis.configuration.targetFluid.toLowerCase()}-bearing rock.` : 'Net pay above counts all good rock; the model also places the fluid contact.'} See <strong>3D Reservoir</strong>.</p>
        </> : <p className="fine-print">{reservoirModel?.loading ? 'Building the reservoir model…' : reservoirModel?.error ?? 'No reservoir model for this target.'}</p>}
        <details className="plain-details"><summary>How the score is calculated</summary>
          <p>Score = expected {pays} × porosity × log₁₀(1 + permeability in mD) ÷ (1 + uncertainty).
            {selected.scoreComponents && <> For this target: {selected.scoreComponents.p50NetPayM.toFixed(0)} m × {selected.scoreComponents.porosityFactor.toFixed(2)} ×
              {' '}{selected.scoreComponents.permeabilityLogFactor.toFixed(2)} ÷ {selected.scoreComponents.uncertaintyDivisor.toFixed(2)} = {selected.score.toFixed(1)}.</>}
            {' '}Uncertainty grows when nearby wells disagree and when the target is far from them. The ranges are not yet calibrated against outcomes.</p>
        </details>
      </section>
      <ProspectBriefPanel key={`${analysis.analysisSha256}:${selected.candidateId}`} pkg={pkg} analysis={analysis} reservoir={reservoir}
        configuration={settings.analysis} candidate={selected} wells={wells} />
    </div>}
  </section>
}
