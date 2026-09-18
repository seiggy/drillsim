import assert from 'node:assert/strict'
import { pathToFileURL } from 'node:url'
import { installSimulationFixture } from './simulation-fixture.mjs'

if (!process.env.DRILLSIM_PLAYWRIGHT_MODULE) throw new Error('Set DRILLSIM_PLAYWRIGHT_MODULE to an existing playwright/index.mjs.')
const { chromium } = await import(pathToFileURL(process.env.DRILLSIM_PLAYWRIGHT_MODULE).href)
const browser = await chromium.launch({ channel: 'msedge', headless: true })
try {
  const context = await browser.newContext({ viewport: { width: 1440, height: 1000 }, reducedMotion: 'reduce' })
  await installSimulationFixture(context)
  const page = await context.newPage()
  const errors = []
  page.on('pageerror', error => errors.push(error.message))
  await page.goto(process.env.DRILLSIM_TEST_URL ?? 'http://localhost:5173')
  const link = page.getByRole('link', { name: 'Demo & architecture guide', exact: true })
  await link.waitFor()
  const guideUrl = new URL(await link.getAttribute('href'), page.url()).href
  assert.equal(new URL(guideUrl).origin, new URL(page.url()).origin, 'The guide must open locally before GitHub Pages is deployed.')
  const response = await page.goto(guideUrl)
  assert.equal(response.status(), 200)
  const guideRequests = []
  page.on('request', request => guideRequests.push(request.url()))
  const preflight = page.locator('#preflight')
  assert.equal(await preflight.locator('h2').innerText(), await page.locator('.outline a[href="#preflight"]').innerText())
  for (const id of ['tools', 'secrets', 'azure', 'start', 'datasets', 'model', 'reset', 'checklist'])
    assert.equal(await preflight.locator(`#preflight-${id}`).count(), 1)
  const preflightText = await preflight.textContent()
  for (const setting of ['reservoir-simulation-operator-key', 'drilling-operations-internal-key',
    'drilling-operations-analysis-callback-key', 'publication-import-key', 'AZURE_CONFIG_DIR', '/openai/v1/',
    'Azure Maps Data Reader', 'Cognitive Services OpenAI User', 'AzureResources:Provision',
    'AzureResources:ModelName', 'AzureResources:ModelVersion', 'AzureResources:ModelCapacity'])
    assert.ok(preflightText.includes(setting), `Preflight must cover ${setting}.`)
  assert.match(preflightText, /Aspire generates any missing local service keys/)
  assert.ok(!preflightText.includes('$keyNames'), 'Key generation belongs in the AppHost, not a pasted setup script.')
  assert.equal((await preflight.locator('#preflight-azure-login').innerText()).trim(), 'az login')
  const firstStart = await preflight.locator('#preflight-start-command').innerText()
  assert.match(firstStart, /aspire run --apphost/)
  assert.ok(!firstStart.includes('--non-interactive'), 'First-time Azure setup must allow Aspire prompts.')
  assert.match(preflightText, /Aspire asks for any missing tenant, subscription, resource group and location choices/)
  assert.match(preflightText, /Complete existing connection settings.*are reused/)
  assert.match(preflightText, /does not delete the provisioned Azure resources/)
  assert.ok(!preflightText.includes('dotnet user-secrets set "Parameters:azure-'),
    'Azure connection values must be supplied by Aspire rather than a mandatory manual setup block.')
  const importCommands = await preflight.locator('#preflight-load-troll').innerText()
  assert.match(importCommands, /--dataset force-2020 --dataset sodir-force-troll/)
  assert.match(importCommands, /--limit 46 --dry-run/)
  assert.match(await preflight.locator('#preflight-seed-model').innerText(), /seed_reservoir_world\.py/)
  const resetCommands = await preflight.locator('#preflight-reset-data').innerText()
  assert.ok(resetCommands.indexOf('aspire stop') < resetCommands.indexOf('Move-Item'))
  assert.ok(resetCommands.indexOf('Move-Item') < resetCommands.indexOf('aspire start'))
  assert.match(resetCommands, /-wal\|-shm\|-journal/)
  assert.ok(!resetCommands.includes('Remove-Item'), 'The documented reset must archive, not delete, the database set.')
  const screenshotUrls = await page.locator('a[href^="screenshots/"], img[src^="screenshots/"]').evaluateAll(elements =>
    [...new Set(elements.map(element => element.href || element.src))])
  assert.ok(screenshotUrls.length >= 19)
  for (const url of screenshotUrls) {
    assert.equal(new URL(url).origin, new URL(guideUrl).origin)
    const image = await context.request.get(url)
    assert.equal(image.status(), 200, url)
    assert.match(image.headers()['content-type'], /^image\/png/, url)
    const bytes = await image.body()
    assert.equal(bytes.subarray(0, 8).toString('hex'), '89504e470d0a1a0a', url)
  }
  await page.locator('.walkthrough-shot img').evaluateAll(images => {
    images.forEach(image => { image.loading = 'eager' })
    return Promise.all(images.map(image => image.decode()))
  })
  assert.ok(await page.locator('.walkthrough-shot img').evaluateAll(images =>
    images.every(image => image.naturalWidth > 0 && image.alt.trim().length > 0)))
  assert.equal(await page.locator('.chapter.task').count(), 19)
  const chapter4 = await page.locator('#guided-simulation').innerText()
  for (const action of ['Create scenario', 'Save draft', 'Seal reviewed revision', 'Approve reviewed prediction',
    'Prepare simulator', 'Start simulation', 'Approve reviewed completion', 'Publish reveal and evaluate']) assert.ok(chapter4.includes(action))
  assert.match(chapter4, /demonstration estimates/)
  for (const id of ['evidence', 'qc', 'position', 'correlation', 'criteria', 'pay', 'search', 'exclusions', 'model',
    'uncertainty', 'targets', 'challenge', 'alternatives', 'compare', 'bundle', 'prediction', 'simulation', 'new-evidence', 'evaluation']) {
    const task = page.locator(`#${id}.task`)
    assert.equal(await task.count(), 1)
    assert.ok(await task.locator(':scope > figure.task-shot').count() >= 1, `${id} must illustrate its own workspace inline.`)
    assert.ok(await task.locator('figure.task-shot img').evaluateAll(images =>
      images.every(image => image.naturalWidth > 0 && image.alt.trim().length > 0)), `${id} screenshot must render with alt text.`)
    assert.ok((await task.locator('figure.task-shot figcaption').allTextContents()).every(caption => caption.trim().length > 30),
      `${id} screenshot needs a useful caption, not just an image filename.`)
    assert.deepEqual(await task.locator('.task-facts > dt').allTextContents(),
      ['SAY', 'DO', 'EXPECT', 'INPUT', 'OUTPUT', 'STATE EFFECT', 'ARCHITECTURE', 'RISKS'])
  }
  const broken = await page.locator('a[href^="#"]').evaluateAll(links =>
    links.map(link => link.getAttribute('href')).filter(href => href.length > 1 && !document.getElementById(decodeURIComponent(href.slice(1)))))
  assert.deepEqual(broken, [])
  await page.getByRole('button', { name: 'Presenter mode', exact: true }).click()
  assert.equal(await page.locator('main > .chapter:visible').count(), 1)
  await page.locator('#next-button').click()
  assert.equal(await page.locator('#routes').isVisible(), true)
  await page.locator('#previous-button').click()
  assert.equal(await page.locator('#start').isVisible(), true)
  await page.locator('.outline a[href="#architecture"]').click()
  assert.equal(await page.locator('#architecture').isVisible(), true)
  const flow = page.locator('[data-flow]')
  assert.equal(await flow.count(), 3)
  await flow.nth(1).click()
  assert.equal(await flow.nth(1).getAttribute('aria-pressed'), 'true')
  assert.ok(await page.locator('[data-path].active').count())
  for (const link of await page.locator('.outline .task-link').all()) {
    const id = (await link.getAttribute('href')).slice(1)
    await link.click()
    assert.equal(await page.locator(`#${id}.task`).isVisible(), true)
    assert.ok(await page.locator(`#${id} figure.task-shot img:visible`).count() >= 1,
      `${id} must retain its screenshot in presenter mode.`)
  }
  const theme = page.locator('#theme-button')
  const initialTheme = await page.locator('html').getAttribute('data-theme')
  await theme.click()
  assert.notEqual(await page.locator('html').getAttribute('data-theme'), initialTheme)
  await page.setViewportSize({ width: 390, height: 844 })
  assert.ok(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth))
  await page.emulateMedia({ media: 'print' })
  assert.equal(await page.locator('main > .chapter:visible').count(), await page.locator('main > .chapter').count())
  await page.emulateMedia({ media: 'screen' })
  assert.ok(guideRequests.every(url => new URL(url).origin === new URL(guideUrl).origin &&
    /\/screenshots\/[^/]+\.png$/.test(new URL(url).pathname)),
  'Guide controls may load local screenshots but must not call APIs or external services.')
  assert.deepEqual(errors, [])
  const noJs = await browser.newContext({ javaScriptEnabled: false })
  const staticPage = await noJs.newPage()
  await staticPage.goto(guideUrl)
  assert.equal(await staticPage.locator('.chapter.task:visible').count(), 19)
  assert.equal(await staticPage.locator('#mode-button').isVisible(), false)
  await noJs.close()
  console.log(JSON.stringify({ status: 'passed', localGuideLink: true, tasks: 19, presenterNavigation: true,
    architectureControls: true, lightDarkMobilePrint: true, noJavaScriptReadable: true,
    preflightAndDatasetSetup: true, illustratedEngineeringTasks: 19,
    screenshotLinks: screenshotUrls.length, screenshotRequestsOnly: true }))
} finally { await browser.close() }
