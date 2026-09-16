import type { OperatorView } from './operator'

export type JsonRecord = Record<string, unknown>

export interface FieldSummary {
  id: string
  name: string
  description?: string
}

export interface FieldPackage {
  generatedAt: string
  fieldId: string
  sha256: string
  field: JsonRecord
  clusters: JsonRecord[]
  wells: JsonRecord[]
  wellBores: JsonRecord[]
  wellBoreArchitectures: JsonRecord[]
  trajectories: JsonRecord[]
  geologicalProperties: JsonRecord[]
  sourceCounts: {
    fields: number
    clusters: number
    wells: number
    wellBores: number
    wellBoreArchitectures: number
    trajectories: number
    geologicalProperties: number
  }
  dataGaps: string[]
}

export interface WellSummary {
  wellId: string
  wellEvidenceId: string
  geologyEvidenceIds: string[]
  netPayThicknessM: number
  meanPayPorosity: number
  meanPayPermeabilityM2: number
  eastingM: number
  northingM: number
  geologicalResults?: Array<{
    evidenceId: string
    netPayThicknessM: number
    meanPayPorosity: number
    meanPayPermeabilityM2: number
    qualifyingSampleCount: number
  }>
}

export interface AnalysisConfiguration {
  version: string
  porosityCutoff: number
  permeabilityCutoffM2: number
  wellExclusionRadiusM: number
  gridPointsPerAxis: number
  idwNeighborCount: number
}

export interface RankedCandidate {
  rank: number
  candidateId: string
  eastingM: number
  northingM: number
  longitudeDegrees: number
  latitudeDegrees: number
  nearestWellDistanceM: number
  p90NetPayM: number
  p50NetPayM: number
  p10NetPayM: number
  sigmaNetPayM: number
  meanPayPorosity: number
  meanPayPermeabilityMd: number
  relativeUncertainty: number
  score: number
  neighborEvidenceIds: string[]
  scoreComponents?: {
    p50NetPayM: number
    porosityFactor: number
    permeabilityMd: number
    permeabilityLogFactor: number
    unpenalizedScore: number
    uncertaintyDivisor: number
  } | null
  uncertaintyComponents?: {
    disagreementVarianceM2: number
    weightedDistanceM: number
    gridDiagonalM: number
    distanceSigmaM: number
    sigmaNetPayM: number
    quantileZScore: number
    calibrated: boolean
  } | null
}

export interface CandidateGridPoint {
  candidateId: string
  eastingM: number
  northingM: number
  longitudeDegrees: number
  latitudeDegrees: number
  status: string
  reasons: string[]
  nearestWellDistanceM?: number | null
  prediction?: RankedCandidate | null
}

export interface AnalysisResult {
  generatedAt: string
  fieldId: string
  reservoirName?: string
  packageSha256: string
  wellSummaries: WellSummary[]
  ranking: RankedCandidate[]
  dataGaps: string[]
  modelVersion?: string
  configuration?: AnalysisConfiguration
  configurationSha256?: string
  analysisSha256?: string
  candidateGridBounds?: { minEastingM: number; maxEastingM: number; minNorthingM: number; maxNorthingM: number } | null
  candidateGrid?: CandidateGridPoint[]
  methodology?: {
    label: string
    porosityCutoff: number
    permeabilityCutoffM2: number
    pressureDifferentialRule: string
    netPayThicknessMethod: string
    candidateGridMethod: string
    idwNeighborCount: number
    wellExclusionRadiusM: number
    quantileZScore: number
    uncertaintyMethod: string
    scoreFormula: string
  }
}

export interface AgentStatus {
  configured: boolean
  provider?: string
  deployment?: string
}

export interface MapsStatus {
  configured: boolean
  clientId?: string
}

export interface AgentRunState {
  running: boolean
  output: string
  error: string
}

export type ScenarioStatus =
  | 'Draft'
  | 'Armed'
  | 'PredictionDrafted'
  | 'PredictionSealed'
  | 'HumanApproved'
  | 'WorldBound'
  | 'Queued'
  | 'Drilling'
  | 'Surveying'
  | 'Logging'
  | 'CompletionDesigned'
  | 'Producing'
  | 'ReadyToReveal'
  | 'Revealed'
  | 'Scored'
  | 'Cancelled'
  | 'Failed'
  | 'PublishFailed'
  | 'Reset'

export interface Scenario {
  scenarioId: string
  sourceFieldId: string
  clonedFieldId?: string
  reservoirName: string
  initialAsOfUtc: string
  asOfUtc: string
  seedLabel: string
  worldModelVersion: string
  observationModelVersion: string
  scoringModelVersion: string
  status: ScenarioStatus
  assumptionsSha256: string
  createdUtc: string
  modifiedUtc: string
}

export interface EvidenceVisibilityCount {
  recordKind: string
  status: 'Visible' | 'Hidden'
  count: number
  evidenceIds?: string[]
}

export interface EvidenceVisibilitySummary {
  scenarioId: string
  asOfUtc: string
  counts: EvidenceVisibilityCount[]
}

export interface QuantileValues {
  p90: number
  p50: number
  p10: number
}

export interface ProposedWellPathStation {
  measuredDepthM: number
  trueVerticalDepthM: number
  eastingM: number
  northingM: number
}

export interface FormationPrediction {
  formationName: string
  topTrueVerticalDepthM: QuantileValues
  baseTrueVerticalDepthM: QuantileValues
}

export interface FluidContactPrediction {
  contactType: 'GOC' | 'GWC' | 'OWC'
  trueVerticalDepthM: QuantileValues
}

export interface ProductionForecast {
  year: number
  oilM3: number
  gasM3: number
  waterM3: number
}

export interface PredictionCapabilities {
  version: string
  configuredSaveSupported: boolean
  configuredSealSupported: boolean
  bindingVersion: string
  analysisModelVersion: string
  configurationVersion: string
  baselineBindingVersion: string
  baselineModelVersions: string[]
  sourceScope: string
  targetRule: string
  minimumLocatedControls: number
  fourNeighborBaselineRule: string
  limitation: string
  legacyNullBindingSupported: boolean
}

export interface PredictionAnalysisTarget {
  candidateId: string
  eastingM: number
  northingM: number
  expectedPaydirtM: QuantileValues
}
export interface PredictionAnalysisBinding {
  configuration: AnalysisConfiguration
  configurationSha256: string
  analysisSha256: string
  version?: string | null
  modelVersion?: string | null
  scenarioId?: string | null
  fieldId?: string | null
  reservoirName?: string | null
  asOfUtc?: string | null
  target?: PredictionAnalysisTarget | null
}
export interface VersionedPredictionAnalysisBinding extends PredictionAnalysisBinding {
  version: 'prediction-analysis-binding-v1'
  modelVersion: 'petrophysics-screening-v1'
  scenarioId: string
  fieldId: string
  reservoirName: string
  asOfUtc: string
  target: PredictionAnalysisTarget
}

export interface PredictionBody {
  candidateId: string
  proposedWellPath: ProposedWellPathStation[]
  formations: FormationPrediction[]
  expectedPaydirtM: QuantileValues
  fluidClasses: Array<'Oil' | 'Gas' | 'Water'>
  contactPredictions: FluidContactPrediction[]
  productionForecasts: ProductionForecast[]
  uncertaintyAssumptions: string[]
  citedEvidenceIds: string[]
  fieldPackageSha256: string
  rationale: string
  analysisBinding?: PredictionAnalysisBinding | null
}

export interface PredictionSeal {
  sha256: string
  baselinesSha256: string
  sealedUtc: string
}

export interface PredictionApproval {
  actor: string
  approvedUtc: string
  sealedSha256: string
}

export interface BaselineSnapshot {
  baselineId: string
  contentSha256: string
  scenarioId: string
  kind: 'NearestWell' | 'FieldMean' | 'FourNeighborIdw' | 'UncertaintyAwareRank1'
  modelVersion: string
  packageSha256: string
  candidateId: string
  targetEastingM: number
  targetNorthingM: number
  contributingEvidenceIds: string[]
  formationTopTrueVerticalDepthM?: QuantileValues
  formationBaseTrueVerticalDepthM?: QuantileValues
  expectedPaydirtM?: QuantileValues
  fluidClasses?: Array<'Oil' | 'Gas' | 'Water'>
  contactPredictions?: FluidContactPrediction[]
  productionForecasts?: ProductionForecast[]
  limitation: string
  analysisBinding?: {
    version: string
    modelVersion: string
    configuration: AnalysisConfiguration
    configurationSha256: string
    analysisSha256: string
    predictionAnalysisSha256: string
  } | null
}

export interface PredictionRecord {
  scenarioId: string
  body: PredictionBody
  revision: number
  createdUtc: string
  modifiedUtc: string
  seal?: PredictionSeal
  approval?: PredictionApproval
  baselines: BaselineSnapshot[]
}

export interface RevealReceipt {
  scenarioId: string
  revealId: string
  clonedFieldId: string
  asOfUtc: string
  status: 'Prepared' | 'Revealed'
  manifestSha256: string
  evidenceCount: number
  productionSeriesId: string
}

export interface PublicProductionSeriesMetadata {
  scenarioId: string
  revealId: string
  seriesId: string
  modelVersion: string
  contentSha256: string
  monthCount: number
  checkpointYears: number[]
  createdUtc: string
}

export interface ScorecardMetric {
  name: string
  basis: 'HiddenTruth' | 'RevealedObservation' | 'ObservationGap' | 'Baseline'
  status: 'Scored' | 'Unavailable'
  value?: number | null
  unit?: string | null
  lowerBound?: number | null
  upperBound?: number | null
  limitation?: string | null
}

export interface PublicScorecard {
  scenarioId: string
  scorecardId: string
  revealId: string
  scoringModelVersion: string
  inputSha256: string
  headlineMetric?: string | null
  metrics: ScorecardMetric[]
  createdValidTimeUtc: string
  limitation?: string | null
  contentSha256: string
}

export interface ScenarioEvidence {
  initial: EvidenceVisibilitySummary
  current: EvidenceVisibilitySummary
}

export interface ScenarioResources {
  scenarioId?: string
  loading: boolean
  error: string
  evidence?: ScenarioEvidence
  prediction?: PredictionRecord
  reveal?: RevealReceipt
  production?: PublicProductionSeriesMetadata
  scorecard?: PublicScorecard
  operator?: OperatorView
  operatorSetup?: import('./simulatorSetup').SimulatorSetup
}
