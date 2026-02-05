import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// When public API URLs are set (Dev Spaces), browser calls APIs directly; no proxy (avoids ENOTFOUND nfl-wallet-apis).
const usePublicUrls = !!process.env.API_CUSTOMERS_URL
const proxyHost = process.env.PROXY_API_HOST || 'localhost'
const isRemote = !usePublicUrls && proxyHost !== 'localhost'
const customersTarget = isRemote ? `http://${proxyHost}:8080` : 'http://localhost:5001'
const billsTarget = isRemote ? `http://${proxyHost}:8081` : 'http://localhost:5002'
const raidersTarget = isRemote ? `http://${proxyHost}:8082` : 'http://localhost:5003'

const proxy = usePublicUrls
  ? {}
  : {
      '/api-customers': { target: customersTarget, changeOrigin: true, rewrite: (p) => p.replace(/^\/api-customers/, '/api') },
      '/api-bills': { target: billsTarget, changeOrigin: true, rewrite: (p) => p.replace(/^\/api-bills/, '/api') },
      '/api-raiders': { target: raidersTarget, changeOrigin: true, rewrite: (p) => p.replace(/^\/api-raiders/, '/api') },
    }

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
    proxy,
  },
})
