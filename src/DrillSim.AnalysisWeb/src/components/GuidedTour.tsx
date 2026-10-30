import type { Screen } from '../workflow'

const tour: Array<{ screen: Screen; title: string; purpose: string; doThis: string; lookFor: string }> = [
  { screen: 'field', title: '1 · Field', purpose: 'See what data we have before deciding anything.',
    doThis: 'Click a well in the list. Switch the view to 2D Section or Logs to see its reservoir and rock quality.',
    lookFor: 'How many wells have rock logs, and how much net pay they show. Few or inconsistent wells mean more uncertainty later.' },
  { screen: 'prospects', title: '2 · Prospects', purpose: 'Find the best place to drill a new well.',
    doThis: 'Click the targets on the left. Then ask the AI for a second opinion on why the target looks good and what could go wrong.',
    lookFor: 'The best estimate of net pay and how wide its range is. A wide range means the nearby wells disagree.' },
  { screen: 'forecast', title: '3 · Forecast', purpose: 'Write down what you expect the well to find, before it is drilled.',
    doThis: 'Click “Draft forecast with AI”, read its reasoning, adjust any value you disagree with, then lock the forecast.',
    lookFor: 'The reservoir depth the AI estimated from nearby wells, and the warnings it lists.' },
  { screen: 'drill', title: '4 · Drill', purpose: 'Drill the well in a hidden simulated reservoir you have never seen.',
    doThis: 'Click “Drill this well” and watch each step complete. It takes a few minutes.',
    lookFor: 'If something fails, the message says what happened and what to do next.' },
  { screen: 'results', title: '5 · Results', purpose: 'Compare the locked forecast with what the well actually found.',
    doThis: 'Read the headline, then compare the forecast with the simple rules of thumb.',
    lookFor: 'Whether the result fell inside the forecast range, and whether the forecast beat the simple methods.' },
]

export function GuidedTour({ open, screen, onNavigate, onClose }: { open: boolean; screen: Screen; onNavigate: (screen: Screen) => void; onClose: () => void }) {
  if (!open) return null
  const index = Math.max(0, tour.findIndex(step => step.screen === screen))
  const step = tour[index]
  return <aside className="tour" aria-labelledby="tour-title">
    <header><h2 id="tour-title">{step.title}</h2><button type="button" onClick={onClose} aria-label="Close tour">Close</button></header>
    <p className="tour-purpose">{step.purpose}</p>
    <h3>Do this</h3><p>{step.doThis}</p>
    <h3>Look for</h3><p>{step.lookFor}</p>
    {screen === 'settings' && <p className="fine-print">You are in Settings. Changes here re-score every target.</p>}
    <div className="button-row">
      <button type="button" disabled={index === 0} onClick={() => onNavigate(tour[index - 1].screen)}>Back</button>
      <button type="button" className="primary" disabled={index === tour.length - 1} onClick={() => onNavigate(tour[index + 1].screen)}>Next step</button>
    </div>
  </aside>
}
