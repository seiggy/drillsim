import assert from 'node:assert/strict'
import { readFile } from 'node:fs/promises'
import { fileURLToPath } from 'node:url'
import { createElement } from 'react'
import { renderToStaticMarkup } from 'react-dom/server'
import { createServer } from 'vite'

const server = await createServer({ root: fileURLToPath(new URL('..', import.meta.url)), server: { middlewareMode: true }, appType: 'custom' })
try {
  const { buildWellViews } = await server.ssrLoadModule('/src/components/AnalysisWorkspace.tsx')
  const { buildReservoirMeshes, positionAtMd, sampleClass, cutAtDepth } = await server.ssrLoadModule('/src/reservoirGeometry.ts')
  const { ReservoirView } = await server.ssrLoadModule('/src/components/ReservoirView.tsx')
  const { buildFormationColumn } = await server.ssrLoadModule('/src/fluidColumn.ts')
  const { AnalysisWorkspace } = await server.ssrLoadModule('/src/components/AnalysisWorkspace.tsx')
  const entity = (id, rest = {}) => ({ MetaInfo: { ID: id }, ...rest })
  const origin = { east: 100000, north: 200000 }
  const positions = [[0, 0], [1000, 0], [0, 1000], [1000, 1000]]
  const months = []
  function fixture() {
    return {
      fieldId: 'field', field: { ReferencePoint: { RiemannianEast: origin.east, RiemannianNorth: origin.north } },
      wells: positions.map((_, i) => entity(`well-${i}`, { Name: `Well ${i}`, Dataset: { MonthlyProduction: months } })),
      wellBores: positions.map((_, i) => entity(`bore-${i}`, { WellID: `well-${i}`, Name: `Bore ${i}` })),
      wellBoreArchitectures: [],
      trajectories: positions.map(([east, north], i) => entity(`survey-${i}`, {
        WellBoreID: `bore-${i}`, IsDefinitive: true, TrajectoryType: 'Actual',
        TieInPoint: { MD: 0, RiemannianEast: origin.east + east, RiemannianNorth: origin.north + north },
        SurveyStationList: [0, 100, 200].map((md) => ({
          MD: md, TVD: md, RiemannianEast: origin.east + east + md / 2, RiemannianNorth: origin.north + north,
        })),
      })),
      geologicalProperties: positions.map((_, i) => entity(`geology-${i}`, {
        WellBoreID: `bore-${i}`, Petrophysics: {
          FormationIntervals: [{ FormationName: 'Target',
            TopDepth: { Value: 100, Reference: 'MeasuredDepth', Unit: 'm', PositiveDown: true },
            BaseDepth: { Value: 200, Reference: 'MeasuredDepth', Unit: 'm', PositiveDown: true } }],
          LogRuns: [{ DepthValues: Array.from({ length: 101 }, (_, i) => 100 + i), Curves:
            Object.entries({ PHIE: .2, PERM: 2e-15, SW: .2, SO: 0, SG: .8 }).map(([CanonicalMnemonic, value]) => ({
              CanonicalMnemonic, CanonicalUnit: CanonicalMnemonic === 'PERM' ? 'm2' : 'fraction',
              Values: Array(101).fill(value), NullFlags: Array(101).fill(false),
            })),
          }],
        },
      })),
    }
  }
  const views = (pkg) => buildWellViews(pkg, [], 'Target')
  const pkg = fixture(), wells = views(pkg)
  assert.equal(wells.length, 4)
  assert.equal(wells[0].fluidIntervals.length, 1)
  assert.equal(wells[0].fluidIntervals[0].fluid, 'gas')
  assert.equal(positionAtMd(wells[0].stations, 150).east, 75, 'Use downhole XY, not the collar.')
  assert.equal(positionAtMd(wells[0].stations, 201), undefined, 'Never extrapolate survey coverage.')
  assert.equal(positionAtMd(wells[0].stations, -1), undefined)
  const twoZones = fixture()
  for (const geo of twoZones.geologicalProperties) {
    const run = geo.Petrophysics.LogRuns[0]
    run.Curves.find(c => c.CanonicalMnemonic === 'PHIE').Values = run.DepthValues.map(d => d > 130 && d < 170 ? .01 : .2)
  }
  const zoned = views(twoZones)
  assert.equal(zoned[0].fluidIntervals.length, 2, 'Retain both gas intervals.')
  assert.equal(zoned[0].contacts.length, 0, 'Do not invent a fluid contact across non-pay.')
  assert.equal(sampleClass(zoned[0].samples[50]), 'nonpay')
  assert.deepEqual(zoned[0].column.bounds, { top: 100, base: 200 })
  assert.equal(zoned[0].column.fluids.length, 1, 'Fluid remains continuous through low-quality rock when the phase is known.')
  assert.equal(zoned[0].column.fluids[0].value, 'gas')
  assert.equal(zoned[0].column.quality.length, 3)
  assert.equal(zoned[0].column.quality[1].value, 'below-cutoff')
  const unknown = fixture()
  for (const geo of unknown.geologicalProperties) {
    geo.Petrophysics.LogRuns[0].Curves = geo.Petrophysics.LogRuns[0].Curves.filter(c => !['SW', 'SO', 'SG'].includes(c.CanonicalMnemonic))
  }
  assert.equal(views(unknown)[0].fluidIntervals.length, 0)
  assert.equal(sampleClass(views(unknown)[0].samples[0]), undefined)
  assert.equal(views(unknown)[0].column.fluids[0].value, 'unknown')
  assert.equal(views(unknown)[0].column.quality[0].value, 'qualifying', 'Unknown fluid is independent of known rock quality.')
  assert.equal(buildReservoirMeshes(views(unknown), 2000, 12).meshes.length, 0, 'Missing phases never create water.')
  const masked = fixture()
  masked.geologicalProperties[0].Petrophysics.LogRuns[0].Curves[0].NullFlags[50] = true
  assert.equal(sampleClass(views(masked)[0].samples[50]), undefined)
  assert.equal(views(masked)[0].column.fluids.length, 1, 'Missing porosity must not erase known gas.')
  assert.ok(views(masked)[0].column.quality.some(b => b.value === 'unknown'))
  const sparseSamples = wells[0].samples.filter(s => s.depth < 130 || s.depth > 170)
  const sparse = buildFormationColumn(sparseSamples, [{ top: 90, base: 210 }])
  for (const bands of [sparse.fluids, sparse.quality, sparse.intervals]) {
    assert.equal(bands[0].top, 90)
    assert.equal(bands.at(-1).base, 210)
    for (let i = 1; i < bands.length; i++) assert.equal(bands[i - 1].base, bands[i].top, 'Full coverage without gaps or overlaps.')
    assert.ok(bands.every(b => b.base > b.top))
  }
  assert.ok(sparse.fluids.some(b => b.value === 'unknown' && b.top < 150 && b.base > 150 && b.reason === 'unobserved'))
  assert.equal(sparse.contacts.length, 0, 'No contacts across unsampled intervals.')
  const transitioning = structuredClone(wells[0].samples)
  transitioning.forEach(s => { if (s.depth >= 150) { s.water = .9; s.gas = .1; s.porosity = .01 } })
  const transition = buildFormationColumn(transitioning, [{ top: 100, base: 200 }])
  assert.deepEqual(transition.contacts, [{ type: 'GasWaterContact', depth: 149.5 }], 'Known fluid transitions can occur in below-cutoff rock.')
  const unclassified = transitioning.map(s => s.depth === 150 ? { ...s, fluidKnown: false } : s)
  assert.equal(buildFormationColumn(unclassified, [{ top: 100, base: 200 }]).contacts.length, 0)
  const empty = buildFormationColumn([], [{ top: 100, base: 200 }])
  assert.deepEqual(empty.fluids, [{ top: 100, base: 200, value: 'unknown', reason: 'unobserved' }])
  assert.equal(buildFormationColumn(transitioning, [{ top: 100, base: 160 }, { top: 140, base: 200 }]).error?.includes('overlap'), true)
  const separate = buildFormationColumn(transitioning, [{ top: 100, base: 130 }, { top: 170, base: 200 }])
  assert.ok(separate.fluids.some(b => b.value === 'outside' && b.top === 130 && b.base === 170))
  assert.equal(separate.contacts.length, 0)
  const conflicting = [...wells[0].samples, { ...wells[0].samples[50], gas: .1, water: .9 }]
  assert.ok(buildFormationColumn(conflicting, [{ top: 100, base: 200 }]).fluids.some(b => b.reason === 'conflict'))
  const sectionHtml = renderToStaticMarkup(createElement(AnalysisWorkspace, {
    mode: 'section', onMode: () => {}, fieldPackage: pkg, wells: zoned, candidates: [],
    selectedWellId: zoned[0].id, onCandidate: () => {}, onWell: () => {},
  }))
  assert.match(sectionHtml, /FORMATION TOP · 100.0/)
  assert.match(sectionHtml, /FORMATION BASE · 200.0/)
  assert.match(sectionHtml, /QUALITY/)
  assert.match(sectionHtml, /Hatched: unknown/)
  assert.match(sectionHtml, /not empty rock or absent fluid/)
  assert.match(sectionHtml, /Depth intervals and reasons/)
  assert.match(sectionHtml, /Green \/ solid: meets rock cutoffs/)
  assert.match(sectionHtml, /Red \/ striped: below rock cutoffs/)
  assert.match(sectionHtml, /class="column-quality below-cutoff" fill="url\([^"]+-below-cutoff\)"/)
  const maskedHtml = renderToStaticMarkup(createElement(AnalysisWorkspace, {
    mode: 'section', onMode: () => {}, fieldPackage: masked, wells: views(masked), candidates: [],
    selectedWellId: wells[0].id, onCandidate: () => {}, onWell: () => {},
  }))
  assert.match(maskedHtml, /class="column-quality unknown" fill="url\([^"]+-unknown\)"/)
  const css = await readFile(new URL('../src/index.css', import.meta.url), 'utf8')
  assert.match(css, /\.column-quality\.qualifying\s*\{\s*fill:\s*var\(--cp-success\)/)
  assert.match(css, /\.column-below-bed\s*\{\s*fill:\s*var\(--cp-danger\)/)
  assert.doesNotMatch(sectionHtml, /resolved contacts|NaN|Infinity/)
  const missingSurvey = fixture()
  missingSurvey.trajectories = []
  assert.equal(buildReservoirMeshes(views(missingSurvey), 2000, 12).excludedBores, 4)
  const contradictory = fixture()
  contradictory.trajectories[0].SurveyStationList.forEach(s => { s.RiemannianEast -= origin.east; s.RiemannianNorth -= origin.north })
  assert.equal(views(contradictory)[0].stations.length, 0)
  assert.match(views(contradictory)[0].geometryWarnings.join(' '), /tie-in/)
  const ordering = fixture()
  ordering.trajectories.push({ ...ordering.trajectories[0], MetaInfo: { ID: 'zzz-plan' }, IsDefinitive: false, TrajectoryType: 'Planned', SurveyStationList: [] })
  assert.deepEqual(views(ordering)[0].stations, wells[0].stations)
  ordering.trajectories.reverse()
  assert.deepEqual(views(ordering)[0].stations, wells[0].stations)
  const wrongFormation = buildWellViews(pkg, [], 'Absent formation')
  assert.equal(wrongFormation[0].samples.length, 0)
  assert.equal(wrongFormation[0].fluidIntervals.length, 0)
  const wrongUnit = fixture()
  wrongUnit.geologicalProperties[0].Petrophysics.FormationIntervals[0].TopDepth.Unit = 'ft'
  assert.equal(views(wrongUnit)[0].formationRanges.length, 0)
  const wrongCurveUnit = fixture()
  wrongCurveUnit.geologicalProperties[0].Petrophysics.LogRuns[0].Curves.find(c => c.CanonicalMnemonic === 'PERM').CanonicalUnit = 'mD'
  assert.equal(sampleClass(views(wrongCurveUnit)[0].samples[0]), undefined)
  const mesh = buildReservoirMeshes(wells, 2000, 12)
  assert.equal(mesh.meshes.length, 1)
  assert.equal(mesh.meshes[0].fluid, 'gas')
  assert.ok(mesh.meshes[0].volumeM3 > 0)
  assert.deepEqual(buildReservoirMeshes([...wells].reverse(), 2000, 12), mesh, 'Deterministic order-independent model.')
  const gas = mesh.meshes[0]
  const edgeCounts = new Map()
  for (const [a, b, c] of gas.triangles) {
    for (const [i, j] of [[a, b], [b, c], [c, a]]) {
      const edge = i < j ? `${i}:${j}` : `${j}:${i}`
      edgeCounts.set(edge, (edgeCounts.get(edge) ?? 0) + 1)
    }
  }
  assert.ok([...edgeCounts.values()].every(n => n === 2), 'The full extracted surface must be closed.')
  assert.ok(gas.vertices.every(p => Object.values(p).every(Number.isFinite)))
  const negative = structuredClone(wells[0])
  negative.id = 'negative-control'
  negative.stations.forEach(p => { p.east += 500; p.north += 500 })
  negative.samples.forEach(s => { s.porosity = .01 })
  const constrained = buildReservoirMeshes([...wells, negative], 2000, 12).meshes[0]
  assert.ok(constrained.volumeM3 < gas.volumeM3, 'Observed non-pay must constrain the gas region.')
  negative.samples.forEach(s => { s.rockKnown = false; s.fluidKnown = false })
  const unconstrained = buildReservoirMeshes([...wells, negative], 2000, 12).meshes[0]
  assert.ok(Math.abs(unconstrained.volumeM3 / gas.volumeM3 - 1) < 1e-10, 'An unknown control must not behave like non-pay.')
  const collinear = structuredClone(wells)
  collinear.forEach((w, i) => w.stations.forEach(p => { p.north = 0; p.east += i * 1000 }))
  assert.equal(buildReservoirMeshes(collinear, 5000, 12).meshes.length, 0, 'Collinear controls do not constrain a footprint.')
  const separated = buildReservoirMeshes(zoned, 2000, 12).meshes[0]
  assert.ok(separated.volumeM3 < gas.volumeM3)
  assert.ok(separated.triangles.every(t => !t.some(i => separated.vertices[i].tvd < 140) || !t.some(i => separated.vertices[i].tvd > 160)), 'No facets bridge the non-pay gap.')
  assert.equal(buildReservoirMeshes(wells.slice(0, 2), 2000, 12).meshes.length, 0)
  assert.throws(() => buildReservoirMeshes(wells, NaN), /Geometry supports/)
  const clipped = cutAtDepth([{ east: 0, north: 0, tvd: 0 }, { east: 10, north: 0, tvd: 20 }, { east: 0, north: 10, tvd: 20 }], 10)
  assert.equal(clipped.length, 4)
  assert.ok(clipped.every(p => p.tvd >= 10))
  assert.deepEqual(cutAtDepth(clipped, 30), [])
  const html = renderToStaticMarkup(createElement(ReservoirView, { wells, selectedWellId: wells[0].id, onWell: () => {} }))
  assert.match(html, /reservoir-mesh gas/)
  assert.doesNotMatch(html, /<ellipse|NaN|Infinity|confidence:|% support/)
  assert.match(html, /not a probability/)
  assert.match(html, /Cut above/)
  for (const [, d] of html.matchAll(/class="reservoir-mesh gas[^"]*" d="([^"]+)"/g)) {
    for (const face of d.split('Z').filter(Boolean)) {
      const points = [...face.matchAll(/[ML](-?[\d.]+),(-?[\d.]+)/g)].map((m) => [Number(m[1]), Number(m[2])])
      const area = points.reduce((sum, p, i) => {
        const q = points[(i + 1) % points.length]
        return sum + p[0] * q[1] - q[0] * p[1]
      }, 0)
      assert.ok(area >= -.1, 'Batched SVG faces must not cancel each other through opposite winding.')
    }
  }
  console.log(`Reservoir checks passed: interval/unknown semantics, actual downhole coordinates, definitive surveys, closed mesh (${gas.triangles.length} facets), separated zones, determinism and cuts.`)
} finally { await server.close() }
