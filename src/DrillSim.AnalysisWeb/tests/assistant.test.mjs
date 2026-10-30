import assert from 'node:assert/strict'
import { fileURLToPath } from 'node:url'
import { createElement } from 'react'
import { renderToStaticMarkup } from 'react-dom/server'
import { createServer } from 'vite'

const server = await createServer({ root: fileURLToPath(new URL('..', import.meta.url)), server: { middlewareMode: true }, appType: 'custom' })
try {
  const { Markdown } = await server.ssrLoadModule('/src/components/AssistantPanel.tsx')
  const { parseChartSpec, AssistantChart } = await server.ssrLoadModule('/src/components/AssistantChart.tsx')
  const md = text => renderToStaticMarkup(createElement(Markdown, { text }))

  const table = md('Wells:\n\n| Well | Net pay (m) | Fluid |\n|---|---|:-:|\n| 31/2-1 | 80.3 | **gas** |\n| a \\| b | 1,200 | <img src=x onerror=alert(1)> |\n\nDone.')
  assert.match(table, /<table>/, 'Pipe tables render as tables.')
  assert.match(table, /<td class="num">80.3<\/td>/, 'Numeric columns are right-aligned.')
  assert.match(table, /<td class="center"><strong>gas<\/strong><\/td>/, 'Separator alignment and inline bold apply in cells.')
  assert.match(table, /<td>a \| b<\/td>/, 'Escaped pipes stay in the cell.')
  assert.ok(!table.includes('<img'), 'HTML in model output is shown as text, never markup.')
  assert.match(table, /&lt;img src=x onerror=alert\(1\)&gt;/)
  assert.match(table, /<p>Done.<\/p>$/, 'Text after the table continues as paragraphs.')
  assert.ok(!md('a | b\nnot a separator').includes('<table>'), 'A stray pipe is not a table.')
  assert.ok(md('<script>alert(1)</script>').includes('&lt;script&gt;'), 'Script tags are escaped.')

  const spec = { title: 'Net pay by well (m)', kind: 'bar', xLabel: 'Well', yLabel: 'Net pay (m)',
    series: [{ name: 'Net pay', points: [{ x: '31/2-1', y: 80.3 }, { x: '<b>31/5-4 S</b>', y: 100.3 }] }] }
  const parsed = parseChartSpec(spec)
  assert.equal(typeof parsed, 'object', 'A valid bar chart parses.')
  const svg = renderToStaticMarkup(createElement(AssistantChart, { spec: parsed }))
  assert.equal((svg.match(/<rect /g) ?? []).length, 2, 'One bar per point.')
  assert.ok(!svg.includes('<b>') && svg.includes('&lt;b&gt;'), 'Chart labels are text, never markup.')
  assert.equal(typeof parseChartSpec({ ...spec, kind: 'pie' }), 'string', 'Unknown kinds are refused.')
  assert.equal(typeof parseChartSpec({ ...spec, kind: 'line' }), 'string', 'Line charts need numeric x.')
  assert.equal(typeof parseChartSpec({ ...spec, series: [{ name: 'x', points: [{ x: 'a', y: Infinity }] }] }), 'string', 'Non-finite values are refused.')
  assert.equal(typeof parseChartSpec({ ...spec, series: Array(7).fill(spec.series[0]) }), 'string', 'Series count is capped.')
  const line = parseChartSpec({ ...spec, kind: 'line', series: [{ name: 'Gas', points: [{ x: 2, y: 5 }, { x: '1', y: 3 }] }] })
  assert.equal(typeof line, 'object', 'Numeric x (number or numeric text) is accepted for lines.')
  assert.match(renderToStaticMarkup(createElement(AssistantChart, { spec: line })), /<polyline/, 'Line charts draw a line.')
  console.log('Assistant rich-content checks passed: tables, escaping and chart validation.')
} finally {
  await server.close()
}
