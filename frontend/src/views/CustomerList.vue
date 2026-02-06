<template>
  <div class="home">
    <!-- Hero: NFL + product intro (ESPN-style) -->
    <section class="hero">
      <div class="container">
        <div class="hero-content">
          <img
            src="https://upload.wikimedia.org/wikipedia/en/a/a2/National_Football_League_logo.svg"
            alt="National Football League"
            class="hero-logo"
          />
          <h1 class="hero-title">NFL Stadium Wallet</h1>
          <p class="hero-tagline">Your digital wallet for gameday at every NFL stadium.</p>
          <div class="hero-copy">
            <p class="hero-description">
              The <strong>National Football League (NFL)</strong> is a professional American football league in the United States. 
              Composed of 32 teams divided between the American Football Conference (AFC) and the National Football Conference (NFC), 
              the NFL is the highest level of professional American football in the world.
            </p>
            <p class="hero-wallet">
              This app is your <strong>stadium wallet</strong>: load funds, pay at concessions and team stores, 
              and manage your Buffalo Bills and Las Vegas Raiders wallets—all in one place for use inside NFL venues.
            </p>
          </div>
          <a
            v-if="mobileAppDownloadUrl"
            :href="mobileAppDownloadUrl"
            :download="isSameOriginDownload ? downloadFilename : undefined"
            :target="isSameOriginDownload ? undefined : '_blank'"
            :rel="isSameOriginDownload ? undefined : 'noopener noreferrer'"
            class="hero-download-apk"
          >
            Download mobile app (APK)
          </a>
        </div>
      </div>
    </section>

    <div class="container mt-4">
      <h2 class="nfl-page-title">Select a customer</h2>
      <p class="nfl-subtitle">Choose a customer to view Buffalo Bills and Las Vegas Raiders wallets.</p>
      <div v-if="loading" class="loading">Loading...</div>
      <div v-else-if="error" class="error">{{ error }}</div>
      <div v-else class="nfl-customer-list">
        <router-link
          v-for="c in customers"
          :key="c.id"
          :to="{ name: 'wallets', params: { id: c.id } }"
          class="list-group-item list-group-item-action"
        >
          <span class="nfl-customer-name"><strong>{{ c.lastName }}, {{ c.firstName }}</strong></span>
          <span class="nfl-customer-email">{{ c.email }}</span>
          <span class="nfl-customer-action">View wallets →</span>
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { getCustomers } from '../api/client'

const customers = ref([])
const loading = ref(true)
const error = ref('')
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

onMounted(async () => {
  const config = window.__API_CONFIG__ || {}
  if (config.mobileAppDownloadUrl) {
    mobileAppDownloadUrl.value = config.mobileAppDownloadUrl
  }
  try {
    customers.value = await getCustomers()
  } catch (e) {
    error.value = e.message || 'Failed to load customers'
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.home { padding-bottom: 2rem; }

/* ESPN-style hero: dark bar, bold type, red accent */
.hero {
  background: linear-gradient(180deg, #0c0c0c 0%, #1a1a1a 50%, #222 100%);
  border-bottom: 4px solid #e31837;
  padding: 2rem 0 2.5rem;
  margin-bottom: 2rem;
}
.hero-content { text-align: center; max-width: 720px; margin: 0 auto; }
.hero-logo {
  display: block;
  height: 72px;
  width: auto;
  margin: 0 auto 1rem;
  filter: drop-shadow(0 2px 8px rgba(0,0,0,0.4));
}
.hero-title {
  font-family: "ESPN", "Arial Black", "Helvetica Neue", sans-serif;
  font-size: clamp(1.75rem, 4vw, 2.5rem);
  font-weight: 900;
  color: #fff;
  margin: 0 0 0.5rem;
  letter-spacing: 0.02em;
  text-transform: uppercase;
}
.hero-tagline {
  font-size: 1.1rem;
  color: #e31837;
  font-weight: 700;
  margin: 0 0 1.5rem;
  letter-spacing: 0.03em;
}
.hero-copy { text-align: left; }
.hero-description,
.hero-wallet {
  color: #ccc;
  font-size: 0.95rem;
  line-height: 1.6;
  margin: 0 0 1rem;
}
.hero-description strong,
.hero-wallet strong { color: #fff; }
.hero-wallet { margin-bottom: 0; }

.hero-download-apk {
  display: inline-block;
  margin-top: 1.25rem;
  padding: 0.6rem 1.25rem;
  background: #e31837;
  color: #fff;
  font-weight: 700;
  text-decoration: none;
  border-radius: 6px;
  font-size: 1rem;
}
.hero-download-apk:hover { background: #c41430; color: #fff; }

.loading, .error { padding: 1rem; }
.error { color: #e31837; }
</style>
