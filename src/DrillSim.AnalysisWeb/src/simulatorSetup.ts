import type { OperatorAttempt, OperatorView } from './operator'

export type SimulatorResolution = 'Preview' | 'Standard'
export interface SimulatorSettings {
  profileId: string
  resolution: SimulatorResolution
  realizationSeed: number
}
export interface SimulatorSetup {
  scenarioId: string
  available: boolean
  reason: string | null
  prepared: boolean
  current: (SimulatorSettings & { profileName: string; preparedBy: string; preparedUtc: string }) | null
  profiles: Array<{ profileId: string; name: string; description: string; worldModelVersion: string }>
  defaults: { profileId: string | null; resolution: SimulatorResolution; realizationSeed: number }
  reviewedSealHash: string | null
}

export const setupActorError = (actor: string) =>
  !/^[\x20-\x7e]{1,100}$/.test(actor.trim()) || /[^\x20-\x7e]/.test(actor)
    ? 'Enter an operator name or initials using 1–100 visible ASCII characters.' : ''

export function setupSettingsError(settings: Partial<SimulatorSettings>) {
  if (typeof settings.profileId !== 'string' || !settings.profileId.trim()) return 'Choose an available model preset.'
  if (!['Preview', 'Standard'].includes(settings.resolution ?? '')) return 'Choose Preview or Standard resolution.'
  if (!Number.isInteger(settings.realizationSeed) || settings.realizationSeed! < 0 || settings.realizationSeed! > 2147483647) {
    return 'Realization seed must be an integer from 0 to 2147483647.'
  }
  return ''
}

export function validateSimulatorSetup(value: SimulatorSetup, scenarioId: string): SimulatorSetup {
  const text = (item: unknown) => typeof item === 'string' && Boolean(item.trim())
  if (!value || value.scenarioId !== scenarioId) throw new Error('Simulator setup belongs to another scenario. Refresh before preparing.')
  if (typeof value.available !== 'boolean' || typeof value.prepared !== 'boolean' ||
      value.available && value.prepared || value.reason !== null && typeof value.reason !== 'string' ||
      value.reviewedSealHash !== null && !/^[a-f0-9]{64}$/i.test(value.reviewedSealHash) ||
      !Array.isArray(value.profiles) || !value.profiles.every(profile => profile && text(profile.profileId) &&
        text(profile.name) && typeof profile.description === 'string' && text(profile.worldModelVersion)) ||
      new Set(value.profiles.map(profile => profile.profileId)).size !== value.profiles.length ||
      !value.defaults || value.defaults.profileId !== null && !text(value.defaults.profileId) ||
      setupSettingsError({ ...value.defaults, profileId: value.defaults?.profileId ?? 'no-profile' }) ||
      value.current !== null && (!value.current || setupSettingsError(value.current) || !text(value.current.profileName) ||
        !text(value.current.preparedBy) || !Number.isFinite(Date.parse(value.current.preparedUtc)))) {
    throw new Error('Simulator setup is incomplete or unsupported. Refresh setup; no preparation is assumed.')
  }
  return value
}

export function setupReviewKey(setup: SimulatorSetup, settings: SimulatorSettings) {
  return JSON.stringify([setup.scenarioId, setup.reviewedSealHash, settings.profileId, settings.resolution, settings.realizationSeed])
}

export function simulatorSetupBlock(setup: SimulatorSetup | undefined, view: OperatorView | undefined,
  scenarioId: string, actor: string, settings: SimulatorSettings) {
  if (!setup || !view?.enabled || view.scenarioId !== scenarioId || setup.scenarioId !== scenarioId) {
    return 'Read the current simulator setup for this scenario before preparing.'
  }
  if (setup.prepared || view.preflight.worldBound) return 'This simulator is already prepared. Use a new scenario to change its settings.'
  if (view.run) return 'A run already exists. Inspect its progress or create a new scenario; this setup cannot be changed.'
  if (!view.prediction.sealed || !view.prediction.approved) return 'Save, seal and approve the reviewed prediction before preparing the simulator.'
  if (!setup.available) return setup.reason || 'Preparation is not available for this scenario. Refresh operator status.'
  if (!setup.reviewedSealHash || setup.reviewedSealHash !== view.prediction.sealHash) return 'The setup does not match the approved prediction seal. Refresh and review again.'
  const error = setupActorError(actor) || setupSettingsError(settings)
  if (error) return error
  if (!setup.profiles.some(profile => profile.profileId === settings.profileId)) return 'Choose a model preset offered for this field and reservoir.'
  return ''
}

export function setupRetryBlock(view: OperatorView | undefined, setup: SimulatorSetup | undefined, attempt: OperatorAttempt | undefined) {
  if (attempt?.action !== 'setup') return ''
  if (!view?.enabled || !setup || view.scenarioId !== attempt.scenarioId || setup.scenarioId !== attempt.scenarioId) {
    return 'Refresh this scenario’s simulator setup before replaying the retained preparation request.'
  }
  if (view.prediction.sealHash !== attempt.reviewedSealHash || setup.reviewedSealHash !== attempt.reviewedSealHash) {
    return 'The approved seal changed. Inspect current state and retire this attempt before reviewing a new setup; its saved seal will not be replaced.'
  }
  if (setup.current && (setup.current.profileId !== attempt.profileId || setup.current.resolution !== attempt.resolution ||
      setup.current.realizationSeed !== attempt.realizationSeed)) {
    return 'A different immutable setup is already prepared. Do not change or replay these settings; inspect the current setup and retire the local attempt.'
  }
  return ''
}
