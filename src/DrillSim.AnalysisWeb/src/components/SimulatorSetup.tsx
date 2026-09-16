import { useState } from 'react'
import { simulatorSetupBlock, setupReviewKey, type SimulatorSettings, type SimulatorSetup as SetupView } from '../simulatorSetup'
import type { OperatorAttempt, OperatorView } from '../operator'
import './SimulationForms.css'

export function SimulatorSetup({ setup, view, actor, blocked, editingBlocked, pending, error, onPrepare }: {
  setup?: SetupView
  view: OperatorView
  actor: string
  blocked: boolean
  editingBlocked?: boolean
  pending?: OperatorAttempt
  error: string
  onPrepare: (settings: SimulatorSettings) => void
}) {
  const [selection, setSelection] = useState<SimulatorSettings>()
  const [reviewed, setReviewed] = useState('')
  const settings: SimulatorSettings = pending?.action === 'setup'
    ? { profileId: pending.profileId!, resolution: pending.resolution!, realizationSeed: pending.realizationSeed! }
    : selection ?? { profileId: setup?.defaults.profileId ?? '', resolution: setup?.defaults.resolution ?? 'Preview',
      realizationSeed: setup?.defaults.realizationSeed ?? Number.NaN }
  const reason = simulatorSetupBlock(setup, view, view.scenarioId, actor, settings)
  const key = setup ? JSON.stringify([setupReviewKey(setup, settings), actor.trim()]) : ''
  const prepared = Boolean(setup?.prepared || view.preflight.worldBound)
  const selectedProfile = setup?.profiles.find(profile => profile.profileId === settings.profileId)
  function edit(next: SimulatorSettings) { setSelection(next); setReviewed('') }
  return <section className="simulation-form simulator-setup" data-tutorial="simulator-setup" aria-labelledby="simulator-setup-title">
    <h4 id="simulator-setup-title">{prepared ? 'Simulator prepared' : 'Configure the simulator'}</h4>
    {prepared ? <>
      {setup?.current ? <dl className="dense-readout">
        <div><dt>MODEL PRESET</dt><dd>{setup.current.profileName}</dd></div>
        <div><dt>RESOLUTION</dt><dd>{setup.current.resolution} · {setup.current.resolution === 'Preview' ? '16 × 16 × 8' : '64 × 64 × 20'}</dd></div>
        <div><dt>REALIZATION SEED</dt><dd>{setup.current.realizationSeed}</dd></div>
        <div><dt>PREPARED BY</dt><dd>{setup.current.preparedBy}</dd></div>
      </dl> : <p>A compatible simulator setup is already bound to this scenario. Earlier setups may not include a public settings summary.</p>}
      <p>Settings are fixed for this scenario. Start is a separate action; use New simulation for a different model preset or resolution.</p>
    </> : <>
      <p>Choose a curated model preset and computational size. Preview uses 16 × 16 × 8 cells; Standard uses 64 × 64 × 20.
        The realization seed controls a reproducible synthetic realization, not your scenario’s name.</p>
      {error && <p className="form-error" role="alert">{error}</p>}
      <fieldset disabled={editingBlocked || !setup}>
        <legend>Model settings</legend>
        <div className="simulation-form-grid">
          <label>Model preset<select value={settings.profileId} onChange={event => edit({ ...settings, profileId: event.target.value })}>
            <option value="">Choose a preset</option>
            {setup?.profiles.map(profile => <option key={profile.profileId} value={profile.profileId}>{profile.name}</option>)}
          </select></label>
          <label>Simulation resolution<select value={settings.resolution}
            onChange={event => edit({ ...settings, resolution: event.target.value as SimulatorSettings['resolution'] })}>
            <option value="Preview">Preview · 16 × 16 × 8</option><option value="Standard">Standard · 64 × 64 × 20</option>
          </select></label>
          <label>Realization seed<input type="number" min={0} max={2147483647} step={1} required
            value={Number.isFinite(settings.realizationSeed) ? settings.realizationSeed : ''}
            onChange={event => edit({ ...settings, realizationSeed: event.target.value === '' ? Number.NaN : event.target.valueAsNumber })} /></label>
        </div>
        {selectedProfile && <p>{selectedProfile.description} Model version: {selectedProfile.worldModelVersion}.</p>}
      </fieldset>
      {reason && <p className="operator-notice">{reason}</p>}
      <label className="simulation-check"><input type="checkbox" checked={Boolean(key) && reviewed === key} disabled={blocked || Boolean(reason)}
        data-tutorial="review-simulator-setup" onChange={event => setReviewed(event.target.checked ? key : '')} />
        I reviewed these simulator settings and the approved prediction. Preparation fixes these settings for this scenario.</label>
      <button type="button" data-tutorial="prepare-simulator" disabled={blocked || Boolean(reason) || !key || reviewed !== key}
        onClick={() => { if (!reason && reviewed === key) { setReviewed(''); onPrepare(settings) } }}>Prepare simulator</button>
      <details><summary>Model and audit boundary</summary>
        <p>Preset calibration and the hidden geological model stay on the server. Preparation binds the exact approved seal and records the operator label; it neither starts drilling nor reveals new measurements.</p>
      </details>
    </>}
  </section>
}
