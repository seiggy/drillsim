import { completionApprovalBlock, completionAuditError, completionReviewKey } from '../operator'
import type { OperatorView } from '../operator'
import type { Scenario } from '../types'

const numeric = (value: number) => typeof value === 'number' && Number.isFinite(value) ? String(value) : 'Not supplied'

export function OperatorCompletionReview({ view, scenario, actor, blocked, reviewedKey, onReview, onApprove }: {
  view: OperatorView
  scenario: Scenario
  actor: string
  blocked: boolean
  reviewedKey: string
  onReview: (key: string) => void
  onApprove: () => void
}) {
  const review = view.completion
  const reason = completionApprovalBlock(view, scenario.scenarioId, scenario.reservoirName) || completionAuditError(actor)
  const identity = completionReviewKey(view)
  const checked = !reason && reviewedKey === identity
  const approved = review.available && review.status === 'Approved' && review.scenarioId === scenario.scenarioId && review.runId === view.run?.runId
  return <section className="operator-completion" data-tutorial="completion-review" aria-labelledby="completion-review-title">
    <h4 id="completion-review-title">Observable completion review</h4>
    <p>Review every opening below. Approval binds the displayed openings fingerprint and allows the production workflow to continue.
      It does not select a hidden world or approve an unseen completion design.</p>
    {review.available && <>
      <p>Completion status: {review.status ?? 'Not supplied'} · scenario <code>{review.scenarioId ?? 'Not supplied'}</code> · run <code>{review.runId ?? 'Not supplied'}</code>.</p>
      <p>Openings fingerprint <code>{review.openingsHash || 'Not supplied'}</code></p>
      <div className="scenario-table-wrap" tabIndex={0} role="region" aria-label="All observable completion opening parameters">
        <table className="scenario-table">
          <caption>All {review.openings.length} openings · numeric values are shown without display rounding</caption>
          <thead><tr><th scope="col">Reservoir</th><th scope="col">Type</th><th scope="col">Top m MD</th><th scope="col">Base m MD</th><th scope="col">Wellbore radius m</th><th scope="col">Skin</th><th scope="col">Efficiency</th><th scope="col">Uncertainty m</th></tr></thead>
          <tbody>{review.openings.map((opening, index) => <tr key={index}>
            <th scope="row">{opening.reservoirName || 'Not supplied'}</th><td>{opening.type || 'Not supplied'}</td>
            <td>{numeric(opening.topMdM)}</td><td>{numeric(opening.baseMdM)}</td><td>{numeric(opening.wellboreRadiusM)}</td>
            <td>{numeric(opening.skin)}</td><td>{numeric(opening.efficiency)}</td><td>{numeric(opening.uncertaintyM)}</td>
          </tr>)}</tbody>
        </table>
      </div>
    </>}
    {approved ? <p data-tutorial="completion-approved">Completion approved. The production workflow can continue; the reviewed openings remain unchanged.</p> : <>
    {reason && <p className="operator-notice">{reason}</p>}
    {!reason && <p>{review.reason}</p>}
    <label className="operator-check"><input type="checkbox" data-tutorial="review-completion" checked={checked} disabled={blocked || Boolean(reason)}
      onChange={(event) => onReview(event.target.checked ? identity : '')} />
      I reviewed every displayed opening and approve this exact fingerprint for this scenario and run.
    </label>
    <div className="operator-action-row"><button type="button" disabled={blocked || Boolean(reason) || !checked} onClick={onApprove}>Approve reviewed completion</button></div>
    </>}
  </section>
}
