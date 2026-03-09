<template>
  <div id="app">
    <nav class="navbar">
      <div class="navbar-brand">
        <strong>Apollo</strong>
      </div>
      <div class="navbar-menu">
        <router-link to="/settings" class="nav-link">Settings</router-link>
      </div>
    </nav>
    <main class="content">
      <router-view />
    </main>
  </div>
</template>

<style>
#app {
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
  color: #2c3e50;
  min-height: 100vh;
  background-color: #f9f9f9;
}

.navbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 2rem;
  background-color: #2c3e50;
  color: white;
}

.navbar-brand {
  font-size: 1.5rem;
}

.nav-link {
  color: rgba(255, 255, 255, 0.8);
  text-decoration: none;
  font-weight: 500;
  transition: color 0.2s;
}

.nav-link:hover, .router-link-active {
  color: white;
}

.content {
  padding: 1rem;
}

body {
  margin: 0;
  padding: 0;
}
</style>
