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
        Authorization: 'Bearer eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InNvbWVlbWFpbEBnbWFpbC5jb20iLCJnaXZlbl9uYW1lIjoic29tZV91c2VybmFtZSIsInN1YiI6IjE4NWZkOWFjLTA2YmUtNGIzMS05OWRlLTZiZDA0ZWY5NjQ4YSIsIm5iZiI6MTcyODQyMDEyOSwiZXhwIjoxNzI5MDI0OTI5LCJpYXQiOjE3Mjg0MjAxMjksImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NDQzMDQiLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjQ0MzA0In0.I0qmJTPbk4rlXnogTiK9TIuXfeVsgEJ7TqU0OCxHxeofeO2uYV_p_XWWNKNDwUoKSKjw5CUx1tcr3dfzDz2jjg',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        id: formData.value.productId,
        price: formData.value.price,
      }),
    };

    console.log(JSON.stringify({
      id: formData.value.productId,
      price: +formData.value.price,
    }));

    const response = await fetch('https://localhost:44300/api/products/price', options);
    if (!response.ok) {
      throw new Error(`Http Error. Status: ${response.data}`);
    }

    await fetchData();

    formData.value.price = 0;
    store.commit('setSelectedId', null);
  } catch (error) {
    console.log('Error submitting form: ', error);
  }
};

</script>

<template>
  <div>
    <form @submit.prevent="submitForm">
      <input v-model="formData.productId" placeholder="product id"/>
      <input v-model="formData.price" placeholder="price"/>
      <button type="submit" :disabled="loading">Update Price</button>
    </form>
    <button @click="fetchData" :disabled="loading">{{ buttonText }}</button>
  </div>
</template>

<style scoped>

</style>
