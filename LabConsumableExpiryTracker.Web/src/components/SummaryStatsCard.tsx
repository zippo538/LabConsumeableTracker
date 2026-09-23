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
}

const colorConfig: Record<
  SummaryCardColor,
  {
    badge: string;
  }
> = {
  teal: {
    badge: "bg-teal-50 text-teal-600",
  },
  amber: {
    badge: "bg-amber-50 text-amber-500",
  },
  rose: {
    badge: "bg-rose-50 text-rose-600",
  },
  red: {
    badge: "bg-red-50 text-red-600",
  },
  blue: {
    badge: "bg-blue-50 text-blue-600",
  },
  slate: {
    badge: "bg-slate-100 text-slate-600",
  },
};


export function SummaryStatsCard({
  stats,
  className = "",
}: SummaryStatsCardProps) {
  const gridColumns =
    stats.length === 1
      ? "grid-cols-1"
      : stats.length === 2
        ? "sm:grid-cols-2"
        : stats.length === 3
          ? "sm:grid-cols-3"
          : "sm:grid-cols-2 lg:grid-cols-4";
  

  return (
    <section className={`mt-6 grid w-full gap-3 ${gridColumns} ${className}`}>
      {stats.map((stat) => {
        const color = colorConfig[stat.color ?? "slate"];

        return (
          <article
            key={stat.title}
            className="rounded-xl border border-slate-200 bg-white p-3 shadow-[0_1px_2px_rgb(15_23_42/0.02)]"
          >
            <div className="flex gap-3">
              <span
                className={`grid h-7 w-7 place-items-center rounded-lg ${color.badge}`}
              >
                <span className="h-2.5 w-2.5 rounded-[3px] bg-current" />
              </span>

              <div>
                <strong className="text-2xl font-bold leading-5 text-slate-900">
                  {stat.value}
                </strong>

                <span className="block text-xs font-medium text-slate-500">
                  {stat.title}
                </span>
              </div>
            </div>

            {stat.note && (
              <p className="mt-4 text-xs text-slate-400">{stat.note}</p>
            )}
          </article>
        );
      })}
    </section>
  );
}
