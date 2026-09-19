export interface DashboardSummary {
  lowStockItemCount: number;
  expiringSoonLotCount: number; 
  expiredLotCount: number;
  blockedLotCount: number;
  evaluatedAt: string;
  validLotCount : number;
  countCriteria?: string | Record<string, unknown>;
}

export type DashboardAlertType =
  | "LowStock"
  | "ExpiringSoon"
  | "Expired"
  | "Blocked";
export type AlertResourceType = "Item" | "Lot";

export interface QuickAction {
  code: string;
  label: string;
  href: string;
}

export interface DashboardAlert {
  alertType: DashboardAlertType;
  resourceType: AlertResourceType;
  resourceId: string;
  itemId: string;
  lotId?: string | null;
  itemName?: string | null;
  itemCode?: string | null;
  lotNumber?: string | null;
  remainingQuantity?: number | null;
  baseUnit?: string | number | null;
  expiryDate?: string | null;
  message: string;
  evaluatedAt: string;
  allowedActions?: QuickAction[];
}

export interface DashboardAlertQuery {
  type?: DashboardAlertType;
  itemId?: string;
  page?: number;
  pageSize?: number;
}
