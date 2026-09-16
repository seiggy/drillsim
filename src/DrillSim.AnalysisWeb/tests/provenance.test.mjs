import assert from 'node:assert/strict'
import { createElement } from 'react'
import { renderToStaticMarkup } from 'react-dom/server'
import { fileURLToPath } from 'node:url'
import { createServer } from 'vite'

const server = await createServer({
  root: fileURLToPath(new URL('..', import.meta.url)),
  server: { middlewareMode: true }, appType: 'custom',
})
const originalFetch = globalThis.fetch
try {
  const { buildProvenance, provenanceExport, sourceLink, sourceTitle } = await server.ssrLoadModule('/src/provenance.ts')
  const { curveInventory } = await server.ssrLoadModule('/src/evidence.ts')
  const { DataSources } = await server.ssrLoadModule('/src/components/DataSources.tsx')
  const { getFieldPackage } = await server.ssrLoadModule('/src/api.ts')
  const source = { ID: 'source-one', Url: 'https://example.org/files/core_log.csv/content', License: 'CC-BY-4.0',
    Attribution: 'Original data provider', SHA256: 'a'.repeat(64) }
  const changed = { ...source, SHA256: 'b'.repeat(64) }
  const group = { DatasetName: 'Field measurements', DatasetVersion: '5 m resample', Classification: 'Derived',
    SourceArtifacts: [source, source], ProcessingNotes: { supplied: 'Preserve this in the download.' } }
  const pkg = {
    fieldId: 'field-one', sha256: 'c'.repeat(64),
    wellBores: [{ MetaInfo: { ID: 'bore-one' }, Name: '31/2-1' }, { metaInfo: { id: 'bore-two' }, name: '31/2-2' }],
    geologicalProperties: [
      { MetaInfo: { ID: 'geo-one' }, WellBoreID: 'bore-one', Petrophysics: { Provenance: group } },
      { metaInfo: { id: 'geo-two' }, wellBoreID: 'bore-two', petrophysics: { provenance: {
        ...group, Classification: 2, SourceArtifacts: [{ SHA256: source.SHA256, Attribution: source.Attribution,
          License: source.License, Url: source.Url, ID: source.ID }],
      } } },
      { MetaInfo: { ID: 'geo-three' }, WellBoreID: 'bore-two', Petrophysics: { Provenance: {
        ...group, DatasetVersion: 'Formation intervals', Classification: 'HumanInterpreted', SourceArtifacts: [changed],
      } } },
    ],
  }
  const untouched = structuredClone(pkg)
  const view = buildProvenance(pkg)
  assert.equal(view.groups.length, 2)
  assert.equal(view.groups[0].entries.length, 2)
  assert.equal(view.groups[0].classification, 'Derived')
  assert.equal(view.groups[1].classification, 'Human interpreted')
  assert.equal(view.sources.length, 2, 'Reordered fields deduplicate, changed content hashes stay distinct.')
  assert.equal(view.sources[0].records.length, 2, 'Repeated source references within one record count once.')
  assert.equal(view.groups[0].entries[0].boreName, '31/2-1')
  assert.deepEqual(view.groups[0].entries[0].sourceIndexes, [0])
  const exported = provenanceExport(pkg)
  assert.equal(exported.packageSha256, pkg.sha256)
  assert.deepEqual(exported.records[0].provenance, group, 'Downloads retain original case, duplicate references and extra metadata.')
  assert.deepEqual(pkg, untouched, 'Presentation cannot alter source metadata.')
  const html = renderToStaticMarkup(createElement(DataSources, { pkg, onWell: () => { throw new Error('Rendering selected a bore.') } }))
  assert.doesNotMatch(html, /<pre|&quot;DatasetName&quot;|<textarea/)
  assert.match(html, /Data sources/)
  assert.match(html, /5 m resample/)
  assert.match(html, /Human interpreted/)
  assert.match(html, /31\/2-1/)
  assert.match(html, /Download source metadata JSON/)
  assert.equal((html.match(/Original data provider/g) ?? []).length, 2, 'Attributions appear once per distinct source definition.')
  for (const url of ['javascript:alert(1)', 'data:text/html,unsafe', 'file:///private', 'https://secret:password@example.org', 'bad URL', null]) {
    assert.equal(sourceLink(url), undefined)
  }
  assert.equal(sourceLink('https://example.org/data')?.hostname, 'example.org')
  assert.match(sourceTitle(source.Url, 0), /core log.csv/)
  assert.match(sourceTitle('https://example.org/rest/MapServer/5000/query?where=secret', 0), /data layer 5000/)
  assert.equal(sourceTitle('https://factmaps.sodir.no/rest/MapServer/2101/query', 0), 'SODIR formation intervals')
  assert.equal(sourceTitle('https://zenodo.org/api/records/4351156/files/NPD_Casing_depth_most_wells.xlsx/content', 0), 'FORCE 2020 casing depths')
  assert.doesNotMatch(sourceTitle('https://example.org/rest/MapServer/5000/query?where=secret', 0), /where=/)
  const malformed = { ...pkg, geologicalProperties: [
    { MetaInfo: { ID: 'missing' } },
    { MetaInfo: { ID: 'malformed' }, Petrophysics: { Provenance: { DatasetName: '<script>alert(1)</script>', SourceArtifacts: [null, { Url: 'javascript:alert(1)' }] } } },
  ] }
  const missingView = buildProvenance(malformed)
  assert.equal(missingView.groups[0].dataset, 'Dataset not supplied')
  assert.equal(missingView.groups[0].entries[0].canSelectBore, false)
  assert.match(missingView.groups[1].entries[0].warning, /cannot be read/)
  const invalidHtml = renderToStaticMarkup(createElement(DataSources, { pkg: malformed, onWell: () => {} }))
  assert.doesNotMatch(invalidHtml, /href="javascript:|<script>/)
  assert.match(invalidHtml, /&lt;script&gt;/)
  assert.match(invalidHtml, /License not supplied/)
  const qc = curveInventory({ geologicalProperties: [{ MetaInfo: { ID: 'qc' }, WellBoreID: 'bore-one',
    Petrophysics: { LogRuns: [{ Name: 'Wireline', Tool: 'Density tool', DepthAxis: { Reference: 0, CanonicalUnit: 'm', Datum: 'RKB', PositiveDown: true },
      DepthValues: [1, 2], Curves: [{ CanonicalMnemonic: 'PHIE', Values: [0, null] }] }] } }] })[0]
  assert.equal(qc.depthReference, 'Measured depth (MD)')
  assert.equal(qc.depthUnit, 'm')
  assert.equal(qc.depthDatum, 'RKB')
  assert.equal(qc.depthPositiveDown, 'Yes')
  assert.equal(qc.missing, 1)
  assert.equal(qc.zeroValues, 1)
  assert.equal(qc.runName, 'Wireline')
  globalThis.fetch = async () => new Response(JSON.stringify({ title: 'Data unavailable', detail: 'The field could not be loaded.', traceId: 'internal-trace' }), { status: 500 })
  await assert.rejects(getFieldPackage('field-one'), error => error.message === '500: The field could not be loaded.')
  globalThis.fetch = async () => new Response('<html>upstream error</html>', { status: 502 })
  await assert.rejects(getFieldPackage('field-one'), error => error.message === '502: The request could not be completed. Try again.')
  console.log('Provenance checks passed: readable groups, exact source deduplication, well links, missing metadata, safe URLs, unchanged exports and nested depth-axis units.')
} finally { globalThis.fetch = originalFetch; await server.close() }
