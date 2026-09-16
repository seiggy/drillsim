import { useMemo, useState } from 'react'
import { cumulativeProduction, productionPath, productionPhases, readRevealedProduction } from '../production'
import type { FieldPackage, PredictionRecord, PublicProductionSeriesMetadata, RevealReceipt, Scenario } from '../types'

const formatVolume = (value: number | null) => value === null ? 'Missing' : new Intl.NumberFormat(undefined, {
  maximumSignificantDigits: 4,
}).format(value)
const phaseLabels = { oil: 'Oil', gas: 'Gas', water: 'Water' }
const phaseColors = { oil: 'var(--cp-step-orange)', gas: 'var(--cp-step-yellow)', water: 'var(--cp-step-white)' }
const phaseDashes = { oil: undefined, gas: '8 4', water: '2 4' }

export function ProductionPane({ fieldPackage, scenario, reveal, metadata, prediction }: {
  fieldPackage: FieldPackage
  scenario: Scenario
  reveal: RevealReceipt
  metadata: PublicProductionSeriesMetadata
  prediction?: PredictionRecord
}) {
  const [horizon, setHorizon] = useState(5)
  const [mode, setMode] = useState('monthly')
  const result = useMemo(() => {
    try {
      const series = readRevealedProduction(fieldPackage, scenario, reveal, metadata)
      return { series, cumulative: cumulativeProduction(series.months), error: '' }
    } catch (error) {
      return { error: error instanceof Error ? error.message : 'Unable to interpret the revealed production series.' }
    }
  }, [fieldPackage, scenario, reveal, metadata])

  if (!result.series || !result.cumulative) {
    return <div className="scenario-error" role="alert"><strong>Production unavailable</strong><span>{result.error}</span></div>
  }
  const { series, cumulative } = result
  const months = series.months.slice(0, horizon * 12)
  const plot = mode === 'monthly' ? months : cumulative.slice(0, months.length)
  const x = (index: number) => 100 + 690 * index / (months.length - 1)
  const missing = months.filter((month) => productionPhases.some((phase) => month[phase] === null)).length
  const forecast = prediction?.scenarioId === scenario.scenarioId && prediction.seal ? prediction.body.productionForecasts : []

  return (
    <section className="production-pane" aria-labelledby="production-title">
      <header className="production-heading">
        <div>
          <h4 id="production-title">Synthetic corrected production</h4>
          <p>{series.wellName} · {metadata.monthCount} published months · m³, not reserves.</p>
        </div>
        <div className="production-controls">
          <label>Display<select value={mode} onChange={(event) => setMode(event.target.value)}>
            <option value="monthly">Monthly volume</option><option value="cumulative">Cumulative volume</option>
          </select></label>
          <label>Window<select value={horizon} onChange={(event) => setHorizon(Number(event.target.value))}>
            {metadata.checkpointYears.map((year) => <option key={year} value={year}>{year} year{year === 1 ? '' : 's'}</option>)}
          </select></label>
        </div>
      </header>
      <p className="production-note" role="status">
        {months[0].month} to {months.at(-1)!.month} · {missing} month{missing === 1 ? '' : 's'} with missing phase readings.
        {' '}Gaps are not zero; a cumulative total stays unknown after a missing reading. Each phase uses its own linear scale.
      </p>
      <div className="production-chart-scroll" tabIndex={0} role="region" aria-label="Production chart, scroll horizontally on narrow screens">
        <svg className="production-chart" viewBox="0 0 860 440" role="img" aria-label={`${horizon}-year ${mode} oil, gas and water volumes in cubic metres. Exact values follow in the tables.`}>
          <title>{`${horizon}-year ${mode} production`}</title>
          {productionPhases.map((phase, phaseIndex) => {
            const values = plot.map((month) => month[phase])
            const maximum = Math.max(0, ...values.filter((value): value is number => value !== null))
            const top = 26 + phaseIndex * 130
            const y = (value: number) => top + 82 - 82 * value / (maximum || 1)
            return <g key={phase}>
              <text x="12" y={top + 10} className="production-phase-label">{phaseLabels[phase]}</text>
              <text x="12" y={top + 30}>{mode === 'monthly' ? 'm³/month' : 'm³ total'}</text>
              {[0, .5, 1].map((fraction) => <g key={fraction}>
                <line className="plot-grid" x1="100" x2="790" y1={top + 82 * (1 - fraction)} y2={top + 82 * (1 - fraction)} />
                <text x="800" y={top + 82 * (1 - fraction) + 4}>{maximum > 0 && maximum < .001 && fraction > 0
                  ? (maximum * fraction).toExponential(2) : formatVolume(maximum * fraction)}</text>
              </g>)}
              {metadata.checkpointYears.filter((year) => year <= horizon).map((year) => <g key={year}>
                <line className="production-checkpoint-line" x1={x(year * 12 - 1)} x2={x(year * 12 - 1)} y1={top} y2={top + 82} />
                <text x={x(year * 12 - 1)} y={top - 8} textAnchor="end">{year}y</text>
              </g>)}
              <path d={productionPath(values, x, y)} fill="none" stroke={phaseColors[phase]} strokeWidth="2" strokeDasharray={phaseDashes[phase]} />
              {values.map((value, index) => value === null ? null : <circle key={index} cx={x(index)} cy={y(value)} r="2.5" fill={phaseColors[phase]}>
                <title>{`${months[index].month}: ${phaseLabels[phase]} ${formatVolume(value)} m³`}</title>
              </circle>)}
              {values.every((value) => value === null) && <text x="110" y={top + 40}>No complete {mode} values</text>}
            </g>
          })}
          <text x="100" y="410">{months[0].month}</text>
          <text x="790" y="410" textAnchor="end">{months.at(-1)!.month}</text>
          <text x="445" y="434" textAnchor="middle">Published calendar months since reveal</text>
        </svg>
      </div>
      <div className="scenario-table-wrap" tabIndex={0} role="region" aria-label="Cumulative production checkpoints">
        <table className="scenario-table">
          <caption>1-, 3-, and 5-year cumulative volumes (m³)</caption>
          <thead><tr><th scope="col">Checkpoint</th>{productionPhases.map((phase) => <th key={phase} scope="col">{phaseLabels[phase]}</th>)}</tr></thead>
          <tbody>{metadata.checkpointYears.map((year) => {
            const total = cumulative[year * 12 - 1]
            const predicted = forecast.find((item) => item.year === year)
            return <tr key={year}><th scope="row">{year} year{year === 1 ? '' : 's'}<small>Months 1–{year * 12}</small></th>
              {productionPhases.map((phase) => {
                const observed = series.months.slice(0, year * 12).filter((month) => month[phase] !== null).length
                return <td key={phase}>{formatVolume(total[phase])}
                  {total[phase] === null && <small>{observed}/{year * 12} readings available</small>}
                  {predicted && <small>Sealed forecast: {formatVolume(predicted[`${phase}M3`])}</small>}
                </td>
              })}
            </tr>
          })}</tbody>
        </table>
      </div>
      <p className="production-note">Checkpoints sum the first 12/36/60 published monthly observations, not hidden solver state.
        {' '}Missing readings are not interpolated. The ontology exposes corrected, allocated values; separate reported readings and meter-error bands are not available.
      </p>
      <details className="production-values">
        <summary>Monthly readings and allocation ({months.length} months)</summary>
        <div className="scenario-table-wrap" tabIndex={0} role="region" aria-label="Monthly production readings">
          <table className="scenario-table">
            <caption>Corrected monthly observations (m³)</caption>
            <thead><tr><th scope="col">Month</th>{productionPhases.map((phase) => <th key={phase} scope="col">{phaseLabels[phase]}</th>)}<th scope="col">Days producing</th><th scope="col">Allocated</th></tr></thead>
            <tbody>{months.map((month) => <tr key={month.month}>
              <th scope="row">{month.month}</th>{productionPhases.map((phase) => <td key={phase}>{formatVolume(month[phase])}</td>)}
              <td>{month.daysOnProduction ?? 'Missing'}</td><td>{month.allocated ? 'Yes' : 'No'}</td>
            </tr>)}</tbody>
          </table>
        </div>
      </details>
      <p className="production-note">Well <code>{series.wellId}</code> · {metadata.modelVersion}<br />
        Series <code title={metadata.contentSha256}>{metadata.contentSha256.slice(0, 12)}</code> · package <code title={fieldPackage.sha256}>{fieldPackage.sha256.slice(0, 12)}</code>
      </p>
    </section>
  )
}
