import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// In Dev Spaces, PROXY_API_HOST=apis (K8s service name). Locally use localhost with default ports.
const proxyHost = process.env.PROXY_API_HOST || 'localhost'
const customersTarget = proxyHost === 'apis' ? 'http://apis:8080' : 'http://localhost:5001'
const billsTarget = proxyHost === 'apis' ? 'http://apis:8081' : 'http://localhost:5002'
const raidersTarget = proxyHost === 'apis' ? 'http://apis:8082' : 'http://localhost:5003'

export default defineConfig({
  plugins: [vue()],
  build: {
    outDir: 'dist',
    emptyOutDir: true,
  },
  server: {
    host: '0.0.0.0',
    port: 5173,
    allowedHosts: true,
    proxy: {
      '/api-customers': { target: customersTarget, changeOrigin: true, rewrite: (p) => p.replace(/^\/api-customers/, '/api') },
      '/api-bills': { target: billsTarget, changeOrigin: true, rewrite: (p) => p.replace(/^\/api-bills/, '/api') },
      '/api-raiders': { target: raidersTarget, changeOrigin: true, rewrite: (p) => p.replace(/^\/api-raiders/, '/api') },
    },
  },
})
