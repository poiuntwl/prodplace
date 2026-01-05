<script setup lang="ts">
import { RouterView } from 'vue-router'
</script>

<template>
  <v-app>
    <v-navigation-drawer width="280" permanent class="nav-surface">
      <div class="nav-brand">
        <v-list-item>
          <template #prepend>
            <v-avatar class="nav-avatar" size="40">PP</v-avatar>
          </template>
          <v-list-item-title class="text-h6">ProdPlace</v-list-item-title>
          <v-list-item-subtitle class="muted">Backoffice</v-list-item-subtitle>
        </v-list-item>
      </div>
      <v-divider />
      <v-list density="comfortable" nav>
        <v-list-subheader class="nav-section-title">Core</v-list-subheader>
        <v-list-item
          prepend-icon="mdi-view-dashboard"
          title="Dashboard"
          value="dashboard"
          to="/"
        />
        <v-list-item
          prepend-icon="mdi-account-group"
          title="Roles"
          value="roles"
          to="/roles"
        />
        <v-list-item
          prepend-icon="mdi-currency-usd"
          title="Currencies"
          value="currencies"
          to="/currencies"
        />
      </v-list>
      <div class="px-4 pt-4">
        <v-card class="info-card" variant="flat">
          <v-card-text>
            <div class="text-caption muted">System status</div>
            <div class="text-subtitle-1 font-weight-medium">All services green</div>
            <div class="stat-pill mt-3">
              <v-icon size="14" icon="mdi-circle-slice-8" />
              24h uptime
            </div>
          </v-card-text>
        </v-card>
      </div>
    </v-navigation-drawer>

    <v-app-bar height="72" flat color="transparent" class="app-main">
      <v-toolbar-title>
        <div class="section-title">ProdPlace Control Desk</div>
        <div class="muted text-caption">Operations, access, and currency rates</div>
      </v-toolbar-title>
      <v-spacer />
      <v-chip color="secondary" variant="tonal" class="text-caption">Live</v-chip>
      <v-btn
        color="primary"
        variant="flat"
        prepend-icon="mdi-lightning-bolt"
        style="margin-left: 12px"
      >
        Quick Actions
      </v-btn>
    </v-app-bar>

    <v-main class="app-main">
      <div class="app-background"></div>
      <v-container fluid class="app-shell">
        <RouterView />
      </v-container>
    </v-main>
  </v-app>
</template>

<style>
/* Remove default vue styles that might interfere with vuetify */
body {
  margin: 0;
}
</style>
