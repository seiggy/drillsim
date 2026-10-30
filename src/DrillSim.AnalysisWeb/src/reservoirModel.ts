import type { AnalysisConfiguration } from './types'

export interface ReservoirModelColumn {
  id: string
  eastingM: number
  northingM: number
  insideModel: boolean
  topTvdM: number
  baseTvdM: number
  grossM: number
  netRockM: number
  gasM: number
  oilM: number
  waterM: number
  netToGross: number
  porosity: number
  permeabilityMd: number
  fluidAtTop: 'gas' | 'oil' | 'water'
  nearestControlM: number
  nearestPropertyWellM: number
  propertyEvidenceIds: string[]
}

export interface FluidContactEstimate {
  type: 'GasOilContact' | 'OilWaterContact' | 'GasWaterContact'
  trueVerticalDepthM: number
  deepestAboveTvdM: number
  shallowestBelowTvdM: number
  supportingSamples: number
  misclassifiedSamples: number
  wells: number
}

export interface ReservoirModelWell {
  wellBoreId: string
  wellId: string | null
  name: string
  control: boolean
  logged: boolean
  path: Array<[number, number, number]>
  topTvdM: number | null
  baseTvdM: number | null
  pickEastingM: number | null
  pickNorthingM: number | null
}

export interface StaticReservoirModel {
  method: string
  fieldId: string
  reservoirName: string
  packageSha256: string
  configurationSha256: string
  grid: {
    minEastingM: number; minNorthingM: number; cellSizeM: number; columns: number; rows: number; layers: number; paddingM: number
    topTvdM: number[]; baseTvdM: number[]; netRockM: number[]; gasM: number[]; oilM: number[]; waterM: number[]; porosity: number[]
  }
  contacts: FluidContactEstimate[]
  volumes: { grossRockM3: number; netRockM3: number; gasNetRockM3: number; oilNetRockM3: number; hydrocarbonPoreVolumeM3: number }
  inputs: { structuralControls: number; propertyWells: number; fluidWells: number; classifiedSamples: number; medianGrossM: number; hydrocarbon: string }
  wells: ReservoirModelWell[]
  targets: ReservoirModelColumn[]
  limitations: string[]
}

export type ReservoirModelReadout = Pick<StaticReservoirModel, 'method' | 'packageSha256' | 'contacts' | 'inputs' | 'limitations'> & { columns: ReservoirModelColumn[] }
type Scope = { scenarioId: string; asOf: string }
type Target = { id: string; eastingM: number; northingM: number }

export const contactLabel: Record<FluidContactEstimate['type'], string> = {
  GasOilContact: 'Gas–oil contact', OilWaterContact: 'Oil–water contact', GasWaterContact: 'Gas–water contact',
}
export const contactCode = { GasOilContact: 'GOC', OilWaterContact: 'OWC', GasWaterContact: 'GWC' } as const

async function post<T>(path: string, fieldId: string, reservoir: string, configuration: AnalysisConfiguration,
  targets: Target[], scope?: Scope, signal?: AbortSignal): Promise<T> {
  const response = await fetch(`/analysis-api/api/fields/${encodeURIComponent(fieldId)}/${path}`, {
    method: 'POST', headers: { 'Content-Type': 'application/json' }, signal,
    body: JSON.stringify({ configuration, reservoir, targets, ...(scope ? { scenarioId: scope.scenarioId, asOf: scope.asOf } : {}) }),
  })
  if (!response.ok) throw new Error(`${response.status}: the reservoir model could not be built.`)
  return response.json() as Promise<T>
}

export const getReservoirModel = (fieldId: string, reservoir: string, configuration: AnalysisConfiguration,
  targets: Target[], scope?: Scope, signal?: AbortSignal) =>
  post<StaticReservoirModel>('reservoir-model', fieldId, reservoir, configuration, targets, scope, signal)

export const getReservoirColumns = (fieldId: string, reservoir: string, configuration: AnalysisConfiguration,
  targets: Target[], scope?: Scope, signal?: AbortSignal) =>
  post<ReservoirModelReadout>('reservoir-model/columns', fieldId, reservoir, configuration, targets, scope, signal)

/** The model's pay for the configured target fluid: oil or gas thickness, or all good rock when no fluid is targeted. */
export const modelPay = (column: ReservoirModelColumn, configuration?: AnalysisConfiguration) =>
  configuration?.targetFluid === 'Oil' ? column.oilM : configuration?.targetFluid === 'Gas' ? column.gasM : column.netRockM
