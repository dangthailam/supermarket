export interface Product {
  id: number;
  sku: string;
  name: string;
  description?: string;
  barcode?: string;
  price: number;
  costPrice: number;
  stockQuantity: number;
  minStockLevel: number;
  categoryId: number;
  categoryName: string;
  imageUrl?: string;
  isActive: boolean;
  lowStock: boolean;
}

export interface CreateProduct {
  sku: string;
  name: string;
  description?: string;
  barcode?: string;
  price: number;
  costPrice: number;
  stockQuantity: number;
  minStockLevel: number;
  categoryId: number;
  imageUrl?: string;
}

export interface UpdateProduct {
  name?: string;
  description?: string;
  barcode?: string;
  price?: number;
  costPrice?: number;
  minStockLevel?: number;
  categoryId?: number;
  imageUrl?: string;
  isActive?: boolean;
}
