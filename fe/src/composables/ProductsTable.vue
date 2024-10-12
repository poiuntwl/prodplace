<script setup lang="ts">
import { computed } from 'vue';
import { useProductsStore } from '@/store';

const store = useProductsStore();

const data = computed(() => store.getters.getData);
const loading = computed(() => store.getters.getLoading);
const error = computed(() => store.getters.getError);

const selectId = async (id: string) => {
  await store.commit('setSelectedId', id);
};

</script>

<template>
  <div class="component-root">
    <div v-if="loading">Loading...</div>
    <div v-else-if="error">Error: {{ error }}</div>
    <table v-else-if="data">
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
        <td @click="selectId(row.id)">{{ row.id }}</td>
        <td>{{ row.name }}</td>
        <td>{{ row.description }}</td>
        <td>{{ row.price }}</td>
      </tr>
      </tbody>
    </table>
  </div>
</template>

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
