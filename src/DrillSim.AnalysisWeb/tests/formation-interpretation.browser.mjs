import assert from 'node:assert/strict'
import { pathToFileURL } from 'node:url'

if (!process.env.DRILLSIM_PLAYWRIGHT_MODULE) throw new Error('Set DRILLSIM_PLAYWRIGHT_MODULE to an existing playwright/index.mjs.')
const { chromium } = await import(pathToFileURL(process.env.DRILLSIM_PLAYWRIGHT_MODULE).href)
const browser = await chromium.launch({ channel: 'msedge', headless: true })
try {
  const page = await browser.newPage({ viewport: { width: 1440, height: 1000 } })
  page.setDefaultTimeout(30000)
  const drafts = [], unexpected = [], errors = []
  let analysis, release, incoming, responseMode = 'valid', configured = true
  page.on('pageerror', error => errors.push(error.message))
  page.on('request', request => {
    if (!['POST', 'PUT', 'PATCH', 'DELETE'].includes(request.method())) return
    if (!/\/api\/formation-interpretation$|\/api\/fields\/[^/]+\/analysis$/.test(request.url())) unexpected.push(request.url())
  })
  page.on('response', async response => {
    if (response.status() === 200 && /\/api\/fields\/[^/]+\/analysis(?:\?|$)/.test(response.url())) analysis = await response.json()
  })
  await page.route('**/api/formation-interpretation/status', route => route.fulfill({
    status: 200, contentType: 'application/json', body: JSON.stringify({
      configured, reason: configured ? null : 'AI is unavailable for this test.',
    }),
  }))
  await page.route('**/api/formation-interpretation', async route => {
    const request = route.request().postDataJSON()
    drafts.push({ request, headers: route.request().headers() })
    const mode = responseMode
    await new Promise(resolve => { release = resolve; incoming() })
    if (mode === 'error' || mode === 'rate-limited') {
      await route.fulfill({ status: mode === 'rate-limited' ? 429 : 502, contentType: 'application/json', body: JSON.stringify({
        detail: mode === 'rate-limited' ? 'AI capacity limit reached. No automatic retry was made.'
          : 'The model could not produce a validated interpretation.',
      }) })
      return
    }
    const response = {
      version: 'formation-interpretation-v1', promptVersion: 'formation-interpretation-prompt-v2',
      scope: request.scope, packageSha256: request.packageSha256, analysisSha256: request.analysisSha256,
      configurationSha256: analysis.configurationSha256, selectedCandidateId: request.selectedCandidateId,
      savedHypothesis: request.savedHypothesis, snapshotSha256: request.snapshotSha256, generatedAt: new Date().toISOString(),
      draft: { name: 'AI formation draft', rationale: 'Review the target against the available formation intervals.',
        correlationNotes: 'Check the reported formation depths against their reference datums before inferring structural continuity.',
        citedEvidenceIds: [mode === 'invalid' ? 'wellbore:foreign-record' : `field:${request.scope.fieldId}`],
        limitations: ['This draft does not establish a common vertical datum.'] },
    }
    await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(response) })
  })
  await page.goto(process.env.DRILLSIM_TEST_URL ?? 'http://localhost:5173')
  await page.locator('.sequencer').waitFor({ timeout: 60000 })
  await page.locator('[data-tutorial="task-correlation"]').click()
  const agent = page.getByRole('region', { name: 'Draft a formation interpretation', exact: true })
  const generate = agent.getByRole('button', { name: 'Draft interpretation with AI', exact: true })
  const beginDraft = async () => {
    const received = new Promise(resolve => { incoming = resolve })
    await generate.click()
    await received
  }
  const name = page.getByLabel('Hypothesis name', { exact: true })
  const rationale = page.getByRole('textbox', { name: 'Reason for choosing this target', exact: true })
  const correlation = page.getByRole('textbox', { name: 'Correlation notes', exact: true })
  await generate.waitFor()
  assert.equal(drafts.length, 0)
  await name.fill('My hypothesis name')
  await rationale.fill('My initial reasoning')
  await correlation.fill('My initial formation notes')
  await beginDraft()
  assert.equal(drafts.length, 1)
  assert.equal(drafts[0].request.notes.correlationNotes, 'My initial formation notes')
  assert.ok(drafts[0].headers['x-drillsim-csrf'])
  assert.ok(drafts[0].headers['idempotency-key'])
  assert.equal(await correlation.inputValue(), 'My initial formation notes')
  await correlation.fill('Notes edited while AI was working')
  release()
  await agent.getByRole('heading', { name: 'Review the AI draft', exact: true }).waitFor()
  const use = agent.getByRole('button', { name: 'Use this draft', exact: true })
  const review = agent.getByRole('checkbox', { name: /I reviewed this draft/ })
  assert.equal(await use.isDisabled(), true)
  assert.equal(await correlation.inputValue(), 'Notes edited while AI was working')
  await review.check()
  await rationale.fill('Later reasoning must also be reviewed before replacement')
  assert.equal(await review.isChecked(), false)
  assert.equal(await use.isDisabled(), true)
  await review.check()
  await use.click()
  assert.equal(await name.inputValue(), 'My hypothesis name', 'An existing hypothesis name is retained.')
  assert.match(await correlation.inputValue(), /AI-assisted interpretation/)
  assert.match(await correlation.inputValue(), /field:/)
  assert.match(await correlation.inputValue(), /common vertical datum/)
  assert.match(await rationale.inputValue(), /^Review the target/)
  assert.equal(unexpected.length, 0, 'Accepting a draft changes only local form text.')

  const accepted = await correlation.inputValue()
  responseMode = 'invalid'
  await beginDraft()
  await agent.getByRole('button', { name: 'Cancel drafting', exact: true }).waitFor()
  release()
  await agent.getByRole('alert').waitFor()
  assert.match(await agent.getByRole('alert').innerText(), /unsupported evidence/)
  assert.equal(await correlation.inputValue(), accepted)
  assert.equal(await use.count(), 0)
  assert.equal(drafts.length, 2)

  responseMode = 'rate-limited'
  await beginDraft()
  release()
  await agent.getByRole('alert').waitFor()
  assert.match(await agent.getByRole('alert').innerText(), /429: AI capacity limit reached/)
  assert.equal(await correlation.inputValue(), accepted)
  assert.equal(drafts.length, 3, 'A provider rate limit must not trigger an automatic inference retry.')

  responseMode = 'valid'
  await beginDraft()
  await agent.getByRole('button', { name: 'Cancel drafting', exact: true }).click()
  release()
  await agent.getByText('Drafting cancelled. Your notes are unchanged.', { exact: true }).waitFor()
  assert.equal(await correlation.inputValue(), accepted)
  assert.equal(await use.count(), 0)

  configured = false
  await agent.getByRole('button', { name: 'Refresh AI status', exact: true }).click()
  await agent.getByText('AI is unavailable for this test.', { exact: true }).waitFor()
  assert.equal(await generate.isDisabled(), true)
  assert.equal(drafts.length, 4)
  configured = true
  await agent.getByRole('button', { name: 'Refresh AI status', exact: true }).click()
  await page.locator('[data-tutorial="task-search"]').click()
  await page.getByRole('spinbutton', { name: /Grid points per axis/ }).fill('9')
  const applied = page.waitForResponse(response => /\/api\/fields\/[^/]+\/analysis$/.test(response.url()))
  await page.getByRole('button', { name: 'Apply analysis settings', exact: true }).click()
  analysis = await (await applied).json()
  await page.locator('[data-tutorial="task-correlation"]').click()
  await beginDraft()
  await agent.getByRole('button', { name: 'Cancel drafting', exact: true }).waitFor()
  assert.equal(drafts.at(-1).request.configuration.gridPointsPerAxis, 9)
  assert.equal(drafts.at(-1).request.analysisSha256, analysis.analysisSha256)
  await page.locator('[data-tutorial="task-targets"]').click()
  release()
  await page.locator('[data-tutorial="task-correlation"]').click()
  assert.equal(await use.count(), 0, 'A response from the abandoned editor must not replace the current draft.')
  assert.equal(unexpected.length, 0)
  await page.setViewportSize({ width: 390, height: 844 })
  assert.ok(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth))
  assert.deepEqual(errors, [])
  console.log(JSON.stringify({ status: 'passed', mockedAiOnly: true, explicitGeneration: true, generatedRequests: drafts.length,
    reviewBeforeReplacing: true, existingNameKept: true, citationsRetained: true, invalidOutputRejected: true, rateLimitPreservesNotes: true,
    cancellationAndNavigationSafe: true, configuredAnalysisBound: true, artifactWrites: 0, mobileOverflow: false }))
} finally { await browser.close() }
