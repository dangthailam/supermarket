export interface Category {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
  productCount: number;
}

export interface CreateCategory {
  name: string;
  description?: string;
}
