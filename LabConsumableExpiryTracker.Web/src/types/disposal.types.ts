import type { LotAdministrativeStatus } from "./lot.types";

export interface Disposal {
  id: string;
  lotId: string;
  itemId: string;
  quantity: number;
  baseUnit: string;
  reason: string;
  disposedAt: string; 
  disposedBy: string; 

  itemName?: string;
  itemCode?: string;
  lotNumber?: string;
  disposedByName?: string;
}

export interface LotSnapshot {
  id: string;
  remainingQuantity: number;
  administrativeStatus: LotAdministrativeStatus;
  rowVersion: string;
  evaluatedAt?: string;
}

export interface CreateDisposalRequest {
  quantity: number;
  reason: string;
  lotId: string; 
}

export interface DisposalListParams {
  page?: number;
  pageSize?: number;
  itemId?: string;
  lotId?: string;
  from?: string; 
  to?: string; 
}

export interface CreateDisposalResponse {
  disposal: Disposal;
  lot: LotSnapshot;
}
