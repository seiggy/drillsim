import { useMemo, useRef, useState } from 'react'
import type { ProposedWellPathStation } from '../types'
import { contactCode, contactLabel, type ReservoirModelColumn, type StaticReservoirModel } from '../reservoirModel'

type P3 = { e: number; n: number; d: number }
type Fluid = 'gas' | 'oil' | 'water'

const km3 = (m3: number) => `${(m3 / 1e9).toLocaleString(undefined, { maximumFractionDigits: m3 < 1e10 ? 1 : 0 })} km³`
const metres = (m: number) => `${Math.round(m).toLocaleString()} m`
const roles = ['background', 'logged', 'reference', 'selected']

/** Display grid (at most ~44 nodes per axis) plus the fluid bands implied by the fitted contacts. */
export function modelGeometry(model: StaticReservoirModel) {
  const g = model.grid
  const step = Math.max(1, Math.ceil(Math.max(g.columns, g.rows) / 44))
  const axis = (count: number) => [...new Set([...Array.from({ length: Math.ceil(count / step) }, (_, i) => i * step), count - 1])]
  const xs = axis(g.columns), ys = axis(g.rows)
  const nodes = ys.map(row => xs.map(column => {
    const i = row * g.columns + column
    return { e: g.minEastingM + column * g.cellSizeM, n: g.minNorthingM + row * g.cellSizeM, d: g.topTvdM[i],
      top: g.topTvdM[i], base: g.baseTvdM[i], gas: g.gasM[i], oil: g.oilM[i] }
  }))
  const goc = model.contacts.find(c => c.type === 'GasOilContact')?.trueVerticalDepthM
  const lower = model.contacts.find(c => c.type !== 'GasOilContact')?.trueVerticalDepthM
  const hydrocarbon: Fluid = model.inputs.hydrocarbon === 'oil' ? 'oil' : 'gas'
  function bands(top: number, base: number): Array<{ fluid: Fluid; from: number; to: number }> {
    if (model.inputs.hydrocarbon === 'none') return [{ fluid: 'water', from: top, to: base }]
    const clamp = (v: number | undefined, fallback: number) => Math.min(base, Math.max(top, v ?? fallback))
    const water = clamp(lower, base)
    const gasOil = goc === undefined ? (hydrocarbon === 'gas' ? water : top) : clamp(goc, top)
    return ([['gas', top, gasOil], ['oil', gasOil, water], ['water', water, base]] as const)
      .filter(([, from, to]) => to - from > 0.01).map(([fluid, from, to]) => ({ fluid, from, to }))
  }
  const flat = nodes.flat()
  const minD = Math.min(...flat.map(n => n.top)), maxD = Math.max(...flat.map(n => n.base))
  const maxTop = Math.max(...flat.map(n => n.top))
  const maxHc = Math.max(1, ...flat.map(n => n.gas + n.oil))
  return { nodes, bands, minD, maxD, maxTop, maxHc, lower, goc, step,
    width: (g.columns - 1) * g.cellSizeM, height: (g.rows - 1) * g.cellSizeM,
    midE: g.minEastingM + (g.columns - 1) * g.cellSizeM / 2, midN: g.minNorthingM + (g.rows - 1) * g.cellSizeM / 2 }
}

export function ReservoirView({ model, loading, error, selectedWellId, referenceWellIds = [], onWell, proposedPath, target, targetColumn }: {
  model?: StaticReservoirModel
  loading?: boolean
  error?: string
  selectedWellId: string
  referenceWellIds?: string[]
  onWell: (id: string) => void
  proposedPath?: ProposedWellPathStation[]
  target?: { eastingM: number; northingM: number; label: string }
  targetColumn?: ReservoirModelColumn
}) {
  const [rotation, setRotation] = useState(35)
  const [tilt, setTilt] = useState(32)
  const [exaggeration, setExaggeration] = useState<number>()
  const [showContacts, setShowContacts] = useState(true)
  const [showWells, setShowWells] = useState(true)
  const drag = useRef<{ x: number; y: number; rotation: number; tilt: number } | undefined>(undefined)
  const geometry = useMemo(() => model ? modelGeometry(model) : undefined, [model])
  const autoScale = geometry ? Math.min(150, Math.max(1, Math.round(0.28 * Math.max(geometry.width, geometry.height) / Math.max(1, geometry.maxD - geometry.minD)))) : 1
  const scaleZ = exaggeration ?? autoScale

  const scene = useMemo(() => {
    if (!geometry || !model) return undefined
    const { nodes, bands, minD, maxD, maxHc, lower, goc, midE, midN } = geometry
    const window = { top: minD - (maxD - minD) * .6, base: maxD + (maxD - minD) * .15 }
    const angle = rotation * Math.PI / 180, pitch = tilt * Math.PI / 180
    const project = (p: P3) => {
      const e = p.e - midE, n = p.n - midN, z = (p.d - (minD + maxD) / 2) * scaleZ
      const across = e * Math.cos(angle) - n * Math.sin(angle), away = e * Math.sin(angle) + n * Math.cos(angle)
      // Camera above the block looking down at `tilt`: far (+away) rises on screen, and deeper points are farther away.
      return { x: across, y: -away * Math.sin(pitch) + z * Math.cos(pitch), depth: away * Math.cos(pitch) + z * Math.sin(pitch) }
    }
    const corners = [nodes[0][0], nodes[0].at(-1)!, nodes.at(-1)![0], nodes.at(-1)!.at(-1)!]
      .flatMap(c => [{ ...c, d: window.top }, { ...c, d: window.base }]).map(project)
    const extent = corners.reduce((b, p) => ({ x: Math.max(b.x, Math.abs(p.x)), y: Math.max(b.y, Math.abs(p.y)) }), { x: 1, y: 1 })
    const fit = Math.min(390 / extent.x, 235 / extent.y)
    const screen = (p: P3) => { const v = project(p); return [430 + v.x * fit, 262 + v.y * fit] as const }
    const pt = (p: P3) => screen(p).map(v => v.toFixed(1)).join(',')

    const faces: Array<{ cls: string; depth: number; d: string }> = []
    const face = (cls: string, points: P3[]) => faces.push({
      cls, depth: points.reduce((sum, p) => sum + project(p).depth, 0) / points.length, d: `M${points.map(pt).join(' L')}Z` })
    for (let y = 0; y < nodes.length - 1; y++) for (let x = 0; x < nodes[y].length - 1; x++) {
      const quad = [nodes[y][x], nodes[y][x + 1], nodes[y + 1][x + 1], nodes[y + 1][x]]
      const gas = quad.reduce((sum, n) => sum + n.gas, 0) / 4, oil = quad.reduce((sum, n) => sum + n.oil, 0) / 4
      face(gas + oil < .5 ? 'rm-top water' : `rm-top ${gas >= oil ? 'gas' : 'oil'} l${Math.min(4, Math.ceil((gas + oil) / maxHc * 4))}`,
        quad.map(n => ({ ...n, d: n.top })))
      if (showContacts) for (const contact of [goc, lower]) {
        if (contact === undefined) continue
        const top = quad.reduce((s, n) => s + n.top, 0) / 4, base = quad.reduce((s, n) => s + n.base, 0) / 4
        if (top < contact && base > contact) face('rm-contact', quad.map(n => ({ ...n, d: contact })))
      }
    }
    const edges = [nodes[0], nodes.map(row => row.at(-1)!), nodes.at(-1)!.slice().reverse(), nodes.map(row => row[0]).reverse()]
    for (const edge of edges) for (let i = 0; i < edge.length - 1; i++) {
      const a = edge[i], b = edge[i + 1]
      const ab = bands(a.top, a.base), bb = bands(b.top, b.base)
      for (const fluid of ['gas', 'oil', 'water'] as const) {
        const fa = ab.find(x => x.fluid === fluid), fb = bb.find(x => x.fluid === fluid)
        if (!fa && !fb) continue
        const edgeOf = (n: typeof a) => { const z = fluid === 'water' ? n.base : n.top; return { from: z, to: z } }
        const za = fa ?? edgeOf(a), zb = fb ?? edgeOf(b)
        face(`rm-wall ${fluid}`, [{ ...a, d: za.from }, { ...b, d: zb.from }, { ...b, d: zb.to }, { ...a, d: za.to }])
      }
    }
    faces.sort((a, b) => b.depth - a.depth)
    const batches: Array<{ cls: string; d: string }> = []
    for (const f of faces) {
      const last = batches.at(-1)
      if (last?.cls === f.cls) last.d += f.d
      else batches.push({ cls: f.cls, d: f.d })
    }

    // Wells are drawn over the block from just above the reservoir; anything shallower is clipped.
    const clip = (points: P3[]) => {
      const runs: P3[][] = []
      let run: P3[] = []
      points.forEach((p, i) => {
        const q = points[i - 1]
        if (q && (q.d < window.top) !== (p.d < window.top) && p.d !== q.d) {
          const f = (window.top - q.d) / (p.d - q.d)
          run.push({ e: q.e + f * (p.e - q.e), n: q.n + f * (p.n - q.n), d: window.top })
        }
        if (p.d >= window.top && p.d <= window.base) run.push(p)
        else if (run.length) { runs.push(run); run = [] }
      })
      if (run.length) runs.push(run)
      return runs.filter(r => r.length > 1).map(r => `M${r.map(pt).join(' L')}`).join(' ')
    }
    const wells = model.wells.map(well => {
      const role = well.wellBoreId === selectedWellId ? 'selected' : well.wellId && referenceWellIds.includes(well.wellId) ? 'reference'
        : well.logged ? 'logged' : 'background'
      const pick = well.pickEastingM !== null && well.pickNorthingM !== null && well.topTvdM !== null
        ? screen({ e: well.pickEastingM, n: well.pickNorthingM, d: well.topTvdM }) : undefined
      return { well, role, d: clip(well.path.map(([e, n, d]) => ({ e, n, d }))), pick }
    }).sort((a, b) => roles.indexOf(a.role) - roles.indexOf(b.role))

    const targetPath = target ? clip(proposedPath && proposedPath.length > 1
      ? proposedPath.map(s => ({ e: s.eastingM, n: s.northingM, d: s.trueVerticalDepthM }))
      : [{ e: target.eastingM, n: target.northingM, d: window.top }, { e: target.eastingM, n: target.northingM, d: window.base }]) : ''
    const targetTop = target && targetColumn ? screen({ e: target.eastingM, n: target.northingM, d: targetColumn.topTvdM }) : undefined
    const targetHead = target ? screen({ e: target.eastingM, n: target.northingM, d: window.top }) : undefined
    return { batches, wells, targetPath, targetTop, targetHead }
  }, [geometry, model, rotation, tilt, scaleZ, showContacts, selectedWellId, referenceWellIds, proposedPath, target, targetColumn])

  if (!model || !geometry || !scene) return <div className="workspace-empty" role="status">
    <strong>{loading ? 'Building the reservoir model…' : 'No reservoir model'}</strong>
    <span>{error || (loading ? 'Fitting surfaces, rock properties and fluid contacts to the visible wells.' : 'No located well pick carries this formation.')}</span></div>

  return <div className="reservoir-view">
    <div className="reservoir-controls" aria-label="Reservoir view controls">
      <label>Rotate {rotation}°<input type="range" min="0" max="360" value={rotation} onChange={e => setRotation(Number(e.target.value))} /></label>
      <label>Tilt {tilt}°<input type="range" min="10" max="85" value={tilt} onChange={e => setTilt(Number(e.target.value))} /></label>
      <label>Depth stretch {scaleZ}×<input type="range" min="1" max="150" value={scaleZ} onChange={e => setExaggeration(Number(e.target.value))} /></label>
      <label className="reservoir-wire"><input type="checkbox" checked={showContacts} onChange={e => setShowContacts(e.target.checked)} />Contact surface</label>
      <label className="reservoir-wire"><input type="checkbox" checked={showWells} onChange={e => setShowWells(e.target.checked)} />Wells</label>
      <button type="button" onClick={() => { setRotation(35); setTilt(32); setExaggeration(undefined); setShowContacts(true); setShowWells(true) }}>Reset view</button>
    </div>
    <div className="reservoir-canvas" tabIndex={0} role="region" aria-label="Reservoir model, drag to rotate">
      <svg className="reservoir-scene" viewBox="0 0 860 530" role="group" aria-label="3D reservoir model with wells"
        onPointerDown={e => { drag.current = { x: e.clientX, y: e.clientY, rotation, tilt }; e.currentTarget.setPointerCapture(e.pointerId) }}
        onPointerMove={e => {
          const start = drag.current
          if (!start) return
          setRotation(Math.round(((start.rotation - (e.clientX - start.x) * .4) % 360 + 360) % 360))
          setTilt(Math.round(Math.min(85, Math.max(10, start.tilt + (e.clientY - start.y) * .3))))
        }}
        onPointerUp={() => { drag.current = undefined }} onPointerCancel={() => { drag.current = undefined }}>
        <title>Static reservoir model</title>
        <rect className="plot-bed" width="860" height="530" />
        {scene.batches.map((batch, i) => <path key={i} className={batch.cls} d={batch.d} />)}
        {showWells && scene.wells.map(({ well, role, d, pick }) => d && <g key={well.wellBoreId} className={`rm-well ${role}`}
          role="button" tabIndex={role === 'background' ? -1 : 0} aria-label={`Select ${well.name}`}
          onPointerDown={e => e.stopPropagation()} onClick={() => onWell(well.wellBoreId)}
          onKeyDown={e => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); onWell(well.wellBoreId) } }}>
          <path d={d} /><title>{`${well.name}${well.topTvdM !== null ? ` · enters the reservoir at ${metres(well.topTvdM)}` : ''}`}</title>
          {pick && <circle cx={pick[0]} cy={pick[1]} r={role === 'background' ? 2.5 : 4} />}
          {pick && (role === 'selected' || role === 'reference') && <text x={pick[0] + 7} y={pick[1] - 6}>{well.name}</text>}
        </g>)}
        {target && scene.targetPath && <g className="rm-target">
          <path d={scene.targetPath} />
          {scene.targetTop && <circle cx={scene.targetTop[0]} cy={scene.targetTop[1]} r="6" />}
          {scene.targetHead && <text x={scene.targetHead[0] + 9} y={scene.targetHead[1] + 4}>{target.label}</text>}
        </g>}
        <text x="18" y="24" className="plot-label">Top of the reservoir · depth stretched {scaleZ}× · drag to rotate</text>
        <text x="18" y="510" className="plot-label">Model box {(geometry.width / 1000).toFixed(0)} × {(geometry.height / 1000).toFixed(0)} km · reservoir top {metres(geometry.minD)} – {metres(geometry.maxTop)}</text>
      </svg>
    </div>
    <div className="rm-legend" aria-label="Legend">
      <span><i className="legend gas" />Gas-bearing rock (darker = thicker)</span>
      {model.inputs.hydrocarbon.includes('oil') && <span><i className="legend oil" />Oil-bearing rock</span>}
      <span><i className="legend water" />Water-bearing rock</span>
      {model.contacts.length > 0 && <span><i className="legend contact" />Fluid contact</span>}
      <span><i className="legend well" />Wells · dot = where they enter the reservoir</span>
      {target && <span><i className="legend target" />{target.label}</span>}
    </div>
    <div className="reservoir-ledger" aria-label="Reservoir model summary">
      {model.contacts.length ? model.contacts.map(c => <div key={c.type}>
        <strong>{contactLabel[c.type]} ({contactCode[c.type]}) at {metres(c.trueVerticalDepthM)}</strong>
        <span>Fitted to {c.supportingSamples + c.misclassifiedSamples} log readings in {c.wells} wells; {c.supportingSamples} agree with one flat contact.</span>
      </div>) : <div><strong>No fluid contact found</strong><span>{model.limitations.at(-1)}</span></div>}
      <div><strong>{km3(model.volumes.gasNetRockM3 + model.volumes.oilNetRockM3)} of hydrocarbon-bearing good rock</strong>
        <span>Out of {km3(model.volumes.netRockM3)} good rock and {km3(model.volumes.grossRockM3)} in total inside the model box. Rock volume, not reserves.</span></div>
      <div><strong>Built from {model.inputs.structuralControls} well tops and {model.inputs.propertyWells} logged wells</strong>
        <span>{model.grid.layers} layers of about {(model.inputs.medianGrossM / model.grid.layers).toFixed(0)} m · {Math.round(model.grid.cellSizeM)} m cells, drawn every {Math.round(model.grid.cellSizeM * geometry.step)} m.</span></div>
    </div>
    <details className="reservoir-method">
      <summary>How this model is built</summary>
      <p>The top and base of {model.reservoirName} are interpolated between the depths where wells crossed it. The rock in between is split
        into layers; each layer takes its share of good rock, porosity and permeability from the nearest logged wells. One flat fluid contact
        is fitted to every log reading that shows gas, oil or water in good rock: rock above it holds hydrocarbons, rock below holds water.</p>
      <ul>{model.limitations.map(item => <li key={item}>{item}</li>)}</ul>
    </details>
  </div>
}
