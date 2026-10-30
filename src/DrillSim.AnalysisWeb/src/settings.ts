import { configurationErrors, defaultConfiguration } from './analysisConfiguration'
import type { AnalysisConfiguration } from './types'
import type { SimulatorResolution } from './simulatorSetup'

export const millidarcyM2 = 9.869233e-16

export interface AppSettings {
  operatorName: string
  analysis: AnalysisConfiguration
  simulatorResolution: SimulatorResolution
  autoApproveCompletion: boolean
  hideAutomatedRuns: boolean
}

export const defaultSettings: AppSettings = {
  operatorName: 'Demo analyst',
  analysis: { ...defaultConfiguration },
  simulatorResolution: 'Preview',
  autoApproveCompletion: true,
  hideAutomatedRuns: true,
}

const key = 'drillsim-settings-v2'

export function operatorNameError(name: string) {
  return /^[\x20-\x7e]{1,100}$/.test(name.trim()) ? '' : 'Use 1–100 plain characters (letters, numbers, spaces).'
}

export function loadSettings(): AppSettings {
  try {
    const value = JSON.parse(localStorage.getItem(key) ?? 'null') as Partial<AppSettings> | null
    if (!value) return { ...defaultSettings, analysis: { ...defaultConfiguration } }
    const analysis = { ...defaultConfiguration, ...value.analysis }
    return {
      operatorName: typeof value.operatorName === 'string' && !operatorNameError(value.operatorName) ? value.operatorName : defaultSettings.operatorName,
      analysis: configurationErrors(analysis).length ? { ...defaultConfiguration } : analysis,
      simulatorResolution: value.simulatorResolution === 'Standard' ? 'Standard' : 'Preview',
      autoApproveCompletion: value.autoApproveCompletion !== false,
      hideAutomatedRuns: value.hideAutomatedRuns !== false,
    }
  } catch { return { ...defaultSettings, analysis: { ...defaultConfiguration } } }
}

export function saveSettings(settings: AppSettings) {
  try { localStorage.setItem(key, JSON.stringify(settings)) } catch { /* Settings still apply for this session. */ }
}

export const isAutomatedRun = (label: string) => /\bautomated\b|fixture|live check/i.test(label)
