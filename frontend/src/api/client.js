const customersBase = import.meta.env.VITE_API_CUSTOMERS_URL || '/api-customers'
const billsBase = import.meta.env.VITE_API_BILLS_URL || '/api-bills'
const raidersBase = import.meta.env.VITE_API_RAIDERS_URL || '/api-raiders'

async function get(url) {
  const res = await fetch(url)
  if (!res.ok) throw new Error(res.statusText)
  return res.json()
}

export async function getCustomers() {
  return get(`${customersBase}/Customers`)
}

export async function getCustomer(id) {
  return get(`${customersBase}/Customers/${id}`)
}

export async function getBillsBalance(customerId) {
  return get(`${billsBase}/Wallet/balance/${customerId}`)
}

export async function getBillsTransactions(customerId) {
  return get(`${billsBase}/Wallet/transactions/${customerId}`)
}

export async function getRaidersBalance(customerId) {
  return get(`${raidersBase}/Wallet/balance/${customerId}`)
}

export async function getRaidersTransactions(customerId) {
  return get(`${raidersBase}/Wallet/transactions/${customerId}`)
}
