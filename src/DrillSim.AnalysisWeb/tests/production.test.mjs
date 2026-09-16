import assert from 'node:assert/strict'
import { readFile } from 'node:fs/promises'
import { fileURLToPath } from 'node:url'
import { createElement } from 'react'
import { renderToStaticMarkup } from 'react-dom/server'
import { createServer } from 'vite'

const server = await createServer({
  root: fileURLToPath(new URL('..', import.meta.url)),
  server: { middlewareMode: true },
  appType: 'custom',
})
try {
  const { readRevealedProduction, cumulativeProduction, productionPath } = await server.ssrLoadModule('/src/production.ts')
  const { ProductionPane } = await server.ssrLoadModule('/src/components/ProductionPane.tsx')
  const { ScenarioSequenceWorkspace } = await server.ssrLoadModule('/src/components/ScenarioWorkspace.tsx')
  const scenario = {
    scenarioId: 'scenario', clonedFieldId: 'clone', sourceFieldId: 'source', status: 'Scored',
    initialAsOfUtc: '2024-01-01T00:00:00Z', asOfUtc: '2024-01-02T00:00:00Z',
  }
  const reveal = {
    scenarioId: 'scenario', revealId: 'reveal', clonedFieldId: 'clone', status: 'Revealed',
    asOfUtc: scenario.asOfUtc, productionSeriesId: 'series',
  }
  const metadata = {
    scenarioId: 'scenario', revealId: 'reveal', seriesId: 'series', modelVersion: 'production-meter-v1',
    monthCount: 60, checkpointYears: [1, 3, 5], contentSha256: 'a'.repeat(64),
  }
  const quantity = (value) => ({ Value: value, Unit: 'm3' })
  const rows = Array.from({ length: 60 }, (_, index) => ({
    Year: 2024 + Math.floor(index / 12), Month: index % 12 + 1,
    Oil: quantity(index + 1), Gas: quantity(2 * (index + 1)), Water: quantity(0),
    DaysOnProduction: 20, IsAllocated: true, Classification: 'Synthetic',
  }))
  const well = {
    MetaInfo: { ID: 'well' }, Name: 'Synthetic production well',
    Dataset: { Provenance: { SourceArtifacts: [{ ID: 'series' }] }, MonthlyProduction: rows },
  }
  const pkg = {
    fieldId: 'clone', generatedAt: scenario.asOfUtc, sha256: 'b'.repeat(64),
    wells: [{ MetaInfo: { ID: 'control' }, Dataset: { MonthlyProduction: null } }, well],
  }
  const parse = (p = pkg, s = scenario, r = reveal, m = metadata) => readRevealedProduction(p, s, r, m)
  const parsed = parse()
  assert.equal(parsed.wellId, 'well')
  assert.equal(parsed.months.length, 60)
  assert.equal(parsed.months[59].month, '2028-12')
  const totals = cumulativeProduction(parsed.months)
  assert.deepEqual([11, 35, 59].map((index) => totals[index].oil), [78, 666, 1830])
  assert.equal(totals[59].gas, 3660)
  assert.equal(totals[59].water, 0)
  const missing = structuredClone(pkg)
  missing.wells[1].Dataset.MonthlyProduction[11].Oil = null
  missing.wells[1].Dataset.MonthlyProduction[35].Gas = quantity(null)
  const missingTotals = cumulativeProduction(parse(missing).months)
  assert.equal(missingTotals[10].oil, 66)
  assert.equal(missingTotals[11].oil, null)
  assert.equal(missingTotals[59].oil, null)
  assert.equal(missingTotals[59].gas, null)
  assert.equal(missingTotals[59].water, 0)
  assert.match(productionPath([1, null, 2, 3], (i) => i, (v) => v), /^M0.00,1.00\s+M2.00,2.00 L3.00,3.00$/)
  assert.equal(productionPath([null, null], (i) => i, (v) => v).trim(), '')
  assert.throws(() => parse(pkg, { ...scenario, status: 'HumanApproved' }), /revealed scenario/)
  assert.throws(() => parse(pkg, scenario, { ...reveal, status: 'Prepared' }), /revealed scenario/)
  assert.throws(() => parse({ ...pkg, fieldId: 'source' }), /revealed scenario/)
  assert.throws(() => parse(pkg, scenario, reveal, { ...metadata, scenarioId: 'foreign' }), /revealed scenario/)
  assert.throws(() => parse({ ...pkg, generatedAt: scenario.initialAsOfUtc }), /revealed scenario/)
  assert.throws(() => parse(pkg, { ...scenario, asOfUtc: 'invalid' }), /revealed scenario/)
  assert.throws(() => parse({ ...pkg, wells: [...pkg.wells, well] }), /exactly one well/)
  assert.throws(() => parse(pkg, scenario, reveal, { ...metadata, seriesId: 'foreign' }), /revealed scenario/)
  assert.throws(() => parse(pkg, scenario, reveal, { ...metadata, monthCount: 12 }), /60 months/)
  for (const [change, pattern] of [
    [(months) => { months[0].Oil.Unit = 'bbl' }, /must be in m3/],
    [(months) => { months[0].Oil.Value = -1 }, /non-negative/],
    [(months) => { months[0].Oil.Value = Number.NaN }, /finite/],
    [(months) => { months[0].Month = 0 }, /invalid calendar/],
    [(months) => { months[1].Month = 1 }, /consecutive/],
    [(months) => { months.reverse() }, /consecutive/],
    [(months) => { months.pop() }, /60-month/],
    [(months) => { months[1].DaysOnProduction = 30 }, /invalid days/],
    [(months) => { months[0].Classification = 'Observed' }, /classification/],
  ]) {
    const altered = structuredClone(pkg)
    change(altered.wells[1].Dataset.MonthlyProduction)
    assert.throws(() => parse(altered), pattern)
  }
  const props = { fieldPackage: pkg, scenario, reveal, metadata }
  const html = renderToStaticMarkup(createElement(ProductionPane, props))
  assert.match(html, /5-year monthly/)
  assert.match(html, /Monthly readings and allocation \(60 months\)/)
  assert.match(html, /1-, 3-, and 5-year cumulative/)
  assert.doesNotMatch(html, /NaN|Infinity|undefined/)
  const gaps = renderToStaticMarkup(createElement(ProductionPane, { ...props, fieldPackage: missing }))
  assert.match(gaps, /11\/12 readings available/)
  const withheld = renderToStaticMarkup(createElement(ScenarioSequenceWorkspace, {
    fieldPackage: pkg, scenario: { ...scenario, status: 'HumanApproved' },
    stepIndex: 18, resources: { loading: false, error: '' },
  }))
  assert.doesNotMatch(withheld, /production-chart|Synthetic production well/)
  const mismatch = renderToStaticMarkup(createElement(ProductionPane, { ...props, fieldPackage: { ...pkg, fieldId: 'foreign' } }))
  assert.match(mismatch, /role="alert"/)
  assert.doesNotMatch(mismatch, /production-chart/)
  const css = await readFile(new URL('../src/index.css', import.meta.url), 'utf8')
  assert.match(css, /\.stage-canvas\.scenario-stage\s*\{[^}]*display:\s*block/, 'Scenario panes must not inherit the map/readout grid.')
  console.log('Production view checks passed: scope, provenance, calendars, nulls, exact checkpoints, chart gaps and rendered states.')
} finally {
  await server.close()
}
