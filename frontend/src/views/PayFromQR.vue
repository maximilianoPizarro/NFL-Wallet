<template>
  <div class="container mt-4">
    <router-link to="/" class="nfl-back-link">← Back to app</router-link>
    <div v-if="!walletType || !customerId" class="pay-qr-invalid">
      <p>Invalid payment link. Use the QR from a wallet to pay.</p>
    </div>
    <div v-else class="card pay-qr-card" :class="walletType === 'bills' ? 'nfl-card-bills' : 'nfl-card-raiders'">
      <div class="card-header">
        Pay from {{ walletLabel }}
      </div>
      <div class="card-body">
        <p class="pay-qr-meta">Customer ID: {{ customerId }}</p>
        <div class="pay-qr-form">
          <label>Amount to deduct (USD)</label>
          <input v-model.number="amount" type="number" min="0" step="0.01" placeholder="0.00" class="pay-qr-input" />
          <button type="button" class="pay-qr-btn" :disabled="loading || !isAmountValid" @click="confirmPay">
            {{ loading ? 'Processing...' : 'Confirm payment' }}
          </button>
        </div>
        <p v-if="error" class="pay-qr-error">{{ error }}</p>
        <p v-if="success" class="pay-qr-success">Payment successful. New balance: {{ successBalance }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { payBills, payRaiders } from '../api/client'

const route = useRoute()
const walletType = ref('')
const customerId = ref(null)
const amount = ref('')
const loading = ref(false)
const error = ref('')
const success = ref('')
const successBalance = ref('')

const walletLabel = computed(() =>
  walletType.value === 'bills' ? 'Buffalo Bills' : walletType.value === 'raiders' ? 'Las Vegas Raiders' : ''
)

const isAmountValid = computed(() => {
  const n = Number(amount.value)
  return !isNaN(n) && n > 0
})

function pay() {
  const base = walletType.value === 'bills' ? payBills : payRaiders
  return base(customerId.value, amount.value)
}

async function confirmPay() {
  if (!isAmountValid.value || loading.value) return
  loading.value = true
  error.value = ''
  success.value = ''
  try {
    const balance = await pay()
    success.value = 'OK'
    successBalance.value = `${balance.currency} ${Number(balance.availableBalance).toLocaleString('en-US', { minimumFractionDigits: 2 })}`
  } catch (e) {
    error.value = e.message || 'Error processing payment'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  walletType.value = (route.query.wallet || '').toLowerCase()
  const id = route.query.customerId
  customerId.value = id != null && id !== '' ? Number(id) : null
})
</script>

<style scoped>
.pay-qr-invalid { padding: 1.5rem; background: #fff3cd; border-radius: 8px; }
.pay-qr-card .card-header { font-size: 1.1rem; }
.pay-qr-meta { color: #555; margin-bottom: 1rem; }
.pay-qr-form label { display: block; font-weight: 600; margin-bottom: 0.35rem; }
.pay-qr-input { width: 100%; max-width: 200px; padding: 0.5rem; margin-bottom: 1rem; border: 1px solid #ccc; border-radius: 4px; font-size: 1rem; }
.pay-qr-btn {
  display: inline-block;
  padding: 0.5rem 1.25rem;
  background: var(--nfl-red);
  color: var(--nfl-white);
  font-weight: 600;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}
.pay-qr-btn:disabled { opacity: 0.6; cursor: not-allowed; }
.pay-qr-error { color: #c60c30; margin-top: 1rem; }
.pay-qr-success { color: #0d6e0d; margin-top: 1rem; font-weight: 600; }
</style>
