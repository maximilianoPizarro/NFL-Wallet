import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  build: {
    outDir: 'dist',
    emptyOutDir: true,
  },
  server: {
    proxy: {
      '/api-customers': { target: 'http://localhost:5001', changeOrigin: true, rewrite: (p) => p.replace(/^\/api-customers/, '/api') },
      '/api-bills': { target: 'http://localhost:5002', changeOrigin: true, rewrite: (p) => p.replace(/^\/api-bills/, '/api') },
      '/api-raiders': { target: 'http://localhost:5003', changeOrigin: true, rewrite: (p) => p.replace(/^\/api-raiders/, '/api') },
    },
  },
})
