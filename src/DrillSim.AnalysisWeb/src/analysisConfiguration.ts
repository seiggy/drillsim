import { visibleEvidenceIds } from './evidence'
import type { AnalysisConfiguration, AnalysisResult, FieldPackage, RankedCandidate, Scenario } from './types'

export const defaultConfiguration: AnalysisConfiguration = {
  version: 'analysis-configuration-v1',
  porosityCutoff: .12,
  permeabilityCutoffM2: 9.869233e-16,
  wellExclusionRadiusM: 500,
  gridPointsPerAxis: 15,
  idwNeighborCount: 4,
}

export const screeningControlLimitation =
  'Distances and exclusion radii use only located, logged screening-control summaries. Structural-only wells/bores and other known field wells are not all covered. These are not anti-collision checks; general field and common-datum validation remain incomplete.'

export function configurationKey(config: AnalysisConfiguration) {
  return JSON.stringify([
    config.version, config.porosityCutoff, config.permeabilityCutoffM2,
    config.wellExclusionRadiusM, config.gridPointsPerAxis, config.idwNeighborCount,
  ])
}

export function configurationErrors(config: AnalysisConfiguration) {
  const errors: string[] = []
  if (config.version !== defaultConfiguration.version) errors.push('Unsupported configuration version. Reload this client before editing.')
  if (!Number.isFinite(config.porosityCutoff) || config.porosityCutoff < 0 || config.porosityCutoff > 1) errors.push('Porosity must be between 0 and 1 (fraction).')
  if (!Number.isFinite(config.permeabilityCutoffM2) || config.permeabilityCutoffM2 <= 0 || config.permeabilityCutoffM2 > 1e-8) errors.push('Permeability must be greater than 0 and at most 1e-8 m².')
  if (!Number.isFinite(config.wellExclusionRadiusM) || config.wellExclusionRadiusM < 0 || config.wellExclusionRadiusM > 100000) errors.push('Screening-control exclusion radius must be between 0 and 100000 m.')
  if (!Number.isInteger(config.gridPointsPerAxis) || config.gridPointsPerAxis < 2 || config.gridPointsPerAxis > 51) errors.push('Grid points per axis must be an integer from 2 to 51.')
  if (!Number.isInteger(config.idwNeighborCount) || config.idwNeighborCount < 1 || config.idwNeighborCount > 32) errors.push('IDW neighbors must be an integer from 1 to 32.')
  return errors
}

export function analysisIsStale(analysis: AnalysisResult, pkg: FieldPackage, reservoir: string, draft: AnalysisConfiguration) {
  return analysis.fieldId !== pkg.fieldId || analysis.packageSha256 !== pkg.sha256 ||
    analysis.reservoirName?.toLowerCase() !== reservoir.toLowerCase() ||
    configurationKey(analysis.configuration ?? defaultConfiguration) !== configurationKey(draft)
}

export function verifyAppliedAnalysis(result: AnalysisResult, pkg: FieldPackage, reservoir: string, requested: AnalysisConfiguration) {
  if (!result.configuration || configurationErrors(result.configuration).length ||
      configurationKey(result.configuration) !== configurationKey(requested)) throw new Error('The server did not return the requested supported configuration. Previous output is retained.')
  if (!result.configurationSha256 || !result.analysisSha256 || !result.candidateGrid) throw new Error('The server did not return revision fingerprints and full grid output. Previous output is retained.')
  if (analysisIsStale(result, pkg, reservoir, requested)) throw new Error('The result belongs to different evidence or reservoir scope. Refresh evidence before applying again.')
}

export function predictionHandoffBlock(analysis: AnalysisResult, stale: boolean, applying: boolean) {
  if (applying) return 'Analysis is recalculating. Prediction handoff is blocked until a matching result is available.'
  if (stale) return 'Settings or evidence differ from this analysis result. Apply the pending settings or restore the result settings before prediction handoff.'
  if (analysis.configuration && configurationErrors(analysis.configuration).length) return 'The applied analysis configuration is unsupported.'
  return undefined
}

export function createEvidenceBundle(pkg: FieldPackage, analysis: AnalysisResult, candidate: RankedCandidate | undefined, reservoir: string, draft: AnalysisConfiguration, rationale: string, scenario?: Scenario) {
  if (analysisIsStale(analysis, pkg, reservoir, draft)) throw new Error('Apply pending settings before exporting a current bundle.')
  if (!analysis.configuration || configurationErrors(analysis.configuration).length ||
      !analysis.configurationSha256 || !analysis.analysisSha256) throw new Error('A supported, fingerprinted analysis result is required. Apply settings to create one.')
  const selected = analysis.ranking.find((item) => item.candidateId === candidate?.candidateId)
  if (!selected) throw new Error('Select a retained target before exporting.')
  if (!rationale.trim()) throw new Error('Enter the decision rationale before exporting.')
  const visible = visibleEvidenceIds(pkg)
  const cited = [...new Set([
    ...selected.neighborEvidenceIds,
    ...analysis.wellSummaries.filter((well) => selected.neighborEvidenceIds.includes(well.wellEvidenceId))
      .flatMap((well) => well.geologyEvidenceIds),
  ])]
  if (cited.some((id) => !visible.has(id))) throw new Error('A target citation is absent from the current visible package. Refresh evidence before exporting.')
  return {
    version: 'analysis-evidence-bundle-v1',
    scope: { fieldId: pkg.fieldId, reservoir, scenarioId: scenario?.scenarioId ?? null, asOfUtc: scenario?.asOfUtc ?? null },
    evidence: { packageSha256: pkg.sha256, generatedAt: pkg.generatedAt, sourceCounts: pkg.sourceCounts, citedEvidenceIds: cited },
    analysis: {
      modelVersion: analysis.modelVersion ?? null,
      configuration: analysis.configuration,
      configurationSha256: analysis.configurationSha256,
      analysisSha256: analysis.analysisSha256,
      methodology: analysis.methodology ?? null,
    },
    selectedTarget: selected,
    rationale: rationale.trim(),
    limitations: [...pkg.dataGaps, ...analysis.dataGaps,
      screeningControlLimitation,
      'Quantiles and score are uncalibrated rock-screening estimates, not reserves or economic hydrocarbon pay.',
      'Frontend fluid geometry is a separate view-only method; no simulation truth is included.',
      'This is a local JSON export, not a saved hypothesis, prediction seal, or evidence publication.'],
  }
}

export function downloadJson(value: unknown, name: string) {
  const url = URL.createObjectURL(new Blob([JSON.stringify(value, null, 2)], { type: 'application/json' }))
  const link = document.createElement('a')
  link.href = url
  link.download = name
  link.click()
  URL.revokeObjectURL(url)
}
