// src/components/lots/LotSummaryCards.tsx
import type { LotSummary } from "../../types/lot.types";
import { Alert } from "../MessageAlert";

interface LotSummaryCardsProps {
  summary: LotSummary;
  message?: {
    variant: "success" | "warning" | "error" | "info";
    title: string;
    description?: string;
  };
}

export const LotSummaryCards = ({ summary, message }: LotSummaryCardsProps) => {
  const cards = [
    {
      label: "Active lots",
      value: summary.activeLotCount,
      dotColor: "bg-teal-500",
    },
    {
      label: "Expiring soon",
      value: summary.expiringSoonLotCount,
      dotColor: "bg-orange-500",
    },
    {
      label: "Not eligible",
      value: summary.notEligibleLotCount,
      dotColor: "bg-red-500",
    },
  ];

  return (
    <div className="space-y-12">
      {/* Message Alert - ditampilkan jika ada pesan dari parent */}
      {message && (
        <Alert
          variant={message.variant}
          title={message.title}
          description={message.description}
          dismissible={false}
        />
      )}
      {/* Summary Cards */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        {cards.map((card) => (
          <div
            key={card.label}
            className="rounded-lg border border-gray-200 bg-white p-6"
          >
            <div className="flex items-center gap-3">
              <div className={`h-3 w-3 rounded-full ${card.dotColor}`} />
              <span className="text-3xl font-bold text-gray-900">
                {card.value}
              </span>
            </div>
            <p className="mt-2 text-sm text-gray-500">{card.label}</p>
          </div>
        ))}
      </div>
    </div>
  );
};