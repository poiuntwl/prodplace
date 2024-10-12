export interface Product {
  id: string,
  name: string,
  description: string,
  price: number
}

export interface ProductsTableState {
  data: Product[] | null,
  loading: boolean,
  error: string | null,
  selectedId: string | null
}
