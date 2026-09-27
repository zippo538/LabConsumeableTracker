
import { SummaryStatsCard } from "../SummaryStatsCard";

interface ItemSummaryCardsProps {
  stats: {
    totalActive: number;
    lowStock: number;
    uniqueUnitsCount: number;
  };
}

export const ItemSummaryCards = ({ stats }: ItemSummaryCardsProps) => {
  return (
    <SummaryStatsCard
      columns={3}
      className="col-span-1 mt-0 sm:col-span-2 lg:col-span-3"
      stats={[
        {
          value: stats.totalActive,
          title: "All items",
          color: "blue",
        },
        {
          value: stats.lowStock,
          title: "Low stock",
          color: "amber",
        },
        {
          value: stats.uniqueUnitsCount,
          title: "Unit types",
          color: "teal",
        },
      ]}
    />
  );
};