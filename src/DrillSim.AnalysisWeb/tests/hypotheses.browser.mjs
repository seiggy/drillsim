import assert from 'node:assert/strict'
import { createHash, randomUUID } from 'node:crypto'
import { pathToFileURL } from 'node:url'

// Optional browser workflow check using an existing Playwright installation; it adds no project dependency.
if (!process.env.DRILLSIM_PLAYWRIGHT_MODULE) throw new Error('Set DRILLSIM_PLAYWRIGHT_MODULE to an existing playwright/index.mjs.')
const { chromium } = await import(pathToFileURL(process.env.DRILLSIM_PLAYWRIGHT_MODULE).href)
const base = process.env.DRILLSIM_TEST_URL ?? 'http://localhost:5173'
const browser = await chromium.launch({ channel: 'msedge', headless: true })
const digest = value => createHash('sha256').update(JSON.stringify(value)).digest('hex')
let diagnostics = async () => ({})
try {
  const context = await browser.newContext({ viewport: { width: 1440, height: 1100 } })
  const page = await context.newPage()
  const revisions = new Map(), challenges = new Map(), analyses = new Map(), mutations = []
  const unexpectedWrites = [], errors = []
  diagnostics = async () => ({
    headings: await page.locator('h2,h3,h4').allTextContents(),
    alerts: await page.getByRole('alert').allTextContents(),
    selects: await page.locator('select').evaluateAll(elements => elements.map(element => element.closest('label')?.textContent?.slice(0, 120))),
    overflow: await page.evaluate(() => [...document.querySelectorAll('body *')].filter(element => {
      if (element.getBoundingClientRect().right <= innerWidth + 1) return false
      for (let parent = element.parentElement; parent && parent !== document.body; parent = parent.parentElement) {
        if (['auto', 'scroll', 'hidden', 'clip'].includes(getComputedStyle(parent).overflowX)) return false
      }
      return true
    }).slice(0, 12).map(element => ({ tag: element.tagName, class: element.className,
      width: element.getBoundingClientRect().width, parent: element.parentElement?.className }))),
    errors,
  })
  let currentPackage, currentAnalysis, conflictNextRevision = false
  page.on('pageerror', error => errors.push(error.message))
  page.on('console', message => { if (message.type() === 'error' && /same key/i.test(message.text())) errors.push(message.text()) })
  page.on('response', async response => {
    if (response.request().method() !== 'GET' || response.status() !== 200) return
    if (/\/api\/fields\/[^/]+\/package(?:\?|$)/.test(response.url())) currentPackage = await response.json()
    if (/\/api\/fields\/[^/]+\/analysis(?:\?|$)/.test(response.url())) {
      currentAnalysis = await response.json()
      analyses.set(currentAnalysis.analysisSha256, currentAnalysis)
    }
  })
  const summary = revision => ({
    hypothesisId: revision.hypothesisId, revision: revision.revision, name: revision.name, scope: revision.scope,
    packageSha256: revision.package.sha256, configurationSha256: revision.analysis.configurationSha256,
    analysisSha256: revision.analysis.analysisSha256, selectedCandidateId: revision.input.selectedCandidateId,
    snapshotSha256: revision.snapshotSha256, createdUtc: revision.createdUtc,
  })
  const makeRevision = (id, number, name, scope, input, branchedFrom, pkg = currentPackage) => {
    const value = structuredClone({ schemaVersion: 'hypothesis-revision-v1', hypothesisId: id, revision: number, name, scope, input,
      branchedFrom, package: pkg, analysis: analyses.get(input.analysisSha256), createdUtc: new Date().toISOString() })
    assert.ok(value.analysis, 'Fixture must use an actual analysis response, not invented output')
    value.snapshotSha256 = digest(value)
    const history = revisions.get(id) ?? []
    history.push(value); revisions.set(id, history)
    return value
  }
  const exact = reference => revisions.get(reference.hypothesisId)?.find(item => item.revision === reference.revision)
  const candidateOf = value => value.analysis.candidateGrid.find(cell => cell.candidateId === value.input.selectedCandidateId).prediction
  await page.route('**/analysis-api/**', async route => {
    const request = route.request(), url = new URL(request.url()), method = request.method()
    const respond = (value, status = 200) => route.fulfill({ status, contentType: 'application/json', body: JSON.stringify(value) })
    if (!url.pathname.includes('/api/hypotheses')) {
      if (!['GET', 'HEAD', 'OPTIONS'].includes(method)) { unexpectedWrites.push(request.url()); return route.abort() }
      return route.continue()
    }
    const tail = url.pathname.split('/api/hypotheses')[1]
    const parts = tail.split('/').filter(Boolean)
    const body = method === 'GET' ? null : request.postDataJSON()
    const offset = Number(url.searchParams.get('offset') ?? 0)
    const limit = Number(url.searchParams.get('limit') ?? 20)
    if (method === 'POST' && tail.endsWith('/analysis')) {
      const source = exact({ hypothesisId: parts[0], revision: Number(parts[2]) })
      // Only read-oriented deterministic calculation hits the live backend; all artifact writes stay in this fixture.
      const response = await context.request.post(`${base}/analysis-api/api/fields/${source.scope.fieldId}/analysis`, {
        data: { reservoir: source.scope.reservoirName, configuration: body.configuration,
          ...(source.scope.scenarioId ? { scenarioId: source.scope.scenarioId, asOf: source.scope.asOfUtc } : {}) },
      })
      assert.equal(response.status(), 200)
      const result = await response.json()
      assert.equal(result.packageSha256, source.package.sha256)
      analyses.set(result.analysisSha256, result)
      return respond(result)
    }
    if (method === 'POST' && tail === '/compare') {
      const values = body.revisions.map(selection => exact(selection.reference))
      assert.ok(values.every(Boolean))
      const baseline = values[0]
      return respond({ schemaVersion: 'hypothesis-comparison-v1',
        baseline: { hypothesisId: baseline.hypothesisId, revision: baseline.revision },
        entries: values.map(value => ({
          revision: summary(value), configuration: value.input.configuration, selectedCandidate: candidateOf(value),
          rationale: value.input.rationale, correlationNotes: value.input.correlationNotes, controlNotes: value.input.controlNotes,
          likeForLikeEvidence: value.package.sha256 === baseline.package.sha256,
          scopeDifferences: value.package.sha256 === baseline.package.sha256 ? [] : ['different-visible-package'],
          differences: Object.keys(value.input.configuration).filter(key => value.input.configuration[key] !== baseline.input.configuration[key])
            .map(key => ({ field: `configuration.${key}`, baselineValue: baseline.input.configuration[key], value: value.input.configuration[key] })),
        })), comparisonSha256: digest(body.revisions) })
    }
    if (method !== 'GET') {
      const headers = request.headers()
      assert.ok(headers['x-drillsim-csrf'])
      assert.ok(headers['idempotency-key'])
      mutations.push({ path: tail, method, body: structuredClone(body), headers })
    }
    if (method === 'POST' && tail === '') return respond(makeRevision(randomUUID(), 1, body.name, body.scope, body.input), 201)
    if (method === 'POST' && tail === '/branches') {
      const source = exact(body.source)
      return respond(makeRevision(randomUUID(), 1, body.name, body.scope, body.input, body.source, source.package), 201)
    }
    if (method === 'POST' && parts.length === 2 && parts[1] === 'revisions') {
      const previous = revisions.get(parts[0]).at(-1)
      assert.equal(request.headers()['if-match'], `"${previous.revision}"`)
      if (conflictNextRevision) {
        conflictNextRevision = false
        makeRevision(previous.hypothesisId, previous.revision + 1, previous.name, previous.scope, { ...previous.input, rationale: 'Another writer saved this rationale.' })
        return respond({ title: 'Hypothesis conflict', detail: 'A newer saved revision exists; inspect and reconcile.' }, 409)
      }
      return respond(makeRevision(previous.hypothesisId, previous.revision + 1, previous.name, body.scope, body.input), 201)
    }
    if (parts[3] === 'challenges') {
      const source = exact({ hypothesisId: parts[0], revision: Number(parts[2]) })
      if (method === 'POST') {
        const value = { schemaVersion: 'hypothesis-challenge-v1', challengeId: randomUUID(), version: 1,
          hypothesis: { hypothesisId: source.hypothesisId, revision: source.revision }, hypothesisSnapshotSha256: source.snapshotSha256,
          analysisSha256: body.analysisSha256, summary: body.summary, createdBy: body.actor, lastModifiedBy: body.actor,
          createdUtc: new Date().toISOString(), modifiedUtc: new Date().toISOString(),
          objections: body.objections.map(objection => ({ ...objection, objectionId: randomUUID(), disposition: 'open' })) }
        value.sha256 = digest(value); challenges.set(value.challengeId, [value])
        return respond(value, 201)
      }
      if (method === 'PUT') {
        const history = challenges.get(parts[4]), previous = history.at(-1)
        assert.equal(request.headers()['if-match'], `"${previous.version}"`)
        const value = { ...previous, version: previous.version + 1, lastModifiedBy: body.actor,
          objections: previous.objections.map(item => {
            const edit = body.dispositions.find(edit => edit.objectionId === item.objectionId)
            return edit ? { ...item, disposition: edit.disposition, dispositionReason: edit.reason, dispositionActor: body.actor } : item
          }) }
        value.sha256 = digest(value); history.push(value)
        return respond(value)
      }
      if (parts[4]) {
        const history = challenges.get(parts[4])
        return respond(url.searchParams.has('version') ? history.find(item => item.version === Number(url.searchParams.get('version'))) : history.at(-1))
      }
      return respond([...challenges.values()].map(history => history.at(-1)).filter(item =>
        item.hypothesis.hypothesisId === source.hypothesisId && item.hypothesis.revision === source.revision).slice(offset, offset + limit))
    }
    if (method === 'GET' && !parts.length) return respond([...revisions.values()].map(history => summary(history.at(-1))).slice(offset, offset + limit))
    if (method === 'GET' && parts.length === 1) return respond(revisions.get(parts[0]).at(-1))
    if (method === 'GET' && parts.length === 2) return respond([...revisions.get(parts[0])].reverse().map(summary).slice(offset, offset + limit))
    if (method === 'GET' && parts.length === 3) return respond(exact({ hypothesisId: parts[0], revision: Number(parts[2]) }))
    throw new Error(`Unexpected fixture request: ${method} ${tail}`)
  })
  await page.goto(base, { waitUntil: 'domcontentloaded' })
  await page.locator('.sequencer').waitFor({ timeout: 60000 })
  assert.ok(currentPackage && currentAnalysis)
  const liveHash = currentAnalysis.analysisSha256
  const task = name => page.locator('.sequencer').getByRole('button', { name: new RegExp(`^${name}\\.`) }).click()
  await task('Alternatives')
  await page.getByLabel('Hypothesis name', { exact: true }).fill('Workflow fixture baseline')
  await page.getByLabel('Reason for choosing this target', { exact: true }).fill('Baseline from the current evidence, retained for comparison.')
  await page.getByRole('button', { name: 'Save hypothesis', exact: true }).click()
  await page.getByRole('heading', { name: 'Loaded snapshot · Workflow fixture baseline · r1', exact: true }).waitFor()
  const baseline = [...revisions.values()][0][0]
  const artifactSummary = page.getByText('Last confirmed saved artifact', { exact: true })
  await artifactSummary.click()
  assert.ok((await artifactSummary.locator('..').innerText()).includes(baseline.snapshotSha256))
  assert.equal(await artifactSummary.locator('..').getByRole('button', { name: 'Download confirmed artifact JSON', exact: true }).count(), 1)
  await task('Correlation')
  assert.equal(await page.getByLabel('Correlation notes', { exact: true }).count(), 1)
  await page.getByText('Example correlation note', { exact: true }).click()
  const writesBeforeExample = mutations.length
  await page.getByRole('button', { name: 'Use example note', exact: true }).click()
  assert.match(await page.getByLabel('Correlation notes', { exact: true }).inputValue(), /^Check whether differences in formation depth/)
  assert.equal(mutations.length, writesBeforeExample, 'Inserting an example does not save or call AI.')
  await page.getByLabel('Correlation notes', { exact: true }).fill('Review formation continuity; annotation only, not a solver tie.')
  assert.equal(await page.getByRole('button', { name: 'Use example note', exact: true }).isDisabled(), true)
  await page.getByRole('button', { name: 'Select bore for note', exact: true }).first().click()
  await page.getByRole('button', { name: 'Add control note', exact: true }).click()
  await page.getByLabel(/^Control note ·/).fill('Selected for manual review only; do not change control inclusion.')
  await page.getByRole('button', { name: 'Save notes as revision 2', exact: true }).click()
  await page.getByRole('heading', { name: 'Loaded snapshot · Workflow fixture baseline · r2', exact: true }).waitFor()
  const savedInputSummary = page.getByText('Saved configuration, annotations and target', { exact: true })
  await savedInputSummary.click()
  const savedInputText = await savedInputSummary.locator('..').innerText()
  for (const text of ['Porosity cutoff', 'fraction', 'Permeability cutoff', 'm²', 'Neighbor count',
    'Review formation continuity; annotation only, not a solver tie.', 'Selected for manual review only; do not change control inclusion.']) {
    assert.ok(savedInputText.includes(text), `Readable saved input includes ${text}`)
  }
  assert.equal(await page.locator('.hypothesis-workbench pre').count(), 0)
  await task('Alternatives')
  await page.getByLabel('Branch preset', { exact: true }).selectOption('conservative')
  await page.getByRole('button', { name: 'Reanalyze stored evidence', exact: true }).click()
  await page.getByText(/Stored-evidence reanalysis available/).waitFor({ timeout: 60000 })
  await page.getByText('Reanalysis grid/status output', { exact: true }).click()
  const grid = page.getByRole('region', { name: 'Reanalysis grid cells', exact: true })
  assert.ok(await grid.locator('tbody tr').count() <= 20)
  const selectedBranchTarget = await page.getByLabel('Branch eligible target', { exact: true }).inputValue()
  const nextGridPage = page.getByRole('button', { name: 'Next grid cells', exact: true })
  if (await nextGridPage.isEnabled()) {
    await nextGridPage.click()
    assert.match(await grid.locator('caption').innerText(), /^Cells 21–/)
    await page.getByRole('button', { name: 'Previous grid cells', exact: true }).click()
    assert.match(await grid.locator('caption').innerText(), /^Cells 1–/)
  }
  assert.equal(await page.getByLabel('Branch eligible target', { exact: true }).inputValue(), selectedBranchTarget, 'Grid browsing does not select a new branch target')
  assert.equal(await page.getByRole('button', { name: 'Download full grid JSON', exact: true }).count(), 1)
  await page.getByLabel('New alternative name', { exact: true }).fill('Workflow fixture conservative')
  await page.getByLabel('Alternative rationale', { exact: true }).fill('Higher rock cutoffs on the same source package.')
  await page.getByRole('button', { name: 'Save named alternative branch', exact: true }).click()
  await page.getByRole('heading', { name: 'Loaded snapshot · Workflow fixture conservative · r1', exact: true }).waitFor()
  const branch = [...revisions.values()].find(history => history[0].name === 'Workflow fixture conservative')[0]
  assert.equal(branch.branchedFrom.revision, 2)
  assert.ok(branch.analysis.configuration.porosityCutoff > baseline.analysis.configuration.porosityCutoff)
  await page.getByRole('button', { name: 'Compare loaded exact revision', exact: true }).click()
  const list = page.getByRole('region', { name: 'Scoped saved hypotheses and exact revisions', exact: true })
  await list.getByRole('row').filter({ hasText: 'Workflow fixture baseline' }).getByRole('button', { name: 'Add exact revision to comparison', exact: true }).click()
  await task('Compare hypotheses')
  const comparisonText = await page.getByLabel('Comparison scopes and exact references · JSON', { exact: true }).inputValue()
  await page.getByRole('button', { name: 'Compare selected exact revisions', exact: true }).click()
  await page.getByRole('region', { name: 'Exact hypothesis comparison results', exact: true }).waitFor()
  assert.equal(await page.getByRole('button', { name: 'Download comparison JSON', exact: true }).count(), 1)
  assert.equal(await page.locator('.hypothesis-workbench pre').count(), 0)
  await page.getByText(/ · [1-9]\d* differences from exact baseline$/).first().click()
  assert.match(await page.locator('.hypothesis-workbench details[open]').last().innerText(), /configuration.porosityCutoff/)
  await task('Challenge')
  await page.getByLabel('Challenge summary', { exact: true }).fill('Manual critique of the exact conservative branch.')
  await page.getByLabel('Challenge audit label · not authentication', { exact: true }).fill('Workflow fixture reviewer')
  await page.getByLabel('Objection text', { exact: true }).fill('Validate the retained control evidence before preferring this branch.')
  await page.getByLabel('Cited evidence · select 1–32 visible IDs', { exact: true }).selectOption({ index: 0 })
  await page.getByRole('button', { name: 'Save challenge to exact revision', exact: true }).click()
  await page.getByRole('heading', { name: 'Disposition review · challenge version 1', exact: true }).waitFor()
  await page.getByLabel('Disposition audit label', { exact: true }).fill('Workflow fixture reviewer')
  await page.getByLabel('Disposition', { exact: true }).selectOption('deferred')
  await page.getByLabel('Disposition reason', { exact: true }).fill('Await an independent evidence review.')
  await page.getByRole('button', { name: 'Save disposition changes', exact: true }).click()
  await page.getByRole('heading', { name: 'Disposition review · challenge version 2', exact: true }).waitFor()
  const finalChallenge = [...challenges.values()][0].at(-1)
  assert.equal(finalChallenge.hypothesisSnapshotSha256, branch.snapshotSha256)
  assert.equal(finalChallenge.analysisSha256, branch.input.analysisSha256)
  assert.equal(finalChallenge.objections[0].disposition, 'deferred')
  await task('Correlation')
  await page.getByLabel('Reason for choosing this target', { exact: true }).fill('Unsaved reasoning must survive a version conflict.')
  conflictNextRevision = true
  await page.getByRole('button', { name: 'Save notes as revision 2', exact: true }).click()
  await page.getByText(/409: Hypothesis conflict/).waitFor()
  const capturedSummary = page.getByText('Captured request and stable attempt key', { exact: true })
  await capturedSummary.click()
  const capturedText = await capturedSummary.locator('..').innerText()
  assert.ok(capturedText.includes(mutations.at(-1).headers['idempotency-key']))
  assert.ok(capturedText.includes('Unsaved reasoning must survive a version conflict.'))
  assert.ok(capturedText.includes('Porosity cutoff'))
  assert.equal(await capturedSummary.locator('..').getByRole('button', { name: 'Download captured request JSON', exact: true }).count(), 1)
  assert.equal(await page.getByLabel('Reason for choosing this target', { exact: true }).inputValue(), 'Unsaved reasoning must survive a version conflict.')
  await page.getByRole('button', { name: 'Inspect latest saved revision', exact: true }).click()
  await page.getByRole('heading', { name: 'Loaded snapshot · Workflow fixture conservative · r2', exact: true }).waitFor()
  assert.ok(await page.evaluate(() => Object.values(sessionStorage).some(text => text.includes('Unsaved reasoning must survive a version conflict.'))))
  await page.getByText('Load an explicit saved reference in this scope', { exact: true }).click()
  await page.getByLabel('Hypothesis ID', { exact: true }).fill(branch.hypothesisId)
  await page.getByLabel('Exact revision', { exact: true }).fill('1')
  await page.getByRole('button', { name: 'Load exact reference', exact: true }).click()
  await page.getByRole('heading', { name: 'Loaded snapshot · Workflow fixture conservative · r1', exact: true }).waitFor()
  assert.equal(await page.getByLabel('Reason for choosing this target', { exact: true }).inputValue(), 'Unsaved reasoning must survive a version conflict.')
  await page.getByRole('button', { name: 'Inspect latest saved revision', exact: true }).click()
  await page.getByRole('heading', { name: 'Loaded snapshot · Workflow fixture conservative · r2', exact: true }).waitFor()
  await page.getByLabel('Reason for choosing this target', { exact: true }).fill('Explicit reconciliation: keep the other writer context and my retained reasoning.')
  await page.getByLabel('I inspected saved server history before retrying or retiring this local attempt.', { exact: true }).check()
  page.once('dialog', dialog => dialog.accept())
  await page.getByRole('button', { name: 'Retire local action record', exact: true }).click()
  await page.getByRole('button', { name: 'Save notes as revision 3', exact: true }).click()
  await page.getByRole('heading', { name: 'Loaded snapshot · Workflow fixture conservative · r3', exact: true }).waitFor()
  assert.notEqual(mutations.at(-1).headers['idempotency-key'], mutations.at(-2).headers['idempotency-key'], 'Reconciled save needs a new explicit attempt key')
  assert.equal(mutations.at(-1).headers['if-match'], '"2"')
  await task('Compare hypotheses')
  assert.equal(await page.getByLabel('Comparison scopes and exact references · JSON', { exact: true }).inputValue(), comparisonText)
  assert.ok((await page.locator('.analysis-revision').innerText()).includes(liveHash.slice(0, 16)), 'Historical loads never relabel live output')
  await page.setViewportSize({ width: 390, height: 844 })
  assert.ok(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth))
  assert.deepEqual(unexpectedWrites, [])
  assert.deepEqual(errors, [])
  console.log(JSON.stringify({ status: 'passed', fixtureOnlyArtifactPersistence: true, realDeterministicReanalysis: true,
    artifactWrites: mutations.length, namedHypotheses: revisions.size, versionedChallenges: finalChallenge.version,
    staleTextRetained: true, conflictExplicitlyReconciled: true, exactComparisonsRetained: true, historicalIsolation: true, noLiveArtifactWrites: true }))
} catch (error) { console.error(await diagnostics()); throw error }
finally { await browser.close() }
