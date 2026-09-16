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
  const api = await server.ssrLoadModule('/src/formationInterpretation.ts')
  const { FormationInterpretationAgent } = await server.ssrLoadModule('/src/components/FormationInterpretationAgent.tsx')
  const { defaultConfiguration } = await server.ssrLoadModule('/src/analysisConfiguration.ts')
  const field = '11111111-1111-1111-1111-111111111111'
  const bore = '22222222-2222-2222-2222-222222222222'
  const pkg = { fieldId: field, sha256: 'a'.repeat(64), field: { MetaInfo: { ID: field }, Name: 'Field' },
    wellBores: [{ MetaInfo: { ID: bore }, Name: 'Bore 1' }], wells: [], clusters: [], trajectories: [], geologicalProperties: [], wellBoreArchitectures: [] }
  const request = {
    scope: { fieldId: field, reservoirName: 'Formation', scenarioId: null, asOfUtc: null },
    configuration: { ...defaultConfiguration, porosityCutoff: .13, idwNeighborCount: 3 },
    packageSha256: pkg.sha256, analysisSha256: 'b'.repeat(64), selectedCandidateId: 'configured:target',
    savedHypothesis: null, snapshotSha256: null,
    notes: { name: 'My name', rationale: 'My reason', correlationNotes: 'Check datum.', controlNotes: [] },
  }
  const result = {
    version: 'formation-interpretation-v1', promptVersion: 'formation-interpretation-prompt-v2', scope: request.scope,
    packageSha256: pkg.sha256, analysisSha256: request.analysisSha256, configurationSha256: 'c'.repeat(64),
    selectedCandidateId: request.selectedCandidateId, savedHypothesis: null, snapshotSha256: null,
    generatedAt: '2026-09-16T12:00:00Z',
    draft: { name: 'Formation review', rationale: 'Review this target with the available well evidence.',
      correlationNotes: 'Formation positions require checking against the depth reference.',
      citedEvidenceIds: [`wellbore:${bore}`], limitations: ['Depths have not been aligned to a common datum.'] },
  }
  assert.equal(api.formationDraftBlock(request, pkg), '')
  assert.equal(api.formationContextKey({ ...request, notes: { ...request.notes, rationale: 'changed' } }), api.formationContextKey(request))
  assert.notEqual(api.formationNotesKey(request.notes), api.formationNotesKey({ ...request.notes, rationale: 'changed' }))
  assert.notEqual(api.formationContextKey(request), api.formationContextKey({ ...request, selectedCandidateId: 'other' }))
  for (const change of [{ packageSha256: 'd'.repeat(64) }, { configuration: { ...request.configuration, gridPointsPerAxis: 1 } },
    { savedHypothesis: { hypothesisId: bore, revision: 1 } },
    { notes: { ...request.notes, controlNotes: [{ evidenceId: 'hidden:value', note: 'invalid' }] } }]) {
    assert.ok(api.formationDraftBlock({ ...request, ...change }, pkg))
  }
  assert.doesNotThrow(() => api.verifyFormationDraft(result, request, pkg, 'c'.repeat(64)))
  const original = structuredClone(result)
  const accepted = api.formationNotesText(result)
  assert.match(accepted, /AI-assisted interpretation/)
  assert.match(accepted, /Depths have not been aligned/)
  assert.match(accepted, new RegExp(bore))
  assert.ok(accepted.length <= 4000)
  assert.deepEqual(result, original)
  for (const change of [
    { version: 'unsupported' }, { analysisSha256: 'other' }, { packageSha256: 'wrong' }, { configurationSha256: 'wrong' },
    { scope: { ...request.scope, fieldId: bore } }, { selectedCandidateId: 'other' }, { generatedAt: 'invalid' },
    { savedHypothesis: { hypothesisId: bore, revision: 1 } }, { snapshotSha256: 'wrong' },
    { draft: { ...result.draft, correlationNotes: '' } }, { draft: { ...result.draft, name: 'a'.repeat(121) } },
    { draft: { ...result.draft, citedEvidenceIds: ['hidden:truth'] } }, { draft: { ...result.draft, citedEvidenceIds: [] } },
    { draft: { ...result.draft, citedEvidenceIds: [result.draft.citedEvidenceIds[0], result.draft.citedEvidenceIds[0]] } },
    { draft: { ...result.draft, limitations: ['a'.repeat(181)] } },
    { draft: { ...result.draft, limitations: [] } },
  ]) assert.throws(() => api.verifyFormationDraft({ ...result, ...change }, request, pkg, 'c'.repeat(64)))
  const session = { enabled: true, csrfHeaderName: 'X-DrillSim-CSRF', csrfRequestToken: 'test-csrf' }
  const calls = []
  globalThis.fetch = async (url, options) => {
    calls.push({ url, options })
    return new Response(JSON.stringify(url.endsWith('/session') ? session : url.endsWith('/status') ? { configured: true, reason: null } : result))
  }
  assert.equal((await api.getFormationAgentStatus()).configured, true)
  calls.length = 0
  assert.deepEqual(await api.draftFormationInterpretation(request, pkg, 'c'.repeat(64), new AbortController().signal), result)
  assert.equal(calls.length, 2, 'Exactly one session read and one inference request; no automatic retries or saves.')
  assert.equal(calls[1].url, '/analysis-api/api/formation-interpretation')
  assert.equal(calls[1].options.method, 'POST')
  assert.equal(calls[1].options.headers['X-DrillSim-CSRF'], 'test-csrf')
  assert.ok(calls[1].options.headers['Idempotency-Key'])
  assert.equal(calls[1].options.credentials, 'same-origin')
  assert.deepEqual(JSON.parse(calls[1].options.body), request)
  assert.equal('package' in JSON.parse(calls[1].options.body), false, 'The caller never provides an unverified raw evidence package.')
  calls.length = 0
  const cancellation = new AbortController()
  cancellation.abort()
  await assert.rejects(api.draftFormationInterpretation(request, pkg, 'c'.repeat(64), cancellation.signal))
  assert.equal(calls.length, 0)
  await assert.rejects(api.draftFormationInterpretation({ ...request, packageSha256: 'wrong' }, pkg, 'c'.repeat(64), new AbortController().signal))
  assert.equal(calls.length, 0)
  globalThis.fetch = async url => new Response(JSON.stringify(url.endsWith('/session') ? { ...session, enabled: false } : result))
  await assert.rejects(api.draftFormationInterpretation(request, pkg, 'c'.repeat(64), new AbortController().signal), /required/)
  globalThis.fetch = async url => url.endsWith('/session') ? new Response(JSON.stringify(session)) :
    new Response(JSON.stringify({ detail: 'Model unavailable. Try later.' }), { status: 503 })
  await assert.rejects(api.draftFormationInterpretation(request, pkg, 'c'.repeat(64), new AbortController().signal), /503: Model unavailable/)
  let rateLimitedRequests = 0
  globalThis.fetch = async url => {
    if (url.endsWith('/session')) return new Response(JSON.stringify(session))
    rateLimitedRequests++
    return new Response(JSON.stringify({ title: 'AI capacity limit reached', detail: 'Try again after capacity is available. No automatic retry was made.' }), { status: 429 })
  }
  await assert.rejects(api.draftFormationInterpretation(request, pkg, 'c'.repeat(64), new AbortController().signal), /429: Try again after capacity/)
  assert.equal(rateLimitedRequests, 1)
  let effects = 0
  globalThis.fetch = async () => { effects++; throw new Error('No inference during render') }
  const html = renderToStaticMarkup(createElement(FormationInterpretationAgent, {
    request, pkg, configurationSha256: 'c'.repeat(64), blocked: false, onUse: () => effects++,
  }))
  assert.match(html, /Draft interpretation with AI/)
  assert.match(html, /disabled=""[^>]*>Draft interpretation with AI/)
  assert.match(html, /Checking AI availability/)
  assert.doesNotMatch(html, /<pre|<textarea/)
  assert.equal(effects, 0)
  console.log('Formation AI checks passed: exact evidence/config/revision binding, current note context, bounded cited output, cancellation, guarded explicit request and no automatic edits or persistence.')
} finally {
  globalThis.fetch = originalFetch
  await server.close()
}
