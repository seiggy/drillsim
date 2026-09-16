import { useEffect, useId, useLayoutEffect, useRef, useState } from 'react'
import type { SelectionOption } from '../selection'
import './StatusSelect.css'

export function StatusSelect({ label, value, options, disabled, onChange }: {
  label: string
  value: string
  options: SelectionOption[]
  disabled?: boolean
  onChange: (value: string) => void
}) {
  const id = useId()
  const trigger = useRef<HTMLButtonElement>(null)
  const popup = useRef<HTMLDivElement>(null)
  const [open, setOpen] = useState(false)
  const [active, setActive] = useState(0)
  const [position, setPosition] = useState({ top: 0, left: 0, width: 320, maxHeight: 320 })
  const selected = options.find(option => option.value === value)
  const selectedIndex = Math.max(0, options.findIndex(option => option.value === value))
  const searching = useRef({ text: '', time: 0 })

  useLayoutEffect(() => {
    if (!open) return
    const place = () => {
      const box = trigger.current!.getBoundingClientRect()
      const width = Math.min(Math.max(box.width, 400), window.innerWidth - 16)
      const below = window.innerHeight - box.bottom - 16
      const maxHeight = Math.min(480, Math.max(below, box.top - 16))
      setPosition({ width, maxHeight, left: Math.max(8, Math.min(box.left, window.innerWidth - width - 8)),
        top: below >= Math.min(260, maxHeight) ? box.bottom + 6 : Math.max(8, box.top - maxHeight - 6) })
    }
    place()
    window.addEventListener('resize', place)
    window.addEventListener('scroll', place, true)
    return () => { window.removeEventListener('resize', place); window.removeEventListener('scroll', place, true) }
  }, [open])
  useEffect(() => {
    if (disabled) popup.current?.hidePopover()
  }, [disabled])
  useEffect(() => {
    if (open) popup.current?.querySelector<HTMLElement>(`[data-option-index="${active}"]`)?.scrollIntoView({ block: 'nearest' })
  }, [open, active])
  function choose(index: number) {
    const option = options[index]
    if (!option || disabled) return
    popup.current?.hidePopover()
    trigger.current?.focus({ preventScroll: true })
    if (option.value !== value) onChange(option.value)
  }
  function keyDown(event: React.KeyboardEvent) {
    let next: number | undefined
    if (event.key === 'ArrowDown') next = open ? Math.min(active + 1, options.length - 1) : selectedIndex
    else if (event.key === 'ArrowUp') next = open ? Math.max(0, active - 1) : selectedIndex
    else if (event.key === 'Home') next = 0
    else if (event.key === 'End') next = options.length - 1
    else if ((event.key === 'Enter' || event.key === ' ') && open) { event.preventDefault(); choose(active); return }
    else if (event.key === 'Escape' && open) {
      event.preventDefault(); event.stopPropagation(); popup.current?.hidePopover(); trigger.current?.focus(); return
    } else if (event.key === 'Tab') { popup.current?.hidePopover(); return }
    else if (event.key.length === 1 && !event.ctrlKey && !event.metaKey && !event.altKey) {
      const time = Date.now()
      searching.current.text = time - searching.current.time < 800 ? searching.current.text + event.key : event.key
      searching.current.time = time
      const index = options.findIndex(option => option.name.toLowerCase().startsWith(searching.current.text.toLowerCase()))
      if (index >= 0) next = index
    }
    if (next !== undefined) {
      event.preventDefault()
      if (!open) popup.current?.showPopover()
      setActive(next)
    }
  }
  return <div className="status-select">
    <span id={`${id}-label`} className="status-select-label">{label.toUpperCase()}</span>
    <button type="button" ref={trigger} role="combobox" aria-haspopup="listbox"
      aria-labelledby={`${id}-label`} aria-expanded={open} aria-controls={`${id}-options`}
      aria-activedescendant={open ? `${id}-option-${active}` : undefined}
      disabled={disabled || !options.length} popoverTarget={`${id}-options`}
      onClick={() => setActive(selectedIndex)} onKeyDown={keyDown}
      title={selected ? `${selected.name} · ${selected.badge}. ${selected.description}` : `Select ${label.toLowerCase()}`}>
      <span className="status-select-name">{selected?.name || `Select ${label.toLowerCase()}`}</span>
      {selected && <span className={`selection-badge ${selected.tone ?? ''}`}>{selected.badge}</span>}
      <span aria-hidden="true">▾</span>
    </button>
    <div id={`${id}-options`} ref={popup} popover="auto" role="listbox" aria-label={`${label} options`}
      className="status-select-options" style={position}
      onToggle={event => { setOpen(event.newState === 'open'); if (event.newState === 'open') trigger.current?.focus({ preventScroll: true }) }}
      onKeyDown={keyDown}>
      {options.map((option, index) => <div id={`${id}-option-${index}`} key={option.value}
        role="option" aria-selected={option.value === value} data-option-index={index}
        data-value={option.value} data-active={index === active} className="status-select-option"
        onPointerMove={() => setActive(index)} onMouseDown={event => event.preventDefault()} onClick={() => choose(index)}>
        <div><span className="status-select-option-name">{option.name}</span>
          <span className={`selection-badge ${option.tone ?? ''}`}>{option.badge}</span></div>
        <p>{option.description}</p>
      </div>)}
    </div>
  </div>
}
