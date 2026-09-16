import { useState } from 'react'
import { downloadJson } from '../analysisConfiguration'

export function JsonDownload({ value, filename, children }: { value: unknown; filename: string; children: React.ReactNode }) {
  const [error, setError] = useState('')
  return <>
    <button type="button" onClick={() => {
      try { downloadJson(value, filename); setError('') }
      catch { setError('The download could not be created. Try again; the source data is unchanged.') }
    }}>{children}</button>
    {error && <span role="alert">{error}</span>}
  </>
}
