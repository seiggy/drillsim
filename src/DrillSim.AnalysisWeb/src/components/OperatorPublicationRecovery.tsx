import { publicationRecoveryBlock, publicationRecoveryKey, recoveryAuditError, recoveryReasonError } from '../operator'
import type { OperatorView } from '../operator'

export function OperatorPublicationRecovery({ view, scenarioId, actor, reason, blocked, reviewedKey, onReason, onReview, onRecover }: {
  view: OperatorView
  scenarioId: string
  actor: string
  reason: string
  blocked: boolean
  reviewedKey: string
  onReason: (reason: string) => void
  onReview: (key: string) => void
  onRecover: () => void
}) {
  const review = view.publicationRecovery
  if (!review) return null
  const unavailable = publicationRecoveryBlock(view, scenarioId)
  const inputError = recoveryAuditError(actor) || recoveryReasonError(reason)
  const key = publicationRecoveryKey(view)
  const checked = !unavailable && reviewedKey === key
  return <section className="operator-completion" aria-labelledby="publication-recovery-title">
    <h4 id="publication-recovery-title">Recover staged publication</h4>
    <p>Recovery reopens this rejected S8 publication for a separate retry. It preserves the same run, staged plan, payloads,
      completed checkpoints and verified writes. It does not publish evidence, advance the clock, or rerun drilling.</p>
    <p>Scenario <code>{review.scenarioId}</code> · run <code>{review.runId}</code></p>
    <dl className="dense-readout">
      <div><dt>COMPLETED STAGES</dt><dd>{review.completedStageCount} / 8 prerequisites</dd></div>
      <div><dt>OPERATIONS</dt><dd>{review.operationCount}</dd></div>
      <div><dt>VERIFIED</dt><dd>{review.verifiedOperationCount}</dd></div>
      <div><dt>UNVERIFIED</dt><dd>{review.pendingOperationCount}</dd></div>
    </dl>
    <p>Review fingerprint <code>{review.reviewedPublicationHash || 'Unavailable'}</code></p>
    <p>Staged manifest <code>{review.stagedManifestSha256 || 'Unavailable'}</code></p>
    <p>Publication plan <code>{review.publicationPlanSha256 || 'Unavailable'}</code></p>
    <p className="operator-notice">{unavailable || review.reason}</p>
    <label className="operator-actor">Recovery reason
      <input type="text" maxLength={500} value={reason} disabled={blocked || Boolean(unavailable)} autoComplete="off"
        onChange={event => onReason(event.target.value)} />
    </label>
    {inputError && <p>{inputError}</p>}
    <label className="operator-check"><input type="checkbox" checked={checked}
      disabled={blocked || Boolean(unavailable || inputError)}
      onChange={event => onReview(event.target.checked ? key : '')} />
      I reviewed this exact staged publication fingerprint and the correction described above. Recover this same run without publishing it.
    </label>
    <div className="operator-action-row"><button type="button" disabled={blocked || Boolean(unavailable || inputError) || !checked}
      onClick={onRecover}>Recover reviewed publication</button></div>
  </section>
}
