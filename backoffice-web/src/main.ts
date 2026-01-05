import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

// Vuetify
import 'vuetify/styles'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import '@mdi/font/css/materialdesignicons.css'

import App from './App.vue'
import router from './router'

const vuetify = createVuetify({
  components,
  directives,
  theme: {
    defaultTheme: 'ppLight',
    themes: {
      ppLight: {
        dark: false,
        colors: {
          background: '#f5efe6',
          surface: '#fbf8f2',
          primary: '#0f6f66',
          secondary: '#f07a4b',
          info: '#136b86',
          warning: '#d97706',
          success: '#1f8a70',
          error: '#c2410c'
        }
      }
    }
  }
})

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(vuetify)

app.mount('#app')
