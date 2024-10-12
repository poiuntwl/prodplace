import { InjectionKey } from 'vue';
import { createStore, Store, useStore as baseUseStore } from 'vuex';
import { Product, ProductsTableState } from '@/types/types';

export const key: InjectionKey<Store<ProductsTableState>> = Symbol('ProductTableState');

export const store = createStore<ProductsTableState>({
  state: {
    data: null,
    loading: false,
    error: null,
    selectedId: null,
  },
  mutations: {
    setData(state, data: Product[]) {
      state.data = data;
    },
    setLoading(state, loading: boolean) {
      state.loading = loading;
    },
    setError(state, error: string | null) {
      state.error = error;
    },
    setSelectedId(state, id: string | null) {
      state.selectedId = id;
    },
  },
  actions: {
    async fetchData({ commit }) {
      commit('setLoading', true);
      commit('setError', null);
      try {
        const options = {
          method: 'GET',
          headers: {
            Authorization: 'Bearer eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InNvbWVlbWFpbEBnbWFpbC5jb20iLCJnaXZlbl9uYW1lIjoic29tZV91c2VybmFtZSIsInN1YiI6IjE4NWZkOWFjLTA2YmUtNGIzMS05OWRlLTZiZDA0ZWY5NjQ4YSIsIm5iZiI6MTcyODQyMDEyOSwiZXhwIjoxNzI5MDI0OTI5LCJpYXQiOjE3Mjg0MjAxMjksImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NDQzMDQiLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjQ0MzA0In0.I0qmJTPbk4rlXnogTiK9TIuXfeVsgEJ7TqU0OCxHxeofeO2uYV_p_XWWNKNDwUoKSKjw5CUx1tcr3dfzDz2jjg',
          },
        };
        const response = await fetch('https://localhost:44300/api/products/', options);
        const data = await response.json() as Product[];
        commit('setData', data);
      } catch (error) {
        commit('setError', (error as Error).message);
      } finally {
        commit('setLoading', false);
      }
    },
  },
  getters: {
    getData: (state) => state.data,
    getLoading: (state) => state.loading,
    getError: (state) => state.error,
    getSelectedId: (state) => state.selectedId,
  },
});

export function useProductsStore() {
  return baseUseStore(key);
}
