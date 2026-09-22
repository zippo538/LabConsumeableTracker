import { useCallback, useEffect, useMemo, useState } from "react";
import { useSearchParams } from "react-router";
import { Download } from "lucide-react";

// Shared components
import {
  DataTable,
  type DataTableColumn,
} from "../components/DataTable";
import { InlineAlert } from "../components/FeedBackStates";
import {
  LoadingState,
  EmptyState,
} from "../components/FeedBackStates";

// Audit components
import { AuditFilterBar } from "../components/audit/AuditFilterBar";
import { AuditStatsCards } from "../components/audit/AuditSummaryCard";
import { AuditDetailModal } from "../components/audit/AuditDetailModal";

// API & types
import {
  fetchAuditEvents,
  fetchAuditStats,
  parseAuditError,
} from "../api/auditApi";
import type {
  AuditEvent,
  AuditFilter,
  AuditPageState,
  AuditStats,
  LookupOption,
} from "../types/audit.types";
import { DEFAULT_AUDIT_FILTER } from "../types/audit.types";

// Layout
import { AppLayout } from "../components/layout/AppLayout";

// ============================================================
// Helper: Format tanggal ISO ke human-readable
// ============================================================
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



// ============================================================
// Main Page Component
// ============================================================
export function AuditHistoryPage() {
  // URL search params untuk filter persistence (UI-14, UI-15)
  const [searchParams, setSearchParams] = useSearchParams();

  // Parse filter dari URL atau gunakan default
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
      page: parseInt(searchParams.get("page") || "1", 10),
      pageSize: parseInt(searchParams.get("pageSize") || "20", 10),
    };
  }, [searchParams]);

  // State
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

  // Mock lookup options (ganti dengan API call ke /api/items dan /api/lots)
  const [itemOptions] = useState<LookupOption[]>([
    { id: "item-1", label: "Buffer Reagent A", sublabel: "BUF-001" },
    { id: "item-2", label: "Reagent B", sublabel: "REB-002" },
  ]);
  const [lotOptions] = useState<LookupOption[]>([
    { id: "lot-1", label: "LOT-713947A0-002" },
    { id: "lot-2", label: "LOT-XXXX-001" },
  ]);

  // ============================================================
  // Sync filter ke URL (UI-14: hanya filter aman)
  // ============================================================
  useEffect(() => {
    const params = new URLSearchParams();
    if (filter.eventType !== "All") params.set("eventType", filter.eventType);
    if (filter.itemId) params.set("itemId", filter.itemId);
    if (filter.lotId) params.set("lotId", filter.lotId);
    if (filter.jobId) params.set("jobId", filter.jobId);
    if (filter.from) params.set("from", filter.from);
    if (filter.to) params.set("to", filter.to);
    if (filter.page > 1) params.set("page", String(filter.page));
    if (filter.pageSize !== 20) params.set("pageSize", String(filter.pageSize));

    setSearchParams(params, { replace: true });
  }, [filter, setSearchParams]);

  // ============================================================
  // Fetch data audit
  // ============================================================
  const loadData = useCallback(async () => {
    setPageState({ status: "loading" });
    setStatsLoading(true);

    try {
      // Fetch stats dan data secara paralel
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

  // Load data saat filter berubah
  useEffect(() => {
    loadData();
  }, [loadData]);

  // ============================================================
  // Handler: Filter change
  // ============================================================
  const handleFilterChange = useCallback((newFilter: AuditFilter) => {
    setFilter(newFilter);
  }, []);

  const handleFilterReset = useCallback(() => {
    setFilter(DEFAULT_AUDIT_FILTER);
  }, []);

  // ============================================================
  // Handler: Pagination
  // ============================================================
  const handlePageChange = useCallback((page: number) => {
    setFilter((prev) => ({ ...prev, page }));
  }, []);

  const handlePageSizeChange = useCallback((pageSize: number) => {
    setFilter((prev) => ({ ...prev, pageSize, page: 1 }));
  }, []);

  // ============================================================
  // Handler: View detail
  // ============================================================
  const handleViewDetail = useCallback(async (event: AuditEvent) => {
    setDetailModal({ isOpen: true, event });
    setDetailLoading(true);

    try {
      // Fetch detail lengkap dari endpoint spesifik
      // const detail = event.eventType === "Consumption"
      //   ? await fetchConsumptionDetail(event.eventId)
      //   : await fetchDisposalDetail(event.eventId);
      // setDetailModal({ isOpen: true, event: detail });

      // Untuk demo, gunakan data yang sudah ada
      setDetailModal({ isOpen: true, event });
    } catch (error) {
      const parsed = parseAuditError(error);
      // Tampilkan error di modal atau toast
      console.error("Failed to load detail:", parsed.message);
    } finally {
      setDetailLoading(false);
    }
  }, []);

  const handleCloseDetail = useCallback(() => {
    setDetailModal({ isOpen: false, event: null });
  }, []);

  // ============================================================
  // Handler: Export (F-10 — Nice/PROPOSED, disabled untuk MVP)
  // ============================================================
  const handleExportCSV = useCallback(() => {
    // F-10 masih Nice/PROPOSED — tampilkan tooltip "Available in future release"
    alert("Export CSV akan tersedia di rilis mendatang.");
  }, []);

  // ============================================================
  // Kolom tabel (DataTable)
  // ============================================================
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

  // ============================================================
  // Render
  // ============================================================
  return (
    <AppLayout
      title="Audit & History"
      description="Review immutable Consumption and Disposal records from the server"
    >
      <div className="space-y-6">
        {/* Read-only info banner (MessageAlert) */}
        <InlineAlert
          tone="info"
          title="Read-only audit trail"
          className="max-w-sm"
        >
          Records cannot be edited or deleted.
        </InlineAlert>

        {/* Stats Cards */}
        <AuditStatsCards stats={stats} loading={statsLoading} />

        {/* Export Buttons (F-10 — Nice/PROPOSED) */}
        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={handleExportCSV}
            disabled
            className="inline-flex items-center gap-1.5 rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-400 cursor-not-allowed"
            title="Available in future release"
          >
            <Download className="h-4 w-4" />
            Export CSV
          </button>
          {/* Export PDF dihapus — tidak ada dalam SRS */}
        </div>

        {/* Filter Bar */}
        <AuditFilterBar
          filter={filter}
          onFilterChange={handleFilterChange}
          onReset={handleFilterReset}
          itemOptions={itemOptions}
          lotOptions={lotOptions}
        />

        {/* Table / State Display */}
        {pageState.status === "loading" && (
          <LoadingState variant="table" rows={5} columns={6} />
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

        {/* Detail Modal */}
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