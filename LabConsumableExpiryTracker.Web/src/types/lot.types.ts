export type LotAdministrativeStatus = 
  | 'Active' 
  | 'Quarantined' 
  | 'ManuallyBlocked' 
  | 'Disposed';

export type ExpiryCondition = 
  | 'Valid' 
  | 'ExpiringSoon' 
  | 'Expired';

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
}

export interface UpdateLotRequest {
  lotNumber: string;
  expiryDate: string;
  storageLocation: string;
  expectedRowVersion: string;
}

export interface ItemOption {
  id: string;
  code: string;
  name: string;
  baseUnit: string;
}