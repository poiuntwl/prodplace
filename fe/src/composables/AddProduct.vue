<script setup lang="ts">
import { ref, computed } from 'vue';
import { useProductsStore } from '@/store';

const store = useProductsStore();
const loading = computed(() => store.getters.getLoading);

const formData = ref({
  name: '',
  description: '',
  price: 0,
});

const submitForm = async () => {
  if (!formData.value.name || !formData.value.price) return;

  await store.dispatch('createProduct', {
    name: formData.value.name,
    description: formData.value.description,
    price: formData.value.price,
  });

  // Reset form
  formData.value.name = '';
  formData.value.description = '';
  formData.value.price = 0;
};
</script>

<template>
  <div class="add-product-container">
    <h3>Add New Product</h3>
    <form @submit.prevent="submitForm" class="add-product-form">
      <div class="form-group">
        <input
          v-model="formData.name"
          placeholder="Product Name"
          required
          aria-label="Product Name"
        />
      </div>
      <div class="form-group">
        <input v-model="formData.description" placeholder="Description" aria-label="Description" />
      </div>
      <div class="form-group">
        <input
          v-model.number="formData.price"
          type="number"
          step="0.01"
          placeholder="Price"
          required
          aria-label="Price"
        />
      </div>
      <button type="submit" :disabled="loading">
        {{ loading ? 'Adding...' : 'Add Product' }}
      </button>
    </form>
  </div>
</template>

<style scoped>
.add-product-container {
  margin: 20px 0;
  padding: 15px;
  background-color: #f9f9f9;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.add-product-form {
  display: flex;
  gap: 10px;
  align-items: center;
  flex-wrap: wrap;
}

input {
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
}

button {
  padding: 8px 16px;
  background-color: #4CAF50;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

button:disabled {
  background-color: #cccccc;
  cursor: not-allowed;
}

h3 {
  margin-top: 0;
  margin-bottom: 15px;
}
</style>
