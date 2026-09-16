import assert from 'node:assert/strict'
import { createHash } from 'node:crypto'
import { fileURLToPath } from 'node:url'
import { createElement } from 'react'
import { renderToStaticMarkup } from 'react-dom/server'
import { createServer } from 'vite'

const server = await createServer({ root: fileURLToPath(new URL('..', import.meta.url)), server: { middlewareMode: true }, appType: 'custom' })
const originalFetch = globalThis.fetch
const storage = new Map()
globalThis.sessionStorage = {
  getItem: key => storage.get(key) ?? null, setItem: (key, value) => storage.set(key, value), removeItem: key => storage.delete(key),
}
try {
  const { prepareScenarioCreation, readScenarioCreationAttempt, verifyScenarioCreationAttempt, scenarioCreationKey, scenarioCreationError } =
    await server.ssrLoadModule('/src/scenarioCreation.ts')
  const { defaultConfiguration } = await server.ssrLoadModule('/src/analysisConfiguration.ts')
  const { createScenario } = await server.ssrLoadModule('/src/api.ts')
  const { ScenarioCreation } = await server.ssrLoadModule('/src/components/ScenarioCreation.tsx')
  const input = {
    sourceFieldId: 'original-field', reservoirName: 'SOGNEFJORD FM', name: ' Reviewed demo ',
    asOfUtc: '2026-09-16T12:30:00Z', purpose: ' Explore the synthetic well outcome ',
    assumptions: ' Editable forecasts are demonstration estimates, not calibrated truth. ',
    configuration: { ...defaultConfiguration, gridPointsPerAxis: 9, idwNeighborCount: 6 },
  }
  assert.equal(scenarioCreationError(input), '')
  for (const changed of [{ sourceFieldId: '' }, { name: '' }, { name: 'a'.repeat(101) }, { name: 'name\n' }, { purpose: '' },
    { assumptions: '' }, { asOfUtc: '2026-01-01' }, { asOfUtc: '2026-02-30T00:00:00Z' }, { asOfUtc: '2026-09-16T12:30:00-04:00' },
    { configuration: { ...input.configuration, gridPointsPerAxis: 1 } }]) {
    assert.ok(scenarioCreationError({ ...input, ...changed }))
    await assert.rejects(prepareScenarioCreation({ ...input, ...changed }))
  }
  const attempt = await prepareScenarioCreation(input)
  assert.equal(attempt.payload.seedLabel, input.name.trim())
  assert.equal(attempt.payload.asOfUtc, '2026-09-16T12:30:00.000Z')
  assert.equal(attempt.reviewed.version, 'guided-simulation-assumptions-v1')
  assert.deepEqual(attempt.reviewed.initialAnalysisSettings, input.configuration)
  assert.notEqual(attempt.reviewed.initialAnalysisSettings, input.configuration)
  assert.equal(attempt.payload.assumptionsSha256, createHash('sha256').update(JSON.stringify(attempt.reviewed)).digest('hex'))
  assert.deepEqual(await prepareScenarioCreation(input), attempt, 'Reviewing the same inputs gives the same deterministic payload.')
  assert.notEqual((await prepareScenarioCreation({ ...input, purpose: 'Different question' })).payload.assumptionsSha256, attempt.payload.assumptionsSha256)
  assert.notEqual(scenarioCreationKey('original-field', 'A'), scenarioCreationKey('original-field', 'B'))
  const restored = readScenarioCreationAttempt(JSON.stringify(attempt), input.sourceFieldId, input.reservoirName)
  await verifyScenarioCreationAttempt(restored)
  assert.throws(() => readScenarioCreationAttempt(JSON.stringify(attempt), 'different-field', input.reservoirName))
  await assert.rejects(verifyScenarioCreationAttempt({ ...restored, reviewed: { ...restored.reviewed, purpose: 'Changed after review' } }), /fingerprint/)
  const created = {
    scenarioId: 'new-scenario', sourceFieldId: input.sourceFieldId, reservoirName: input.reservoirName,
    initialAsOfUtc: attempt.payload.asOfUtc, asOfUtc: attempt.payload.asOfUtc, seedLabel: attempt.payload.seedLabel,
    assumptionsSha256: attempt.payload.assumptionsSha256, status: 'Draft',
  }
  const requests = []
  globalThis.fetch = async (url, options) => {
    requests.push({ url, options })
    if (requests.length === 1) throw new Error('Response lost after creation')
    return new Response(JSON.stringify(created))
  }
  await assert.rejects(createScenario(attempt.payload), /Response lost/)
  assert.deepEqual(await createScenario(restored.payload), created)
  assert.deepEqual(requests[0], requests[1], 'An uncertain response retries exact original payload, not a newly generated UTC cutoff.')
  assert.equal(requests[0].url, '/analysis-api/api/scenarios')
  assert.equal(requests[0].options.method, 'POST')
  assert.equal(requests[0].options.credentials, 'same-origin')
  assert.deepEqual(JSON.parse(requests[0].options.body), attempt.payload)
  assert.equal(requests.length, 2, 'Creation cannot chain analysis, preparation or a run.')
  globalThis.fetch = async () => new Response(JSON.stringify({ ...created, sourceFieldId: 'wrong' }))
  await assert.rejects(createScenario(attempt.payload), /does not match/)
  globalThis.fetch = async () => new Response(JSON.stringify({ title: 'Invalid cutoff', detail: 'Review the UTC timestamp.' }), { status: 400 })
  await assert.rejects(createScenario(attempt.payload), /Review the UTC/)
  let effects = 0
  globalThis.fetch = async () => { effects++; throw new Error('Rendering is read-only') }
  const props = { sourceFieldId: input.sourceFieldId, sourceFieldName: 'Troll original', reservoirName: input.reservoirName,
    configuration: input.configuration, onCreated: () => effects++, onClose: () => effects++ }
  const html = renderToStaticMarkup(createElement(ScenarioCreation, props))
  assert.match(html, /Scenario name/)
  assert.match(html, /Evidence cutoff \(UTC\)/)
  assert.match(html, /disabled=""[^>]*>Create scenario/)
  assert.doesNotMatch(html, /type="text"[^>]*value="[a-f0-9]{64}"/, 'No raw hash entry is required.')
  storage.set(scenarioCreationKey(input.sourceFieldId, input.reservoirName), JSON.stringify(attempt))
  const retry = renderToStaticMarkup(createElement(ScenarioCreation, props))
  assert.match(retry, /<fieldset disabled=""/)
  assert.match(retry, /Retry same scenario creation/)
  assert.match(retry, /Reviewed demo/)
  assert.equal(effects, 0)
  console.log('Scenario creation checks passed: versioned Web Crypto fingerprint, UTC validation, stable uncertain retries, response identity, local restoration and read-only rendering.')
} finally {
  globalThis.fetch = originalFetch
  delete globalThis.sessionStorage
  await server.close()
}
