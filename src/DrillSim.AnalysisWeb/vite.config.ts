import { readFileSync } from 'node:fs'
import { fileURLToPath } from 'node:url'
import { defineConfig, searchForWorkspaceRoot, type Plugin } from 'vite'
import react from '@vitejs/plugin-react'

const analysisApiUrl = process.env.ANALYSIS_API_URL ?? 'http://127.0.0.1:5050'

function guideScreenshots(): Plugin {
  return {
    name: 'guide-screenshots',
    apply: 'build',
    generateBundle(_, bundle) {
      const guideUrl = new URL('../../docs/index.html', import.meta.url)
      const source = readFileSync(fileURLToPath(guideUrl), 'utf8')
      const screenshots = new Set([...source.matchAll(/(?:src|href)="screenshots\/([a-z0-9-]+\.png)"/g)]
        .map(match => match[1]))
      if (!screenshots.size) return
      const guide = Object.values(bundle).find(item => item.type === 'asset' &&
        (typeof item.source === 'string' ? item.source : Buffer.from(item.source).toString('utf8')) === source)
      if (!guide) throw new Error('The demo guide asset is missing from the build.')
      const directory = guide.fileName.slice(0, guide.fileName.lastIndexOf('/') + 1)
      // The imported HTML is an opaque asset; Vite does not follow its relative image links.
      for (const name of screenshots) this.emitFile({
        type: 'asset',
        fileName: `${directory}screenshots/${name}`,
        source: readFileSync(fileURLToPath(new URL(`screenshots/${name}`, guideUrl))),
      })
    },
  }
}

export default defineConfig({
  plugins: [react(), guideScreenshots()],
  server: {
    open: false,
    fs: {
      allow: [searchForWorkspaceRoot(process.cwd()), fileURLToPath(new URL('../../docs/screenshots', import.meta.url))],
    },
    proxy: {
      '/analysis-api': {
        target: analysisApiUrl,
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/analysis-api/, ''),
      },
    },
  },
})
