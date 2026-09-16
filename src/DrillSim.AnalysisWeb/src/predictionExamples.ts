import type { PredictionBody } from './types'
import { createPredictionBinding, type PredictionSourceContext } from './predictionBinding'
import { visibleEvidenceIds } from './evidence'

type DraftNumbers<T> = T extends number ? number | null
  : T extends Array<infer Item> ? Array<DraftNumbers<Item>>
  : T extends object ? { [Key in keyof T]: DraftNumbers<T[Key]> } : T
export type PredictionFormDocument = DraftNumbers<PredictionBody>

export const demoForecastVersion = 'editable-demo-forecast-v1'
export const demoForecastAssumption = `DEMONSTRATION ESTIMATES (${demoForecastVersion}): depth, formation, fluid and cumulative production examples are editable assumptions, not inferred truth or calibration.`

export function readPredictionForm(text: string): PredictionFormDocument | undefined {
  try {
    const value = JSON.parse(text) as PredictionFormDocument
    const numeric = (number: unknown) => number === null || typeof number === 'number' && Number.isFinite(number)
    const quantiles = (q: unknown) => Boolean(q && typeof q === 'object' &&
      ['p90', 'p50', 'p10'].every(key => numeric((q as Record<string, unknown>)[key])))
    if (!value || typeof value !== 'object' || typeof value.candidateId !== 'string' || typeof value.rationale !== 'string' ||
        typeof value.fieldPackageSha256 !== 'string' || !quantiles(value.expectedPaydirtM) ||
        !Array.isArray(value.proposedWellPath) || !value.proposedWellPath.every(station => station &&
          ['measuredDepthM', 'trueVerticalDepthM', 'eastingM', 'northingM'].every(key => numeric(station[key as keyof typeof station]))) ||
        !Array.isArray(value.formations) || !value.formations.every(formation => formation && typeof formation.formationName === 'string' &&
          quantiles(formation.topTrueVerticalDepthM) && quantiles(formation.baseTrueVerticalDepthM)) ||
        !Array.isArray(value.contactPredictions) || !value.contactPredictions.every(contact => contact &&
          ['GOC', 'GWC', 'OWC'].includes(contact.contactType) && quantiles(contact.trueVerticalDepthM)) ||
        !Array.isArray(value.fluidClasses) || !value.fluidClasses.every(fluid => ['Oil', 'Gas', 'Water'].includes(fluid)) ||
        !Array.isArray(value.productionForecasts) || !value.productionForecasts.every(forecast => forecast &&
          ['year', 'oilM3', 'gasM3', 'waterM3'].every(key => numeric(forecast[key as keyof typeof forecast]))) ||
        !Array.isArray(value.uncertaintyAssumptions) || !value.uncertaintyAssumptions.every(item => typeof item === 'string') ||
        !Array.isArray(value.citedEvidenceIds) || !value.citedEvidenceIds.every(item => typeof item === 'string')) return undefined
    return value
  } catch { return undefined }
}

export function fillDemoForecast(text: string, context: PredictionSourceContext): string {
  const binding = createPredictionBinding(context)
  const body = readPredictionForm(text)
  if (!body) throw new Error('This JSON cannot be edited with the guided form. Preserve it and use Advanced, or explicitly start a new point-screening template.')
  if (body.analysisBinding && body.analysisBinding.version !== binding.version) {
    throw new Error('Rebuild the unsupported binding explicitly before filling examples. Its metadata will not be silently upgraded.')
  }
  if (body.candidateId && body.candidateId !== binding.target.candidateId ||
      body.proposedWellPath.some(station =>
        station.eastingM !== null && station.eastingM !== binding.target.eastingM ||
        station.northingM !== null && station.northingM !== binding.target.northingM)) {
    throw new Error('The existing target or path differs from the selected point. Examples never flatten a directional path. Preserve this draft, or explicitly choose “Start new point-screening template” before using a different target.')
  }
  if (body.analysisBinding && (body.analysisBinding.analysisSha256 !== binding.analysisSha256 ||
      body.analysisBinding.configurationSha256 !== binding.configurationSha256)) {
    throw new Error('The saved analysis binding differs from these settings. Rebuild and review it explicitly before filling examples.')
  }
  if ((['p90', 'p50', 'p10'] as const).some(key => body.expectedPaydirtM[key] !== null &&
      body.expectedPaydirtM[key] !== binding.target.expectedPaydirtM[key])) {
    throw new Error('Existing pay estimates differ from the verified target. Review an explicit binding rebuild; examples will not overwrite these values.')
  }
  const visible = visibleEvidenceIds(context.fieldPackage)
  if (binding.target && context.candidate!.neighborEvidenceIds.some(id => !visible.has(id))) {
    throw new Error('The selected analysis cites evidence outside the frozen package. Reanalyze the earlier measurements; no examples were applied.')
  }
  const fillQuantiles = (current: { p90: number | null; p50: number | null; p10: number | null }, examples: number[]) =>
    ({ ...current, p90: current.p90 ?? examples[0], p50: current.p50 ?? examples[1], p10: current.p10 ?? examples[2] })
  body.proposedWellPath = body.proposedWellPath.map((station, index) => ({
    ...station,
    measuredDepthM: station.measuredDepthM ?? (index === 0 ? 0 : index === body.proposedWellPath.length - 1 ? 2200 : null),
    trueVerticalDepthM: station.trueVerticalDepthM ?? (index === 0 ? 0 : index === body.proposedWellPath.length - 1 ? 2200 : null),
    eastingM: station.eastingM ?? binding.target.eastingM,
    northingM: station.northingM ?? binding.target.northingM,
  }))
  body.formations = body.formations.map(formation => formation.formationName === context.scenario.reservoirName ? {
    ...formation,
    topTrueVerticalDepthM: fillQuantiles(formation.topTrueVerticalDepthM, [1400, 1450, 1500]),
    baseTrueVerticalDepthM: fillQuantiles(formation.baseTrueVerticalDepthM, [1600, 1650, 1700]),
  } : formation)
  if (!body.fluidClasses.length) body.fluidClasses = ['Oil', 'Gas', 'Water']
  const forecasts: Record<number, { oilM3: number; gasM3: number; waterM3: number }> = {
    1: { oilM3: 12000, gasM3: 1200000, waterM3: 3000 },
    3: { oilM3: 30000, gasM3: 2800000, waterM3: 15000 },
    5: { oilM3: 45000, gasM3: 4000000, waterM3: 36000 },
  }
  body.productionForecasts = body.productionForecasts.map(forecast => {
    const example = forecast.year === null ? undefined : forecasts[forecast.year]
    return example ? { ...forecast, oilM3: forecast.oilM3 ?? example.oilM3, gasM3: forecast.gasM3 ?? example.gasM3, waterM3: forecast.waterM3 ?? example.waterM3 } : forecast
  })
  body.candidateId = binding.target.candidateId
  body.expectedPaydirtM = { ...binding.target.expectedPaydirtM }
  body.fieldPackageSha256 = context.fieldPackage.sha256
  body.analysisBinding = binding
  body.citedEvidenceIds = [...new Set([...body.citedEvidenceIds, ...context.candidate!.neighborEvidenceIds])]
  if (!body.rationale.trim()) body.rationale = `Demonstration forecast at ${binding.target.candidateId}; compare the frozen-source rock-screening estimate with a synthetic well. Review the example assumptions before saving.`
  if (!body.uncertaintyAssumptions.includes(demoForecastAssumption)) body.uncertaintyAssumptions.push(demoForecastAssumption)
  const payAssumption = 'Expected pay is copied exactly from the verified analysis as an uncalibrated qualifying-rock proxy, not fluid-conditioned pay or reserves.'
  if (!body.uncertaintyAssumptions.includes(payAssumption)) body.uncertaintyAssumptions.push(payAssumption)
  return JSON.stringify(body, null, 2)
}
