import { useState } from 'react'
import { configurationErrors, defaultConfiguration, withTargetFluid } from '../analysisConfiguration'
import { defaultSettings, millidarcyM2, operatorNameError, type AppSettings } from '../settings'
import type { AnalysisResult, TargetFluid } from '../types'

function Field({ label, help, children }: { label: string; help: string; children: React.ReactNode }) {
  return <label className="setting"><span className="setting-label">{label}</span>{children}<small>{help}</small></label>
}

export function SettingsScreen({ settings, analysis, onChange }: { settings: AppSettings; analysis: AnalysisResult; onChange: (next: AppSettings) => void }) {
  const [draft, setDraft] = useState(settings)
  const [saved, setSaved] = useState('')
  const config = draft.analysis
  const errors = [...configurationErrors(config), operatorNameError(draft.operatorName)].filter(Boolean)
  const changed = JSON.stringify(draft) !== JSON.stringify(settings)
  const setConfig = (patch: Partial<typeof config>) => { setDraft({ ...draft, analysis: { ...config, ...patch } }); setSaved('') }
  const number = (value: string) => value === '' ? NaN : Number(value)

  return <section className="screen-grid settings" aria-labelledby="settings-title">
    <header className="screen-head">
      <div><h1 id="settings-title">Settings</h1>
        <p>Everything adjustable lives here. Changing the analysis settings re-scores every target on Prospects.
          Runs already started keep the settings they were locked with.</p></div>
      <div className="button-row">
        <button type="button" onClick={() => { setDraft({ ...defaultSettings, operatorName: draft.operatorName, analysis: { ...defaultConfiguration } }); setSaved('') }}>Restore defaults</button>
        <button type="button" className="primary" disabled={!changed || errors.length > 0} onClick={() => { onChange(draft); setSaved('Saved. Prospects are being re-scored.') }}>Save settings</button>
      </div>
    </header>
    {saved && <p className="status-line" role="status">{saved}</p>}
    {errors.length > 0 && <ul className="app-error" role="alert">{errors.map(item => <li key={item}>{item}</li>)}</ul>}

    <section className="panel settings-group" aria-labelledby="rock-title">
      <h2 id="rock-title">What counts as good rock</h2>
      <p>Net pay is the thickness of rock that passes both cutoffs. Raise them to be stricter; targets and expected net pay will shrink.</p>
      <div className="settings-grid">
        <Field label="Minimum porosity (%)" help="How much open pore space the rock needs to hold oil or gas. Typical sandstone reservoirs: 10–30%.">
          <input type="number" min={0} max={100} step={1} value={Number.isFinite(config.porosityCutoff) ? +(config.porosityCutoff * 100).toFixed(4) : ''}
            onChange={event => setConfig({ porosityCutoff: number(event.target.value) / 100 })} /></Field>
        <Field label="Minimum permeability (mD)" help="How easily fluid flows through the rock, in millidarcies. Below about 1 mD, gas and oil barely flow.">
          <input type="number" min={0} step="any" value={Number.isFinite(config.permeabilityCutoffM2) ? +(config.permeabilityCutoffM2 / millidarcyM2).toPrecision(6) : ''}
            onChange={event => setConfig({ permeabilityCutoffM2: number(event.target.value) * millidarcyM2 })} /></Field>
        <Field label="Target hydrocarbon" help="Oil or gas counts only good rock holding that fluid, using the reservoir model's fluid contacts. Any counts all good rock.">
          <select value={config.targetFluid ?? ''} onChange={event => { setDraft({ ...draft, analysis: withTargetFluid(config, (event.target.value || undefined) as TargetFluid | undefined) }); setSaved('') }}>
            <option value="">Any hydrocarbon</option><option value="Oil">Oil</option><option value="Gas">Gas</option>
          </select></Field>
      </div>
    </section>

    <section className="panel settings-group" aria-labelledby="search-title">
      <h2 id="search-title">How targets are searched</h2>
      <p>Targets are points on a grid across the field. Each is scored from its nearest logged wells.</p>
      <div className="settings-grid">
        <Field label="Wells used per target" help="How many nearby logged wells each target averages. More wells smooth the estimate; fewer follow local detail.">
          <input type="number" min={1} max={32} step={1} value={Number.isFinite(config.idwNeighborCount) ? config.idwNeighborCount : ''} onChange={event => setConfig({ idwNeighborCount: number(event.target.value) })} /></Field>
        <Field label="Keep-away distance from existing wells (m)" help="Targets closer than this to a logged well are skipped, since drilling there adds little new information.">
          <input type="number" min={0} step={50} value={Number.isFinite(config.wellExclusionRadiusM) ? config.wellExclusionRadiusM : ''} onChange={event => setConfig({ wellExclusionRadiusM: number(event.target.value) })} /></Field>
        <Field label="Grid points per side" help="A finer grid tests more locations but takes longer. 15 gives 225 points.">
          <input type="number" min={2} max={51} step={1} value={Number.isFinite(config.gridPointsPerAxis) ? config.gridPointsPerAxis : ''} onChange={event => setConfig({ gridPointsPerAxis: number(event.target.value) })} /></Field>
      </div>
      <p className="fine-print">Current result: {analysis.candidateGrid?.length ?? 0} grid points, {analysis.wellSummaries.length} logged wells.</p>
    </section>

    <section className="panel settings-group" aria-labelledby="sim-title">
      <h2 id="sim-title">Simulator</h2>
      <div className="settings-grid">
        <Field label="Model detail" help="Preview runs in a few minutes. Standard is finer and slower.">
          <select value={draft.simulatorResolution} onChange={event => { setDraft({ ...draft, simulatorResolution: event.target.value as AppSettings['simulatorResolution'] }); setSaved('') }}>
            <option value="Preview">Preview (faster)</option><option value="Standard">Standard (slower, finer)</option></select></Field>
        <label className="setting check"><input type="checkbox" checked={draft.autoApproveCompletion}
          onChange={event => { setDraft({ ...draft, autoApproveCompletion: event.target.checked }); setSaved('') }} />
          <span><span className="setting-label">Approve the completion design automatically</span>
            <small>The simulator picks where to open the well to the reservoir. Turn off to review it yourself before production starts.</small></span></label>
      </div>
    </section>

    <section className="panel settings-group" aria-labelledby="you-title">
      <h2 id="you-title">You and your runs</h2>
      <div className="settings-grid">
        <Field label="Your name or initials" help="Recorded when you lock a forecast or drill a well.">
          <input type="text" maxLength={100} value={draft.operatorName} onChange={event => { setDraft({ ...draft, operatorName: event.target.value }); setSaved('') }} /></Field>
        <label className="setting check"><input type="checkbox" checked={draft.hideAutomatedRuns}
          onChange={event => { setDraft({ ...draft, hideAutomatedRuns: event.target.checked }); setSaved('') }} />
          <span><span className="setting-label">Hide automated test runs</span><small>Keeps the Run menu to runs people created.</small></span></label>
      </div>
    </section>
  </section>
}
