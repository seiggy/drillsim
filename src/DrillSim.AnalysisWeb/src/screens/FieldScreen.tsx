import { useMemo, useState } from 'react'
import { AnalysisWorkspace, buildWellViews, type WorkspaceMode } from '../components/AnalysisWorkspace'
import { millidarcyM2 } from '../settings'
import { payLabel } from '../analysisConfiguration'
import { useAssistantContext } from '../assistantContext'
import { metres } from '../structure'
import type { SimulatorCoverage } from '../prospects'
import { contactCode, type StaticReservoirModel } from '../reservoirModel'
import type { AnalysisResult, FieldPackage, MapsStatus } from '../types'

export function FieldScreen({ pkg, analysis, reservoir, mapsStatus, coverage, reservoirModel, onNext }: {
  pkg: FieldPackage
  analysis: AnalysisResult
  reservoir: string
  mapsStatus?: MapsStatus
  coverage?: SimulatorCoverage
  reservoirModel?: { model?: StaticReservoirModel; loading?: boolean; error?: string }
  onNext: () => void
}) {
  const wells = useMemo(() => buildWellViews(pkg, analysis.wellSummaries, reservoir, analysis.configuration),
    [pkg, analysis.wellSummaries, reservoir, analysis.configuration])
  const crossing = wells.filter(well => well.reservoir || well.formationRanges.length)
  const logged = analysis.wellSummaries
  const [mode, setMode] = useState<WorkspaceMode>('map')
  const [wellId, setWellId] = useState(() => wells.find(well => well.summary)?.id ?? wells[0]?.id ?? '')
  const selected = wells.find(well => well.id === wellId)
  const fieldName = String(pkg.field.Name ?? pkg.field.name ?? 'Field')
  const averagePay = logged.length ? logged.reduce((sum, well) => sum + well.netPayThicknessM, 0) / logged.length : undefined
  const pays = payLabel(analysis.configuration)
  const ranked = [...logged].sort((a, b) => b.netPayThicknessM - a.netPayThicknessM)
  useAssistantContext('field', 'Field screen: the field overview and logged wells as shown', {
    wellsInField: pkg.sourceCounts.wells, wellboresCrossingReservoir: crossing.length, loggedWells: logged.length,
    averagePayM: averagePay && +averagePay.toFixed(1), payMeasure: pays,
    simulatorArea: coverage && { eastWestKm: +((coverage.maxEastingM - coverage.minEastingM) / 1000).toFixed(1), northSouthKm: +((coverage.maxNorthingM - coverage.minNorthingM) / 1000).toFixed(1), maxDepthM: Math.round(coverage.maxTrueVerticalDepthM) },
    loggedWellsByPay: ranked.map(summary => ({ name: wells.find(well => well.wellId === summary.wellId)?.name, payM: +summary.netPayThicknessM.toFixed(1),
      porosity: +summary.meanPayPorosity.toFixed(3), permeabilityMd: Math.round(summary.meanPayPermeabilityM2 / millidarcyM2) })),
    selectedWell: selected && { name: selected.name, formationMdRange: selected.reservoir, fluidZones: selected.column.fluids.filter(band => !['unknown', 'outside'].includes(band.value)).map(band => ({ fluid: band.value, topMd: +band.top.toFixed(1), baseMd: +band.base.toFixed(1) })) },
    lens: mode, dataGaps: [...pkg.dataGaps, ...analysis.dataGaps].slice(0, 12),
  })
  const contact = reservoirModel?.model?.contacts.find(item => item.type !== 'GasOilContact') ?? reservoirModel?.model?.contacts[0]

  return <section className="screen-grid" aria-labelledby="field-title">
    <header className="screen-head">
      <div>
        <h1 id="field-title">{fieldName} · {reservoir}</h1>
        <p><strong>{crossing.length} wellbores</strong> cross this reservoir. <strong>{logged.length}</strong> of them have rock-quality
          logs we can measure; the rest only mark where the reservoir starts and ends. Everything on the next screens is built from these wells.</p>
      </div>
      <button type="button" className="primary" onClick={onNext}>See where to drill →</button>
    </header>

    <dl className="fact-row">
      <div><dt>Wells in the field</dt><dd>{pkg.sourceCounts.wells}</dd></div>
      <div><dt>Wells with rock logs</dt><dd>{logged.length}</dd></div>
      <div><dt>Average {pays} in logged wells</dt><dd>{metres(averagePay)}</dd><small>{analysis.configuration?.targetFluid ? `Good rock holding ${analysis.configuration.targetFluid.toLowerCase()}` : 'Rock that passes the cutoffs in Settings'}</small></div>
      <div><dt>Area the simulator can drill</dt><dd>{coverage ? `${((coverage.maxEastingM - coverage.minEastingM) / 1000).toFixed(0)} × ${((coverage.maxNorthingM - coverage.minNorthingM) / 1000).toFixed(0)} km` : '—'}</dd>
        <small>{coverage ? `Down to ${metres(coverage.maxTrueVerticalDepthM)} depth` : 'Not reported'}</small></div>
      <div><dt>Fluid contact</dt><dd>{contact ? `${contactCode[contact.type]} ${metres(contact.trueVerticalDepthM)}` : reservoirModel?.loading ? '…' : 'None found'}</dd>
        <small>{contact ? `From logs in ${contact.wells} wells · see 3D Reservoir` : 'Hydrocarbons above, water below'}</small></div>
    </dl>

    <div className="split">
      <div className="lens-panel">
        <AnalysisWorkspace mode={mode} onMode={setMode} fieldPackage={pkg} wells={wells} candidates={[]}
          selectedWellId={selected?.id ?? ''} onWell={setWellId} onCandidate={() => {}} mapsStatus={mapsStatus} reservoirModel={reservoirModel} />
        <p className="lens-hint">Map shows well locations. Pick a well, then use <strong>2D Section</strong> or <strong>Logs</strong> to see where the
          reservoir is and how good the rock is. <strong>3D Reservoir</strong> shows the model built from all of them.</p>
      </div>
      <aside className="side-panel" aria-label="Wells with rock logs">
        {selected && <section className="well-card">
          <h2>{selected.name}</h2>
          {selected.summary ? <dl className="mini-facts">
            <div><dt>{pays[0].toUpperCase() + pays.slice(1)}</dt><dd>{metres(selected.summary.netPayThicknessM, 1)}</dd></div>
            <div><dt>Porosity</dt><dd>{(selected.summary.meanPayPorosity * 100).toFixed(0)}%</dd></div>
            <div><dt>Permeability</dt><dd>{(selected.summary.meanPayPermeabilityM2 / millidarcyM2).toFixed(0)} mD</dd></div>
            <div><dt>Reservoir depth</dt><dd>{selected.reservoir ? `${selected.reservoir.top.toFixed(0)}–${selected.reservoir.base.toFixed(0)} m` : '—'}</dd></div>
          </dl> : <p>No rock-quality logs for this well. It only tells us where the reservoir is.</p>}
        </section>}
        <h2 className="list-title">Logged wells by {pays}</h2>
        <ol className="well-list">{ranked.map(summary => {
          const view = wells.find(well => well.wellId === summary.wellId)
          return <li key={summary.wellId}><button type="button" aria-pressed={view?.id === selected?.id}
            onClick={() => view && setWellId(view.id)}>
            <span>{view?.name ?? summary.wellId.slice(0, 8)}</span><span>{metres(summary.netPayThicknessM)}</span>
          </button></li>
        })}</ol>
        <details className="plain-details"><summary>What is “net pay”?</summary>
          <p>The thickness of reservoir rock that is porous and permeable enough to flow, using the cutoffs in Settings
            (porosity ≥ {(analysis.configuration!.porosityCutoff * 100).toFixed(0)}%, permeability ≥ {(analysis.configuration!.permeabilityCutoffM2 / millidarcyM2).toFixed(1)} mD).
            It is rock quality only: it does not prove oil or gas is present, and it is not a reserves estimate.</p>
        </details>
        {(pkg.dataGaps.length > 0 || analysis.dataGaps.length > 0) && <details className="plain-details">
          <summary>Data gaps ({pkg.dataGaps.length + analysis.dataGaps.length})</summary>
          <ul>{[...pkg.dataGaps, ...analysis.dataGaps].map((gap, index) => <li key={index}>{gap}</li>)}</ul>
        </details>}
      </aside>
    </div>
  </section>
}
