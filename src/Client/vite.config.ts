import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      // If you expose any HTTP endpoints (e.g., /api or OpenAPI), proxy them in dev:
      '/api': 'http://localhost:5144',
      '/interactions': 'http://localhost:5144',
    },
  },
  build: {
    // Build straight into ASP.NET's static files for production
    outDir: '../Apollo.API/wwwroot',
    emptyOutDir: true,
  },
})