import type { WellView } from './components/AnalysisWorkspace'
import { defaultRockCriteria, fluidPhase, rockQuality } from './reservoirGeometry'
import type { Fluid } from './reservoirGeometry'

type Range = { top: number; base: number }
type Gap = 'sample' | 'unobserved' | 'conflict' | 'outside'
export type FluidState = Fluid | 'unknown' | 'outside'
export type QualityState = 'qualifying' | 'below-cutoff' | 'unknown' | 'outside'
export interface ColumnBand<T> extends Range { value: T; reason: Gap }
export interface ColumnInterval extends Range { fluid: FluidState; quality: QualityState; reason: Gap }
export interface FormationColumn {
  bounds?: Range
  fluids: ColumnBand<FluidState>[]
  quality: ColumnBand<QualityState>[]
  intervals: ColumnInterval[]
  contacts: Array<{ type: 'GasWaterContact' | 'GasOilContact' | 'OilWaterContact'; depth: number }>
  spacingM?: number
  error?: string
}

export const fluidLabel = (value: FluidState) => ({
  gas: 'Gas', oil: 'Oil', water: 'Water', unknown: 'Unknown fluid', outside: 'Outside selected formation',
})[value]
export const qualityLabel = (value: QualityState) => ({
  qualifying: 'Meets rock cutoffs', 'below-cutoff': 'Below rock cutoffs', unknown: 'Unknown rock quality',
  outside: 'Outside selected formation',
})[value]
export function gapLabel(reason: Gap) {
  return {
    sample: 'Sample-derived; an unknown track means missing, invalid or unclassified inputs.',
    unobserved: 'No nearby log sample supports this interval.',
    conflict: 'Conflicting classifications at the same measured depth.',
    outside: 'Between separate occurrences of the selected formation; no fluid inferred here.',
  }[reason]
}

function append<T>(bands: ColumnBand<T>[], band: ColumnBand<T>) {
  if (band.base <= band.top) return
  const last = bands.at(-1)
  if (last && last.value === band.value && last.reason === band.reason && Math.abs(last.base - band.top) < 1e-7) last.base = band.base
  else bands.push(band)
}

export function buildFormationColumn(samples: WellView['samples'], ranges: Range[], criteria = defaultRockCriteria): FormationColumn {
  const result: FormationColumn = { fluids: [], quality: [], intervals: [], contacts: [] }
  if (!ranges.length) return result
  const orderedRanges = [...ranges].sort((a, b) => a.top - b.top)
  if (orderedRanges.some((r, i) => !Number.isFinite(r.top) || !Number.isFinite(r.base) || r.base <= r.top ||
      (i > 0 && r.top < orderedRanges[i - 1].base))) {
    return { ...result, error: 'Formation bounds overlap or are invalid; a continuous column cannot be inferred.' }
  }
  result.bounds = { top: orderedRanges[0].top, base: orderedRanges.at(-1)!.base }
  const ordered = [...samples].filter((s) => Number.isFinite(s.depth)).sort((a, b) => a.depth - b.depth)
  const readings: Array<{ depth: number; fluid: FluidState; quality: QualityState; reason: Gap }> = []
  for (const sample of ordered) {
    const fluid = fluidPhase(sample) ?? 'unknown', quality = rockQuality(sample, criteria)
    const last = readings.at(-1)
    if (last?.depth === sample.depth) {
      if (last.fluid !== fluid || last.quality !== quality) {
        last.fluid = 'unknown'; last.quality = 'unknown'; last.reason = 'conflict'
      }
    } else readings.push({ depth: sample.depth, fluid, quality, reason: 'sample' })
  }
  const deltas = readings.slice(1).flatMap((s, i) => orderedRanges.some((r) =>
    readings[i].depth >= r.top && s.depth <= r.base) ? [s.depth - readings[i].depth] : []).sort((a, b) => a - b)
  // A lone reading or pair cannot establish a reliable sampling interval.
  const step = deltas.length >= 2 ? deltas[Math.floor((deltas.length - 1) / 2)] : 0
  result.spacingM = step || undefined
  let cursor = result.bounds.top
  function add(top: number, base: number, fluid: FluidState, quality: QualityState, reason: Gap) {
    if (base <= top) return
    append(result.fluids, { top, base, value: fluid, reason })
    append(result.quality, { top, base, value: quality, reason })
    const last = result.intervals.at(-1)
    if (last && last.fluid === fluid && last.quality === quality && last.reason === reason && Math.abs(last.base - top) < 1e-7) last.base = base
    else result.intervals.push({ top, base, fluid, quality, reason })
  }
  for (const range of orderedRanges) {
    add(cursor, range.top, 'outside', 'outside', 'outside')
    cursor = range.top
    const local = readings.filter((s) => s.depth >= range.top && s.depth <= range.base)
    const rangeFluids: ColumnBand<FluidState>[] = []
    if (step) local.forEach((sample, i) => {
      const before = local[i - 1], after = local[i + 1]
      const top = Math.max(range.top, before && sample.depth - before.depth <= step * 1.6
        ? (before.depth + sample.depth) / 2 : sample.depth - step / 2)
      const base = Math.min(range.base, after && after.depth - sample.depth <= step * 1.6
        ? (sample.depth + after.depth) / 2 : sample.depth + step / 2)
      add(cursor, top, 'unknown', 'unknown', 'unobserved')
      append(rangeFluids, { top: cursor, base: top, value: 'unknown', reason: 'unobserved' })
      add(top, base, sample.fluid, sample.quality, sample.reason)
      append(rangeFluids, { top, base, value: sample.fluid, reason: sample.reason })
      cursor = base
    })
    add(cursor, range.base, 'unknown', 'unknown', 'unobserved')
    cursor = range.base
    rangeFluids.slice(1).forEach((band, i) => {
      const previous = rangeFluids[i]
      if (previous.base !== band.top || previous.value === band.value ||
          ['unknown', 'outside'].includes(previous.value) || ['unknown', 'outside'].includes(band.value)) return
      const pair = [previous.value, band.value].sort().join(':')
      const type = pair === 'gas:water' ? 'GasWaterContact' : pair === 'gas:oil' ? 'GasOilContact' : 'OilWaterContact'
      result.contacts.push({ type, depth: band.top })
    })
  }
  return result
}
