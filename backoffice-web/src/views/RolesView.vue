<script setup lang="ts">
import { onMounted, ref } from 'vue'
import api from '@/services/api'

const roleName = ref('')
const roleDescription = ref('')
const userId = ref('')
const assignRoleName = ref('')
const loading = ref(false)
const loadingRoles = ref(false)
const loadingUsers = ref(false)
const snackbar = ref(false)
const snackbarText = ref('')
const roles = ref<{ name: string; description: string }[]>([])
const users = ref<
  { userId: string; email: string; firstName: string; lastName: string; roles: string[] }[]
>([])

async function fetchRoles() {
  loadingRoles.value = true
  try {
    const response = await api.get('/roles')
    roles.value = response.data ?? []
  } catch (error: any) {
    snackbarText.value = 'Error: ' + (error.response?.data || error.message)
    snackbar.value = true
  } finally {
    loadingRoles.value = false
  }
}

async function fetchUsersWithRoles() {
  loadingUsers.value = true
  try {
    const response = await api.get('/roles/users')
    users.value = response.data ?? []
  } catch (error: any) {
    snackbarText.value = 'Error: ' + (error.response?.data || error.message)
    snackbar.value = true
  } finally {
    loadingUsers.value = false
  }
}

async function refreshDirectory() {
  await Promise.all([fetchRoles(), fetchUsersWithRoles()])
}

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

onMounted(() => {
  refreshDirectory()
})
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

    <v-row class="mt-6">
      <v-col cols="12" lg="5">
        <v-card class="info-card" variant="flat">
          <v-card-title class="d-flex align-center justify-space-between">
            <span>Available roles</span>
            <v-btn
              size="small"
              variant="text"
              prepend-icon="mdi-refresh"
              :loading="loadingRoles"
              @click="fetchRoles"
            >
              Refresh
            </v-btn>
          </v-card-title>
          <v-card-text>
            <div v-if="loadingRoles" class="muted">Loading roles...</div>
            <div v-else-if="roles.length === 0" class="muted">No roles found.</div>
            <div v-else class="form-grid">
              <div v-for="role in roles" :key="role.name" class="action-row">
                <div>
                  <div class="text-subtitle-2 font-weight-medium">{{ role.name }}</div>
                  <div class="text-caption muted">{{ role.description || 'No description' }}</div>
                </div>
                <v-chip color="primary" variant="tonal">Role</v-chip>
              </div>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" lg="7">
        <v-card class="info-card" variant="flat">
          <v-card-title class="d-flex align-center justify-space-between">
            <span>Users & roles</span>
            <v-btn
              size="small"
              variant="text"
              prepend-icon="mdi-refresh"
              :loading="loadingUsers"
              @click="fetchUsersWithRoles"
            >
              Refresh
            </v-btn>
          </v-card-title>
          <v-card-text>
            <div v-if="loadingUsers" class="muted">Loading users...</div>
            <div v-else-if="users.length === 0" class="muted">No users found.</div>
            <v-table v-else density="compact">
              <thead>
                <tr>
                  <th>User</th>
                  <th>Email</th>
                  <th>Roles</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="user in users" :key="user.userId">
                  <td>
                    <div class="text-subtitle-2 font-weight-medium">
                      {{ user.firstName || user.lastName ? `${user.firstName} ${user.lastName}` : 'Unnamed' }}
                    </div>
                    <div class="text-caption muted">{{ user.userId }}</div>
                  </td>
                  <td>{{ user.email || 'N/A' }}</td>
                  <td>
                    <div class="d-flex flex-wrap" style="gap: 8px">
                      <v-chip
                        v-for="role in user.roles"
                        :key="`${user.userId}-${role}`"
                        color="secondary"
                        variant="tonal"
                        size="small"
                      >
                        {{ role }}
                      </v-chip>
                      <span v-if="user.roles.length === 0" class="text-caption muted">
                        No roles
                      </span>
                    </div>
                  </td>
                </tr>
              </tbody>
            </v-table>
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
