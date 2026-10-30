import { useEffect, useRef, useState } from 'react'
import * as atlas from 'azure-maps-control'
import 'azure-maps-control/dist/atlas.min.css'
import { idOf, nameOf, read } from '../data'
import type { FieldPackage, MapsStatus, RankedCandidate } from '../types'

type Position = [number, number]

function radiansToDegrees(value: number) {
  return value * 180 / Math.PI
}

function clusterPosition(cluster: Record<string, unknown>): Position | undefined {
  const point = read(cluster, 'ReferencePoint', 'referencePoint') as Record<string, unknown> | undefined
  const latitude = read(point, 'Latitude', 'latitude')
  const longitude = read(point, 'Longitude', 'longitude')
  return typeof latitude === 'number' && typeof longitude === 'number'
    ? [radiansToDegrees(longitude), radiansToDegrees(latitude)]
    : undefined
}

function fieldBoundary(field: Record<string, unknown>): Position[][] {
  const lines = read(field, 'DelineationLines', 'delineationLines')
  if (!Array.isArray(lines)) return []
  return lines.flatMap((line) => {
    const points = read(line as Record<string, unknown>, 'Points', 'points')
    if (!Array.isArray(points)) return []
    const positions = points.flatMap((point) => {
      const latitude = read(point as Record<string, unknown>, 'Latitude', 'latitude')
      const longitude = read(point as Record<string, unknown>, 'Longitude', 'longitude')
      return typeof latitude === 'number' && typeof longitude === 'number'
        ? [[radiansToDegrees(longitude), radiansToDegrees(latitude)] as Position]
        : []
    })
    return positions.length > 2 ? [positions] : []
  })
}

export function AzureFieldMap({
  fieldPackage,
  candidates,
  selectedCandidate,
  selectedWellId,
  referenceWellIds = [],
  mapsStatus,
  onCandidate,
  onWell,
}: {
  fieldPackage: FieldPackage
  candidates: RankedCandidate[]
  selectedCandidate?: RankedCandidate
  selectedWellId?: string
  /** Existing wells the selected target is interpolated from; drawn as rings tied to the target. */
  referenceWellIds?: string[]
  mapsStatus?: MapsStatus
  onCandidate: (candidate: RankedCandidate) => void
  onWell: (wellId: string) => void
}) {
  const container = useRef<HTMLDivElement>(null)
  const mapRef = useRef<atlas.Map | undefined>(undefined)
  const sourceRef = useRef<atlas.source.DataSource | undefined>(undefined)
  const fieldPackageRef = useRef(fieldPackage)
  const candidatesRef = useRef(candidates)
  const selectedCandidateRef = useRef(selectedCandidate)
  const selectedWellIdRef = useRef(selectedWellId)
  const referenceWellIdsRef = useRef(referenceWellIds)
  const onCandidateRef = useRef(onCandidate)
  const onWellRef = useRef(onWell)
  const fittedFieldIdRef = useRef('')
  const [error, setError] = useState('')

  fieldPackageRef.current = fieldPackage
  candidatesRef.current = candidates
  selectedCandidateRef.current = selectedCandidate
  selectedWellIdRef.current = selectedWellId
  referenceWellIdsRef.current = referenceWellIds
  onCandidateRef.current = onCandidate
  onWellRef.current = onWell

  function refreshSource(fitCamera: boolean) {
    const map = mapRef.current
    const source = sourceRef.current
    if (!map || !source) return
    const currentPackage = fieldPackageRef.current
    const positions: Position[] = []
    source.clear()
    fieldBoundary(currentPackage.field).forEach((boundary, index) => {
      positions.push(...boundary)
      source.add(new atlas.data.Feature(new atlas.data.LineString(boundary), { kind: 'boundary', index }))
    })
    const hasTargets = candidatesRef.current.length > 0
    const focus: Position[] = []
    const wellPositions = new Map<string, Position>()
    currentPackage.clusters.forEach((cluster) => {
      const position = clusterPosition(cluster)
      if (!position) return
      const clusterId = idOf(cluster)
      const well = currentPackage.wells.find((item) => String(read(item, 'ClusterID', 'clusterID')) === clusterId)
      const wellId = well ? idOf(well) : ''
      const reference = referenceWellIdsRef.current.includes(wellId)
      if (wellId) wellPositions.set(wellId, position)
      positions.push(position)
      source.add(new atlas.data.Feature(new atlas.data.Point(position), {
        kind: 'well',
        wellId,
        // With targets on the map, the shown well is a reference for the target, never the target itself.
        role: !hasTargets ? (wellId === selectedWellIdRef.current ? 'selected' : 'well')
          : reference ? (wellId === selectedWellIdRef.current ? 'shown-reference' : 'reference') : 'background',
        label: well ? nameOf(well).replace(/^SYNTH-\S+\s+/i, '') : nameOf(cluster).replace(/ location$/i, ''),
      }))
    })
    candidatesRef.current.forEach((candidate) => {
      const position: Position = [candidate.longitudeDegrees, candidate.latitudeDegrees]
      const selected = candidate.candidateId === selectedCandidateRef.current?.candidateId
      positions.push(position)
      if (selected) focus.push(position)
      if (selected) referenceWellIdsRef.current.forEach((wellId) => {
        const wellPosition = wellPositions.get(wellId)
        if (wellPosition) focus.push(wellPosition)
        if (wellPosition) source.add(new atlas.data.Feature(new atlas.data.LineString([position, wellPosition]), { kind: 'tie' }))
      })
      source.add(new atlas.data.Feature(new atlas.data.Point(position), {
        kind: 'candidate',
        rank: candidate.rank,
        role: selected ? 'target' : 'candidate',
        label: selected ? `#${candidate.rank} · drill here` : `#${candidate.rank}`,
      }))
    })
    const framed = focus.length > 1 ? focus : positions
    if (fitCamera && framed.length) {
      map.setCamera({ bounds: atlas.data.BoundingBox.fromPositions(framed), padding: 80, maxZoom: 13 })
    }
  }

  useEffect(() => {
    if (!container.current || !mapsStatus?.configured || !mapsStatus.clientId) return
    const styles = getComputedStyle(document.documentElement)
    const candidateColor = styles.getPropertyValue('--cp-step-red').trim()
    const selectedColor = styles.getPropertyValue('--cp-step-yellow').trim()
    const wellColor = styles.getPropertyValue('--cp-step-white').trim()
    const textColor = styles.getPropertyValue('--cp-machine-text').trim()
    const boundaryColor = styles.getPropertyValue('--cp-step-orange').trim()
    const mutedColor = styles.getPropertyValue('--cp-machine-muted').trim()
    const haloColor = styles.getPropertyValue('--cp-machine-deep').trim()
    const map = new atlas.Map(container.current, {
      style: 'satellite_road_labels',
      language: 'en-US',
      authOptions: {
        authType: atlas.AuthenticationType.anonymous,
        clientId: mapsStatus.clientId,
        getToken: (resolve, reject) => {
          fetch('/analysis-api/api/maps/token')
            .then((response) => {
              if (!response.ok) throw new Error(`Azure Maps token failed: ${response.status}`)
              return response.text()
            })
            .then(resolve)
            .catch(reject)
        },
      },
    })
    // Re-frame after layout changes so the selected target is not left at the edge of a resized panel.
    const resizeObserver = new ResizeObserver(() => { map.resize(); refreshSource(true) })
    resizeObserver.observe(container.current)
    mapRef.current = map

    map.events.add('ready', () => {
      const source = new atlas.source.DataSource()
      map.sources.add(source)
      sourceRef.current = source

      const boundaryLayer = new atlas.layer.LineLayer(source, undefined, {
        filter: ['==', ['get', 'kind'], 'boundary'],
        strokeColor: boundaryColor,
        strokeWidth: 3,
      })
      const tieLayer = new atlas.layer.LineLayer(source, undefined, {
        filter: ['==', ['get', 'kind'], 'tie'],
        strokeColor: boundaryColor,
        strokeWidth: 2,
        strokeDashArray: [2, 2],
      })
      const role: atlas.Expression = ['get', 'role']
      const pointLayer = new atlas.layer.BubbleLayer(source, undefined, {
        filter: ['any', ['==', ['get', 'kind'], 'well'], ['==', ['get', 'kind'], 'candidate']],
        color: ['match', role, 'target', selectedColor, 'selected', selectedColor, 'candidate', candidateColor,
          'background', mutedColor, wellColor],
        radius: ['match', role, 'target', 16, 'selected', 13, 'candidate', 10, 'shown-reference', 11, 'reference', 9, 'background', 5, 7],
        strokeColor: ['match', role, 'shown-reference', boundaryColor, 'reference', boundaryColor, textColor],
        strokeWidth: ['match', role, 'shown-reference', 5, 'reference', 3, 'background', 1, 2],
        opacity: ['match', role, 'background', 0.55, 1],
      })
      const targetLabelLayer = new atlas.layer.SymbolLayer(source, undefined, {
        filter: ['==', role, 'target'],
        iconOptions: { image: 'none' },
        textOptions: { textField: ['get', 'label'], color: selectedColor, haloColor: haloColor, haloWidth: 2,
          anchor: 'top', offset: [0, 1.3], size: 15, allowOverlap: true, ignorePlacement: true },
      })
      const labelLayer = new atlas.layer.SymbolLayer(source, undefined, {
        filter: ['all', ['!=', role, 'target'], ['any', ['==', ['get', 'kind'], 'candidate'],
          ['all', ['==', ['get', 'kind'], 'well'], ['!=', role, 'background']]]],
        iconOptions: { image: 'none' },
        textOptions: {
          textField: ['case', ['==', role, 'shown-reference'], ['concat', ['get', 'label'], ' · reference'], ['get', 'label']],
          color: textColor,
          haloColor: haloColor,
          haloWidth: 1.5,
          anchor: 'left',
          offset: [1.1, 0],
          size: ['match', role, 'target', 14, 12],
        },
      })
      map.layers.add([boundaryLayer, tieLayer, pointLayer, labelLayer, targetLabelLayer])
      map.events.add('click', pointLayer, (event) => {
        const shape = event.shapes?.[0]
        const properties = shape instanceof atlas.Shape ? shape.getProperties() : undefined
        if (properties?.kind === 'candidate') {
          const candidate = candidatesRef.current.find((item) => item.rank === Number(properties.rank))
          if (candidate) onCandidateRef.current(candidate)
        }
        if (properties?.kind === 'well' && properties.wellId) onWellRef.current(String(properties.wellId))
      })
      fittedFieldIdRef.current = `${fieldPackageRef.current.fieldId}:${selectedCandidateRef.current?.candidateId ?? ''}`
      refreshSource(true)
    })
    map.events.add('error', (event) => setError(String(event.error?.message ?? 'Azure Maps failed to load.')))
    return () => {
      sourceRef.current = undefined
      mapRef.current = undefined
      resizeObserver.disconnect()
      map.dispose()
    }
  }, [mapsStatus?.configured, mapsStatus?.clientId])

  useEffect(() => {
    const frame = `${fieldPackage.fieldId}:${selectedCandidate?.candidateId ?? ''}`
    const fitCamera = fittedFieldIdRef.current !== frame
    fittedFieldIdRef.current = frame
    refreshSource(fitCamera)
  }, [fieldPackage, candidates, selectedCandidate?.candidateId, selectedWellId, referenceWellIds.join(',')])

  if (!mapsStatus?.configured) {
    return (
      <div className="map-unconfigured">
        <strong>AZURE MAPS OFFLINE</strong>
        <p>Set <code>AZURE_MAPS_CLIENT_ID</code> on the analysis API to activate the top-down field map.</p>
      </div>
    )
  }
  return (
    <div className="azure-map-frame">
      <div ref={container} className="azure-map" role="application" aria-label="Azure Maps field view with wells and ranked targets" />
      {error && <p className="map-error" role="alert">{error}</p>}
    </div>
  )
}
