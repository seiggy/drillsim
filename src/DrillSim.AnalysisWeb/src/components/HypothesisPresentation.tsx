import { defaultConfiguration, downloadJson } from '../analysisConfiguration'
import type { HypothesisChallenge, HypothesisInput, HypothesisRevision } from '../hypotheses'
import type { AnalysisConfiguration } from '../types'

const fieldLabels: Record<string, string> = {
  configuration: 'Analysis settings', version: 'Version', porosityCutoff: 'Porosity cutoff',
  permeabilityCutoffM2: 'Permeability cutoff', wellExclusionRadiusM: 'Screening-control exclusion radius',
  gridPointsPerAxis: 'Grid', idwNeighborCount: 'Neighbor count',
  scope: 'Evidence scope', fieldId: 'Field reference', reservoirName: 'Reservoir',
  scenarioId: 'Scenario reference', asOfUtc: 'Evidence time', source: 'Branch origin',
  hypothesisId: 'Hypothesis reference', revision: 'Revision', input: 'Captured hypothesis',
  name: 'Hypothesis name', selectedCandidateId: 'Selected target reference', candidateId: 'Target reference',
  rationale: 'Rationale', correlationNotes: 'Correlation notes', controlNotes: 'Control notes',
  evidenceId: 'Evidence reference', note: 'Note', geology: 'Geology record references',
  packageSha256: 'Package fingerprint', analysisSha256: 'Result fingerprint', configurationSha256: 'Settings fingerprint',
  summary: 'Summary', actor: 'Audit label', objections: 'Objections', text: 'Objection',
  citedEvidenceIds: 'Cited evidence', dispositions: 'Disposition changes', objectionId: 'Objection reference',
  disposition: 'Disposition', reason: 'Reason',
  rank: 'Rank', eastingM: 'Easting', northingM: 'Northing', longitudeDegrees: 'Longitude', latitudeDegrees: 'Latitude',
  nearestWellDistanceM: 'Nearest screening-control distance', p90NetPayM: 'P90 qualifying rock',
  p50NetPayM: 'P50 qualifying rock', p10NetPayM: 'P10 qualifying rock', sigmaNetPayM: 'Rock-proxy standard deviation',
  meanPayPorosity: 'Mean qualifying-rock porosity', meanPayPermeabilityMd: 'Mean qualifying-rock permeability',
  relativeUncertainty: 'Relative uncertainty', score: 'Screening score', neighborEvidenceIds: 'Neighbor evidence',
  scoreComponents: 'Score components', uncertaintyComponents: 'Uncertainty components',
  porosityFactor: 'Porosity factor', permeabilityMd: 'Permeability', permeabilityLogFactor: 'Permeability log factor',
  unpenalizedScore: 'Unpenalized score', uncertaintyDivisor: 'Uncertainty divisor',
  disagreementVarianceM2: 'Control disagreement variance', weightedDistanceM: 'Weighted control distance',
  gridDiagonalM: 'Grid diagonal', distanceSigmaM: 'Distance uncertainty', quantileZScore: 'Quantile multiplier',
  calibrated: 'Calibrated',
}
const units: Record<string, string> = {
  porosityCutoff: 'fraction', permeabilityCutoffM2: 'm²', wellExclusionRadiusM: 'm',
  eastingM: 'm', northingM: 'm', longitudeDegrees: '°', latitudeDegrees: '°',
  nearestWellDistanceM: 'm', p90NetPayM: 'm', p50NetPayM: 'm', p10NetPayM: 'm', sigmaNetPayM: 'm',
  meanPayPorosity: 'fraction', meanPayPermeabilityMd: 'mD', relativeUncertainty: 'fraction',
  permeabilityMd: 'mD', disagreementVarianceM2: 'm²', weightedDistanceM: 'm', gridDiagonalM: 'm', distanceSigmaM: 'm',
}
const leafOf = (field: string) => field.split('.').at(-1) ?? field
export function hypothesisFieldLabel(field: string) {
  const leaf = leafOf(field)
  return Object.hasOwn(fieldLabels, leaf) ? fieldLabels[leaf]
    : leaf.replace(/([a-z0-9])([A-Z])/g, '$1 $2').replace(/[-_]/g, ' ').toLowerCase().replace(/^./, letter => letter.toUpperCase())
}

export function RecordedValue({ field, value, downloadLabel, depth = 0 }: {
  field: string; value: unknown; downloadLabel: string; depth?: number
}) {
  const leaf = leafOf(field)
  if (value === null || value === undefined) {
    return <span>{value === null && leaf === 'scenarioId' ? 'Live evidence (no scenario)'
      : value === null && leaf === 'asOfUtc' ? 'Live service time' : 'Not recorded'}</span>
  }
  if (typeof value === 'number') return <span>{!Number.isFinite(value) ? 'Unavailable'
    : leaf === 'gridPointsPerAxis' ? `${value} × ${value} points` : `${value}${Object.hasOwn(units, leaf) ? ` ${units[leaf]}` : ''}`}</span>
  if (typeof value === 'boolean') return <span>{value ? 'Yes' : 'No'}</span>
  if (typeof value === 'string') return /(?:Ids?|Sha256)$/.test(leaf)
    ? <code>{value || 'Not recorded'}</code> : <span style={{ whiteSpace: 'pre-wrap' }}>{value || 'Not recorded'}</span>
  if (Array.isArray(value)) return value.length
    ? <ul>{value.map((item, index) => <li key={index}><RecordedValue field={field} value={item} downloadLabel={downloadLabel} depth={depth} /></li>)}</ul>
    : <span>None recorded</span>
  if (typeof value === 'object') {
    const entries = Object.entries(value)
    if (!entries.length) return <span>No fields recorded</span>
    if (depth >= 3) return <span>{entries.length} recorded fields: {entries.map(([key]) => hypothesisFieldLabel(key)).join(', ')}.
      {' '}Full values: {downloadLabel}, under <code>{field}</code>.</span>
    return <dl>{entries.map(([key, item]) => <div key={key}>
      <dt>{hypothesisFieldLabel(key)}{!Object.hasOwn(fieldLabels, key) && <small> · additional recorded field <code>{key}</code></small>}</dt>
      <dd><RecordedValue field={`${field}.${key}`} value={item} downloadLabel={downloadLabel} depth={depth + 1} /></dd>
    </div>)}</dl>
  }
  return <span>See {downloadLabel} for this recorded value.</span>
}

export function AnalysisSettingsSummary({ configuration, downloadLabel, compact = false }: {
  configuration: AnalysisConfiguration; downloadLabel: string; compact?: boolean
}) {
  const fields = ['porosityCutoff', 'permeabilityCutoffM2', 'wellExclusionRadiusM', 'gridPointsPerAxis', 'idwNeighborCount', 'version'] as const
  const additional = Object.fromEntries(Object.entries(configuration).filter(([key]) => !fields.some(field => field === key)))
  return <>
    <dl className="dense-readout" style={compact ? { gridTemplateColumns: '1fr' } : undefined}>{fields.map(field => <div key={field}><dt>{hypothesisFieldLabel(field)}</dt>
      <dd><RecordedValue field={field} value={configuration[field]} downloadLabel={downloadLabel} /></dd></div>)}</dl>
    {configuration.version !== defaultConfiguration.version && <p className="task-notice">This recorded settings version is not supported by this editor.</p>}
    {Object.keys(additional).length > 0 && <details><summary>Additional recorded settings</summary>
      <RecordedValue field="configuration" value={additional} downloadLabel={downloadLabel} />
    </details>}
  </>
}

export function SavedHypothesisInput({ input }: { input: HypothesisInput }) {
  const additional = Object.fromEntries(Object.entries(input).filter(([key]) =>
    !['configuration', 'packageSha256', 'analysisSha256', 'selectedCandidateId', 'rationale', 'correlationNotes', 'controlNotes'].includes(key)))
  return <>
    <AnalysisSettingsSummary configuration={input.configuration} downloadLabel="Download exact snapshot JSON" />
    <p>Selected target reference: <code>{input.selectedCandidateId}</code></p>
    <h4>Rationale</h4><p style={{ whiteSpace: 'pre-wrap' }}>{input.rationale || 'No rationale recorded.'}</p>
    <h4>Correlation notes</h4><p style={{ whiteSpace: 'pre-wrap' }}>{input.correlationNotes || 'No correlation notes recorded.'}</p>
    <h4>Control notes</h4>
    {input.controlNotes?.length ? <ul>{input.controlNotes.map((note, index) => <li key={index}>
      <span style={{ whiteSpace: 'pre-wrap' }}>{note.note}</span><br /><small>Evidence reference: <code>{note.evidenceId}</code></small>
    </li>)}</ul> : <p>No control notes recorded.</p>}
    <details><summary>Recorded input fingerprints</summary><dl>
      <div><dt>Package</dt><dd><code>{input.packageSha256}</code></dd></div>
      <div><dt>Analysis result</dt><dd><code>{input.analysisSha256}</code></dd></div>
    </dl></details>
    {Object.keys(additional).length > 0 && <details><summary>Additional recorded input fields</summary>
      <RecordedValue field="input" value={additional} downloadLabel="Download exact snapshot JSON" />
    </details>}
  </>
}

export function SavedArtifactSummary({ artifact }: { artifact: HypothesisRevision | HypothesisChallenge }) {
  const revision = 'snapshotSha256' in artifact
  const id = revision ? artifact.hypothesisId : artifact.challengeId
  const version = revision ? artifact.revision : artifact.version
  return <details><summary>Last confirmed saved artifact</summary>
    <p><strong>{revision ? artifact.name : artifact.summary}</strong> · {revision ? 'revision' : 'challenge version'} {version}</p>
    <dl>
      <div><dt>{revision ? 'Hypothesis reference' : 'Challenge reference'}</dt><dd><code>{id}</code></dd></div>
      <div><dt>Saved fingerprint</dt><dd><code>{revision ? artifact.snapshotSha256 : artifact.sha256}</code></dd></div>
      {!revision && <div><dt>Attached hypothesis</dt><dd>Revision {artifact.hypothesis.revision} · <code>{artifact.hypothesis.hypothesisId}</code></dd></div>}
    </dl>
    <button type="button" onClick={() => downloadJson(artifact, `${revision ? 'hypothesis' : 'challenge'}-${id}-v${version}.json`)}>Download confirmed artifact JSON</button>
  </details>
}
