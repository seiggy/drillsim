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
  assert.equal(await page.locator('.chapter.task').count(), 19)
  const chapter4 = await page.locator('#guided-simulation').innerText()
  for (const action of ['Create scenario', 'Save draft', 'Seal reviewed revision', 'Approve reviewed prediction',
    'Prepare simulator', 'Start simulation', 'Approve reviewed completion', 'Publish reveal and evaluate']) assert.ok(chapter4.includes(action))
  assert.match(chapter4, /demonstration estimates/)
  for (const id of ['evidence', 'qc', 'position', 'correlation', 'criteria', 'pay', 'search', 'exclusions', 'model',
    'uncertainty', 'targets', 'challenge', 'alternatives', 'compare', 'bundle', 'prediction', 'simulation', 'new-evidence', 'evaluation']) {
    assert.equal(await page.locator(`#${id}.task`).count(), 1)
    assert.deepEqual(await page.locator(`#${id} .task-facts > dt`).allTextContents(),
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
  const theme = page.locator('#theme-button')
  const initialTheme = await page.locator('html').getAttribute('data-theme')
  await theme.click()
  assert.notEqual(await page.locator('html').getAttribute('data-theme'), initialTheme)
  await page.setViewportSize({ width: 390, height: 844 })
  assert.ok(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth))
  await page.emulateMedia({ media: 'print' })
  assert.equal(await page.locator('main > .chapter:visible').count(), await page.locator('main > .chapter').count())
  await page.emulateMedia({ media: 'screen' })
  assert.equal(guideRequests.length, 0, 'Reading the guide and using its controls needs no network requests.')
  assert.deepEqual(errors, [])
  const noJs = await browser.newContext({ javaScriptEnabled: false })
  const staticPage = await noJs.newPage()
  await staticPage.goto(guideUrl)
  assert.equal(await staticPage.locator('.chapter.task:visible').count(), 19)
  assert.equal(await staticPage.locator('#mode-button').isVisible(), false)
  await noJs.close()
  console.log(JSON.stringify({ status: 'passed', localGuideLink: true, tasks: 19, presenterNavigation: true,
    architectureControls: true, lightDarkMobilePrint: true, noJavaScriptReadable: true, guideNetworkRequests: 0 }))
} finally { await browser.close() }
