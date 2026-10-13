export interface RequestFefoAllocationPayload {
  itemId: string;
  requestedQuantity: number;
}

export interface LotAllocationDto {
  lotId: string;
  lotNumber: string;
  expiryDate: string;
  remainingQuantity: number;
  quantity: number;
  storageLocation?: string;
  rowVersion: string | number[];
}

export interface FefoAllocationPreviewDto {
  jobId: string;
  itemId: string;
  baseUnit: string;
  requestedQuantity: number;
  allocatedQuantity: number;
  previewedAt: string;
  isReservation: boolean;
  allocations: LotAllocationDto[];
}

export interface ConsumptionAllocationRequestDto {
  lotId: string;
  quantity: number;
  expectedRowVersion: string | number[];
}

/** F-15 Fase 1: draft dicatat, stok BELUM berkurang. */
export interface CreateDraftConsumptionPayload {
  itemId: string;
  allocations: ConsumptionAllocationRequestDto[];
}

export type ConsumptionStatus = "PendingScan" | "Verified";

export interface DraftConsumptionRecordDto {
  id: string;
  lotId: string;
  quantity: number;
  status: number;
  createdAt: string;
}

export interface DraftConsumptionResponseDto {
  jobId: string;
  itemId: string;
  consumptions: DraftConsumptionRecordDto[];
}

/** F-15 Fase 2: hasil scan barcode. */
export interface VerifyScanPayload {
  itemId: string;
  scannedLotNumber: string;
}

export interface UpdatedLotDto {
  lotId: string;
  remainingQuantity: number;
  rowVersion: string | number[];
}

export interface VerifyScanResponseDto {
  consumptionId: string;
  lotId: string;
  lotNumber: string;
  quantity: number;
  status: number;
  verifiedAt: string | null;
  updatedLot: UpdatedLotDto;
}
