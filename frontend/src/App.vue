<template>
  <div class="app">
    <header class="nfl-header">
      <div class="container header-inner">
        <router-link to="/" class="navbar-brand">
          <img src="https://upload.wikimedia.org/wikipedia/en/a/a2/National_Football_League_logo.svg" alt="NFL" class="header-logo" />
          NFL Stadium Wallet
        </router-link>
      </div>
    </header>
    <main>
      <router-view />
    </main>
    <footer class="nfl-footer">
      <div class="container">NFL Wallet — Buffalo Bills & Las Vegas Raiders</div>
    </footer>
  </div>
</template>

<script setup>
</script>

<style scoped>
.app { min-height: 100vh; display: flex; flex-direction: column; }
main { flex: 1; padding: 1.5rem 0 2rem; }
.header-inner { display: flex; align-items: center; }
.header-logo {
  height: 32px;
  width: auto;
  margin-right: 0.6rem;
  vertical-align: middle;
}
.navbar-brand { display: inline-flex; align-items: center; }
</style>
