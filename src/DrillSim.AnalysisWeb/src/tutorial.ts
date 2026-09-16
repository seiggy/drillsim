import { configurationKey, defaultConfiguration } from './analysisConfiguration'
import type { AnalysisConfiguration, Scenario, ScenarioResources } from './types'
import type { TaskId } from './sequencer'
import type { PredictionPreparationState } from './predictionBinding'

export const tutorialSourceField = '502fea8b-4d85-5d40-86a7-f5b7655d89d6'
export const tutorialScenario = '8e96385c-84cb-8720-85cd-0e2073588b09'

type TutorialCheck = 'source' | 'grid-draft' | 'grid-applied' | 'defaults-applied' | 'section-lens' |
  'scenario-created' | 'frozen-analysis' | 'eligible-target' | 'forecast-ready' | 'prediction-saved' | 'prediction-sealed' |
  'prediction-approved' | 'model-prepared' | 'run-started' | 'completion-review' | 'completion-approved' |
  'production-complete' | 'published' | 'production-visible' | 'scorecard'
export interface TutorialStep {
  id: string
  title: string
  target: string
  targetName: string
  task?: TaskId
  instruction: string
  why: string
  effect: string
  check?: TutorialCheck
  missing?: string
}
export interface TutorialChapter {
  id: string
  title: string
  duration: string
  description: string
  steps: TutorialStep[]
}
export interface TutorialContext {
  fieldId: string
  packageFieldId?: string
  reservoir: string
  scenarioId: string
  scenarioStatus?: string
  activeTask: TaskId
  loading: boolean
  applying: boolean
  error: string
  stale: boolean
  draft: AnalysisConfiguration
  applied?: AnalysisConfiguration
  analysisSha256?: string
  gridCount?: number
  scenario?: Scenario
  resources?: ScenarioResources
  preparation?: PredictionPreparationState
  creatingScenario?: boolean
}

export const tutorialChapters: TutorialChapter[] = [
  {
    id: 'evidence', title: 'Read a field', duration: '5–7 minutes',
    description: 'Find the wells, check data quality and inspect drilling targets.',
    steps: [
      {
        id: 'field', title: 'Select the Troll field', target: '[data-tutorial="scope"]', targetName: 'Field, reservoir and scenario selectors',
        instruction: 'Choose TROLL FORCE-SODIR Field with the “Original data” badge, then SOGNEFJORD FM. Leave SCENARIO on “Live evidence”.',
        why: 'This dataset has 29 wells and 46 wellbores. Nine wellbores have logs and surveys.',
        effect: 'The app loads the selected field’s wells, geology and analysis.',
        check: 'source', missing: 'The seeded Troll field may not be installed. Use another field with usable controls or skip this exercise.',
      },
      {
        id: 'inventory', title: 'Inspect the available data', target: '[data-tutorial="task-workspace"]', targetName: 'Evidence inventory', task: 'evidence',
        instruction: 'Check the well counts and missing values. Expand “Data sources” to see the datasets, processing details, affected wells and source licenses.',
        why: 'Coverage varies between wells. Start by checking which wells have usable measurements.',
        effect: '“Refresh visible evidence” reloads the data from the services.',
      },
      {
        id: 'qc', title: 'Check units and missing values', target: '[data-tutorial="task-workspace"]', targetName: 'Units and missingness table', task: 'qc',
        instruction: 'Inspect a curve’s units, depth reference and missing-value count.',
        why: 'MD is distance along the wellbore. TVD is vertical depth. Check these references before comparing measurements.',
        effect: 'This screen reports data quality. Stored measurements stay unchanged.',
      },
      {
        id: 'position', title: 'Locate a wellbore', target: '[data-tutorial="task-workspace"]', targetName: 'Position diagnostics', task: 'position',
        instruction: 'Select a wellbore row. Check its survey stations and formation-midpoint coordinates.',
        why: 'Survey stations locate the path underground. A well’s surface location alone cannot provide that path.',
        effect: 'The selected wellbore is shown in the map, section and log views.',
      },
      {
        id: 'pay', title: 'Inspect reservoir intervals', target: '[data-tutorial="task-workspace"]', targetName: 'Per-bore interval contributions', task: 'pay',
        instruction: 'Select a wellbore and read its interval table. Green meets the rock cutoffs, red is below cutoff, and hatching marks missing data.',
        why: 'Fluid type and rock quality are shown separately. Water-bearing rock can meet the same quality cutoffs as oil-bearing rock.',
        effect: 'The totals show thickness supported by the available samples. They are not reserve estimates.',
      },
      {
        id: 'lens', title: 'Open the 2D section', target: '[data-tutorial="lens-section"]', targetName: '2D Section button', task: 'pay',
        instruction: 'Click “2D Section”. Use the WELL selector beside it to view another wellbore.',
        why: 'The section places the path, formation intervals and fluid interpretation on a depth axis.',
        effect: 'Only the view changes. Your analysis settings stay the same.',
        check: 'section-lens',
      },
      {
        id: 'target', title: 'Compare drilling targets', target: '[data-tutorial="target-readout"]', targetName: 'Selected target and nearby wells', task: 'targets',
        instruction: 'Choose a ranked target. Read its estimated thickness and the nearby wells used to calculate it.',
        why: 'P90 is the low estimate, P50 the median and P10 the high estimate. Ranking also considers porosity, permeability and uncertainty.',
        effect: 'Selection updates the target details. It does not choose a well to drill.',
      },
    ],
  },
  {
    id: 'sensitivity', title: 'Change an analysis', duration: '4–6 minutes',
    description: 'Change grid size, calculate new targets and restore the defaults.',
    steps: [
      {
        id: 'settings', title: 'Review the analysis settings', target: '[data-tutorial="analysis-settings"]', targetName: 'Analysis settings form', task: 'search',
        instruction: 'Read the current grid size. We’ll change it to 9, calculate the result, then restore the defaults.',
        why: 'Changing grid size is a quick way to see how analysis settings affect the candidate search.',
        effect: 'Changes take effect when you click Apply. Closing the tutorial leaves your chosen settings in place.',
      },
      {
        id: 'grid-edit', title: 'Set a 9-by-9 search grid', target: '[data-tutorial="setting-gridPointsPerAxis"]', targetName: 'Grid points per axis', task: 'search',
        instruction: 'Change “Grid points per axis” to 9. Do not click Apply yet. If 9 is already entered, that part is complete.',
        why: 'Nine points on each axis produce 81 candidate cells.',
        effect: 'The form changes immediately. Results still use the previous settings until you click Apply.',
        check: 'grid-draft',
      },
      {
        id: 'grid-apply', title: 'Calculate the new grid', target: '[data-tutorial="analysis-settings"]', targetName: 'Apply analysis settings and result status', task: 'search',
        instruction: 'Click “Apply analysis settings”. The result should contain 81 candidate cells.',
        why: 'The table includes excluded cells and their reasons. The map shows only the ranked shortlist.',
        effect: 'The Analysis API recalculates the targets. Field measurements stay unchanged.',
        check: 'grid-applied',
      },
      {
        id: 'exclusions', title: 'Inspect excluded locations', target: '[data-tutorial="task-workspace"]', targetName: 'Exclusion decisions and screening radius', task: 'exclusions',
        instruction: 'Read the exclusion reasons and distances to nearby wells. You can change the radius and click Apply to compare results.',
        why: 'The radius uses wells with usable analysis data. It does not cover every known wellbore or replace an anti-collision study.',
        effect: 'A larger radius can remove more targets.',
      },
      {
        id: 'criteria', title: 'Adjust rock cutoffs', target: '[data-tutorial="analysis-settings"]', targetName: 'Rock criteria', task: 'criteria',
        instruction: 'Inspect minimum porosity and permeability. Optionally change porosity from 0.12 to 0.13, click Apply, and compare the targets.',
        why: 'Porosity is entered as a fraction; permeability is in m². Higher cutoffs reduce the samples that qualify.',
        effect: 'Apply updates the current analysis. Previously saved interpretations keep their original results.',
      },
      {
        id: 'restore', title: 'Restore the default settings', target: '[data-tutorial="analysis-settings"]', targetName: 'Load defaults and Apply', task: 'search',
        instruction: 'Click “Load defaults into form”, then “Apply analysis settings”. Choose “Skip exercise” if you want to keep your settings.',
        why: 'The default grid has 15 points per axis and 225 cells.',
        effect: 'Apply recalculates using all default settings, including rock cutoffs, radius and neighbor count.',
        check: 'defaults-applied',
      },
      {
        id: 'uncertainty', title: 'Inspect uncertainty', target: '[data-tutorial="task-workspace"]', targetName: 'Uncertainty components', task: 'uncertainty',
        instruction: 'Look at how disagreement between wells and distance from measurements contribute to the estimate.',
        why: 'The displayed ranges come from the model. Their accuracy has not yet been calibrated across many fields.',
        effect: 'This screen explains the result; reading it changes no settings.',
      },
    ],
  },
  {
    id: 'interpretations', title: 'Compare interpretations', duration: '5–8 minutes',
    description: 'Inspect the reservoir model, save alternatives and compare results.',
    steps: [
      {
        id: 'reservoir', title: 'Explore the 3D reservoir', target: '[data-tutorial="visualization"]', targetName: '3D Reservoir view', task: 'model',
        instruction: 'Rotate the model, adjust depth exaggeration and select a fluid. Try a smaller search radius to see where the data stops supporting a volume.',
        why: 'The model needs measurements from at least three independent locations for each fluid. Its shape depends on the available evidence.',
        effect: 'Camera and display controls change the view. The separate “Ranking IDW neighbors” setting changes analysis only after Apply.',
      },
      {
        id: 'correlation', title: 'Draft a formation interpretation', target: '[data-tutorial="formation-ai"]', targetName: 'AI interpretation draft', task: 'correlation',
        instruction: 'Click “Draft interpretation with AI”. Read its proposed notes, supporting evidence and checks. If you agree, acknowledge the review and choose “Use this draft”. You can write your own notes if AI is unavailable.',
        why: 'The agent compares the displayed formation evidence, current analysis and any notes you have already entered. It identifies possible explanations and what still needs checking.',
        effect: 'The AI request uses the configured model. “Use this draft” fills the form; it does not save the hypothesis or change calculations. Save and edit the interpretation in the next step.',
      },
      {
        id: 'hypothesis', title: 'Save an interpretation', target: '[data-tutorial="task-workspace"]', targetName: 'Alternatives and saved revisions', task: 'alternatives',
        instruction: 'Enter a hypothesis name and your reason for choosing the target, then click “Save hypothesis”. Your correlation notes are saved with it. You can also load an existing revision to inspect.',
        why: 'A saved hypothesis keeps the evidence, settings, target and reasoning used for that result.',
        effect: 'Saving creates a persistent record. Loading one shows its history alongside the current field analysis.',
      },
      {
        id: 'branches', title: 'Create an alternative', target: '[data-tutorial="hypothesis-branch"]', targetName: 'Alternative settings', task: 'alternatives',
        instruction: 'Load a saved revision, choose a preset and click “Reanalyze stored evidence”. Review the result before saving the named alternative.',
        why: 'Spatial chooses a distant eligible target. Conservative raises the rock cutoffs. Both use the saved evidence.',
        effect: 'Saving creates a separate hypothesis and keeps the original unchanged.',
        missing: 'Load or save an exact revision in Alternatives to reveal the branch editor. You can skip this step without creating data.',
      },
      {
        id: 'challenge', title: 'Review a hypothesis', target: '[data-tutorial="task-workspace"]', targetName: 'Challenge and review controls', task: 'challenge',
        instruction: 'Load a saved hypothesis. Write an objection, cite supporting evidence and save it. Record whether each objection is accepted, rejected or deferred, with a reason.',
        why: 'This keeps the review attached to the version that was examined.',
        effect: 'Saving records the review. Optional AI requests run only when you click an AI action and may incur usage charges.',
      },
      {
        id: 'comparison', title: 'Compare saved results', target: '[data-tutorial="task-workspace"]', targetName: 'Saved revision comparison', task: 'compare',
        instruction: 'Add two to eight saved revisions to the comparison. Click “Compare selected exact revisions” and review the differences.',
        why: 'Check whether the results use the same evidence before attributing a difference to settings.',
        effect: 'The comparison uses the versions you selected. Save your preference in the rationale field.',
      },
      {
        id: 'bundle', title: 'Download the analysis', target: '[data-tutorial="task-workspace"]', targetName: 'Evidence bundle', task: 'bundle',
        instruction: 'Add a decision rationale and click “Download evidence bundle JSON” when you want a copy.',
        why: 'The file includes the current evidence, settings, result and selected target.',
        effect: 'A JSON file downloads to your computer. The app’s stored data stays unchanged.',
      },
    ],
  },
  {
    id: 'simulation', title: 'Run a guided simulation', duration: '15–25 minutes plus simulation time',
    description: 'Create a new scenario, review editable demo estimates, approve and run it, then inspect the new well and scorecard. Every write is your explicit action. Resume an existing scenario or skip exercises to read without changing data.',
    steps: [
      {
        id: 'simulation-field', title: 'Choose the original field', target: '[data-tutorial="scope"]', targetName: 'Original field and reservoir selectors',
        instruction: 'Choose TROLL FORCE-SODIR with the “Original data” badge, SOGNEFJORD FM, and “Live evidence”. To continue a scenario instead, return to the chapter outline and choose Resume current scenario.',
        why: 'A new simulation must begin with the original measurements, not a previous simulated well’s results.',
        effect: 'Selecting a dataset only reads evidence. Existing completed scenarios remain unchanged.',
        check: 'source',
      },
      {
        id: 'scenario', title: 'Create your scenario', target: '[data-tutorial="scenario-create"]', targetName: 'New simulation setup', task: 'prediction',
        instruction: 'Choose “New simulation” if the form is not open. Review the scenario name, evidence cutoff in UTC, purpose and assumptions, acknowledge the review, then click “Create scenario”.',
        why: 'The scenario records what can be known before drilling. A versioned assumptions fingerprint is calculated for you.',
        effect: 'Create saves or reuses this exact request. It does not create a forecast, approve anything or start a simulator.',
        check: 'scenario-created',
        missing: 'Choose New simulation above to open the reviewed scenario setup. Existing completed results stay unchanged.',
      },
      {
        id: 'frozen-analysis', title: 'Analyze the earlier measurements', target: '[data-tutorial="analyze-frozen-source"]', targetName: 'Analyze frozen initial source with applied settings', task: 'prediction',
        instruction: 'Click “Analyze frozen initial source with applied settings”. If analysis settings are pending, first Apply them in the relevant analysis task and return here.',
        why: 'The prediction must use this scenario’s original field at its initial UTC cutoff. Applied rock, grid and neighbor settings are preserved.',
        effect: 'An explicit calculation produces a verified analysis. It leaves saved predictions and the displayed field unchanged.',
        check: 'frozen-analysis',
      },
      {
        id: 'prediction-target', title: 'Choose an eligible target', target: '[data-tutorial="prediction-target"]', targetName: 'Frozen-source eligible target', task: 'prediction',
        instruction: 'Choose an eligible target from the frozen-source list. Read its exact east/north coordinates and P90/P50/P10 thickness estimates.',
        why: 'This guided flow supports one horizontal location. Every proposed path station must keep the chosen point’s exact coordinates.',
        effect: 'Selection chooses the source for the forecast. It never rewrites an existing draft or flattens a directional path.',
        check: 'eligible-target',
      },
      {
        id: 'demo-forecast', title: 'Review editable forecast examples', target: '[data-tutorial="demo-forecast"]', targetName: 'Editable demo forecast', task: 'prediction',
        instruction: 'Click “Use editable demo forecast”, then review or edit the path depths, formation bounds, fluids, cumulative production, rationale and uncertainties in the form. If you intentionally chose a different target from an existing draft, preserve it before explicitly starting a new point-screening template.',
        why: 'Pay quantiles and citations come from the verified analysis. Depth, formation, fluid and production examples are demonstration estimates, not inferred truth or calibration.',
        effect: 'The preset fills blank local inputs only. It does not save or approve anything; Advanced retains manual JSON editing.',
        check: 'forecast-ready',
      },
      {
        id: 'save-prediction', title: 'Save the forecast draft', target: '[data-tutorial="save-prediction"]', targetName: 'Save draft', task: 'prediction',
        instruction: 'Resolve any missing inputs, then click “Save draft”. Wait for a numbered saved revision. If a conflicting revision exists, preserve your text and deliberately load the latest draft.',
        why: 'Only a server-validated saved record is eligible for review and sealing.',
        effect: 'Save persists a draft. It remains editable until sealed.',
        check: 'prediction-saved',
      },
      {
        id: 'prediction', title: 'Inspect the saved prediction', target: '[data-tutorial="prediction-ledger"]', targetName: 'Saved prediction ledger', task: 'prediction',
        instruction: 'Read the saved path, formation and fluid assumptions, cumulative production and uncertainty notes. Confirm this is the forecast you want evaluated.',
        why: 'The saved ledger is the subject you will seal, not an unsaved form or a later simulated observation.',
        effect: 'Reading changes nothing. Four comparison baselines will be frozen with the seal.',
        check: 'prediction-saved',
      },
      {
        id: 'seal-prediction', title: 'Seal the reviewed revision', target: '[data-tutorial="seal-prediction"]', targetName: 'Seal reviewed revision', task: 'prediction',
        instruction: 'Select “I reviewed saved revision…” and click “Seal reviewed revision”. If already sealed, continue without trying to seal again.',
        why: 'Sealing permanently freezes this revision and its baselines before any simulated answer is available.',
        effect: 'Seal is separate from human approval and execution. No run starts.',
        check: 'prediction-sealed',
      },
      {
        id: 'approve-prediction', title: 'Approve the prediction', target: '[data-tutorial="approve-prediction"], [data-tutorial="prediction-approved"]', targetName: 'Prediction approval controls', task: 'prediction',
        instruction: 'Enter your operator name or initials above the approval controls, or explicitly choose “Use demo operator label”. Review the immutable seal, acknowledge it, then click “Approve reviewed prediction”.',
        why: 'Your review connects the exact sealed prediction to the simulation decision.',
        effect: 'Approval advances this scenario to an approved prediction. It does not prepare or start the simulator.',
        check: 'prediction-approved',
      },
      {
        id: 'prepare-simulator', title: 'Configure and prepare the simulator', target: '[data-tutorial="simulator-setup"]', targetName: 'Simulator preset, resolution and realization seed', task: 'simulation',
        instruction: 'Choose a model preset, Preview or Standard resolution, and an integer realization seed. Review those settings and the approved prediction, select the acknowledgement, then click “Prepare simulator”. Preview is the smaller demo calculation.',
        why: 'The server supplies curated model presets for this field. Preparation fixes the selected setup; different settings require a new scenario.',
        effect: 'The server privately prepares and binds the simulator. No hidden geology is returned, and drilling has not started.',
        check: 'model-prepared',
      },
      {
        id: 'execution', title: 'Start this simulation', target: '[data-tutorial="start-simulation"], [data-tutorial="simulation-progress"]', targetName: 'Start simulation and run progress', task: 'simulation',
        instruction: 'Click the enabled “Start simulation” button once, then watch the run’s checkpoint table. If a run already exists, inspect it rather than starting another.',
        why: 'A run requires the approved seal and prepared model. Failed requests keep their stable retry identity.',
        effect: 'Start explicitly begins drilling, surveys and log generation. Navigating here never starts or reruns a scenario.',
        check: 'run-started',
      },
      {
        id: 'completion-review', title: 'Review the completion openings', target: '[data-tutorial="completion-review"], [data-tutorial="simulation-progress"]', targetName: 'S6 completion review', task: 'simulation',
        instruction: 'Wait for the S6 completion pause. Inspect every opening’s reservoir, depth interval, radius, skin, efficiency and uncertainty. Status refreshes while the run is active; use Refresh operator status if needed.',
        why: 'Completion is designed from observations. You approve the visible design before production can proceed.',
        effect: 'Reading the openings is not approval.',
        check: 'completion-review',
      },
      {
        id: 'approve-completion', title: 'Approve the reviewed completion', target: '[data-tutorial="completion-review"]', targetName: 'Approve reviewed completion', task: 'simulation',
        instruction: 'Check the opening-review acknowledgement and click “Approve reviewed completion”. If the fingerprint changes, review the changed design again.',
        why: 'The approval applies only to these exact openings for this scenario and run.',
        effect: 'This explicit approval permits the production workflow to continue. It does not publish the measurements yet.',
        check: 'completion-approved',
      },
      {
        id: 'production', title: 'Wait for production checkpoints', target: '[data-tutorial="simulation-progress"]', targetName: 'Production checkpoint progress', task: 'simulation',
        instruction: 'Wait for S7 production to complete. Inspect any reported failure instead of starting another run. When ready, continue to New evidence.',
        why: 'The production calculation must finish before its measurements can be published together.',
        effect: 'The existing run produces its checkpoints. Reading progress sends no new command.',
        check: 'production-complete',
      },
      {
        id: 'publish', title: 'Publish the new measurements', target: '[data-tutorial="publish-simulation"], [data-tutorial="published-simulation"]', targetName: 'Publication review and action', task: 'new-evidence',
        instruction: 'Review the ready run, acknowledge publication and click “Publish reveal and evaluate”. If already published, inspect the receipt instead; do not republish.',
        why: 'Publication releases verified synthetic observations to a separate results field and advances this scenario’s evidence clock.',
        effect: 'Publishing adds the new evidence and requests evaluation. If evaluation fails, the published evidence remains available.',
        check: 'published',
      },
      {
        id: 'reveal', title: 'Inspect the new well and production', target: '[data-tutorial="revealed-evidence"]', targetName: 'New evidence and production', task: 'new-evidence',
        instruction: 'Compare the before-and-after record counts. Inspect monthly and cumulative production over one, three and five years.',
        why: 'The original field remains unchanged. The results field contains the newly published synthetic well and measurements.',
        effect: 'This is a read-only inspection of this scenario’s actual receipt and production metadata, not a hard-coded demo total.',
        check: 'production-visible',
      },
      {
        id: 'evaluate', title: 'Compare predictions with results', target: '[data-tutorial="simulation-scorecard"], [data-tutorial="evaluate-simulation"]', targetName: 'Evaluation scorecard or evaluation action', task: 'evaluation',
        instruction: 'Inspect the published scorecard, its baselines and unavailable metrics. If publication succeeded but evaluation is pending or failed, review operator status and explicitly use “Run / retry evaluation” when enabled.',
        why: 'The two comparisons separate prediction error from measurement error.',
        effect: 'Evaluation compares this run’s immutable forecast with results; it never redrills or republishes the well.',
        check: 'scorecard',
      },
      {
        id: 'architecture', title: 'Review the architecture', target: '[data-tutorial="task-workspace"]', targetName: 'Evaluation workspace', task: 'evaluation',
        instruction: 'Open the full demo guide for the architecture diagram and presenter notes.',
        why: 'Separate services store wells, surveys and geology. Analysis combines them; the simulators generate new observations; evaluation compares those observations with the prediction.',
        effect: 'Aspire runs the services locally. GitHub Pages hosts the walkthrough document.',
      },
    ],
  },
]

export function tutorialStepSatisfied(step: TutorialStep, context: TutorialContext, pressed = false) {
  if (!step.check) return true
  if (context.loading || context.applying || context.error) return false
  if (step.task && step.task !== context.activeTask) return false
  switch (step.check) {
    case 'source':
      return context.fieldId === tutorialSourceField && context.packageFieldId === tutorialSourceField &&
        context.reservoir === 'SOGNEFJORD FM' && !context.scenarioId
    case 'section-lens': return pressed
    case 'grid-draft': return context.draft.gridPointsPerAxis === 9
    case 'grid-applied':
      return !context.stale && context.applied?.gridPointsPerAxis === 9 && context.gridCount === 81 && Boolean(context.analysisSha256)
    case 'defaults-applied':
      return !context.stale && Boolean(context.applied) && configurationKey(context.applied!) === configurationKey(defaultConfiguration) &&
        context.gridCount === 225 && Boolean(context.analysisSha256)
    default: return simulationChecks(context)[step.check] ?? false
  }

}

export function tutorialCheckMessage(step: TutorialStep, complete: boolean) {
  switch (step.check) {
    case 'source': return complete ? 'Troll is loaded. Next, inspect the available data.' : 'Select Troll marked Original data, SOGNEFJORD FM and Live evidence to continue.'
    case 'grid-draft': return complete ? 'Grid size set to 9. Next, calculate the new result.' : 'Enter 9 in the grid-size field to continue.'
    case 'grid-applied': return complete ? 'Analysis updated: 81 candidate cells.' : 'Click Apply analysis settings and wait for the result.'
    case 'defaults-applied': return complete ? 'Defaults applied: 225 candidate cells.' : 'Load defaults into form, then click Apply analysis settings.'
    case 'section-lens': return complete ? 'The 2D section is open.' : 'Click 2D Section to continue.'
    case 'scenario-created': return complete ? 'Scenario loaded. Its earlier evidence can now be analyzed.' : 'Review the setup and click Create scenario, or resume a different existing scenario.'
    case 'frozen-analysis': return complete ? 'Verified frozen-source analysis is available.' : 'Analyze the frozen initial source with the applied settings; wait for a matching result.'
    case 'eligible-target': return complete ? 'An eligible frozen-source target is selected.' : 'Choose an eligible target from the verified frozen-source result.'
    case 'forecast-ready': return complete ? 'The forecast inputs are complete for review and saving.' : 'Fill and review the forecast; resolve the required-input or binding messages.'
    case 'prediction-saved': return complete ? 'A saved prediction revision is available.' : 'Click Save draft and wait for the saved revision.'
    case 'prediction-sealed': return complete ? 'The reviewed prediction is sealed.' : 'Review the saved revision, acknowledge it and click Seal reviewed revision.'
    case 'prediction-approved': return complete ? 'The exact sealed prediction has been approved.' : 'Enter an operator label, review the seal and explicitly approve the prediction.'
    case 'model-prepared': return complete ? 'The simulator is prepared. Start remains a separate action.' : 'Review the model settings and click Prepare simulator when available.'
    case 'run-started': return complete ? 'A run exists for this scenario. Continue with that run.' : 'Prepare the simulator, then explicitly click Start simulation.'
    case 'completion-review': return complete ? 'The completion review is available or has already been approved.' : 'Wait for the S6 completion review and inspect any reported failure.'
    case 'completion-approved': return complete ? 'Completion approval is recorded for this run.' : 'Review every opening and explicitly approve the matching completion.'
    case 'production-complete': return complete ? 'Production checkpoints are complete.' : 'Wait for the production checkpoint to finish; do not start another run.'
    case 'published': return complete ? 'A verified publication receipt is available.' : 'Review and publish this ready run; wait for its receipt.'
    case 'production-visible': return complete ? 'New evidence and production metadata are available.' : 'Wait for the matching reveal receipt and production metadata, then refresh the scenario if needed.'
    case 'scorecard': return complete ? 'The new scenario’s scorecard is available.' : 'Inspect evaluation status; explicitly run or retry evaluation only when enabled.'
    default: return ''
  }
}

function simulationChecks(context: TutorialContext): Partial<Record<TutorialCheck, boolean>> {
  const scenario = context.scenario
  if (!scenario || scenario.scenarioId !== context.scenarioId || scenario.sourceFieldId !== tutorialSourceField ||
      scenario.reservoirName !== 'SOGNEFJORD FM') return {}
  const resources = context.resources?.scenarioId === scenario.scenarioId && !context.resources.error
    ? context.resources : undefined
  const prediction = resources?.prediction?.scenarioId === scenario.scenarioId ? resources.prediction : undefined
  const saved = Boolean(prediction && prediction.revision >= 1)
  const sealed = Boolean(saved && prediction?.seal?.sha256 && /^[a-f0-9]{64}$/i.test(prediction.seal.sha256))
  const approved = Boolean(sealed && prediction?.approval?.sealedSha256 === prediction?.seal?.sha256)
  const operator = resources?.operator?.scenarioId === scenario.scenarioId && resources.operator.enabled ? resources.operator : undefined
  const setup = resources?.operatorSetup?.scenarioId === scenario.scenarioId ? resources.operatorSetup : undefined
  const run = operator?.run
  const completedStage = (prefix: string) => Boolean(run?.stages.some(stage => stage.stage.startsWith(prefix) && stage.status === 'Completed'))
  const completionApproved = Boolean(run && (completedStage('S6') || operator?.completion.status === 'Approved' &&
    operator.completion.scenarioId === scenario.scenarioId && operator.completion.runId === run.runId))
  const published = resources?.reveal?.scenarioId === scenario.scenarioId && resources.reveal.status === 'Revealed'
  const productionVisible = Boolean(published && resources?.production?.scenarioId === scenario.scenarioId &&
    resources.production.revealId === resources.reveal?.revealId && resources.production.monthCount > 0)
  const binding = prediction?.body.analysisBinding
  const savedSource = Boolean(saved && binding?.scenarioId === scenario.scenarioId && binding.fieldId === scenario.sourceFieldId &&
    binding.reservoirName === scenario.reservoirName && Date.parse(binding.asOfUtc ?? '') === Date.parse(scenario.initialAsOfUtc))
  const preparation = context.preparation
  const preparedSource = Boolean(preparation && preparation.scenarioId === scenario.scenarioId &&
    preparation.sourceFieldId === scenario.sourceFieldId && Date.parse(preparation.asOfUtc) === Date.parse(scenario.initialAsOfUtc) &&
    preparation.configurationKey === configurationKey(context.applied ?? defaultConfiguration) && !context.stale && preparation.analysisSha256)
  return {
    'scenario-created': scenario.scenarioId !== tutorialScenario,
    'frozen-analysis': savedSource || preparedSource,
    'eligible-target': savedSource || Boolean(preparedSource && preparation?.candidateId),
    'forecast-ready': saved || Boolean(preparedSource && preparation?.candidateId && preparation.forecastReady),
    'prediction-saved': saved,
    'prediction-sealed': sealed,
    'prediction-approved': approved,
    'model-prepared': Boolean(approved && (setup?.prepared || operator?.preflight.worldBound) &&
      operator?.prediction.sealHash === prediction?.seal?.sha256),
    'run-started': Boolean(run?.runId),
    'completion-review': completionApproved || Boolean(run && operator?.completion.available &&
      operator.completion.scenarioId === scenario.scenarioId && operator.completion.runId === run.runId && operator.completion.openingsHash),
    'completion-approved': completionApproved,
    'production-complete': completedStage('S7') || productionVisible,
    'published': Boolean(published),
    'production-visible': productionVisible,
    'scorecard': Boolean(published && resources?.scorecard?.scenarioId === scenario.scenarioId &&
      resources.scorecard.revealId === resources.reveal?.revealId && resources.scorecard.metrics.length > 0),
  }
}

export function tutorialResumeIndex(context: TutorialContext) {
  const chapter = tutorialChapters.find(item => item.id === 'simulation')!
  if (!context.scenario || context.scenario.scenarioId !== context.scenarioId) return 0
  if (['Revealed', 'Scored'].includes(context.scenario.status)) return chapter.steps.findIndex(step => step.id === 'reveal')
  const checks = simulationChecks(context)
  const index = chapter.steps.findIndex((step, index) => index >= 2 && step.check && !checks[step.check])
  return index < 0 ? chapter.steps.findIndex(step => step.id === 'reveal') : index
}
