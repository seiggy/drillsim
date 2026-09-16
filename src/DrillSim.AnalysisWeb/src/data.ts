import type { JsonRecord } from './types'

export function read(record: JsonRecord | undefined, ...keys: string[]): unknown {
  return keys.map((key) => record?.[key]).find((value) => value !== undefined)
}

export function idOf(record: JsonRecord): string {
  const meta = read(record, 'MetaInfo', 'metaInfo') as JsonRecord | undefined
  return String(read(meta, 'ID', 'id') ?? '')
}

export function nameOf(record: JsonRecord): string {
  return String(read(record, 'Name', 'name') ?? idOf(record).slice(0, 8))
}

export function classificationLabel(value: unknown) {
  const labels = ['Observed', 'Human interpreted', 'Derived', 'Model estimated', 'Synthetic']
  if (typeof value === 'number') return labels[value] ?? 'Unclassified'
  if (typeof value !== 'string' || !value.trim()) return 'Unclassified'
  return labels.find(label => label.replace(/\s/g, '').toLowerCase() === value.replace(/\s/g, '').toLowerCase()) ??
    value.replace(/([a-z])([A-Z])/g, '$1 $2')
}

export function pointOf(record: JsonRecord): { east: number; north: number } | undefined {
  const point = read(record, 'ReferencePoint', 'referencePoint') as JsonRecord | undefined
  const east = read(point, 'RiemannianEast', 'riemannianEast', 'Y', 'y')
  const north = read(point, 'RiemannianNorth', 'riemannianNorth', 'X', 'x')
  return typeof east === 'number' && typeof north === 'number' ? { east, north } : undefined
}

export function reservoirOptionsOf(records: JsonRecord[]) {
  const counts = new Map<string, number>()
  records.forEach((geology) => {
    const petrophysics = read(geology, 'Petrophysics', 'petrophysics') as JsonRecord | undefined
    const intervals = read(petrophysics, 'FormationIntervals', 'formationIntervals')
    const tops = Array.isArray(intervals) && intervals.length
      ? intervals
      : read(petrophysics, 'FormationTops', 'formationTops')
    const names = new Set(
      (Array.isArray(tops) ? tops : [])
        .map((top) => String(read(top as JsonRecord, 'FormationName', 'formationName') ?? '').trim())
        .filter(Boolean),
    )
    names.forEach((name) => counts.set(name, (counts.get(name) ?? 0) + 1))
  })
  return [...counts]
    .filter(([, controls]) => controls >= 4)
    .map(([name, controls]) => ({ name, controls }))
    .sort((left, right) => right.controls - left.controls || left.name.localeCompare(right.name))
}
