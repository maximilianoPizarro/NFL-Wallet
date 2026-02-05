<template>
  <div class="container mt-4">
    <router-link to="/" class="nfl-back-link">← Back to Customers</router-link>
    <div v-if="loading" class="loading">Loading...</div>
    <div v-else-if="error" class="error">{{ error }}</div>
    <template v-else-if="customer">
      <div class="nfl-customer-header">
        <h1>{{ customer.firstName }} {{ customer.lastName }}</h1>
        <p class="nfl-customer-meta">{{ customer.email }} · Document: {{ customer.documentNumber }}</p>
      </div>
      <div class="row">
        <div class="col-md-6">
          <div class="card nfl-wallet-card nfl-card-bills mb-4">
            <div class="card-header">Buffalo Bills</div>
            <div class="card-body">
              <template v-if="billsBalance">
                <div class="nfl-balance-row">
                  <span class="nfl-balance-label">Available</span>
                  <span class="nfl-balance-value">{{ billsBalance.currency }} {{ formatAmount(billsBalance.availableBalance) }}</span>
                </div>
                <div class="nfl-balance-row">
                  <span class="nfl-balance-label">Pending</span>
                  <span class="nfl-balance-value">{{ billsBalance.currency }} {{ formatAmount(billsBalance.pendingBalance) }}</span>
                </div>
                <p class="small text-muted mt-2 mb-0">Updated: {{ formatDate(billsBalance.updatedAt) }}</p>
                <div class="nfl-wallet-actions">
                  <button type="button" class="nfl-action-btn" @click="showLoadBills = !showLoadBills">Load balance</button>
                  <button type="button" class="nfl-action-btn nfl-action-qr" @click="showQR('bills')">Share QR</button>
                </div>
                <div v-if="showLoadBills" class="nfl-load-form">
                  <label>Amount (USD)</label>
                  <input v-model.number="loadBillsAmount" type="number" min="0" step="0.01" placeholder="0.00" />
                  <span class="nfl-mock-card">Mock card: **** **** **** 1234</span>
                  <button type="button" class="nfl-submit-btn" :disabled="loadBillsSubmitting || !(loadBillsAmount > 0)" @click="submitLoad('bills')">
                    {{ loadBillsSubmitting ? 'Loading...' : 'Load' }}
                  </button>
                  <p v-if="loadBillsError" class="nfl-form-error">{{ loadBillsError }}</p>
                </div>
                <h3 class="nfl-transactions-title">Recent transactions</h3>
                <div v-for="t in billsTransactions.slice(0, 10)" :key="t.id" class="nfl-transaction-item">
                  <span class="nfl-transaction-desc">{{ t.description }} — {{ t.type }}</span>
                  <span :class="['nfl-transaction-amount', t.type === 'Credit' ? 'credit' : 'debit']">
                    {{ t.type === 'Credit' ? '+' : '-' }}{{ formatAmount(t.amount) }}
                    <span class="nfl-transaction-status">({{ t.status }})</span>
                  </span>
                </div>
              </template>
              <p v-else class="text-muted mb-0">No wallet data for this customer.</p>
            </div>
          </div>
        </div>
        <div class="col-md-6">
          <div class="card nfl-wallet-card nfl-card-raiders mb-4">
            <div class="card-header">Las Vegas Raiders</div>
            <div class="card-body">
              <template v-if="raidersBalance">
                <div class="nfl-balance-row">
                  <span class="nfl-balance-label">Available</span>
                  <span class="nfl-balance-value">{{ raidersBalance.currency }} {{ formatAmount(raidersBalance.availableBalance) }}</span>
                </div>
                <div class="nfl-balance-row">
                  <span class="nfl-balance-label">Pending</span>
                  <span class="nfl-balance-value">{{ raidersBalance.currency }} {{ formatAmount(raidersBalance.pendingBalance) }}</span>
                </div>
                <p class="small text-muted mt-2 mb-0">Updated: {{ formatDate(raidersBalance.updatedAt) }}</p>
                <div class="nfl-wallet-actions">
                  <button type="button" class="nfl-action-btn" @click="showLoadRaiders = !showLoadRaiders">Load balance</button>
                  <button type="button" class="nfl-action-btn nfl-action-qr" @click="showQR('raiders')">Share QR</button>
                </div>
                <div v-if="showLoadRaiders" class="nfl-load-form">
                  <label>Amount (USD)</label>
                  <input v-model.number="loadRaidersAmount" type="number" min="0" step="0.01" placeholder="0.00" />
                  <span class="nfl-mock-card">Mock card: **** **** **** 1234</span>
                  <button type="button" class="nfl-submit-btn" :disabled="loadRaidersSubmitting || !(loadRaidersAmount > 0)" @click="submitLoad('raiders')">
                    {{ loadRaidersSubmitting ? 'Loading...' : 'Load' }}
                  </button>
                  <p v-if="loadRaidersError" class="nfl-form-error">{{ loadRaidersError }}</p>
                </div>
                <h3 class="nfl-transactions-title">Recent transactions</h3>
                <div v-for="t in raidersTransactions.slice(0, 10)" :key="t.id" class="nfl-transaction-item">
                  <span class="nfl-transaction-desc">{{ t.description }} — {{ t.type }}</span>
                  <span :class="['nfl-transaction-amount', t.type === 'Credit' ? 'credit' : 'debit']">
                    {{ t.type === 'Credit' ? '+' : '-' }}{{ formatAmount(t.amount) }}
                    <span class="nfl-transaction-status">({{ t.status }})</span>
                  </span>
                </div>
              </template>
              <p v-else class="text-muted mb-0">No wallet data for this customer.</p>
            </div>
          </div>
        </div>
      </div>
      <!-- QR modal -->
      <div v-if="qrWallet" class="nfl-qr-overlay" @click.self="qrWallet = null">
        <div class="nfl-qr-modal">
          <h3>Scan to pay from {{ qrWallet === 'bills' ? 'Buffalo Bills' : 'Las Vegas Raiders' }}</h3>
          <img :src="qrDataUrl" alt="QR" class="nfl-qr-img" />
          <p class="nfl-qr-url">{{ payUrl }}</p>
          <button type="button" class="nfl-action-btn" @click="qrWallet = null">Close</button>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import QRCode from 'qrcode'
import {
  getCustomer,
  getBillsBalance,
  getBillsTransactions,
  getRaidersBalance,
  getRaidersTransactions,
  loadBillsBalance,
  loadRaidersBalance,
} from '../api/client'

const route = useRoute()
const customer = ref(null)
const billsBalance = ref(null)
const billsTransactions = ref([])
const raidersBalance = ref(null)
const raidersTransactions = ref([])
const loading = ref(true)
const error = ref('')

const showLoadBills = ref(false)
const showLoadRaiders = ref(false)
const loadBillsAmount = ref('')
const loadRaidersAmount = ref('')
const loadBillsSubmitting = ref(false)
const loadRaidersSubmitting = ref(false)
const loadBillsError = ref('')
const loadRaidersError = ref('')
const qrWallet = ref(null)
const qrDataUrl = ref('')
const payUrl = ref('')

const id = computed(() => Number(route.params.id))

function getPayUrl(wallet) {
  const base = typeof window !== 'undefined' ? window.location.origin : ''
  const path = (import.meta.env.BASE_URL || '/').replace(/\/$/, '') + '/pay'
  return `${base}${path}?wallet=${wallet}&customerId=${id.value}`
}

async function showQR(wallet) {
  payUrl.value = getPayUrl(wallet)
  qrDataUrl.value = await QRCode.toDataURL(payUrl.value, { width: 256 })
  qrWallet.value = wallet
}

async function submitLoad(wallet) {
  const isBills = wallet === 'bills'
  const amount = isBills ? loadBillsAmount.value : loadRaidersAmount.value
  if (!amount || Number(amount) <= 0) return
  if (isBills) {
    loadBillsSubmitting.value = true
    loadBillsError.value = ''
  } else {
    loadRaidersSubmitting.value = true
    loadRaidersError.value = ''
  }
  try {
    const loadFn = isBills ? loadBillsBalance : loadRaidersBalance
    await loadFn(id.value, amount)
    if (isBills) {
      billsBalance.value = await getBillsBalance(id.value)
      billsTransactions.value = await getBillsTransactions(id.value)
      showLoadBills.value = false
      loadBillsAmount.value = ''
    } else {
      raidersBalance.value = await getRaidersBalance(id.value)
      raidersTransactions.value = await getRaidersTransactions(id.value)
      showLoadRaiders.value = false
      loadRaidersAmount.value = ''
    }
  } catch (e) {
    const msg = e.message || 'Error loading balance'
    if (isBills) loadBillsError.value = msg
    else loadRaidersError.value = msg
  } finally {
    loadBillsSubmitting.value = false
    loadRaidersSubmitting.value = false
  }
}

async function load() {
  if (!id.value) return
  loading.value = true
  error.value = ''
  try {
    customer.value = await getCustomer(id.value)
    const [billsBal, billsTx, raidersBal, raidersTx] = await Promise.all([
      getBillsBalance(id.value).catch(() => null),
      getBillsTransactions(id.value).catch(() => []),
      getRaidersBalance(id.value).catch(() => null),
      getRaidersTransactions(id.value).catch(() => []),
    ])
    billsBalance.value = billsBal
    billsTransactions.value = Array.isArray(billsTx) ? billsTx : []
    raidersBalance.value = raidersBal
    raidersTransactions.value = Array.isArray(raidersTx) ? raidersTx : []
  } catch (e) {
    error.value = e.message || 'Failed to load data'
  } finally {
    loading.value = false
  }
}

function formatAmount(n) {
  return Number(n).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}
function formatDate(s) {
  if (!s) return ''
  const d = new Date(s)
  return isNaN(d.getTime()) ? s : d.toLocaleString()
}

onMounted(load)
watch(id, load)
</script>

<style scoped>
.loading, .error { padding: 1rem; }
.error { color: #c60c30; }
.nfl-wallet-actions { display: flex; gap: 0.5rem; margin: 1rem 0; flex-wrap: wrap; }
.nfl-action-btn {
  padding: 0.4rem 0.75rem;
  background: var(--nfl-navy);
  color: var(--nfl-white);
  border: none;
  border-radius: 4px;
  font-weight: 600;
  cursor: pointer;
  font-size: 0.9rem;
}
.nfl-action-qr { background: #0d6e0d; }
.nfl-load-form { background: #fff; padding: 1rem; border-radius: 6px; margin-bottom: 1rem; border: 1px solid #dee2e6; }
.nfl-load-form label { display: block; font-weight: 600; margin-bottom: 0.25rem; }
.nfl-load-form input { width: 100%; max-width: 160px; padding: 0.4rem; margin-bottom: 0.5rem; border: 1px solid #ccc; border-radius: 4px; }
.nfl-mock-card { display: block; font-size: 0.8rem; color: #666; margin-bottom: 0.5rem; }
.nfl-submit-btn {
  padding: 0.4rem 1rem;
  background: var(--nfl-red);
  color: var(--nfl-white);
  border: none;
  border-radius: 4px;
  font-weight: 600;
  cursor: pointer;
}
.nfl-submit-btn:disabled { opacity: 0.6; cursor: not-allowed; }
.nfl-form-error { color: #c60c30; font-size: 0.9rem; margin-top: 0.5rem; }
.nfl-qr-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}
.nfl-qr-modal {
  background: #fff;
  padding: 1.5rem;
  border-radius: 8px;
  text-align: center;
  max-width: 90vw;
}
.nfl-qr-modal h3 { margin: 0 0 1rem; font-size: 1.1rem; }
.nfl-qr-img { display: block; margin: 0 auto 1rem; }
.nfl-qr-url { font-size: 0.75rem; word-break: break-all; color: #555; margin-bottom: 1rem; }
</style>
