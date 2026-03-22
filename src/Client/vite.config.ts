/// <reference types="vitest/config" />
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  test: {
    environment: 'happy-dom',
    include: ['src/**/*.test.ts'],
  },
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': 'http://localhost:5144',
      '/interactions': 'http://localhost:5144',
    },
  },
  build: {
    outDir: '../Apollo.API/wwwroot',
    emptyOutDir: true,
  },
})
