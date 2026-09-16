import { readPredictionForm, type PredictionFormDocument } from '../predictionExamples'
import './SimulationForms.css'

function NumericInput({ label, value, onChange, readOnly = false }: {
  label: string; value: number | null; onChange: (value: number | null) => void; readOnly?: boolean
}) {
  return <input type="number" aria-label={label} step="any" value={value ?? ''} readOnly={readOnly}
    onChange={event => onChange(event.target.value === '' ? null : event.target.valueAsNumber)} />
}

export function PredictionForecastForm({ text, disabled, onChange }: {
  text: string; disabled: boolean; onChange: (text: string) => void
}) {
  const document = readPredictionForm(text)
  if (!document) return <p className="prediction-error">The guided form cannot read this document. Your text is unchanged; use Advanced to inspect or repair the JSON.</p>
  const bound = Boolean(document.analysisBinding)
  function change(update: (body: PredictionFormDocument) => void) {
    if (disabled || !document) return
    const next = structuredClone(document)
    update(next)
    onChange(JSON.stringify(next, null, 2))
  }
  function quantiles(values: PredictionFormDocument['expectedPaydirtM'], label: string,
    onQuantile: (key: 'p90' | 'p50' | 'p10', value: number | null) => void, readOnly = false) {
    return <div className="forecast-quantiles">{(['p90', 'p50', 'p10'] as const).map(key =>
      <label key={key}>{key.toUpperCase()}<NumericInput label={`${label} ${key.toUpperCase()}`} value={values[key]}
        readOnly={readOnly} onChange={value => onQuantile(key, value)} /></label>)}</div>
  }
  return <div className="simulation-form forecast-form" data-tutorial="forecast-form">
    <fieldset disabled={disabled}>
      <legend>Well path · metres, positive down</legend>
      <p>{bound ? 'Every station stays at the selected target’s exact east/north position. Directional JSON edits remain visible and are blocked, not flattened.' : 'Supply at least two stations with increasing measured depth. MD is distance along the well; TVD is vertical depth.'}</p>
      <div className="scenario-table-wrap" role="region" tabIndex={0} aria-label="Editable well path">
        <table className="scenario-table"><thead><tr><th>Station</th><th>Measured depth (m)</th><th>Vertical depth (m)</th><th>Easting (m)</th><th>Northing (m)</th><th>Action</th></tr></thead>
          <tbody>{document.proposedWellPath.map((station, index) => <tr key={index}>
            <th scope="row">{index + 1}</th>
            {(['measuredDepthM', 'trueVerticalDepthM', 'eastingM', 'northingM'] as const).map((key, field) =>
              <td key={key}><NumericInput label={`Station ${index + 1} ${['measured depth', 'vertical depth', 'easting', 'northing'][field]} (m)`}
                value={station[key]} readOnly={bound && (key === 'eastingM' || key === 'northingM')}
                onChange={value => change(body => { body.proposedWellPath[index][key] = value })} /></td>)}
            <td><button type="button" aria-label={`Remove station ${index + 1}`} disabled={document.proposedWellPath.length <= 2}
              onClick={() => change(body => { body.proposedWellPath.splice(index, 1) })}>Remove</button></td>
          </tr>)}</tbody></table>
      </div>
      <button type="button" onClick={() => change(body => {
        const target = body.analysisBinding?.target
        body.proposedWellPath.push({ measuredDepthM: null, trueVerticalDepthM: null, eastingM: target?.eastingM ?? null, northingM: target?.northingM ?? null })
      })}>Add path station</button>
    </fieldset>
    <fieldset disabled={disabled}>
      <legend>Formation and expected thickness</legend>
      <p>Use increasing P90 ≤ P50 ≤ P10 values. Each formation base must be deeper than its corresponding top.</p>
      {document.formations.map((formation, index) => <div className="forecast-formation" key={index}>
        <label>Formation {index + 1}<input type="text" value={formation.formationName}
          onChange={event => change(body => { body.formations[index].formationName = event.target.value })} /></label>
        <div className="simulation-form-grid">
          <div><h4>Top · m TVD</h4>{quantiles(formation.topTrueVerticalDepthM, `Formation ${index + 1} top (m TVD)`,
            (key, value) => change(body => { body.formations[index].topTrueVerticalDepthM[key] = value }))}</div>
          <div><h4>Base · m TVD</h4>{quantiles(formation.baseTrueVerticalDepthM, `Formation ${index + 1} base (m TVD)`,
            (key, value) => change(body => { body.formations[index].baseTrueVerticalDepthM[key] = value }))}</div>
        </div>
        {document.formations.length > 1 && <button type="button" onClick={() => change(body => { body.formations.splice(index, 1) })}>Remove formation {index + 1}</button>}
      </div>)}
      <button type="button" onClick={() => change(body => { body.formations.push({
        formationName: '', topTrueVerticalDepthM: { p90: null, p50: null, p10: null }, baseTrueVerticalDepthM: { p90: null, p50: null, p10: null },
      }) })}>Add formation</button>
      <h4>Expected thickness · m</h4>
      {quantiles(document.expectedPaydirtM, 'Expected thickness (m)',
        (key, value) => change(body => { body.expectedPaydirtM[key] = value }), bound)}
      <p>{bound ? 'Copied exactly from the verified target; change the analysis and explicitly rebuild to change these values.' : 'Supply the expected-pay quantiles.'}
        {' '}Rock-screening thickness is not calibrated hydrocarbon pay or reserves.</p>
    </fieldset>
    <fieldset disabled={disabled}>
      <legend>Fluids and optional contacts</legend>
      <div className="forecast-fluids">{(['Oil', 'Gas', 'Water'] as const).map(fluid => <label className="simulation-check" key={fluid}>
        <input type="checkbox" checked={document.fluidClasses.includes(fluid)}
          onChange={event => change(body => { body.fluidClasses = event.target.checked ? [...body.fluidClasses, fluid] : body.fluidClasses.filter(item => item !== fluid) })} />{fluid}
      </label>)}</div>
      {document.contactPredictions.map((contact, index) => <div className="forecast-contact" key={index}>
        <label>Contact {index + 1}<select value={contact.contactType} onChange={event => change(body => {
          body.contactPredictions[index].contactType = event.target.value as typeof contact.contactType
        })}><option value="GOC">Gas–oil contact</option><option value="GWC">Gas–water contact</option><option value="OWC">Oil–water contact</option></select></label>
        {quantiles(contact.trueVerticalDepthM, `Contact ${index + 1} depth (m TVD)`,
          (key, value) => change(body => { body.contactPredictions[index].trueVerticalDepthM[key] = value }))}
        <button type="button" onClick={() => change(body => { body.contactPredictions.splice(index, 1) })}>Remove contact {index + 1}</button>
      </div>)}
      <button type="button" onClick={() => change(body => { body.contactPredictions.push({ contactType: 'OWC', trueVerticalDepthM: { p90: null, p50: null, p10: null } }) })}>Add optional contact forecast</button>
      {!document.contactPredictions.length && <p>No contact forecast supplied. Contact predictions are optional.</p>}
    </fieldset>
    <fieldset disabled={disabled}>
      <legend>Cumulative production forecast · m³</legend>
      <p>Totals from the start of production through years 1, 3 and 5, not monthly rates. Each phase’s cumulative volume must not decrease.</p>
      <div className="scenario-table-wrap" role="region" tabIndex={0} aria-label="Editable cumulative production forecast">
        <table className="scenario-table"><thead><tr><th>Year</th><th>Oil (m³)</th><th>Gas (m³)</th><th>Water (m³)</th></tr></thead>
          <tbody>{document.productionForecasts.map((forecast, index) => <tr key={index}>
            <th scope="row">{forecast.year ?? 'Not supplied'}</th>{(['oilM3', 'gasM3', 'waterM3'] as const).map(key =>
              <td key={key}><NumericInput label={`Year ${forecast.year} ${key.replace('M3', '')} cumulative (m³)`} value={forecast[key]}
                onChange={value => change(body => { body.productionForecasts[index][key] = value })} /></td>)}
          </tr>)}</tbody></table>
      </div>
    </fieldset>
    <fieldset disabled={disabled}>
      <legend>Reasoning and uncertainty</legend>
      <label>Prediction rationale<textarea rows={3} value={document.rationale}
        onChange={event => change(body => { body.rationale = event.target.value })} /></label>
      <label>Uncertainty assumptions (one per line)<textarea rows={4} value={document.uncertaintyAssumptions.join('\n')}
        onChange={event => change(body => { body.uncertaintyAssumptions = event.target.value.split('\n') })} /></label>
      <details><summary>Citations from the frozen evidence ({document.citedEvidenceIds.length})</summary>
        <ul>{document.citedEvidenceIds.map((id, index) => <li key={index}><code>{id}</code></li>)}</ul>
        <p>Use Advanced to add your own visible evidence citations; no newer observations can be cited.</p>
      </details>
    </fieldset>
  </div>
}
