<script setup lang="ts">
import { ref } from 'vue'
import api from '@/services/api'

const loading = ref(false)
const snackbar = ref(false)
const snackbarText = ref('')

async function forceUpdate() {
  loading.value = true
  try {
    const response = await api.post('/api/currencies/force-update')
    snackbarText.value = response.data.message
    snackbar.value = true
  } catch (error: any) {
    snackbarText.value = 'Error: ' + (error.response?.data || error.message)
    snackbar.value = true
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div>
    <div class="section-title">Currency Operations</div>
    <div class="muted mb-6">
      Trigger rates updates and keep pricing aligned across the marketplace.
    </div>

    <v-row>
      <v-col cols="12" lg="7">
        <v-card class="form-card" variant="flat">
          <v-card-title>Rates maintenance</v-card-title>
          <v-card-text>
            <div class="muted mb-4">
              Sync the latest exchange rates from the configured provider. This runs an
              immediate refresh and updates all cached currency pairs.
            </div>
            <div class="action-row">
              <div>
                <div class="text-subtitle-2 font-weight-medium">Last sync</div>
                <div class="text-caption muted">2 hours ago by automation</div>
              </div>
              <v-btn
                color="secondary"
                variant="flat"
                prepend-icon="mdi-refresh"
                @click="forceUpdate"
                :loading="loading"
              >
                Force update
              </v-btn>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" lg="5">
        <v-card class="info-card" variant="flat">
          <v-card-text>
            <div class="text-caption muted">Monitoring</div>
            <div class="section-title">Rate coverage</div>
            <div class="mt-3 form-grid">
              <div class="action-row">
                <div class="text-subtitle-2 font-weight-medium">Tracked currencies</div>
                <v-chip color="primary" variant="tonal">42</v-chip>
              </div>
              <v-divider />
              <div class="action-row">
                <div class="text-subtitle-2 font-weight-medium">Provider status</div>
                <v-chip color="success" variant="tonal">Healthy</v-chip>
              </div>
              <v-divider />
              <div class="action-row">
                <div class="text-subtitle-2 font-weight-medium">Next auto refresh</div>
                <div class="text-caption muted">In 6 hours</div>
              </div>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-snackbar v-model="snackbar">
      {{ snackbarText }}
      <template v-slot:actions>
        <v-btn class="snackbar-action" variant="text" @click="snackbar = false">Close</v-btn>
      </template>
    </v-snackbar>
  </div>
</template>
