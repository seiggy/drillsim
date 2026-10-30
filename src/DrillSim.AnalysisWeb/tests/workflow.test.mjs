import assert from 'node:assert/strict'
import { fileURLToPath } from 'node:url'
import { createElement } from 'react'
import { renderToStaticMarkup } from 'react-dom/server'
import { createServer } from 'vite'

const server = await createServer({ root: fileURLToPath(new URL('..', import.meta.url)), server: { middlewareMode: true }, appType: 'custom' })
const originalFetch = globalThis.fetch
const storage = new Map()
globalThis.localStorage = {
  getItem: key => storage.get(key) ?? null, setItem: (key, value) => storage.set(key, String(value)), removeItem: key => storage.delete(key),
}
try {
  const { runStage, runStageLabel } = await server.ssrLoadModule('/src/workflow.ts')
  const { defaultSettings, loadSettings, saveSettings, operatorNameError, isAutomatedRun } = await server.ssrLoadModule('/src/settings.ts')
  const { defaultConfiguration } = await server.ssrLoadModule('/src/analysisConfiguration.ts')
  const { insideCoverage } = await server.ssrLoadModule('/src/prospects.ts')
  const { shortlist, ProspectsScreen } = await server.ssrLoadModule('/src/screens/ProspectsScreen.tsx')
  const { SettingsScreen } = await server.ssrLoadModule('/src/screens/SettingsScreen.tsx')

  // Five-step routing and plain run labels.
  const run = status => ({ scenarioId: 'run', sourceFieldId: 'field', reservoirName: 'Target', initialAsOfUtc: '2026-01-01T00:00:00Z',
    asOfUtc: '2026-01-01T00:00:00Z', seedLabel: 'Target #1', status })
  for (const status of ['Draft', 'Armed', 'PredictionDrafted', 'PredictionSealed']) assert.equal(runStage(run(status)), 'forecast', status)
  for (const status of ['Revealed', 'Scored']) assert.equal(runStage(run(status)), 'results', status)
  for (const status of ['HumanApproved', 'WorldBound', 'Running', 'Failed', 'Cancelled']) assert.equal(runStage(run(status)), 'drill', status)
  const labels = {
    Draft: 'forecast not started', Armed: 'forecast not started', PredictionDrafted: 'forecast in progress',
    PredictionSealed: 'forecast locked, needs approval', HumanApproved: 'ready to drill', WorldBound: 'ready to drill',
    Revealed: 'awaiting evaluation', Scored: 'results ready', Failed: 'drilling failed', Cancelled: 'cancelled', Running: 'drilling',
  }
  for (const [status, label] of Object.entries(labels)) assert.equal(runStageLabel(run(status)), label, status)
  assert.equal(runStageLabel(run('HumanApproved'), 'Failed'), 'drilling failed', 'A failed simulator run is never shown as ready to drill.')
  assert.equal(runStageLabel(run('WorldBound'), 'Failed'), 'drilling failed')
  assert.equal(runStageLabel(run('HumanApproved'), 'Cancelled'), 'cancelled')
  assert.equal(runStageLabel(run('HumanApproved'), 'Running'), 'drilling')
  assert.equal(runStageLabel(run('WorldBound'), 'AwaitingApproval'), 'drilling')
  assert.equal(runStageLabel(run('Scored'), 'Scored'), 'results ready')

  // Settings persistence.
  const settingsKey = 'drillsim-settings-v2'
  const fresh = loadSettings()
  assert.deepEqual(fresh, defaultSettings)
  assert.notEqual(fresh.analysis, defaultConfiguration, 'Defaults are copied, never shared for mutation.')
  const custom = { operatorName: 'Field analyst', analysis: { ...defaultConfiguration, porosityCutoff: .18, idwNeighborCount: 6 },
    simulatorResolution: 'Standard', autoApproveCompletion: false, hideAutomatedRuns: false }
  saveSettings(custom)
  assert.deepEqual(JSON.parse(storage.get(settingsKey)), custom)
  assert.deepEqual(loadSettings(), custom)
  storage.set(settingsKey, '{not json')
  assert.deepEqual(loadSettings(), defaultSettings, 'Corrupt storage falls back to defaults.')
  storage.set(settingsKey, JSON.stringify({ operatorName: 'Jörg\n', analysis: { gridPointsPerAxis: 1 }, simulatorResolution: 'Ultra',
    autoApproveCompletion: 'no', hideAutomatedRuns: 0 }))
  assert.deepEqual(loadSettings(), defaultSettings,
    'Invalid names, configurations and resolutions are replaced; only an explicit false disables automation.')
  storage.set(settingsKey, JSON.stringify({ analysis: { porosityCutoff: .2 } }))
  assert.deepEqual(loadSettings().analysis, { ...defaultConfiguration, porosityCutoff: .2 }, 'Partial analysis settings merge over defaults.')
  storage.set(settingsKey, JSON.stringify({ analysis: { version: 'future' } }))
  assert.deepEqual(loadSettings().analysis, defaultConfiguration)
  const realStorage = globalThis.localStorage
  globalThis.localStorage = { getItem: () => { throw new Error('blocked') }, setItem: () => { throw new Error('quota') } }
  assert.deepEqual(loadSettings(), defaultSettings, 'Blocked storage still loads defaults.')
  assert.doesNotThrow(() => saveSettings(custom), 'Settings still apply when storage is unavailable.')
  globalThis.localStorage = realStorage

  assert.equal(operatorNameError('Demo analyst'), '')
  assert.equal(operatorNameError('  ZW  '), '')
  assert.equal(operatorNameError('a'.repeat(100)), '')
  for (const name of ['', '   ', 'a'.repeat(101), 'Jörg', 'tab\there']) assert.ok(operatorNameError(name), JSON.stringify(name))
  for (const label of ['Automated live check', 'fixture: target 3', 'Live check 12', 'semi-automated run']) assert.ok(isAutomatedRun(label), label)
  for (const label of ['Target #1 · Oct 27, 2026', 'Automation review', 'Checked live']) assert.equal(isAutomatedRun(label), false, label)

  // Simulator coverage.
  const coverage = { method: 'fixture', minEastingM: 0, maxEastingM: 1000, minNorthingM: -500, maxNorthingM: 500,
    maxTrueVerticalDepthM: 3000, controlCount: 4, marginM: 100, limitation: 'Fixture' }
  assert.equal(insideCoverage({ eastingM: 99999, northingM: 99999 }), true, 'Unknown coverage hides nothing.')
  for (const point of [[0, -500], [1000, 500], [500, 0]]) assert.equal(insideCoverage({ eastingM: point[0], northingM: point[1] }, coverage), true, String(point))
  for (const point of [[-0.01, 0], [1000.01, 0], [500, -500.01], [500, 500.01]]) assert.equal(insideCoverage({ eastingM: point[0], northingM: point[1] }, coverage), false, String(point))

  // Prospects shortlist: top five drillable eligible targets by score, re-ranked 1..5.
  const candidate = (id, score, eastingM = 500, northingM = 0) => ({
    candidateId: `candidate:${id}`, rank: 100 + id, eastingM, northingM, longitudeDegrees: 3, latitudeDegrees: 60, nearestWellDistanceM: 1200,
    p90NetPayM: score / 2, p50NetPayM: score, p10NetPayM: score * 2, sigmaNetPayM: 1, meanPayPorosity: .2, meanPayPermeabilityMd: 50,
    relativeUncertainty: .3, score, neighborEvidenceIds: ['well:w1'],
  })
  const cell = (prediction, status = 'eligible') => ({ candidateId: prediction.candidateId, eastingM: prediction.eastingM, northingM: prediction.northingM,
    longitudeDegrees: 3, latitudeDegrees: 60, status, reasons: [], prediction })
  const grid = [
    cell(candidate(1, 90, 5000)), cell(candidate(2, 80, 500, 900)), // highest two are outside coverage
    cell(candidate(3, 70)), cell(candidate(4, 10)), cell(candidate(5, 60)), cell(candidate(6, 50)), cell(candidate(7, 40)),
    cell(candidate(8, 30)), cell(candidate(9, 99), 'excluded'), { ...cell(candidate(10, 95)), prediction: null },
  ]
  const analysis = { generatedAt: '2026-01-01T00:00:00Z', fieldId: 'field', packageSha256: 'a'.repeat(64), analysisSha256: 'b'.repeat(64),
    configuration: defaultConfiguration, wellSummaries: [], dataGaps: [], ranking: [candidate(11, 1000)], candidateGrid: grid }
  const list = shortlist(analysis, coverage)
  assert.deepEqual(list.targets.map(item => item.candidateId), [3, 5, 6, 7, 8].map(id => `candidate:${id}`))
  assert.deepEqual(list.targets.map(item => item.rank), [1, 2, 3, 4, 5])
  assert.deepEqual({ hiddenAbove: list.hiddenAbove, outside: list.outside, scored: list.scored }, { hiddenAbove: 2, outside: 2, scored: 8 },
    'Excluded cells and cells without predictions are not scored; the ranking fallback is ignored when the grid has eligible cells.')
  assert.equal(grid[2].prediction.rank, 103, 'Shortlist ranks are copies; analysis results are not mutated.')
  const open = shortlist(analysis)
  assert.deepEqual(open.targets.map(item => item.candidateId), [1, 2, 3, 5, 6].map(id => `candidate:${id}`))
  assert.deepEqual([open.hiddenAbove, open.outside, open.scored], [0, 0, 8])
  const fallback = shortlist({ ...analysis, candidateGrid: undefined, ranking: [candidate(12, 5), candidate(13, 50), candidate(14, 20, 9999)] }, coverage)
  assert.deepEqual(fallback.targets.map(item => [item.candidateId, item.rank]), [['candidate:13', 1], ['candidate:12', 2]])
  assert.deepEqual([fallback.hiddenAbove, fallback.outside, fallback.scored], [0, 1, 3])
  const none = shortlist({ ...analysis, candidateGrid: [cell(candidate(15, 5, -10))], ranking: [] }, coverage)
  assert.deepEqual([none.targets.length, none.hiddenAbove, none.outside, none.scored], [0, 0, 1, 1])

  // Screens render read-only, without requests.
  let requests = 0
  globalThis.fetch = async () => { requests++; throw new Error('Rendering must not send requests') }
  const pkg = { fieldId: 'field', sha256: 'a'.repeat(64), generatedAt: analysis.generatedAt, field: { Name: 'Fixture field' }, wells: [], wellBores: [],
    clusters: [], trajectories: [], geologicalProperties: [], wellBoreArchitectures: [], sourceCounts: { wells: 0 }, dataGaps: [] }
  const prospects = renderToStaticMarkup(createElement(ProspectsScreen, { pkg, analysis, reservoir: 'Target', coverage, settings: defaultSettings,
    onCandidate: () => requests++, onForecast: () => requests++ }))
  assert.match(prospects, /Where should we drill\?/)
  assert.match(prospects, /5 best-scoring points the simulator can drill/)
  assert.equal((prospects.match(/class="target-card"/g) ?? []).length, 5)
  assert.match(prospects, /Forecast target #1/)
  assert.match(prospects, /2 higher-scoring points are outside the area the simulator can drill/)
  assert.match(prospects, /2 of 8 scored points fall outside that area/)
  assert.match(prospects, /Target #1 in numbers/)
  const settingsHtml = renderToStaticMarkup(createElement(SettingsScreen, { settings: custom, analysis, onChange: () => requests++ }))
  assert.match(settingsHtml, /value="Field analyst"/)
  assert.match(settingsHtml, /value="18"/, 'Porosity is shown as a percentage.')
  assert.match(settingsHtml, /value="6"/)
  assert.match(settingsHtml, /disabled=""[^>]*>Save settings/, 'Nothing to save until a value changes.')
  assert.match(settingsHtml, /Current result: 10 grid points, 0 logged wells/)
  assert.doesNotMatch(settingsHtml, /role="alert"/)
  assert.equal(requests, 0)
  console.log('Workflow checks passed: run routing and failure-aware labels, settings persistence/validation, coverage bounds, drillable top-five shortlist and read-only Prospects/Settings renders.')
} finally {
  globalThis.fetch = originalFetch
  delete globalThis.localStorage
  await server.close()
}

