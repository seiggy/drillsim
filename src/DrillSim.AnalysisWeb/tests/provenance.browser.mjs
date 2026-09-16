import assert from 'node:assert/strict'
import fs from 'node:fs/promises'
import path from 'node:path'
import { pathToFileURL } from 'node:url'

if (!process.env.DRILLSIM_PLAYWRIGHT_MODULE) throw new Error('Set DRILLSIM_PLAYWRIGHT_MODULE to an existing playwright/index.mjs.')
const { chromium } = await import(pathToFileURL(process.env.DRILLSIM_PLAYWRIGHT_MODULE).href)
const browser = await chromium.launch({ channel: 'msedge', headless: true })
let diagnostics = async () => ({})
try {
  const page = await browser.newPage({ viewport: { width: 1440, height: 1000 }, acceptDownloads: true })
  const errors = [], writes = []
  diagnostics = async () => ({ errors, alerts: await page.getByRole('alert').allTextContents(),
    selectors: await page.locator('.top-panel').innerText() })
  page.on('pageerror', error => errors.push(error.message))
  page.on('request', request => { if (['POST', 'PUT', 'PATCH', 'DELETE'].includes(request.method())) writes.push(request.url()) })
  const base = process.env.DRILLSIM_TEST_URL ?? 'http://localhost:5173'
  await page.goto(base)
  await page.locator('.sequencer').waitFor({ timeout: 60000 })
  await page.locator('[data-tutorial="task-evidence"]').click()
  const sources = page.locator('.data-sources')
  await sources.getByText('Data sources', { exact: true }).click()
  assert.equal(await sources.locator('pre,textarea').count(), 0)
  assert.equal(await sources.locator('.provenance-group').count(), 2)
  assert.equal(await sources.locator('.provenance-source').count(), 4)
  assert.ok((await sources.boundingBox()).height < 1800, 'Opening sources should not expand to the original 18,000px JSON dump.')
  assert.match(await sources.innerText(), /5 m FORCE visualization resample/)
  assert.match(await sources.innerText(), /SODIR interval only/)
  await sources.getByText('Affected wellbores (9 records)', { exact: true }).click()
  const bore = sources.locator('tbody tr').first().getByRole('button').first()
  const name = await bore.innerText()
  await bore.click()
  assert.ok(name)
  const downloadPromise = page.waitForEvent('download')
  await sources.getByRole('button', { name: 'Download source metadata JSON', exact: true }).click()
  const downloaded = JSON.parse(await fs.readFile(await (await downloadPromise).path(), 'utf8'))
  const response = await page.request.get(`${base}/analysis-api/api/fields/${downloaded.fieldId}/package`)
  assert.equal(response.status(), 200)
  const pkg = await response.json()
  assert.equal(downloaded.packageSha256, pkg.sha256)
  assert.deepEqual(downloaded.records, pkg.geologicalProperties.map(geology => ({
    geologyId: (geology.MetaInfo ?? geology.metaInfo).ID ?? (geology.MetaInfo ?? geology.metaInfo).id,
    wellBoreId: geology.WellBoreID ?? geology.wellBoreID ?? null,
    provenance: (geology.Petrophysics ?? geology.petrophysics)?.Provenance ?? (geology.Petrophysics ?? geology.petrophysics)?.provenance ?? null,
  })))
  await sources.getByText('Affected wellbores (9 records)', { exact: true }).click()
  if (process.env.DRILLSIM_TEST_SCREENSHOTS) await sources.screenshot({ path: path.join(process.env.DRILLSIM_TEST_SCREENSHOTS, 'data-sources-desktop.png') })
  await page.locator('[data-tutorial="task-qc"]').click()
  const details = page.locator('#task-workspace details').first()
  await details.getByText('Curve details', { exact: true }).click()
  assert.match(await page.locator('#task-workspace').innerText(), /Measured depth \(MD\)/)
  assert.match(await page.locator('#task-workspace').innerText(), /Datum: RKB/)
  assert.equal(await page.locator('#task-workspace pre').count(), 0)
  await page.getByRole('button', { name: 'Guided tutorial', exact: true }).click()
  const curveLayouts = []
  for (const viewport of [{ width: 1440, height: 1000 }, { width: 390, height: 844 }]) {
    await page.setViewportSize(viewport)
    const layout = await details.evaluate(element => {
      const cell = element.closest('td')
      const grid = element.querySelector('.metadata-readout')
      return {
        cellWidth: cell.getBoundingClientRect().width,
        cellHeight: cell.getBoundingClientRect().height,
        valueWidths: [...grid.querySelectorAll('dd')].map(value => value.getBoundingClientRect().width),
        overflow: grid.scrollWidth > grid.clientWidth + 1,
        pageOverflow: document.documentElement.scrollWidth > innerWidth,
      }
    })
    assert.ok(layout.valueWidths.every(width => width >= 70), `Curve values collapsed in a ${layout.cellWidth}px cell.`)
    assert.ok(layout.cellHeight < 700, `Curve row expanded to ${layout.cellHeight}px.`)
    assert.equal(layout.overflow, false)
    assert.equal(layout.pageOverflow, false)
    curveLayouts.push({ viewport: viewport.width, height: layout.cellHeight })
  }
  await page.getByRole('button', { name: 'Close guided tutorial', exact: true }).click()
  await page.setViewportSize({ width: 1440, height: 1000 })
  await page.locator('[data-tutorial="task-position"]').click()
  await page.getByText('Field coordinate reference', { exact: true }).click()
  assert.match(await page.locator('#task-workspace').innerText(), /6740415/)
  assert.equal(await page.locator('#task-workspace pre').count(), 0)
  await page.locator('[data-tutorial="task-evidence"]').click()
  await page.locator('.data-sources > summary').click()
  await page.setViewportSize({ width: 390, height: 844 })
  assert.ok(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth))
  assert.equal(await page.locator('.data-sources a').evaluateAll(links => links.every(link => ['http:', 'https:'].includes(new URL(link.href).protocol))), true)
  if (process.env.DRILLSIM_TEST_SCREENSHOTS) await page.screenshot({ path: path.join(process.env.DRILLSIM_TEST_SCREENSHOTS, 'data-sources-mobile.png') })
  const scenarioId = process.env.DRILLSIM_TEST_SCENARIO
  if (scenarioId) {
    await page.setViewportSize({ width: 1440, height: 1000 })
    await page.getByRole('combobox', { name: 'SCENARIO', exact: true }).click()
    await page.getByRole('listbox', { name: 'Scenario options', exact: true }).locator(`[data-value="${scenarioId}"]`).click()
    await page.locator('.sequencer').waitFor({ timeout: 60000 })
    await page.locator('[data-tutorial="task-prediction"]').click()
    await page.locator('.prediction-pane').waitFor({ timeout: 60000 })
    assert.equal(await page.locator('#prediction-json').count(), 0, 'A sealed prediction is presented through its readable ledger.')
    assert.equal(await page.locator('#task-workspace pre').count(), 0)
    const savedResponse = await page.request.get(`${base}/analysis-api/api/scenarios/${scenarioId}/prediction`)
    const saved = await savedResponse.json()
    const predictionDownload = page.waitForEvent('download')
    await page.getByRole('button', { name: 'Download immutable saved JSON', exact: true }).click()
    assert.deepEqual(JSON.parse(await fs.readFile(await (await predictionDownload).path(), 'utf8')), saved.body)
    assert.deepEqual(await (await page.request.get(`${base}/analysis-api/api/scenarios/${scenarioId}/prediction`)).json(), saved)
  }
  assert.deepEqual(writes, [])
  assert.deepEqual(errors, [])
  console.log(JSON.stringify({ status: 'passed', groups: 2, sharedSources: 4, compactOverview: true,
    noReadOnlyJson: true, originalMetadataDownloadMatches: true, sealedPredictionCheck: scenarioId ? 'passed' : 'not requested',
    qcAndCoordinatesReadable: true, curveLayouts, mobileOverflow: false, writes: 0 }))
} catch (error) { console.error(await diagnostics()); throw error }
finally { await browser.close() }
