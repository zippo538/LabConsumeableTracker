
import type { AuditStats } from "../../types/audit.types";

interface AuditStatsCardsProps {
  stats: AuditStats;
  loading?: boolean;
}

export function AuditStatsCards({
  stats,
  loading = false,
}: AuditStatsCardsProps) {
  const cards = [
    {
      label: "All events",
      value: stats.allEvents,
      color: "teal",
      borderColor: "border-l-teal-500",
    },
    {
      label: "Consumption",
      value: stats.consumption,
      color: "emerald",
      borderColor: "border-l-emerald-500",
    },
    {
      label: "Disposal",
      value: stats.disposal,
      color: "red",
      borderColor: "border-l-red-500",
    },
  ];

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
      {cards.map((card) => (
        <div
          key={card.label}
          className={`rounded-xl border border-slate-200 border-l-4 ${card.borderColor} bg-white p-5 shadow-sm`}
        >
          <div className="text-2xl font-bold text-slate-900">
            {loading ? (
              <span className="inline-block h-8 w-16 animate-pulse rounded bg-slate-200" />
            ) : (
              card.value
            )}
          </div>
          <div className="mt-1 text-sm text-slate-500">{card.label}</div>
        </div>
      ))}
    </div>
  );
}
