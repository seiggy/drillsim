import type { WellView } from './components/AnalysisWorkspace'

export interface SurveyPoint { md: number; east: number; north: number; tvd: number }
export type Fluid = 'gas' | 'oil' | 'water'
export type Point3 = Pick<SurveyPoint, 'east' | 'north' | 'tvd'>
export interface FluidMesh {
  fluid: Fluid
  vertices: Point3[]
  triangles: Array<[number, number, number]>
  volumeM3: number
}
export const geometryVersion = 'visible-interval-mesh-v1'
export const fluids: Fluid[] = ['gas', 'oil', 'water']
export interface RockCriteria { porosityCutoff: number; permeabilityCutoffM2: number }
export const defaultRockCriteria: RockCriteria = { porosityCutoff: .12, permeabilityCutoffM2: 9.869233e-16 }

export function positionAtMd(stations: SurveyPoint[], md: number): SurveyPoint | undefined {
  if (!stations.length || md < stations[0].md || md > stations.at(-1)!.md || !Number.isFinite(md)) return undefined
  let lo = 0, hi = stations.length - 1
  while (lo < hi) {
    const mid = Math.floor((lo + hi) / 2)
    if (stations[mid].md < md) lo = mid + 1
    else hi = mid
  }
  const after = stations[lo]
  if (after.md === md) return { ...after }
  const before = stations[lo - 1]
  if (!before || after.md <= before.md) return undefined
  const f = (md - before.md) / (after.md - before.md)
  return {
    md, east: before.east + f * (after.east - before.east),
    north: before.north + f * (after.north - before.north),
    tvd: before.tvd + f * (after.tvd - before.tvd),
  }
}

export function sampleClass(sample: WellView['samples'][number], criteria = defaultRockCriteria): Fluid | 'nonpay' | undefined {
  const quality = rockQuality(sample, criteria)
  if (quality === 'unknown') return undefined
  if (quality === 'below-cutoff') return 'nonpay'
  return fluidPhase(sample)
}

export function rockQuality(sample: WellView['samples'][number], criteria = defaultRockCriteria): 'qualifying' | 'below-cutoff' | 'unknown' {
  if (!sample.rockKnown) return 'unknown'
  return sample.porosity >= criteria.porosityCutoff && sample.permeabilityM2 >= criteria.permeabilityCutoffM2 ? 'qualifying' : 'below-cutoff'
}

export function fluidPhase(sample: WellView['samples'][number]): Fluid | undefined {
  if (!sample.fluidKnown) return undefined
  if (sample.water >= .55) return 'water'
  if (sample.gas >= .15 && sample.gas >= sample.oil) return 'gas'
  if (sample.oil >= .15) return 'oil'
  return undefined
}

function hasPlanarSupport(points: Point3[]) {
  if (points.length < 3) return false
  const a = points[0]
  const b = points.find((p) => Math.hypot(p.east - a.east, p.north - a.north) > 1)
  return b && points.some((c) => Math.abs((b.east - a.east) * (c.north - a.north) - (b.north - a.north) * (c.east - a.east)) > 1)
}

function nearestSample(samples: WellView['samples'], md: number, spacing: number, criteria: RockCriteria) {
  let lo = 0, hi = samples.length - 1
  while (lo < hi) {
    const mid = Math.floor((lo + hi) / 2)
    if (samples[mid].depth < md) lo = mid + 1
    else hi = mid
  }
  const a = samples[lo], b = samples[lo - 1]
  const sample = b && Math.abs(b.depth - md) < Math.abs(a.depth - md) ? b : a
  return sample && Math.abs(sample.depth - md) <= spacing * .75 ? sampleClass(sample, criteria) : undefined
}

export function buildReservoirMeshes(wells: WellView[], radiusM = 10000, resolution = 24) {
  if (!Number.isFinite(radiusM) || radiusM < 100 || radiusM > 20000 ||
      !Number.isInteger(resolution) || resolution < 8 || resolution > 32 || wells.length > 512) {
    throw new Error('Geometry supports 100–20000 m search distance, 8–32 grid divisions and at most 512 bores.')
  }
  const columns = wells.flatMap((well) => {
    if (well.formationRanges.length !== 1) return []
    const range = well.formationRanges[0]
    const top = positionAtMd(well.stations, range.top)
    const base = positionAtMd(well.stations, range.base)
    const middle = positionAtMd(well.stations, (range.top + range.base) / 2)
    if (!top || !base || !middle || base.tvd - top.tvd < 1) return []
    const samples = well.samples.filter((sample) => sample.depth >= range.top && sample.depth <= range.base)
    if (samples.some((sample, index) => index > 0 && sample.depth <= samples[index - 1].depth)) return []
    const deltas = samples.slice(1).map((s, i) => s.depth - samples[i].depth).filter((d) => d > 0).sort((a, b) => a - b)
    return [{ well, range, top, base, middle, samples, spacing: deltas[Math.floor(deltas.length / 2)] ?? 0 }]
  }).sort((a, b) => a.well.id.localeCompare(b.well.id))
  const logged = columns.filter((column) => column.samples.some((sample) => sampleClass(sample, column.well.criteria) !== undefined))
  const diagnostics = {
    surveyedControls: columns.length, loggedControls: logged.length, excludedBores: wells.length - columns.length,
    radiusM, resolution, verticalLayers: 32,
  }
  if (!hasPlanarSupport(logged.map((column) => column.middle))) return { meshes: [] as FluidMesh[], ...diagnostics }
  const minE = Math.min(...logged.map((c) => c.middle.east)) - radiusM
  const maxE = Math.max(...logged.map((c) => c.middle.east)) + radiusM
  const minN = Math.min(...logged.map((c) => c.middle.north)) - radiusM
  const maxN = Math.max(...logged.map((c) => c.middle.north)) + radiusM
  const points: Point3[] = []
  const values: number[][] = []
  const n = resolution, nz = diagnostics.verticalLayers
  const index = (x: number, y: number, z: number) => (z * (n + 1) + y) * (n + 1) + x

  // ponytail: finite-radius IDW on 32 stratigraphic layers; thin beds/faults need adaptive correlated horizons, not invented detail.
  for (let y = 0; y <= n; y++) for (let x = 0; x <= n; x++) {
    const east = minE + (maxE - minE) * x / n, north = minN + (maxN - minN) * y / n
    const structural = columns.map((column) => ({
      column, d: Math.hypot(east - column.middle.east, north - column.middle.north),
    }))
    const weights = structural.map((c) => 1 / Math.max(1, c.d ** 2))
    const weightSum = weights.reduce((sum, w) => sum + w, 0)
    const top = weightSum ? structural.reduce((sum, c, i) => sum + c.column.top.tvd * weights[i], 0) / weightSum : 0
    const base = weightSum ? structural.reduce((sum, c, i) => sum + c.column.base.tvd * weights[i], 0) / weightSum : 1
    for (let z = 0; z <= nz; z++) {
      const t = -.05 + 1.1 * z / nz
      const id = index(x, y, z)
      points[id] = { east, north, tvd: top + (base - top) * t }
      values[id] = [0, 0, 0]
      if (x === 0 || y === 0 || x === n || y === n || z === 0 || z === nz || t < 0 || t > 1) continue
      const support = logged.flatMap((column) => {
        const md = column.range.top + t * (column.range.base - column.range.top)
        const position = positionAtMd(column.well.stations, md)
        const classification = nearestSample(column.samples, md, column.spacing, column.well.criteria ?? defaultRockCriteria)
        if (!position || classification === undefined) return []
        const d = Math.hypot(east - position.east, north - position.north)
        return d < radiusM ? [{ position, classification, d }] : []
      }).sort((a, b) => a.d - b.d)
      const coincident = (a: typeof support[number], b: typeof support[number]) =>
        Math.hypot(a.position.east - b.position.east, a.position.north - b.position.north) < 1
      const independent = support.filter((s, i) => !support.slice(0, i).some((p) => coincident(s, p)) &&
        !support.some((p) => coincident(s, p) && s.classification !== p.classification))
      if (!hasPlanarSupport(independent.map((s) => s.position))) continue
      const sum = [0, 0, 0]
      let total = 0
      for (const s of independent) {
        const w = (1 - (s.d / radiusM) ** 2) ** 2 / Math.max(1, s.d ** 2)
        total += w
        if (s.classification !== 'nonpay') sum[fluids.indexOf(s.classification)] += w
      }
      values[id] = sum.map((v, phase) => {
        const positive = independent.filter((s) => s.classification === fluids[phase])
        if (!hasPlanarSupport(positive.map((s) => s.position))) return 0
        const taper = Math.min(1, (radiusM - positive[2].d) / (radiusM * .25))
        return v / total * taper
      })
    }
  }

  const meshes = fluids.map((fluid, phase): FluidMesh => {
    const vertices: Point3[] = [], triangles: Array<[number, number, number]> = []
    const edgeVertices = new Map<string, number>()
    function intersection(a: number, b: number) {
      if (a > b) [a, b] = [b, a]
      const f = (.5 - values[a][phase]) / (values[b][phase] - values[a][phase])
      const key = f === 0 ? `${a}` : f === 1 ? `${b}` : `${a}:${b}`
      const cached = edgeVertices.get(key)
      if (cached !== undefined) return cached
      const p = points[a], q = points[b]
      const id = vertices.length
      vertices.push({ east: p.east + f * (q.east - p.east), north: p.north + f * (q.north - p.north), tvd: p.tvd + f * (q.tvd - p.tvd) })
      edgeVertices.set(key, id)
      return id
    }
    function face(a: number, b: number, c: number, inside: Point3) {
      if (a === b || b === c || a === c) return
      const p = vertices[a], q = vertices[b], r = vertices[c]
      const normal = cross(subtract(q, p), subtract(r, p))
      if (dot(normal, normal) < 1e-14) return
      triangles.push(dot(normal, subtract(inside, p)) > 0 ? [a, c, b] : [a, b, c])
      if (triangles.length > 50000) throw new Error('Geometry exceeds 50,000 facets per phase. Reduce the grid resolution.')
    }
    for (let z = 0; z < nz; z++) for (let y = 0; y < n; y++) for (let x = 0; x < n; x++) {
      const cube = [index(x, y, z), index(x + 1, y, z), index(x + 1, y + 1, z), index(x, y + 1, z),
        index(x, y, z + 1), index(x + 1, y, z + 1), index(x + 1, y + 1, z + 1), index(x, y + 1, z + 1)]
      for (const tetra of [[0, 1, 2, 6], [0, 2, 3, 6], [0, 3, 7, 6], [0, 7, 4, 6], [0, 4, 5, 6], [0, 5, 1, 6]]) {
        const ids = tetra.map((i) => cube[i])
        const inside = ids.filter((i) => values[i][phase] > .5), outside = ids.filter((i) => values[i][phase] <= .5)
        if (!inside.length || !outside.length) continue
        if (inside.length === 1) {
          const v = outside.map((b) => intersection(inside[0], b))
          face(v[0], v[1], v[2], points[inside[0]])
        } else if (outside.length === 1) {
          const v = inside.map((a) => intersection(a, outside[0]))
          face(v[0], v[1], v[2], points[inside[0]])
        } else {
          const a = intersection(inside[0], outside[0]), b = intersection(inside[0], outside[1])
          const c = intersection(inside[1], outside[0]), d = intersection(inside[1], outside[1])
          face(a, b, c, points[inside[0]]); face(b, d, c, points[inside[1]])
        }
      }
    }
    const origin = vertices[0]
    const volumeM3 = origin ? Math.abs(triangles.reduce((sum, [a, b, c]) =>
      sum + dot(subtract(vertices[a], origin), cross(subtract(vertices[b], origin), subtract(vertices[c], origin))) / 6, 0)) : 0
    return { fluid, vertices, triangles, volumeM3 }
  }).filter((mesh) => mesh.triangles.length > 0)
  return { meshes, ...diagnostics }
}

function subtract(a: Point3, b: Point3): Point3 { return { east: a.east - b.east, north: a.north - b.north, tvd: a.tvd - b.tvd } }
function cross(a: Point3, b: Point3): Point3 { return { east: a.north * b.tvd - a.tvd * b.north, north: a.tvd * b.east - a.east * b.tvd, tvd: a.east * b.north - a.north * b.east } }
function dot(a: Point3, b: Point3) { return a.east * b.east + a.north * b.north + a.tvd * b.tvd }

export function cutAtDepth(polygon: Point3[], depth: number): Point3[] {
  const output: Point3[] = []
  polygon.forEach((p, i) => {
    const q = polygon[(i + 1) % polygon.length]
    if (p.tvd >= depth) output.push(p)
    if ((p.tvd < depth) !== (q.tvd < depth)) {
      const f = (depth - p.tvd) / (q.tvd - p.tvd)
      output.push({ east: p.east + f * (q.east - p.east), north: p.north + f * (q.north - p.north), tvd: depth })
    }
  })
  return output
}
