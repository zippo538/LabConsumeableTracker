import { useCallback, useEffect, useMemo, useState, useRef } from "react";
import { useSearchParams } from "react-router";
import { Download, ChevronDown } from "lucide-react";
import { useAtomValue } from "jotai";
import { userAtom } from "../lib/auth/authAtom";

import { DataTable, type DataTableColumn } from "../components/DataTable";
import { InlineAlert } from "../components/FeedBackStates";
import { LoadingState, EmptyState, ErrorState } from "../components/FeedBackStates";

import { AuditFilterBar } from "../components/audit/AuditFilterBar";
import { AuditStatsCards } from "../components/audit/AuditSummaryCard";
import { AuditDetailModal } from "../components/audit/AuditDetailModal";

import { fetchAuditEvents, fetchAuditStats, parseAuditError } from "../api/auditApi";
import type { AuditEvent, AuditFilter, AuditPageState, AuditStats, LookupOption } from "../types/audit.types";
import { DEFAULT_AUDIT_FILTER } from "../types/audit.types";

import { useExport, useExportFilterMapper } from "../hooks/useExport";
import type { ExportType } from "../types/export.types";
import { EXPORT_TYPE_LABELS } from "../types/export.types";

import { useItem } from "../hooks/useItem";
import { useLots } from "../hooks/useLot";

import { AppLayout } from "../components/layout/AppLayout";

function formatOccurredAt(isoString: string): string {
  const date = new Date(isoString);
  return (
    date.toLocaleString("en-GB", {
      day: "2-digit",
      month: "short",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
      timeZone: "UTC",
    }) + " UTC"
  );
}

export function AuditHistoryPage() {
  const user = useAtomValue(userAtom);
  const isWarehouseAdmin = user?.roles?.includes("WarehouseAdmin") ?? false;

  const [searchParams, setSearchParams] = useSearchParams();
  

  const initialFilter = useMemo<AuditFilter>(() => {
    return {
      eventType:
        (searchParams.get("eventType") as AuditFilter["eventType"]) ||
        DEFAULT_AUDIT_FILTER.eventType,
      itemId: searchParams.get("itemId") || DEFAULT_AUDIT_FILTER.itemId,
      lotId: searchParams.get("lotId") || DEFAULT_AUDIT_FILTER.lotId,
      jobId: searchParams.get("jobId") || DEFAULT_AUDIT_FILTER.jobId,
      from: searchParams.get("from") || DEFAULT_AUDIT_FILTER.from,
      to: searchParams.get("to") || DEFAULT_AUDIT_FILTER.to,
      search : searchParams.get("search") || DEFAULT_AUDIT_FILTER.search,
      page: parseInt(searchParams.get("page") || "1", 10),
      pageSize: parseInt(searchParams.get("pageSize") || "20", 10),
    };
  }, [searchParams]);

  const [filter, setFilter] = useState<AuditFilter>(initialFilter);
  const [pageState, setPageState] = useState<AuditPageState>({
    status: "idle",
  });
  const [stats, setStats] = useState<AuditStats>({
    allEvents: 0,
    consumption: 0,
    disposal: 0,
  });
  const [statsLoading, setStatsLoading] = useState(true);
  const [detailModal, setDetailModal] = useState<{
    isOpen: boolean;
    event: AuditEvent | null;
  }>({
    isOpen: false,
    event: null,
  });
  const [detailLoading, setDetailLoading] = useState(false);

  const [isExportDropdownOpen, setIsExportDropdownOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const { state: exportState, handleExport, resetState, isExporting } = useExport();
  const { mapDatesForExport } = useExportFilterMapper();

  const { items: allItems, isLoading: isLoadingItems } = useItem();
  const { data: lotsData, isLoading: isLoadingLots } = useLots({
    page: 1,
    pageSize: 100,
  });

  const itemOptions: LookupOption[] = useMemo(() => {
    return (allItems ?? []).map((item) => ({
      id: item.id,
      label: item.name,
      sublabel: item.code,
    }));
  }, [allItems]);

  const lotOptions: LookupOption[] = useMemo(() => {
    const lots = lotsData?.items ?? [];
    return lots.map((lot) => ({
      id: lot.id,
      label: lot.lotNumber,
      sublabel: lot.itemName,
    }));
  }, [lotsData]);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setIsExportDropdownOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  useEffect(() => {
    const params = new URLSearchParams();
    if (filter.eventType !== "All") params.set("eventType", filter.eventType);
    if (filter.itemId) params.set("itemId", filter.itemId);
    if (filter.lotId) params.set("lotId", filter.lotId);
    if (filter.jobId) params.set("jobId", filter.jobId);
    if (filter.from) params.set("from", filter.from);
    if (filter.to) params.set("to", filter.to);
    if (filter.search) params.set("search", filter.search);
    if (filter.page > 1) params.set("page", String(filter.page));
    if (filter.pageSize !== 20) params.set("pageSize", String(filter.pageSize));

    setSearchParams(params, { replace: true });
  }, [filter, setSearchParams]);

  const loadData = useCallback(async () => {
    setPageState({ status: "loading" });
    setStatsLoading(true);

    try {
      const [data, statsData] = await Promise.all([
        fetchAuditEvents(filter),
        fetchAuditStats(filter),
      ]);

      setStats(statsData);
      setStatsLoading(false);

      if (data.items.length === 0) {
        setPageState({ status: "empty" });
      } else {
        setPageState({ status: "success", data, stats: statsData });
      }
    } catch (error) {
      setStatsLoading(false);
      const parsed = parseAuditError(error);

      if (parsed.code === 401) {
        setPageState({ status: "unauthorized" });
      } else if (parsed.code === 403) {
        setPageState({ status: "forbidden" });
      } else {
        setPageState({
          status: "error",
          message: parsed.message,
          code: parsed.code,
        });
      }
    }
  }, [filter]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleFilterChange = useCallback((newFilter: AuditFilter) => {
    setFilter(newFilter);
  }, []);

  const handleFilterReset = useCallback(() => {
    setFilter(DEFAULT_AUDIT_FILTER);
  }, []);

  const handlePageChange = useCallback((page: number) => {
    setFilter((prev) => ({ ...prev, page }));
  }, []);

  const handlePageSizeChange = useCallback((pageSize: number) => {
    setFilter((prev) => ({ ...prev, pageSize, page: 1 }));
  }, []);

  const handleViewDetail = useCallback(async (event: AuditEvent) => {
    setDetailModal({ isOpen: true, event });
    setDetailLoading(true);

    try {
      setDetailModal({ isOpen: true, event });
    } catch (error) {
      const parsed = parseAuditError(error);
      console.error("Failed to load detail:", parsed.message);
    } finally {
      setDetailLoading(false);
    }
  }, []);

  const handleCloseDetail = useCallback(() => {
    setDetailModal({ isOpen: false, event: null });
  }, []);

  const handleExportClick = useCallback(
    async (exportType: ExportType) => {
      const { from, to } = mapDatesForExport(filter.from, filter.to);

      await handleExport(exportType, {
        from,
        to,
        itemId: filter.itemId,
        lotId: filter.lotId,
        jobId: filter.jobId,
      });

      // Tutup dropdown setelah export
      setIsExportDropdownOpen(false);
    },
    [filter, handleExport, mapDatesForExport]
  );

  const columns: DataTableColumn<AuditEvent>[] = useMemo(
    () => [
      {
        id: "itemLot",
        header: "ITEM / LOT",
        minWidth: "200px",
        cell: (row) => (
          <div>
            <div className="font-medium text-slate-900">{row.itemName}</div>
            <div className="text-xs text-slate-500">{row.lotNumber}</div>
          </div>
        ),
      },
      {
        id: "quantity",
        header: "QUANTITY",
        width: "120px",
        align: "right",
        cell: (row) => (
          <span className="font-medium text-slate-900">
            {row.quantity} {row.baseUnit}
          </span>
        ),
      },
      {
        id: "jobReason",
        header: "JOB / REASON",
        minWidth: "200px",
        cell: (row) => (
          <span className="text-sm text-slate-700">
            {row.eventType === "Consumption"
              ? row.jobNumber || "—"
              : row.reason || "—"}
          </span>
        ),
      },
      {
        id: "occurredAt",
        header: "OCCURRED AT",
        width: "180px",
        sortValue: (row) => new Date(row.occurredAt),
        cell: (row) => (
          <span className="text-sm text-slate-600">
            {formatOccurredAt(row.occurredAt)}
          </span>
        ),
      },
      {
        id: "event",
        header: "EVENT",
        width: "140px",
        cell: (row) => (
          <span
            className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
              row.eventType === "Consumption"
                ? "bg-cyan-100 text-cyan-700"
                : "bg-red-100 text-red-700"
            }`}
          >
            {row.eventType}
          </span>
        ),
      },
      {
        id: "action",
        header: "",
        width: "100px",
        align: "right",
        cell: (row) => (
          <button
            type="button"
            onClick={() => handleViewDetail(row)}
            className="rounded-lg bg-teal-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-teal-700 focus:outline-none focus:ring-2 focus:ring-teal-500 focus:ring-offset-2"
          >
            View
          </button>
        ),
      },
    ],
    [handleViewDetail]
  );

  const renderExportDropdown = () => (
    <div className="relative" ref={dropdownRef}>
      <button
        type="button"
        onClick={() => setIsExportDropdownOpen(!isExportDropdownOpen)}
        disabled={isExporting}
        className={`inline-flex items-center gap-1.5 rounded-lg border px-4 py-2 text-sm font-medium transition-colors ${
          isExporting
            ? "border-slate-200 bg-slate-100 text-slate-400 cursor-not-allowed"
            : "border-slate-200 bg-white text-slate-700 hover:bg-slate-50 hover:border-teal-300"
        }`}
        aria-expanded={isExportDropdownOpen}
        aria-haspopup="true"
      >
        <Download className="h-4 w-4" />
        {isExporting ? "Exporting..." : "Export CSV"}
        <ChevronDown
          className={`h-4 w-4 transition-transform ${
            isExportDropdownOpen ? "rotate-180" : ""
          }`}
        />
      </button>

      {isExportDropdownOpen && (
        <div className="absolute right-0 top-full z-20 mt-2 w-48 rounded-xl border border-slate-200 bg-white py-1.5 shadow-lg animate-in fade-in zoom-in-95 duration-100">
          {/* Header */}
          <div className="px-3.5 py-2 border-b border-slate-100">
            <p className="text-[11px] font-semibold uppercase tracking-wider text-slate-500">
              Pilih tipe export
            </p>
          </div>

          {/* Menu items */}
          {(Object.keys(EXPORT_TYPE_LABELS) as ExportType[]).map((type) => (
            <button
              key={type}
              type="button"
              onClick={() => handleExportClick(type)}
              disabled={isExporting}
              className="w-full flex items-center gap-2.5 px-3.5 py-2.5 text-left text-sm font-medium text-slate-700 hover:bg-slate-50 hover:text-teal-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <span className="grid h-7 w-7 place-items-center rounded-lg bg-cyan-50 text-teal-700">
                <Download className="h-3.5 w-3.5" />
              </span>
              <div className="flex-1">
                <div className="font-medium">{EXPORT_TYPE_LABELS[type]}</div>
                <div className="text-[11px] text-slate-500 font-normal">
                  {type === "stock"
                    ? "Export stock items"
                    : `Export ${type.toLowerCase()} records`}
                </div>
              </div>
            </button>
          ))}

          {/* Info footer */}
          <div className="px-3.5 py-2 border-t border-slate-100 bg-slate-50">
            <p className="text-[10px] text-slate-500 leading-tight">
              Filter tanggal dari pencarian akan diterapkan pada export.
            </p>
          </div>
        </div>
      )}
    </div>
  );

  return (
    <AppLayout
      title="Audit & History"
      description="Review immutable Consumption and Disposal records from the server"
    >
      <div className="space-y-6">
        <InlineAlert
          tone="info"
          title="Read-only audit trail"
          className="max-w-sm"
        >
          Records cannot be edited or deleted.
        </InlineAlert>

        <AuditStatsCards stats={stats} loading={statsLoading} />

        <div className="flex items-center justify-end">
          {renderExportDropdown()}
        </div>

        {(exportState.status === "success" || exportState.status === "error") && (
          <div className="fixed bottom-6 right-6 z-50 animate-in fade-in slide-in-from-bottom-4 duration-300">
            <InlineAlert
              tone={exportState.status === "success" ? "success" : "error"}
              title={
                exportState.status === "success"
                  ? "Export Berhasil"
                  : "Export Gagal"
              }
              onDismiss={resetState}
              className="min-w-[320px] shadow-lg"
            >
              {exportState.status === "success"
                ? `File ${exportState.filename} telah diunduh.`
                : exportState.message}
            </InlineAlert>
          </div>
        )}

        <AuditFilterBar
          filter={filter}
          onFilterChange={handleFilterChange}
          onReset={handleFilterReset}
          itemOptions={itemOptions}
          lotOptions={lotOptions}
          itemCountLoading={isLoadingItems}
          lotCountLoading={isLoadingLots}
        />

        {pageState.status === "loading" && (
          <LoadingState variant="table" rows={5} columns={6} />
        )}

        {pageState.status === "error" && (
          <ErrorState
            title="Failed to load audit data"
            description={pageState.message}
            onRetry={loadData}
            retryLabel="Try again"
          />
        )}

        {pageState.status === "unauthorized" && (
          <ErrorState
            title="Session expired"
            description="Please log in again to continue."
            onRetry={() => (window.location.href = "/login")}
            retryLabel="Sign in again"
          />
        )}

        {pageState.status === "forbidden" && (
          <ErrorState
            title="Access denied"
            description="You do not have permission to view this page."
            onRetry={() => (window.location.href = "/app/dashboard")}
            retryLabel="Back to Dashboard"
          />
        )}

        {pageState.status === "empty" && (
          <EmptyState
            title="No audit events found"
            description="Try adjusting your filters or reset to see all events."
            action={{ label: "Reset filters", onClick: handleFilterReset }}
          />
        )}

        {pageState.status === "success" && (
          <DataTable
            columns={columns}
            rows={pageState.data.items}
            getRowId={(row) => row.eventId}
            caption="Audit events list"
            page={filter.page}
            pageSize={filter.pageSize}
            totalItems={pageState.data.totalCount}
            onPageChange={handlePageChange}
            onPageSizeChange={handlePageSizeChange}
            emptyState={
              <EmptyState
                title="No events match your filters"
                description="Try adjusting your search criteria."
                action={{ label: "Reset filters", onClick: handleFilterReset }}
              />
            }
          />
        )}

        <AuditDetailModal
          isOpen={detailModal.isOpen}
          onClose={handleCloseDetail}
          event={detailModal.event}
          loading={detailLoading}
        />
      </div>
    </AppLayout>
  );
}