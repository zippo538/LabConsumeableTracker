import { useMemo, useState } from "react";
import { useAtomValue } from "jotai";

import { useConsumptionHistory } from "../hooks/useConsumptionHistory";
import { userAtom } from "../lib/auth/authAtom";

import { AppLayout } from "../components/layout/AppLayout";
import StatCard from "../components/consumption-history/StatCard";
import ConsumptionHistoryFilters from "../components/consumption-history/ConsumptionHistoryFilters";
import ConsumptionHistoryTable from "../components/consumption-history/ConsumptionHistoryTable";
import { Pagination } from "../components/Pagination";

const DATE_RANGE_OPTIONS = [
  { label: "All time", value: "all" },
  { label: "Last 7 days", value: "7d" },
  { label: "Last 30 days", value: "30d" },
  { label: "This month", value: "month" },
] as const;

type DateRangeValue =
  (typeof DATE_RANGE_OPTIONS)[number]["value"];

const UNIT_LABELS: Record<string, string> = {
  Milliliter: "mL",
  Liter: "L",
  Gram: "g",
  Kilogram: "kg",
  Microliter: "µL",
  Vial: "vial",
  Unit: "unit",
  Box: "box",
};

const formatUnit = (baseUnit: string) =>
  UNIT_LABELS[baseUnit] ?? baseUnit.toLowerCase();

const formatConsumedAt = (isoDate: string) => {
  const date = new Date(isoDate);

  const formatted = new Intl.DateTimeFormat("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
    timeZone: "UTC",
  }).format(date);

  return `${formatted} UTC`;
};

const formatQuantity = (quantity: number) =>
  Number.isInteger(quantity)
    ? quantity.toString()
    : quantity.toFixed(2);

const toConsumptionRef = (id: string) =>
  `CON-${id.slice(0, 8).toUpperCase()}`;

const resolveDateRange = (
  value: DateRangeValue
): { from?: string; to?: string } => {
  if (value === "all") {
    return {};
  }

  const now = new Date();
  const to = now.toISOString();

  if (value === "7d") {
    const start = new Date(now);
    start.setDate(start.getDate() - 7);

    return {
      from: start.toISOString(),
      to,
    };
  }

  if (value === "30d") {
    const start = new Date(now);
    start.setDate(start.getDate() - 30);

    return {
      from: start.toISOString(),
      to,
    };
  }

  const start = new Date(
    now.getFullYear(),
    now.getMonth(),
    1
  );

  return {
    from: start.toISOString(),
    to,
  };
};

export default function ConsumptionHistoryPage() {
  const [jobId, setJobId] = useState("");
  const [itemId, setItemId] = useState("");
  const [lotNumber, setLotNumber] = useState("");
  const [dateRange, setDateRange] =
    useState<DateRangeValue>("all");

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const user = useAtomValue(userAtom);

  const { from, to } = resolveDateRange(dateRange);

  const {
    data,
    isLoading,
    isError,
  } = useConsumptionHistory({
    page,
    pageSize,
    jobId: jobId || undefined,
    itemId: itemId || undefined,
    lotNumber: lotNumber || undefined,
    from,
    to,
  });

  const items = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;

  const jobOptions = useMemo(() => {
    const map = new Map<string, string>();

    items.forEach((item) => {
      map.set(item.jobId, item.jobNumber);
    });

    return Array.from(map.entries());
  }, [items]);

  const itemOptions = useMemo(() => {
    const map = new Map<string, string>();

    items.forEach((item) => {
      map.set(item.itemId, item.itemName);
    });

    return Array.from(map.entries());
  }, [items]);

  const uniqueJobCount = useMemo(
    () => new Set(items.map((item) => item.jobId)).size,
    [items]
  );

  const uniqueItemCount = useMemo(
    () => new Set(items.map((item) => item.itemId)).size,
    [items]
  );

  const resetPage = () => {
    setPage(1);
  };

  const handleJobChange = (value: string) => {
    setJobId(value);
    resetPage();
  };

  const handleItemChange = (value: string) => {
    setItemId(value);
    resetPage();
  };

  const handleLotNumberChange = (value: string) => {
    setLotNumber(value);
    resetPage();
  };

  const handleDateRangeChange = (value: string) => {
    setDateRange(value as DateRangeValue);
    resetPage();
  };

  const handlePageSizeChange = (value: number) => {
    setPage(1);
    setPageSize(value);
  };

  return (
    <AppLayout
      title="Consumption History"
      description="Review committed consumption records"
    >
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">
            Consumption History
          </h1>

          <p className="mt-1 text-sm text-slate-500">
            Review committed consumption records from the server.
          </p>
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <StatCard
            value={totalCount}
            label="Consumption records"
            accent="bg-teal-600"
          />

          <StatCard
            value={uniqueJobCount}
            label="Jobs with consumption"
            accent="bg-amber-500"
          />

          <StatCard
            value={uniqueItemCount}
            label="Items used"
            accent="bg-emerald-600"
          />

          <div className="rounded-xl border border-teal-100 bg-teal-50 p-5">
            <p className="text-sm font-semibold text-teal-800">
              Read-only audit
            </p>

            <p className="mt-1 text-sm text-teal-700">
              Newest events appear first.
            </p>

            <p className="text-sm text-teal-700">
              Job filters apply only to Consumption.
            </p>
          </div>
        </div>

        <ConsumptionHistoryFilters
          lotNumber={lotNumber}
          jobId={jobId}
          itemId={itemId}
          dateRange={dateRange}
          jobOptions={jobOptions}
          itemOptions={itemOptions}
          dateRangeOptions={DATE_RANGE_OPTIONS}
          onLotNumberChange={handleLotNumberChange}
          onJobChange={handleJobChange}
          onItemChange={handleItemChange}
          onDateRangeChange={handleDateRangeChange}
        />

        <div className="overflow-hidden rounded-xl border border-slate-200 bg-white">
          <ConsumptionHistoryTable
            items={items}
            isLoading={isLoading}
            isError={isError}
            userName={user?.fullName ?? "-"}
            formatQuantity={formatQuantity}
            formatUnit={formatUnit}
            formatConsumedAt={formatConsumedAt}
            toConsumptionRef={toConsumptionRef}
          />

          <Pagination
            page={page}
            onPageChange={setPage}
            totalItems={totalCount}
            pageSize={pageSize}
            onPageSizeChange={handlePageSizeChange}
            pageSizeOptions={[10, 20, 50, 100]}
          />
        </div>
      </div>
    </AppLayout>
  );
}