import { configurationErrors, configurationKey, defaultConfiguration } from './analysisConfiguration'
import { visibleEvidenceIds } from './evidence'
import type {
  AnalysisResult, FieldPackage, PredictionBody, PredictionCapabilities, QuantileValues,
  RankedCandidate, Scenario, VersionedPredictionAnalysisBinding,
} from './types'

export const predictionBindingVersion = 'prediction-analysis-binding-v1'
export interface PredictionSourceContext {
  scenario: Scenario
  fieldPackage: FieldPackage
  analysis: AnalysisResult
  candidate?: RankedCandidate
  capabilities?: PredictionCapabilities
}
export interface PredictionPreparationState {
  scenarioId: string
  sourceFieldId: string
  asOfUtc: string
  configurationKey: string
  analysisSha256: string
  packageSha256: string
  candidateId?: string
  forecastReady: boolean
}
const hash = (value: unknown) => typeof value === 'string' && /^[a-f0-9]{64}$/.test(value)
const sameInstant = (a: string | null | undefined, b: string | null | undefined) =>
  Boolean(a && b && Number.isFinite(Date.parse(a)) && Date.parse(a) === Date.parse(b))
const samePay = (left: QuantileValues, right: QuantileValues) =>
  left.p90 === right.p90 && left.p50 === right.p50 && left.p10 === right.p10
export const candidatePay = (candidate: RankedCandidate): QuantileValues =>
  ({ p90: candidate.p90NetPayM, p50: candidate.p50NetPayM, p10: candidate.p10NetPayM })

export function predictionCapabilityBlock(capabilities?: PredictionCapabilities, action: 'save' | 'seal' = 'save') {
  if (!capabilities) return 'Configured handoff capabilities are unavailable. Refresh capabilities; no default configuration will be substituted.'
  if (capabilities.version !== 'prediction-handoff-capabilities-v1' || capabilities.bindingVersion !== predictionBindingVersion ||
      capabilities.analysisModelVersion !== 'petrophysics-screening-v1' || capabilities.configurationVersion !== 'analysis-configuration-v1' ||
      capabilities.baselineBindingVersion !== 'baseline-analysis-binding-v1' ||
      !Number.isInteger(capabilities.minimumLocatedControls) || capabilities.minimumLocatedControls < 4 ||
      !['baseline-nearest-well-v3', 'baseline-field-mean-v3', 'baseline-four-neighbor-idw-v3', 'baseline-uncertainty-aware-rank1-v3']
        .every(version => capabilities.baselineModelVersions?.includes(version))) {
    return 'The server advertises an unsupported prediction-handoff contract. Configured handoff is blocked; existing records remain unchanged.'
  }
  if (!capabilities.configuredSaveSupported || (action === 'seal' && !capabilities.configuredSealSupported)) {
    return `The server does not advertise configured prediction ${action} support.`
  }
  return undefined
}

export function isInitialPredictionPackage(scenario: Scenario, pkg: FieldPackage) {
  return pkg.fieldId === scenario.sourceFieldId && sameInstant(pkg.generatedAt, scenario.initialAsOfUtc)
}

export function predictionSourceBlock(context: PredictionSourceContext, requireCandidate = true) {
  const capability = predictionCapabilityBlock(context.capabilities)
  if (capability) return capability
  const { scenario, fieldPackage: pkg, analysis, candidate } = context
  if (!scenario.scenarioId || !scenario.sourceFieldId || !scenario.reservoirName || !scenario.initialAsOfUtc) return 'An explicit scenario and frozen initial source scope are required.'
  if (!isInitialPredictionPackage(scenario, pkg)) return 'Use the original field at this scenario’s initial UTC evidence cutoff, not the later simulated results. Analyze the earlier measurements before preparing a prediction.'
  if (!analysis.configuration || configurationErrors(analysis.configuration).length ||
      analysis.modelVersion !== context.capabilities!.analysisModelVersion || analysis.fieldId !== scenario.sourceFieldId ||
      analysis.reservoirName !== scenario.reservoirName || analysis.packageSha256 !== pkg.sha256 ||
      !hash(analysis.configurationSha256) || !hash(analysis.analysisSha256) || !hash(pkg.sha256)) {
    return 'The frozen-source result, model, configuration or package fingerprints do not match. Explicitly reanalyze the frozen source.'
  }
  if (!Array.isArray(analysis.wellSummaries) || analysis.wellSummaries.filter(item => Number.isFinite(item.eastingM) && Number.isFinite(item.northingM)).length < context.capabilities!.minimumLocatedControls) {
    return `Configured handoff requires at least ${context.capabilities!.minimumLocatedControls} located screening controls.`
  }
  if (requireCandidate) {
    const actual = analysis.candidateGrid?.find(cell => cell.status === 'eligible' && cell.candidateId === candidate?.candidateId)?.prediction
    if (!actual || !candidate || [actual.eastingM, actual.northingM, actual.p90NetPayM, actual.p50NetPayM, actual.p10NetPayM]
      .some(value => typeof value !== 'number' || !Number.isFinite(value)) || actual.eastingM !== candidate.eastingM || actual.northingM !== candidate.northingM ||
        !samePay(candidatePay(actual), candidatePay(candidate))) return 'Select an eligible full-grid target from this exact frozen-source result; target geometry or pay does not match.'
  }
  return undefined
}

export function createPredictionBinding(context: PredictionSourceContext): VersionedPredictionAnalysisBinding {
  const problem = predictionSourceBlock(context)
  if (problem) throw new Error(problem)
  const { analysis, scenario, candidate } = context
  return {
    version: predictionBindingVersion, modelVersion: 'petrophysics-screening-v1',
    configuration: { ...analysis.configuration! },
    configurationSha256: analysis.configurationSha256!, analysisSha256: analysis.analysisSha256!,
    scenarioId: scenario.scenarioId, fieldId: scenario.sourceFieldId, reservoirName: scenario.reservoirName,
    asOfUtc: new Date(scenario.initialAsOfUtc).toISOString(),
    target: { candidateId: candidate!.candidateId, eastingM: candidate!.eastingM, northingM: candidate!.northingM, expectedPaydirtM: candidatePay(candidate!) },
  }
}

function object(value: unknown, keys: string[], label: string): asserts value is Record<string, unknown> {
  if (!value || typeof value !== 'object' || Array.isArray(value) || Object.keys(value).length !== keys.length ||
      keys.some(key => !Object.hasOwn(value, key))) throw new Error(`${label} must contain exactly: ${keys.join(', ')}.`)
}
export function validatePredictionBinding(binding: unknown): asserts binding is VersionedPredictionAnalysisBinding {
  if (!binding || typeof binding !== 'object' || Array.isArray(binding)) throw new Error('analysisBinding must be a versioned object.')
  const value = binding as Record<string, unknown>
  if (value.version !== predictionBindingVersion) {
    throw new Error('Unversioned or unsupported analysis-bound draft requires an explicit rebuild from a supported frozen-source result. Its binding must not be stripped or silently upgraded.')
  }
  object(value, ['version', 'modelVersion', 'configuration', 'configurationSha256', 'analysisSha256', 'scenarioId', 'fieldId', 'reservoirName', 'asOfUtc', 'target'], 'analysisBinding')
  if (value.modelVersion !== 'petrophysics-screening-v1') throw new Error('Unsupported analysis binding model version.')
  object(value.configuration, ['version', 'porosityCutoff', 'permeabilityCutoffM2', 'wellExclusionRadiusM', 'gridPointsPerAxis', 'idwNeighborCount'], 'analysisBinding.configuration')
  const config = value.configuration
  if (typeof config.version !== 'string' || typeof config.porosityCutoff !== 'number' || typeof config.permeabilityCutoffM2 !== 'number' ||
      typeof config.wellExclusionRadiusM !== 'number' || typeof config.gridPointsPerAxis !== 'number' || typeof config.idwNeighborCount !== 'number' ||
      configurationErrors({ version: config.version, porosityCutoff: config.porosityCutoff, permeabilityCutoffM2: config.permeabilityCutoffM2,
        wellExclusionRadiusM: config.wellExclusionRadiusM, gridPointsPerAxis: config.gridPointsPerAxis, idwNeighborCount: config.idwNeighborCount }).length) {
    throw new Error('Analysis binding configuration is invalid; every setting requires its documented numeric type.')
  }
  if (!hash(value.configurationSha256) || !hash(value.analysisSha256)) throw new Error('Binding fingerprints must be 64 lowercase hexadecimal characters.')
  for (const field of ['scenarioId', 'fieldId', 'reservoirName']) {
    if (typeof value[field] !== 'string' || !value[field].trim()) throw new Error(`analysisBinding.${field} is required.`)
  }
  if (typeof value.asOfUtc !== 'string' || !Number.isFinite(Date.parse(value.asOfUtc)) || !/(Z|\+00:00)$/.test(value.asOfUtc)) throw new Error('Analysis binding asOfUtc must be an explicit UTC clock.')
  object(value.target, ['candidateId', 'eastingM', 'northingM', 'expectedPaydirtM'], 'analysisBinding.target')
  if (typeof value.target.candidateId !== 'string' || !value.target.candidateId) throw new Error('The bound candidate ID is required.')
  for (const field of ['eastingM', 'northingM']) {
    if (typeof value.target[field] !== 'number' || !Number.isFinite(value.target[field]) || Math.abs(value.target[field]) > 1e9) throw new Error('Binding target coordinates must be finite local metres.')
  }
  object(value.target.expectedPaydirtM, ['p90', 'p50', 'p10'], 'analysisBinding.target.expectedPaydirtM')
  for (const field of ['p90', 'p50', 'p10']) {
    const scalar = value.target.expectedPaydirtM[field]
    if (typeof scalar !== 'number' || !Number.isFinite(scalar)) throw new Error('Binding pay quantiles require finite numbers.')
  }
}

export function validateBoundPredictionBody(body: PredictionBody) {
  if (body.analysisBinding == null) {
    if (body.candidateId.startsWith('configured:')) throw new Error('A configured candidate requires its versioned analysisBinding. Removing metadata cannot create a legacy handoff.')
    return
  }
  validatePredictionBinding(body.analysisBinding)
  const binding = body.analysisBinding
  if (binding.target.candidateId !== body.candidateId || !samePay(binding.target.expectedPaydirtM, body.expectedPaydirtM)) throw new Error('Body candidate and expected pay must match the bound target exactly.')
  if (body.proposedWellPath.some(station => station.eastingM !== binding.target.eastingM || station.northingM !== binding.target.northingM)) {
    throw new Error('Point-screening binding v1 requires every path station at the candidate easting/northing. Directional edits are retained, not flattened; this geometry cannot be saved or sealed under v1.')
  }
  if (!body.formations.some(formation => formation.formationName === binding.reservoirName)) throw new Error('The bound reservoir formation must be present in the prediction.')
}

export function validatePredictionSource(body: PredictionBody, context: PredictionSourceContext, action: 'save' | 'seal' = 'save') {
  validateBoundPredictionBody(body)
  if (body.analysisBinding == null) {
    if (context.capabilities?.legacyNullBindingSupported === false) throw new Error('This server does not advertise legacy null-binding handoff.')
    if (context.analysis.configuration && configurationKey(context.analysis.configuration) !== configurationKey(defaultConfiguration)) {
      throw new Error('The active analysis is configured; explicitly build a versioned binding rather than sealing it under defaults.')
    }
    if (!isInitialPredictionPackage(context.scenario, context.fieldPackage) || body.fieldPackageSha256 !== context.fieldPackage.sha256) throw new Error('Legacy draft package does not match the frozen initial source package.')
    return
  }
  const actual = context.analysis.candidateGrid?.find(cell => cell.status === 'eligible' && cell.candidateId === body.candidateId)?.prediction
  const problem = predictionSourceBlock({ ...context, candidate: actual ?? undefined }) || predictionCapabilityBlock(context.capabilities, action)
  if (problem) throw new Error(problem)
  const expected = createPredictionBinding({ ...context, candidate: actual! })
  const binding = body.analysisBinding
  if (binding.scenarioId !== expected.scenarioId || binding.fieldId !== expected.fieldId || binding.reservoirName !== expected.reservoirName ||
      !sameInstant(binding.asOfUtc, expected.asOfUtc) || body.fieldPackageSha256 !== context.fieldPackage.sha256 ||
      binding.analysisSha256 !== expected.analysisSha256 || binding.configurationSha256 !== expected.configurationSha256 ||
      configurationKey(binding.configuration) !== configurationKey(expected.configuration) ||
      binding.target!.eastingM !== expected.target.eastingM || binding.target!.northingM !== expected.target.northingM ||
      !samePay(binding.target!.expectedPaydirtM, expected.target.expectedPaydirtM)) throw new Error('Draft binding differs from the scenario frozen-source scope, configuration, result or target. Explicitly rebuild/review; no fallback defaults are used.')
  const evidence = visibleEvidenceIds(context.fieldPackage)
  if (body.citedEvidenceIds.some(id => !evidence.has(id))) throw new Error('The draft cites evidence outside the frozen initial source package.')
}

export function rebuildPredictionBinding(text: string, context: PredictionSourceContext): string {
  const body: unknown = JSON.parse(text)
  if (!body || typeof body !== 'object' || Array.isArray(body)) throw new Error('The existing draft must be a JSON object before its binding can be rebuilt.')
  const binding = createPredictionBinding(context)
  return JSON.stringify({
    ...body, candidateId: binding.target.candidateId, fieldPackageSha256: context.fieldPackage.sha256,
    expectedPaydirtM: binding.target.expectedPaydirtM, analysisBinding: binding,
  }, null, 2)
}
