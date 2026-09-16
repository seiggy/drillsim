import { recoveryAuditError, recoveryReasonError, scoreCorrectionBlock, scoreCorrectionKey } from '../operator'
import type { OperatorView } from '../operator'

export function OperatorScoreCorrection({ view, scenarioId, actor, reason, blocked, reviewedKey, onReason, onReview, onCorrect }: {
  view: OperatorView
  scenarioId: string
  actor: string
  reason: string
  blocked: boolean
  reviewedKey: string
  onReason: (reason: string) => void
  onReview: (key: string) => void
  onCorrect: () => void
}) {
  const review = view.scoreCorrection
  if (!review) return null
  const unavailable = scoreCorrectionBlock(view, scenarioId)
  const inputError = recoveryAuditError(actor) || recoveryReasonError(reason)
  const key = scoreCorrectionKey(view)
  const checked = !unavailable && reviewedKey === key
  return <section className="operator-completion" aria-labelledby="score-correction-title">
    <h4 id="score-correction-title">Append corrected evaluation</h4>
    <p>Preserve the rejected scorecard and its original inputs. Append a separately versioned correction to absolute-error
      bounds, then publish evaluation in a separate action. Drilling, the published reveal, and the clock remain unchanged.</p>
    <p>Scenario <code>{review.scenarioId}</code> · run <code>{review.runId}</code></p>
    <dl className="dense-readout">
      <div><dt>METRICS</dt><dd>{review.metricCount}</dd></div>
      <div><dt>INVALID METRICS</dt><dd>{review.invalidMetricCount}</dd></div>
      <div><dt>CHANGED METRICS</dt><dd>{review.changedMetricCount}</dd></div>
      <div><dt>CORRECTION</dt><dd>{review.correctionVersion || 'Unavailable'}</dd></div>
    </dl>
    <p>Review fingerprint <code>{review.reviewedCorrectionHash || 'Unavailable'}</code></p>
    <p>Retained scorecard <code>{review.rejectedScorecardId || 'Unavailable'}</code> · <code>{review.rejectedScorecardSha256 || 'Unavailable'}</code></p>
    <p>Original input <code>{review.scoringInputSha256 || 'Unavailable'}</code></p>
    <p>Corrected scorecard <code>{review.correctedScorecardId || 'Unavailable'}</code> · <code>{review.correctedScorecardSha256 || 'Unavailable'}</code></p>
    <p>Correction provenance envelope <code>{review.correctedInputSha256 || 'Unavailable'}</code></p>
    <p className="operator-notice">{unavailable || review.reason}</p>
    <label className="operator-actor">Correction reason
      <input type="text" maxLength={500} value={reason} disabled={blocked || Boolean(unavailable)}
        autoComplete="off" onChange={event => onReason(event.target.value)} />
    </label>
    {inputError && <p>{inputError}</p>}
    <label className="operator-check"><input type="checkbox" checked={checked}
      disabled={blocked || Boolean(unavailable || inputError)}
      onChange={event => onReview(event.target.checked ? key : '')} />
      I reviewed this exact correction and its retained original artifact. Append the correction without publishing evaluation.
    </label>
    <div className="operator-action-row"><button type="button" disabled={blocked || Boolean(unavailable || inputError) || !checked}
      onClick={onCorrect}>Append reviewed score correction</button></div>
  </section>
}
