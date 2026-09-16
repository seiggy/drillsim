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
  const api = await server.ssrLoadModule('/src/hypotheses.ts')
  const { defaultConfiguration } = await server.ssrLoadModule('/src/analysisConfiguration.ts')
  const { readTextDraft, parseHypothesisAttempt, PendingHypothesisAttempt } = await server.ssrLoadModule('/src/components/HypothesisSupport.tsx')
  const { HypothesisWorkbench, ComparisonOutput } = await server.ssrLoadModule('/src/components/HypothesisWorkbench.tsx')
  const { HypothesisRevisionEditor, HypothesisBranchEditor, BranchGridOutput } = await server.ssrLoadModule('/src/components/HypothesisRevisionEditor.tsx')
  const { SavedArtifactSummary, SavedHypothesisInput, AnalysisSettingsSummary } = await server.ssrLoadModule('/src/components/HypothesisPresentation.tsx')
  const { HypothesisChallenges, ChallengeDispositions } = await server.ssrLoadModule('/src/components/HypothesisChallenges.tsx')
  const fieldId = '11111111-1111-1111-1111-111111111111'
  const id = '22222222-2222-2222-2222-222222222222'
  const otherId = '33333333-3333-3333-3333-333333333333'
  const boreId = '44444444-4444-4444-4444-444444444444'
  const scope = { fieldId, reservoirName: 'Formation', scenarioId: null, asOfUtc: null }
  const scenarioScope = { ...scope, scenarioId: otherId, asOfUtc: '2026-01-01T00:00:00Z' }
  const pkg = { fieldId, sha256: 'a'.repeat(64), generatedAt: '2026-01-01T00:00:00Z',
    field: { MetaInfo: { ID: fieldId } }, clusters: [], wells: [{ MetaInfo: { ID: id }, Name: 'Control well' }],
    wellBores: [{ MetaInfo: { ID: boreId }, WellID: id, Name: 'Control bore' }],
    wellBoreArchitectures: [], trajectories: [], geologicalProperties: [],
    sourceCounts: { fields: 1, wells: 1, wellBores: 1, clusters: 0, trajectories: 0, wellBoreArchitectures: 0, geologicalProperties: 0 }, dataGaps: [] }
  const candidate = { candidateId: 'candidate:01:01', rank: 1, eastingM: 0, northingM: 0, latitudeDegrees: 58, longitudeDegrees: 3,
    nearestWellDistanceM: 600, p90NetPayM: 10, p50NetPayM: 20, p10NetPayM: 30, sigmaNetPayM: 8,
    meanPayPorosity: .2, meanPayPermeabilityMd: 5, relativeUncertainty: .4, score: 1, neighborEvidenceIds: [`well:${id}`] }
  const other = { ...candidate, candidateId: 'candidate:09:09', rank: 2, eastingM: 5000, northingM: 1000 }
  const analysis = { generatedAt: pkg.generatedAt, fieldId, reservoirName: 'Formation', packageSha256: pkg.sha256,
    modelVersion: 'petrophysics-screening-v1', configuration: { ...defaultConfiguration }, configurationSha256: 'b'.repeat(64), analysisSha256: 'c'.repeat(64),
    wellSummaries: [], ranking: [candidate, other], dataGaps: [],
    candidateGrid: [candidate, other].map(prediction => ({ ...prediction, status: 'eligible', reasons: [], prediction })) }
  const input = api.hypothesisInput(pkg, analysis, candidate.candidateId, 'Saved reasoning', 'Formation notes', [{ evidenceId: `well:${id}`, note: 'Review quality' }])
  const revision = { schemaVersion: 'hypothesis-revision-v1', hypothesisId: id, revision: 1, name: 'Baseline', scope, input,
    package: pkg, analysis, snapshotSha256: 'd'.repeat(64), createdUtc: pkg.generatedAt }
  const selection = api.selectionOf(revision)
  const summary = { hypothesisId: id, revision: 1, name: revision.name, scope, packageSha256: pkg.sha256,
    configurationSha256: analysis.configurationSha256, analysisSha256: analysis.analysisSha256, selectedCandidateId: candidate.candidateId,
    snapshotSha256: revision.snapshotSha256, createdUtc: pkg.generatedAt }
  const challenge = { schemaVersion: 'hypothesis-challenge-v1', challengeId: otherId, version: 1,
    hypothesis: api.referenceOf(revision), hypothesisSnapshotSha256: revision.snapshotSha256, analysisSha256: analysis.analysisSha256,
    summary: 'Question evidence sufficiency', createdBy: 'Reviewer', lastModifiedBy: 'Reviewer', createdUtc: pkg.generatedAt, modifiedUtc: pkg.generatedAt,
    sha256: 'e'.repeat(64), objections: [{ objectionId: boreId, text: 'Check this control.', citedEvidenceIds: [`well:${id}`], disposition: 'open' }] }
  const session = { enabled: true, csrfHeaderName: 'X-DrillSim-CSRF', csrfRequestToken: 'unit-test-csrf' }
  assert.equal(api.hypothesisQuery(scope).get('reservoir'), 'Formation')
  assert.equal(api.hypothesisQuery(scope).has('scenarioId'), false)
  assert.equal(api.hypothesisQuery(scenarioScope).get('asOf'), scenarioScope.asOfUtc)
  assert.throws(() => api.hypothesisQuery({ ...scope, scenarioId: id }))
  assert.throws(() => api.hypothesisScopeKey({ fieldId, reservoirName: 'Formation' }))
  assert.notEqual(api.hypothesisScopeKey(scope), api.hypothesisScopeKey(scenarioScope))
  assert.notEqual(api.hypothesisSelectionKey(selection), api.hypothesisSelectionKey({ ...selection, reference: { hypothesisId: id, revision: 2 } }))
  assert.doesNotThrow(() => api.verifyHypothesis(revision, selection))
  for (const changed of [{ revision: 2 }, { snapshotSha256: '' }, { analysis: { ...analysis, analysisSha256: 'wrong' } },
    { package: { ...pkg, sha256: 'wrong' } }, { scope: { ...scope, fieldId: otherId } }]) {
    assert.throws(() => api.verifyHypothesis({ ...revision, ...changed }, selection))
  }
  assert.equal(api.branchTarget(revision, analysis, 'spatial').candidateId, other.candidateId)
  assert.equal(api.branchTarget(revision, { ...analysis, candidateGrid: [analysis.candidateGrid[0]] }, 'spatial'), undefined)
  const conservative = api.branchPreset(revision, 'conservative')
  assert.ok(conservative.porosityCutoff > defaultConfiguration.porosityCutoff)
  assert.equal(conservative.permeabilityCutoffM2, defaultConfiguration.permeabilityCutoffM2 * 2)
  assert.equal(revision.input.configuration.porosityCutoff, .12, 'Branch preset never mutates source configuration')
  assert.throws(() => api.hypothesisInput(pkg, analysis, 'not-eligible', 'why', '', []), /eligible/)
  assert.throws(() => api.hypothesisInput(pkg, analysis, candidate.candidateId, '', '', []), /rationale/)
  assert.throws(() => api.hypothesisInput(pkg, analysis, candidate.candidateId, 'why', '', [{ evidenceId: 'hidden:record', note: 'No' }]), /visible/)
  assert.doesNotThrow(() => api.validateObjections(pkg, [{ text: 'Check', citedEvidenceIds: [`well:${id}`] }]))
  assert.throws(() => api.validateObjections(pkg, [{ text: 'Check', citedEvidenceIds: ['hidden:record'] }]))
  assert.throws(() => api.validateObjections(pkg, [{ text: 'Check', citedEvidenceIds: [`well:${id}`, `well:${id}`] }]))
  assert.deepEqual(readTextDraft('{"rationale":"unsaved note"}', { rationale: '' }), { rationale: 'unsaved note' })
  assert.throws(() => readTextDraft('{"rationale":4}', { rationale: '' }))

  const requests = []
  const attempt = { key: 'same-create-attempt', path: '', method: 'POST', kind: 'revision', label: 'Save', body: { name: 'Baseline', scope, input } }
  assert.deepEqual(parseHypothesisAttempt(JSON.stringify(attempt)), attempt)
  assert.throws(() => parseHypothesisAttempt(JSON.stringify({ ...attempt, path: '/../scenarios/delete' })))
  globalThis.fetch = async (url, options) => {
    requests.push({ url, options })
    return new Response(JSON.stringify(url.endsWith('/session') ? session : revision), { status: 200 })
  }
  await api.writeHypothesis(attempt)
  await api.writeHypothesis(attempt)
  const saves = requests.filter(({ options }) => options.method === 'POST')
  assert.equal(saves.length, 2)
  assert.ok(saves.every(({ options }) => options.headers['X-DrillSim-CSRF'] === session.csrfRequestToken &&
    options.headers['Idempotency-Key'] === attempt.key && options.credentials === 'same-origin'))
  assert.deepEqual(JSON.parse(saves[0].options.body).scope, scope, 'Live mutation scope explicitly includes null/null')
  assert.ok(saves.every(({ options }) => !options.headers['X-DrillSim-Human-Actor'] && !options.headers.Authorization))
  requests.length = 0
  globalThis.fetch = async (url, options) => {
    requests.push({ url, options })
    return new Response(JSON.stringify(url.endsWith('/session') ? session : { ...revision, revision: 2 }), { status: 200 })
  }
  await api.writeHypothesis({ ...attempt, path: `/${id}/revisions`, expectedVersion: 1, body: { scope, input } })
  assert.equal(requests.at(-1).options.headers['If-Match'], '"1"')
  globalThis.fetch = async url => url.endsWith('/session') ? new Response(JSON.stringify(session)) :
    new Response(JSON.stringify({ title: 'Hypothesis conflict', detail: 'A newer revision exists' }), { status: 409 })
  await assert.rejects(api.writeHypothesis(attempt), /409.*newer revision/)
  let blockedWrites = 0
  globalThis.fetch = async (url, options) => {
    if (options.method === 'POST') blockedWrites++
    return new Response(JSON.stringify({ ...session, enabled: false, reason: 'Local session disabled' }))
  }
  await assert.rejects(api.writeHypothesis(attempt), /disabled/)
  assert.equal(blockedWrites, 0)
  requests.length = 0
  globalThis.fetch = async (url, options) => { requests.push({ url, options }); return new Response(JSON.stringify(url.endsWith('/analysis') ? analysis : url.includes('/revisions/1?') ? revision : [summary])) }
  assert.equal((await api.listHypotheses(scope, 20))[0].hypothesisId, id)
  assert.match(requests[0].url, /limit=20&offset=20/)
  await api.listHypotheses(scope, 0, id)
  assert.match(requests.at(-1).url, new RegExp(`/${id}/revisions\\?`))
  assert.deepEqual(await api.getHypothesis(selection), revision)
  await api.analyzeHypothesis(selection, conservative)
  assert.deepEqual(JSON.parse(requests.at(-1).options.body), { scope, configuration: conservative })
  assert.equal(requests.at(-1).options.headers['X-DrillSim-CSRF'], undefined, 'Stored analysis POST is the documented read-only computation, not persistence')
  requests.length = 0
  const comparisonSelections = [selection, { scope: scenarioScope, reference: { hypothesisId: otherId, revision: 3 } }]
  globalThis.fetch = async (url, options) => { requests.push({ url, options }); return new Response(JSON.stringify({ entries: [], comparisonSha256: 'f'.repeat(64) })) }
  await api.compareHypotheses(comparisonSelections)
  assert.deepEqual(JSON.parse(requests[0].options.body).revisions, comparisonSelections)
  assert.equal(comparisonSelections[1].reference.revision, 3, 'Latest reads never replace selected exact comparison references')
  requests.length = 0
  globalThis.fetch = async (url, options) => { requests.push({ url, options }); return new Response(JSON.stringify(url.endsWith('/session') ? session : challenge)) }
  await api.writeHypothesis({ key: 'challenge-key', path: `${api.revisionPath(revision)}/challenges`, method: 'POST', kind: 'challenge', label: 'Challenge',
    body: { scope, analysisSha256: analysis.analysisSha256, summary: challenge.summary, actor: 'Reviewer', objections: [{ text: 'Check', citedEvidenceIds: [`well:${id}`] }] } })
  assert.equal(JSON.parse(requests.at(-1).options.body).analysisSha256, revision.input.analysisSha256)
  await api.getHypothesisChallenge(selection, otherId, 1)
  assert.match(requests.at(-1).url, /version=1/)
  globalThis.fetch = async (url, options) => { requests.push({ url, options }); return new Response(JSON.stringify(url.endsWith('/session') ? session : { ...challenge, version: 2 })) }
  await api.writeHypothesis({ key: 'disposition-key', path: `${api.revisionPath(revision)}/challenges/${otherId}/dispositions`, method: 'PUT', kind: 'challenge', label: 'Disposition', expectedVersion: 1,
    body: { scope, actor: 'Reviewer', dispositions: [{ objectionId: boreId, disposition: 'deferred', reason: 'Need more evidence.' }] } })
  assert.equal(requests.at(-1).options.method, 'PUT')
  assert.equal(requests.at(-1).options.headers['If-Match'], '"1"')
  assert.equal(requests.at(-1).options.headers['Idempotency-Key'], 'disposition-key')
  globalThis.fetch = async url => new Response(JSON.stringify(url.endsWith('/session') ? session : { ...challenge, version: 7 }))
  await assert.rejects(api.writeHypothesis({ key: 'bad-version', path: `${api.revisionPath(revision)}/challenges/${otherId}/dispositions`, method: 'PUT', kind: 'challenge', label: 'Disposition', expectedVersion: 1, body: { scope, actor: 'Reviewer', dispositions: [] } }), /identity\/version/)

  let calls = 0
  globalThis.fetch = async () => { calls++; throw new Error('Rendering cannot save or run AI') }
  const writes = { busy: false, blocked: false, pending: undefined, error: '', message: '', send: () => { calls++; return Promise.resolve(undefined) } }
  const editorProps = { pkg, analysis, scope, candidateId: candidate.candidateId, livePackage: pkg, liveScope: scope, stale: false, writes, onSaved: () => {}, correlation: true }
  const liveEditor = renderToStaticMarkup(createElement(HypothesisRevisionEditor, editorProps))
  assert.match(liveEditor, /Save hypothesis/)
  assert.match(liveEditor, /Multi-bore formation and control annotation table/)
  assert.match(liveEditor, /Draft interpretation with AI/)
  assert.match(liveEditor, /reads these notes when you request a draft/)
  assert.match(liveEditor, /Example correlation note/)
  assert.match(liveEditor, /Use example note/)
  assert.match(liveEditor, /Check whether differences in formation depth reflect geological structure/)
  assert.match(liveEditor, /Package fingerprint/)
  assert.match(liveEditor, /Settings fingerprint/)
  assert.match(liveEditor, /Download input references JSON/)
  assert.doesNotMatch(liveEditor, /<pre[ >]/)
  const staleEditor = renderToStaticMarkup(createElement(HypothesisRevisionEditor, { ...editorProps, stale: true }))
  assert.match(staleEditor, /disabled=""[^>]*>Save hypothesis/)
  assert.match(staleEditor, /disabled=""[^>]*>Use example note/)
  const historical = renderToStaticMarkup(createElement(HypothesisRevisionEditor, { ...editorProps, selected: revision, livePackage: { ...pkg, sha256: 'new' } }))
  assert.match(historical, /disabled=""[^>]*>Save notes as revision 2/)
  assert.match(historical, /Save notes as named snapshot branch/)
  assert.match(historical, /disabled=""[^>]*>Use example note/, 'Existing notes cannot be replaced by the example.')
  const savedInput = renderToStaticMarkup(createElement(SavedHypothesisInput, { input }))
  for (const value of ['Porosity cutoff', '0.12 fraction', '9.869233e-16 m²', '500 m', '15 × 15 points', 'Neighbor count',
    'analysis-configuration-v1', 'Saved reasoning', 'Formation notes', 'Review quality', `well:${id}`, pkg.sha256, analysis.analysisSha256]) {
    assert.ok(savedInput.includes(value), `Saved input includes ${value}`)
  }
  assert.doesNotMatch(savedInput, /<pre[ >]|&quot;configuration&quot;/)
  const addedSettings = renderToStaticMarkup(createElement(AnalysisSettingsSummary, {
    configuration: { ...defaultConfiguration, version: 'future-settings-v2', futureControlRule: { weighting: 'experimental' }, constructor: 17 },
    downloadLabel: 'Download comparison JSON',
  }))
  assert.match(addedSettings, /Additional recorded settings/)
  assert.match(addedSettings, /Future control rule/)
  assert.match(addedSettings, /experimental/)
  assert.match(addedSettings, /Constructor<small> · additional recorded field/)
  assert.match(addedSettings, />17<\/span>/)
  assert.match(addedSettings, /not supported by this editor/)
  const confirmed = renderToStaticMarkup(createElement(SavedArtifactSummary, { artifact: revision }))
  assert.match(confirmed, /Baseline/)
  assert.ok(confirmed.includes(revision.snapshotSha256))
  assert.match(confirmed, /Download confirmed artifact JSON/)
  const confirmedChallenge = renderToStaticMarkup(createElement(SavedArtifactSummary, { artifact: challenge }))
  assert.match(confirmedChallenge, /Question evidence sufficiency/)
  assert.match(confirmedChallenge, /challenge version.*1/)
  assert.ok(confirmedChallenge.includes(challenge.sha256))
  assert.doesNotMatch(confirmedChallenge, /<pre[ >]/)
  const comparison = { schemaVersion: 'hypothesis-comparison-v1', baseline: selection.reference, comparisonSha256: 'f'.repeat(64),
    entries: [
      { revision: summary, configuration: defaultConfiguration, selectedCandidate: candidate, rationale: input.rationale,
        correlationNotes: input.correlationNotes, controlNotes: input.controlNotes, likeForLikeEvidence: true, scopeDifferences: [], differences: [] },
      { revision: { ...summary, hypothesisId: otherId, name: 'Alternative', scope: scenarioScope }, configuration: conservative, selectedCandidate: other,
        rationale: 'Alternative rationale', correlationNotes: 'Alternative correlation', controlNotes: input.controlNotes, likeForLikeEvidence: false,
        scopeDifferences: ['different-scenario', 'different-as-of', 'future-scope-rule'], differences: [
          { field: 'configuration.porosityCutoff', baselineValue: .12, value: .14 },
          { field: 'configuration.permeabilityCutoffM2', baselineValue: 1e-15, value: 2e-15 },
          { field: 'selectedCandidate.p50NetPayM', baselineValue: 20, value: 22 },
          { field: 'selectedCandidate.neighborEvidenceIds', baselineValue: [`well:${id}`], value: [`well:${otherId}`] },
          { field: 'selectedCandidate.uncertaintyComponents', baselineValue: null, value: { weightedDistanceM: 602, calibrated: false } },
          { field: 'controlNotes', baselineValue: [], value: [{ evidenceId: `well:${id}`, note: 'Keep this control note.' }] },
          { field: 'futureScope', baselineValue: null, value: { scope: scenarioScope, futureRule: { nested: { exact: { retained: 'download-only value' } } } } },
        ] },
    ] }
  const comparisonHtml = renderToStaticMarkup(createElement(ComparisonOutput, { comparison, stale: false, onLoad: () => {} }))
  for (const value of ['Porosity cutoff', '0.14 fraction', '2e-15 m²', '22 m', '602 m', 'Calibrated', '>No<',
    'Keep this control note.', 'Alternative rationale', 'Alternative correlation', `well:${otherId}`, 'NOT LIKE-FOR-LIKE',
    'Different scenario', 'Different evidence time', scenarioScope.asOfUtc, 'future-scope-rule', 'Full values: ',
    'Download comparison JSON', 'futureScope.futureRule.nested.exact']) assert.ok(comparisonHtml.includes(value), `Comparison includes ${value}`)
  assert.doesNotMatch(comparisonHtml, /<pre[ >]|<textarea|&quot;weightedDistanceM&quot;/)
  assert.match(renderToStaticMarkup(createElement(ComparisonOutput, { comparison, stale: true, onLoad: () => {} })), /prior comparison output/)
  const pendingAttempt = { ...attempt, expectedVersion: 1, body: { scope: scenarioScope, input, actor: 'Retained reviewer',
    dispositions: [{ objectionId: boreId, disposition: 'deferred', reason: 'Await evidence.' }] } }
  const pendingHtml = renderToStaticMarkup(createElement(PendingHypothesisAttempt, { attempt: pendingAttempt }))
  for (const value of [attempt.key, 'Retained reviewer', scenarioScope.asOfUtc, 'Await evidence.', 'deferred', '0.12 fraction',
    'Saved reasoning', 'Review quality', 'Expected saved version', 'Download captured request JSON']) assert.ok(pendingHtml.includes(value), `Captured request includes ${value}`)
  assert.doesNotMatch(pendingHtml, /<pre[ >]|&quot;input&quot;/)
  const extendedGrid = [
    ...analysis.candidateGrid,
    ...Array.from({ length: 21 }, (_, index) => ({ candidateId: `excluded:${index}`, eastingM: index, northingM: index * 2,
      status: 'excluded', reasons: ['within-well-exclusion-radius'], nearestWellDistanceM: 100, prediction: null })),
    { candidateId: 'unsupported', eastingM: 0, northingM: 0, status: 'unsupported', reasons: ['insufficient-located-well-controls'], prediction: null },
    { candidateId: 'future', eastingM: 0, northingM: 0, status: 'future-status', reasons: ['future-reason'], prediction: null },
  ]
  const gridHtml = renderToStaticMarkup(createElement(BranchGridOutput, { result: { ...analysis, candidateGrid: extendedGrid }, candidateId: candidate.candidateId }))
  for (const value of ['25 grid cells', 'Eligible: 2', 'Excluded: 21', 'Unsupported: 1', 'Unrecognized status: future-status',
    'Additional recorded reason: future-reason', 'Within screening-control exclusion radius', 'Too few located screening controls',
    'Cells 1–20 of 25', '10 / 20 / 30', 'data-selected="true"', 'Download full grid JSON', 'not hydrocarbon pay']) assert.ok(gridHtml.includes(value), `Grid includes ${value}`)
  assert.equal((gridHtml.match(/<tr/g) ?? []).length, 21, 'Grid renders one header and at most twenty cells')
  assert.doesNotMatch(gridHtml, /<pre[ >]|excluded:20/)
  assert.match(renderToStaticMarkup(createElement(BranchGridOutput, { result: { ...analysis, candidateGrid: [] }, candidateId: '' })), /returned grid is empty/)
  assert.match(renderToStaticMarkup(createElement(BranchGridOutput, { result: { ...analysis, candidateGrid: undefined }, candidateId: '' })), /Grid output was not returned/)

  async function checkDownload(element, label, expected) {
    const find = node => {
      if (!node || typeof node !== 'object') return undefined
      if (Array.isArray(node)) return node.map(find).find(Boolean)
      if (node.type === 'button' && node.props.children === label) return node
      return find(node.props?.children)
    }
    const button = find(element)
    assert.ok(button, `Download action exists: ${label}`)
    const oldDocument = globalThis.document, oldCreate = URL.createObjectURL, oldRevoke = URL.revokeObjectURL
    let captured, clicked = false
    try {
      globalThis.document = { createElement: () => ({ click: () => { clicked = true } }) }
      URL.createObjectURL = blob => { captured = blob; return 'blob:fixture' }
      URL.revokeObjectURL = () => {}
      button.props.onClick()
      assert.ok(clicked)
      assert.deepEqual(JSON.parse(await captured.text()), expected, 'Download preserves the full original artifact, not the presentation summary')
    } finally {
      globalThis.document = oldDocument; URL.createObjectURL = oldCreate; URL.revokeObjectURL = oldRevoke
    }
  }
  await checkDownload(SavedArtifactSummary({ artifact: revision }), 'Download confirmed artifact JSON', revision)
  await checkDownload(SavedArtifactSummary({ artifact: challenge }), 'Download confirmed artifact JSON', challenge)
  await checkDownload(ComparisonOutput({ comparison, stale: false, onLoad: () => {} }), 'Download comparison JSON', comparison)
  await checkDownload(PendingHypothesisAttempt({ attempt: pendingAttempt }), 'Download captured request JSON', pendingAttempt)
  assert.match(renderToStaticMarkup(createElement(HypothesisBranchEditor, { source: revision, writes, onSaved: () => {} })), /Reanalyze stored evidence/)
  const critique = renderToStaticMarkup(createElement(HypothesisChallenges, { source: revision, writes, ai: null }))
  assert.match(critique, /Save challenge to exact revision/)
  assert.ok(critique.includes(revision.snapshotSha256))
  assert.ok(critique.includes(`well:${id}`))
  const dispositions = renderToStaticMarkup(createElement(ChallengeDispositions, { source: revision, challenge, writes, onSaved: () => {} }))
  for (const name of ['open', 'accepted', 'rejected', 'deferred']) assert.ok(dispositions.includes(`>${name}</option>`))
  assert.match(dispositions, /Disposition reason/)
  for (const taskId of ['alternatives', 'compare', 'challenge', 'correlation']) {
    const html = renderToStaticMarkup(createElement(HypothesisWorkbench, { taskId, pkg, analysis, scope, candidateId: candidate.candidateId, stale: false, ai: null }))
    assert.match(html, /Scoped saved hypotheses and exact revisions/)
    assert.doesNotMatch(html, /branching is not yet implemented|No saved alternatives engine/)
    assert.doesNotMatch(html, /<pre[ >]/)
    if (taskId === 'compare') assert.match(html, /<textarea[^>]+aria-label="Comparison scopes and exact references · JSON"/, 'Advanced comparison input remains editable')
  }
  assert.equal(calls, 0)
  console.log('Hypothesis checks passed: exact scoped API, CSRF/idempotency/version conflicts, immutable snapshots, stored-evidence presets, exact comparisons, cited challenges/disposition history, local text and distinct mutation-free workspaces.')
} finally {
  globalThis.fetch = originalFetch
  delete globalThis.sessionStorage
  await server.close()
}
