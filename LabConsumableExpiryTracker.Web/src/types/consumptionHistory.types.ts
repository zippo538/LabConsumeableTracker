export interface ConsumptionRecordDto {
  id: string;
  jobId: string;
  jobNumber: string;
  lotId: string;
  lotNumber: string;
  itemId: string;
  itemName: string;
  itemCode: string;
  quantity: number;
  baseUnit: string;
  consumedAt: string;
  consumedBy: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface GetConsumptionHistoryParams {
  page?: number;
  pageSize?: number;
  jobId?: string;
  itemId?: string;
  lotId?: string;
  lotNumber?: string;
  from?: string;
  to?: string;
}

export interface ConsumptionHistoryDetailDto {
  id: string;

  jobId: string;
  jobNumber: string;
  jobDescription: string;

  itemId: string;
  itemCode: string;
  itemName: string;
  baseUnit: string;

  lotId: string;
  lotNumber: string;
  expiryDate: string;
  storageLocation: string;

  quantity: number;

  consumedBy: string;
  consumedAt: string;
}export interface ConsumptionHistoryDetailResponse {
  success: boolean;
  message: string;
  data: ConsumptionHistoryDetailDto;
  errors: string[];
}