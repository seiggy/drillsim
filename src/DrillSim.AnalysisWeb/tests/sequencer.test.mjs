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
const originalFetch = globalThis.fetch
try {
  const { tasks, evidenceScopeKey, lifecycleStatus, taskOutputStatus } = await server.ssrLoadModule('/src/sequencer.ts')
  const { defaultConfiguration, configurationErrors, analysisIsStale, verifyAppliedAnalysis, predictionHandoffBlock, createEvidenceBundle } =
    await server.ssrLoadModule('/src/analysisConfiguration.ts')
  const { curveInventory, columnTotals } = await server.ssrLoadModule('/src/evidence.ts')
  const { buildFormationColumn } = await server.ssrLoadModule('/src/fluidColumn.ts')
  const { applyAnalysis } = await server.ssrLoadModule('/src/api.ts')
  const { HypothesisSequencer } = await server.ssrLoadModule('/src/components/HypothesisSequencer.tsx')
  const { TaskWorkspace } = await server.ssrLoadModule('/src/components/TaskWorkspace.tsx')
  const { HypothesisWorkbench } = await server.ssrLoadModule('/src/components/HypothesisWorkbench.tsx')
  const { AiFieldNote } = await server.ssrLoadModule('/src/components/AiFieldNote.tsx')
  const pkg = {
    fieldId: 'field', sha256: 'a'.repeat(64), generatedAt: '2026-01-01T00:00:00Z',
    field: {}, wells: [{ MetaInfo: { ID: 'well-one' } }], wellBores: [{ MetaInfo: { ID: 'bore-one' } }],
    geologicalProperties: [{
      MetaInfo: { ID: 'geo-one' }, WellBoreID: 'bore-one',
      Petrophysics: { Provenance: { DatasetName: 'Synthetic test only' },
        LogRuns: [{
          DepthValues: [100, 101, 102, 103, 104], DepthReference: 'MeasuredDepth', DepthUnit: 'm',
          Curves: [{ CanonicalMnemonic: 'PHIE', OriginalUnit: '%', CanonicalUnit: 'fraction',
            Values: [0, null, .3, .4, .5], NullFlags: [false, false, true, false, false],
            QualityFlags: [null, null, null, 'Invalid', null] }],
        }] },
    }],
    clusters: [], trajectories: [], wellBoreArchitectures: [], sourceCounts: { wells: 1, wellBores: 1 }, dataGaps: [],
  }
  const candidate = {
    candidateId: 'candidate:one', rank: 1, eastingM: 100, northingM: 200,
    longitudeDegrees: 3, latitudeDegrees: 58, nearestWellDistanceM: 600,
    p90NetPayM: 10, p50NetPayM: 20, p10NetPayM: 30, sigmaNetPayM: 8, meanPayPorosity: .2,
    meanPayPermeabilityMd: 10, relativeUncertainty: .4, score: .8,
    neighborEvidenceIds: ['well:well-one'],
    scoreComponents: { p50NetPayM: 20, porosityFactor: .2, permeabilityMd: 10, permeabilityLogFactor: 1, unpenalizedScore: 4, uncertaintyDivisor: 5 },
    uncertaintyComponents: { disagreementVarianceM2: 4, weightedDistanceM: 100, gridDiagonalM: 900,
      distanceSigmaM: 3, sigmaNetPayM: 8, quantileZScore: 1.28155, calibrated: false },
  }
  const analysis = {
    generatedAt: pkg.generatedAt, fieldId: pkg.fieldId, reservoirName: 'Formation', packageSha256: pkg.sha256,
    configuration: { ...defaultConfiguration }, configurationSha256: 'b'.repeat(64), analysisSha256: 'c'.repeat(64),
    ranking: [candidate], wellSummaries: [{ wellId: 'well-one', wellEvidenceId: 'well:well-one', geologyEvidenceIds: ['geology:geo-one'] }],
    candidateGrid: [{ ...candidate, status: 'eligible', reasons: ['Test cell'], prediction: candidate }], dataGaps: ['Test missingness warning'],
  }
  const scenario = { scenarioId: 'scenario-one', seedLabel: 'Fixture', sourceFieldId: pkg.fieldId, reservoirName: 'Formation', asOfUtc: pkg.generatedAt, status: 'Draft' }
  const resources = { scenarioId: scenario.scenarioId, loading: false, error: '' }
  assert.equal(tasks.length, 19)
  assert.equal(new Set(tasks.map((task) => task.id)).size, tasks.length)
  assert.equal(tasks.filter((task) => task.id === 'alternatives').length, 1)
  assert.ok(!tasks.some((task) => /Alternate [AB]|^Seal$|^Execute$|^Reveal$|^Score$/.test(task.label)))
  for (const [changedPkg, reservoir, changedScenario] of [
    [{ ...pkg, fieldId: 'two' }, 'Formation', scenario], [pkg, 'Other', scenario],
    [{ ...pkg, sha256: 'new' }, 'Formation', scenario], [pkg, 'Formation', { ...scenario, scenarioId: 'two' }],
    [pkg, 'Formation', { ...scenario, asOfUtc: '2027-01-01T00:00:00Z' }],
  ]) assert.notEqual(evidenceScopeKey(pkg, 'Formation', scenario), evidenceScopeKey(changedPkg, reservoir, changedScenario))

  assert.equal(lifecycleStatus('prediction', scenario, resources), 'No draft')
  assert.equal(lifecycleStatus('prediction', scenario, { ...resources, prediction: {} }), 'Draft')
  assert.equal(lifecycleStatus('prediction', scenario, { ...resources, prediction: { seal: {} } }), 'Sealed')
  assert.equal(lifecycleStatus('simulation', scenario, { ...resources, prediction: { seal: {} } }), 'Approval required')
  assert.equal(lifecycleStatus('simulation', { ...scenario, status: 'Logging' }, resources), 'Running')
  assert.equal(lifecycleStatus('simulation', { ...scenario, status: 'Failed' }, resources), 'Failed')
  assert.equal(lifecycleStatus('new-evidence', { ...scenario, status: 'ReadyToReveal' }, resources), 'Ready to publish')
  assert.equal(lifecycleStatus('new-evidence', { ...scenario, status: 'Revealed' }, resources), 'Reveal receipt unavailable', 'A state name alone is not a reveal receipt')
  assert.equal(lifecycleStatus('new-evidence', scenario, { ...resources, reveal: { status: 'Revealed' } }), 'Revealed')
  assert.equal(lifecycleStatus('evaluation', scenario, resources), 'Reveal required')
  assert.equal(lifecycleStatus('evaluation', scenario, { ...resources, scorecard: {} }), 'Scored')
  assert.equal(lifecycleStatus('prediction', scenario, { ...resources, scenarioId: 'other', prediction: { seal: {} } }), 'Loading ledger')
  assert.equal(taskOutputStatus('targets', { ...analysis, ranking: [] }, false), 'No targets')
  assert.equal(taskOutputStatus('targets', analysis, true), 'Previous result')
  assert.deepEqual(configurationErrors(defaultConfiguration), [])
  for (const invalid of [
    { version: 'future-version' }, { porosityCutoff: NaN }, { porosityCutoff: 1.1 }, { permeabilityCutoffM2: 0 },
    { permeabilityCutoffM2: Infinity }, { wellExclusionRadiusM: -1 }, { gridPointsPerAxis: 2.5 },
    { gridPointsPerAxis: 52 }, { idwNeighborCount: 0 }, { idwNeighborCount: 33 },
  ]) assert.ok(configurationErrors({ ...defaultConfiguration, ...invalid }).length)
  const changed = { ...defaultConfiguration, porosityCutoff: .25 }
  assert.equal(analysisIsStale(analysis, pkg, 'Formation', defaultConfiguration), false)
  assert.equal(analysisIsStale(analysis, pkg, 'Formation', changed), true)
  assert.equal(analysisIsStale(analysis, { ...pkg, sha256: 'changed' }, 'Formation', defaultConfiguration), true)
  assert.equal(analysisIsStale(analysis, pkg, 'Other', defaultConfiguration), true)
  assert.equal(analysisIsStale({ ...analysis, cameraRotation: 90 }, pkg, 'Formation', defaultConfiguration), false)
  assert.doesNotThrow(() => verifyAppliedAnalysis(analysis, pkg, 'Formation', defaultConfiguration))
  assert.throws(() => verifyAppliedAnalysis({ ...analysis, configuration: undefined }, pkg, 'Formation', defaultConfiguration))
  assert.throws(() => verifyAppliedAnalysis(analysis, pkg, 'Formation', changed))
  assert.throws(() => verifyAppliedAnalysis({ ...analysis, packageSha256: 'different' }, pkg, 'Formation', defaultConfiguration))
  assert.equal(predictionHandoffBlock(analysis, false, false), undefined)
  assert.equal(predictionHandoffBlock({ ...analysis, configuration: changed }, false, false), undefined, 'Configured permission is now gated by prediction capabilities and frozen-source validation')
  assert.ok(predictionHandoffBlock(analysis, true, false))
  assert.ok(predictionHandoffBlock(analysis, false, true))
  const bundle = createEvidenceBundle(pkg, analysis, candidate, 'Formation', defaultConfiguration, 'Measured evidence review', scenario)
  assert.equal(bundle.analysis.analysisSha256, analysis.analysisSha256)
  assert.deepEqual(bundle.evidence.citedEvidenceIds, ['well:well-one', 'geology:geo-one'])
  assert.equal(bundle.scope.scenarioId, scenario.scenarioId)
  assert.ok(bundle.limitations.some((limitation) => limitation.includes('located, logged screening-control summaries')))
  assert.equal(bundle.selectedTarget, candidate)
  assert.throws(() => createEvidenceBundle(pkg, analysis, candidate, 'Formation', changed, 'why', scenario), /pending settings/)
  assert.throws(() => createEvidenceBundle(pkg, analysis, candidate, 'Formation', defaultConfiguration, '', scenario), /rationale/)
  assert.throws(() => createEvidenceBundle(pkg, analysis, { candidateId: 'missing' }, 'Formation', defaultConfiguration, 'why'), /retained target/)
  assert.throws(() => createEvidenceBundle({ ...pkg, geologicalProperties: [] }, analysis, candidate, 'Formation', defaultConfiguration, 'why'), /citation/)
  const [curve] = curveInventory(pkg)
  assert.equal(curve.missing, 3)
  assert.equal(curve.zeroValues, 1, 'A valid zero is measured, not missing')
  assert.equal(curve.usable, 2)
  assert.equal(curve.nullFlagged, 1)
  assert.equal(curve.qualityFlagged, 1)
  assert.equal(curve.sourceUnit, '%')
  const samples = [100, 101, 102, 103, 104].map((depth) => ({
    depth, rockKnown: true, fluidKnown: true, porosity: .2, permeabilityM2: 1e-14, water: 1, oil: 0, gas: 0,
  }))
  const well = {
    id: 'bore-one', wellId: 'well-one', wellBoreId: 'bore-one', name: 'Fixture bore', formationName: 'Formation',
    formationRanges: [{ top: 100, base: 104 }], column: buildFormationColumn(samples, [{ top: 100, base: 104 }]),
    samples, stations: [], contacts: [], geometryWarnings: ['No covering survey'], sourceLabel: 'Synthetic test only',
  }
  const totals = columnTotals(well)
  assert.equal(totals.qualifying, 4)
  assert.equal(totals.qualifyingWater, 4)
  assert.equal(totals.qualifyingOilGas, 0, 'Water-bearing quality must not become hydrocarbon pay')
  const newColumn = buildFormationColumn(samples, well.formationRanges, changed)
  assert.equal(columnTotals({ ...well, column: newColumn }).qualifying, 0, 'Applied thresholds affect visual columns')
  assert.equal(columnTotals({ ...well, column: { intervals: [] } }), undefined, 'No bounded column is unavailable, not zero')

  const requests = []
  globalThis.fetch = async (url, options) => {
    requests.push({ url, options })
    return new Response(JSON.stringify(analysis), { status: 200 })
  }
  await applyAnalysis('field', 'Formation', defaultConfiguration, { scenarioId: 'scenario-one', asOf: pkg.generatedAt })
  assert.equal(requests.length, 1)
  assert.match(requests[0].url, /\/api\/fields\/field\/analysis$/)
  assert.equal(requests[0].options.method, 'POST')
  assert.deepEqual(JSON.parse(requests[0].options.body), { scenarioId: 'scenario-one', asOf: pkg.generatedAt, reservoir: 'Formation', configuration: defaultConfiguration })
  assert.deepEqual(Object.keys(requests[0].options.headers), ['Content-Type'])
  globalThis.fetch = async () => new Response('Invalid configuration', { status: 400 })
  await assert.rejects(applyAnalysis('field', 'Formation', changed), /400.*Invalid configuration/)

  let mutations = 0
  globalThis.fetch = async () => { mutations++; throw new Error('Navigation cannot fetch/mutate') }
  const html = renderToStaticMarkup(createElement(HypothesisSequencer, {
    active: 'targets', visited: new Set(['targets', 'evidence']), onSelect: () => mutations++, analysis, stale: false, scenario, resources,
  }))
  assert.match(html, /data-state="visited"/)
  assert.doesNotMatch(html, /data-state="reviewed"| ✓|available<\/em>/)
  assert.match(html, /class="sequence-head"[\s\S]*<\/div><div class="sequence-rail"[^>]*><div class="step-row"/,
    'Header must remain outside the scrolling task rail')
  assert.match(html, /class="sequence-rail"[^>]*tabindex="0"/, 'Rail remains keyboard-scrollable')
  const css = await readFile(new URL('../src/index.css', import.meta.url), 'utf8')
  assert.match(css, /\.sequence-rail\s*\{[^}]*overflow-x:\s*auto/)
  assert.doesNotMatch(css, /\.sequencer\s*\{[^}]*overflow-x:/, 'Navigation header must not share horizontal scroll')
  const controls = { draft: defaultConfiguration, applying: false, error: '', message: '', stale: false, onDraft: () => mutations++, onApply: () => mutations++ }
  const props = { pkg, analysis, wells: [well], selectedWellId: well.id, onWell: () => {}, candidate, onCandidate: () => {}, controls,
    scenario, onRefresh: () => mutations++, rationale: 'Test reasoning', onRationale: () => {} }
  const content = {}
  for (const taskId of ['evidence', 'qc', 'position', 'pay', 'targets', 'uncertainty', 'criteria', 'search', 'exclusions', 'model', 'bundle']) {
    content[taskId] = renderToStaticMarkup(createElement(TaskWorkspace, { ...props, taskId }))
    assert.ok(content[taskId].length > 100)
  }
  assert.match(content.qc, /Valid zeros|valid zeros/)
  assert.match(content.pay, /not hydrocarbon pay/)
  assert.match(content.criteria, /Apply analysis settings/)
  assert.match(content.search, /full grid and exclusions are tabulated/)
  assert.match(content.search, /Structural-only wells\/bores and other known field wells are not all covered/)
  assert.match(content.exclusions, /Screening-control exclusion radius/)
  assert.match(content.targets, /Nearest located screening control/)
  assert.doesNotMatch(content.targets, /Nearest well · m|Nearest-well distance/)
  assert.match(content.model, /do not recalculate API rankings/)
  assert.match(content.bundle, /Download evidence bundle JSON/)
  assert.match(content.uncertainty, /Uncalibrated/)
  for (const taskId of ['correlation', 'alternatives', 'compare']) {
    const output = renderToStaticMarkup(createElement(HypothesisWorkbench, {
      taskId, pkg, analysis, scope: { fieldId: pkg.fieldId, reservoirName: 'Formation', scenarioId: null, asOfUtc: null },
      candidateId: candidate.candidateId, stale: false, ai: null,
    }))
    assert.match(output, /Scoped saved hypotheses/)
  }
  renderToStaticMarkup(createElement(AiFieldNote, {
    status: { configured: true }, state: { running: false, output: '', error: '' }, candidate,
    actions: ['challenge'], onRun: () => mutations++,
  }))
  assert.equal(mutations, 0, 'Opening any workspace never performs a mutation or paid AI request')
  console.log('Sequencer checks passed: stable IDs, scope, artifact statuses, missingness, rock/fluid separation, configuration contract and invalidation, safe bundles, and side-effect-free task rendering.')
} finally {
  globalThis.fetch = originalFetch
  await server.close()
}
