import react from '@vitejs/plugin-react'
import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vite'

// Aspire injects the API address as services__api__http__0 when running under the AppHost.
// Running `npm run dev` on its own falls back to the API's launchSettings port.
const apiUrl = process.env.services__api__https__0 ?? process.env.services__api__http__0 ?? 'http://localhost:5180'

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: {
    port: process.env.PORT !== undefined ? Number(process.env.PORT) : 5173,
    proxy: {
      '/api': { target: apiUrl, changeOrigin: true, secure: false },
      '/hangfire': { target: apiUrl, changeOrigin: true, secure: false }
    }
  }
})
