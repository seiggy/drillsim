import assert from 'node:assert/strict'
import { createElement } from 'react'
import { renderToStaticMarkup } from 'react-dom/server'
import { fileURLToPath } from 'node:url'
import { createServer } from 'vite'

const server = await createServer({ root: fileURLToPath(new URL('..', import.meta.url)), server: { middlewareMode: true }, appType: 'custom' })
const originalFetch = globalThis.fetch
try {
  const api = await server.ssrLoadModule('/src/forecastDraft.ts')
  const { parsePrediction } = await server.ssrLoadModule('/src/prediction.ts')
  const { validatePredictionSource } = await server.ssrLoadModule('/src/predictionBinding.ts')
  const { defaultConfiguration } = await server.ssrLoadModule('/src/analysisConfiguration.ts')
  const { defaultSettings } = await server.ssrLoadModule('/src/settings.ts')
  const { ForecastScreen } = await server.ssrLoadModule('/src/screens/ForecastScreen.tsx')
  const capabilities = {
    version: 'prediction-handoff-capabilities-v1', configuredSaveSupported: true, configuredSealSupported: true,
    bindingVersion: 'prediction-analysis-binding-v1', analysisModelVersion: 'petrophysics-screening-v1',
    configurationVersion: 'analysis-configuration-v1', baselineBindingVersion: 'baseline-analysis-binding-v1',
    baselineModelVersions: ['baseline-nearest-well-v3', 'baseline-field-mean-v3', 'baseline-four-neighbor-idw-v3', 'baseline-uncertainty-aware-rank1-v3'],
    minimumLocatedControls: 4, legacyNullBindingSupported: true,
  }
  const scenario = { scenarioId: '11111111-1111-1111-1111-111111111111', sourceFieldId: '22222222-2222-2222-2222-222222222222',
    reservoirName: 'Target', initialAsOfUtc: '2026-01-01T00:00:00Z', asOfUtc: '2026-01-01T00:00:00Z', status: 'Draft', seedLabel: 'Fixture' }
  const wells = [1, 2, 3, 4].map(i => ({ MetaInfo: { ID: `well-${i}` } }))
  const pkg = { fieldId: scenario.sourceFieldId, sha256: 'a'.repeat(64), generatedAt: scenario.initialAsOfUtc, field: {}, wells,
    wellBores: [], clusters: [], trajectories: [], geologicalProperties: [], wellBoreArchitectures: [], sourceCounts: {}, dataGaps: [] }
  const candidate = { candidateId: `configured:${'b'.repeat(64)}:candidate:05:08`, rank: 1, eastingM: 1250.125, northingM: -820.25,
    longitudeDegrees: 3, latitudeDegrees: 58, p90NetPayM: 4, p50NetPayM: 10, p10NetPayM: 16, sigmaNetPayM: 4, meanPayPorosity: .18,
    meanPayPermeabilityMd: 2, nearestWellDistanceM: 800, relativeUncertainty: .4, score: 1, neighborEvidenceIds: ['well:well-1', 'well:well-2'] }
  const analysis = { modelVersion: 'petrophysics-screening-v1', generatedAt: pkg.generatedAt, fieldId: pkg.fieldId, reservoirName: 'Target',
    packageSha256: pkg.sha256, configuration: { ...defaultConfiguration, porosityCutoff: .16 }, configurationSha256: 'b'.repeat(64),
    analysisSha256: 'c'.repeat(64), wellSummaries: wells.map((_, i) => ({ wellId: `well-${i + 1}`, eastingM: i * 100, northingM: i * 200 })),
    ranking: [candidate], candidateGrid: [{ ...candidate, status: 'eligible', reasons: [], prediction: candidate }], dataGaps: [] }
  const context = { scenario, fieldPackage: pkg, analysis, candidate, capabilities }
  const request = api.forecastRequest(context)
  assert.deepEqual(request, { scenarioId: scenario.scenarioId, candidateId: candidate.candidateId, configuration: analysis.configuration,
    packageSha256: pkg.sha256, analysisSha256: analysis.analysisSha256 })
  const result = {
    version: 'forecast-draft-v1', promptVersion: 'forecast-draft-prompt-v3', scenarioId: scenario.scenarioId,
    candidateId: candidate.candidateId, packageSha256: pkg.sha256, analysisSha256: analysis.analysisSha256,
    configurationSha256: analysis.configurationSha256, generatedAt: '2026-01-01T00:00:00Z',
    structureEstimate: { method: 'formation-structure-idw-v1', controlCount: 4, topTrueVerticalDepthM: { p90: 1500, p50: 1550, p10: 1600 },
      baseTrueVerticalDepthM: { p90: 1700, p50: 1750, p10: 1800 }, grossThicknessM: 200, nearestControls: [], limitations: [] },
    draft: { totalDepthM: 1900, expectedPaydirtM: { p90: 3, p50: 7, p10: 12 }, formationTopTrueVerticalDepthM: { p90: 1490, p50: 1550, p10: 1610 },
      formationBaseTrueVerticalDepthM: { p90: 1690, p50: 1750, p10: 1810 }, fluidClasses: ['Gas', 'Water'],
      contactPredictions: [{ contactType: 'GWC', trueVerticalDepthM: { p90: 1600, p50: 1650, p10: 1700 } }],
      productionForecasts: [{ year: 5, oilM3: 0, gasM3: 3e6, waterM3: 9e3 }, { year: 1, oilM3: 0, gasM3: 1e6, waterM3: 1e3 }, { year: 3, oilM3: 0, gasM3: 2e6, waterM3: 4e3 }],
      uncertaintyAssumptions: ['Structure from well:well-3.'], citedEvidenceIds: ['well:well-3'], rationale: 'Nearby well:well-3 pick.', limitations: ['Uncalibrated.'] },
  }
  api.verifyForecastDraft(result, request, context)
  for (const change of [{ candidateId: 'other' }, { analysisSha256: 'd'.repeat(64) }, { version: 'old' },
    { draft: { ...result.draft, citedEvidenceIds: ['well:hidden'] } }, { draft: { ...result.draft, totalDepthM: 1700 } },
    { draft: { ...result.draft, productionForecasts: result.draft.productionForecasts.slice(1) } },
    { draft: { ...result.draft, expectedPaydirtM: { p90: 9, p50: 7, p10: 12 } } }])
    assert.throws(() => api.verifyForecastDraft({ ...result, ...change }, request, context))

  const body = parsePrediction(api.applyForecastDraft(context, result))
  validatePredictionSource(body, context)
  assert.deepEqual(body.proposedWellPath.map(s => [s.measuredDepthM, s.trueVerticalDepthM, s.eastingM, s.northingM]),
    [[0, 0, candidate.eastingM, candidate.northingM], [1900, 1900, candidate.eastingM, candidate.northingM]])
  assert.deepEqual(body.productionForecasts.map(p => p.year), [1, 3, 5])
  assert.equal(body.uncertaintyAssumptions[0], api.aiForecastAssumption)
  assert.deepEqual(body.citedEvidenceIds, ['well:well-1', 'well:well-2', 'well:well-3'])
  assert.deepEqual(body.expectedPaydirtM, { p90: 3, p50: 7, p10: 12 }, 'The forecast carries the AI net pay.')
  assert.deepEqual(body.analysisBinding.target.expectedPaydirtM, { p90: 4, p50: 10, p10: 16 }, 'The binding keeps the screening estimate as the baseline.')

  const session = { enabled: true, csrfHeaderName: 'X-DrillSim-CSRF', csrfRequestToken: 'test-csrf' }
  const calls = []
  globalThis.fetch = async (url, options) => {
    calls.push({ url, options })
    return new Response(JSON.stringify(url.endsWith('/session') ? session : result))
  }
  assert.deepEqual(await api.draftForecast(context, new AbortController().signal), result)
  assert.equal(calls.length, 2, 'One session read and one inference request; no retries or saves.')
  assert.equal(calls[1].url, '/analysis-api/api/forecast-drafts')
  assert.equal(calls[1].options.headers['X-DrillSim-CSRF'], 'test-csrf')
  assert.deepEqual(JSON.parse(calls[1].options.body), request)

  let effects = 0
  globalThis.fetch = async () => { effects++; throw new Error('No inference during render') }
  const screenProps = { run: scenario, pkg, reservoir: 'Target', candidate, settings: defaultSettings,
    onRunChanged: () => effects++, onProspects: () => effects++, onDrill: () => effects++ }
  const html = renderToStaticMarkup(createElement(ForecastScreen, screenProps))
  assert.match(html, /What will this well find\?/)
  assert.match(html, /target #1/)
  assert.match(html, /disabled=""[^>]*>Lock forecast/, 'Nothing can be locked before the frozen evidence is loaded.')
  assert.doesNotMatch(html, /Draft forecast with AI|Locked forecast/, 'The AI panel waits for the frozen source context.')
  const empty = renderToStaticMarkup(createElement(ForecastScreen, { ...screenProps, run: undefined, candidate: undefined }))
  assert.match(empty, /Choose a target first/)
  assert.equal(effects, 0, 'Rendering never requests inference, creates a run or navigates.')
  console.log('Forecast AI checks passed: exact scenario/target binding, verified citations, validator-compatible apply, guarded request, read-only Forecast screen render.')
} finally {
  globalThis.fetch = originalFetch
  await server.close()
}
