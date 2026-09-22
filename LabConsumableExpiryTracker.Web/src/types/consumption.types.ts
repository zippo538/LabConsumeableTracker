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

export interface ConfirmConsumptionPayload {
  itemId: string;
  requestedQuantity: number;
  allocations: ConsumptionAllocationRequestDto[];
}

export interface ConsumptionRecordDto {
  id: string;
  jobId: string;
  lotId: string;
  quantity: number;
  consumedAt: string;
  consumedBy: string;
}

export interface UpdatedLotDto {
  lotId: string;
  remainingQuantity: number;
  rowVersion: string | number[];
}

export interface ConsumptionResponseDto {
  jobId: string;
  itemId: string;
  baseUnit: string;
  totalConsumedQuantity: number;
  consumptions: ConsumptionRecordDto[];
  updatedLots: UpdatedLotDto[];
}
