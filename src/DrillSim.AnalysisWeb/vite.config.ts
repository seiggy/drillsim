import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

const analysisApiUrl = process.env.ANALYSIS_API_URL ?? 'http://127.0.0.1:5050'

export default defineConfig({
  plugins: [react()],
  server: {
    open: false,
    proxy: {
      '/analysis-api': {
        target: analysisApiUrl,
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/analysis-api/, ''),
      },
    },
  },
})
