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
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import {
  getCustomer,
  getBillsBalance,
  getBillsTransactions,
  getRaidersBalance,
  getRaidersTransactions,
} from '../api/client'

const route = useRoute()
const customer = ref(null)
const billsBalance = ref(null)
const billsTransactions = ref([])
const raidersBalance = ref(null)
const raidersTransactions = ref([])
const loading = ref(true)
const error = ref('')

const id = computed(() => Number(route.params.id))

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
</style>
