<template>
  <div>
    <v-row>
      <v-col cols="12" lg="7">
        <v-card class="hero-card" variant="flat">
          <v-card-text>
            <div class="text-overline">Backoffice</div>
            <div class="text-h4 mb-2">Run the marketplace with clarity</div>
            <div class="text-body-1">
              Monitor critical systems, keep access tidy, and trigger currency updates
              without leaving your control desk.
            </div>
            <div class="d-flex flex-wrap mt-6" style="gap: 12px">
              <v-btn color="secondary" variant="flat" to="/roles" prepend-icon="mdi-account-key">
                Manage roles
              </v-btn>
              <v-btn color="white" variant="outlined" to="/currencies" prepend-icon="mdi-refresh">
                Update rates
              </v-btn>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" lg="5">
        <v-card class="info-card" variant="flat">
          <v-card-text>
            <div class="text-caption muted">Today</div>
            <div class="section-title">Operational highlights</div>
            <div class="mt-4 form-grid">
              <div class="action-row">
                <div>
                  <div class="text-subtitle-2 font-weight-medium">Role changes</div>
                  <div class="text-caption muted">New access requests to review</div>
                </div>
                <v-chip color="primary" variant="tonal">8 pending</v-chip>
              </div>
              <v-divider />
              <div class="action-row">
                <div>
                  <div class="text-subtitle-2 font-weight-medium">Currency sync</div>
                  <div class="text-caption muted">Last update 2h ago</div>
                </div>
                <v-chip color="secondary" variant="tonal">Manual</v-chip>
              </div>
              <v-divider />
              <div class="action-row">
                <div>
                  <div class="text-subtitle-2 font-weight-medium">Partner onboarding</div>
                  <div class="text-caption muted">New seller verifications</div>
                </div>
                <v-chip color="info" variant="tonal">3 reviews</v-chip>
              </div>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row class="mt-6">
      <v-col cols="12" md="6" xl="4">
        <v-card class="info-card" variant="flat">
          <v-card-text>
            <div class="text-caption muted">Access</div>
            <div class="section-title">Role management</div>
            <div class="muted mb-4">
              Create roles, assign permissions, and keep teams aligned across services.
            </div>
            <v-btn color="primary" variant="flat" to="/roles" append-icon="mdi-arrow-right">
              Go to Roles
            </v-btn>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" md="6" xl="4">
        <v-card class="info-card" variant="flat">
          <v-card-text>
            <div class="text-caption muted">Rates</div>
            <div class="section-title">Currency updates</div>
            <div class="muted mb-4">
              Trigger rate refreshes and ensure pricing stays accurate across markets.
            </div>
            <v-btn color="secondary" variant="flat" to="/currencies" append-icon="mdi-arrow-right">
              Go to Currencies
            </v-btn>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" xl="4">
        <v-card class="info-card" variant="flat">
          <v-card-text>
            <div class="text-caption muted">Health</div>
            <div class="section-title">Service pulse</div>
            <div class="muted mb-4">
              All integration nodes are responding within target latency. No incidents
              reported in the last 24 hours.
            </div>
            <div class="stat-pill">
              <v-icon size="14" icon="mdi-pulse" />
              99.98% uptime
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </div>
</template>
