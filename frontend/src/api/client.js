function getBaseUrls() {
  const c = window.__API_CONFIG__ || {}
  return {
    customers: c.apiCustomersUrl || import.meta.env.VITE_API_CUSTOMERS_URL || '/api-customers',
    bills: c.apiBillsUrl || import.meta.env.VITE_API_BILLS_URL || '/api-bills',
    raiders: c.apiRaidersUrl || import.meta.env.VITE_API_RAIDERS_URL || '/api-raiders',
  }
}
// One API key per API; overridable via window.__API_CONFIG__.apiKeys (e.g. config.json)
function getApiKeys() {
  const c = window.__API_CONFIG__ || {}
  const keys = c.apiKeys || {}
  return {
    customers: keys.customers ?? 'nfl-wallet-customers-key',
    bills: keys.bills ?? 'nfl-wallet-bills-key',
    raiders: keys.raiders ?? 'nfl-wallet-raiders-key',
  }
}
const customersBase = () => getBaseUrls().customers
const billsBase = () => getBaseUrls().bills
const raidersBase = () => getBaseUrls().raiders

function headersWithApiKey(apiKey) {
  const h = { 'Content-Type': 'application/json' }
  if (apiKey) h['X-API-Key'] = apiKey
  return h
}

async function get(url, apiKey) {
  const res = await fetch(url, {
    headers: apiKey ? { 'X-API-Key': apiKey } : {},
  })
  if (!res.ok) throw new Error(res.statusText)
  return res.json()
}

async function post(url, body, apiKey) {
  const res = await fetch(url, {
    method: 'POST',
    headers: headersWithApiKey(apiKey),
    body: JSON.stringify(body),
  })
  if (!res.ok) {
    const text = await res.text()
    throw new Error(text || res.statusText)
  }
  return res.json()
}

const apiKeys = () => getApiKeys()

export async function getCustomers() {
  return get(`${customersBase()}/Customers`, apiKeys().customers)
}

export async function getCustomer(id) {
  return get(`${customersBase()}/Customers/${id}`, apiKeys().customers)
}

export async function getBillsBalance(customerId) {
  return get(`${billsBase()}/Wallet/balance/${customerId}`, apiKeys().bills)
}

export async function getBillsTransactions(customerId) {
  return get(`${billsBase()}/Wallet/transactions/${customerId}`, apiKeys().bills)
}

export async function getRaidersBalance(customerId) {
  return get(`${raidersBase()}/Wallet/balance/${customerId}`, apiKeys().raiders)
}

export async function getRaidersTransactions(customerId) {
  return get(`${raidersBase()}/Wallet/transactions/${customerId}`, apiKeys().raiders)
}

export async function loadBillsBalance(customerId, amount) {
  return post(`${billsBase()}/Wallet/load/${customerId}`, { amount: Number(amount) }, apiKeys().bills)
}

export async function loadRaidersBalance(customerId, amount) {
  return post(`${raidersBase()}/Wallet/load/${customerId}`, { amount: Number(amount) }, apiKeys().raiders)
}

export async function payBills(customerId, amount) {
  return post(`${billsBase()}/Wallet/pay/${customerId}`, { amount: Number(amount) }, apiKeys().bills)
}

export async function payRaiders(customerId, amount) {
  return post(`${raidersBase()}/Wallet/pay/${customerId}`, { amount: Number(amount) }, apiKeys().raiders)
}
