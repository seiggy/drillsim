import type { FieldPackage, PredictionBody, RankedCandidate, Scenario, VersionedPredictionAnalysisBinding } from './types'
import { validateBoundPredictionBody, validatePredictionBinding } from './predictionBinding'

export const predictionText = (body: PredictionBody) => JSON.stringify(body, null, 2)
export const predictionDraftKey = (scenarioId: string) => `drillsim-prediction-draft-v1:${scenarioId}`
export const predictionSealAttemptKey = (scenarioId: string, revision: number) => `drillsim-prediction-seal-v1:${scenarioId}:${revision}`
export function readSealActionKey(value: string) {
  if (!/^[\x21-\x7e]{1,128}$/.test(value)) throw new Error('The saved seal attempt key is invalid. No seal request was sent; inspect the saved prediction before removing the local attempt record.')
  return value
}
export interface LocalPredictionDraft { text: string; baseRevision: number | null; requiresBinding?: boolean }

export function readLocalPredictionDraft(value: string): LocalPredictionDraft {
  const saved: unknown = JSON.parse(value)
  if (!saved || typeof saved !== 'object' || !('text' in saved) || typeof saved.text !== 'string' ||
      !('baseRevision' in saved) || (saved.baseRevision !== null &&
        (typeof saved.baseRevision !== 'number' || !Number.isSafeInteger(saved.baseRevision) || saved.baseRevision < 1))) {
    throw new Error('The local draft is unreadable. Download or clear it before editing.')
  }
  if ('requiresBinding' in saved && typeof saved.requiresBinding !== 'boolean') throw new Error('The local binding-origin marker is invalid. Preserve this draft for review.')
  return { text: saved.text, baseRevision: saved.baseRevision, ...('requiresBinding' in saved ? { requiresBinding: saved.requiresBinding as boolean } : {}) }
}

export function predictionTemplate(scenario: Scenario, fieldPackage: FieldPackage, candidate?: RankedCandidate, binding?: VersionedPredictionAnalysisBinding): string {
  if (binding) {
    validatePredictionBinding(binding)
    if (!candidate || binding.target.candidateId !== candidate.candidateId ||
        binding.target.eastingM !== candidate.eastingM || binding.target.northingM !== candidate.northingM ||
        binding.target.expectedPaydirtM.p90 !== candidate.p90NetPayM || binding.target.expectedPaydirtM.p50 !== candidate.p50NetPayM ||
        binding.target.expectedPaydirtM.p10 !== candidate.p10NetPayM ||
        binding.scenarioId !== scenario.scenarioId || binding.fieldId !== fieldPackage.fieldId ||
        Date.parse(binding.asOfUtc) !== Date.parse(fieldPackage.generatedAt)) {
      throw new Error('Template candidate/package differs from its versioned binding. No fallback template was generated.')
    }
  }
  const unknownQuantiles = { p90: null, p50: null, p10: null }
  return JSON.stringify({
    candidateId: candidate?.candidateId ?? '',
    proposedWellPath: [0, 1].map(() => ({
      measuredDepthM: null, trueVerticalDepthM: null,
      eastingM: candidate?.eastingM ?? null, northingM: candidate?.northingM ?? null,
    })),
    formations: [{
      formationName: scenario.reservoirName,
      topTrueVerticalDepthM: unknownQuantiles, baseTrueVerticalDepthM: unknownQuantiles,
    }],
    expectedPaydirtM: candidate
      ? { p90: candidate.p90NetPayM, p50: candidate.p50NetPayM, p10: candidate.p10NetPayM }
      : unknownQuantiles,
    fluidClasses: [],
    contactPredictions: [],
    productionForecasts: [1, 3, 5].map((year) => ({ year, oilM3: null, gasM3: null, waterM3: null })),
    uncertaintyAssumptions: [],
    citedEvidenceIds: candidate?.neighborEvidenceIds ?? [],
    fieldPackageSha256: fieldPackage.sha256,
    rationale: '',
    ...(binding ? { analysisBinding: binding } : {}),
  }, null, 2)
}

// Require the complete numeric contract here: omitted JSON numbers otherwise bind to zero on the server.
export function parsePrediction(text: string): PredictionBody {
  const body: unknown = JSON.parse(text)
  function object(value: unknown, keys: string[], label: string): asserts value is Record<string, unknown> {
    if (!value || typeof value !== 'object' || Array.isArray(value) ||
        Object.keys(value).length !== keys.length || keys.some((key) => !Object.hasOwn(value, key))) {
      throw new Error(`${label} must contain exactly: ${keys.join(', ')}.`)
    }
  }
  function number(value: unknown, label: string) {
    if (typeof value !== 'number' || !Number.isFinite(value)) throw new Error(`${label} needs a finite number, not null.`)
  }
  function string(value: unknown, label: string) {
    if (typeof value !== 'string' || !value.trim()) throw new Error(`${label} needs text.`)
  }
  function array(value: unknown, label: string): asserts value is unknown[] {
    if (!Array.isArray(value)) throw new Error(`${label} must be an array.`)
  }
  function quantiles(value: unknown, label: string) {
    object(value, ['p90', 'p50', 'p10'], label)
    for (const key of ['p90', 'p50', 'p10']) number(value[key], `${label}.${key}`)
  }
  function validate(value: unknown): asserts value is PredictionBody {
    const bindingPresent = value && typeof value === 'object' && Object.hasOwn(value, 'analysisBinding')
    object(value, ['candidateId', 'proposedWellPath', 'formations', 'expectedPaydirtM', 'fluidClasses',
      'contactPredictions', 'productionForecasts', 'uncertaintyAssumptions', 'citedEvidenceIds',
      'fieldPackageSha256', 'rationale', ...(bindingPresent ? ['analysisBinding'] : [])], 'Prediction')
    if (value.analysisBinding != null) validatePredictionBinding(value.analysisBinding)
    for (const key of ['candidateId', 'fieldPackageSha256', 'rationale']) string(value[key], key)
    for (const key of ['proposedWellPath', 'formations', 'fluidClasses', 'contactPredictions',
      'productionForecasts', 'uncertaintyAssumptions', 'citedEvidenceIds']) array(value[key], key)
    quantiles(value.expectedPaydirtM, 'expectedPaydirtM')
    array(value.proposedWellPath, 'proposedWellPath')
    value.proposedWellPath.forEach((station, i) => {
      const keys = ['measuredDepthM', 'trueVerticalDepthM', 'eastingM', 'northingM']
      object(station, keys, `proposedWellPath[${i}]`)
      for (const key of keys) number(station[key], `proposedWellPath[${i}].${key}`)
    })
    array(value.formations, 'formations')
    value.formations.forEach((formation, i) => {
      object(formation, ['formationName', 'topTrueVerticalDepthM', 'baseTrueVerticalDepthM'], `formations[${i}]`)
      string(formation.formationName, `formations[${i}].formationName`)
      quantiles(formation.topTrueVerticalDepthM, `formations[${i}].topTrueVerticalDepthM`)
      quantiles(formation.baseTrueVerticalDepthM, `formations[${i}].baseTrueVerticalDepthM`)
    })
    array(value.contactPredictions, 'contactPredictions')
    value.contactPredictions.forEach((contact, i) => {
      object(contact, ['contactType', 'trueVerticalDepthM'], `contactPredictions[${i}]`)
      if (typeof contact.contactType !== 'string' || !['GOC', 'GWC', 'OWC'].includes(contact.contactType)) throw new Error('Use GOC, GWC, or OWC for contactType.')
      quantiles(contact.trueVerticalDepthM, `contactPredictions[${i}].trueVerticalDepthM`)
    })
    array(value.productionForecasts, 'productionForecasts')
    value.productionForecasts.forEach((forecast, i) => {
      const keys = ['year', 'oilM3', 'gasM3', 'waterM3']
      object(forecast, keys, `productionForecasts[${i}]`)
      for (const key of keys) number(forecast[key], `productionForecasts[${i}].${key}`)
    })
    array(value.fluidClasses, 'fluidClasses')
    if (!value.fluidClasses.every((fluid) => typeof fluid === 'string' && ['Oil', 'Gas', 'Water'].includes(fluid))) throw new Error('Use Oil, Gas, or Water for fluidClasses.')
    for (const key of ['uncertaintyAssumptions', 'citedEvidenceIds']) {
      const items = value[key]
      array(items, key)
      items.forEach((item) => string(item, key))
    }
  }
  validate(body)
  if (body.proposedWellPath.length < 2) throw new Error('Supply at least two well-path stations.')
  for (const [index, station] of body.proposedWellPath.entries()) {
    if (station.measuredDepthM < 0 || station.trueVerticalDepthM < 0 ||
        index > 0 && station.measuredDepthM <= body.proposedWellPath[index - 1].measuredDepthM) {
      throw new Error('Well-path depths must be nonnegative, with strictly increasing measured depth.')
    }
  }
  const orderedQuantiles = (values: { p90: number; p50: number; p10: number }, label: string) => {
    if (values.p90 < 0 || values.p90 > values.p50 || values.p50 > values.p10) {
      throw new Error(`${label} must use nonnegative, increasing P90 ≤ P50 ≤ P10 values.`)
    }
  }
  orderedQuantiles(body.expectedPaydirtM, 'Expected thickness')
  if (!body.formations.length) throw new Error('Supply at least one formation forecast.')
  body.formations.forEach(formation => {
    orderedQuantiles(formation.topTrueVerticalDepthM, `${formation.formationName} top`)
    orderedQuantiles(formation.baseTrueVerticalDepthM, `${formation.formationName} base`)
    if ((['p90', 'p50', 'p10'] as const).some(key => formation.baseTrueVerticalDepthM[key] <= formation.topTrueVerticalDepthM[key])) {
      throw new Error(`${formation.formationName}: each formation base must be deeper than its corresponding top.`)
    }
  })
  body.contactPredictions.forEach(contact => orderedQuantiles(contact.trueVerticalDepthM, `${contact.contactType} contact`))
  if (!body.fluidClasses.length) throw new Error('Select at least one predicted fluid.')
  if (!body.uncertaintyAssumptions.length) throw new Error('Describe at least one uncertainty assumption.')
  if (!body.citedEvidenceIds.length) throw new Error('Cite the visible evidence supporting this prediction.')
  const forecasts = [...body.productionForecasts].sort((left, right) => left.year - right.year)
  if (![1, 3, 5].every(year => forecasts.some(forecast => forecast.year === year)) ||
      new Set(forecasts.map(forecast => forecast.year)).size !== forecasts.length) {
    throw new Error('Supply distinct cumulative production forecasts for years 1, 3 and 5.')
  }
  forecasts.forEach((forecast, index) => {
    if ((['oilM3', 'gasM3', 'waterM3'] as const).some(key => forecast[key] < 0 || index > 0 && forecast[key] < forecasts[index - 1][key])) {
      throw new Error('Cumulative production volumes must be nonnegative and must not decrease between forecast years.')
    }
  })
  validateBoundPredictionBody(body)
  return body
}
