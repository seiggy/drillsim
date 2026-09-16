import { classificationLabel, idOf, nameOf, read } from './data'
import { record } from './evidence'
import type { FieldPackage, JsonRecord } from './types'

const text = (value: unknown) => typeof value === 'string' && value.trim() ? value : undefined
const canonical = (value: unknown): string => JSON.stringify(value, (_, current) =>
  record(current) ? Object.fromEntries(Object.entries(current).sort(([left], [right]) => left.localeCompare(right))) : current)

export function sourceLink(value: unknown) {
  if (typeof value !== 'string') return undefined
  try {
    const url = new URL(value)
    if (!['https:', 'http:'].includes(url.protocol) || url.username || url.password) return undefined
    return url
  } catch { return undefined }
}

export function sourceTitle(value: unknown, index: number) {
  const url = sourceLink(value)
  if (!url) return `Source ${index + 1}`
  const parts = url.pathname.split('/').filter(Boolean)
  const fileIndex = parts.indexOf('files')
  if (fileIndex >= 0 && parts[fileIndex + 1]) {
    let filename = parts[fileIndex + 1]
    try { filename = decodeURIComponent(filename) } catch { /* Keep the supplied path when percent encoding is invalid. */ }
    if (url.hostname === 'zenodo.org') {
      if (filename === 'LAS_files_Force_2020_all_wells_train_test_blind_hidden_final.zip') return 'FORCE 2020 well logs'
      if (filename === 'NPD_Casing_depth_most_wells.xlsx') return 'FORCE 2020 casing depths'
    }
    return `${url.hostname} · ${filename.replace(/_/g, ' ')}`
  }
  const layer = parts.indexOf('MapServer')
  if (url.hostname === 'factmaps.sodir.no' && layer >= 0) {
    if (parts[layer + 1] === '5000') return 'SODIR wellbore records'
    if (parts[layer + 1] === '2101') return 'SODIR formation intervals'
  }
  if (layer >= 0 && parts[layer + 1]) return `${url.hostname} · data layer ${parts[layer + 1]}`
  return url.hostname
}

interface ProvenanceRecord {
  id: string
  boreId: string
  boreName: string
  canSelectBore: boolean
  sourceIndexes: number[]
  raw: unknown
  warning?: string
}
interface SourceSummary {
  raw: JsonRecord
  title: string
  href?: string
  license: string
  attribution: string
  sha256: string
  records: ProvenanceRecord[]
}
interface ProvenanceGroup {
  dataset: string
  version: string
  classification: string
  entries: ProvenanceRecord[]
}

export function buildProvenance(pkg: FieldPackage) {
  const bores = new Map(pkg.wellBores.map(bore => [idOf(bore), nameOf(bore)]))
  const groups = new Map<string, ProvenanceGroup>()
  const sourceIndexes = new Map<string, number>()
  const sources: SourceSummary[] = []
  for (const geology of pkg.geologicalProperties) {
    const petro = record(read(geology, 'Petrophysics', 'petrophysics'))
    const raw = read(petro, 'Provenance', 'provenance')
    const provenance = record(raw)
    const dataset = text(read(provenance, 'DatasetName', 'datasetName')) ?? 'Dataset not supplied'
    const version = text(read(provenance, 'DatasetVersion', 'datasetVersion')) ?? 'Processing details not supplied'
    const classification = classificationLabel(read(provenance, 'Classification', 'classification'))
    const key = canonical([dataset, version, classification])
    let group = groups.get(key)
    if (!group) { group = { dataset, version, classification, entries: [] }; groups.set(key, group) }
    const boreId = text(read(geology, 'WellBoreID', 'wellBoreID')) ?? ''
    const entry: ProvenanceRecord = {
      id: idOf(geology), boreId, boreName: bores.get(boreId) || text(read(geology, 'Name', 'name')) || 'Wellbore not identified',
      canSelectBore: bores.has(boreId),
      sourceIndexes: [], raw: raw ?? null,
      warning: raw != null && !provenance ? 'Source metadata cannot be read. Download the record to inspect it.' : undefined,
    }
    const artifacts = read(provenance, 'SourceArtifacts', 'sourceArtifacts')
    if (artifacts != null && !Array.isArray(artifacts)) entry.warning = 'The source list cannot be read.'
    for (const artifact of Array.isArray(artifacts) ? artifacts : []) {
      const source = record(artifact)
      if (!source) { entry.warning = 'One or more source entries cannot be read.'; continue }
      const sourceKey = canonical(source)
      let index = sourceIndexes.get(sourceKey)
      if (index === undefined) {
        index = sources.length
        const url = read(source, 'Url', 'url')
        sourceIndexes.set(sourceKey, index)
        sources.push({
          raw: source, title: sourceTitle(url, index), href: sourceLink(url)?.href,
          license: text(read(source, 'License', 'license')) ?? 'License not supplied',
          attribution: text(read(source, 'Attribution', 'attribution')) ?? 'Attribution not supplied',
          sha256: text(read(source, 'SHA256', 'sha256')) ?? 'Checksum not supplied', records: [],
        })
      }
      if (!entry.sourceIndexes.includes(index)) {
        entry.sourceIndexes.push(index)
        sources[index].records.push(entry)
      }
    }
    group.entries.push(entry)
  }
  return { groups: [...groups.values()], sources }
}

export function provenanceExport(pkg: FieldPackage) {
  return {
    fieldId: pkg.fieldId, packageSha256: pkg.sha256,
    records: pkg.geologicalProperties.map(geology => ({
      geologyId: idOf(geology), wellBoreId: read(geology, 'WellBoreID', 'wellBoreID') ?? null,
      provenance: read(record(read(geology, 'Petrophysics', 'petrophysics')), 'Provenance', 'provenance') ?? null,
    })),
  }
}
