import { useMemo, useState } from 'react'
import type { WellView } from './AnalysisWorkspace'
import type { ProposedWellPathStation } from '../types'
import { buildReservoirMeshes, cutAtDepth, geometryVersion } from '../reservoirGeometry'
import type { Fluid, Point3 } from '../reservoirGeometry'

export function ReservoirView({ wells, selectedWellId, onWell, proposedPath }: {
  wells: WellView[]
  selectedWellId: string
  onWell: (id: string) => void
  proposedPath?: ProposedWellPathStation[]
}) {
  const [rotation, setRotation] = useState(35)
  const [tilt, setTilt] = useState(30)
  const [exaggeration, setExaggeration] = useState(3)
  const [cut, setCut] = useState(0)
  const [phase, setPhase] = useState('all')
  const [radius, setRadius] = useState(10000)
  const [wireframe, setWireframe] = useState(false)
  const model = useMemo(() => {
    try { return { result: buildReservoirMeshes(wells, radius), error: '' } }
    catch (error) { return { error: error instanceof Error ? error.message : String(error) } }
  }, [wells, radius])
  const meshes = model.result?.meshes ?? []
  const proposed = proposedPath?.map((p) => ({ east: p.eastingM, north: p.northingM, tvd: p.trueVerticalDepthM })) ?? []
  const scene = [...wells.flatMap((w) => w.stations), ...meshes.flatMap((m) => m.vertices), ...proposed]
  if (!scene.length) return <div className="workspace-empty"><strong>No located geometry</strong><span>Covering surveys and explicit coordinates are required; a wellhead is not a downhole location.</span></div>
  const bounds = scene.reduce((b, p) => ({
    minE: Math.min(b.minE, p.east), maxE: Math.max(b.maxE, p.east),
    minN: Math.min(b.minN, p.north), maxN: Math.max(b.maxN, p.north),
    minD: Math.min(b.minD, p.tvd), maxD: Math.max(b.maxD, p.tvd),
  }), { minE: Infinity, maxE: -Infinity, minN: Infinity, maxN: -Infinity, minD: Infinity, maxD: -Infinity })
  const angle = rotation * Math.PI / 180, pitch = tilt * Math.PI / 180
  const project = (p: Point3) => {
    const e = p.east - (bounds.minE + bounds.maxE) / 2, n = p.north - (bounds.minN + bounds.maxN) / 2
    const z = (p.tvd - (bounds.minD + bounds.maxD) / 2) * exaggeration
    const across = e * Math.cos(angle) - n * Math.sin(angle), away = e * Math.sin(angle) + n * Math.cos(angle)
    return { x: across, y: away * Math.sin(pitch) + z * Math.cos(pitch), depth: away * Math.cos(pitch) - z * Math.sin(pitch) }
  }
  const projected = scene.map(project)
  const extents = projected.reduce((b, p) => ({ x: Math.max(b.x, Math.abs(p.x)), y: Math.max(b.y, Math.abs(p.y)) }), { x: 1, y: 1 })
  const scale = Math.min(355 / extents.x, 225 / extents.y)
  const screen = (p: Point3) => { const v = project(p); return `${(430 + v.x * scale).toFixed(2)},${(255 + v.y * scale).toFixed(2)}` }
  const faces = meshes.filter((m) => phase === 'all' || phase === m.fluid).flatMap((mesh) => mesh.triangles.flatMap((triangle) => {
    const points = cutAtDepth(triangle.map((i) => mesh.vertices[i]), cut)
    const projected = points.map(project)
    const winding = projected.reduce((sum, p, i) => {
      const q = projected[(i + 1) % projected.length]
      return sum + p.x * q.y - q.x * p.y
    }, 0)
    if (winding < 0) points.reverse()
    return points.length < 3 ? [] : [{
      fluid: mesh.fluid, depth: points.reduce((sum, p) => sum + project(p).depth, 0) / points.length,
      d: `M${points.map(screen).join(' L')}Z`,
    }]
  })).sort((a, b) => b.depth - a.depth)
  const batches: Array<{ fluid: Fluid; d: string }> = []
  for (const face of faces) {
    const last = batches.at(-1)
    if (last?.fluid === face.fluid) last.d += face.d
    else batches.push({ fluid: face.fluid, d: face.d })
  }
  const line = (points: Point3[]) => points.slice(1).map((p, i) => {
    const clipped = cutAtDepth([points[i], p], cut)
    return clipped.length > 1 ? `M${screen(clipped[0])} L${screen(clipped[1])}` : ''
  }).join(' ')
  const warnings = wells.flatMap((w) => w.geometryWarnings.map((message) => `${w.name}: ${message}`))

  return <div className="reservoir-view">
    <div className="reservoir-controls" aria-label="Reservoir view controls">
      <label>Rotation {rotation}°<input type="range" min="0" max="360" value={rotation} onChange={(e) => setRotation(Number(e.target.value))} /></label>
      <label>Tilt {tilt}°<input type="range" min="10" max="80" value={tilt} onChange={(e) => setTilt(Number(e.target.value))} /></label>
      <label>Depth scale {exaggeration}×<input type="range" min="1" max="8" value={exaggeration} onChange={(e) => setExaggeration(Number(e.target.value))} /></label>
      <label>Cut above {cut} m TVD<input type="range" min="0" max={Math.ceil(bounds.maxD)} step="10" value={cut} onChange={(e) => setCut(Number(e.target.value))} /></label>
      <label>Fluid<select value={phase} onChange={(e) => setPhase(e.target.value)}>
        <option value="all">All fluids</option><option value="gas">Gas</option><option value="oil">Oil</option><option value="water">Water</option>
      </select></label>
      <label>Mesh radius · view only<select value={radius} onChange={(e) => setRadius(Number(e.target.value))}>
        {[3000, 6000, 10000].map((r) => <option key={r} value={r}>{r / 1000} km</option>)}
      </select></label>
      <label className="reservoir-wire"><input type="checkbox" checked={wireframe} onChange={(e) => setWireframe(e.target.checked)} />Mesh edges</label>
      <button type="button" onClick={() => { setRotation(35); setTilt(30); setExaggeration(3); setCut(0); setPhase('all'); setRadius(10000); setWireframe(false) }}>Reset view</button>
    </div>
    {model.error && <p role="alert" className="reservoir-notice">{model.error}</p>}
    <p className="reservoir-notice" role="status">
      Model-estimated fluid volumes · {model.result?.loggedControls ?? 0} classified, surveyed controls · {model.result?.excludedBores ?? wells.length} bores excluded from this mesh.
      {' '}Each fluid needs three independent, non-collinear positive controls. Known non-pay constrains it; unknown is not dry.
    </p>
    <div className="reservoir-canvas" tabIndex={0} role="region" aria-label="Reservoir scene, scroll horizontally on narrow screens">
    <svg className="reservoir-scene" viewBox="0 0 860 530" aria-label="Interactive estimated reservoir volumes and survey trajectories" role="group">
      <title>Visible-evidence reservoir mesh</title>
      <rect className="plot-bed" width="860" height="530" />
      {batches.map((batch, i) => <path key={i} className={`reservoir-mesh ${batch.fluid} ${wireframe ? 'wire' : ''}`} d={batch.d} />)}
      {wells.filter((w) => w.stations.length).map((well) => <g key={well.id} className={`trajectory-well ${well.id === selectedWellId ? 'selected' : ''}`}
        role="button" tabIndex={0} aria-label={`Select ${well.name}`} onClick={() => onWell(well.id)}
        onKeyDown={(event) => { if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); onWell(well.id) } }}>
        <path className="trajectory-line" d={line(well.stations)} />
        <title>{well.name}</title>
        {well.id === selectedWellId && <text className="well-label" transform={`translate(${screen(well.stations.at(-1)!)} )`} dx="8">{well.name}</text>}
      </g>)}
      {proposed.length > 1 && <g className="proposed-path"><path d={line(proposed)} /><title>Approved proposed path</title></g>}
      <text x="18" y="24" className="plot-label">Field-local east/north · supplied TVD datum · uniform {exaggeration}× depth scale</text>
      <text x="18" y="510" className="plot-label">E: {Math.round(bounds.minE)}…{Math.round(bounds.maxE)} m · N: {Math.round(bounds.minN)}…{Math.round(bounds.maxN)} m</text>
      {!faces.length && <text x="430" y="265" textAnchor="middle" className="plot-label">No supported fluid surface at these settings</text>}
    </svg>
    </div>
    <div className="reservoir-ledger" aria-label="Mesh geometry measurements">
      {meshes.map((mesh) => <div key={mesh.fluid}>
        <strong><i className={`legend ${mesh.fluid}`} />{mesh.fluid.toUpperCase()}</strong>
        <span>{(mesh.volumeM3 / 1e6).toFixed(2)} Mm³ gross geometric zone</span>
        <span>{mesh.triangles.length.toLocaleString()} facets · full mesh before depth cutting</span>
      </div>)}
    </div>
    <details className="reservoir-method">
      <summary>Method, unsupported evidence, and limitations</summary>
      <p>{geometryVersion}: finite-radius inverse-distance interpolation in a 24×24×32 stratigraphic grid;
        a 0.5 weighted phase indicator is extracted with marching tetrahedra. The indicator is not a probability.
        Outer edges include the search/support limit, not proven geological closure. Thin beds below grid resolution can disappear.</p>
      <p>Top/base depth follows covering surveys; samples use their interpolated downhole coordinates. Each selected formation occurrence is normalized top-to-base.
        Repeated occurrences, missing surveys, or tie-in coordinate disagreements are excluded rather than guessed.
        Common inter-well datum alignment, fault geometry, and calibrated uncertainty envelopes remain unresolved.
        Trajectories are drawn over the mesh for inspection. Volumes are not reserves.</p>
      <ul>{warnings.map((warning) => <li key={warning}>{warning}</li>)}</ul>
    </details>
  </div>
}
