import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// Locally: no PROXY_API_HOST → localhost:5001,5002,5003. Dev Spaces: PROXY_API_HOST = K8s service name (e.g. nfl-wallet-apis) → host:8080,8081,8082
const proxyHost = process.env.PROXY_API_HOST || 'localhost'
const isRemote = proxyHost !== 'localhost'
const customersTarget = isRemote ? `http://${proxyHost}:8080` : 'http://localhost:5001'
const billsTarget = isRemote ? `http://${proxyHost}:8081` : 'http://localhost:5002'
const raidersTarget = isRemote ? `http://${proxyHost}:8082` : 'http://localhost:5003'

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
