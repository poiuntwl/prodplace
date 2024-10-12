<template>
  <fetch-example/>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import FetchExample from '@/FetchExample.vue';

export default defineComponent({
  name: 'App',
  components: {
    FetchExample,
  },
});
</script>
