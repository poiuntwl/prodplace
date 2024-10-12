<template>
  <div class="component-root">
    <h1>{{ title }}</h1>
    <div v-if="loading">Loading...</div>
    <div v-else-if="error">Error: {{ error }}</div>
    <table v-if="data.length > 0">
      <thead>
      <tr>
        <th>Id</th>
        <th>Title</th>
        <th>Description</th>
        <th>Price</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="(row, rowIndex) in data" :key="rowIndex">
        <td>{{ row.id }}</td>
        <td>{{ row.name }}</td>
        <td>{{ row.description }}</td>
        <td>{{ row.price }}</td>
      </tr>
      </tbody>
    </table>
  </div>
</template>

<script lang="ts">
import { onMounted, ref } from 'vue';
import { Product } from '@/types/types';

export default {
  name: 'FetchExample',
  setup() {
    const title = ref<string>('Fetch Example');
    const data = ref<Product[]>([]);
    const loading = ref<boolean>(false);
    const error = ref<string>(null);

    const fetchData = async () => {
      loading.value = true;
      error.value = null;
      try {
        const options = {
          method: 'GET',
          headers: {
            'User-Agent': 'insomnia/10.0.0',
            Authorization: 'Bearer eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InNvbWVlbWFpbEBnbWFpbC5jb20iLCJnaXZlbl9uYW1lIjoic29tZV91c2VybmFtZSIsInN1YiI6IjE4NWZkOWFjLTA2YmUtNGIzMS05OWRlLTZiZDA0ZWY5NjQ4YSIsIm5iZiI6MTcyODQyMDEyOSwiZXhwIjoxNzI5MDI0OTI5LCJpYXQiOjE3Mjg0MjAxMjksImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NDQzMDQiLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjQ0MzA0In0.I0qmJTPbk4rlXnogTiK9TIuXfeVsgEJ7TqU0OCxHxeofeO2uYV_p_XWWNKNDwUoKSKjw5CUx1tcr3dfzDz2jjg',
          },
        };
        const response = await fetch('https://localhost:44300/api/products/', options);
        if (!response.ok) {
          throw new Error('Network response was not ok');
        }
        data.value = await response.json() as Product[];
      } catch (err) {
        error.value = err.message;
      } finally {
        loading.value = false;
      }
    };

    onMounted(() => {
      fetchData();
    });

    return {
      title,
      data,
      loading,
      error,
      fetchData,
    };
  },
};
</script>

<style scoped>
@font-face {
  font-family: 'Helvetica';
  src: url('https://cdnjs.cloudflare.com/ajax/libs/helvetica-neue/88fbd05829e93d46c05da922e33ddf79/helvetica-neue.woff2') format('woff2'),
  url('https://cdnjs.cloudflare.com/ajax/libs/helvetica-neue/88fbd05829e93d46c05da922e33ddf79/helvetica-neue.woff') format('woff');
  font-weight: normal;
  font-style: normal;
}

table {
  border-collapse: collapse;
  width: 100%;
}

th, td {
  border: 1px solid #ddd;
  padding: 8px;
  text-align: left;
}

th {
  background-color: #f2f2f2;
  font-weight: bold;
}

.component-root {
  font-family: 'Helvetica', Arial, sans-serif;
}
</style>
