import type { QuantileValues } from './types'

export interface FormationStructureEstimate {
  method: string
  controlCount: number
  topTrueVerticalDepthM: QuantileValues | null
  baseTrueVerticalDepthM: QuantileValues | null
  grossThicknessM: number | null
  nearestControls: Array<{ geologyEvidenceId: string; wellEvidenceId: string | null; distanceM: number;
    topTrueVerticalDepthM: number; baseTrueVerticalDepthM: number; depthBasis: string }>
  limitations: string[]
}

export const metres = (value: number | null | undefined, digits = 0) =>
  value == null || !Number.isFinite(value) ? '—' : `${value.toLocaleString(undefined, { maximumFractionDigits: digits, minimumFractionDigits: digits })} m`
