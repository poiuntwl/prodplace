<script setup lang="ts">
import { ref } from 'vue'
import api from '@/services/api'

const roleName = ref('')
const roleDescription = ref('')
const userId = ref('')
const assignRoleName = ref('')
const loading = ref(false)
const snackbar = ref(false)
const snackbarText = ref('')

async function createRole() {
  loading.value = true
  try {
    await api.post('/roles', {
      name: roleName.value,
      description: roleDescription.value
    })
    snackbarText.value = 'Role created successfully'
    snackbar.value = true
    roleName.value = ''
    roleDescription.value = ''
  } catch (error: any) {
    snackbarText.value = 'Error: ' + (error.response?.data || error.message)
    snackbar.value = true
  } finally {
    loading.value = false
  }
}

async function assignRole() {
  loading.value = true
  try {
    await api.post('/roles/assign', {
      userId: userId.value,
      roleName: assignRoleName.value
    })
    snackbarText.value = 'Role assigned successfully'
    snackbar.value = true
    userId.value = ''
    assignRoleName.value = ''
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
    <div class="section-title">Role Management</div>
    <div class="muted mb-6">Create roles and assign access to keep teams aligned.</div>

    <v-row>
      <v-col cols="12" lg="6">
        <v-card class="form-card" variant="flat">
          <v-card-title>Create a new role</v-card-title>
          <v-card-text>
            <div class="form-grid">
              <v-text-field
                v-model="roleName"
                label="Role name"
                variant="outlined"
                density="comfortable"
                prepend-inner-icon="mdi-shield-account"
              />
              <v-textarea
                v-model="roleDescription"
                label="Role description"
                variant="outlined"
                density="comfortable"
                rows="3"
                prepend-inner-icon="mdi-text"
              />
            </div>
          </v-card-text>
          <v-card-actions class="px-6 pb-6">
            <v-btn color="primary" variant="flat" @click="createRole" :loading="loading">
              Create role
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>

      <v-col cols="12" lg="6">
        <v-card class="form-card" variant="flat">
          <v-card-title>Assign a role</v-card-title>
          <v-card-text>
            <div class="form-grid">
              <v-text-field
                v-model="userId"
                label="User ID"
                variant="outlined"
                density="comfortable"
                prepend-inner-icon="mdi-account"
              />
              <v-text-field
                v-model="assignRoleName"
                label="Role name"
                variant="outlined"
                density="comfortable"
                prepend-inner-icon="mdi-badge-account-horizontal"
              />
            </div>
          </v-card-text>
          <v-card-actions class="px-6 pb-6">
            <v-btn color="secondary" variant="flat" @click="assignRole" :loading="loading">
              Assign role
            </v-btn>
          </v-card-actions>
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
