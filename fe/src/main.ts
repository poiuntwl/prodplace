import { createApp } from 'vue';
import { key, store } from '@/store';
import App from './App.vue';

createApp(App)
  .use(store, key)
  .mount('#app');
