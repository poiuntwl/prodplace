<script setup lang="ts">

import { computed, ref } from 'vue';
import { useProductsStore } from '@/store';

const store = useProductsStore();
const loading = computed(() => store.getters.getLoading);
const buttonText = computed(() => (loading.value ? 'Fetching...' : 'Fetch Data'));
const selectedId = computed(() => store.getters.getSelectedId);

const formData = ref({
  productId: selectedId,
  price: 0,
});

const fetchData = async () => {
  await store.dispatch('fetchData');
};

const submitForm = async () => {
  try {
    const options = {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        id: formData.value.productId,
        price: formData.value.price,
      }),
    };

    // Use Proxy URL
    const response = await fetch('http://localhost:44303/api/products/price', options);
    if (!response.ok) {
      // Log the status text if available
      throw new Error(`Http Error. Status: ${response.status} ${response.statusText}`);
    }

    await fetchData();

    formData.value.price = 0;
    store.commit('setSelectedId', null);
  } catch (error) {
    // console.log('Error submitting form: ', error);
  }
};

</script>

<template>
  <div>
    <form @submit.prevent="submitForm">
      <input v-model="formData.productId" placeholder="product id" aria-label="Product ID"/>
      <input v-model="formData.price" placeholder="price" aria-label="New Price"/>
      <button type="submit" :disabled="loading">Update Price</button>
    </form>
    <button @click="fetchData" :disabled="loading">{{ buttonText }}</button>
  </div>
</template>

<style scoped>

</style>
