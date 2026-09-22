import type { LotAdministrativeStatus } from "./lot.types";

export interface Disposal {
  id: string;
  lotId: string;
  quantity: number;
  reason: string;
  disposedAt: string;
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
  expectedRowVersion: string; // Base64 string dari lot.rowVersion
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
  lot: {
    id: string;
    remainingQuantity: number;
    administrativeStatus: LotAdministrativeStatus;
    rowVersion: string; // RowVersion terbaru dari server
  };
}
