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
        const response = await fetch('http://localhost:44303/api/products/');
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
        const data = await response.json() as Product[];
        commit('setData', data);
      } catch (error) {
        commit('setError', (error as Error).message);
      } finally {
        commit('setLoading', false);
      }
    },
    async createProduct(
      { commit, dispatch },
      product: { name: string, description: string, price: number },
    ) {
      commit('setLoading', true);
      commit('setError', null);
      try {
        const options = {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(product),
        };
        const response = await fetch('http://localhost:44303/api/products/', options);
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);

        await dispatch('fetchData');
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
