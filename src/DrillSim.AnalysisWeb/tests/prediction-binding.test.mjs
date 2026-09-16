import assert from 'node:assert/strict'
import { fileURLToPath } from 'node:url'
import { createElement } from 'react'
import { renderToStaticMarkup } from 'react-dom/server'
import { createServer } from 'vite'

const server = await createServer({ root: fileURLToPath(new URL('..', import.meta.url)), server: { middlewareMode: true }, appType: 'custom' })
const originalFetch = globalThis.fetch
const storage = new Map()
globalThis.sessionStorage = { getItem: key => storage.get(key) ?? null, setItem: (key, value) => storage.set(key, value), removeItem: key => storage.delete(key) }
try {
  const bindingApi = await server.ssrLoadModule('/src/predictionBinding.ts')
  const { predictionTemplate, predictionText, parsePrediction, predictionDraftKey } = await server.ssrLoadModule('/src/prediction.ts')
  const { getPredictionCapabilities } = await server.ssrLoadModule('/src/api.ts')
  const { fillDemoForecast, demoForecastAssumption, readPredictionForm } = await server.ssrLoadModule('/src/predictionExamples.ts')
  const { defaultConfiguration } = await server.ssrLoadModule('/src/analysisConfiguration.ts')
  const { PredictionEditor } = await server.ssrLoadModule('/src/components/PredictionEditor.tsx')
  const { PredictionPane } = await server.ssrLoadModule('/src/components/ScenarioWorkspace.tsx')
  const capabilities = {
    version: 'prediction-handoff-capabilities-v1', configuredSaveSupported: true, configuredSealSupported: true,
    bindingVersion: 'prediction-analysis-binding-v1', analysisModelVersion: 'petrophysics-screening-v1',
    configurationVersion: 'analysis-configuration-v1', baselineBindingVersion: 'baseline-analysis-binding-v1',
    baselineModelVersions: ['baseline-nearest-well-v3', 'baseline-field-mean-v3', 'baseline-four-neighbor-idw-v3', 'baseline-uncertainty-aware-rank1-v3'],
    sourceScope: 'Frozen initial source only', targetRule: 'Every station retains exact easting/northing',
    minimumLocatedControls: 4, fourNeighborBaselineRule: 'Exactly four independently of main neighbors',
    limitation: 'Point-screening rock proxy, not reserves', legacyNullBindingSupported: true,
  }
  const scenario = {
    scenarioId: '11111111-1111-1111-1111-111111111111', sourceFieldId: '22222222-2222-2222-2222-222222222222',
    reservoirName: 'Target', initialAsOfUtc: '2026-01-01T00:00:00Z', asOfUtc: '2026-01-02T00:00:00Z',
    status: 'Draft', seedLabel: 'Configured fixture',
  }
  const wells = [1, 2, 3, 4].map(i => ({ MetaInfo: { ID: `well-${i}` } }))
  const pkg = {
    fieldId: scenario.sourceFieldId, sha256: 'a'.repeat(64), generatedAt: scenario.initialAsOfUtc, field: {}, wells,
    wellBores: [], clusters: [], trajectories: [], geologicalProperties: [], wellBoreArchitectures: [], sourceCounts: {}, dataGaps: [],
  }
  const configuration = { ...defaultConfiguration, porosityCutoff: .16, idwNeighborCount: 7 }
  const candidate = {
    candidateId: `configured:${'b'.repeat(64)}:candidate:05:08`, rank: 18,
    eastingM: 1250.125, northingM: -820.25, longitudeDegrees: 3, latitudeDegrees: 58,
    p90NetPayM: 4, p50NetPayM: 10, p10NetPayM: 16, sigmaNetPayM: 4, meanPayPorosity: .18,
    meanPayPermeabilityMd: 2, nearestWellDistanceM: 800, relativeUncertainty: .4, score: 1,
    neighborEvidenceIds: ['well:well-1', 'well:well-2'],
  }
  const analysis = {
    modelVersion: 'petrophysics-screening-v1', generatedAt: pkg.generatedAt, fieldId: pkg.fieldId, reservoirName: scenario.reservoirName,
    packageSha256: pkg.sha256, configuration, configurationSha256: 'b'.repeat(64), analysisSha256: 'c'.repeat(64),
    wellSummaries: wells.map((_, i) => ({ wellId: `well-${i + 1}`, eastingM: i * 100, northingM: i * 200 })), ranking: [],
    candidateGrid: [{ ...candidate, status: 'eligible', reasons: [], prediction: candidate }], dataGaps: [],
  }
  const context = { scenario, fieldPackage: pkg, analysis, candidate, capabilities }
  assert.equal(bindingApi.predictionCapabilityBlock(capabilities), undefined)
  assert.match(bindingApi.predictionCapabilityBlock(undefined), /unavailable/)
  assert.match(bindingApi.predictionCapabilityBlock({ ...capabilities, configuredSaveSupported: false }), /does not advertise/)
  assert.match(bindingApi.predictionCapabilityBlock({ ...capabilities, configuredSealSupported: false }, 'seal'), /seal support/)
  assert.match(bindingApi.predictionCapabilityBlock({ ...capabilities, bindingVersion: 'future' }), /unsupported/)
  assert.equal(bindingApi.predictionSourceBlock(context), undefined, 'Any eligible full-grid target is supported, even outside retained ranking')
  const binding = bindingApi.createPredictionBinding(context)
  assert.equal(binding.asOfUtc, new Date(scenario.initialAsOfUtc).toISOString())
  assert.notEqual(Date.parse(binding.asOfUtc), Date.parse(scenario.asOfUtc))
  assert.equal(binding.fieldId, scenario.sourceFieldId)
  assert.deepEqual(binding.configuration, configuration)
  assert.notEqual(binding.configuration, configuration, 'Binding captures configuration rather than mutating a result')
  assert.equal(binding.target.candidateId, candidate.candidateId)
  const template = JSON.parse(predictionTemplate(scenario, pkg, candidate, binding))
  assert.deepEqual(template.analysisBinding, binding)
  assert.equal(template.proposedWellPath[0].measuredDepthM, null)
  assert.equal(template.proposedWellPath[1].trueVerticalDepthM, null)
  assert.equal(template.productionForecasts[0].oilM3, null)
  assert.deepEqual(template.fluidClasses, [])
  assert.equal(template.proposedWellPath[1].eastingM, candidate.eastingM)
  assert.equal(template.proposedWellPath[1].northingM, candidate.northingM)
  const beforeExamples = JSON.stringify(template)
  const exampleText = fillDemoForecast(beforeExamples, context)
  const example = parsePrediction(exampleText)
  assert.deepEqual(example.expectedPaydirtM, binding.target.expectedPaydirtM)
  assert.deepEqual(example.citedEvidenceIds, candidate.neighborEvidenceIds)
  assert.deepEqual(example.analysisBinding, binding)
  assert.ok(example.uncertaintyAssumptions.includes(demoForecastAssumption))
  assert.ok(example.proposedWellPath.every(station => station.eastingM === candidate.eastingM && station.northingM === candidate.northingM))
  assert.equal(JSON.stringify(template), beforeExamples, 'The input template is not mutated by example generation.')
  assert.deepEqual(parsePrediction(fillDemoForecast(exampleText, context)), example, 'Applying examples again preserves existing inputs and does not duplicate warnings.')
  const personalized = structuredClone(example)
  personalized.rationale = 'Retain my review'
  personalized.proposedWellPath[1].measuredDepthM = 2600
  personalized.productionForecasts.forEach(row => { row.oilM3 = 0 })
  const retained = parsePrediction(fillDemoForecast(predictionText(personalized), context))
  assert.equal(retained.rationale, personalized.rationale)
  assert.equal(retained.proposedWellPath[1].measuredDepthM, 2600)
  assert.deepEqual(retained.productionForecasts, personalized.productionForecasts, 'Explicit zero and edited cumulative forecasts are retained.')
  assert.equal(readPredictionForm('invalid-json'), undefined)
  assert.throws(() => fillDemoForecast(exampleText, { ...context, capabilities: undefined }), /capabilities/)
  assert.throws(() => predictionTemplate(scenario, pkg, { ...candidate, eastingM: 4 }, binding), /differs/)
  for (const changed of [
    { capabilities: undefined },
    { fieldPackage: { ...pkg, fieldId: 'revealed-clone' } },
    { fieldPackage: { ...pkg, generatedAt: scenario.asOfUtc } },
    { analysis: { ...analysis, packageSha256: 'wrong' } },
    { analysis: { ...analysis, modelVersion: 'other' } },
    { analysis: { ...analysis, wellSummaries: analysis.wellSummaries.slice(0, 3) } },
    { candidate: { ...candidate, p50NetPayM: 999 } },
    { analysis: { ...analysis, candidateGrid: [{ ...analysis.candidateGrid[0], status: 'excluded' }] } },
  ]) assert.ok(bindingApi.predictionSourceBlock({ ...context, ...changed }))
  const body = {
    ...template, rationale: 'Explicit human fixture; rock screening is not economic pay.', fluidClasses: ['Water'],
    uncertaintyAssumptions: ['Uncalibrated screening proxy.'],
    proposedWellPath: [
      { measuredDepthM: 0, trueVerticalDepthM: 0, eastingM: candidate.eastingM, northingM: candidate.northingM },
      { measuredDepthM: 2000, trueVerticalDepthM: 2000, eastingM: candidate.eastingM, northingM: candidate.northingM },
    ],
    formations: [{ formationName: 'Target', topTrueVerticalDepthM: { p90: 1000, p50: 1100, p10: 1200 }, baseTrueVerticalDepthM: { p90: 1400, p50: 1500, p10: 1600 } }],
    productionForecasts: [1, 3, 5].map(year => ({ year, oilM3: 0, gasM3: 0, waterM3: year * 10 })),
  }
  assert.deepEqual(parsePrediction(predictionText(body)), body)
  assert.doesNotThrow(() => bindingApi.validatePredictionSource(body, context, 'save'))
  assert.doesNotThrow(() => bindingApi.validatePredictionSource(body, context, 'seal'))
  assert.throws(() => bindingApi.validatePredictionSource(body, { ...context, capabilities: undefined }), /capabilities/)
  for (const mutate of [
    value => { value.analysisBinding.scenarioId = 'another' },
    value => { value.analysisBinding.fieldId = 'clone' },
    value => { value.analysisBinding.asOfUtc = scenario.asOfUtc },
    value => { value.analysisBinding.analysisSha256 = 'd'.repeat(64) },
    value => { value.analysisBinding.configuration.porosityCutoff = .2 },
    value => { value.fieldPackageSha256 = 'e'.repeat(64) },
    value => { value.citedEvidenceIds = ['well:not-visible'] },
    value => { value.analysisBinding.target.eastingM += 1 },
  ]) {
    const invalid = structuredClone(body); mutate(invalid)
    assert.throws(() => bindingApi.validatePredictionSource(invalid, context))
  }
  const directional = structuredClone(body)
  directional.proposedWellPath[1].eastingM += 250
  const originalPath = structuredClone(directional.proposedWellPath)
  assert.throws(() => fillDemoForecast(predictionText(directional), context), /never flatten/)
  assert.throws(() => parsePrediction(predictionText(directional)), /Directional edits are retained, not flattened/)
  assert.deepEqual(directional.proposedWellPath, originalPath)
  const rebuiltDirectional = JSON.parse(bindingApi.rebuildPredictionBinding(predictionText(directional), context))
  assert.deepEqual(rebuiltDirectional.proposedWellPath, originalPath)
  assert.throws(() => parsePrediction(JSON.stringify(rebuiltDirectional)), /Directional/)
  const unversioned = { ...body, analysisBinding: { configuration, configurationSha256: analysis.configurationSha256, analysisSha256: analysis.analysisSha256 } }
  assert.throws(() => parsePrediction(predictionText(unversioned)), /explicit rebuild/)
  const rebuilt = parsePrediction(bindingApi.rebuildPredictionBinding(predictionText(unversioned), context))
  assert.equal(rebuilt.analysisBinding.version, 'prediction-analysis-binding-v1')
  assert.deepEqual(rebuilt.proposedWellPath, body.proposedWellPath)
  assert.deepEqual(rebuilt.productionForecasts, body.productionForecasts)
  assert.throws(() => parsePrediction(predictionText({ ...body, analysisBinding: null })), /configured candidate requires/)

  globalThis.fetch = async () => new Response('', { status: 404 })
  assert.equal(await getPredictionCapabilities(), undefined)
  globalThis.fetch = async () => { throw new Error('Capability service offline') }
  await assert.rejects(getPredictionCapabilities(), /offline/)
  globalThis.fetch = async () => new Response('{}')
  await assert.rejects(getPredictionCapabilities(), /incomplete or invalid/)
  globalThis.fetch = async () => new Response(JSON.stringify(capabilities))
  assert.deepEqual(await getPredictionCapabilities(), capabilities)

  const saved = { scenarioId: scenario.scenarioId, body, revision: 3, createdUtc: pkg.generatedAt, modifiedUtc: pkg.generatedAt, baselines: [] }
  const props = { scenario, fieldPackage: pkg, candidate, prediction: saved, sourceContext: context, onSaved: () => {} }
  const html = renderToStaticMarkup(createElement(PredictionEditor, props))
  assert.doesNotMatch(html, /<textarea id="prediction-json"[^>]*readOnly=""/)
  assert.match(html, /Advanced · prediction JSON and binding/)
  assert.match(html, /Use editable demo forecast/)
  assert.match(html, /Station 1 measured depth/)
  assert.doesNotMatch(html, /disabled=""[^>]*>Save draft/)
  assert.match(html, /disabled=""[^>]*>Seal reviewed revision/, 'Explicit saved-revision acknowledgement is still required')
  const noCapability = renderToStaticMarkup(createElement(PredictionEditor, { ...props, sourceContext: { ...context, capabilities: undefined } }))
  assert.match(noCapability, /capabilities are unavailable/)
  assert.match(noCapability, /disabled=""[^>]*>Save draft/)
  const old = renderToStaticMarkup(createElement(PredictionEditor, { ...props, prediction: { ...saved, body: unversioned } }))
  assert.match(old, /readOnly=""/)
  assert.match(old, /explicit rebuild/)
  assert.doesNotMatch(old, /disabled=""[^>]*>Rebuild binding from selected frozen target/)
  const edited = renderToStaticMarkup(createElement(PredictionEditor, { ...props, prediction: { ...saved, body: directional } }))
  assert.match(edited, /disabled=""[^>]*>Save draft/)
  assert.match(edited, /not flattened/)
  storage.set(predictionDraftKey(scenario.scenarioId), JSON.stringify({ text: '{"rationale":"LOCAL UNSAVED CONTENT"}', baseRevision: 1 }))
  const immutable = { ...saved, seal: { sha256: 'f'.repeat(64), baselinesSha256: '9'.repeat(64), sealedUtc: pkg.generatedAt } }
  const historical = renderToStaticMarkup(createElement(PredictionEditor, {
    ...props, scenario: { ...scenario, status: 'Scored' }, prediction: immutable,
    sourceContext: { ...context, capabilities: undefined, fieldPackage: { ...pkg, fieldId: 'clone' } },
  }))
  assert.doesNotMatch(historical, /<textarea/)
  assert.match(historical, /Saved revision 3 · Sealed/)
  assert.match(historical, /Download immutable saved JSON/)
  assert.ok(historical.includes(immutable.seal.sha256))
  assert.doesNotMatch(historical, /LOCAL UNSAVED CONTENT/)
  assert.doesNotMatch(historical, />Save draft|>Seal reviewed revision|>Rebuild binding/)
  assert.ok(storage.get(predictionDraftKey(scenario.scenarioId)).includes('LOCAL UNSAVED CONTENT'), 'Historical viewing preserves the separate local backup')
  const baselines = [
    { baselineId: 'old', kind: 'NearestWell', modelVersion: 'baseline-nearest-well-v2', candidateId: 'legacy', contributingEvidenceIds: [], limitation: 'Original legacy baseline' },
    { baselineId: 'new', kind: 'FourNeighborIdw', modelVersion: 'baseline-four-neighbor-idw-v3', candidateId: candidate.candidateId,
      contributingEvidenceIds: [], limitation: 'Configured four-control baseline', analysisBinding: {
        version: 'baseline-analysis-binding-v1', configuration: { ...configuration, idwNeighborCount: 4 }, configurationSha256: 'b'.repeat(64),
      } },
  ]
  const ledger = renderToStaticMarkup(createElement(PredictionPane, { scenario, prediction: { ...immutable, baselines } }))
  assert.match(ledger, /baseline-nearest-well-v2/)
  assert.match(ledger, /baseline-four-neighbor-idw-v3/)
  assert.match(ledger, /configuration IDW setting: 4/)
  assert.equal(configuration.idwNeighborCount, 7)
  console.log('Configured prediction checks passed: capabilities absent/failure, frozen T0 source scope, eligible full-grid targets, exact binding/templates, explicit old-draft rebuild, directional edits rejected without mutation, immutable historical display and baseline versions.')
} finally {
  globalThis.fetch = originalFetch
  delete globalThis.sessionStorage
  await server.close()
}
