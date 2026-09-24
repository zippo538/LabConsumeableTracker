export type UnitOfMeasure = 'Milliliter' | 'Gram' | 'Unit' | 'Vial' | 'Box' | string;

export type StockLevelStatus = 'Sufficient' | 'AtMinimum' | 'LowStock' | 'OutOfStock' | number;

export interface LotSummaryDto {
  lotId: string;
  lotNumber: string;
  remainingQuantity: number;
  expiryDate: string;
  status: string;
}

export interface ItemDto {
  id: string;
  code: string;
  name: string;
  imageId?: string | null;
  baseUnit: UnitOfMeasure;
  minimumStock: number;
  stockStatus: StockLevelStatus;
  totalRemainingQuantity: number;
  isLowStock: boolean;
  lots?: LotSummaryDto[];
  expiringSoonDays?: number;
}

export interface CreateItemPayload {
  code: string;
  name: string;
  baseUnit: string;
  minimumStock: number;
  expiringSoonDays: number;
  image?: File | null;
}

export interface UpdateItemPayload {
  id: string;
  name: string;
  baseUnit: string;
  minimumStock: number;
  expiringSoonDays: number;
  image?: File | null;
}
export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
};