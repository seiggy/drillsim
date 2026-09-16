import assert from 'node:assert/strict'
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
const storage = new Map()
globalThis.sessionStorage = {
  getItem: (key) => storage.get(key) ?? null,
  setItem: (key, value) => storage.set(key, value),
  removeItem: (key) => storage.delete(key),
}
try {
  const { parsePrediction, predictionTemplate, predictionText, predictionDraftKey, readLocalPredictionDraft, predictionSealAttemptKey, readSealActionKey } =
    await server.ssrLoadModule('/src/prediction.ts')
  const { PredictionEditor } = await server.ssrLoadModule('/src/components/PredictionEditor.tsx')
  const { savePrediction, sealPrediction } = await server.ssrLoadModule('/src/api.ts')
  const scenario = { scenarioId: 'one', sourceFieldId: 'field', reservoirName: 'Target', initialAsOfUtc: '2026-01-01T00:00:00Z', asOfUtc: '2026-01-01T00:00:00Z', status: 'Draft' }
  const pkg = { fieldId: 'field', sha256: 'a'.repeat(64), generatedAt: scenario.asOfUtc }
  const candidate = {
    candidateId: 'candidate:one', eastingM: 12, northingM: 23, p90NetPayM: 10, p50NetPayM: 20, p10NetPayM: 30,
    neighborEvidenceIds: ['well:11111111-1111-1111-1111-111111111111'],
  }
  const template = JSON.parse(predictionTemplate(scenario, pkg, candidate))
  assert.equal(template.proposedWellPath[1].measuredDepthM, null)
  assert.equal(template.productionForecasts[0].oilM3, null)
  assert.deepEqual(template.fluidClasses, [])
  assert.equal(template.expectedPaydirtM.p50, 20)
  assert.equal(template.fieldPackageSha256, pkg.sha256)
  assert.deepEqual(template.citedEvidenceIds, candidate.neighborEvidenceIds)
  const body = {
    ...template, rationale: 'Synthetic fixture for editor checks.', fluidClasses: ['Gas'],
    uncertaintyAssumptions: ['Synthetic fixture, not reserves.'],
    proposedWellPath: [
      { measuredDepthM: 0, trueVerticalDepthM: 0, eastingM: 12, northingM: 23 },
      { measuredDepthM: 2000, trueVerticalDepthM: 2000, eastingM: 12, northingM: 23 },
    ],
    formations: [{
      formationName: 'Target',
      topTrueVerticalDepthM: { p90: 1500, p50: 1550, p10: 1600 },
      baseTrueVerticalDepthM: { p90: 1700, p50: 1750, p10: 1800 },
    }],
    productionForecasts: [1, 3, 5].map((year) => ({ year, oilM3: 0, gasM3: year * 100, waterM3: year * 10 })),
    contactPredictions: [{ contactType: 'GWC', trueVerticalDepthM: { p90: 1700, p50: 1750, p10: 1800 } }],
  }
  assert.deepEqual(parsePrediction(predictionText(body)), body)
  assert.deepEqual(parsePrediction(predictionText({ ...body, analysisBinding: null })), { ...body, analysisBinding: null })
  assert.throws(() => parsePrediction(predictionText({ ...body, analysisBinding: {
    configuration: { version: 'analysis-configuration-v1' }, configurationSha256: 'c'.repeat(64), analysisSha256: 'd'.repeat(64),
  } })), /explicit rebuild/)
  for (const mutate of [
    (x) => { delete x.proposedWellPath[1].measuredDepthM },
    (x) => { x.proposedWellPath[1].trueVerticalDepthM = null },
    (x) => { delete x.productionForecasts[0].oilM3 },
    (x) => { x.productionForecasts[0].oilM3 = '0' },
    (x) => { x.expectedPaydirtM.p50 = null },
    (x) => { x.typo = 3 },
    (x) => { x.fluidClasses = ['Unknown'] },
    (x) => { x.fluidClasses = [['Oil']] },
    (x) => { x.contactPredictions[0].contactType = 'OWG' },
    (x) => { x.citedEvidenceIds = [5] },
    (x) => { x.formations[0].formationName = '' },
  ]) {
    const invalid = structuredClone(body)
    mutate(invalid)
    assert.throws(() => parsePrediction(JSON.stringify(invalid)))
  }
  assert.throws(() => parsePrediction('{"x": 1e999}'))
  assert.throws(() => parsePrediction('null'))
  assert.throws(() => readLocalPredictionDraft('{"text":"draft","baseRevision":-1}'))
  assert.notEqual(predictionDraftKey('one'), predictionDraftKey('two'))
  assert.notEqual(predictionSealAttemptKey('one', 1), predictionSealAttemptKey('one', 2))
  assert.notEqual(predictionSealAttemptKey('one', 1), predictionSealAttemptKey('two', 1))
  assert.equal(readSealActionKey('stable-key'), 'stable-key')
  assert.throws(() => readSealActionKey('invalid key'))
  const local = { text: 'unparseable JSON edits are still preserved', baseRevision: 3 }
  assert.deepEqual(readLocalPredictionDraft(JSON.stringify(local)), local)
  const saved = { scenarioId: 'one', body, revision: 3, seal: null, approval: null, baselines: [] }
  const session = { enabled: true, csrfRequestToken: 'csrf-seal-test-token', csrfHeaderName: 'X-DrillSim-CSRF' }
  const requests = []
  globalThis.fetch = async (url, options) => {
    requests.push({ url, options })
    const response = url.endsWith('/session') ? session : url.endsWith('/seal') ? { ...saved, seal: { sha256: 'b'.repeat(64) } } : saved
    return new Response(JSON.stringify(response), { status: 200, headers: { ETag: '"3"' } })
  }
  await savePrediction('one', body, null)
  await savePrediction('one', body, 3)
  await sealPrediction('one', 3, 'stable-seal-key')
  assert.equal(requests[0].options.headers['If-Match'], undefined)
  assert.equal(requests[1].options.headers['If-Match'], '"3"')
  assert.match(requests[2].url, /\/api\/operator\/session$/)
  assert.equal(requests[3].options.headers['If-Match'], '"3"')
  assert.equal(requests[3].options.headers['X-DrillSim-CSRF'], session.csrfRequestToken)
  assert.equal(requests[3].options.headers['Idempotency-Key'], 'stable-seal-key')
  assert.equal(requests[3].options.credentials, 'same-origin')
  assert.equal(requests[3].options.method, 'POST')
  assert.equal(requests[3].options.body, undefined)
  assert.match(requests[3].url, /\/prediction\/seal$/)
  assert.ok(requests.every(({ options }) => !options.headers?.['X-DrillSim-Human-Actor']))
  await sealPrediction('one', 3, 'stable-seal-key')
  assert.equal(requests.at(-1).options.headers['Idempotency-Key'], requests[3].options.headers['Idempotency-Key'])
  globalThis.fetch = async (url) => url.endsWith('/session') ? new Response(JSON.stringify(session)) : new Response('Draft changed', { status: 409 })
  await assert.rejects(sealPrediction('one', 2, 'stable-seal-key'), /409.*Draft changed/)
  let blockedRequests = 0
  globalThis.fetch = async () => { blockedRequests++; return new Response(JSON.stringify({ ...session, enabled: false, reason: 'Operator disabled' })) }
  await assert.rejects(sealPrediction('one', 3, 'stable-seal-key'), /Operator disabled/)
  assert.equal(blockedRequests, 1, 'Disabled session cannot send a seal mutation')
  await assert.rejects(sealPrediction('one', 3, ''), /attempt key/)
  assert.equal(blockedRequests, 1, 'Missing action key cannot even request a session')
  globalThis.fetch = async (url) => new Response(JSON.stringify(url.endsWith('/session') ? session : saved))
  await assert.rejects(sealPrediction('one', 3, 'stable-seal-key'), /did not confirm a sealed prediction/)
  globalThis.fetch = async () => new Response(JSON.stringify({ ...saved, scenarioId: 'two' }))
  await assert.rejects(savePrediction('one', body, 3), /different scenario/)
  const props = { scenario, fieldPackage: pkg, prediction: saved, candidate, onSaved: () => {} }
  const html = renderToStaticMarkup(createElement(PredictionEditor, props))
  assert.match(html, /Save draft/)
  assert.match(html, /<textarea id="prediction-json"/)
  assert.doesNotMatch(html, /readOnly=""/)
  assert.match(html, /type="checkbox"/)
  assert.match(html, /disabled=""[^>]*>Seal reviewed revision/)
  const blocked = renderToStaticMarkup(createElement(PredictionEditor, { ...props, handoffBlock: 'Configured handoff blocked.' }))
  assert.match(blocked, /Configured handoff blocked/)
  assert.match(blocked, /disabled=""[^>]*>Save draft/)
  assert.match(blocked, /disabled=""[^>]*>Seal reviewed revision/)
  const bound = renderToStaticMarkup(createElement(PredictionEditor, {
    ...props, prediction: { ...saved, body: { ...body, analysisBinding: { configurationSha256: 'c'.repeat(64), analysisSha256: 'd'.repeat(64) } } },
  }))
  assert.match(bound, /unversioned or unsupported bound draft needs an explicit rebuild/)
  assert.match(bound, /disabled=""[^>]*>Save draft/)
  assert.match(bound, /disabled=""[^>]*>Seal reviewed revision/)
  storage.set(predictionDraftKey('one'), JSON.stringify(local))
  const restored = renderToStaticMarkup(createElement(PredictionEditor, props))
  assert.match(restored, /unparseable JSON edits are still preserved/)
  const sealed = renderToStaticMarkup(createElement(PredictionEditor, {
    ...props, scenario: { ...scenario, status: 'PredictionSealed' }, prediction: { ...saved, seal: { sha256: 'b'.repeat(64) } },
  }))
  assert.match(sealed, /Saved revision 3 · Sealed/)
  assert.match(sealed, /ledger below/)
  assert.match(sealed, /Scenario reference/)
  assert.ok(sealed.includes('b'.repeat(64)))
  assert.match(sealed, /Download immutable saved JSON/)
  assert.match(sealed, /separate local backup differs/)
  assert.doesNotMatch(sealed, /<textarea|Prediction JSON ·|unparseable JSON edits are still preserved|Edit the complete prediction as JSON/)
  assert.doesNotMatch(sealed, />Save draft|>Seal reviewed revision/)
  assert.deepEqual(JSON.parse(storage.get(predictionDraftKey('one'))), local, 'Immutable presentation retains the original local backup')
  const approved = renderToStaticMarkup(createElement(PredictionEditor, {
    ...props, prediction: { ...saved, approval: { actor: 'Reviewer', approvedUtc: scenario.asOfUtc, sealedSha256: 'b'.repeat(64) } },
  }))
  assert.match(approved, /Saved revision 3 · Approved/)
  assert.doesNotMatch(approved, /<textarea|>Save draft/)
  const absent = renderToStaticMarkup(createElement(PredictionEditor, {
    ...props, prediction: undefined, scenario: { ...scenario, status: 'Scored' },
  }))
  assert.match(absent, /No saved prediction record is loaded/)
  assert.match(absent, /Download local JSON/)
  assert.doesNotMatch(absent, /<textarea|Saved revision undefined|separate local backup differs|Download immutable saved JSON/)
  console.log('Prediction checks passed: complete contract, missing numbers, scoped local copy, revision/CSRF/action-key seal headers, disabled sessions, conflicts and read-only seal.')
} finally {
  globalThis.fetch = originalFetch
  delete globalThis.sessionStorage
  await server.close()
}
