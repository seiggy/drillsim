import type { PredictionBody } from './types'
import { createPredictionBinding, type PredictionSourceContext } from './predictionBinding'
import { visibleEvidenceIds } from './evidence'
import { modelPay, type ReservoirModelReadout } from './reservoirModel'

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
    1: { oilM3: 10000, gasM3: 250000000, waterM3: 0 },
    3: { oilM3: 35000, gasM3: 650000000, waterM3: 1000 },
    5: { oilM3: 60000, gasM3: 950000000, waterM3: 5000 },
  }
  body.productionForecasts = body.productionForecasts.map(forecast => {
    const example = forecast.year === null ? undefined : forecasts[forecast.year]
    return example ? { ...forecast, oilM3: forecast.oilM3 ?? example.oilM3, gasM3: forecast.gasM3 ?? example.gasM3, waterM3: forecast.waterM3 ?? example.waterM3 } : forecast
  })
  body.candidateId = binding.target.candidateId
  body.expectedPaydirtM = fillQuantiles(body.expectedPaydirtM, [binding.target.expectedPaydirtM.p90, binding.target.expectedPaydirtM.p50, binding.target.expectedPaydirtM.p10])
  body.fieldPackageSha256 = context.fieldPackage.sha256
  body.analysisBinding = binding
  body.citedEvidenceIds = [...new Set([...body.citedEvidenceIds, ...context.candidate!.neighborEvidenceIds])]
  if (!body.rationale.trim()) body.rationale = `Demonstration forecast at ${binding.target.candidateId}; compare the frozen-source rock-screening estimate with a synthetic well. Review the example assumptions before saving.`
  if (!body.uncertaintyAssumptions.includes(demoForecastAssumption)) body.uncertaintyAssumptions.push(demoForecastAssumption)
  const payAssumption = 'Expected pay is copied exactly from the verified analysis as an uncalibrated qualifying-rock proxy, not fluid-conditioned pay or reserves.'
  if (!body.uncertaintyAssumptions.includes(payAssumption)) body.uncertaintyAssumptions.push(payAssumption)
  return JSON.stringify(body, null, 2)
}

export const reservoirModelAssumption = 'Depths, net pay, fluids and contacts come from the static reservoir model (inverse-distance surfaces, layer rock properties from logs, flat fitted contacts); uncalibrated, not reserves.'
export const productionPlanAssumption = 'Production: the published operating plan (continuous 5-year flow, 15 MPa bottom-hole pressure, at most 4,320 reservoir m3/day) split between gas and oil by the model reservoir hydrocarbon thickness and converted to surface m3 (Bg 0.00515, Bo 1.12); plateau then decline factors 1, 2.9 and 4.1 years of plateau rate at years 1, 3 and 5. No flow model; water assumed nil.'
// Mirrors DrillingOperations production-calibration-default-v3 and ProductionSurfaceConversion.Troll.
const plan = { reservoirRateM3PerYear: .05 * 365.25 * 86400, gasFvf: .00515, oilFvf: 1.12, cumulativeYears: { 1: 1, 3: 2.9, 5: 4.1 } as Record<number, number> }

/** Fills the form from the reservoir model column at the target, and production from the published operating plan. */
export function fillFromReservoirModel(text: string, context: PredictionSourceContext, readout: ReservoirModelReadout, maximumTotalDepthM?: number): string {
  const column = readout.columns.find(item => item.id === context.candidate?.candidateId)
  if (!column) throw new Error('The reservoir model has no reading at this target. Fill the values yourself.')
  const body = JSON.parse(fillDemoForecast(text, context)) as PredictionFormDocument
  const round = (value: number) => Math.round(value * 10) / 10
  const range = (p50: number, spread: number) => ({ p90: round(Math.max(0, p50 - spread)), p50: round(p50), p10: round(p50 + spread) })
  // Same spread rule as the structural estimate: 2% of distance to the nearest pick, at the P90/P10 z-score.
  const depthSpread = Math.max(5, 1.2816 * .02 * column.nearestControlM)
  const top = range(column.topTvdM, depthSpread), base = range(column.baseTvdM, depthSpread)
  const pay = modelPay(column, context.analysis.configuration)
  const paySpread = 1.2816 * (context.candidate!.relativeUncertainty || .2) * pay
  body.expectedPaydirtM = { p90: round(Math.max(0, pay - paySpread)), p50: round(pay),
    p10: round(Math.max(pay, Math.min(pay + paySpread, base.p10 - top.p90))) }
  body.formations = body.formations.map(formation => formation.formationName === context.scenario.reservoirName
    ? { ...formation, topTrueVerticalDepthM: top, baseTrueVerticalDepthM: base } : formation)
  const td = round(Math.min(maximumTotalDepthM ?? Infinity, base.p10 + 50))
  body.proposedWellPath = body.proposedWellPath.map((station, index) => index === body.proposedWellPath.length - 1
    ? { ...station, measuredDepthM: td, trueVerticalDepthM: td } : station)
  body.fluidClasses = (['Oil', 'Gas', 'Water'] as const).filter(fluid =>
    fluid === 'Oil' ? column.oilM > 0 : fluid === 'Gas' ? column.gasM > 0 : column.waterM > 0 || column.gasM + column.oilM === 0)
  const codes = { GasOilContact: 'GOC', GasWaterContact: 'GWC', OilWaterContact: 'OWC' } as const
  body.contactPredictions = readout.contacts.map(contact => ({ contactType: codes[contact.type],
    trueVerticalDepthM: range(contact.trueVerticalDepthM, Math.max(5, contact.shallowestBelowTvdM - contact.deepestAboveTvdM)) }))
  const lower = readout.contacts.find(contact => contact.type !== 'GasOilContact')
  body.rationale = `Reservoir model (${readout.method}) at target ${context.candidate!.rank || 1}: top ${top.p50} m and base ${base.p50} m TVD; ` +
    `${round(column.netRockM)} m of rock passes the cutoffs (${round(column.gasM)} m gas, ${round(column.oilM)} m oil) above ` +
    (lower ? `the ${codes[lower.type]} at ${lower.trueVerticalDepthM} m, fitted to ${lower.supportingSamples + lower.misclassifiedSamples} log readings in ${lower.wells} wells. ` : 'the deepest observed hydrocarbon. ') +
    `Forecast pay is the ${context.analysis.configuration?.targetFluid?.toLowerCase() ?? "good-rock"} thickness, ${round(pay)} m; the screening estimate is ${round(context.candidate!.p50NetPayM)} m. Review before locking.`
  const payAssumption = 'Expected pay is copied exactly from the verified analysis as an uncalibrated qualifying-rock proxy, not fluid-conditioned pay or reserves.'
  const hydrocarbon = column.gasM + column.oilM
  const gasShare = hydrocarbon > 0 ? column.gasM / hydrocarbon : 0, oilShare = hydrocarbon > 0 ? column.oilM / hydrocarbon : 0
  body.productionForecasts = body.productionForecasts.map(row => {
    const years = row.year === null ? undefined : plan.cumulativeYears[row.year]
    if (years === undefined) return row
    const reservoir = plan.reservoirRateM3PerYear * years
    return { ...row, gasM3: Math.round(reservoir * gasShare / plan.gasFvf), oilM3: Math.round(reservoir * oilShare / plan.oilFvf), waterM3: 0 }
  })
  body.uncertaintyAssumptions = [...body.uncertaintyAssumptions.filter(item => item !== payAssumption && item !== reservoirModelAssumption &&
    item !== productionPlanAssumption && item !== demoForecastAssumption), reservoirModelAssumption, productionPlanAssumption]
  body.citedEvidenceIds = [...new Set([...body.citedEvidenceIds, ...column.propertyEvidenceIds])]
  return JSON.stringify(body, null, 2)
}