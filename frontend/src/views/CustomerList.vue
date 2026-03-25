<template>
  <div class="home">
    <section class="hero">
      <div class="container">
        <div class="hero-content">
          <img
            src="/sw.svg"
            alt="Stadium Wallet"
            class="hero-logo"
          />
          <h1 class="hero-title">Stadium Wallet</h1>
          <p class="hero-tagline">Your digital wallet for gameday inside football stadiums.</p>
          <div class="hero-copy">
            <p class="hero-description">
              <strong>Stadium Wallet</strong> is a digital wallet designed for fans attending football games.
              Pay for food, drinks, merchandise, and more—directly from your phone, without cash or cards.
              Built for a seamless in-stadium experience at every venue.
            </p>
            <p class="hero-wallet">
              Load funds, pay at concessions and team stores, and manage your
              Buffalo Bills and Las Vegas Raiders wallets—all in one place, ready to use inside the stadium.
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
      <template v-if="auth.enabled && !auth.authenticated">
        <div class="nfl-login-prompt">
          <h2 class="nfl-page-title">Welcome to Stadium Wallet</h2>
          <p class="nfl-subtitle">Sign in to access your wallet and manage your funds.</p>
          <button class="nfl-login-cta" @click="handleLogin">Sign in with NeuroFace</button>
        </div>
      </template>
      <template v-else-if="auth.enabled && auth.authenticated">
        <div v-if="loading" class="loading">Finding your wallet...</div>
        <div v-else-if="error" class="error">{{ error }}</div>
      </template>
      <template v-else>
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
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getCustomers } from '../api/client'
import { authState as auth, login as handleLogin } from '../auth/keycloak'

const router = useRouter()
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
  if (!url) return 'stadium-wallet.apk'
  const name = url.split('/').pop() || url.split('\\').pop()
  return name.includes('.') ? name : 'stadium-wallet.apk'
})

onMounted(async () => {
  const config = window.__API_CONFIG__ || {}
  if (config.mobileAppDownloadUrl) {
    mobileAppDownloadUrl.value = config.mobileAppDownloadUrl
  }

  if (auth.enabled && !auth.authenticated) {
    loading.value = false
    return
  }

  try {
    const all = await getCustomers()

    if (auth.enabled && auth.authenticated) {
      const myCustomer = all.find(c => c.email === auth.email)
      if (myCustomer) {
        router.replace({ name: 'wallets', params: { id: myCustomer.id } })
        return
      }
      error.value = `No wallet found for ${auth.email}. Contact support.`
    } else {
      customers.value = all
    }
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
.nfl-login-prompt { text-align: center; padding: 2rem 0; }
.nfl-login-cta {
  display: inline-block;
  margin-top: 1rem;
  padding: 0.75rem 2rem;
  background: #e31837;
  color: #fff;
  border: none;
  border-radius: 6px;
  font-size: 1.1rem;
  font-weight: 700;
  cursor: pointer;
  transition: background 0.2s;
}
.nfl-login-cta:hover { background: #c41430; }
</style>
