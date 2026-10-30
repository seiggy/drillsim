import assert from 'node:assert/strict'
import { fileURLToPath } from 'node:url'
import { createServer } from 'vite'

const server = await createServer({ root: fileURLToPath(new URL('..', import.meta.url)), server: { middlewareMode: true }, appType: 'custom' })
try {
  const { locateActual, actualCandidates, formatVolume } = await server.ssrLoadModule('/src/screens/ResultsScreen.tsx')
  const scored = (value, lowerBound, upperBound) => ({ name: 'x', basis: 'HiddenTruth', status: 'Scored', value, lowerBound, upperBound })
  const coverage = value => ({ name: 'c', basis: 'HiddenTruth', status: 'Scored', value })
  const pay = { p90: 0, p50: 31.5, p10: 40.5 }
  // Actual 60.29: error 28.79, outside above the range, lower bound 19.79, upper 60.29.
  assert.equal(locateActual(pay, scored(28.79, 19.79, 60.29), coverage(0)), 31.5 + 28.79, 'Coverage and bounds pick the side.')
  const top = { p90: 1474, p50: 1560, p10: 1645 }
  // Actual 1540.8 lies inside; upper bound max(66.8, 104.2) distinguishes it from 1579.2 (upper 105.2).
  assert.ok(Math.abs(locateActual(top, scored(19.2, 0, 1645 - 1540.8), coverage(1)) - 1540.8) < 1e-9)
  const contact = { p90: 1546, p50: 1554, p10: 1562 }
  assert.equal(locateActual(contact, scored(42.5, 34.5, 50.5)), undefined, 'A symmetric range cannot tell the side.')
  assert.deepEqual(actualCandidates(contact, scored(42.5, 34.5, 50.5)).sort(), [1511.5, 1596.5])
  assert.deepEqual(actualCandidates(contact, { name: 'x', basis: 'HiddenTruth', status: 'Unavailable' }), [])
  assert.equal(locateActual(pay, scored(28.79, 0, 99), coverage(0)), undefined, 'Inconsistent bounds yield no guess.')
  assert.equal(formatVolume(1_541_931_290), '1.5 bn m³')
  assert.equal(formatVolume(366_000), '366 k m³')
  console.log('Results checks passed: actual values recovered from published errors, bounds and coverage; ambiguity stays explicit.')
} finally { await server.close() }
