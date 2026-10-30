import type { WellView } from './components/AnalysisWorkspace'

export interface SurveyPoint { md: number; east: number; north: number; tvd: number }
export type Fluid = 'gas' | 'oil' | 'water'
export interface RockCriteria { porosityCutoff: number; permeabilityCutoffM2: number }
export const defaultRockCriteria: RockCriteria = { porosityCutoff: .12, permeabilityCutoffM2: 9.869233e-16 }

export function positionAtMd(stations: SurveyPoint[], md: number): SurveyPoint | undefined {
  if (!stations.length || md < stations[0].md || md > stations.at(-1)!.md || !Number.isFinite(md)) return undefined
  let lo = 0, hi = stations.length - 1
  while (lo < hi) {
    const mid = Math.floor((lo + hi) / 2)
    if (stations[mid].md < md) lo = mid + 1
    else hi = mid
  }
  const after = stations[lo]
  if (after.md === md) return { ...after }
  const before = stations[lo - 1]
  if (!before || after.md <= before.md) return undefined
  const f = (md - before.md) / (after.md - before.md)
  return {
    md, east: before.east + f * (after.east - before.east),
    north: before.north + f * (after.north - before.north),
    tvd: before.tvd + f * (after.tvd - before.tvd),
  }
}

export function sampleClass(sample: WellView['samples'][number], criteria = defaultRockCriteria): Fluid | 'nonpay' | undefined {
  const quality = rockQuality(sample, criteria)
  if (quality === 'unknown') return undefined
  if (quality === 'below-cutoff') return 'nonpay'
  return fluidPhase(sample)
}

export function rockQuality(sample: WellView['samples'][number], criteria = defaultRockCriteria): 'qualifying' | 'below-cutoff' | 'unknown' {
  if (!sample.rockKnown) return 'unknown'
  return sample.porosity >= criteria.porosityCutoff && sample.permeabilityM2 >= criteria.permeabilityCutoffM2 ? 'qualifying' : 'below-cutoff'
}

export function fluidPhase(sample: WellView['samples'][number]): Fluid | undefined {
  if (!sample.fluidKnown) return undefined
  if (sample.water >= .55) return 'water'
  if (sample.gas >= .15 && sample.gas >= sample.oil) return 'gas'
  if (sample.oil >= .15) return 'oil'
  return undefined
}
