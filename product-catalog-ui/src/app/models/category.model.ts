export interface Category {
  id: number;
  name: string;
  description?: string;
  parentId?: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface CategoryTree {
  id: number;
  name: string;
  description?: string;
  parentId?: number;
  children: CategoryTree[];
}
