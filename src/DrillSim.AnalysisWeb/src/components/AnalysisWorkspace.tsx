import { lazy, Suspense, useId, useState } from 'react'
import { classificationLabel, idOf, nameOf, read } from '../data'
import type { FieldPackage, JsonRecord, ProposedWellPathStation, RankedCandidate, WellSummary } from '../types'
import { defaultRockCriteria, fluidPhase, positionAtMd, sampleClass, type RockCriteria } from '../reservoirGeometry'
import { ReservoirView } from './ReservoirView'
import { buildFormationColumn, fluidLabel, qualityLabel, gapLabel } from '../fluidColumn'
import type { FormationColumn } from '../fluidColumn'

const AzureFieldMap = lazy(() => import('./AzureFieldMap').then((module) => ({ default: module.AzureFieldMap })))

export type WorkspaceMode = 'map' | 'section' | 'trajectory' | 'logs' | 'crossplot'

interface Station {
  md: number
  tvd: number
  east: number
  north: number
}

interface Sample {
  rockKnown: boolean
  fluidKnown: boolean
  depth: number
  porosity: number
  permeabilityM2: number
  pressure: number
  water: number
  oil: number
  gas: number
  oilFlow: number
  gasFlow: number
  waterFlow: number
}

interface Interval {
  top: number
  base: number
}

type FluidClass = 'gas' | 'oil' | 'water'

interface FluidInterval extends Interval {
  fluid: FluidClass
}

interface FluidContact {
  type: 'GasWaterContact' | 'GasOilContact' | 'OilWaterContact'
  depth: number
}

export interface WellView {
  criteria?: RockCriteria
  id: string
  wellId: string
  name: string
  wellBoreId: string
  summary?: WellSummary
  stations: Station[]
  samples: Sample[]
  reservoir?: Interval
  formationName: string
  formationRanges: Interval[]
  geometryWarnings: string[]
  fluidIntervals: FluidInterval[]
  contacts: FluidContact[]
  column: FormationColumn
  openings: Interval[]
  production: Array<{ month: string; oil: number; gas: number; water: number }>
  sourceLabel: string
}

const modes: Array<{ id: WorkspaceMode; label: string }> = [
  { id: 'map', label: 'Map' },
  { id: 'section', label: '2D Section' },
  { id: 'trajectory', label: '3D Reservoir' },
  { id: 'logs', label: 'Logs' },
  { id: 'crossplot', label: 'Crossplot' },
]

function asRecord(value: unknown): JsonRecord | undefined {
  return value && typeof value === 'object' && !Array.isArray(value) ? value as JsonRecord : undefined
}

function asRecords(value: unknown): JsonRecord[] {
  return Array.isArray(value) ? value.filter((item) => asRecord(item)).map((item) => item as JsonRecord) : []
}

function number(value: unknown): number | undefined {
  if (typeof value === 'number' && Number.isFinite(value)) return value
  const wrapper = asRecord(value)
  const gaussian = asRecord(read(wrapper, 'GaussianValue', 'gaussianValue'))
  const scalar = asRecord(read(wrapper, 'DiracDistributionValue', 'diracDistributionValue'))
  const candidate = read(gaussian, 'Mean', 'mean') ?? read(scalar, 'Value', 'value')
  return typeof candidate === 'number' && Number.isFinite(candidate) ? candidate : undefined
}

function fieldValue(record: JsonRecord | undefined, ...keys: string[]) {
  return number(read(record, ...keys))
}

function measuredDepth(record: JsonRecord | undefined) {
  return read(record, 'Reference', 'reference') === 'MeasuredDepth' &&
    read(record, 'Unit', 'unit') === 'm' && read(record, 'PositiveDown', 'positiveDown') === true
    ? fieldValue(record, 'Value', 'value') : undefined
}

function displayName(record: JsonRecord) {
  return nameOf(record).replace(/^SYNTH-\S+\s+/i, '')
}

function curveMap(geology: JsonRecord | undefined) {
  const petrophysics = asRecord(read(geology, 'Petrophysics', 'petrophysics'))
  const run = asRecords(read(petrophysics, 'LogRuns', 'logRuns'))[0]
  const depths = (read(run, 'DepthValues', 'depthValues') as unknown[] | undefined) ?? []
  const curves = new Map<string, Array<number | null>>()
  asRecords(read(run, 'Curves', 'curves')).forEach((curve) => {
    const mnemonic = String(read(curve, 'CanonicalMnemonic', 'canonicalMnemonic') ?? '')
    const unit = read(curve, 'CanonicalUnit', 'canonicalUnit')
    if (['PHIE', 'SW', 'SO', 'SG'].includes(mnemonic.toUpperCase()) && !['fraction', 'm3/m3'].includes(String(unit))) return
    if (mnemonic.toUpperCase() === 'PERM' && unit !== 'm2') return
    const raw = (read(curve, 'Values', 'values') as Array<number | null> | undefined) ?? []
    const nulls = read(curve, 'NullFlags', 'nullFlags') as boolean[] | undefined
    const quality = read(curve, 'QualityFlags', 'qualityFlags') as Array<string | null> | undefined
    const values = raw.map((value, index) => typeof value === 'number' && Number.isFinite(value) &&
      !nulls?.[index] && !/missing|bad.?hole|invalid/i.test(quality?.[index] ?? '') ? value : null)
    curves.set(mnemonic.toUpperCase(), values)
  })
  return { depths, curves }
}

function buildSamples(geology: JsonRecord | undefined): Sample[] {
  const { depths, curves } = curveMap(geology)
  const tableSamples = asRecords(read(geology, 'GeologicalPropertyTable', 'geologicalPropertyTable')).flatMap((entry) => {
    const depth = fieldValue(entry, 'MeasuredDepth', 'measuredDepth')
    const porosity = fieldValue(entry, 'Porosity', 'porosity')
    const permeabilityM2 = fieldValue(entry, 'Permeability', 'permeability')
    const pressure = fieldValue(entry, 'PressureDifferential', 'pressureDifferential')
    return depth === undefined || porosity === undefined || permeabilityM2 === undefined || pressure === undefined
      ? []
      : [{ depth, porosity, permeabilityM2, pressure }]
  })

  if (depths.length) {
    return depths.flatMap((depth, index) => {
      if (typeof depth !== 'number' || !Number.isFinite(depth)) return []
      const table = tableSamples.reduce<typeof tableSamples[number] | undefined>((nearest, sample) =>
        !nearest || Math.abs(sample.depth - depth) < Math.abs(nearest.depth - depth) ? sample : nearest, undefined)
      const localTable = table && Math.abs(table.depth - depth) <= 1 ? table : undefined
      const porosity = curves.has('PHIE') ? curves.get('PHIE')?.[index] ?? undefined : localTable?.porosity
      const permeability = curves.has('PERM') ? curves.get('PERM')?.[index] ?? undefined : localTable?.permeabilityM2
      const phases = ['SW', 'SO', 'SG'].map((name) => curves.get(name)?.[index])
      return [{
        depth,
        rockKnown: porosity !== undefined && porosity >= 0 && porosity <= 1 && permeability !== undefined && permeability >= 0,
        fluidKnown: phases.every((v) => typeof v === 'number' && v >= 0 && v <= 1) &&
          Math.abs(phases.reduce<number>((sum, v) => sum + (v ?? 0), 0) - 1) <= .05,
        porosity: porosity ?? 0,
        permeabilityM2: permeability ?? 0,
        pressure: table?.pressure ?? 0,
        water: curves.get('SW')?.[index] ?? 0,
        oil: curves.get('SO')?.[index] ?? 0,
        gas: curves.get('SG')?.[index] ?? 0,
        oilFlow: curves.get('QO')?.[index] ?? 0,
        gasFlow: curves.get('QG')?.[index] ?? 0,
        waterFlow: curves.get('QW')?.[index] ?? 0,
      }]
    })
  }

  return tableSamples.map((sample) => ({
    ...sample,
    rockKnown: true,
    fluidKnown: false,
    water: 0,
    oil: 0,
    gas: 0,
    oilFlow: 0,
    gasFlow: 0,
    waterFlow: 0,
  }))
}

function buildOpenings(architecture: JsonRecord | undefined): Interval[] {
  return asRecords(read(architecture, 'CasingSections', 'casingSections')).flatMap((section) => {
    const top = fieldValue(section, 'TopDepth', 'topDepth') ?? 0
    const length = fieldValue(section, 'Length', 'length') ?? 0
    const openHole = asRecord(read(section, 'OpenHoleSection', 'openHoleSection'))
    const openLength = asRecords(read(openHole, 'HoleSizes', 'holeSizes'))
      .reduce((sum, size) => sum + (fieldValue(size, 'Length', 'length') ?? 0), 0)
    return openLength > 0 ? [{ top: top + length, base: top + length + openLength }] : []
  })
}

function buildProduction(well: JsonRecord) {
  const dataset = asRecord(read(well, 'Dataset', 'dataset'))
  return asRecords(read(dataset, 'MonthlyProduction', 'monthlyProduction')).map((entry) => ({
    month: `${String(read(entry, 'Year', 'year'))}-${String(read(entry, 'Month', 'month')).padStart(2, '0')}`,
    oil: fieldValue(asRecord(read(entry, 'Oil', 'oil')), 'Value', 'value') ?? 0,
    gas: fieldValue(asRecord(read(entry, 'Gas', 'gas')), 'Value', 'value') ?? 0,
    water: fieldValue(asRecord(read(entry, 'Water', 'water')), 'Value', 'value') ?? 0,
  }))
}

function buildFluidColumn(samples: Sample[], bounds: Interval, criteria: RockCriteria) {
  const ordered = [...samples].sort((left, right) => left.depth - right.depth)
  const deltas = ordered.slice(1).map((sample, index) => sample.depth - ordered[index].depth).filter((delta) => delta > 0)
  const step = deltas.sort((left, right) => left - right)[Math.floor(deltas.length / 2)] ?? 1
  const intervals: FluidInterval[] = []
  ordered.forEach((sample) => {
    const fluid = sampleClass(sample, criteria)
    if (!fluid || fluid === 'nonpay') return
    const top = Math.max(bounds.top, sample.depth - step / 2)
    const base = Math.min(bounds.base, sample.depth + step / 2)
    if (base <= top) return
    const current = intervals.at(-1)
    if (current?.fluid === fluid && Math.abs(top - current.base) < step * .01) current.base = base
    else intervals.push({ fluid, top, base })
  })
  return {
    reservoir: intervals.length ? { top: intervals[0].top, base: intervals.at(-1)!.base } : undefined,
    fluidIntervals: intervals,
  }
}

export function buildWellViews(fieldPackage: FieldPackage, summaries: WellSummary[], reservoirName?: string, criteria = defaultRockCriteria): WellView[] {
  const referencePoint = asRecord(read(fieldPackage.field, 'ReferencePoint', 'referencePoint'))
  const originEast = fieldValue(referencePoint, 'RiemannianEast', 'riemannianEast', 'Y', 'y') ?? 0
  const originNorth = fieldValue(referencePoint, 'RiemannianNorth', 'riemannianNorth', 'X', 'x') ?? 0
  const wellById = new Map(fieldPackage.wells.map((record) => [idOf(record), record]))
  const trajectoryByWellBore = new Map<string, JsonRecord>()
  const priority = (record: JsonRecord) => read(record, 'IsDefinitive', 'isDefinitive') === true ? 2
    : ['Actual', 1].includes(read(record, 'TrajectoryType', 'trajectoryType') as string | number) ? 1 : 0
  for (const record of [...fieldPackage.trajectories].sort((a, b) => idOf(a).localeCompare(idOf(b)))) {
    const bore = String(read(record, 'WellBoreID', 'wellBoreID') ?? '')
    const current = trajectoryByWellBore.get(bore)
    if (!current || priority(record) > priority(current)) trajectoryByWellBore.set(bore, record)
  }
  const geologyByWellBore = new Map(fieldPackage.geologicalProperties.map((record) => [
    String(read(record, 'WellBoreID', 'wellBoreID') ?? ''),
    record,
  ]))
  const architectureByWellBore = new Map(fieldPackage.wellBoreArchitectures.map((record) => [
    String(read(record, 'WellBoreID', 'wellBoreID') ?? ''),
    record,
  ]))
  const summaryByWell = new Map(summaries.map((summary) => [summary.wellId, summary]))

  return fieldPackage.wellBores.map((wellBore) => {
    const wellBoreId = idOf(wellBore)
    const wellId = String(read(wellBore, 'WellID', 'wellID') ?? '')
    const well = wellById.get(wellId)
    const trajectory = trajectoryByWellBore.get(wellBoreId)
    const geology = geologyByWellBore.get(wellBoreId)
    const allSamples = buildSamples(geology).sort((a, b) => a.depth - b.depth)
    const geometryWarnings: string[] = []
    const rawStations = asRecords(read(trajectory, 'SurveyStationList', 'surveyStationList'))
    let stations = rawStations.flatMap((station) => {
      const md = fieldValue(station, 'MD', 'md')
      const tvd = fieldValue(station, 'TVD', 'tvd')
      const east = fieldValue(station, 'RiemannianEast', 'riemannianEast')
      const north = fieldValue(station, 'RiemannianNorth', 'riemannianNorth')
      return md === undefined || tvd === undefined || east === undefined || north === undefined
        ? [] : [{ md, tvd, east: east - originEast, north: north - originNorth }]
    })
    if (stations.length !== rawStations.length ||
        fieldValue(referencePoint, 'RiemannianEast', 'riemannianEast', 'Y', 'y') === undefined ||
        fieldValue(referencePoint, 'RiemannianNorth', 'riemannianNorth', 'X', 'x') === undefined) {
      geometryWarnings.push('Incomplete coordinate frame; trajectory excluded rather than joined across missing stations.')
      stations = []
    }
    if (!stations.length) geometryWarnings.push('No located survey; structural-only evidence is not a dry control.')
    if (stations.some((station, index) => station.md < 0 || (index > 0 && station.md <= stations[index - 1].md))) {
      geometryWarnings.push('Survey MD is not strictly increasing; trajectory excluded.')
      stations = []
    }
    const tie = asRecord(read(trajectory, 'TieInPoint', 'tieInPoint'))
    const tieMd = fieldValue(tie, 'MD', 'md')
    const tieE = fieldValue(tie, 'RiemannianEast', 'riemannianEast')
    const tieN = fieldValue(tie, 'RiemannianNorth', 'riemannianNorth')
    const tieStation = tieMd === undefined ? undefined : positionAtMd(stations, tieMd)
    if (tieStation && tieE !== undefined && tieN !== undefined &&
        Math.hypot(tieStation.east - (tieE - originEast), tieStation.north - (tieN - originNorth)) > 25) {
      geometryWarnings.push('Survey coordinates disagree with its tie-in by more than 25 m; no guessed translation applied.')
      stations = []
    }
    const petrophysics = asRecord(read(geology, 'Petrophysics', 'petrophysics'))
    const provenance = asRecord(read(petrophysics, 'Provenance', 'provenance'))
    const formationTops = asRecords(read(petrophysics, 'FormationTops', 'formationTops'))
      .flatMap((top) => {
        const depth = asRecords(read(top, 'Depths', 'depths')).map(measuredDepth).find((value) => value !== undefined)
        return depth === undefined ? [] : [{
          depth,
          name: String(read(top, 'FormationName', 'formationName') ?? 'Reservoir interval'),
        }]
      })
      .sort((left, right) => left.depth - right.depth)
    const formationIntervals = asRecords(read(petrophysics, 'FormationIntervals', 'formationIntervals'))
      .flatMap((interval) => {
        const top = measuredDepth(asRecord(read(interval, 'TopDepth', 'topDepth')))
        const base = measuredDepth(asRecord(read(interval, 'BaseDepth', 'baseDepth')))
        return top === undefined || base === undefined || base <= top ? [] : [{
          top,
          base,
          name: String(read(interval, 'FormationName', 'formationName') ?? 'Reservoir interval'),
        }]
      })
      .sort((left, right) => left.top - right.top)
    const selectedIntervals = formationIntervals.filter((interval) =>
      interval.name.localeCompare(reservoirName ?? '', undefined, { sensitivity: 'accent' }) === 0)
    const selectedInterval = selectedIntervals[0]
    const selectedFormationIndex = formationTops.findIndex((top) =>
      top.name.localeCompare(reservoirName ?? '', undefined, { sensitivity: 'accent' }) === 0)
    const selectedFormation = selectedFormationIndex >= 0 ? formationTops[selectedFormationIndex] : undefined
    const selectedFormationBase = selectedInterval?.base ?? (selectedFormationIndex >= 0
      ? formationTops[selectedFormationIndex + 1]?.depth ?? Number.POSITIVE_INFINITY
      : Number.POSITIVE_INFINITY)
    const selectedFormationTop = selectedInterval?.top ?? selectedFormation?.depth
    const formationRanges = selectedIntervals.length ? selectedIntervals : selectedFormationTop !== undefined && Number.isFinite(selectedFormationBase)
      ? [{ top: selectedFormationTop, base: selectedFormationBase }] : []
    const samples = formationRanges.length ? allSamples.filter((sample) =>
      formationRanges.some((range) => sample.depth >= range.top && sample.depth <= range.base)) : reservoirName ? [] : allSamples
    const bounds = formationRanges.length ? formationRanges : !reservoirName && samples.length ? [{ top: samples[0].depth, base: samples.at(-1)!.depth }] : []
    if (!formationRanges.length) geometryWarnings.push('No bounded positive-down MD interval in metres for the selected formation.')
    if (!samples.some((sample) => sampleClass(sample, criteria) !== undefined)) geometryWarnings.push('No classifiable rock/fluid samples; absence of data does not imply dry rock.')
    const columns = bounds.map((range) => buildFluidColumn(samples.filter((sample) => sample.depth >= range.top && sample.depth <= range.base), range, criteria))
    const interpretedColumn = buildFormationColumn(samples, formationRanges, criteria)
    const fluidIntervals = columns.flatMap((column) => column.fluidIntervals)
    const fluidColumn = {
      fluidIntervals,
      contacts: interpretedColumn.contacts,
      reservoir: fluidIntervals.length ? { top: fluidIntervals[0].top, base: fluidIntervals.at(-1)!.base } : undefined,
    }
    const reservoir = fluidColumn.reservoir
    const formationBefore = reservoir
      ? [...formationTops].reverse().find((top) => top.depth <= reservoir.top)
      : undefined
    const formationAfter = reservoir
      ? formationTops.find((top) => top.depth > reservoir.top && top.depth - reservoir.top <= 10)
      : undefined
    const formation = selectedInterval ?? selectedFormation ?? formationAfter ?? formationBefore ?? formationTops[0]

    return {
      criteria,
      id: wellBoreId,
      wellId,
      name: displayName(wellBore).replace(/ main bore$/i, ''),
      wellBoreId,
      summary: summaryByWell.get(wellId),
      stations,
      formationRanges,
      geometryWarnings,
      samples,
      reservoir,
      formationName: formation?.name ?? 'Qualifying reservoir interval',
      fluidIntervals: fluidColumn.fluidIntervals,
      contacts: fluidColumn.contacts,
      column: interpretedColumn,
      openings: buildOpenings(architectureByWellBore.get(wellBoreId)),
      production: well ? buildProduction(well) : [],
      sourceLabel: [
        classificationLabel(read(provenance, 'Classification', 'classification')),
        String(read(provenance, 'DatasetName', 'datasetName') ?? 'unknown source'),
      ].join(' · '),
    }
  }).filter((well) => well.id && well.wellId && wellById.has(well.wellId))
}

function path(points: Array<[number, number]>) {
  return points.map(([x, y], index) => `${index ? 'L' : 'M'}${x.toFixed(1)},${y.toFixed(1)}`).join(' ')
}

function depthAtMd(stations: Station[], md: number) {
  const afterIndex = stations.findIndex((station) => station.md >= md)
  if (afterIndex < 0) return stations.at(-1)?.tvd ?? md
  if (afterIndex === 0) return stations[0]?.tvd ?? md
  const before = stations[afterIndex - 1]
  const after = stations[afterIndex]
  const ratio = (md - before.md) / Math.max(after.md - before.md, 1)
  return before.tvd + (after.tvd - before.tvd) * ratio
}

function SectionView({ well, proposedPath }: { well?: WellView; proposedPath?: ProposedWellPathStation[] }) {
  const patternId = useId()
  if (!well?.stations.length) return <WorkspaceEmpty text="This well has no calculated trajectory stations." />
  const column = well.column
  const formation = column.bounds
  const stations = well.stations
  const first = stations[0]
  const last = stations.at(-1) ?? first
  const finalEast = last.east - first.east
  const finalNorth = last.north - first.north
  const finalDistance = Math.hypot(finalEast, finalNorth) || 1
  const deviations = stations.map((station) =>
    ((station.east - first.east) * finalEast + (station.north - first.north) * finalNorth) / finalDistance)
  const proposedDeviations = proposedPath?.map((station) =>
    ((station.eastingM - first.east) * finalEast + (station.northingM - first.north) * finalNorth) / finalDistance) ?? []
  const maxDeviation = Math.max(100, ...deviations.map(Math.abs), ...proposedDeviations.map(Math.abs))
  const maxDepth = Math.max(
    1,
    ...stations.map((station) => station.tvd),
    ...(proposedPath?.map((station) => station.trueVerticalDepthM) ?? []),
    well.reservoir?.base ?? 0,
  )
  const x = (value: number) => 70 + 420 * (.5 + value / (2 * maxDeviation))
  const y = (value: number) => 34 + 350 * value / maxDepth
  const detailY = (value: number) => 112 + 224 * (value - (formation?.top ?? 0)) /
    Math.max((formation?.base ?? 1) - (formation?.top ?? 0), 1)
  const trajectory = path(stations.map((station, index) => [x(deviations[index]), y(station.tvd)]))

  return (
    <div className="subsurface-view section-view">
      <div className="section-plot" tabIndex={0} role="region" aria-label="Section and fluid column, scroll horizontally on narrow screens">
      <svg viewBox="0 0 820 420" role="img" aria-label={`Vertical section for ${well.name}`}>
        <defs>
          <pattern id={`${patternId}-unknown`} width="6" height="6" patternUnits="userSpaceOnUse">
            <rect width="6" height="6" className="column-unknown-bed" />
            <path d="M0,6 L6,0" className="column-hatch-line" />
          </pattern>
          <pattern id={`${patternId}-outside`} width="6" height="6" patternUnits="userSpaceOnUse">
            <rect width="6" height="6" className="column-unknown-bed" />
            <circle cx="3" cy="3" r="1" className="column-hatch-dot" />
          </pattern>
          <pattern id={`${patternId}-below-cutoff`} width="6" height="6" patternUnits="userSpaceOnUse">
            <rect width="6" height="6" className="column-below-bed" />
            <path d="M0,3 H6" className="column-below-line" />
          </pattern>
        </defs>
        <rect className="plot-bed" x="0" y="0" width="820" height="420" />
        {[0, .25, .5, .75, 1].map((ratio) => (
          <g key={ratio}>
            <line className="plot-grid" x1="54" x2="520" y1={y(maxDepth * ratio)} y2={y(maxDepth * ratio)} />
            <text className="plot-label" x="10" y={y(maxDepth * ratio) + 4}>{Math.round(maxDepth * ratio)} m</text>
          </g>
        ))}
        {formation && (
          <g>
            {column.fluids.map((interval) => {
              const top = positionAtMd(stations, interval.top), base = positionAtMd(stations, interval.base)
              if (!top || !base) return null
              return <rect key={`${interval.value}:${interval.top}`} className={`fluid-zone ${interval.value}`} x="54"
                fill={interval.value === 'unknown' || interval.value === 'outside' ? `url(#${patternId}-${interval.value})` : undefined}
                y={Math.min(y(top.tvd), y(base.tvd))} width="466" height={Math.abs(y(base.tvd) - y(top.tvd))} />
            })}
            {positionAtMd(stations, formation.top) && <>
              <text className="reservoir-label" x="64" y={y(depthAtMd(stations, formation.top)) - 7}>{well.formationName.toUpperCase()} · FLUID INTERPRETATION</text>
              <line className="boundary-line" x1="54" x2="520" y1={y(depthAtMd(stations, formation.top))} y2={y(depthAtMd(stations, formation.top))} />
            </>}
            {positionAtMd(stations, formation.base) && <line className="boundary-line" x1="54" x2="520" y1={y(depthAtMd(stations, formation.base))} y2={y(depthAtMd(stations, formation.base))} />}
            <g className="reservoir-detail">
              <rect className="reservoir-detail-bed" x="570" y="42" width="230" height="328" />
              <text className="detail-title" x="580" y="56">{well.formationName.toUpperCase()} · FLUID COLUMN</text>
              <text className="contact-label column-top-label" x="580" y="78">FORMATION TOP · {formation.top.toFixed(1)} m MD</text>
              <text className="detail-track-title" x="580" y="100">FLUID</text>
              <text className="detail-track-title" x="720" y="100">QUALITY</text>
              {column.fluids.map((interval) => (
                <rect key={`detail:${interval.value}:${interval.top}`} className={`fluid-zone detail ${interval.value}`}
                  fill={interval.value === 'unknown' || interval.value === 'outside' ? `url(#${patternId}-${interval.value})` : undefined}
                  x="580" y={detailY(interval.top)} width="126" height={detailY(interval.base) - detailY(interval.top)}>
                  <title>{`${interval.top.toFixed(1)}–${interval.base.toFixed(1)} m MD: ${fluidLabel(interval.value)}. ${gapLabel(interval.reason)}`}</title>
                </rect>
              ))}
              {column.quality.map((interval) => (
                <rect key={`quality:${interval.value}:${interval.top}`} className={`column-quality ${interval.value}`}
                  fill={interval.value !== 'qualifying' ? `url(#${patternId}-${interval.value})` : undefined}
                  x="720" y={detailY(interval.top)} width="68" height={detailY(interval.base) - detailY(interval.top)}>
                  <title>{`${interval.top.toFixed(1)}–${interval.base.toFixed(1)} m MD: ${qualityLabel(interval.value)}. ${gapLabel(interval.reason)}`}</title>
                </rect>
              ))}
              <line className="boundary-line" x1="580" x2="788" y1="112" y2="112" />
              {column.contacts.map((contact) => (
                <line key={`${contact.type}:${contact.depth}`} className="contact-line" x1="580" x2="706" y1={detailY(contact.depth)} y2={detailY(contact.depth)}>
                  <title>{`${contact.type} inferred near ${contact.depth.toFixed(1)} m MD; not an independent contact measurement.`}</title>
                </line>
              ))}
              <line className="boundary-line" x1="580" x2="788" y1="336" y2="336" />
              <text className="contact-label" x="580" y="358">FORMATION BASE · {formation.base.toFixed(1)} m MD</text>
            </g>
          </g>
        )}
        <path className="trajectory-shadow" d={trajectory} />
        <path className="trajectory-line selected" d={trajectory} />
        {proposedPath && proposedPath.length > 1 && (
          <g className="proposed-path">
            <path d={path(proposedPath.map((station, index) => [x(proposedDeviations[index]), y(station.trueVerticalDepthM)]))} />
            <text x={x(proposedDeviations[0]) + 8} y={y(proposedPath[0].trueVerticalDepthM) - 8}>PROPOSED</text>
          </g>
        )}
        {stations.map((station, index) => <circle key={station.md} className="survey-station" cx={x(deviations[index])} cy={y(station.tvd)} r="4" />)}
        <g className="completion-column">
          <text className="plot-label" x="466" y="24">OPENING</text>
          <line x1="540" x2="540" y1="34" y2="384" />
          {well.openings.map((opening) => (
            <line key={opening.top} className="opening-interval" x1="540" x2="540"
              y1={y(depthAtMd(stations, opening.top))} y2={y(depthAtMd(stations, opening.base))} />
          ))}
        </g>
      </svg>
      </div>
      <div className="view-caption">
        <strong>{well.name}</strong>
        <span>
          <i className="legend gas" /> gas <i className="legend oil" /> oil <i className="legend water" /> water · {column.contacts.length} inferred contact{column.contacts.length === 1 ? '' : 's'}
          {proposedPath?.length ? ' · proposed path overlay' : ''}
        </span>
      </div>
      <div className="column-explanation">
        {column.error && <p role="alert">{column.error}</p>}
        {!formation && !column.error && <p>No bounded formation interval is available for a full-depth column.</p>}
        <div className="column-key">
          <span><i className="column-swatch unknown" />Hatched: unknown</span>
          <span><i className="column-swatch qualifying" />Green / solid: meets rock cutoffs</span>
          <span><i className="column-swatch below-cutoff" />Red / striped: below rock cutoffs</span>
          {column.intervals.some((band) => band.reason === 'outside') && <span><i className="column-swatch outside" />Dotted: outside selected formation</span>}
        </div>
        <p>Fluid color is independent of rock quality. Green meets both rock cutoffs; red stripes mean porosity below {((well.criteria ?? defaultRockCriteria).porosityCutoff * 100).toFixed(1)}% or permeability below {((well.criteria ?? defaultRockCriteria).permeabilityCutoffM2 / 9.869233e-16).toFixed(2)} mD—not empty rock or absent fluid.
          Neutral diagonal hatching means missing, invalid or unclassified data, or no nearby log sample. Unknown quality is neither passing nor failing; no fluid is assumed for unknown fluid intervals.</p>
        <p>{column.spacingM ? `Nominal sample spacing: ${column.spacingM.toFixed(2)} m. ` : 'Too few samples to infer interval coverage. '}
          Adjacent matching classifications are merged using sample midpoints; large sampling gaps stay unknown.
          Formation top/base set the column extent. Dashed lines are inferred fluid transitions, not separate measured contact records.
          The 3D qualifying-volume model still applies the rock cutoffs.</p>
        {column.contacts.length > 0 && <ul className="column-contacts" aria-label="Inferred fluid transitions">
          {column.contacts.map((contact) => <li key={`${contact.type}:${contact.depth}`}>
            {contact.type.replace(/([a-z])([A-Z])/g, '$1 $2')} · {contact.depth.toFixed(1)} m MD (sample-derived)
          </li>)}
        </ul>}
        {formation && <details>
          <summary>Depth intervals and reasons ({column.intervals.length})</summary>
          <div className="scenario-table-wrap" tabIndex={0} role="region" aria-label="Fluid and rock-quality interval table">
            <table className="scenario-table">
              <thead><tr><th scope="col">Top–base (m MD)</th><th scope="col">Fluid</th><th scope="col">Rock quality</th><th scope="col">Evidence / gap reason</th></tr></thead>
              <tbody>{column.intervals.map((band) => <tr key={band.top}>
                <th scope="row">{band.top.toFixed(1)}–{band.base.toFixed(1)}</th>
                <td>{fluidLabel(band.fluid)}</td><td>{qualityLabel(band.quality)}</td><td>{gapLabel(band.reason)}</td>
              </tr>)}</tbody>
            </table>
          </div>
        </details>}
      </div>
    </div>
  )
}

function LogsView({ well }: { well?: WellView }) {
  if (!well?.samples.length) return <WorkspaceEmpty text="This well has no depth-indexed petrophysics samples." />
  const samples = well.samples
  const minDepth = Math.min(...samples.map((sample) => sample.depth))
  const maxDepth = Math.max(...samples.map((sample) => sample.depth))
  const y = (depth: number) => 42 + 286 * (depth - minDepth) / Math.max(maxDepth - minDepth, 1)
  const tracks = [
    { label: 'PHIE', x: 92, width: 120, value: (sample: Sample) => sample.porosity, min: 0, max: .3, color: 'var(--cp-step-yellow)' },
    { label: 'PERM mD', x: 230, width: 120, value: (sample: Sample) => Math.log10(1 + sample.permeabilityM2 / 9.869233e-16), min: 0, max: 3, color: 'var(--cp-step-orange)' },
  ]
  const productionMax = Math.max(1, ...well.production.map((month) => month.oil + month.water))

  return (
    <div className="subsurface-view log-view">
      <svg viewBox="0 0 860 420" role="img" aria-label={`Depth-aligned petrophysics tracks for ${well.name}`}>
        <rect className="plot-bed" x="0" y="0" width="860" height="420" />
        <text className="plot-label" x="12" y="24">MD</text>
        {tracks.map((track) => (
          <g key={track.label}>
            <rect className="track-bed" x={track.x} y="32" width={track.width} height="306" />
            <text className="track-title" x={track.x + 8} y="24">{track.label}</text>
            <path fill="none" stroke={track.color} strokeWidth="3" d={samples.map((sample, index) => sample.rockKnown
              ? `${index && samples[index - 1].rockKnown ? 'L' : 'M'}${(track.x + track.width * (track.value(sample) - track.min) / (track.max - track.min)).toFixed(1)},${y(sample.depth).toFixed(1)}`
              : '').join(' ')} />
          </g>
        ))}
        <g>
          <rect className="track-bed" x="368" y="32" width="150" height="306" />
          <text className="track-title" x="376" y="24">FLUID SATURATION</text>
          {samples.slice(0, -1).map((sample, index) => {
            if (!sample.fluidKnown) return null
            const height = Math.max(2, y(samples[index + 1].depth) - y(sample.depth))
            return (
              <g key={sample.depth}>
                <rect className="fluid-water" x="368" y={y(sample.depth)} width={150 * sample.water} height={height} />
                <rect className="fluid-oil" x={368 + 150 * sample.water} y={y(sample.depth)} width={150 * sample.oil} height={height} />
                <rect className="fluid-gas" x={368 + 150 * (sample.water + sample.oil)} y={y(sample.depth)} width={150 * sample.gas} height={height} />
              </g>
            )
          })}
        </g>
        <g>
          <rect className="track-bed" x="536" y="32" width="150" height="306" />
          <text className="track-title" x="544" y="24">INTERVAL FLOW</text>
          {samples.map((sample) => {
            const total = sample.oilFlow + sample.waterFlow + sample.gasFlow / 25
            return <rect key={sample.depth} className="flow-bar" x="536" y={y(sample.depth) - 2} width={Math.min(150, total * 2.2)} height="4" />
          })}
        </g>
        {[minDepth, (minDepth + maxDepth) / 2, maxDepth].map((depth) => (
          <text key={depth} className="plot-label" x="10" y={y(depth) + 4}>{Math.round(depth)}</text>
        ))}
        <g className="production-strip">
          <text className="track-title" x="708" y="24">12-MONTH LIQUIDS</text>
          {well.production.map((month, index) => {
            const x = 710 + index * 11
            const oilHeight = 92 * month.oil / productionMax
            const waterHeight = 92 * month.water / productionMax
            return (
              <g key={month.month}>
                <rect className="fluid-water" x={x} y={338 - waterHeight} width="8" height={waterHeight} />
                <rect className="fluid-oil" x={x} y={338 - waterHeight - oilHeight} width="8" height={oilHeight} />
              </g>
            )
          })}
          <text className="plot-label" x="710" y="356">{well.production[0]?.month ?? 'No production'}</text>
          <text className="plot-label" x="790" y="356">{well.production.at(-1)?.month ?? ''}</text>
        </g>
      </svg>
      <div className="view-caption">
        <strong>{well.name} petrophysics</strong>
        <span><i className="legend water" /> water <i className="legend oil" /> oil <i className="legend gas" /> gas · {well.sourceLabel}</span>
      </div>
    </div>
  )
}

function CrossplotView({ well }: { well?: WellView }) {
  if (!well?.samples.length) return <WorkspaceEmpty text="This well has no samples for crossplotting." />
  const samples = well.samples.filter((sample) => sample.rockKnown)
  const criteria = well.criteria ?? defaultRockCriteria
  const maxPorosity = Math.max(.3, criteria.porosityCutoff, ...samples.map((sample) => sample.porosity))
  const maxLogPerm = Math.max(3, Math.log10(1 + criteria.permeabilityCutoffM2 / 9.869233e-16),
    ...samples.map((sample) => Math.log10(1 + sample.permeabilityM2 / 9.869233e-16)))
  const x = (porosity: number) => 90 + 650 * porosity / maxPorosity
  const y = (permeabilityM2: number) => 350 - 280 * Math.log10(1 + permeabilityM2 / 9.869233e-16) / maxLogPerm
  return (
    <div className="subsurface-view">
      <svg viewBox="0 0 860 420" role="img" aria-label={`Porosity permeability crossplot for ${well.name}`}>
        <rect className="plot-bed" x="0" y="0" width="860" height="420" />
        {[0, .25, .5, .75, 1].map((fraction) => fraction * maxPorosity).map((value) => (
          <g key={value}>
            <line className="plot-grid" x1={x(value)} x2={x(value)} y1="50" y2="350" />
            <text className="plot-label" x={x(value) - 10} y="374">{Math.round(value * 100)}%</text>
          </g>
        ))}
        {[0, 1, 2, 3].map((tick) => tick * maxLogPerm / 3).map((value) => (
          <g key={value}>
            <line className="plot-grid" x1="90" x2="740" y1={350 - value * 280 / maxLogPerm} y2={350 - value * 280 / maxLogPerm} />
            <text className="plot-label" x="42" y={354 - value * 280 / maxLogPerm}>{(10 ** value - 1).toFixed(0)}</text>
          </g>
        ))}
        <line className="cutoff-line" x1={x(criteria.porosityCutoff)} x2={x(criteria.porosityCutoff)} y1="50" y2="350" />
        <line className="cutoff-line" x1="90" x2="740" y1={y(criteria.permeabilityCutoffM2)} y2={y(criteria.permeabilityCutoffM2)} />
        {samples.map((sample) => {
          const phase = fluidPhase(sample) ?? 'unknown'
          return <circle key={sample.depth} className={`crossplot-point ${phase}`} cx={x(sample.porosity)} cy={y(sample.permeabilityM2)} r="6">
            <title>{`${Math.round(sample.depth)} m · ${(sample.porosity * 100).toFixed(1)}% · ${(sample.permeabilityM2 / 9.869233e-16).toFixed(1)} mD`}</title>
          </circle>
        })}
        <text className="axis-title" x="356" y="404">EFFECTIVE POROSITY</text>
        <text className="axis-title vertical" x="-250" y="20">PERMEABILITY · mD LOG SCALE</text>
      </svg>
      <div className="view-caption">
        <strong>{well.name} rock quality</strong>
        <span>Applied rock cutoffs: {(criteria.porosityCutoff * 100).toFixed(1)}% / {(criteria.permeabilityCutoffM2 / 9.869233e-16).toFixed(2)} mD. {well.samples.length - samples.length} missing/invalid rock samples omitted; gray means unknown fluid.</span>
      </div>
    </div>
  )
}

function WorkspaceEmpty({ text }: { text: string }) {
  return <div className="workspace-empty"><strong>NO PLOTTABLE SIGNAL</strong><span>{text}</span></div>
}

export function AnalysisWorkspace({
  mode,
  onMode,
  fieldPackage,
  wells,
  candidates,
  selectedCandidate,
  selectedWellId,
  onCandidate,
  onWell,
  mapsStatus,
  proposedPath,
}: {
  mode: WorkspaceMode
  onMode: (mode: WorkspaceMode) => void
  fieldPackage: FieldPackage
  wells: WellView[]
  candidates: RankedCandidate[]
  selectedCandidate?: RankedCandidate
  selectedWellId: string
  onCandidate: (candidate: RankedCandidate) => void
  onWell: (wellId: string) => void
  mapsStatus?: { configured: boolean; clientId?: string }
  proposedPath?: ProposedWellPathStation[]
}) {
  const selectedWell = wells.find((well) => well.id === selectedWellId) ?? wells[0]
  const [mapOpened, setMapOpened] = useState(mode === 'map')
  if (mode === 'map' && !mapOpened) setMapOpened(true)

  return (
    <div className="analysis-workspace">
      <div className="workspace-toolbar" role="group" aria-label="Visualization lens">
        {modes.map((item) => (
          <button key={item.id} data-tutorial={`lens-${item.id}`} aria-pressed={mode === item.id} onClick={() => onMode(item.id)}>{item.label}</button>
        ))}
        <label>
          <span>WELL</span>
          <select value={selectedWell?.id ?? ''} onChange={(event) => onWell(event.target.value)}>
            {wells.map((well) => <option key={well.id} value={well.id}>{well.name}</option>)}
          </select>
        </label>
      </div>
      <div className="workspace-viewport" data-tutorial="visualization">
        {mapOpened && <div hidden={mode !== 'map'} className="map-lens">
          <Suspense fallback={<div className="map-unconfigured"><strong>LOADING AZURE MAPS</strong></div>}>
            <AzureFieldMap fieldPackage={fieldPackage} candidates={candidates} selectedCandidate={selectedCandidate}
              selectedWellId={selectedWell?.wellId} mapsStatus={mapsStatus} onCandidate={onCandidate}
              onWell={(wellId) => {
                const bore = wells.find((well) => well.wellId === wellId)
                if (bore) onWell(bore.id)
              }} />
          </Suspense>
        </div>}
        {mode === 'section' && <SectionView well={selectedWell} proposedPath={proposedPath} />}
        {mode === 'trajectory' && <ReservoirView key={fieldPackage.fieldId} wells={wells} selectedWellId={selectedWell?.id ?? ''} onWell={onWell} proposedPath={proposedPath} />}
        {mode === 'logs' && <LogsView well={selectedWell} />}
        {mode === 'crossplot' && <CrossplotView well={selectedWell} />}
      </div>
    </div>
  )
}
