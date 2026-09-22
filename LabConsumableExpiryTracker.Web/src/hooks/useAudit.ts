import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
  fetchAuditEvents,
  fetchConsumptions,
  fetchDisposals,
  fetchConsumptionDetail,
  fetchDisposalDetail,
  fetchAuditStats,
} from "../api/auditApi";
import type {
  AuditFilter,
} from "../types/audit.types";

export const auditQueryKeys = {
  all: ["audit"] as const,
  lists: () => [...auditQueryKeys.all, "list"] as const,
  list: (filters: AuditFilter) => [...auditQueryKeys.lists(), filters] as const,
  details: () => [...auditQueryKeys.all, "detail"] as const,
  detail: (id: string, type: "consumption" | "disposal") =>
    [...auditQueryKeys.details(), id, type] as const,
  stats: () => [...auditQueryKeys.all, "stats"] as const,
  statsWithFilters: (filters: Omit<AuditFilter, "page" | "pageSize">) =>
    [...auditQueryKeys.stats(), filters] as const,
  consumptions: () => [...auditQueryKeys.all, "consumptions"] as const,
  consumptionsList: (filters: Omit<AuditFilter, "eventType">) =>
    [...auditQueryKeys.consumptions(), filters] as const,
  disposals: () => [...auditQueryKeys.all, "disposals"] as const,
  disposalsList: (filters: Omit<AuditFilter, "eventType">) =>
    [...auditQueryKeys.disposals(), filters] as const,
};

/**
 * Hook: Fetch audit events dengan filter
 */
export const useAuditEvents = (params?: AuditFilter) => {
  const filter: AuditFilter = params || {
    eventType: "All",
    itemId: null,
    lotId: null,
    jobId: null,
    from: null,
    to: null,
    page: 1,
    pageSize: 20,
  };

  return useQuery({
    queryKey: auditQueryKeys.list(filter),
    queryFn: () => fetchAuditEvents(filter),
    staleTime: 30 * 1000, // 30 seconds
  });
};

/**
 * Hook: Fetch audit stats
 */
export const useAuditStats = (params?: Omit<AuditFilter, "page" | "pageSize">) => {
  const filter: Omit<AuditFilter, "page" | "pageSize"> = params || {
    eventType: "All",
    itemId: null,
    lotId: null,
    jobId: null,
    from: null,
    to: null,
  };

  return useQuery({
    queryKey: auditQueryKeys.statsWithFilters(filter),
    queryFn: () => fetchAuditStats(filter as AuditFilter),
    staleTime: 60 * 1000, // 1 minute
  });
};

/**
 * Hook: Fetch consumption detail
 */
export const useConsumptionDetail = (id: string | null) => {
  return useQuery({
    queryKey: auditQueryKeys.detail(id || "", "consumption"),
    queryFn: () => {
      if (!id) throw new Error("Consumption ID is required");
      return fetchConsumptionDetail(id);
    },
    enabled: !!id,
  });
};

/**
 * Hook: Fetch disposal detail
 */
export const useDisposalDetail = (id: string | null) => {
  return useQuery({
    queryKey: auditQueryKeys.detail(id || "", "disposal"),
    queryFn: () => {
      if (!id) throw new Error("Disposal ID is required");
      return fetchDisposalDetail(id);
    },
    enabled: !!id,
  });
};

/**
 * Hook: Fetch consumptions list
 */
export const useConsumptions = (params?: {
  page?: number;
  pageSize?: number;
  jobId?: string;
  itemId?: string;
  lotId?: string;
  from?: string;
  to?: string;
}) => {
  const filter: Omit<AuditFilter, "eventType"> = {
    itemId: params?.itemId || null,
    lotId: params?.lotId || null,
    jobId: params?.jobId || null,
    from: params?.from || null,
    to: params?.to || null,
    page: params?.page || 1,
    pageSize: params?.pageSize || 20,
  };

  return useQuery({
    queryKey: auditQueryKeys.consumptionsList(filter),
    queryFn: () => fetchConsumptions(filter),
    staleTime: 30 * 1000,
  });
};

/**
 * Hook: Fetch disposals list
 */
export const useDisposals = (params?: {
  page?: number;
  pageSize?: number;
  itemId?: string;
  lotId?: string;
  from?: string;
  to?: string;
}) => {
  const filter: Omit<AuditFilter, "eventType"> = {
    itemId: params?.itemId || null,
    lotId: params?.lotId || null,
    jobId: null, // Disposal tidak punya jobId
    from: params?.from || null,
    to: params?.to || null,
    page: params?.page || 1,
    pageSize: params?.pageSize || 20,
  };

  return useQuery({
    queryKey: auditQueryKeys.disposalsList(filter),
    queryFn: () => fetchDisposals(filter),
    staleTime: 30 * 1000,
  });
};

/**
 * Hook: Invalidate audit queries
 * Berguna untuk refresh data setelah mutation
 */
export const useInvalidateAuditQueries = () => {
  const queryClient = useQueryClient();

  return {
    invalidateAll: () => {
      queryClient.invalidateQueries({ queryKey: auditQueryKeys.all });
    },
    invalidateLists: () => {
      queryClient.invalidateQueries({ queryKey: auditQueryKeys.lists() });
    },
    invalidateStats: () => {
      queryClient.invalidateQueries({ queryKey: auditQueryKeys.stats() });
    },
    invalidateConsumptions: () => {
      queryClient.invalidateQueries({ queryKey: auditQueryKeys.consumptions() });
    },
    invalidateDisposals: () => {
      queryClient.invalidateQueries({ queryKey: auditQueryKeys.disposals() });
    },
  };
};