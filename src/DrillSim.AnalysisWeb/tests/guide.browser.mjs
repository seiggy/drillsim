import assert from 'node:assert/strict'
import { pathToFileURL } from 'node:url'
import { installSimulationFixture } from './simulation-fixture.mjs'

if (!process.env.DRILLSIM_PLAYWRIGHT_MODULE) throw new Error('Set DRILLSIM_PLAYWRIGHT_MODULE to an existing playwright/index.mjs.')
const { chromium } = await import(pathToFileURL(process.env.DRILLSIM_PLAYWRIGHT_MODULE).href)

const steps = ['field', 'prospects', 'forecast', 'drill', 'results']
const expectedScreenshots = ['field', 'prospects', 'prospects-ai', 'forecast', 'drill', 'results', 'settings']
  .map(name => `redesign-${name}.png`)
const removedFeatures = [/hypothes[ie]s/i, /guided tutorial/i, /evidence bundle/i, /sequencer/i, /\b19 (engineering )?tasks\b/i,
  /compare json/i, /\bchapter [1-4]\b/i, /presenter mode/i, /review checkbox/i]

const browser = await chromium.launch({ channel: 'msedge', headless: true })
try {
  const context = await browser.newContext({ viewport: { width: 1440, height: 1000 }, reducedMotion: 'reduce' })
  await installSimulationFixture(context)
  const page = await context.newPage()
  const errors = []
  page.on('pageerror', error => errors.push(error.message))
  await page.goto(process.env.DRILLSIM_TEST_URL ?? 'http://localhost:5173')

  const appSteps = await page.locator('nav.steps button strong').allTextContents()
  assert.deepEqual(appSteps, ['Field', 'Prospects', 'Forecast', 'Drill', 'Results'])
  const link = page.getByRole('link', { name: 'Demo guide', exact: true })
  await link.waitFor()
  const guideUrl = new URL(await link.getAttribute('href'), page.url()).href
  assert.equal(new URL(guideUrl).origin, new URL(page.url()).origin, 'The guide must open locally before GitHub Pages is deployed.')
  const response = await page.goto(guideUrl)
  assert.equal(response.status(), 200)
  const guideRequests = []
  page.on('request', request => guideRequests.push(request.url()))

  // Preflight keeps the setup, Azure, dataset and reset procedures.
  const preflight = page.locator('#preflight')
  assert.equal(await preflight.locator('h2').innerText(), 'Preflight & datasets')
  assert.equal(await preflight.locator('h2').innerText(), await page.locator('.outline a[href="#preflight"]').innerText())
  for (const id of ['tools', 'secrets', 'azure', 'start', 'datasets', 'model', 'reset', 'checklist'])
    assert.equal(await preflight.locator(`#preflight-${id}`).count(), 1, `Preflight must keep #preflight-${id}.`)
  const preflightText = await preflight.textContent()
  for (const setting of ['aspire certs trust', 'reservoir-simulation-operator-key', 'drilling-operations-internal-key',
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

  // The product truths are stated up front.
  const start = await page.locator('#start').innerText()
  for (const truth of [/synthetic demo/i, /geoscientist/, /not reserves/, /uncalibrated/, /never reaches the browser or the AI/,
    /never changes numbers or saves anything/]) assert.match(start, truth)

  // Five steps match the app's step bar, each illustrated and explained.
  assert.equal(await page.locator('main > .chapter.step').count(), 5)
  assert.deepEqual(await page.locator('main > .chapter.step h2').allTextContents(), appSteps)
  for (const step of steps) {
    const section = page.locator(`#step-${step}.step`)
    assert.equal(await section.count(), 1, `Missing step ${step}.`)
    assert.equal(await page.locator(`.outline a[href="#step-${step}"]`).count(), 1)
    assert.ok(await section.locator('figure.shot img').count() >= 1, `${step} must reference its screenshot.`)
    assert.ok(await section.locator('figure.shot img').evaluateAll(images => images.every(image => image.alt.trim().length > 20)))
    assert.ok((await section.locator('figure.shot figcaption').allTextContents()).every(caption => caption.trim().length > 30))
    assert.deepEqual(await section.locator('.facts > dt').allTextContents(), ['Shows', 'Do', 'Say', 'Look for'])
  }
  const forecast = await page.locator('#step-forecast').innerText()
  for (const action of ['Draft forecast with AI', 'Fill from reservoir model', 'Lock forecast']) assert.ok(forecast.includes(action))
  assert.match(await page.locator('#step-prospects').innerText(), /AI second opinion/)
  assert.match(await page.locator('#step-drill').innerText(), /Drill this well/)
  assert.match(await page.locator('#step-results').innerText(), /surface m³/)
  const settings = await page.locator('#settings').innerText()
  for (const setting of ['Minimum porosity', 'Minimum permeability', 'Target hydrocarbon', 'Wells used per target', 'Keep-away distance',
    'Grid points per side', 'Simulator model detail', 'Approve completion automatically', 'Your name or initials',
    'Hide automated test runs', 'Tour']) assert.ok(settings.includes(setting), `Settings must describe ${setting}.`)

  // The demo script and recovery advice.
  const script = page.locator('#demo-script')
  const times = (await script.locator('table tbody tr td:first-child').allTextContents()).map(text => text.trim())
  assert.ok(times.length >= 8 && times[0] === '0:00', 'The demo script needs a timed sequence.')
  const minutes = times.map(text => { const [m, s] = text.split(':').map(Number); return m + s / 60 })
  assert.ok(minutes.every((value, index) => index === 0 || value > minutes[index - 1]) && minutes.at(-1) < 10)
  const scriptText = await script.innerText()
  for (const phrase of ['Explain target #1 and its risks', 'Draft forecast with AI', 'Lock forecast', 'Drill this well',
    'Fill from reservoir model', 'Cancel', '429']) assert.ok(scriptText.includes(phrase), `The demo script must mention ${phrase}.`)
  const trouble = await page.locator('#troubleshooting').innerText()
  assert.match(trouble, /429/)
  assert.match(trouble, /outside the simulator/)
  assert.equal(await page.locator('#troubleshooting a[href="#preflight-reset"]').count(), 1)
  const architecture = await page.locator('#architecture').innerText()
  for (const route of ['/api/prospect-briefs', '/api/forecast-drafts', '/api/formation-interpretation', '/agui'])
    assert.ok(architecture.includes(route), `Architecture must list ${route}.`)
  assert.match(architecture, /current UI does not call/)

  const mainText = await page.locator('main').innerText()
  for (const pattern of removedFeatures) assert.doesNotMatch(mainText, pattern, `Removed feature still described: ${pattern}`)

  // Screenshots: exact new paths, local only. Missing captures are reported, not failed.
  const shots = await page.locator('main img').evaluateAll(images => images.map(image => image.getAttribute('src')))
  assert.deepEqual([...new Set(shots)].sort(), expectedScreenshots.map(name => `screenshots/${name}`).sort())
  const missingScreenshots = []
  for (const name of expectedScreenshots) {
    const url = new URL(`screenshots/${name}`, guideUrl).href
    assert.equal(new URL(url).origin, new URL(guideUrl).origin)
    const image = await context.request.get(url)
    const bytes = await image.body()
    const png = image.ok() && /^image\/png/.test(image.headers()['content-type'] ?? '') &&
      bytes.subarray(0, 8).toString('hex') === '89504e470d0a1a0a'
    if (!png) missingScreenshots.push(name)
  }

  const broken = await page.locator('a[href^="#"]').evaluateAll(links =>
    links.map(link => link.getAttribute('href')).filter(href => href.length > 1 && !document.getElementById(decodeURIComponent(href.slice(1)))))
  assert.deepEqual(broken, [])

  // Readable measure (about 65-75 characters per prose line) and no text under 12px.
  const measures = await page.locator('main p').evaluateAll(paragraphs => paragraphs
    .filter(p => !p.closest('table') && p.textContent.trim().length > 180)
    .map(p => {
      const probe = document.createElement('span')
      probe.style.cssText = 'position:absolute;visibility:hidden;white-space:nowrap'
      probe.textContent = p.textContent.replace(/\s+/g, ' ')
      p.appendChild(probe)
      const perChar = probe.getBoundingClientRect().width / probe.textContent.length
      probe.remove()
      return Math.round(p.clientWidth / perChar)
    }))
  assert.ok(measures.length >= 3, 'Expected several long prose paragraphs to measure.')
  assert.ok(measures.every(chars => chars >= 60 && chars <= 78), `Prose line length out of range: ${measures.join(', ')}`)
  const tooSmall = await page.evaluate(() => [...document.querySelectorAll('body *')]
    .filter(element => [...element.childNodes].some(node => node.nodeType === Node.TEXT_NODE && node.textContent.trim()))
    .filter(element => element.getClientRects().length && parseFloat(getComputedStyle(element).fontSize) < 12)
    .map(element => `${element.tagName}:${element.textContent.trim().slice(0, 30)}`))
  assert.deepEqual(tooSmall, [])

  const theme = page.locator('#theme-button')
  const initialTheme = await page.locator('html').getAttribute('data-theme')
  await theme.click()
  assert.notEqual(await page.locator('html').getAttribute('data-theme'), initialTheme)
  await page.setViewportSize({ width: 390, height: 844 })
  assert.ok(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth))
  await page.emulateMedia({ media: 'print' })
  assert.equal(await page.locator('main > .chapter:visible').count(), await page.locator('main > .chapter').count())
  assert.equal(await page.locator('.topbar').isVisible(), false)
  assert.equal(await page.locator('.outline').isVisible(), false)
  await page.emulateMedia({ media: 'screen' })
  assert.ok(guideRequests.every(url => new URL(url).origin === new URL(guideUrl).origin &&
    /\/screenshots\/[^/]+\.png$/.test(new URL(url).pathname)),
  'The guide may load local screenshots but must not call APIs or external services.')
  assert.deepEqual(errors, [])

  const noJs = await browser.newContext({ javaScriptEnabled: false })
  const staticPage = await noJs.newPage()
  await staticPage.goto(guideUrl)
  assert.equal(await staticPage.locator('main > .chapter:visible').count(), await staticPage.locator('main > .chapter').count())
  assert.equal(await staticPage.locator('main > .chapter.step:visible').count(), 5)
  assert.equal(await staticPage.locator('#preflight-load-troll').isVisible(), true)
  assert.equal(await staticPage.locator('#theme-button').isVisible(), false)
  await noJs.close()

  console.log(JSON.stringify({ status: 'passed', localGuideLink: true, steps: 5, stepsMatchApp: true, preflightAndDatasetSetup: true,
    demoScript: times.length, troubleshooting: true, removedFeaturesAbsent: true, proseLineLength: measures,
    minimumTextPx: 12, lightDarkMobilePrint: true, noJavaScriptReadable: true,
    screenshotPaths: expectedScreenshots.length, missingScreenshots, screenshotRequestsOnly: true }))
} finally { await browser.close() }
