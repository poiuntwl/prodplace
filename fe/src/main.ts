import { createApp } from 'vue';
import { key, store } from '@/store';
import App from './App.vue';
import './assets/main.css';

createApp(App)
  .use(store, key)
  .mount('#app');
