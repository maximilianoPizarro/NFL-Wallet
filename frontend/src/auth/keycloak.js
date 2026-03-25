import Keycloak from 'keycloak-js'
import { reactive } from 'vue'

export const authState = reactive({
  authenticated: false,
  username: '',
  fullName: '',
  email: '',
  token: '',
  roles: [],
  keycloak: null,
  ready: false,
  enabled: false,
})

export async function initKeycloak() {
  const config = window.__API_CONFIG__ || {}
  const url = config.keycloakUrl
  const realm = config.keycloakRealm || 'neuroface'
  const clientId = config.keycloakClientId || 'nfl-wallet-app'

  if (!url) {
    authState.ready = true
    authState.enabled = false
    return
  }

  authState.enabled = true

  const kc = new Keycloak({ url, realm, clientId })
  authState.keycloak = kc

  try {
    const authenticated = await kc.init({
      pkceMethod: 'S256',
      checkLoginIframe: false,
      responseMode: 'query',
    })

    authState.authenticated = authenticated
    if (authenticated) {
      syncProfile(kc)
    }

    kc.onTokenExpired = () => {
      kc.updateToken(30).catch(() => {
        authState.authenticated = false
      })
    }

    kc.onAuthSuccess = () => {
      authState.authenticated = true
      syncProfile(kc)
    }

    kc.onAuthLogout = () => {
      authState.authenticated = false
      authState.username = ''
      authState.fullName = ''
      authState.email = ''
      authState.token = ''
      authState.roles = []
    }
  } catch (err) {
    console.warn('Keycloak init failed:', err)
  }

  authState.ready = true
}

function syncProfile(kc) {
  const parsed = kc.tokenParsed || {}
  authState.username = parsed.preferred_username || ''
  authState.fullName = [parsed.given_name, parsed.family_name].filter(Boolean).join(' ')
  authState.email = parsed.email || ''
  authState.token = kc.token || ''
  authState.roles = parsed.realm_access?.roles || []
}

export function login() {
  authState.keycloak?.login()
}

export function logout() {
  authState.keycloak?.logout({ redirectUri: window.location.origin })
}
