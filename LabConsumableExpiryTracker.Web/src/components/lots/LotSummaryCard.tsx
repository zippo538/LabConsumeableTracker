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
      label: "Active Lots",
      value: summary.activeLotCount,
      color: "green",
      icon: (
        <svg
          className="h-5 w-5"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M5 13l4 4L19 7"
          />
        </svg>
      ),
    },
    {
      label: "Expiring Soon",
      value: summary.expiringSoonLotCount,
      color: "orange",
      icon: (
        <svg
          className="h-5 w-5"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
          />
        </svg>
      ),
    },
    {
      label: "Not Eligible",
      value: summary.notEligibleLotCount,
      color: "red",
      icon: (
        <svg
          className="h-5 w-5"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M6 18L18 6M6 6l12 12"
          />
        </svg>
      ),
    },
  ];

  const colorClasses = {
    green: "bg-green-50 text-green-700 border-green-200",
    orange: "bg-orange-50 text-orange-700 border-orange-200",
    red: "bg-red-50 text-red-700 border-red-200",
  };

  const iconColorClasses = {
    green: "bg-green-100 text-green-600",
    orange: "bg-orange-100 text-orange-600",
    red: "bg-red-100 text-red-600",
  };

  return (
    <div className="space-y-4">
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
            className={`rounded-lg border p-4 ${colorClasses[card.color as keyof typeof colorClasses]}`}
          >
            <div className="flex items-center gap-3">
              <div
                className={`rounded-lg p-2 ${iconColorClasses[card.color as keyof typeof iconColorClasses]}`}
              >
                {card.icon}
              </div>
              <div>
                <p className="text-2xl font-bold">{card.value}</p>
                <p className="text-xs font-medium">{card.label}</p>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};
