/**
 * Charts the assistant draws through its show_chart tool. The tool input is untrusted model output: it is validated
 * here (again, after the server) and drawn as plain SVG text and shapes, never as HTML.
 */
export type ChartKind = 'bar' | 'line' | 'scatter'
export interface ChartSpec {
  title: string; kind: ChartKind; xLabel: string; yLabel: string
  series: { name: string; points: { x: string; y: number }[] }[]
}

export const chartLimits = { series: 6, points: 100, categories: 30, title: 120, label: 60, name: 40, x: 40 }

const text = (value: unknown, max: number) => typeof value === 'string' && value.trim() && value.length <= max ? value.trim() : undefined
const numeric = (value: string) => /^\s*[-+]?(\d+\.?\d*|\.\d+)([eE][-+]?\d+)?\s*$/.test(value) ? Number(value) : NaN

/** Returns a safe chart spec, or a reason the input can't be drawn. */
export function parseChartSpec(input: unknown): ChartSpec | string {
  if (!input || typeof input !== 'object') return 'No chart data.'
  const raw = input as Record<string, unknown>
  const title = text(raw.title, chartLimits.title), xLabel = text(raw.xLabel, chartLimits.label), yLabel = text(raw.yLabel, chartLimits.label)
  const kind = raw.kind === 'bar' || raw.kind === 'line' || raw.kind === 'scatter' ? raw.kind : undefined
  if (!title || !xLabel || !yLabel || !kind) return 'A chart needs a title, axis labels and a kind of bar, line or scatter.'
  if (!Array.isArray(raw.series) || !raw.series.length || raw.series.length > chartLimits.series) return `A chart needs 1–${chartLimits.series} series.`
  const series: ChartSpec['series'] = []
  for (const item of raw.series as Record<string, unknown>[]) {
    const name = text(item?.name, chartLimits.name)
    if (!name || !Array.isArray(item.points) || !item.points.length || item.points.length > chartLimits.points) return `Each series needs a name and 1–${chartLimits.points} points.`
    const points: ChartSpec['series'][number]['points'] = []
    for (const point of item.points as Record<string, unknown>[]) {
      const x = typeof point?.x === 'number' ? String(point.x) : text(point?.x, chartLimits.x)
      const y = point?.y
      if (x === undefined || typeof y !== 'number' || !Number.isFinite(y)) return 'Every point needs an x and a finite y.'
      if (kind !== 'bar' && !Number.isFinite(numeric(x))) return 'Line and scatter charts need numeric x values.'
      points.push({ x, y })
    }
    series.push({ name, points })
  }
  if (kind === 'bar' && new Set(series.flatMap(item => item.points.map(point => point.x))).size > chartLimits.categories) return `A bar chart can show at most ${chartLimits.categories} categories.`
  return { title, kind, xLabel, yLabel, series }
}

function ticks(min: number, max: number, count = 4) {
  if (min === max) { min -= Math.abs(min) * .1 || 1; max += Math.abs(max) * .1 || 1 }
  const rough = (max - min) / count, power = 10 ** Math.floor(Math.log10(rough))
  const step = [1, 2, 2.5, 5, 10].map(factor => factor * power).find(candidate => candidate >= rough)!
  const start = Math.floor(min / step) * step, end = Math.ceil(max / step) * step
  return Array.from({ length: Math.round((end - start) / step) + 1 }, (_, index) => +(start + index * step).toPrecision(12))
}

export function formatTick(value: number) {
  const size = Math.abs(value)
  return size >= 1e9 ? `${+(value / 1e9).toPrecision(3)}B` : size >= 1e6 ? `${+(value / 1e6).toPrecision(3)}M`
    : size >= 1e4 ? `${+(value / 1e3).toPrecision(3)}k` : `${+value.toPrecision(4)}`
}

const palette = ['var(--cp-step-yellow)', '#2f8cff', 'var(--cp-step-orange)', 'var(--cp-step-red)', 'var(--cp-step-white)', 'var(--cp-machine-muted)']
const fluidColor = (name: string) => /\bgas\b/i.test(name) ? 'var(--cp-step-red)' : /\boil\b/i.test(name) ? 'var(--cp-step-orange)' : /\bwater\b/i.test(name) ? '#2f8cff' : undefined
const clip = (value: string, max: number) => value.length > max ? `${value.slice(0, max - 1)}…` : value

export function AssistantChart({ spec }: { spec: ChartSpec }) {
  const width = 380, height = 230, left = 50, right = 12, top = 12, bottom = 44
  const plotW = width - left - right, plotH = height - top - bottom
  const colors = spec.series.map((item, index) => fluidColor(item.name) ?? palette[index % palette.length])
  const ys = spec.series.flatMap(item => item.points.map(point => point.y))
  const yTicks = ticks(spec.kind === 'bar' ? Math.min(0, ...ys) : Math.min(...ys), spec.kind === 'bar' ? Math.max(0, ...ys) : Math.max(...ys))
  const y = (value: number) => top + plotH - (value - yTicks[0]) / (yTicks.at(-1)! - yTicks[0]) * plotH
  const categories = [...new Set(spec.series.flatMap(item => item.points.map(point => point.x)))]
  const xs = spec.series.flatMap(item => item.points.map(point => Number(point.x)))
  const xTicks = spec.kind === 'bar' ? [] : ticks(Math.min(...xs), Math.max(...xs))
  const x = (value: number) => left + (value - xTicks[0]) / (xTicks.at(-1)! - xTicks[0]) * plotW
  const band = plotW / Math.max(categories.length, 1), barW = Math.max(2, band * .76 / spec.series.length)
  const labelEvery = Math.ceil(categories.length / 8)

  return <figure className="assistant-chart">
    <figcaption>{spec.title}</figcaption>
    <svg viewBox={`0 0 ${width} ${height}`} role="img" aria-label={`${spec.title}: ${spec.kind} chart of ${spec.yLabel} by ${spec.xLabel}`}>
      {yTicks.map(tick => <g key={tick}><line className="grid" x1={left} x2={width - right} y1={y(tick)} y2={y(tick)} />
        <text x={left - 6} y={y(tick)} dy="0.32em" textAnchor="end">{formatTick(tick)}</text></g>)}
      {xTicks.map(tick => <text key={tick} x={x(tick)} y={top + plotH + 16} textAnchor="middle">{formatTick(tick)}</text>)}
      {spec.kind === 'bar' && categories.map((category, index) => index % labelEvery === 0 &&
        <text key={category} x={left + band * (index + .5)} y={top + plotH + 16} textAnchor="middle">{clip(category, Math.max(4, Math.floor(band * labelEvery / 6.5)))}</text>)}
      <line className="axis" x1={left} x2={width - right} y1={y(Math.max(yTicks[0], Math.min(0, yTicks.at(-1)!)))} y2={y(Math.max(yTicks[0], Math.min(0, yTicks.at(-1)!)))} />
      <text x={left + plotW / 2} y={height - 6} textAnchor="middle" className="label">{clip(spec.xLabel, 56)}</text>
      <text transform={`translate(12 ${top + plotH / 2}) rotate(-90)`} textAnchor="middle" className="label">{clip(spec.yLabel, 36)}</text>
      {spec.series.map((item, seriesIndex) => <g key={seriesIndex} style={{ color: colors[seriesIndex] }}>
        {spec.kind === 'bar' && item.points.map(point => {
          const zero = y(Math.max(yTicks[0], 0)), top0 = y(point.y)
          return <rect key={point.x} x={left + band * categories.indexOf(point.x) + band * .12 + barW * seriesIndex} width={barW}
            y={Math.min(zero, top0)} height={Math.max(1, Math.abs(zero - top0))} fill="currentColor"><title>{`${item.name} · ${point.x}: ${point.y}`}</title></rect>
        })}
        {spec.kind === 'line' && <polyline fill="none" stroke="currentColor" strokeWidth="2"
          points={[...item.points].sort((a, b) => Number(a.x) - Number(b.x)).map(point => `${x(Number(point.x))},${y(point.y)}`).join(' ')} />}
        {spec.kind !== 'bar' && item.points.map((point, index) => <circle key={index} cx={x(Number(point.x))} cy={y(point.y)} r={spec.kind === 'line' ? 2.5 : 3.5} fill="currentColor">
          <title>{`${item.name} · ${point.x}: ${point.y}`}</title></circle>)}
      </g>)}
    </svg>
    {spec.series.length > 1 && <ul className="assistant-legend">{spec.series.map((item, index) =>
      <li key={index}><i style={{ background: colors[index] }} />{item.name}</li>)}</ul>}
    <details><summary>Show data</summary>
      <div className="assistant-table"><table><thead>      <tr><th>{spec.xLabel}</th>{spec.series.map((item, index) => <th key={index} className="num">{item.name}</th>)}</tr></thead>
              <tbody>{(spec.kind === 'bar' ? categories : [...categories].sort((a, b) => Number(a) - Number(b)))
                .map(key => <tr key={key}><td>{key}</td>{spec.series.map((item, index) => <td key={index} className="num">{item.points.find(point => point.x === key)?.y ?? '–'}</td>)}</tr>)}</tbody></table></div>
    </details>
  </figure>
}
