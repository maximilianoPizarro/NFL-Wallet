function getBaseUrls() {
  const c = window.__API_CONFIG__ || {}
  return {
    customers: c.apiCustomersUrl || import.meta.env.VITE_API_CUSTOMERS_URL || '/api-customers',
    bills: c.apiBillsUrl || import.meta.env.VITE_API_BILLS_URL || '/api-bills',
    raiders: c.apiRaidersUrl || import.meta.env.VITE_API_RAIDERS_URL || '/api-raiders',
  }
}
const customersBase = () => getBaseUrls().customers
const billsBase = () => getBaseUrls().bills
const raidersBase = () => getBaseUrls().raiders

async function get(url) {
  const res = await fetch(url)
  if (!res.ok) throw new Error(res.statusText)
  return res.json()
}

async function post(url, body) {
  const res = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!res.ok) {
    const text = await res.text()
    throw new Error(text || res.statusText)
  }
  return res.json()
}

export async function getCustomers() {
  return get(`${customersBase()}/Customers`)
}

export async function getCustomer(id) {
  return get(`${customersBase()}/Customers/${id}`)
}

export async function getBillsBalance(customerId) {
  return get(`${billsBase()}/Wallet/balance/${customerId}`)
}

export async function getBillsTransactions(customerId) {
  return get(`${billsBase()}/Wallet/transactions/${customerId}`)
}

export async function getRaidersBalance(customerId) {
  return get(`${raidersBase()}/Wallet/balance/${customerId}`)
}

export async function getRaidersTransactions(customerId) {
  return get(`${raidersBase()}/Wallet/transactions/${customerId}`)
}

export async function loadBillsBalance(customerId, amount) {
  return post(`${billsBase()}/Wallet/load/${customerId}`, { amount: Number(amount) })
}

export async function loadRaidersBalance(customerId, amount) {
  return post(`${raidersBase()}/Wallet/load/${customerId}`, { amount: Number(amount) })
}

export async function payBills(customerId, amount) {
  return post(`${billsBase()}/Wallet/pay/${customerId}`, { amount: Number(amount) })
}

export async function payRaiders(customerId, amount) {
  return post(`${raidersBase()}/Wallet/pay/${customerId}`, { amount: Number(amount) })
}
