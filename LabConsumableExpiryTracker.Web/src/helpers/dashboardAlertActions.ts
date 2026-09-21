import type {
  DashboardAlert,
  DashboardAlertType,
} from "../types/dashboard.types";

const actionCodes = new Set([
  "RegisterLot",
  "OpenDisposal",
  "ManageLotStatus",
  "Block",
  "Release",
  "Quarantine",
]);

export const getDetailHref = (alert: DashboardAlert): string => {
  const detailAction = alert.allowedActions?.find(
    (action) => action.code === "ViewItem" || action.code === "ViewLot",
  );

  if (detailAction?.href) return detailAction.href;

  return alert.resourceType === "Item"
    ? `/app/items/${alert.resourceId}`
    : `/app/lots/${alert.resourceId}`;
};

export const getActionHref = (alert: DashboardAlert): string | null =>
  alert.allowedActions?.find((action) => actionCodes.has(action.code))?.href ??
  null;

export const getActionLabel = (
  alertType: DashboardAlertType | string,
): string => {
  const type = String(alertType).toLowerCase();

  if (type.includes("lowstock") || type.includes("low stock")) {
    return "Daftarkan lot";
  }
  if (type.includes("expiring") || type.includes("expired")) {
    return "Tinjau disposal";
  }
  if (type.includes("blocked")) return "Kelola status";

  return "Lihat aksi";
};
