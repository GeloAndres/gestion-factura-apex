/// <reference types="vitest/config" />
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// En desarrollo, /api se redirige a la API .NET para evitar CORS.
const apiTarget = process.env.VITE_API_PROXY_TARGET ?? 'http://localhost:5080'
const proxy = { '/api': { target: apiTarget, changeOrigin: true } }

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: { port: 5173, strictPort: true, proxy },
  preview: { port: 4173, proxy },
  // MUI + React suman ~240 kB gzip en un solo chunk; para una app interna es aceptable.
  build: { chunkSizeWarningLimit: 900 },
  test: { environment: 'node', include: ['src/**/*.test.ts'] },
})
