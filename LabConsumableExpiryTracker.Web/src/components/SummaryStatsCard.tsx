
import type { ReactNode } from "react";

export type SummaryCardColor =
  | "teal"
  | "amber"
  | "rose"
  | "red"
  | "blue"
  | "slate";

export interface SummaryStat {
  value: string | number;
  title: string;
  note?: string;
  color?: SummaryCardColor;
}

interface SummaryStatsCardProps {
  stats: SummaryStat[];
  className?: string;
  columns?: 1 | 2 | 3 | 4;
  aside?: ReactNode;
}

const colorConfig: Record<SummaryCardColor, string> = {
  teal: "bg-teal-50 text-teal-600",
  amber: "bg-amber-50 text-amber-500",
  rose: "bg-rose-50 text-rose-600",
  red: "bg-red-50 text-red-600",
  blue: "bg-blue-50 text-blue-600",
  slate: "bg-slate-100 text-slate-600",
};

const columnConfig = {
  1: "grid-cols-1",
  2: "sm:grid-cols-2",
  3: "sm:grid-cols-3",
  4: "sm:grid-cols-2 lg:grid-cols-4",
};

export function SummaryStatsCard({
  stats,
  className = "",
  columns,
  aside,
}: SummaryStatsCardProps) {
  const columnCount =
    columns ?? (aside ? 4 : Math.min(stats.length, 4) as 1 | 2 | 3 | 4);

  return (
    <section
      className={`grid w-full items-stretch gap-3 ${columnConfig[columnCount]} ${className}`}
    >
      {stats.map((stat) => (
        <article
          key={stat.title}
          className="flex h-full min-w-0 flex-col justify-center rounded-xl border border-slate-200 bg-white p-3 shadow-[0_1px_2px_rgb(15_23_42/0.02)]"
        >
          <div className="flex items-center gap-3">
            <span
              className={`grid h-7 w-7 shrink-0 place-items-center rounded-lg ${
                colorConfig[stat.color ?? "slate"]
              }`}
            >
              <span className="h-2.5 w-2.5 rounded-[3px] bg-current" />
            </span>

            <div className="min-w-0">
              <strong className="block text-2xl font-bold leading-6 text-slate-900">
                {stat.value}
              </strong>

              <span className="block text-xs font-medium text-slate-500">
                {stat.title}
              </span>
            </div>
          </div>

          {stat.note && (
            <p className="mt-3 text-xs text-slate-400">
              {stat.note}
            </p>
          )}
        </article>
      ))}

      {aside && (
        <aside className="flex h-full min-w-0 items-center rounded-xl border border-yellow-200 bg-yellow-50 p-3 shadow-[0_1px_2px_rgb(15_23_42/0.02)]">
          {aside}
        </aside>
      )}
    </section>
  );
}