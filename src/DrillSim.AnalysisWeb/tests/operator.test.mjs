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
  const {
    getOperatorSession, getOperatorView, getOperatorSetup, sendOperatorAttempt, operatorAuditError,
    readOperatorAttempt, operatorAttemptKey, operatorOutcomeKey, operatorAttemptConfirmed, operatorAttemptFailed, operatorLifecycleLabel,
    completionApprovalBlock, completionAuditError, completionReviewKey, completionRetryBlock, retainCompletionReview,
    publicationRecoveryBlock, publicationRecoveryKey, publicationRecoveryRetryBlock, retainPublicationRecoveryReview,
    recoveryAuditError, recoveryReasonError,
    scoreCorrectionBlock, scoreCorrectionKey, scoreCorrectionRetryBlock, retainScoreCorrectionReview,
  } = await server.ssrLoadModule('/src/operator.ts')
  const { OperatorControls } = await server.ssrLoadModule('/src/components/OperatorControls.tsx')
  const { OperatorCompletionReview } = await server.ssrLoadModule('/src/components/OperatorCompletionReview.tsx')
  const { OperatorPublicationRecovery } = await server.ssrLoadModule('/src/components/OperatorPublicationRecovery.tsx')
  const { OperatorScoreCorrection } = await server.ssrLoadModule('/src/components/OperatorScoreCorrection.tsx')
  const { SimulatorSetup } = await server.ssrLoadModule('/src/components/SimulatorSetup.tsx')
  const { setupActorError, setupSettingsError, simulatorSetupBlock, setupRetryBlock, validateSimulatorSetup } =
    await server.ssrLoadModule('/src/simulatorSetup.ts')
  const { lifecycleStatus } = await server.ssrLoadModule('/src/sequencer.ts')
  const session = { enabled: true, csrfRequestToken: 'csrf-test-token', csrfHeaderName: 'X-DrillSim-CSRF', auditLabelLimitation: 'Local audit only' }
  const scenario = { scenarioId: 'scenario-one', reservoirName: 'Formation', asOfUtc: '2026-01-01T00:00:00Z', status: 'HumanApproved' }
  const available = { enabled: true, reason: null }
  const view = {
    enabled: true, scenarioId: scenario.scenarioId, scenarioStatus: 'HumanApproved',
    prediction: { sealed: true, approved: true, sealHash: 'a'.repeat(64) }, preflight: { ready: true, worldBound: true },
    actions: { approvePrediction: available, start: available, cancel: available, resume: available,
      publish: available, score: available, approveCompletion: { enabled: false, reason: 'Observable review unavailable' } },
    completion: { available: false, approvalEnabled: false, reason: 'Observable review unavailable', openings: [] },
  }
  const attempt = {
    version: 1, scenarioId: scenario.scenarioId, action: 'approve-prediction',
    key: 'same-attempt-key', actor: 'Local reviewer', reviewedSealHash: 'a'.repeat(64),
  }
  const requests = []
  globalThis.fetch = async (url, options) => {
    requests.push({ url, options })
    return new Response(JSON.stringify(url.endsWith('/session') ? session : view), { status: 200 })
  }
  assert.deepEqual(await getOperatorSession(), session)
  assert.deepEqual(await getOperatorView(scenario.scenarioId), view)
  assert.ok(requests.every(({ options }) => options.credentials === 'same-origin' && options.cache === 'no-store'))
  assert.ok(requests.every(({ options }) => !options.method || options.method === 'GET'))
  requests.length = 0
  let finishSession
  globalThis.fetch = (url, options) => {
    requests.push({ url, options })
    return new Promise(resolve => { finishSession = () => resolve(new Response(JSON.stringify(session))) })
  }
  const abandoned = new AbortController()
  const firstSession = getOperatorSession(abandoned.signal)
  const nextSession = getOperatorSession()
  abandoned.abort()
  finishSession()
  const results = await Promise.allSettled([firstSession, nextSession])
  assert.equal(results[0].status, 'rejected')
  assert.equal(results[1].status, 'fulfilled')
  assert.equal(requests.length, 1, 'Remounts share one cookie-issuing request instead of racing antiforgery cookies.')
  assert.equal(requests[0].options.signal, undefined, 'An abandoned consumer must not abort shared cookie initialization.')
  globalThis.fetch = async (url, options) => {
    requests.push({ url, options })
    return new Response(JSON.stringify(session))
  }
  assert.deepEqual(await getOperatorSession(), session)
  assert.equal(requests.length, 2, 'Later refreshes fetch a current token; no invented token lifetime/cache.')
  assert.equal(operatorAuditError('Local reviewer'), '')
  for (const label of ['', '   ', 'a'.repeat(201), 'bad\nlabel']) assert.ok(operatorAuditError(label))
  assert.deepEqual(readOperatorAttempt(JSON.stringify(attempt), scenario.scenarioId), attempt)
  assert.notEqual(operatorAttemptKey('one'), operatorAttemptKey('two'))
  for (const invalid of [
    { scenarioId: 'another' }, { key: '' }, { actor: '' }, { action: 'arbitrary-route' }, { version: 2 }, { reviewedSealHash: 'wrong' },
  ]) assert.throws(() => readOperatorAttempt(JSON.stringify({ ...attempt, ...invalid }), scenario.scenarioId))
  assert.throws(() => readOperatorAttempt(JSON.stringify({ ...attempt, action: 'publish' }), scenario.scenarioId), /unreadable/)

  requests.length = 0
  globalThis.fetch = async (url, options) => {
    requests.push({ url, options })
    return new Response(JSON.stringify({ scenarioId: scenario.scenarioId, outcome: 'approved', revealSucceeded: false }), { status: 200 })
  }
  await assert.rejects(sendOperatorAttempt({ ...session, enabled: false }, attempt), /required/)
  await assert.rejects(sendOperatorAttempt({ ...session, csrfRequestToken: null }, attempt), /required/)
  await assert.rejects(sendOperatorAttempt({ ...session, csrfHeaderName: 'Other-Header' }, attempt), /required/)
  assert.equal(requests.length, 0, 'No mutation can be sent without supported enabled CSRF session')
  await sendOperatorAttempt(session, attempt)
  await sendOperatorAttempt(session, readOperatorAttempt(JSON.stringify(attempt), scenario.scenarioId))
  assert.ok(requests.every(({ url }) => url.endsWith('/api/operator/scenarios/scenario-one/approve-prediction')))
  assert.deepEqual(requests[0].options.headers, {
    'Content-Type': 'application/json', 'X-DrillSim-CSRF': 'csrf-test-token', 'Idempotency-Key': 'same-attempt-key',
  })
  assert.deepEqual(JSON.parse(requests[0].options.body), { actor: 'Local reviewer', reviewedSealHash: 'a'.repeat(64) })
  assert.equal(requests[1].options.headers['Idempotency-Key'], requests[0].options.headers['Idempotency-Key'])
  assert.ok(requests.every(({ options }) => !options.headers['X-DrillSim-Human-Actor'] && !options.headers['X-Operator-Key']))
  requests.length = 0
  await sendOperatorAttempt(session, { ...attempt, action: 'start' })
  assert.match(requests[0].url, /\/scenarios\/scenario-one\/runs$/)
  assert.deepEqual(JSON.parse(requests[0].options.body), { actor: 'Local reviewer' })

  for (const action of ['cancel', 'resume', 'publish', 'score']) {
    requests.length = 0
    globalThis.fetch = async (url, options) => {
      requests.push({ url, options })
      return new Response(JSON.stringify({
        scenarioId: scenario.scenarioId, runId: 'run-one', outcome: 'scoring-failed',
        revealSucceeded: true, scoringStatus: 'failed', reason: 'Reveal remains published.',
      }), { status: 200 })
    }
    const response = await sendOperatorAttempt(session, { ...attempt, action, runId: 'run-one' })
    assert.match(requests[0].url, new RegExp(`/runs/run-one/${action}$`))
    assert.equal(requests.length, 1, 'Client never automatically chains a second mutation or retry')
    assert.deepEqual(JSON.parse(requests[0].options.body), { actor: 'Local reviewer' })
    assert.equal(operatorAttemptConfirmed(response), false)
    assert.equal(response.revealSucceeded, true, 'Scoring failure must not hide successful publication')
  }
  assert.equal(operatorAttemptConfirmed({ outcome: 'publication-pending' }), false)
  assert.equal(operatorAttemptConfirmed({ outcome: 'scored' }), true)
  assert.equal(operatorAttemptFailed({ outcome: 'publication-failed' }), true)
  assert.equal(operatorAttemptFailed({ outcome: 'scoring-failed' }), true)
  assert.equal(operatorAttemptFailed({ outcome: 'scoring-pending' }), false)
  globalThis.fetch = async () => new Response(JSON.stringify({ scenarioId: 'other' }), { status: 200 })
  await assert.rejects(getOperatorView(scenario.scenarioId), /another scenario/)
  await assert.rejects(sendOperatorAttempt(session, attempt), /another scenario/)
  globalThis.fetch = async () => new Response('<html>not curated</html>', { status: 404 })
  await assert.rejects(getOperatorSession(), /endpoint or requested scenario\/run was not found/)
  globalThis.fetch = async () => new Response(JSON.stringify({ title: 'Scenario not found', detail: 'The requested run does not belong to this scenario.' }), { status: 404 })
  await assert.rejects(getOperatorView(scenario.scenarioId), /404: Scenario not found.*does not belong/)
  globalThis.fetch = async () => new Response(JSON.stringify({ title: 'Antiforgery validation failed' }), { status: 400 })
  await assert.rejects(sendOperatorAttempt(session, attempt), /400: Antiforgery validation failed/)

  assert.equal(operatorLifecycleLabel('simulation', view), 'Ready to start')
  assert.equal(operatorLifecycleLabel('simulation', { ...view, preflight: { ready: false, worldBound: false } }), 'Prepare simulator')
  const run = { runId: 'run-one', status: 'Running', currentStage: 'S2Drill', stages: [], progressPercent: 20 }
  assert.equal(operatorLifecycleLabel('simulation', { ...view, run }), 'Running')
  assert.equal(operatorLifecycleLabel('simulation', { ...view, run: { ...run, status: 'AwaitingApproval' } }), 'Completion approval required')
  const resources = { scenarioId: scenario.scenarioId, loading: false, error: '', operator: { ...view, run: { ...run, status: 'Scored' } } }
  assert.equal(lifecycleStatus('evaluation', scenario, resources), 'Scorecard pending', 'Published run state does not invent a missing scorecard')
  assert.equal(lifecycleStatus('new-evidence', scenario, resources), 'Reveal receipt pending')

  const settings = { profileId: 'field-demo', resolution: 'Preview', realizationSeed: 2147483647 }
  const setup = {
    scenarioId: scenario.scenarioId, available: true, reason: null, prepared: false, current: null,
    defaults: settings, reviewedSealHash: view.prediction.sealHash,
    profiles: [{ profileId: settings.profileId, name: 'Field demonstration', description: 'Curated synthetic model.', worldModelVersion: 'fixture-v1' }],
  }
  const unpreparedView = { ...view, preflight: { ready: false, worldBound: false } }
  assert.deepEqual(validateSimulatorSetup(setup, scenario.scenarioId), setup)
  assert.equal(simulatorSetupBlock(setup, unpreparedView, scenario.scenarioId, 'Demo operator', settings), '')
  assert.equal(setupActorError('Demo operator'), '')
  for (const actor of ['', ' '.repeat(5), 'x'.repeat(101), 'Name\n', 'J\u00f6rg']) assert.ok(setupActorError(actor))
  for (const invalid of [{ realizationSeed: -1 }, { realizationSeed: 2147483648 }, { realizationSeed: 1.5 },
    { realizationSeed: Number.NaN }, { realizationSeed: '1' }, { profileId: '' }, { resolution: 'Custom' }]) {
    assert.ok(setupSettingsError({ ...settings, ...invalid }))
  }
  assert.ok(simulatorSetupBlock(setup, { ...unpreparedView, prediction: { ...view.prediction, approved: false } }, scenario.scenarioId, 'Demo operator', settings))
  assert.ok(simulatorSetupBlock(setup, { ...unpreparedView, run }, scenario.scenarioId, 'Demo operator', settings))
  assert.ok(simulatorSetupBlock(setup, unpreparedView, scenario.scenarioId, 'Demo operator', { ...settings, profileId: 'not-curated' }))
  assert.ok(simulatorSetupBlock({ ...setup, reviewedSealHash: 'f'.repeat(64) }, unpreparedView, scenario.scenarioId, 'Demo operator', settings))
  const setupAttempt = { version: 1, scenarioId: scenario.scenarioId, action: 'setup', key: 'stable-setup-key',
    actor: 'Demo operator', reviewedSealHash: setup.reviewedSealHash, ...settings }
  assert.deepEqual(readOperatorAttempt(JSON.stringify(setupAttempt), scenario.scenarioId), setupAttempt)
  for (const invalid of [{ reviewedSealHash: 'wrong' }, { actor: ' operator ' }, { actor: 'x'.repeat(101) },
    { realizationSeed: null }, { resolution: 'Ultra' }, { profileId: null }, { runId: 'existing-run' }]) {
    assert.throws(() => readOperatorAttempt(JSON.stringify({ ...setupAttempt, ...invalid }), scenario.scenarioId))
  }
  const prepared = { ...setup, available: false, prepared: true, current: {
    ...settings, profileName: 'Field demonstration', preparedBy: 'Demo operator', preparedUtc: '2026-01-01T00:00:00Z',
  } }
  assert.equal(setupRetryBlock(unpreparedView, setup, setupAttempt), '')
  assert.equal(setupRetryBlock(view, prepared, setupAttempt), '', 'An uncertain successful preparation replays the same request even when preparation is no longer available.')
  assert.equal(setupRetryBlock(view, { ...prepared, current: null }, setupAttempt), '', 'Older compatible prepared setups need no invented settings.')
  assert.ok(setupRetryBlock(view, { ...prepared, current: { ...prepared.current, realizationSeed: 10 } }, setupAttempt))
  assert.ok(setupRetryBlock(view, { ...setup, reviewedSealHash: 'f'.repeat(64) }, setupAttempt))
  assert.ok(setupRetryBlock(undefined, setup, setupAttempt))
  requests.length = 0
  globalThis.fetch = async (url, options) => {
    requests.push({ url, options })
    return new Response(JSON.stringify(options.method === 'POST'
      ? { scenarioId: scenario.scenarioId, outcome: 'simulation-prepared', revealSucceeded: false } : setup))
  }
  assert.deepEqual(await getOperatorSetup(scenario.scenarioId), setup)
  assert.equal(requests[0].options.method, undefined, 'Reading setup never mutates a model.')
  assert.equal(operatorAttemptConfirmed(await sendOperatorAttempt(session, setupAttempt)), true)
  await sendOperatorAttempt(session, readOperatorAttempt(JSON.stringify(setupAttempt), scenario.scenarioId))
  assert.equal(requests.length, 3, 'Preparation never chains Start.')
  assert.ok(requests.every(({ url }) => url.endsWith('/scenarios/scenario-one/setup')))
  assert.deepEqual(requests[1].options, requests[2].options, 'Setup retry retains actor, model, seed, seal and key exactly.')
  assert.deepEqual(JSON.parse(requests[1].options.body), {
    actor: setupAttempt.actor, ...settings, reviewedSealHash: setup.reviewedSealHash,
  })
  assert.equal(requests[1].options.headers['Idempotency-Key'], setupAttempt.key)
  assert.equal(requests[1].options.headers['X-DrillSim-CSRF'], session.csrfRequestToken)
  globalThis.fetch = async () => new Response(JSON.stringify({ scenarioId: scenario.scenarioId, outcome: 'started', revealSucceeded: false }))
  await assert.rejects(sendOperatorAttempt(session, setupAttempt), /did not confirm simulator preparation/)
  globalThis.fetch = async () => new Response(JSON.stringify({ scenarioId: scenario.scenarioId, outcome: 'simulation-prepared', revealSucceeded: true }))
  await assert.rejects(sendOperatorAttempt(session, setupAttempt), /did not confirm simulator preparation/)
  for (const invalid of [{ scenarioId: 'foreign' }, { defaults: { ...settings, realizationSeed: -1 } },
    { profiles: [{ profileId: 'only-id' }] }, { prepared: true }, { reviewedSealHash: 'bad' }]) {
    globalThis.fetch = async () => new Response(JSON.stringify({ ...setup, ...invalid }))
    await assert.rejects(getOperatorSetup(scenario.scenarioId))
  }
  const setupHtml = renderToStaticMarkup(createElement(SimulatorSetup, {
    setup, view: unpreparedView, actor: 'Demo operator', blocked: false, error: '', onPrepare: () => { throw new Error('Rendering is read-only') },
  }))
  assert.match(setupHtml, /Field demonstration/)
  assert.match(setupHtml, /disabled=""[^>]*>Prepare simulator/)
  assert.doesNotMatch(setupHtml, /worldId|operatorKey/)
  const preparedHtml = renderToStaticMarkup(createElement(SimulatorSetup, {
    setup: prepared, view, actor: '', blocked: false, error: '', onPrepare: () => {},
  }))
  assert.match(preparedHtml, /Simulator prepared/)
  assert.doesNotMatch(preparedHtml, /<input|<select|>Prepare simulator/)
  const legacySetup = {
    ...prepared, current: null, profiles: [], defaults: { ...settings, profileId: null },
  }
  globalThis.fetch = async (url, options) => {
    assert.match(url, /\/scenarios\/scenario-one\/setup$/)
    assert.equal(options.method, undefined, 'Inspecting a prepared legacy setup remains read-only.')
    return new Response(JSON.stringify(legacySetup))
  }
  assert.deepEqual(await getOperatorSetup(scenario.scenarioId), legacySetup)
  const legacySetupHtml = renderToStaticMarkup(createElement(SimulatorSetup, {
    setup: legacySetup, view, actor: '', blocked: false, error: '', onPrepare: () => { throw new Error('Legacy setup must not be re-prepared') },
  }))
  assert.match(legacySetupHtml, /Simulator prepared/)
  assert.match(legacySetupHtml, /Earlier setups may not include a public settings summary/)
  assert.doesNotMatch(legacySetupHtml, /<input|<select|>Prepare simulator|Choose a preset/)
  assert.equal(operatorLifecycleLabel('simulation', view), 'Ready to start', 'Legacy empty profile options do not override the operator start gate.')

  const opening = { reservoirName: 'Formation', type: 'Perforated', topMdM: 1000.123456789, baseMdM: 1050,
    wellboreRadiusM: .1016, skin: -1.25, efficiency: .87654321, uncertaintyM: 2.75 }
  const completionView = {
    ...view, run: { ...run, status: 'AwaitingApproval', currentStage: 'S6DesignCompletion' },
    completion: { available: true, approvalEnabled: true, reason: 'Review all openings.',
      status: 'Draft', scenarioId: scenario.scenarioId, runId: run.runId, openingsHash: 'd'.repeat(64), openings: [opening] },
    actions: { ...view.actions, approveCompletion: available },
  }
  assert.equal(completionApprovalBlock(completionView, scenario.scenarioId, scenario.reservoirName), '')
  const reviewedIdentity = completionReviewKey(completionView)
  assert.equal(retainCompletionReview(reviewedIdentity, structuredClone(completionView)), reviewedIdentity,
    'Refreshing the same scope/hash must retain the explicitly reviewed completion')
  assert.equal(retainCompletionReview('', completionView), '', 'A refresh never creates acknowledgement')
  assert.equal(retainCompletionReview(reviewedIdentity, { ...completionView,
    completion: { ...completionView.completion, openingsHash: 'e'.repeat(64) } }), '',
    'A new server hash invalidates acknowledgement instead of substituting the new hash')
  assert.equal(retainCompletionReview(reviewedIdentity, { ...completionView,
    completion: { ...completionView.completion, runId: 'another-run' } }), '')
  assert.equal(retainCompletionReview(reviewedIdentity, { ...completionView,
    completion: { ...completionView.completion, scenarioId: 'another-scenario' } }), '')
  for (const change of [
    { available: false }, { openingsHash: null }, { openingsHash: 'invalid' }, { runId: 'another' }, { scenarioId: 'another' },
    { status: 'Approved' }, { openings: [] }, { openings: [{ ...opening, efficiency: null }] },
    { openings: [{ ...opening, reservoirName: 'Other' }] },
  ]) assert.ok(completionApprovalBlock({ ...completionView, completion: { ...completionView.completion, ...change } }, scenario.scenarioId, scenario.reservoirName))
  assert.ok(completionApprovalBlock({ ...completionView, run: { ...run, status: 'Running' } }, scenario.scenarioId, scenario.reservoirName))
  assert.ok(completionApprovalBlock({ ...completionView, actions: { ...view.actions, approveCompletion: { enabled: false } } }, scenario.scenarioId, scenario.reservoirName))
  assert.equal(completionAuditError('Local reviewer'), '')
  assert.ok(completionAuditError('a'.repeat(101)))
  assert.ok(completionAuditError('J\u00f6rg'))
  const completionAttempt = { ...attempt, action: 'approve-completion', runId: run.runId,
    reviewedOpeningHash: completionView.completion.openingsHash }
  assert.deepEqual(readOperatorAttempt(JSON.stringify(completionAttempt), scenario.scenarioId), completionAttempt)
  assert.throws(() => readOperatorAttempt(JSON.stringify({ ...completionAttempt, reviewedOpeningHash: null }), scenario.scenarioId))
  assert.equal(completionRetryBlock(completionView, completionAttempt), '')
  assert.ok(completionRetryBlock({ ...completionView, completion: { ...completionView.completion, openingsHash: 'e'.repeat(64) } }, completionAttempt))
  assert.equal(completionRetryBlock({ ...completionView, completion: { ...completionView.completion, status: 'Approved' } }, completionAttempt), '', 'A lost approval response can be safely retried using the same reviewed hash')
  requests.length = 0
  globalThis.fetch = async (url, options) => {
    requests.push({ url, options })
    return new Response(JSON.stringify({ scenarioId: scenario.scenarioId, runId: run.runId, outcome: 'completion-approved', revealSucceeded: false }))
  }
  assert.equal(operatorAttemptConfirmed(await sendOperatorAttempt(session, completionAttempt)), true)
  assert.match(requests[0].url, /\/runs\/run-one\/approve-completion$/)
  assert.deepEqual(JSON.parse(requests[0].options.body), { actor: attempt.actor, reviewedOpeningHash: completionView.completion.openingsHash })
  assert.equal(requests[0].options.headers['X-DrillSim-CSRF'], session.csrfRequestToken)
  assert.equal(requests[0].options.headers['Idempotency-Key'], attempt.key)
  const reviewProps = { view: completionView, scenario, actor: 'Local reviewer', blocked: false, reviewedKey: '', onReview: () => {}, onApprove: () => {} }
  const notReviewed = renderToStaticMarkup(createElement(OperatorCompletionReview, reviewProps))
  assert.match(notReviewed, /disabled=""[^>]*>Approve reviewed completion/)
  for (const value of Object.values(opening)) assert.ok(notReviewed.includes(String(value)), `Every review field is visible: ${value}`)
  const reviewed = renderToStaticMarkup(createElement(OperatorCompletionReview, { ...reviewProps, reviewedKey: completionReviewKey(completionView) }))
  assert.doesNotMatch(reviewed, /disabled=""[^>]*>Approve reviewed completion/)
  const changedHash = renderToStaticMarkup(createElement(OperatorCompletionReview, {
    ...reviewProps, reviewedKey: completionReviewKey(completionView),
    view: { ...completionView, completion: { ...completionView.completion, openingsHash: 'f'.repeat(64) } },
  }))
  assert.match(changedHash, /disabled=""[^>]*>Approve reviewed completion/, 'Changed review hash cannot inherit an acknowledgement')

  const recoveryView = {
    ...view, run: { ...run, status: 'Failed', currentStage: 'S8PublishReveal' },
    actions: { ...view.actions, publish: { enabled: false }, recoverPublication: available },
    publicationRecovery: { scenarioId: scenario.scenarioId, runId: run.runId, recoveryEnabled: true,
      reason: 'Only the reviewed staged write rejection can be recovered.',
      reviewedPublicationHash: 'b'.repeat(64), stagedManifestSha256: 'c'.repeat(64), publicationPlanSha256: 'd'.repeat(64),
      operationCount: 100, verifiedOperationCount: 63, pendingOperationCount: 37, completedStageCount: 8 },
  }
  const recoveryAttempt = { ...attempt, action: 'recover-publication', runId: run.runId,
    reason: 'Corrected optional trajectory validation.', reviewedPublicationHash: 'b'.repeat(64) }
  assert.equal(publicationRecoveryBlock(recoveryView, scenario.scenarioId), '')
  assert.equal(operatorLifecycleLabel('new-evidence', recoveryView), 'Recovery review required')
  const recoveryKey = publicationRecoveryKey(recoveryView)
  assert.equal(retainPublicationRecoveryReview(recoveryKey, structuredClone(recoveryView)), recoveryKey)
  for (const change of [
    { reviewedPublicationHash: 'e'.repeat(64) }, { runId: 'another' }, { scenarioId: 'another' }, { recoveryEnabled: false },
  ]) assert.equal(retainPublicationRecoveryReview(recoveryKey, { ...recoveryView,
    publicationRecovery: { ...recoveryView.publicationRecovery, ...change } }), '')
  for (const change of [
    { reviewedPublicationHash: 'invalid' }, { stagedManifestSha256: null }, { publicationPlanSha256: null },
    { verifiedOperationCount: 101 }, { pendingOperationCount: 36 }, { pendingOperationCount: -1 }, { completedStageCount: 7 },
    { operationCount: null }, { operationCount: 100.5 },
  ]) assert.ok(publicationRecoveryBlock({ ...recoveryView,
    publicationRecovery: { ...recoveryView.publicationRecovery, ...change } }, scenario.scenarioId))
  assert.ok(publicationRecoveryBlock(view, scenario.scenarioId), 'Old backend without capability remains blocked.')
  assert.ok(publicationRecoveryBlock({ ...recoveryView, run: { ...run, status: 'Scored' } }, scenario.scenarioId))
  assert.equal(recoveryAuditError('Local reviewer'), '')
  assert.ok(recoveryAuditError('a'.repeat(101)))
  for (const reason of ['', ' ', 'a'.repeat(501), 'bad\nreason', 'non-ASCII \u00e9']) assert.ok(recoveryReasonError(reason))
  for (const change of [{ reason: '' }, { reason: ' reason ' }, { reason: 10 }, { actor: ' reviewer ' },
    { actor: 'a'.repeat(101) }, { reviewedPublicationHash: '' }]) {
    assert.throws(() => readOperatorAttempt(JSON.stringify({ ...recoveryAttempt, ...change }), scenario.scenarioId))
  }
  assert.equal(publicationRecoveryRetryBlock(recoveryView, recoveryAttempt), '')
  assert.ok(publicationRecoveryRetryBlock(undefined, recoveryAttempt))
  assert.ok(publicationRecoveryRetryBlock({ ...recoveryView,
    publicationRecovery: { ...recoveryView.publicationRecovery, reviewedPublicationHash: 'e'.repeat(64) } }, recoveryAttempt))
  assert.equal(publicationRecoveryRetryBlock({ ...recoveryView, publicationRecovery: undefined,
    run: { ...run, status: 'PublishFailed' } }, recoveryAttempt), '', 'Lost successful response can replay the exact durable key.')
  requests.length = 0
  const recoveredResponse = { scenarioId: scenario.scenarioId, runId: run.runId, outcome: 'publication-recovered',
    previousStatus: 'Failed', status: 'PublishFailed', reviewedPublicationHash: recoveryAttempt.reviewedPublicationHash,
    auditId: 'audit-one' }
  globalThis.fetch = async (url, options) => {
    requests.push({ url, options })
    return new Response(JSON.stringify(recoveredResponse))
  }
  const recovered = await sendOperatorAttempt(session, recoveryAttempt)
  await sendOperatorAttempt(session, readOperatorAttempt(JSON.stringify(recoveryAttempt), scenario.scenarioId))
  assert.equal(operatorAttemptConfirmed(recovered), true)
  assert.equal(recovered.revealSucceeded, false)
  assert.equal(requests.length, 2, 'Recovery must never chain publication.')
  assert.ok(requests.every(({ url }) => url.endsWith('/runs/run-one/recover-publication')))
  assert.deepEqual(requests[0].options, requests[1].options, 'Retry retains the exact actor/reason/hash/key.')
  assert.deepEqual(JSON.parse(requests[0].options.body), { actor: recoveryAttempt.actor,
    reason: recoveryAttempt.reason, reviewedPublicationHash: recoveryAttempt.reviewedPublicationHash })
  assert.equal(requests[0].options.headers['X-DrillSim-CSRF'], session.csrfRequestToken)
  globalThis.fetch = async () => new Response(JSON.stringify({ ...recoveredResponse, reviewedPublicationHash: 'f'.repeat(64) }))
  await assert.rejects(sendOperatorAttempt(session, recoveryAttempt), /does not confirm/)
  const recoveryProps = { view: recoveryView, scenarioId: scenario.scenarioId, actor: recoveryAttempt.actor,
    reason: recoveryAttempt.reason, blocked: false, reviewedKey: '', onReason: () => {}, onReview: () => {}, onRecover: () => {} }
  assert.match(renderToStaticMarkup(createElement(OperatorPublicationRecovery, recoveryProps)),
    /disabled=""[^>]*>Recover reviewed publication/)
  const reviewedRecovery = renderToStaticMarkup(createElement(OperatorPublicationRecovery, { ...recoveryProps, reviewedKey: recoveryKey }))
  assert.doesNotMatch(reviewedRecovery, /disabled=""[^>]*>Recover reviewed publication/)
  assert.match(reviewedRecovery, /does not publish evidence/)
  assert.match(reviewedRecovery, new RegExp(recoveryAttempt.reviewedPublicationHash))
  assert.match(renderToStaticMarkup(createElement(OperatorPublicationRecovery, { ...recoveryProps, reviewedKey: recoveryKey,
    view: { ...recoveryView, publicationRecovery: { ...recoveryView.publicationRecovery, reviewedPublicationHash: 'e'.repeat(64) } } })),
  /disabled=""[^>]*>Recover reviewed publication/)

  const correctionView = { ...view, run: { ...run, status: 'Failed', currentStage: 'S9Score' },
    actions: { ...view.actions, correctScore: available, score: { enabled: false } },
    scoreCorrection: { scenarioId: scenario.scenarioId, runId: run.runId, correctionEnabled: true, reason: 'Review exact correction.',
      rejectedScorecardId: 'old-score', rejectedScorecardSha256: '1'.repeat(64), scoringInputSha256: '2'.repeat(64),
      correctionVersion: 'absolute-error-bounds-v2', metricCount: 94, invalidMetricCount: 3, changedMetricCount: 5,
      reviewedCorrectionHash: '3'.repeat(64), correctedScorecardId: 'new-score', correctedScorecardSha256: '4'.repeat(64),
      correctedInputSha256: '5'.repeat(64) },
  }
  const correctionAttempt = { ...attempt, action: 'correct-score', runId: run.runId,
    reason: 'Correct absolute-error enclosures without changing original input.', reviewedCorrectionHash: '3'.repeat(64) }
  assert.equal(scoreCorrectionBlock(correctionView, scenario.scenarioId), '')
  assert.equal(operatorLifecycleLabel('evaluation', correctionView), 'Score correction review')
  const correctionKey = scoreCorrectionKey(correctionView)
  assert.equal(retainScoreCorrectionReview(correctionKey, structuredClone(correctionView)), correctionKey)
  for (const change of [
    { reviewedCorrectionHash: '6'.repeat(64) }, { correctionEnabled: false }, { scenarioId: 'wrong' }, { runId: 'wrong' },
  ]) assert.equal(retainScoreCorrectionReview(correctionKey, { ...correctionView,
    scoreCorrection: { ...correctionView.scoreCorrection, ...change } }), '')
  for (const change of [
    { correctedScorecardId: 'old-score' }, { scoringInputSha256: null }, { rejectedScorecardSha256: 'invalid' },
    { correctedInputSha256: null }, { metricCount: 0 }, { invalidMetricCount: 6 }, { changedMetricCount: 95 },
    { correctionVersion: 'unknown' }, { invalidMetricCount: 1.5 },
  ]) assert.ok(scoreCorrectionBlock({ ...correctionView, scoreCorrection: { ...correctionView.scoreCorrection, ...change } }, scenario.scenarioId))
  assert.ok(scoreCorrectionBlock(view, scenario.scenarioId))
  assert.ok(scoreCorrectionBlock({ ...correctionView, run: { ...run, status: 'Revealed' } }, scenario.scenarioId))
  for (const change of [{ reviewedCorrectionHash: null }, { reason: '' }, { reason: ' bad ' }, { actor: 'a'.repeat(101) }]) {
    assert.throws(() => readOperatorAttempt(JSON.stringify({ ...correctionAttempt, ...change }), scenario.scenarioId))
  }
  assert.equal(scoreCorrectionRetryBlock(correctionView, correctionAttempt), '')
  assert.ok(scoreCorrectionRetryBlock(undefined, correctionAttempt))
  assert.ok(scoreCorrectionRetryBlock({ ...correctionView,
    scoreCorrection: { ...correctionView.scoreCorrection, reviewedCorrectionHash: '6'.repeat(64) } }, correctionAttempt))
  assert.equal(scoreCorrectionRetryBlock({ ...correctionView, scoreCorrection: undefined,
    run: { ...run, status: 'AwaitingDependency', currentStage: 'S9Score' } }, correctionAttempt), '')
  const correctedResponse = { ...correctionView.scoreCorrection, outcome: 'score-corrected', status: 'AwaitingDependency', auditId: 'correction-audit' }
  requests.length = 0
  globalThis.fetch = async (url, options) => {
    requests.push({ url, options })
    return new Response(JSON.stringify(correctedResponse))
  }
  const corrected = await sendOperatorAttempt(session, correctionAttempt)
  await sendOperatorAttempt(session, readOperatorAttempt(JSON.stringify(correctionAttempt), scenario.scenarioId))
  assert.equal(operatorAttemptConfirmed(corrected), true)
  assert.equal(corrected.revealSucceeded, true, 'Already published evidence stays visible.')
  assert.equal(corrected.scoringStatus, 'pending')
  assert.equal(requests.length, 2, 'Appending a correction never invokes scoring or publication.')
  assert.ok(requests.every(({ url }) => url.endsWith('/runs/run-one/correct-score')))
  assert.deepEqual(requests[0].options, requests[1].options)
  assert.deepEqual(JSON.parse(requests[0].options.body), { actor: correctionAttempt.actor,
    reason: correctionAttempt.reason, reviewedCorrectionHash: correctionAttempt.reviewedCorrectionHash })
  globalThis.fetch = async () => new Response(JSON.stringify({ ...correctedResponse, correctedScorecardId: 'old-score' }))
  await assert.rejects(sendOperatorAttempt(session, correctionAttempt), /does not confirm/)
  const correctionProps = { view: correctionView, scenarioId: scenario.scenarioId, actor: correctionAttempt.actor,
    reason: correctionAttempt.reason, blocked: false, reviewedKey: '', onReason: () => {}, onReview: () => {}, onCorrect: () => {} }
  assert.match(renderToStaticMarkup(createElement(OperatorScoreCorrection, correctionProps)),
    /disabled=""[^>]*>Append reviewed score correction/)
  const reviewedCorrection = renderToStaticMarkup(createElement(OperatorScoreCorrection, { ...correctionProps, reviewedKey: correctionKey }))
  assert.doesNotMatch(reviewedCorrection, /disabled=""[^>]*>Append reviewed score correction/)
  assert.match(reviewedCorrection, /Preserve the rejected scorecard/)
  assert.match(reviewedCorrection, /without publishing evaluation/)
  assert.match(reviewedCorrection, /old-score/)
  assert.match(reviewedCorrection, /new-score/)
  assert.match(renderToStaticMarkup(createElement(OperatorScoreCorrection, { ...correctionProps, reviewedKey: correctionKey,
    view: { ...correctionView, scoreCorrection: { ...correctionView.scoreCorrection, reviewedCorrectionHash: '6'.repeat(64) } } })),
  /disabled=""[^>]*>Append reviewed score correction/)

  let mutationCount = 0
  globalThis.fetch = async () => { mutationCount++; throw new Error('Rendering must not send requests') }
  storage.set(operatorAttemptKey(scenario.scenarioId), JSON.stringify({ ...attempt, action: 'publish', runId: 'run-one' }))
  storage.set(operatorOutcomeKey(scenario.scenarioId), JSON.stringify({
    scenarioId: scenario.scenarioId, outcome: 'scoring-failed', revealSucceeded: true, scoringStatus: 'failed',
  }))
  const html = renderToStaticMarkup(createElement(OperatorControls, {
    scenario, resources, taskId: 'new-evidence', onView: () => mutationCount++, onArtifactsChanged: () => mutationCount++,
  }))
  assert.match(html, /Unresolved publish attempt/)
  assert.match(html, /Local operator and audit details/)
  assert.match(html, /Reveal succeeded and remains published/)
  assert.match(html, /disabled=""[^>]*>Retry same attempt/)
  assert.doesNotMatch(html, /csrf-test-token/)
  assert.equal(mutationCount, 0)
  storage.set(operatorOutcomeKey(scenario.scenarioId), JSON.stringify({
    scenarioId: scenario.scenarioId, outcome: 'scoring-failed', revealSucceeded: true,
    scoringStatus: 'failed', clientAttemptKey: attempt.key,
  }))
  const failedHtml = renderToStaticMarkup(createElement(OperatorControls, {
    scenario, resources, taskId: 'new-evidence', onView: () => {}, onArtifactsChanged: () => {},
  }))
  assert.match(failedHtml, /Failed publish attempt/)
  assert.match(failedHtml, /same saved outcome/)
  assert.match(failedHtml, /use Evaluation for the new scoring attempt/)
  console.log('Operator checks passed: exact routes, CSRF, stable scoped retries, reviewed completion and staged-publication fingerprints, explicit recovery without publication, partial outcomes and artifact-safe badges.')
} finally {
  globalThis.fetch = originalFetch
  delete globalThis.sessionStorage
  await server.close()
}
