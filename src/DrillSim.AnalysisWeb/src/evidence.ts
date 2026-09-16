import { idOf, read } from './data'
import type { WellView } from './components/AnalysisWorkspace'
import type { FieldPackage, JsonRecord } from './types'

export function record(value: unknown): JsonRecord | undefined {
  return value && typeof value === 'object' && !Array.isArray(value) ? value as JsonRecord : undefined
}
export const records = (value: unknown): JsonRecord[] =>
  Array.isArray(value) ? value.flatMap((item) => record(item) ? [record(item)!] : []) : []
const array = (value: unknown): unknown[] => Array.isArray(value) ? value : []
export const describe = (value: unknown): string => value == null || value === ''
  ? 'Not supplied' : typeof value === 'object' ? 'Unsupported metadata; see download' : String(value)

export function depthReferenceLabel(value: unknown) {
  const label = typeof value === 'number' ? ['MeasuredDepth', 'TrueVerticalDepth', 'TrueVerticalDepthSubsea'][value] : value
  if (typeof value === 'number' && label === undefined) return `Unrecognized depth reference (${value})`
  return label === 'MeasuredDepth' ? 'Measured depth (MD)' : label === 'TrueVerticalDepth' ? 'True vertical depth (TVD)'
    : label === 'TrueVerticalDepthSubsea' ? 'Subsea true vertical depth (TVDSS)' : describe(label)
}

export function curveInventory(pkg: FieldPackage) {
  return pkg.geologicalProperties.flatMap((geology) => {
    const petro = record(read(geology, 'Petrophysics', 'petrophysics'))
    const provenance = record(read(petro, 'Provenance', 'provenance'))
    const boreId = String(read(geology, 'WellBoreID', 'wellBoreID') ?? '')
    return records(read(petro, 'LogRuns', 'logRuns')).flatMap((run, runIndex) => {
      const depths = array(read(run, 'DepthValues', 'depthValues'))
      const axis = record(read(run, 'DepthAxis', 'depthAxis'))
      const positiveDown = read(axis, 'PositiveDown', 'positiveDown') ??
        read(run, 'DepthPositiveDown', 'depthPositiveDown', 'PositiveDown', 'positiveDown')
      return records(read(run, 'Curves', 'curves')).map((curve, curveIndex) => {
        const values = array(read(curve, 'Values', 'values'))
        const nulls = array(read(curve, 'NullFlags', 'nullFlags'))
        const flags = array(read(curve, 'QualityFlags', 'qualityFlags'))
        const count = Math.max(values.length, depths.length)
        let missing = 0, nullFlagged = 0, qualityFlagged = 0, zeroValues = 0
        for (let i = 0; i < count; i++) {
          const value = values[i]
          if (nulls[i] === true) nullFlagged++
          const qualityExcluded = /missing|bad.?hole|invalid/i.test(String(flags[i] ?? ''))
          if (qualityExcluded) qualityFlagged++
          if (value == null || typeof value !== 'number' || !Number.isFinite(value) || nulls[i] === true || qualityExcluded) missing++
          else if (value === 0) zeroValues++
        }
        return {
          id: `${idOf(geology)}:${runIndex}:${curveIndex}`,
          geologyEvidenceId: `geology:${idOf(geology)}`,
          boreId,
          run: idOf(run) || `Run ${runIndex + 1}`,
          runName: describe(read(run, 'Name', 'name')),
          tool: describe(read(run, 'Tool', 'tool')),
          mnemonic: describe(read(curve, 'CanonicalMnemonic', 'canonicalMnemonic')),
          sourceMnemonic: describe(read(curve, 'OriginalMnemonic', 'originalMnemonic')),
          sourceUnit: describe(read(curve, 'OriginalUnit', 'originalUnit')),
          canonicalUnit: describe(read(curve, 'CanonicalUnit', 'canonicalUnit')),
          depthReference: depthReferenceLabel(read(axis, 'Reference', 'reference') ?? read(run, 'DepthReference', 'depthReference')),
          depthUnit: describe(read(axis, 'CanonicalUnit', 'canonicalUnit') ?? read(run, 'DepthUnit', 'depthUnit')),
          depthDatum: describe(read(axis, 'Datum', 'datum')),
          depthPositiveDown: positiveDown === true ? 'Yes' : positiveDown === false ? 'No' : describe(positiveDown),
          count, missing, nullFlagged, qualityFlagged, zeroValues,
          usable: count - missing,
          lengthMismatch: depths.length !== values.length,
          provenance: provenance ?? {},
          curveMetadata: Object.fromEntries(Object.entries(curve).filter(([key]) =>
            !['values', 'nullflags', 'qualityflags'].includes(key.toLowerCase()))),
        }
      })
    })
  })
}

export function columnTotals(well: WellView) {
  if (!well.column.bounds || well.column.error) return undefined
  const result = { gross: 0, qualifying: 0, belowCutoff: 0, unknownQuality: 0, qualifyingOilGas: 0, qualifyingWater: 0, qualifyingUnknownFluid: 0 }
  for (const interval of well.column.intervals) {
    if (interval.reason === 'outside') continue
    const thickness = interval.base - interval.top
    result.gross += thickness
    if (interval.quality === 'qualifying') {
      result.qualifying += thickness
      if (interval.fluid === 'oil' || interval.fluid === 'gas') result.qualifyingOilGas += thickness
      else if (interval.fluid === 'water') result.qualifyingWater += thickness
      else result.qualifyingUnknownFluid += thickness
    } else if (interval.quality === 'below-cutoff') result.belowCutoff += thickness
    else result.unknownQuality += thickness
  }
  return result
}

export function visibleEvidenceIds(pkg: FieldPackage) {
  return new Set([
    `field:${pkg.fieldId}`,
    ...pkg.wells.map((item) => `well:${idOf(item)}`),
    ...pkg.wellBores.map((item) => `wellbore:${idOf(item)}`),
    ...pkg.geologicalProperties.map((item) => `geology:${idOf(item)}`),
    ...pkg.trajectories.map((item) => `trajectory:${idOf(item)}`),
    ...pkg.clusters.map((item) => `cluster:${idOf(item)}`),
    ...pkg.wellBoreArchitectures.map((item) => `architecture:${idOf(item)}`),
  ].filter((id) => !id.endsWith(':')))
}
