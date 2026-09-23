import { Link, useParams } from "react-router";
import {
  ArrowLeft,
  CalendarDays,
  ClipboardList,
  FlaskConical,
  MapPin,
  Package,
  User,
} from "lucide-react";

import { AppLayout } from "../components/layout/AppLayout";
import { useConsumptionHistoryDetail } from "../hooks/useConsumptionHistory";

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

const formatQuantity = (quantity: number) =>
  Number.isInteger(quantity) ? quantity.toString() : quantity.toFixed(2);

const formatDate = (value: string) => {
  const date = new Date(value);

  return new Intl.DateTimeFormat("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
    timeZone: "UTC",
  }).format(date);
};

const formatExpiryDate = (value: string) => {
  const date = new Date(value);

  return new Intl.DateTimeFormat("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    timeZone: "UTC",
  }).format(date);
};

const toConsumptionRef = (id: string) =>
  `CON-${id.slice(0, 8).toUpperCase()}`;

export default function ConsumptionHistoryDetailPage() {
  const { id } = useParams<{ id: string }>();

  const { data, isLoading, isError } =
    useConsumptionHistoryDetail(id);

  const consumption = data?.data;

  if (isLoading) {
    return (
      <AppLayout
        title="Consumption Detail"
        description="View consumption record details"
      >
        <LoadingState />
      </AppLayout>
    );
  }

  if (isError || !consumption) {
    return (
      <AppLayout
        title="Consumption Detail"
        description="View consumption record details"
      >
        <ErrorState />
      </AppLayout>
    );
  }

  return (
    <AppLayout
      title="Consumption Detail"
      description="View consumption record details"
    >
      <div className="space-y-6">
        <Link
          to="/app/consumption-history"
          className="inline-flex items-center gap-2 text-sm font-medium text-slate-500 hover:text-teal-700"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Consumption History
        </Link>
        
        <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-start">
          <div>
            <div className="flex items-center gap-3">
              <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-teal-50">
                <ClipboardList className="h-5 w-5 text-teal-700" />
              </div>

              <div>
                <p className="text-sm font-medium text-slate-400">
                  Consumption Record
                </p>

                <h1 className="text-2xl font-bold text-slate-900">
                  {toConsumptionRef(consumption.id)}
                </h1>
              </div>
            </div>

            <p className="mt-3 text-sm text-slate-500">
              Details of the committed consumable usage.
            </p>
          </div>

          <div className="rounded-lg bg-emerald-50 px-3 py-2 text-sm font-semibold text-emerald-700">
            Committed
          </div>
        </div>
        
        <section className="rounded-xl border border-slate-200 bg-white">
          <div className="border-b border-slate-100 px-6 py-4">
            <div className="flex items-center gap-2">
              <ClipboardList className="h-5 w-5 text-teal-700" />
              <h2 className="font-semibold text-slate-900">
                Consumption
              </h2>
            </div>
          </div>

          <div className="grid grid-cols-1 gap-6 p-6 sm:grid-cols-2 lg:grid-cols-3">
            <DetailItem
              label="Consumption Reference"
              value={toConsumptionRef(consumption.id)}
            />

            <DetailItem
              label="Quantity"
              value={`${formatQuantity(consumption.quantity)} ${formatUnit(
                consumption.baseUnit
              )}`}
            />

            <DetailItem
              label="Consumed At"
              value={`${formatDate(consumption.consumedAt)} UTC`}
            />

            <DetailItem
              label="Consumed By"
              value={consumption.consumedBy}
              icon={<User className="h-4 w-4" />}
            />
          </div>
        </section>
        
        <section className="rounded-xl border border-slate-200 bg-white">
          <div className="border-b border-slate-100 px-6 py-4">
            <div className="flex items-center gap-2">
              <ClipboardList className="h-5 w-5 text-amber-600" />
              <h2 className="font-semibold text-slate-900">
                Job
              </h2>
            </div>
          </div>

          <div className="grid grid-cols-1 gap-6 p-6 sm:grid-cols-2">
            <DetailItem
              label="Job Number"
              value={consumption.jobNumber}
            />

            <DetailItem
              label="Description"
              value={consumption.jobDescription || "-"}
            />
          </div>
        </section>
        
        <section className="rounded-xl border border-slate-200 bg-white">
          <div className="border-b border-slate-100 px-6 py-4">
            <div className="flex items-center gap-2">
              <Package className="h-5 w-5 text-emerald-600" />
              <h2 className="font-semibold text-slate-900">
                Item & Lot
              </h2>
            </div>
          </div>

          <div className="grid grid-cols-1 gap-6 p-6 sm:grid-cols-2 lg:grid-cols-3">
            <DetailItem
              label="Item Code"
              value={consumption.itemCode}
              icon={<FlaskConical className="h-4 w-4" />}
            />

            <DetailItem
              label="Item Name"
              value={consumption.itemName}
            />

            <DetailItem
              label="Base Unit"
              value={formatUnit(consumption.baseUnit)}
            />

            <DetailItem
              label="Lot Number"
              value={consumption.lotNumber}
            />

            <DetailItem
              label="Expiry Date"
              value={formatExpiryDate(consumption.expiryDate)}
              icon={<CalendarDays className="h-4 w-4" />}
            />

            <DetailItem
              label="Storage Location"
              value={consumption.storageLocation}
              icon={<MapPin className="h-4 w-4" />}
            />
          </div>
        </section>
      </div>
    </AppLayout>
  );
}

function DetailItem({
  label,
  value,
  icon,
}: {
  label: string;
  value: string;
  icon?: React.ReactNode;
}) {
  return (
    <div>
      <p className="mb-1.5 flex items-center gap-1.5 text-xs font-medium uppercase tracking-wide text-slate-400">
        {icon}
        {label}
      </p>

      <p className="break-words text-sm font-semibold text-slate-800">
        {value}
      </p>
    </div>
  );
}

function LoadingState() {
  return (
    <div className="space-y-6">
      <div className="h-5 w-48 animate-pulse rounded bg-slate-100" />
      <div className="h-20 w-72 animate-pulse rounded bg-slate-100" />

      {Array.from({ length: 3 }).map((_, index) => (
        <div
          key={index}
          className="rounded-xl border border-slate-200 bg-white p-6"
        >
          <div className="mb-6 h-5 w-32 animate-pulse rounded bg-slate-100" />

          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 3 }).map((_, itemIndex) => (
              <div key={itemIndex} className="space-y-2">
                <div className="h-3 w-24 animate-pulse rounded bg-slate-100" />
                <div className="h-4 w-40 animate-pulse rounded bg-slate-100" />
              </div>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}

function ErrorState() {
  return (
    <div className="flex min-h-[400px] items-center justify-center rounded-xl border border-slate-200 bg-white">
      <div className="text-center">
        <p className="font-semibold text-slate-800">
          Couldn't load consumption details
        </p>

        <p className="mt-1 text-sm text-slate-400">
          The consumption record may not exist or you may not have access
          to it.
        </p>

        <Link
          to="/app/consumptions-history"
          className="mt-4 inline-flex items-center gap-2 rounded-lg bg-teal-800 px-4 py-2 text-sm font-semibold text-white hover:bg-teal-900"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to History
        </Link>
      </div>
    </div>
  );
}