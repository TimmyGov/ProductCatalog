export interface Product {
  id: number;
  name: string;
  sku: string;
  description?: string;
  price: number;
  quantity: number;
  categoryId?: number;
  categoryName?: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateProduct {
  name: string;
  sku: string;
  description?: string;
  price: number;
  quantity: number;
  categoryId?: number;
}

export interface UpdateProduct {
  name?: string;
  sku?: string;
  description?: string;
  price?: number;
  quantity?: number;
  categoryId?: number;
}
