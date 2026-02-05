import { createApp } from 'vue'
import './assets/style.css'
import App from './App.vue'
import router from './router'

async function init() {
  try {
    const r = await fetch('/config.json')
    if (r.ok) {
      const config = await r.json()
      window.__API_CONFIG__ = config
    }
  } catch (_) {}
  createApp(App).use(router).mount('#app')
}
init()
