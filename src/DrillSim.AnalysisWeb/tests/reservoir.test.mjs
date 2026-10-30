import assert from 'node:assert/strict'
import { readFile } from 'node:fs/promises'
import { fileURLToPath } from 'node:url'
import { createElement } from 'react'
import { renderToStaticMarkup } from 'react-dom/server'
import { createServer } from 'vite'

const server = await createServer({ root: fileURLToPath(new URL('..', import.meta.url)), server: { middlewareMode: true }, appType: 'custom' })
try {
  const { buildWellViews } = await server.ssrLoadModule('/src/components/AnalysisWorkspace.tsx')
  const { positionAtMd, sampleClass } = await server.ssrLoadModule('/src/reservoirGeometry.ts')
  const { ReservoirView, modelGeometry } = await server.ssrLoadModule('/src/components/ReservoirView.tsx')
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
  assert.deepEqual(zoned[0].column.fluids.map(b => b.value), ['gas', 'tight', 'gas'], 'Tight rock is not reservoir: its bound-water reading is not a fluid zone.')
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
  transitioning.forEach(s => { if (s.depth >= 150) { s.water = .9; s.gas = .1 } })
  const transition = buildFormationColumn(transitioning, [{ top: 100, base: 200 }])
  assert.deepEqual(transition.contacts, [{ type: 'GasWaterContact', depth: 149.5 }])
  const shaleWater = structuredClone(transitioning).map(s => s.depth >= 150 ? { ...s, porosity: .01 } : s)
  assert.equal(buildFormationColumn(shaleWater, [{ top: 100, base: 200 }]).contacts.length, 0, 'Water readings in tight rock never make a contact.')
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
  assert.equal(views(missingSurvey)[0].stations.length, 0)
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
  const grid = (values) => Array.from({ length: 9 }, (_, i) => values(i % 3, Math.floor(i / 3)))
  const model = {
    method: 'contact-static-model-v1', fieldId: 'field', reservoirName: 'Target', packageSha256: 'x', configurationSha256: 'y',
    grid: { minEastingM: 0, minNorthingM: 0, cellSizeM: 500, columns: 3, rows: 3, layers: 20, paddingM: 1000,
      topTvdM: grid((x) => 100 + x * 40), baseTvdM: grid((x) => 200 + x * 40), netRockM: grid(() => 100),
      gasM: grid((x) => Math.max(0, 150 - (100 + x * 40))), oilM: grid(() => 0), waterM: grid((x) => 100 - Math.max(0, 150 - (100 + x * 40))),
      porosity: grid(() => .2) },
    contacts: [{ type: 'GasWaterContact', trueVerticalDepthM: 150, deepestAboveTvdM: 148, shallowestBelowTvdM: 152, supportingSamples: 40, misclassifiedSamples: 2, wells: 4 }],
    volumes: { grossRockM3: 1e11, netRockM3: 1e11, gasNetRockM3: 3e10, oilNetRockM3: 0, hydrocarbonPoreVolumeM3: 6e9 },
    inputs: { structuralControls: 4, propertyWells: 4, fluidWells: 4, classifiedSamples: 42, medianGrossM: 100, hydrocarbon: 'gas' },
    wells: [{ wellBoreId: 'bore-0', wellId: 'well-0', name: 'Bore 0', control: true, logged: true,
      path: [[0, 0, 0], [0, 0, 120], [0, 0, 250]], topTvdM: 100, baseTvdM: 200, pickEastingM: 0, pickNorthingM: 0 }],
    targets: [{ id: 'candidate:01:01', eastingM: 500, northingM: 500, insideModel: true, topTvdM: 140, baseTvdM: 240, grossM: 100, netRockM: 100,
      gasM: 10, oilM: 0, waterM: 90, netToGross: 1, porosity: .2, permeabilityMd: 200, fluidAtTop: 'gas', nearestControlM: 700,
      nearestPropertyWellM: 700, propertyEvidenceIds: ['geology:geology-0'] }],
    limitations: ['Structure: test.'],
  }
  const geometry = modelGeometry(model)
  assert.deepEqual(geometry.bands(100, 200), [{ fluid: 'gas', from: 100, to: 150 }, { fluid: 'water', from: 150, to: 200 }], 'Gas above the contact, water below.')
  assert.deepEqual(geometry.bands(180, 280), [{ fluid: 'water', from: 180, to: 280 }], 'A reservoir top below the contact is all water.')
  assert.deepEqual(geometry.bands(100, 200).map(b => b.fluid), ['gas', 'water'])
  assert.deepEqual(modelGeometry({ ...model, contacts: [] }).bands(100, 200), [{ fluid: 'gas', from: 100, to: 200 }], 'No observed contact: hydrocarbons down to the base, as the model states.')
  const html = renderToStaticMarkup(createElement(ReservoirView, { model, selectedWellId: 'bore-0', onWell: () => {},
    target: { eastingM: 500, northingM: 500, label: '#1 · drill here' }, targetColumn: model.targets[0] }))
  assert.match(html, /class="rm-top gas l\d"/)
  assert.match(html, /class="rm-wall water"/)
  assert.match(html, /class="rm-contact"/)
  assert.match(html, /rm-well selected/)
  assert.match(html, /#1 · drill here/)
  assert.match(html, /Gas–water contact \(GWC\) at 150 m/)
  assert.match(html, /Rock volume, not reserves/)
  assert.doesNotMatch(html, /NaN|Infinity|undefined/)
  assert.match(renderToStaticMarkup(createElement(ReservoirView, { loading: true, selectedWellId: '', onWell: () => {} })), /Building the reservoir model/)
  console.log('Reservoir checks passed: interval/unknown semantics, actual downhole coordinates, definitive surveys, contact-based model bands and 3D rendering.')} finally { await server.close() }
