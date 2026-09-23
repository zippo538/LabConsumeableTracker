// src/components/lots/LotSummaryCards.tsx
import type { LotSummary } from "../../types/lot.types";
import { Alert } from "../MessageAlert";
import { SummaryStatsCard } from "../SummaryStatsCard";

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
      <div className="mt-6 grid grid-cols-1 gap-4 lg:grid-cols-4">
        {/* Summary Cards */}
        <div className="lg:col-span-3">
          <SummaryStatsCard
            className="mt-0 grid-cols-1 sm:grid-cols-3"
            stats={
              [
                {
                  value: cards[0].value,
                  title: cards[0].label,
                  color: "blue",
                },
                {
                  value: cards[1].value,
                  title: cards[1].label,
                  color: "amber",
                },
                {
                  value: cards[2].value,
                  title: cards[2].label,
                  color: "teal",
                },
              ] as const
            }
          />
        </div>

        {/* FEFO Information Card */}
        <div className="rounded-lg border border-yellow-200 bg-yellow-50 p-6">
          <div className="flex items-start gap-3">
            <svg
              className="h-5 w-5 flex-shrink-0 text-yellow-600"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>

            <div>
              <h4 className="text-sm font-semibold text-yellow-800">
                FEFO Order
              </h4>

              <p className="mt-1 text-xs text-yellow-700">
                Expiry + ReceivedAt + Id
              </p>

              <p className="mt-1 text-xs text-yellow-600">
                Preview is not a reservation
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
