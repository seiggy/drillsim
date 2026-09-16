import assert from 'node:assert/strict'
import { fileURLToPath } from 'node:url'
import { createElement } from 'react'
import { renderToStaticMarkup } from 'react-dom/server'
import { createServer } from 'vite'

const server = await createServer({
  root: fileURLToPath(new URL('..', import.meta.url)),
  server: { middlewareMode: true }, appType: 'custom',
})
const originalFetch = globalThis.fetch
try {
  const { tutorialChapters, tutorialStepSatisfied, tutorialResumeIndex, tutorialSourceField, tutorialScenario } = await server.ssrLoadModule('/src/tutorial.ts')
  const { defaultConfiguration, configurationKey } = await server.ssrLoadModule('/src/analysisConfiguration.ts')
  const { tasks } = await server.ssrLoadModule('/src/sequencer.ts')
  const { GuidedTutorial } = await server.ssrLoadModule('/src/components/GuidedTutorial.tsx')
  const { fieldOptions, scenarioOptions } = await server.ssrLoadModule('/src/selection.ts')
  const { StatusSelect } = await server.ssrLoadModule('/src/components/StatusSelect.tsx')
  const steps = tutorialChapters.flatMap(chapter => chapter.steps)
  assert.equal(tutorialChapters.length, 4)
  assert.ok(tutorialChapters.every(chapter => chapter.steps.length >= 3 && (chapter.id === 'simulation' || chapter.steps.length <= 7)))
  assert.equal(tutorialChapters[3].title, 'Run a guided simulation')
  assert.equal(new Set(steps.map(step => step.id)).size, steps.length)
  assert.deepEqual(new Set(steps.map(step => step.task).filter(Boolean)), new Set(tasks.map(task => task.id)))
  for (const step of steps) {
    assert.ok(step.title && step.instruction && step.why && step.effect && step.target && step.targetName)
    assert.equal(typeof step.target, 'string')
  }
  const context = {
    fieldId: tutorialSourceField, packageFieldId: tutorialSourceField, reservoir: 'SOGNEFJORD FM',
    scenarioId: '', activeTask: 'search', loading: false, applying: false, stale: false, error: '',
    draft: { ...defaultConfiguration }, applied: { ...defaultConfiguration }, gridCount: 225, analysisSha256: 'a'.repeat(64),
  }
  const step = id => steps.find(item => item.id === id)
  assert.equal(tutorialStepSatisfied(step('field'), context), true)
  for (const change of [{ fieldId: 'another' }, { packageFieldId: 'clone' }, { scenarioId: tutorialScenario }, { loading: true }, { error: 'offline' }]) {
    assert.equal(tutorialStepSatisfied(step('field'), { ...context, ...change }), false)
  }
  assert.equal(tutorialStepSatisfied(step('grid-edit'), context), false)
  const edited = { ...context, draft: { ...defaultConfiguration, gridPointsPerAxis: 9 }, stale: true }
  assert.equal(tutorialStepSatisfied(step('grid-edit'), edited), true)
  assert.equal(tutorialStepSatisfied(step('grid-apply'), edited), false, 'Editing a field is not applying a result.')
  const applied = { ...edited, applied: { ...edited.draft }, gridCount: 81, stale: false }
  assert.equal(tutorialStepSatisfied(step('grid-apply'), applied), true)
  for (const change of [{ gridCount: 225 }, { applying: true }, { stale: true }, { error: '502' }, { analysisSha256: '' }, { activeTask: 'targets' }]) {
    assert.equal(tutorialStepSatisfied(step('grid-apply'), { ...applied, ...change }), false)
  }
  assert.equal(tutorialStepSatisfied(step('restore'), context), true)
  assert.equal(tutorialStepSatisfied(step('restore'), { ...context, applied: { ...defaultConfiguration, porosityCutoff: .13 } }), false)
  assert.equal(tutorialStepSatisfied(step('lens'), { ...context, activeTask: 'pay' }, false), false)
  assert.equal(tutorialStepSatisfied(step('lens'), { ...context, activeTask: 'pay' }, true), true)
  assert.equal(tutorialStepSatisfied(step('scenario'), context), false)
  assert.equal(tutorialStepSatisfied(step('scenario'), { ...context, scenarioId: tutorialScenario, scenarioStatus: 'Scored' }), false,
    'The old completed fixture is not a newly created simulation.')
  assert.equal(tutorialStepSatisfied(step('scenario'), { ...context, scenarioId: tutorialScenario, scenarioStatus: 'Failed' }), false)
  const scenario = { scenarioId: 'new-simulation', sourceFieldId: tutorialSourceField, reservoirName: 'SOGNEFJORD FM',
    initialAsOfUtc: '2026-09-16T12:00:00Z', asOfUtc: '2026-09-16T12:00:00Z', status: 'Draft' }
  const simulation = { ...context, scenario, scenarioId: scenario.scenarioId, activeTask: 'prediction',
    resources: { scenarioId: scenario.scenarioId, loading: false, error: '' } }
  assert.equal(tutorialStepSatisfied(step('scenario'), simulation), true)
  const check = (id, state) => tutorialStepSatisfied(step(id), { ...state, activeTask: step(id).task })
  const actions = ['frozen-analysis', 'prediction-target', 'demo-forecast', 'save-prediction', 'seal-prediction', 'approve-prediction',
    'prepare-simulator', 'execution', 'completion-review', 'approve-completion', 'production', 'publish', 'reveal', 'evaluate']
  for (const id of actions) assert.equal(check(id, simulation), false, `Navigation alone does not complete ${id}.`)
  const preparedAnalysis = { ...simulation, preparation: {
    scenarioId: scenario.scenarioId, sourceFieldId: scenario.sourceFieldId, asOfUtc: scenario.initialAsOfUtc,
    configurationKey: configurationKey(defaultConfiguration), analysisSha256: 'a'.repeat(64), packageSha256: 'b'.repeat(64),
    forecastReady: false,
  } }
  assert.equal(check('frozen-analysis', preparedAnalysis), true)
  assert.equal(check('frozen-analysis', { ...preparedAnalysis, stale: true }), false)
  assert.equal(check('frozen-analysis', { ...preparedAnalysis, preparation: { ...preparedAnalysis.preparation, sourceFieldId: 'results-field' } }), false)
  const targeted = { ...preparedAnalysis, preparation: { ...preparedAnalysis.preparation, candidateId: 'chosen' } }
  assert.equal(check('prediction-target', targeted), true)
  assert.equal(check('demo-forecast', targeted), false)
  const forecast = { ...targeted, preparation: { ...targeted.preparation, forecastReady: true } }
  assert.equal(check('demo-forecast', forecast), true)
  assert.equal(check('save-prediction', forecast), false)
  const prediction = { scenarioId: scenario.scenarioId, revision: 1, body: { analysisBinding: {
    scenarioId: scenario.scenarioId, fieldId: scenario.sourceFieldId, reservoirName: scenario.reservoirName, asOfUtc: scenario.initialAsOfUtc,
  } } }
  const saved = { ...simulation, resources: { ...simulation.resources, prediction } }
  assert.equal(check('save-prediction', saved), true)
  assert.equal(check('seal-prediction', saved), false)
  const sealed = { ...saved, resources: { ...saved.resources, prediction: { ...prediction, seal: { sha256: 'c'.repeat(64) } } } }
  assert.equal(check('seal-prediction', sealed), true)
  assert.equal(check('approve-prediction', sealed), false)
  const approved = { ...sealed, resources: { ...sealed.resources, prediction: { ...sealed.resources.prediction, approval: { sealedSha256: 'c'.repeat(64) } } } }
  assert.equal(check('approve-prediction', approved), true)
  assert.equal(check('approve-prediction', { ...approved, resources: { ...approved.resources,
    prediction: { ...approved.resources.prediction, approval: { sealedSha256: 'wrong' } } } }), false)
  const operator = { enabled: true, scenarioId: scenario.scenarioId, prediction: { approved: true, sealed: true, sealHash: 'c'.repeat(64) },
    preflight: { worldBound: true }, completion: { available: false } }
  const prepared = { ...approved, resources: { ...approved.resources, operator } }
  assert.equal(check('prepare-simulator', prepared), true)
  assert.equal(check('execution', prepared), false, 'A prepared model is not a started run.')
  const run = { runId: 'new-run', stages: [], status: 'Running' }
  const started = { ...prepared, resources: { ...prepared.resources, operator: { ...operator, run } } }
  assert.equal(check('execution', started), true)
  assert.equal(check('execution', { ...started, resources: { ...started.resources, loading: true } }), true,
    'A background refresh does not erase a confirmed run or flicker the tutorial navigation.')
  const review = { ...started, resources: { ...started.resources, operator: { ...started.resources.operator, completion: {
    available: true, scenarioId: scenario.scenarioId, runId: run.runId, openingsHash: 'd'.repeat(64), status: 'Draft',
  } } } }
  assert.equal(check('completion-review', review), true)
  assert.equal(check('approve-completion', review), false)
  const completed = { ...review, resources: { ...review.resources, operator: { ...review.resources.operator,
    run: { ...run, stages: [{ stage: 'S6DesignCompletion', status: 'Completed' }, { stage: 'S7RunProduction', status: 'Completed' }] } } } }
  assert.equal(check('approve-completion', completed), true)
  assert.equal(check('production', completed), true)
  assert.equal(check('publish', completed), false, 'Completed stages are not a publication receipt.')
  const published = { ...completed, resources: { ...completed.resources,
    reveal: { scenarioId: scenario.scenarioId, revealId: 'new-reveal', status: 'Revealed' } } }
  assert.equal(check('publish', published), true)
  assert.equal(check('reveal', published), false)
  const visible = { ...published, resources: { ...published.resources,
    production: { scenarioId: scenario.scenarioId, revealId: 'new-reveal', monthCount: 60 } } }
  assert.equal(check('reveal', visible), true)
  assert.equal(check('evaluate', visible), false)
  const evaluated = { ...visible, scenario: { ...scenario, status: 'Scored' }, resources: { ...visible.resources,
    scorecard: { scenarioId: scenario.scenarioId, revealId: 'new-reveal', metrics: [{ name: 'Pay error' }] } } }
  assert.equal(check('evaluate', evaluated), true)
  for (const id of actions) {
    assert.equal(check(id, { ...evaluated, resources: { ...evaluated.resources, scenarioId: 'foreign' }, preparation: undefined }), false,
      'Foreign artifacts cannot satisfy a lifecycle check.')
  }
  assert.equal(tutorialChapters[3].steps[tutorialResumeIndex(approved)].id, 'prepare-simulator')
  assert.equal(tutorialChapters[3].steps[tutorialResumeIndex(evaluated)].id, 'reveal')
  let effects = 0
  globalThis.fetch = async () => { effects++; throw new Error('The tutorial cannot call an API.') }
  const props = { open: true, context, guideUrl: '/guide.html', onClose: () => effects++, onNavigate: () => effects++ }
  const html = renderToStaticMarkup(createElement(GuidedTutorial, props))
  assert.match(html, /aria-label="Guided tutorial"/)
  assert.match(html, /Choose a chapter and follow the highlighted controls/)
  assert.match(html, /Close guided tutorial/)
  assert.match(html, /Start chapter/)
  assert.doesNotMatch(html, /aria-modal="true"/)
  assert.equal(renderToStaticMarkup(createElement(GuidedTutorial, { ...props, open: false })), '')
  assert.equal(effects, 0, 'Rendering or opening a tutorial never calls the API or navigates automatically.')
  const fields = [{ id: 'original', name: 'Troll' }, { id: 'result-one', name: 'Troll' }, { id: 'result-two', name: 'Troll' }]
  const scenarios = [
    { scenarioId: 'one', sourceFieldId: 'original', clonedFieldId: 'result-one', seedLabel: 'First simulation', status: 'Scored', createdUtc: '2026-09-03T12:00:00Z' },
    { scenarioId: 'two', sourceFieldId: 'original', clonedFieldId: 'result-two', seedLabel: 'Second simulation', status: 'Revealed', createdUtc: '2026-09-11T12:00:00Z' },
  ]
  const choices = fieldOptions(fields, scenarios)
  assert.equal(choices.length, 3, 'Distinguish records without discarding or merging them.')
  assert.deepEqual(choices.map(choice => choice.value), fields.map(field => field.id))
  assert.equal(choices[0].badge, 'Original data')
  assert.equal(choices[1].badge, 'Simulated results')
  assert.notEqual(choices[1].description, choices[2].description)
  const runs = scenarioOptions(scenarios)
  assert.equal(runs[0].value, '')
  assert.equal(runs[0].name, 'Live evidence')
  assert.equal(runs[1].badge, 'Scored')
  assert.match(runs[1].description, /Evaluation complete/)
  assert.equal(runs[1].name, 'First simulation', 'Status is separate from the scenario name.')
  const selector = renderToStaticMarkup(createElement(StatusSelect, {
    label: 'Scenario', value: 'one', options: runs, onChange: () => effects++,
  }))
  assert.match(selector, /role="combobox"/)
  assert.match(selector, /role="listbox"/)
  assert.match(selector, /aria-expanded="false"/)
  assert.match(selector, /popover="auto"/)
  assert.equal(effects, 0, 'Rendering a selector never changes the selected data.')
  console.log('Tutorial checks passed: all 19 tasks, guided lifecycle checks, exact artifact scope, distinct Save/Seal/Approve/Prepare/Start/publication, safe resume and mutation-free rendering.')
} finally {
  globalThis.fetch = originalFetch
  await server.close()
}
