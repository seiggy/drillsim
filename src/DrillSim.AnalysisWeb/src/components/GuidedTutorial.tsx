import { useCallback, useEffect, useLayoutEffect, useRef, useState } from 'react'
import { tutorialChapters, tutorialCheckMessage, tutorialResumeIndex, tutorialStepSatisfied, type TutorialContext } from '../tutorial'
import type { TaskId } from '../sequencer'
import './GuidedTutorial.css'

interface Highlight {
  element: HTMLElement
  x: number
  y: number
  width: number
  height: number
  pressed: boolean
}

export function GuidedTutorial({ open, onClose, onNavigate, context, guideUrl }: {
  open: boolean
  onClose: () => void
  onNavigate: (task: TaskId) => void
  context: TutorialContext
  guideUrl: string
}) {
  const [chapterIndex, setChapterIndex] = useState(0)
  const [stepIndex, setStepIndex] = useState<number | null>(null)
  const [skipped, setSkipped] = useState<string[]>([])
  const [finished, setFinished] = useState(false)
  const [highlight, setHighlight] = useState<Highlight>()
  const [compact, setCompact] = useState(false)
  const heading = useRef<HTMLHeadingElement>(null)
  const panel = useRef<HTMLElement>(null)
  const opener = useRef<HTMLElement | null>(null)
  const closeCallback = useRef(onClose)
  closeCallback.current = onClose
  const navigate = useRef(onNavigate)
  navigate.current = onNavigate
  const chapter = tutorialChapters[chapterIndex]
  const step = stepIndex === null ? undefined : chapter.steps[stepIndex]
  const taskMatches = !step?.task || step.task === context.activeTask

  const close = useCallback(() => {
    closeCallback.current()
    const previous = opener.current
    requestAnimationFrame(() => { if (previous?.isConnected) previous.focus({ preventScroll: true }) })
  }, [])

  useEffect(() => {
    if (!open) return
    opener.current = document.activeElement instanceof HTMLElement ? document.activeElement : null
    const key = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && !event.defaultPrevented) { event.preventDefault(); close() }
    }
    document.addEventListener('keydown', key)
    return () => document.removeEventListener('keydown', key)
  }, [open, close])

  useEffect(() => {
    if (open) heading.current?.focus({ preventScroll: true })
  }, [open, chapterIndex, stepIndex, finished])

  useLayoutEffect(() => {
    if (!open || !panel.current) return
    const resize = () => {
      document.documentElement.style.setProperty('--tutorial-panel-height', `${panel.current!.getBoundingClientRect().height}px`)
    }
    const observer = new ResizeObserver(resize)
    observer.observe(panel.current)
    resize()
    return () => {
      observer.disconnect()
      document.documentElement.style.removeProperty('--tutorial-panel-height')
    }
  }, [open])

  useLayoutEffect(() => {
    setHighlight(undefined)
    if (!open || !step || !taskMatches) return
    let frame = 0
    let scrolled = false
    let observed: HTMLElement | undefined
    const resize = new ResizeObserver(() => schedule())
    function measure() {
      const target = [...document.querySelectorAll<HTMLElement>(step!.target)]
        .find(element => !element.closest('[hidden]') && element.getClientRects().length > 0)
      if (!target) { setHighlight(undefined); return }
      if (target !== observed) {
        resize.disconnect()
        resize.observe(target)
        observed = target
      }
      if (!scrolled) {
        target.scrollIntoView({ behavior: 'instant', block: 'start', inline: 'nearest' })
        scrolled = true
      }
      const bounds = target.getBoundingClientRect()
      const dock = panel.current?.getBoundingClientRect()
      const wide = window.matchMedia('(min-width: 1200px)').matches
      const maxX = wide && dock ? dock.left - 8 : window.innerWidth - 8
      const maxY = !wide && dock ? dock.top - 8 : window.innerHeight - 8
      const x = Math.max(6, bounds.left - 4), y = Math.max(6, bounds.top - 4)
      const next = {
        element: target, x, y, width: Math.max(0, Math.min(bounds.right + 4, maxX) - x),
        height: Math.max(0, Math.min(bounds.bottom + 4, maxY) - y),
        pressed: target.getAttribute('aria-pressed') === 'true',
      }
      setHighlight(previous => previous?.element === target && previous.x === x && previous.y === y &&
        previous.width === next.width && previous.height === next.height && previous.pressed === next.pressed ? previous : next)
    }
    function schedule() {
      cancelAnimationFrame(frame)
      frame = requestAnimationFrame(measure)
    }
    const root = document.querySelector('.portal')
    const observer = new MutationObserver(schedule)
    if (root) observer.observe(root, { childList: true, subtree: true, attributes: true,
      attributeFilter: ['hidden', 'aria-pressed', 'disabled', 'class', 'style'] })
    window.addEventListener('scroll', schedule, true)
    window.addEventListener('resize', schedule)
    schedule()
    return () => {
      cancelAnimationFrame(frame)
      observer.disconnect()
      resize.disconnect()
      window.removeEventListener('scroll', schedule, true)
      window.removeEventListener('resize', schedule)
    }
  }, [open, step, taskMatches, compact])

  function show(index: number) {
    const next = chapter.steps[index]
    setStepIndex(index)
    setFinished(false)
    setCompact(false)
    if (next.task) navigate.current(next.task)
  }
  function advance(skip = false) {
    if (stepIndex === null || !step) return
    if (skip) setSkipped(current => current.includes(step.id) ? current : [...current, step.id])
    if (stepIndex + 1 < chapter.steps.length) show(stepIndex + 1)
    else { setStepIndex(null); setFinished(true) }
  }
  function changeChapter(index: number) {
    setChapterIndex(index)
    setStepIndex(null)
    setFinished(false)
    setCompact(false)
    setSkipped([])
  }
  function focusTarget() {
    const target = highlight?.element
    if (!target) return
    const control = target.matches('button,input,select,textarea,a,[tabindex]') ? target
      : target.querySelector<HTMLElement>('button:not(:disabled),input:not(:disabled),select:not(:disabled),textarea:not(:disabled),a,[tabindex]')
    target.scrollIntoView({ behavior: 'instant', block: 'start', inline: 'nearest' })
    if (control) control.focus({ preventScroll: true })
  }
  const exerciseComplete = step ? tutorialStepSatisfied(step, context, highlight?.pressed) : false
  const canAdvance = Boolean(step && taskMatches && (!step.check || exerciseComplete))

  if (!open) return null
  return <>
    {step && taskMatches && highlight && highlight.width > 0 && highlight.height > 0 &&
      <div className="tutorial-highlight" aria-hidden="true" style={{
        left: highlight.x, top: highlight.y, width: highlight.width, height: highlight.height,
      }} />}
    <aside className={`tutorial-panel ${compact ? 'tutorial-compact' : ''}`} ref={panel}
      aria-label="Guided tutorial" aria-describedby={!step ? 'tutorial-safety' : undefined}>
      <header className="tutorial-header">
        <h2 ref={heading} tabIndex={-1}>{step?.title ?? (finished ? 'Chapter complete' : 'Guided tutorial')}</h2>
        <button type="button" onClick={close} aria-label="Close guided tutorial">Close</button>
      </header>
      <button type="button" className="tutorial-compact-toggle" onClick={() => setCompact(value => !value)}
        aria-expanded={!compact} aria-controls="tutorial-content">{compact ? 'Show instructions' : 'Collapse instructions'}</button>
      <div className="tutorial-content" id="tutorial-content">
        <label className="tutorial-chapter">Chapter
          <select aria-label="Chapter" value={chapterIndex} onChange={event => changeChapter(Number(event.target.value))}>
            {tutorialChapters.map((item, index) => <option key={item.id} value={index}>{index + 1}. {item.title}</option>)}
          </select>
        </label>
        {!step && <p id="tutorial-safety" className="tutorial-safety">Choose a chapter and follow the highlighted controls.
          You can close the tutorial at any time.</p>}
        {!step ? <>
          <p className="tutorial-duration">{chapter.duration} · {chapter.steps.length} short steps</p>
          {finished ? <>
            <p>You finished <strong>{chapter.title}</strong>{skipped.length ? ` with ${skipped.length} skipped step${skipped.length === 1 ? '' : 's'}` : ''}.</p>
            <button type="button" onClick={() => { setSkipped([]); show(0) }}>Restart chapter</button>
            {chapterIndex < tutorialChapters.length - 1 &&
              <button type="button" onClick={() => changeChapter(chapterIndex + 1)}>Choose next chapter</button>}
          </> : <>
            <p>{chapter.description}</p>
            {chapter.id === 'simulation' && context.scenarioId && <button type="button"
              onClick={() => show(tutorialResumeIndex(context))}>
              {['Revealed', 'Scored'].includes(context.scenarioStatus ?? '') ? 'Inspect this completed scenario' : 'Resume current scenario'}
            </button>}
            <ol className="tutorial-outline">{chapter.steps.map((item, index) => <li key={item.id}>
              <button type="button" onClick={() => show(index)}>{item.title}</button>
            </li>)}</ol>
            <button type="button" className="tutorial-primary" onClick={() => show(0)}>Start chapter</button>
          </>}
        </> : <>
          <p className="tutorial-progress">Step {stepIndex! + 1} of {chapter.steps.length} · {chapter.title}</p>
          <p className="tutorial-instruction">{step.instruction}</p>
          <dl className="tutorial-explanation">
            <dt>Why it matters</dt><dd>{step.why}</dd>
            <dt>What changes</dt><dd>{step.effect}</dd>
          </dl>
          <p className="tutorial-status" role="status">
            {context.loading ? 'Loading field data…'
              : context.error ? `The app reports: ${context.error}`
              : !taskMatches ? 'You opened another task. Return to this step or skip ahead.'
              : step.check && exerciseComplete ? tutorialCheckMessage(step, true)
              : !highlight ? step.missing ?? `“${step.targetName}” is unavailable for the selected data. You can skip this step.`
              : step.check ? tutorialCheckMessage(step, exerciseComplete)
              : `Highlighted: ${step.targetName}.`}
          </p>
          {!taskMatches && step.task &&
            <button type="button" onClick={() => navigate.current(step.task!)}>Return to this step</button>}
          <button type="button" disabled={!highlight || !taskMatches} onClick={focusTarget}>Go to highlighted control</button>
          {step.check && <button type="button" className="tutorial-skip" onClick={() => advance(true)}>Skip exercise</button>}
          {(!highlight || !taskMatches) && !step.check &&
            <button type="button" className="tutorial-skip" onClick={() => advance(true)}>Skip unavailable step</button>}
          <button type="button" className="tutorial-skip" onClick={() => { setStepIndex(null); setFinished(false) }}>Chapter outline</button>
        </>}
        <a className="tutorial-guide-link" href={guideUrl} target="_blank" rel="noreferrer">Open full demo and architecture guide</a>
      </div>
      {step && <footer className="tutorial-navigation" aria-label="Tutorial step navigation">
        <button type="button" disabled={stepIndex === 0} onClick={() => show(stepIndex! - 1)}>← Prev</button>
        <button type="button" className="tutorial-primary" disabled={!canAdvance} onClick={() => advance()}>
          {stepIndex === chapter.steps.length - 1 ? 'Finish chapter' : 'Next →'}
        </button>
      </footer>}
    </aside>
  </>
}
