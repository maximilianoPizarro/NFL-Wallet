import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  { path: '/', name: 'customers', component: () => import('../views/CustomerList.vue') },
  { path: '/customer/:id', name: 'wallets', component: () => import('../views/CustomerWallets.vue') },
  { path: '/pay', name: 'pay', component: () => import('../views/PayFromQR.vue') },
]

export default createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})
