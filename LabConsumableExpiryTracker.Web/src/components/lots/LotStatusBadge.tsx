// src/components/lots/LotStatusBadge.tsx
import type { ExpiryCondition } from "../../types/lot.types";

interface LotStatusBadgeProps {
  status: ExpiryCondition;
}

export const LotStatusBadge = ({ status }: LotStatusBadgeProps) => {
  const config = {
    Valid: {
      label: "Active",
      className: "bg-green-100 text-green-700",
    },
    ExpiringSoon: {
      label: "Expiring Soon",
      className: "bg-orange-100 text-orange-700",
    },
    Expired: {
      label: "Expired",
      className: "bg-red-100 text-red-700",
    },
  };

  const badge = config[status] || {
    label: status,
    className: "bg-slate-100 text-slate-700",
  };

  return (
    <span
      className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${badge.className}`}
    >
      {badge.label}
    </span>
  );
};
