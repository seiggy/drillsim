import { idOf, nameOf, read } from './data'
import type { FieldPackage, JsonRecord, PublicProductionSeriesMetadata, RevealReceipt, Scenario } from './types'

export const productionPhases = ['oil', 'gas', 'water'] as const
export type ProductionPhase = typeof productionPhases[number]
export type PhaseVolumes = Record<ProductionPhase, number | null>
export interface ProductionMonth extends PhaseVolumes {
  month: string
  daysOnProduction: number | null
  allocated: boolean
}
export interface RevealedProduction {
  wellId: string
  wellName: string
  months: ProductionMonth[]
}

function record(value: unknown): JsonRecord | undefined {
  return value !== null && typeof value === 'object' && !Array.isArray(value) ? value as JsonRecord : undefined
}

function quantity(value: unknown, label: string): number | null {
  if (value == null) return null
  const item = record(value)
  if (!item) throw new Error(`${label} is not a production quantity.`)
  const amount = read(item, 'Value', 'value')
  if (amount == null) return null
  if (read(item, 'Unit', 'unit') !== 'm3') throw new Error(`${label} must be in m3; no unit conversion was assumed.`)
  if (typeof amount !== 'number' || !Number.isFinite(amount) || amount < 0) {
    throw new Error(`${label} must be a finite, non-negative volume.`)
  }
  return amount
}

export function readRevealedProduction(
  fieldPackage: FieldPackage,
  scenario: Scenario,
  reveal: RevealReceipt,
  metadata: PublicProductionSeriesMetadata,
): RevealedProduction {
  if (!['Revealed', 'Scored'].includes(scenario.status) || reveal.status !== 'Revealed' ||
      reveal.scenarioId !== scenario.scenarioId || metadata.scenarioId !== scenario.scenarioId ||
      metadata.revealId !== reveal.revealId || metadata.seriesId !== reveal.productionSeriesId ||
      fieldPackage.fieldId !== reveal.clonedFieldId || scenario.clonedFieldId !== reveal.clonedFieldId ||
      !Number.isFinite(Date.parse(scenario.asOfUtc)) ||
      Date.parse(fieldPackage.generatedAt) !== Date.parse(scenario.asOfUtc) ||
      !(Date.parse(reveal.asOfUtc) <= Date.parse(scenario.asOfUtc))) {
    throw new Error('Production and field package do not belong to the current revealed scenario. Refresh the scenario.')
  }
  if (metadata.monthCount !== 60 || metadata.checkpointYears.join(',') !== '1,3,5') {
    throw new Error('Production metadata must describe 60 months with 1-, 3-, and 5-year checkpoints.')
  }
  const matches = fieldPackage.wells.filter((well) => {
    const dataset = record(read(well, 'Dataset', 'dataset'))
    const provenance = record(read(dataset, 'Provenance', 'provenance'))
    const artifacts = read(provenance, 'SourceArtifacts', 'sourceArtifacts')
    return Array.isArray(artifacts) && artifacts.some((item: unknown) =>
      read(record(item), 'ID', 'id') === metadata.seriesId)
  })
  if (matches.length !== 1 || !idOf(matches[0])) {
    throw new Error('The revealed production series must identify exactly one well in the field snapshot.')
  }
  const dataset = record(read(matches[0], 'Dataset', 'dataset'))
  const rows = read(dataset, 'MonthlyProduction', 'monthlyProduction')
  if (!Array.isArray(rows) || rows.length !== metadata.monthCount) {
    throw new Error('The field snapshot does not contain the committed 60-month production series.')
  }
  let previousMonth: number | undefined
  const months = rows.map((value: unknown): ProductionMonth => {
    const row = record(value)
    const year = read(row, 'Year', 'year')
    const month = read(row, 'Month', 'month')
    if (typeof year !== 'number' || !Number.isInteger(year) || year < 1 || year > 9999 ||
        typeof month !== 'number' || !Number.isInteger(month) || month < 1 || month > 12) {
      throw new Error('Production contains an invalid calendar month.')
    }
    const ordinal = year * 12 + month - 1
    if (previousMonth !== undefined && ordinal !== previousMonth + 1) {
      throw new Error('Production months must be consecutive and ordered; missing readings must remain explicit nulls.')
    }
    previousMonth = ordinal
    const label = `${year.toString().padStart(4, '0')}-${month.toString().padStart(2, '0')}`
    const days = read(row, 'DaysOnProduction', 'daysOnProduction')
    const daysInMonth = new Date(Date.UTC(year, month, 0)).getUTCDate()
    if (days != null && (typeof days !== 'number' || !Number.isFinite(days) || days < 0 || days > daysInMonth)) {
      throw new Error(`${label} has invalid days on production.`)
    }
    const allocated = read(row, 'IsAllocated', 'isAllocated')
    const classification = read(row, 'Classification', 'classification')
    if (typeof allocated !== 'boolean' || (classification !== 'Synthetic' && classification !== 3)) {
      throw new Error(`${label} is missing its synthetic production classification or allocation flag.`)
    }
    return {
      month: label,
      oil: quantity(read(row, 'Oil', 'oil'), `${label} oil`),
      gas: quantity(read(row, 'Gas', 'gas'), `${label} gas`),
      water: quantity(read(row, 'Water', 'water'), `${label} water`),
      daysOnProduction: days ?? null,
      allocated,
    }
  })
  return { wellId: idOf(matches[0]), wellName: nameOf(matches[0]), months }
}

export function cumulativeProduction(months: ProductionMonth[]): PhaseVolumes[] {
  const total: PhaseVolumes = { oil: 0, gas: 0, water: 0 }
  return months.map((month) => {
    for (const phase of productionPhases) {
      const previous = total[phase]
      const value = month[phase]
      total[phase] = previous === null || value === null ? null : previous + value
      if (total[phase] !== null && !Number.isFinite(total[phase])) throw new Error('Cumulative production exceeds the numeric range.')
    }
    return { ...total }
  })
}

export function productionPath(values: Array<number | null>, x: (index: number) => number, y: (value: number) => number): string {
  let connected = false
  return values.map((value, index) => {
    if (value === null) { connected = false; return '' }
    const point = `${connected ? 'L' : 'M'}${x(index).toFixed(2)},${y(value).toFixed(2)}`
    connected = true
    return point
  }).join(' ')
}
