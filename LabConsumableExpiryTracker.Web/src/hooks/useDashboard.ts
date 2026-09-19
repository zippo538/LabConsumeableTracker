import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { getDashboardAlerts, getDashboardSummary } from "../api/dashboardApi";
import type { DashboardAlertQuery } from "../types/dashboard.types";

export const dashboardSummaryKey = (itemId?: string) =>
  ["dashboard-summary", itemId ?? "all"] as const;

export const useDashboardSummary = (itemId?: string) =>
  useQuery({
    queryKey: dashboardSummaryKey(itemId),
    queryFn: () => getDashboardSummary(itemId),
    staleTime: 30_000,
  });

export const dashboardAlertsKey = (query?: DashboardAlertQuery) =>
  [
    "dashboard-alerts",
    query?.type ?? "all",
    query?.itemId ?? "all",
    query?.page ?? 1,
    query?.pageSize ?? 10,
  ] as const;

export const useDashboardAlerts = (query?: DashboardAlertQuery) =>
  useQuery({
    queryKey: dashboardAlertsKey(query),
    queryFn: () => getDashboardAlerts(query),
    placeholderData: keepPreviousData,
    staleTime: 30_000,
  });
