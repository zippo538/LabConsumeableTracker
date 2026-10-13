
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

export const LotSummaryCards = ({
  summary,
  message,
}: LotSummaryCardsProps) => {
  return (
    <div className="space-y-4">
      {message && (
        <Alert
          variant={message.variant}
          title={message.title}
          description={message.description}
          dismissible={false}
        />
      )}

      <SummaryStatsCard
        columns={4}
        className="mt-0"
        stats={[
          {
            title: "Active lots",
            value: summary.activeLotCount,
            color: "blue",
          },
          {
            title: "Expiring soon",
            value: summary.expiringSoonLotCount,
            color: "amber",
          },
          {
            title: "Not eligible",
            value: summary.notEligibleLotCount,
            color: "teal",
          },
        ]}
        aside={
          <div className="flex items-start gap-2.5">
            <svg
              className="mt-0.5 h-4 w-4 shrink-0 text-yellow-600"
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
              <p className="text-xs font-bold text-yellow-800">
                FEFO Order
              </p>

              <p className="mt-1 text-[11px] leading-relaxed text-yellow-700">
                Expiry + ReceivedAt + Id
                <br />
                <span className="font-medium text-yellow-600">
                  Preview is not a reservation
                </span>
              </p>
            </div>
          </div>
        }
      />
    </div>
  );
};