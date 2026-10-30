import { useEffect, useState } from 'react'
import { applyAnalysis, getFieldPackage, getPrediction, getProduction, getReveal, getScorecard } from '../api'
import { downloadJson, payLabel } from '../analysisConfiguration'
import { AssistantContext } from '../assistantContext'
import { ProductionPane } from '../components/ProductionPane'
import { cumulativeProduction, productionPhases, readRevealedProduction, type ProductionMonth, type ProductionPhase } from '../production'
import { PredictionSummary } from './ForecastScreen'
import { metres } from '../structure'
import type { FieldPackage, PredictionRecord, PublicProductionSeriesMetadata, PublicScorecard, QuantileValues, RevealReceipt, Scenario, ScorecardMetric, WellSummary } from '../types'

interface Loaded {
  prediction?: PredictionRecord
  scorecard?: PublicScorecard
  reveal?: RevealReceipt
  production?: PublicProductionSeriesMetadata
  clone?: FieldPackage
  newWell?: WellSummary
  months?: ProductionMonth[]
}

const baselineNames: Record<string, string> = {
  NearestWell: 'Copy the nearest well', FieldMean: 'Use the field average',
  FourNeighborIdw: 'Screening estimate (4 nearest wells, no AI)', UncertaintyAwareRank1: 'Top-ranked screening target',
}
const contactNames = { GOC: 'Gas–oil contact', OWC: 'Oil–water contact', GWC: 'Gas–water contact' } as const
const phaseNames: Record<ProductionPhase, string> = { oil: 'Oil', gas: 'Gas', water: 'Water' }
const readable = (name: string) => name.replace(/\.(HiddenTruth|RevealedObservation|Baseline|ObservationGap)$/, '')
  .replace(/\./g, ' ').replace(/([a-z0-9])([A-Z])/g, '$1 $2').replace(/^./, letter => letter.toUpperCase())
  .replace(/P50/g, 'best-estimate').replace(/P90 P10/g, 'range')

/**
 * The scorecard keeps the hidden value private but publishes |P50 − actual| with its interval bounds and P90–P10
 * coverage. Only one of P50 ± error is usually consistent with all three; return it, or undefined when ambiguous.
 */
export function locateActual(q: QuantileValues, error?: ScorecardMetric, coverage?: ScorecardMetric): number | undefined {
  const fits = actualCandidates(q, error, coverage)
  return fits.length === 1 ? fits[0] : undefined
}

/** Every actual value consistent with the published error, bounds and coverage (one or two). */
export function actualCandidates(q: QuantileValues, error?: ScorecardMetric, coverage?: ScorecardMetric): number[] {
  if (error?.status !== 'Scored' || typeof error.value !== 'number') return []
  const close = (published: number | null | undefined, expected: number) =>
    published == null || Math.abs(published - expected) <= 1e-6 * Math.max(1, Math.abs(expected))
  const fits = [...new Set([q.p50 + error.value, q.p50 - error.value])].filter(actual => {
    const inside = actual >= q.p90 && actual <= q.p10
    if (coverage?.status === 'Scored' && (coverage.value === 1) !== inside) return false
    return close(error.lowerBound, actual < q.p90 ? q.p90 - actual : actual > q.p10 ? actual - q.p10 : 0) &&
      close(error.upperBound, Math.max(Math.abs(q.p90 - actual), Math.abs(q.p10 - actual)))
  })
  return fits
}

export function formatVolume(m3: number) {
  const units: Array<[number, string]> = [[1e9, 'bn'], [1e6, 'M'], [1e3, 'k']]
  const [scale, suffix] = units.find(([size]) => Math.abs(m3) >= size) ?? [1, '']
  return `${(m3 / scale).toLocaleString(undefined, { maximumFractionDigits: m3 / scale < 10 && scale > 1 ? 1 : 0 })}${suffix ? ` ${suffix}` : ''} m³`
}

interface Row {
  key: string
  label: string
  unit: 'thickness' | 'depth'
  forecast: QuantileValues
  truth?: number
  /** Both possible actual values when the published bounds cannot tell which side of the forecast it fell. */
  either?: number[]
  unavailable?: string
  error?: number
  inRange?: boolean
  logs?: number
  logsNote?: string
}

function RangeRow({ row }: { row: Row }) {
  const values = [row.forecast.p90, row.forecast.p10, row.truth, row.logs, ...(row.either ?? [])].filter((v): v is number => typeof v === 'number')
  const low = Math.min(...values), high = Math.max(...values), pad = Math.max(1, (high - low) * .08)
  const x = (v: number) => 12 + 536 * (v - (low - pad)) / ((high + pad) - (low - pad))
  const missed = row.inRange === false
  const direction = row.either ? (row.unit === 'depth' ? 'shallower or deeper' : 'more or less') : row.truth === undefined ? '' : row.unit === 'depth'
    ? row.truth > row.forecast.p50 ? 'deeper' : 'shallower'
    : row.truth > row.forecast.p50 ? 'more' : 'less'
  return <div className={`reality-row ${missed ? 'missed' : row.inRange ? 'hit' : ''}`}>
    <div className="reality-label">
      <strong>{row.label}</strong>
      <span>Forecast {metres(row.forecast.p50)} <small>({metres(row.forecast.p90)} – {metres(row.forecast.p10)})</small></span>
    </div>
    <svg className="reality-plot" viewBox="0 0 560 46" role="img"
      aria-label={`${row.label}: forecast ${metres(row.forecast.p90)} to ${metres(row.forecast.p10)}, best ${metres(row.forecast.p50)}${row.truth !== undefined ? `; actual ${metres(row.truth)}` : ''}${row.logs !== undefined ? `; new well logs ${metres(row.logs)}` : ''}`}>
      <line className="reality-axis" x1="12" x2="548" y1="22" y2="22" />
      <rect className="reality-band" x={x(row.forecast.p90)} y="13" width={Math.max(2, x(row.forecast.p10) - x(row.forecast.p90))} height="18" rx="4" />
      <line className="reality-p50" x1={x(row.forecast.p50)} x2={x(row.forecast.p50)} y1="9" y2="35" />
      {row.logs !== undefined && <path className="reality-logs" d={`M${x(row.logs)},30 l-6,10 h12 z`}><title>{`New well’s logs: ${metres(row.logs)}${row.logsNote ? ` (${row.logsNote})` : ''}`}</title></path>}
      {row.either?.map(value => <path key={value} className="reality-truth either" d={`M${x(value)},12 l7,10 l-7,10 l-7,-10 z`}><title>{`Actual is either ${metres(row.either![0])} or ${metres(row.either![1])}`}</title></path>)}
      {row.truth !== undefined && <path className="reality-truth" d={`M${x(row.truth)},12 l7,10 l-7,10 l-7,-10 z`}><title>{`Actual (simulator): ${metres(row.truth)}`}</title></path>}
      <text className="reality-tick" x="12" y="44">{metres(low)}</text>
      <text className="reality-tick" x="548" y="44" textAnchor="end">{row.unit === 'depth' ? `${metres(high)} · deeper →` : metres(high)}</text>
    </svg>
    <div className="reality-verdict">
      {row.unavailable ? <span>{row.unavailable}</span> : row.inRange === undefined && row.error === undefined ? <span>Not scored</span>
        : <><strong>{row.truth !== undefined ? `Actual ${metres(row.truth)}` : `Off by ${metres(row.error, 1)}`}</strong>
          <span>{row.inRange === true ? 'Inside the forecast range' : row.inRange === false ? `Outside the range · ${metres(row.error)} ${direction}`.trim()
            : row.truth !== undefined ? `${metres(row.error)} ${direction} than forecast` : 'Above or below the forecast; the scores can’t tell which'}</span></>}
    </div>
  </div>
}

function ProductionChart({ phase, months, forecast }: { phase: ProductionPhase; months: ProductionMonth[]; forecast: Array<{ year: number; value: number }> }) {
  const cumulative = cumulativeProduction(months).map(item => item[phase])
  const actual5 = cumulative.at(-1)
  const forecast5 = forecast.find(item => item.year === 5)?.value ?? 0
  const top = Math.max(1, ...cumulative.map(v => v ?? 0), ...forecast.map(f => f.value)) * 1.08
  const x = (month: number) => 8 + 284 * month / 60, y = (v: number) => 104 - 92 * v / top
  let started = false
  const line = cumulative.map((v, i) => { if (v === null) { started = false; return '' } const p = `${started ? 'L' : 'M'}${x(i + 1).toFixed(1)},${y(v).toFixed(1)}`; started = true; return p }).join(' ')
  const ratio = actual5 && forecast5 ? forecast5 / actual5 : undefined
  return <figure className={`production-mini ${phase}`}>
    <figcaption><strong>{phaseNames[phase]}</strong>
      <span>{actual5 === null || actual5 === undefined ? 'Missing readings' : `${formatVolume(actual5)} in 5 years`}</span></figcaption>
    <svg viewBox="0 0 300 116" role="img" aria-label={`${phaseNames[phase]} cumulative production: actual ${actual5 == null ? 'unknown' : formatVolume(actual5)}, forecast ${formatVolume(forecast5)} after 5 years`}>
      <line className="reality-axis" x1="8" x2="292" y1="104" y2="104" />
      {[12, 36, 60].map(m => <line key={m} className="production-year" x1={x(m)} x2={x(m)} y1="10" y2="104" />)}
      <path className="production-actual" d={line} />
      <path className="production-forecast" d={`M${x(0)},${y(0)} ${forecast.sort((a, b) => a.year - b.year).map(f => `L${x(f.year * 12)},${y(f.value)}`).join(' ')}`} />
      {forecast.map(f => <circle key={f.year} className="production-forecast-point" cx={x(f.year * 12)} cy={y(f.value)} r="4"><title>{`Forecast year ${f.year}: ${formatVolume(f.value)}`}</title></circle>)}
      {[1, 3, 5].map(year => <text key={year} className="reality-tick" x={x(year * 12)} y="114" textAnchor="middle">{year}y</text>)}
    </svg>
    <p>{forecast5 >= 1 && (actual5 ?? 0) < Math.max(1, forecast5 / 1000) ? `Forecast ${formatVolume(forecast5)}; the well produced almost none.`
      : forecast5 === 0 && (actual5 ?? 0) > 0 ? `Forecast none; the well produced ${formatVolume(actual5!)}.`
      : ratio === undefined ? `Forecast ${formatVolume(forecast5)}.`
      : ratio > 1.25 ? `Forecast ${formatVolume(forecast5)}, ${ratio.toFixed(ratio < 10 ? 1 : 0)}× too high.`
      : ratio < .8 ? `Forecast ${formatVolume(forecast5)}, ${(1 / ratio).toFixed(1 / ratio < 10 ? 1 : 0)}× too low.`
      : `Forecast ${formatVolume(forecast5)}, within 25%.`}</p>
  </figure>
}

export function ResultsScreen({ run, onProspects }: { run: Scenario; onProspects: () => void }) {
  const [data, setData] = useState<Loaded>()
  const [error, setError] = useState('')

  useEffect(() => {
    const controller = new AbortController()
    void (async () => {
      try {
        const [prediction, scorecard, reveal, production] = await Promise.all([
          getPrediction(run.scenarioId, controller.signal), getScorecard(run.scenarioId, controller.signal),
          getReveal(run.scenarioId, controller.signal), getProduction(run.scenarioId, controller.signal)])
        let clone: FieldPackage | undefined, newWell: WellSummary | undefined, months: ProductionMonth[] | undefined
        if (run.clonedFieldId && reveal?.status === 'Revealed') {
          const scope = { scenarioId: run.scenarioId, asOf: run.asOfUtc }
          clone = await getFieldPackage(run.clonedFieldId, scope, controller.signal)
          const configuration = prediction?.body.analysisBinding?.configuration
          if (production) {
            let drilledWellId = ''
            try { const revealed = readRevealedProduction(clone, run, reveal, production); drilledWellId = revealed.wellId; months = revealed.months } catch { /* No identifiable new well. */ }
            if (configuration) {
              const after = await applyAnalysis(run.clonedFieldId, run.reservoirName, configuration, scope, controller.signal)
              newWell = after.wellSummaries.find(well => well.wellId === drilledWellId)
            }
          }
        }
        if (!controller.signal.aborted) setData({ prediction, scorecard, reveal, production, clone, newWell, months })
      } catch (reason) { if (!controller.signal.aborted) setError(String(reason)) }
    })()
    return () => controller.abort()
  }, [run.scenarioId])

  if (error) return <section className="screen-empty"><h2>Results could not be loaded</h2><p>{error}</p></section>
  if (!data) return <section className="screen-loading" role="status"><strong>Loading results…</strong></section>
  const { prediction, scorecard } = data
  if (!prediction || !scorecard) return <section className="screen-empty"><h2>No results yet</h2>
    <p>This run hasn’t been scored. Finish drilling on the Drill step first.</p></section>

  const metric = (name: string) => scorecard.metrics.find(item => item.name === name && item.status === 'Scored')
  const body = prediction.body
  const targetFluid = body.analysisBinding?.configuration.targetFluid
  const pays = payLabel(body.analysisBinding?.configuration)
  const formation = body.formations[0]
  const quantityRow = (key: string, label: string, unit: Row['unit'], forecast: QuantileValues, stem: string, logs?: number, logsNote?: string): Row => {
    const errorMetric = metric(`${stem}P50AbsoluteError.HiddenTruth`), coverage = metric(`${stem}P90P10Coverage.HiddenTruth`)
    return { key, label, unit, forecast, truth: locateActual(forecast, errorMetric, coverage), error: errorMetric?.value ?? undefined,
      inRange: coverage ? coverage.value === 1 : undefined, logs, logsNote }
  }
  const observed = (forecast: QuantileValues, stem: string) =>
    locateActual(forecast, metric(`${stem}P50AbsoluteError.RevealedObservation`), metric(`${stem}P90P10Coverage.RevealedObservation`))
  const rows: Row[] = [
    quantityRow('pay', pays[0].toUpperCase() + pays.slice(1), 'thickness', body.expectedPaydirtM, 'expectedPaydirt',
      data.newWell?.netPayThicknessM, targetFluid ? 'good rock of any fluid; these logs can’t tell oil from gas' : 'good rock in the logs'),
    ...(formation ? [
      quantityRow('top', 'Reservoir top', 'depth', formation.topTrueVerticalDepthM, 'formationTop', observed(formation.topTrueVerticalDepthM, 'formationTop')),
      quantityRow('base', 'Reservoir base', 'depth', formation.baseTrueVerticalDepthM, 'formationBase', observed(formation.baseTrueVerticalDepthM, 'formationBase')),
    ] : []),
    ...body.contactPredictions.map((contact): Row => {
      const name = `${contact.contactType.toLowerCase()}DepthP50AbsoluteError.HiddenTruth`
      const errorMetric = metric(name), q = contact.trueVerticalDepthM
      const candidates = actualCandidates(q, errorMetric)
      const inside = candidates.map(value => value >= q.p90 && value <= q.p10)
      return { key: contact.contactType, label: contactNames[contact.contactType], unit: 'depth', forecast: q,
        truth: candidates.length === 1 ? candidates[0] : undefined, either: candidates.length === 2 ? candidates : undefined,
        error: errorMetric?.value ?? undefined, inRange: inside.length && inside.every(v => v === inside[0]) ? inside[0] : undefined,
        unavailable: errorMetric ? undefined : scorecard.metrics.some(item => item.name === name && item.status === 'Unavailable')
          ? 'The well didn’t cross this contact, so it can’t be checked.' : undefined }
    }),
  ]
  const scored = rows.filter(row => row.inRange !== undefined)
  const hits = scored.filter(row => row.inRange).length
  const worst = scored.filter(row => !row.inRange).sort((a, b) => (b.error ?? 0) - (a.error ?? 0))[0]

  const precision = metric('fluidClassPrecision.HiddenTruth')?.value, recall = metric('fluidClassRecall.HiddenTruth')?.value
  const present = typeof precision === 'number' ? Math.round(precision * body.fluidClasses.length) : undefined

  const baselines = scorecard.metrics.filter(item => item.basis === 'Baseline' && item.name.startsWith('expectedPaydirtP50AbsoluteError.Baseline.') && item.status === 'Scored')
    .map(item => ({ name: baselineNames[item.name.split('.').at(-1)!] ?? item.name, error: item.value!, ours: false }))
  const ourError = metric('expectedPaydirtP50AbsoluteError.HiddenTruth')?.value
  const methods = [...(typeof ourError === 'number' ? [{ name: 'Our forecast', error: ourError, ours: true }] : []), ...baselines].sort((a, b) => a.error - b.error)
  const maxError = Math.max(1, ...methods.map(item => item.error))
  const rank = methods.findIndex(item => item.ours) + 1

  const months = data.months
  const phases = months ? productionPhases.filter(phase => {
    const actual = cumulativeProduction(months).at(-1)?.[phase] ?? 0
    const forecast = body.productionForecasts.find(item => item.year === 5)?.[`${phase}M3`] ?? 0
    return Math.max(actual, forecast) >= 1
  }) : []

  return <section className="screen-grid results" aria-labelledby="results-title">
    <AssistantContext id="results" description="Results screen: the sealed forecast against what the drilled well found" value={{
      verdict: { insideRange: hits, scored: scored.length, biggestMiss: worst && { quantity: worst.label, errorM: worst.error } },
      rows: rows.map(row => ({ quantity: row.label, forecastP90P50P10: [row.forecast.p90, row.forecast.p50, row.forecast.p10],
        actual: row.truth ?? (row.either ? { eitherOf: row.either } : undefined), errorM: row.error, insideRange: row.inRange, newWellLogs: row.logs, note: row.unavailable })),
      fluids: { forecast: body.fluidClasses, presentOfForecast: present, everyPresentFluidForecast: recall === 1 },
      payErrorByMethod: methods.map(item => ({ method: item.name, errorM: +item.error.toFixed(1) })),
      fiveYearProductionSurfaceM3: months ? Object.fromEntries(productionPhases.map(phase => [phase, {
        actual: cumulativeProduction(months).at(-1)?.[phase], forecast: body.productionForecasts.find(item => item.year === 5)?.[`${phase}M3`] }])) : undefined,
      forecastRationale: body.rationale.slice(0, 1200),
    }} />
    <header className="screen-head">
      <div>
        <h1 id="results-title">How did we do?</h1>
        <p className="results-verdict">{scored.length ? <><strong>{hits} of {scored.length}</strong> forecasts landed inside their range.</> : 'No forecast range could be scored.'}
          {worst && <> Biggest miss: <strong>{worst.label.toLowerCase()}</strong>, {metres(worst.error)} off.</>}
          {rank > 0 && <> The pay forecast ranked <strong>{rank} of {methods.length}</strong> against simple methods.</>}</p>
      </div>
      <div className="button-row">
        <button type="button" onClick={() => downloadJson({ run, prediction: body, seal: prediction.seal, scorecard }, `drillsim-results-${run.scenarioId}.json`)}>Download results</button>
        <button type="button" className="primary" onClick={onProspects}>Try another target</button>
      </div>
    </header>

    <section className="panel reality" aria-labelledby="reality-title">
      <div className="reality-head">
        <h2 id="reality-title">Forecast against what the well found</h2>
        <div className="reality-key" aria-label="Legend">
          <span><i className="key-band" />Forecast range (low – high)</span>
          <span><i className="key-p50" />Best estimate</span>
          <span><i className="key-truth" />Actual, from the simulator</span>
          <span><i className="key-logs" />New well’s logs</span>
        </div>
      </div>
      {rows.map(row => <RangeRow key={row.key} row={row} />)}
      <div className="reality-row fluids">
        <div className="reality-label"><strong>Fluids</strong><span>Forecast {body.fluidClasses.join(', ') || 'none'}</span></div>
        <p className="fluid-verdict">{present === undefined ? 'Not scored.'
          : `${present} of ${body.fluidClasses.length} forecast fluids were in the well${recall === 1 ? ', and every fluid present was forecast.' : '; at least one fluid present was not forecast.'}`}</p>
      </div>
    </section>

    <div className="split results-split">
      <section className="panel" aria-labelledby="production-title">
        <h2 id="production-title">Five years of production</h2>
        <p className="fine-print">Cumulative surface volumes. Solid line: metered production. Dots: the sealed forecast at 1, 3 and 5 years.</p>
        {phases.length ? <div className="production-minis">{phases.map(phase => <ProductionChart key={phase} phase={phase} months={months!}
          forecast={body.productionForecasts.map(item => ({ year: item.year, value: item[`${phase}M3`] }))} />)}</div>
          : <p className="empty-note">No production was published for this run.</p>}
      </section>
      <section className="panel" aria-labelledby="baseline-title">
        <h2 id="baseline-title">Against simple methods</h2>
        <p className="fine-print">How far each method’s {pays} was from the actual value. Shorter is better.</p>
        <ul className="method-bars">{methods.map(item => <li key={item.name} className={item.ours ? 'ours' : ''}>
          <span className="method-name">{item.name}</span>
          <span className="method-track"><span className="method-fill" style={{ width: `${Math.max(1.5, item.error / maxError * 100)}%` }} /></span>
          <span className="method-value">{metres(item.error, 1)}</span>
        </li>)}</ul>
        {rank > 0 && <p className="method-verdict">{rank === 1 ? 'The forecast beat every simple method.'
          : `${methods[0].name} was ${metres(ourError! - methods[0].error, 1)} closer than the forecast.`}</p>}
      </section>
    </div>

    <details className="plain-details panel">
      <summary>The locked forecast and its reasoning</summary>
      <PredictionSummary prediction={prediction} />
      <p>{body.rationale}</p>
      <ul>{body.uncertaintyAssumptions.map((item, index) => <li key={index}>{item}</li>)}</ul>
    </details>

    {data.production && data.reveal && data.clone && <details className="plain-details panel">
      <summary>Monthly production detail</summary>
      <p className="fine-print">The well flows continuously for five years at a 15 MPa bottom-hole pressure, up to 4,320 m³ a day at reservoir
        conditions, converted to surface volumes with fixed gas and oil volume factors. Monthly readings include meter noise. The fluid properties
        are generic, not laboratory-measured, so treat the volumes as indicative. Runs started before 28 October 2026 used a short test schedule
        and show near-zero volumes.</p>
      <ProductionPane fieldPackage={data.clone} scenario={run} reveal={data.reveal} metadata={data.production} prediction={prediction} />
    </details>}

    <details className="plain-details panel">
      <summary>All {scorecard.metrics.length} scoring metrics</summary>
      <table className="plain-table"><thead><tr><th>Metric</th><th>Compared against</th><th>Value</th></tr></thead>
        <tbody>{scorecard.metrics.map((item: ScorecardMetric) => <tr key={`${item.basis}:${item.name}`}>
          <th>{readable(item.name)}</th><td>{item.basis.replace(/([a-z])([A-Z])/g, '$1 $2')}</td>
          <td>{item.status === 'Scored' ? `${Number(item.value).toLocaleString(undefined, { maximumSignificantDigits: 4 })} ${item.unit ?? ''}` : 'Not available'}</td></tr>)}</tbody></table>
      {scorecard.limitation && <p className="fine-print">{scorecard.limitation}</p>}
    </details>
  </section>
}
