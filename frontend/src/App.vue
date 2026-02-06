<template>
  <div class="app">
    <header class="nfl-header">
      <div class="container header-inner">
        <router-link to="/" class="navbar-brand">
          <img src="https://upload.wikimedia.org/wikipedia/en/a/a2/National_Football_League_logo.svg" alt="NFL" class="header-logo" />
          NFL Stadium Wallet
        </router-link>
        <a
          v-if="mobileAppDownloadUrl"
          :href="mobileAppDownloadUrl"
          :download="isSameOriginDownload ? downloadFilename : undefined"
          :target="isSameOriginDownload ? undefined : '_blank'"
          :rel="isSameOriginDownload ? undefined : 'noopener noreferrer'"
          class="header-download-app"
        >
          Download APK
        </a>
      </div>
    </header>
    <main>
      <router-view />
    </main>
    <footer class="nfl-footer">
      <div class="container">NFL Wallet — Buffalo Bills & Las Vegas Raiders</div>
    </footer>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

const mobileAppDownloadUrl = ref('')

const isSameOriginDownload = computed(() => {
  const url = mobileAppDownloadUrl.value
  if (!url) return false
  return url.startsWith('/') || url.startsWith(window.location.origin)
})

const downloadFilename = computed(() => {
  const url = mobileAppDownloadUrl.value
  if (!url) return 'nfl-wallet.apk'
  const name = url.split('/').pop() || url.split('\\').pop()
  return name.includes('.') ? name : 'nfl-wallet.apk'
})

onMounted(() => {
  const config = window.__API_CONFIG__ || {}
  if (config.mobileAppDownloadUrl) {
    mobileAppDownloadUrl.value = config.mobileAppDownloadUrl
  }
})
</script>

<style scoped>
.app { min-height: 100vh; display: flex; flex-direction: column; }
main { flex: 1; padding: 1.5rem 0 2rem; }
.header-inner { display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 0.5rem; }
.header-download-app {
  font-size: 0.9rem;
  padding: 0.35rem 0.75rem;
  background: rgba(255,255,255,0.15);
  color: #fff;
  border-radius: 4px;
  text-decoration: none;
}
.header-download-app:hover { background: rgba(255,255,255,0.25); color: #fff; }
.header-logo {
  height: 32px;
  width: auto;
  margin-right: 0.6rem;
  vertical-align: middle;
}
.navbar-brand { display: inline-flex; align-items: center; }
</style>
