export const FefoInfoBox = () => {
  return (
    <div className="rounded-lg border border-yellow-200 bg-yellow-50 p-4">
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
          <h4 className="text-sm font-semibold text-yellow-800">FEFO Order</h4>
          <p className="mt-1 text-xs text-yellow-700">
            Expiry + ReceivedAt + Id
          </p>
          <p className="mt-1 text-xs text-yellow-600">
            Preview is not a reservation
          </p>
        </div>
      </div>
    </div>
  );
};
