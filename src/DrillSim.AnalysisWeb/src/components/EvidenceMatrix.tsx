import type { FieldPackage } from '../types'

export function EvidenceMatrix({ fieldPackage }: { fieldPackage: FieldPackage }) {
  const rows = [
    ['Clusters', fieldPackage.sourceCounts.clusters],
    ['Wells', fieldPackage.sourceCounts.wells],
    ['Wellbores', fieldPackage.sourceCounts.wellBores],
    ['Architecture', fieldPackage.sourceCounts.wellBoreArchitectures],
    ['Trajectories', fieldPackage.sourceCounts.trajectories],
    ['Geology', fieldPackage.sourceCounts.geologicalProperties],
  ]
  return (
    <section className="evidence-matrix" aria-labelledby="evidence-title">
      <header><h2 id="evidence-title">Evidence bank</h2><code>{fieldPackage.sha256.slice(0, 16)}</code></header>
      <div className="evidence-counts">
        {rows.map(([label, count]) => <div key={label}><span>{label}</span><strong>{count}</strong></div>)}
      </div>
      <div className="gap-strip">
        <strong>{fieldPackage.dataGaps.length ? 'Open gaps' : 'Package coherent'}</strong>
        <span>{fieldPackage.dataGaps[0] ?? 'No blocking relationship gaps detected.'}</span>
      </div>
    </section>
  )
}
