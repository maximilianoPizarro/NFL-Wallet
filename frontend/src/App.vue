<template>
  <div class="app">
    <header class="nfl-header">
      <div class="container header-inner">
        <router-link to="/" class="navbar-brand">
          <img src="/sw.svg" alt="Stadium Wallet" class="header-logo" />
          Stadium Wallet
        </router-link>
        <div class="header-actions">
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
          <template v-if="auth.enabled && auth.ready">
            <div v-if="auth.authenticated" class="header-user">
              <span class="header-user-avatar">{{ auth.fullName?.charAt(0) || auth.username?.charAt(0) || '?' }}</span>
              <span class="header-user-name">{{ auth.fullName || auth.username }}</span>
              <button class="header-btn header-btn-logout" @click="handleLogout">Logout</button>
            </div>
            <button v-else class="header-btn header-btn-login" @click="handleLogin">Login</button>
          </template>
        </div>
      </div>
    </header>
    <div v-if="games.length" class="espn-ticker" ref="tickerRef">
      <div class="espn-ticker-track" :style="trackStyle">
        <div
          v-for="(g, i) in tickerGames"
          :key="i"
          class="espn-ticker-card"
        >
          <div class="espn-ticker-team">
            <img v-if="g.away.logo" :src="g.away.logo" :alt="g.away.abbr" class="espn-ticker-logo" />
            <span class="espn-ticker-abbr">{{ g.away.abbr }}</span>
            <span class="espn-ticker-score">{{ g.away.score }}</span>
          </div>
          <span class="espn-ticker-vs">@</span>
          <div class="espn-ticker-team">
            <img v-if="g.home.logo" :src="g.home.logo" :alt="g.home.abbr" class="espn-ticker-logo" />
            <span class="espn-ticker-abbr">{{ g.home.abbr }}</span>
            <span class="espn-ticker-score">{{ g.home.score }}</span>
          </div>
          <span :class="['espn-ticker-status', g.statusClass]">{{ g.status }}</span>
        </div>
      </div>
    </div>
    <main>
      <router-view />
    </main>
    <footer class="nfl-footer">
      <div class="container">Stadium Wallet — Buffalo Bills & Las Vegas Raiders</div>
    </footer>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { getEspnScoreboard } from './api/client'
import { authState as auth, login, logout } from './auth/keycloak'

const mobileAppDownloadUrl = ref('')

function handleLogin() { login() }
function handleLogout() { logout() }
const games = ref([])
const tickerOffset = ref(0)
let animFrame = null
let lastTime = 0
const speed = 40

const isSameOriginDownload = computed(() => {
  const url = mobileAppDownloadUrl.value
  if (!url) return false
  return url.startsWith('/') || url.startsWith(window.location.origin)
})

const downloadFilename = computed(() => {
  const url = mobileAppDownloadUrl.value
  if (!url) return 'stadium-wallet.apk'
  const name = url.split('/').pop() || url.split('\\').pop()
  return name.includes('.') ? name : 'stadium-wallet.apk'
})

const tickerGames = computed(() => [...games.value, ...games.value])

const trackStyle = computed(() => ({
  transform: `translateX(-${tickerOffset.value}px)`,
}))

function parseGames(data) {
  if (!data?.events) return []
  return data.events.map(ev => {
    const comp = ev.competitions?.[0]
    const competitors = comp?.competitors || []
    const home = competitors.find(c => c.homeAway === 'home') || competitors[0] || {}
    const away = competitors.find(c => c.homeAway === 'away') || competitors[1] || {}
    const statusDesc = comp?.status?.type?.description || ev.status?.type?.description || ''
    const statusState = comp?.status?.type?.state || ''
    return {
      away: { abbr: away.team?.abbreviation || '?', score: away.score ?? '-', logo: away.team?.logo || '' },
      home: { abbr: home.team?.abbreviation || '?', score: home.score ?? '-', logo: home.team?.logo || '' },
      status: statusDesc,
      statusClass: statusState === 'in' ? 'live' : statusState === 'post' ? 'final' : 'scheduled',
    }
  })
}

function animate(time) {
  if (!lastTime) lastTime = time
  const dt = (time - lastTime) / 1000
  lastTime = time
  tickerOffset.value += speed * dt
  const cardW = 220
  const totalW = games.value.length * cardW
  if (totalW > 0 && tickerOffset.value >= totalW) {
    tickerOffset.value -= totalW
  }
  animFrame = requestAnimationFrame(animate)
}

onMounted(async () => {
  const config = window.__API_CONFIG__ || {}
  if (config.mobileAppDownloadUrl) {
    mobileAppDownloadUrl.value = config.mobileAppDownloadUrl
  }
  try {
    const data = await getEspnScoreboard()
    games.value = parseGames(data)
    if (games.value.length) {
      animFrame = requestAnimationFrame(animate)
    }
  } catch (_) {}
})

onBeforeUnmount(() => {
  if (animFrame) cancelAnimationFrame(animFrame)
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
.header-actions { display: flex; align-items: center; gap: 0.75rem; }
.header-user { display: flex; align-items: center; gap: 0.5rem; }
.header-user-avatar {
  width: 30px; height: 30px; border-radius: 50%;
  background: #e31837; color: #fff; font-weight: 800; font-size: 0.85rem;
  display: flex; align-items: center; justify-content: center; text-transform: uppercase;
}
.header-user-name { color: #fff; font-size: 0.85rem; font-weight: 600; }
.header-btn {
  font-size: 0.85rem; padding: 0.35rem 0.85rem; border: none; border-radius: 4px;
  cursor: pointer; font-weight: 700; transition: background 0.2s;
}
.header-btn-login { background: #e31837; color: #fff; }
.header-btn-login:hover { background: #c41430; }
.header-btn-logout { background: rgba(255,255,255,0.15); color: #fff; }
.header-btn-logout:hover { background: rgba(255,255,255,0.25); }
.header-logo {
  height: 32px;
  width: auto;
  margin-right: 0.6rem;
  vertical-align: middle;
}
.navbar-brand { display: inline-flex; align-items: center; }

.espn-ticker {
  background: linear-gradient(180deg, #0a0a0a 0%, #1a1a1a 100%);
  border-bottom: 3px solid #e31837;
  overflow: hidden;
  white-space: nowrap;
  padding: 0.4rem 0;
  position: relative;
}
.espn-ticker::before,
.espn-ticker::after {
  content: '';
  position: absolute;
  top: 0;
  bottom: 0;
  width: 40px;
  z-index: 2;
  pointer-events: none;
}
.espn-ticker::before { left: 0; background: linear-gradient(90deg, #0a0a0a 0%, transparent 100%); }
.espn-ticker::after { right: 0; background: linear-gradient(270deg, #0a0a0a 0%, transparent 100%); }

.espn-ticker-track {
  display: inline-flex;
  will-change: transform;
}
.espn-ticker-card {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.3rem 0.75rem;
  margin: 0 0.35rem;
  background: rgba(255,255,255,0.06);
  border-radius: 4px;
  border: 1px solid rgba(255,255,255,0.08);
  min-width: 200px;
  flex-shrink: 0;
}
.espn-ticker-team {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
}
.espn-ticker-logo {
  height: 32px;
  width: 32px;
  object-fit: contain;
}
.espn-ticker-abbr {
  color: #ccc;
  font-size: 0.85rem;
  font-weight: 700;
  letter-spacing: 0.02em;
}
.espn-ticker-score {
  color: #fff;
  font-size: 0.95rem;
  font-weight: 800;
  min-width: 1.2rem;
  text-align: center;
}
.espn-ticker-vs {
  color: #666;
  font-size: 0.7rem;
  margin: 0 0.1rem;
}
.espn-ticker-status {
  font-size: 0.65rem;
  font-weight: 600;
  padding: 0.1rem 0.35rem;
  border-radius: 3px;
  margin-left: auto;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
.espn-ticker-status.live {
  color: #fff;
  background: #e31837;
  animation: pulse-live 1.5s ease-in-out infinite;
}
.espn-ticker-status.final { color: #999; }
.espn-ticker-status.scheduled { color: #6cb4ee; }

@keyframes pulse-live {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.6; }
}
</style>
