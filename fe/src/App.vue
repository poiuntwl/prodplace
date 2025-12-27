<template>
  <add-product/>
  <update-price/>
  <products-table/>
</template>

<script setup lang="ts">
import ProductsTable from '@/composables/ProductsTable.vue';
import UpdatePrice from '@/composables/UpdatePrice.vue';
import AddProduct from '@/composables/AddProduct.vue';

</script>
