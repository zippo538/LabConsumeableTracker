export type LotAdministrativeStatus =
  | "Active"
  | "Quarantined"
  | "ManuallyBlocked"
  | "Disposed";

export type ExpiryCondition = "Valid" | "ExpiringSoon" | "Expired";

export interface Lot {
  id: string;
  itemId: string;
  itemName: string;
  itemCode: string;
  lotNumber: string;
  initialQuantity: number;
  remainingQuantity: number;
  baseUnit: string;
  expiryDate: string;
  receivedAt: string;
  administrativeStatus: LotAdministrativeStatus;
  expiryCondition: ExpiryCondition;
  isEligible: boolean;
  storageLocation: string;
  rowVersion: string;
  evaluatedAt: string;
  suppliername?: string;
  supplierLotNumber?: string;
}

export interface LotSummary {
  activeLotCount: number;
  expiringSoonLotCount: number;
  notEligibleLotCount: number;
  evaluatedAt: string;
}

export interface CreateLotRequest {
  itemId: string;
  lotNumber: string;
  initialQuantity: number;
  expiryDate: string;
  storageLocation: string;
  receivedAt: string; 
  suppliername: string; 
  supplierLotNumber: string; 
}

export interface UpdateLotRequest {
  storageLocation: string;
  administrativeStatus: LotAdministrativeStatus;
  rowVersion: string;
  suppliername: string;
  supplierLotNumber:  string;
  lotNumber: string;
  expiryDate: string;
}

export type UpdatableLotAdministrativeStatus = Exclude<
  LotAdministrativeStatus,
  "Disposed"
>;

export interface UpdateLotAdministrativeStatusRequest {
  administrativeStatus: UpdatableLotAdministrativeStatus;
  rowVersion: string;
}

export interface DisposeLotRequest {
  LotId: number;
  quantity: number;
  reason: string;
}

export interface LotDetailRouteState {
  lot?: Lot;
}

export interface ItemOption {
  id: string;
  code: string;
  name: string;
  baseUnit: string;
}
