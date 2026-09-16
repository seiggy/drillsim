import assert from 'node:assert/strict'
import { createHash } from 'node:crypto'

export const originalFieldId = '502fea8b-4d85-5d40-86a7-f5b7655d89d6'
export const referenceScenarioId = '8e96385c-84cb-8720-85cd-0e2073588b09'
export const newScenarioId = '11111111-1111-4111-8111-111111111111'
const referenceRunId = '4cd0fd6c-4e59-5de8-9bf7-efd7ab4d371c'
const t0 = '2026-09-16T12:00:00.000Z'
const t1 = '2026-09-17T12:00:00.000Z'
const hash = value => createHash('sha256').update(JSON.stringify(value)).digest('hex')
const configuration = {
  version: 'analysis-configuration-v1', porosityCutoff: .12, permeabilityCutoffM2: 9.869233e-16,
  wellExclusionRadiusM: 500, gridPointsPerAxis: 15, idwNeighborCount: 4,
}
const stages = ['S0BindWorld', 'S1MaterializePlan', 'S2Drill', 'S3Survey', 'S4SampleGeology',
  'S5GenerateLogs', 'S6DesignCompletion', 'S7RunProduction', 'S8PublishReveal', 'S9Score']
const wellIds = [1, 2, 3, 4].map(index => `10000000-0000-4000-8000-${String(index).padStart(12, '0')}`)
const referenceClone = 'f109d7ac-c05f-56cc-91a4-546d2bac9e8b'
const newClone = '22222222-2222-4222-8222-222222222222'

export async function installSimulationFixture(context, options = {}) {
  const state = { mutations: [], unexpected: [], createBodies: [], setupBodies: [], requests: [], created: undefined }
  let revisionClock = 0, createFailureUsed = false, setupFailureUsed = false
  const modified = () => new Date(Date.parse(t0) + ++revisionClock * 1000).toISOString()
  const session = { enabled: true, csrfRequestToken: 'browser-fixture-csrf', csrfHeaderName: 'X-DrillSim-CSRF', auditLabelLimitation: 'Test-local audit annotation.' }
  const scenarios = new Map()
  const receipts = new Map()
  const record = (id, label, status = 'Draft') => ({
    scenarioId: id, sourceFieldId: originalFieldId, reservoirName: 'SOGNEFJORD FM',
    seedLabel: label, initialAsOfUtc: t0, asOfUtc: status === 'Scored' ? t1 : t0, status,
    worldModelVersion: 'fixture-world-v1', observationModelVersion: 'fixture-observation-v1', scoringModelVersion: 'fixture-score-v1',
    assumptionsSha256: hash(label), createdUtc: t0, modifiedUtc: t0,
  })
  const reference = { scenario: { ...record(referenceScenarioId, 'AUTOMATED WORKFLOW FIXTURE 2026-09-11 9f64865b', 'Scored'), clonedFieldId: referenceClone },
    prepared: true, setup: null, run: { runId: referenceRunId, status: 'Scored', currentStage: 'S9Score', progressPercent: 100,
      stages: stages.map(stage => ({ stage, name: stage.slice(2).replace(/([a-z])([A-Z])/g, '$1 $2'), status: 'Completed', attemptCount: 1 })) },
    completionApproved: true, reads: 0 }
  scenarios.set(referenceScenarioId, reference)

  function productionWell(seriesId) {
    return {
      MetaInfo: { ID: `well-${seriesId}` }, Name: 'Mock synthetic production well',
      Dataset: { Provenance: { SourceArtifacts: [{ ID: seriesId }] }, MonthlyProduction: Array.from({ length: 60 }, (_, index) => ({
        Year: 2026 + Math.floor(index / 12), Month: index % 12 + 1,
        Oil: { Value: 100 + index, Unit: 'm3' }, Gas: { Value: 5000 + index * 50, Unit: 'm3' },
        Water: { Value: 10 + index, Unit: 'm3' }, DaysOnProduction: 20, IsAllocated: true, Classification: 'Synthetic',
      })) },
    }
  }
  function packageFor(fieldId, asOf = t0) {
    const scenarioState = [...scenarios.values()].find(value => value.scenario.clonedFieldId === fieldId)
    const wells = wellIds.map((id, index) => ({
      MetaInfo: { ID: id }, Name: `Mock control ${index + 1}`, ReferencePoint: { RiemannianEast: index * 1000, RiemannianNorth: index % 2 * 1000 },
    }))
    if (scenarioState?.reveal) wells.push(productionWell(scenarioState.production.seriesId))
    return {
      fieldId, generatedAt: asOf, sha256: hash([fieldId, asOf, wells.length]),
      field: { MetaInfo: { ID: fieldId }, Name: 'TROLL FORCE-SODIR Field', ReferencePoint: { RiemannianEast: 0, RiemannianNorth: 0 } },
      wells, clusters: [], wellBoreArchitectures: [], trajectories: [],
      wellBores: wellIds.map((id, index) => ({ MetaInfo: { ID: `bore-${index}` }, Name: `Mock bore ${index + 1}`, WellID: id })),
      geologicalProperties: wellIds.map((_, index) => ({
        MetaInfo: { ID: `geology-${index}` }, Name: `Mock geology ${index + 1}`, WellBoreID: `bore-${index}`,
        Petrophysics: { FormationIntervals: [{ FormationName: 'SOGNEFJORD FM' }], LogRuns: [] },
      })),
      sourceCounts: { fields: 1, clusters: 0, wells: wells.length, wellBores: 4, wellBoreArchitectures: 0, trajectories: 0, geologicalProperties: 4 },
      dataGaps: ['Isolated browser fixture; no real field data is accessed.'],
    }
  }
  function analysisFor(fieldId, asOf = t0, config = configuration) {
    const pkg = packageFor(fieldId, asOf), configHash = hash(config)
    const candidate = index => ({
      candidateId: `configured:${configHash}:candidate:00:0${index}`, rank: index + 1,
      eastingM: 100.125 + index * 100, northingM: -200.75, longitudeDegrees: 3.5, latitudeDegrees: 60.1,
      p90NetPayM: 10, p50NetPayM: 20, p10NetPayM: 30, sigmaNetPayM: 5, meanPayPorosity: .2,
      meanPayPermeabilityMd: 10, nearestWellDistanceM: 800, relativeUncertainty: .25, score: 5 - index,
      neighborEvidenceIds: wellIds.map(id => `well:${id}`),
    })
    const ranking = [candidate(0), candidate(1)]
    return {
      fieldId, generatedAt: asOf, reservoirName: 'SOGNEFJORD FM', packageSha256: pkg.sha256,
      modelVersion: 'petrophysics-screening-v1', configuration: config, configurationSha256: configHash,
      analysisSha256: hash([pkg.sha256, configHash]), ranking,
      candidateGridBounds: { minEastingM: 0, maxEastingM: 3000, minNorthingM: -1000, maxNorthingM: 1000 },
      candidateGrid: Array.from({ length: config.gridPointsPerAxis ** 2 }, (_, index) => index < 2
        ? { ...ranking[index], status: 'eligible', prediction: ranking[index], reasons: [] }
        : { candidateId: `excluded-${index}`, eastingM: index * 10, northingM: 0,
          longitudeDegrees: 3.5, latitudeDegrees: 60.1, status: 'excluded', reasons: ['Browser fixture exclusion'] }),
      wellSummaries: wellIds.map((id, index) => ({
        wellId: id, wellEvidenceId: `well:${id}`, geologyEvidenceIds: [`geology:geology-${index}`],
        netPayThicknessM: 20, meanPayPorosity: .2, meanPayPermeabilityM2: 1e-14, eastingM: index * 1000, northingM: index % 2 * 1000,
      })), dataGaps: [],
    }
  }
  function publish(item) {
    const id = item.scenario.scenarioId, revealId = `reveal-${id}`, seriesId = `series-${id}`
    item.scenario = { ...item.scenario, status: 'Scored', asOfUtc: t1,
      clonedFieldId: id === referenceScenarioId ? referenceClone : newClone, modifiedUtc: modified() }
    item.run = { ...item.run, status: 'Scored', currentStage: 'S9Score', progressPercent: 100,
      stages: stages.map(stage => ({ stage, name: stage.slice(2), status: 'Completed', attemptCount: 1 })) }
    item.reveal = { scenarioId: id, revealId, clonedFieldId: item.scenario.clonedFieldId, asOfUtc: t1,
      status: 'Revealed', manifestSha256: hash(id), evidenceCount: 12, productionSeriesId: seriesId }
    item.production = { scenarioId: id, revealId, seriesId, modelVersion: 'fixture-meter-v1', contentSha256: hash(seriesId),
      monthCount: 60, checkpointYears: [1, 3, 5], createdUtc: t1 }
    item.scorecard = { scenarioId: id, scorecardId: `score-${id}`, revealId, scoringModelVersion: 'fixture-score-v1',
      inputSha256: hash(id), contentSha256: hash(['score', id]), createdValidTimeUtc: t1, headlineMetric: 'ExpectedPayError',
      metrics: [{ name: 'ExpectedPayError', basis: 'RevealedObservation', status: 'Scored', value: 2, lowerBound: 1, upperBound: 3, unit: 'm' }],
      limitation: 'Mock aggregate only; no hidden truth is supplied.' }
  }
  const referenceAnalysis = analysisFor(originalFieldId)
  const target = referenceAnalysis.ranking[0], quantiles = { p90: 10, p50: 20, p10: 30 }
  reference.prediction = {
    scenarioId: referenceScenarioId, revision: 1, createdUtc: t0, modifiedUtc: t0, baselines: [],
    seal: { sha256: hash('reference-seal'), baselinesSha256: hash([]), sealedUtc: t0 },
    approval: { actor: 'Historical demo', approvedUtc: t0, sealedSha256: hash('reference-seal') },
    body: { candidateId: target.candidateId, fieldPackageSha256: referenceAnalysis.packageSha256,
      proposedWellPath: [0, 2200].map(depth => ({ measuredDepthM: depth, trueVerticalDepthM: depth, eastingM: target.eastingM, northingM: target.northingM })),
      formations: [{ formationName: 'SOGNEFJORD FM', topTrueVerticalDepthM: { p90: 1400, p50: 1450, p10: 1500 }, baseTrueVerticalDepthM: { p90: 1600, p50: 1650, p10: 1700 } }],
      expectedPaydirtM: quantiles, fluidClasses: ['Oil'], contactPredictions: [],
      productionForecasts: [1, 3, 5].map(year => ({ year, oilM3: year * 1000, gasM3: 0, waterM3: year * 10 })),
      uncertaintyAssumptions: ['Historical browser fixture.'], citedEvidenceIds: target.neighborEvidenceIds,
      rationale: 'Preserved historical forecast.',
    },
  }
  publish(reference)
  const referenceBefore = JSON.stringify(reference)
  const capabilities = {
    version: 'prediction-handoff-capabilities-v1', configuredSaveSupported: true, configuredSealSupported: true,
    bindingVersion: 'prediction-analysis-binding-v1', analysisModelVersion: 'petrophysics-screening-v1',
    configurationVersion: 'analysis-configuration-v1', baselineBindingVersion: 'baseline-analysis-binding-v1',
    baselineModelVersions: ['baseline-nearest-well-v3', 'baseline-field-mean-v3', 'baseline-four-neighbor-idw-v3', 'baseline-uncertainty-aware-rank1-v3'],
    sourceScope: 'Frozen initial source only.', targetRule: 'Every station keeps exact easting and northing.',
    minimumLocatedControls: 4, fourNeighborBaselineRule: 'Four neighbors independently of the ranking setting.',
    limitation: 'Point-screening demo.', legacyNullBindingSupported: true,
  }
  function tick(item) {
    if (item.run?.status !== 'Running') return
    item.reads++
    if (item.reads < 2) return
    if (item.completionApproved) {
      item.scenario.status = 'ReadyToReveal'
      item.run.currentStage = 'S8PublishReveal'
      item.run.status = 'AwaitingDependency'
      item.run.progressPercent = 80
      item.run.stages = stages.map((stage, index) => ({ stage, name: stage.slice(2), status: index <= 7 ? 'Completed' : 'Pending', attemptCount: 1 }))
    } else {
      item.scenario.status = 'CompletionDesigned'
      item.run.currentStage = 'S6DesignCompletion'
      item.run.status = 'AwaitingApproval'
      item.run.progressPercent = 60
      item.run.stages = stages.map((stage, index) => ({ stage, name: stage.slice(2), status: index < 6 ? 'Completed' : 'Pending', attemptCount: 1 }))
    }
    item.scenario.modifiedUtc = modified()
  }
  function operatorView(item) {
    tick(item)
    const scenario = item.scenario, sealed = Boolean(item.prediction?.seal), approved = Boolean(item.prediction?.approval)
    const available = enabled => ({ enabled: Boolean(enabled), reason: enabled ? null : 'Complete the preceding workflow step.' })
    const completionAvailable = Boolean(item.run && (item.run.currentStage === 'S6DesignCompletion' || item.completionApproved))
    return {
      enabled: true, reason: null, scenarioId: scenario.scenarioId, scenarioStatus: scenario.status,
      prediction: { sealed, approved, sealHash: item.prediction?.seal?.sha256 ?? null },
      preflight: { ready: approved && item.prepared && !item.run, worldBound: Boolean(item.prepared),
        reason: scenario.status !== 'HumanApproved' ? 'Scenario is not in HumanApproved state' : item.prepared ? null : 'Prepare simulator first.' },
      run: item.run ?? null,
      completion: { available: completionAvailable, approvalEnabled: completionAvailable && !item.completionApproved,
        reason: 'Review the visible completion openings.', scenarioId: scenario.scenarioId, runId: item.run?.runId ?? null,
        status: item.completionApproved ? 'Approved' : 'Draft', openingsHash: completionAvailable ? hash(['openings', scenario.scenarioId]) : null,
        openings: completionAvailable ? [{ reservoirName: scenario.reservoirName, type: 'Perforated', topMdM: 1450, baseMdM: 1500,
          wellboreRadiusM: .1016, skin: 0, efficiency: .8, uncertaintyM: 2 }] : [] },
      actions: {
        approvePrediction: available(sealed && !approved && !item.run), start: available(approved && item.prepared && !item.run),
        cancel: available(item.run?.status === 'Running'), resume: available(false),
        approveCompletion: available(item.run?.status === 'AwaitingApproval' && !item.completionApproved),
        publish: available(item.scenario.status === 'ReadyToReveal'), score: available(item.reveal && !item.scorecard),
      },
    }
  }
  function setupView(item) {
    return {
      scenarioId: item.scenario.scenarioId, available: Boolean(item.prediction?.approval && !item.prepared && !item.run),
      reason: item.prediction?.approval ? null : 'Approve the sealed prediction first.',
      prepared: Boolean(item.prepared), current: item.setup,
      profiles: [{ profileId: 'troll-demo', name: 'Troll demonstration model', description: 'Curated synthetic example preset.', worldModelVersion: 'fixture-world-v1' }],
      defaults: { profileId: 'troll-demo', resolution: 'Preview', realizationSeed: 1234 },
      reviewedSealHash: item.prediction?.seal?.sha256 ?? null,
    }
  }
  await context.route('**/analysis-api/**', async route => {
    const request = route.request(), url = new URL(request.url()), method = request.method()
    const pathname = url.pathname.replace('/analysis-api', '')
    const respond = (value, status = 200) => route.fulfill({ status, contentType: 'application/json', body: JSON.stringify(value) })
    const missing = () => respond({ title: 'Not found', detail: 'No fixture artifact exists yet.' }, 404)
    state.requests.push({ method, pathname })
    try {
      if (method !== 'GET') state.mutations.push({ method, pathname, body: request.postDataJSON(), headers: request.headers() })
      if (pathname === '/api/fields' && method === 'GET') return respond([
        { MetaInfo: { ID: originalFieldId }, Name: 'TROLL FORCE-SODIR Field', Description: 'Original fixture measurements' },
        { MetaInfo: { ID: referenceClone }, Name: 'TROLL FORCE-SODIR Field', Description: 'Historical fixture results' },
        { MetaInfo: { ID: 'earlier-clone' }, Name: 'TROLL FORCE-SODIR Field', Description: 'Earlier fixture results' },
      ])
      if (pathname === '/api/scenarios' && method === 'GET') return respond([
        ...[...scenarios.values()].map(item => item.scenario),
        { ...record('earlier-scenario', 'Earlier fixture', 'Scored'), clonedFieldId: 'earlier-clone' },
      ])
      if (pathname === '/api/agent/status' || pathname === '/api/maps/status') return respond({ configured: false })
      if (pathname === '/api/formation-interpretation/status') return respond({ configured: false, reason: 'AI is disabled in this isolated browser fixture.' })
      if (pathname.startsWith('/api/hypotheses') && method === 'GET') return respond([])
      if (pathname === '/api/operator/session' && method === 'GET') return respond(session)
      if (pathname === '/api/analysis/prediction-capabilities' && method === 'GET') return respond(capabilities)
      const field = pathname.match(/^\/api\/fields\/([^/]+)\/(package|analysis)$/)
      if (field) {
        const body = method === 'POST' ? request.postDataJSON() : undefined
        const asOf = body?.asOf ?? url.searchParams.get('asOf') ?? t0
        return respond(field[2] === 'package' ? packageFor(field[1], asOf) : analysisFor(field[1], asOf, body?.configuration ?? configuration))
      }
      if (pathname === '/api/scenarios' && method === 'POST') {
        const payload = request.postDataJSON()
        state.createBodies.push(request.postData())
        assert.equal(payload.sourceFieldId, originalFieldId, 'A new scenario must use the original field.')
        if (!state.created) {
          const scenario = { ...record(newScenarioId, payload.seedLabel), ...payload,
            scenarioId: newScenarioId, initialAsOfUtc: payload.asOfUtc, modifiedUtc: modified() }
          state.created = { scenario, prepared: false, setup: null, completionApproved: false, reads: 0 }
          scenarios.set(newScenarioId, state.created)
        } else assert.deepEqual(payload, JSON.parse(state.createBodies[0]), 'Retry payload cannot drift.')
        if (options.failFirstCreate && !createFailureUsed) { createFailureUsed = true; return route.abort('failed') }
        return respond(state.created.scenario)
      }
      const match = pathname.match(/^\/api\/(operator\/)?scenarios\/([^/]+)(.*)$/)
      if (match) {
        const isOperator = Boolean(match[1]), id = match[2], tail = match[3], item = scenarios.get(id)
        if (!item) return missing()
        if (method === 'GET') {
          if (!tail) return respond(isOperator ? operatorView(item) : item.scenario)
          if (isOperator && tail === '/setup') return options.setupUnavailable ? missing() : respond(setupView(item))
          if (tail === '/evidence-visibility') return respond({
            scenarioId: id, asOfUtc: url.searchParams.get('asOf'), counts: [
              { recordKind: 'Well', status: 'Visible', count: url.searchParams.get('asOf') === item.scenario.initialAsOfUtc ? 4 : 5 },
              { recordKind: 'Well', status: 'Hidden', count: 0 },
            ],
          })
          const resource = { '/prediction': 'prediction', '/reveal': 'reveal', '/production': 'production', '/scorecard': 'scorecard' }[tail]
          if (resource) return item[resource] ? respond(item[resource]) : missing()
        } else {
          assert.notEqual(id, referenceScenarioId, 'The completed reference scenario must never receive a mutation.')
          const body = tail.endsWith('/seal') ? undefined : request.postDataJSON()
          if (isOperator || tail.endsWith('/seal')) {
            assert.equal(request.headers()['x-drillsim-csrf'], session.csrfRequestToken)
            assert.ok(request.headers()['idempotency-key'])
          }
          const actionKey = `${pathname}:${request.headers()['idempotency-key']}`
          if (isOperator && tail === '/setup') state.setupBodies.push({ body: request.postData(), key: request.headers()['idempotency-key'] })
          if (receipts.has(actionKey)) return respond(receipts.get(actionKey))
          if (!isOperator && tail === '/prediction' && method === 'PUT') {
            assert.equal(request.headers()['if-match'], item.prediction ? `"${item.prediction.revision}"` : undefined)
            const target = analysisFor(originalFieldId, item.scenario.initialAsOfUtc, body.analysisBinding.configuration).ranking.find(candidate => candidate.candidateId === body.candidateId)
            assert.ok(target)
            assert.ok(body.proposedWellPath.every(station => station.eastingM === target.eastingM && station.northingM === target.northingM))
            assert.deepEqual(body.expectedPaydirtM, { p90: target.p90NetPayM, p50: target.p50NetPayM, p10: target.p10NetPayM })
            item.prediction = { scenarioId: id, body, revision: (item.prediction?.revision ?? 0) + 1, createdUtc: t0, modifiedUtc: modified(), baselines: [] }
            item.scenario.status = 'PredictionDrafted'; item.scenario.modifiedUtc = item.prediction.modifiedUtc
            return respond(item.prediction)
          }
          if (!isOperator && tail === '/prediction/seal') {
            assert.equal(request.headers()['if-match'], `"${item.prediction.revision}"`)
            item.prediction.seal = { sha256: hash(item.prediction.body), baselinesSha256: hash([]), sealedUtc: modified() }
            item.scenario.status = 'PredictionSealed'; item.scenario.modifiedUtc = modified()
            receipts.set(actionKey, structuredClone(item.prediction))
            return respond(item.prediction)
          }
          let outcome
          if (isOperator && tail === '/approve-prediction') {
            assert.equal(body.reviewedSealHash, item.prediction.seal.sha256)
            item.prediction.approval = { actor: body.actor, sealedSha256: body.reviewedSealHash, approvedUtc: modified() }
            item.scenario.status = 'HumanApproved'; outcome = 'approved'
          } else if (isOperator && tail === '/setup') {
            assert.ok(item.prediction.approval && !item.run && !item.prepared)
            assert.equal(body.profileId, 'troll-demo')
            assert.ok(['Preview', 'Standard'].includes(body.resolution))
            assert.ok(Number.isInteger(body.realizationSeed) && body.realizationSeed >= 0 && body.realizationSeed <= 2147483647)
            assert.match(body.actor, /^[\x20-\x7e]{1,100}$/)
            assert.equal(body.reviewedSealHash, item.prediction.seal.sha256)
            item.prepared = true
            item.setup = { profileId: body.profileId, profileName: 'Troll demonstration model', resolution: body.resolution,
              realizationSeed: body.realizationSeed, preparedBy: body.actor, preparedUtc: modified() }
            outcome = 'simulation-prepared'
          } else if (isOperator && tail === '/runs') {
            assert.ok(item.prediction.approval && item.prepared && !item.run)
            item.run = { runId: '33333333-3333-4333-8333-333333333333', status: 'Running', currentStage: 'S2Drill', progressPercent: 20,
              stages: stages.map((stage, index) => ({ stage, name: stage.slice(2), status: index < 2 ? 'Completed' : 'Pending', attemptCount: 1 })) }
            item.scenario.status = 'Drilling'; item.reads = 0; outcome = 'started'
          } else if (isOperator && tail.endsWith('/approve-completion')) {
            assert.equal(item.run.status, 'AwaitingApproval')
            assert.equal(body.reviewedOpeningHash, hash(['openings', id]))
            item.completionApproved = true; item.run.status = 'Running'; item.run.currentStage = 'S7RunProduction'; item.reads = 0
            item.scenario.status = 'Producing'; outcome = 'completion-approved'
          } else if (isOperator && tail.endsWith('/publish')) {
            assert.equal(item.scenario.status, 'ReadyToReveal')
            publish(item); outcome = 'scored'
          }
          if (outcome) {
            item.scenario.modifiedUtc = modified()
            const result = { scenarioId: id, runId: item.run?.runId ?? null, outcome, revealSucceeded: outcome === 'scored', scoringStatus: outcome === 'scored' ? 'scored' : null }
            receipts.set(actionKey, result)
            if (outcome === 'simulation-prepared' && options.failFirstSetup && !setupFailureUsed) {
              setupFailureUsed = true; return route.abort('failed')
            }
            return respond(result)
          }
        }
      }
      state.unexpected.push(`${method} ${pathname}`)
      return respond({ title: 'Unexpected fixture request', detail: `${method} ${pathname}` }, 500)
    } catch (error) {
      state.unexpected.push(`${method} ${pathname}: ${error.message}`)
      return respond({ title: 'Fixture assertion failed', detail: error.message }, 500)
    }
  })
  return { ...state, state, packageFor, analysisFor, referenceUnchanged: () => JSON.stringify(reference) === referenceBefore }
}
