import React, { useState, useEffect } from "react";
import { useSearchParams, useNavigate } from "react-router";
import { Info } from "lucide-react";
import { AppLayout } from "../components/layout/AppLayout";
import { JobItemSection } from "../components/consumption/JobItemSection";
import { FefoAllocationPreview } from "../components/consumption/FefoAllocationPreview";
import { ConsumptionConfirmation } from "../components/consumption/ConsumptionConfirmation";
import { InlineAlert } from "../components/FeedBackStates";

import { useJobs } from "../hooks/useJob";
import { itemApi } from "../api/itemApi";
import { useQuery } from "@tanstack/react-query";
import { useFefoPreview, useConfirmConsumption } from "../hooks/useConsumption";
import type { FefoAllocationPreviewDto } from "../types/consumption.types";
import type { ItemDto } from "../types/item.types";

export const ConsumptionPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const jobIdFromQuery = searchParams.get("jobId") || "";


  const { data: jobs = [], isLoading: isLoadingJobs } = useJobs();
  const { data: items = [], isLoading: isLoadingItems } = useQuery<ItemDto[]>({
    queryKey: ["items", "all"],
    queryFn: itemApi.getAll,
  });

 
  const [selectedJobId, setSelectedJobId] = useState<string>("");
  const [selectedItemId, setSelectedItemId] = useState<string>("");
  const [requestedQuantity, setRequestedQuantity] = useState<string>("");
  const [previewData, setPreviewData] = useState<FefoAllocationPreviewDto | null>(null);

  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const fefoPreviewMutation = useFefoPreview();
  const confirmConsumptionMutation = useConfirmConsumption();

  useEffect(() => {
    if (jobIdFromQuery && jobs.length > 0) {
      const found = jobs.find(
        (j) => j.id === jobIdFromQuery && j.status === "InProgress"
      );
      if (found) {
        setSelectedJobId(found.id);
      }
    }
  }, [jobIdFromQuery, jobs]);


  const handleSelectJobId = (jobId: string) => {
    setSelectedJobId(jobId);
    setPreviewData(null);
    setErrorMessage(null);
    setSuccessMessage(null);
  };

  const handleSelectItemId = (itemId: string) => {
    setSelectedItemId(itemId);
    setPreviewData(null);
    setErrorMessage(null);
    setSuccessMessage(null);
  };

  const handleQuantityChange = (qty: string) => {
    setRequestedQuantity(qty);
    setPreviewData(null);
    setErrorMessage(null);
    setSuccessMessage(null);
  };

  const handlePreview = () => {
    setErrorMessage(null);
    setSuccessMessage(null);

    const qty = parseFloat(requestedQuantity);
    if (!selectedJobId || !selectedItemId || isNaN(qty) || qty <= 0) {
      setErrorMessage("Please select a valid job, item, and positive quantity.");
      return;
    }

    fefoPreviewMutation.mutate(
      {
        jobId: selectedJobId,
        payload: {
          itemId: selectedItemId,
          requestedQuantity: qty,
        },
      },
      {
        onSuccess: (data) => {
          setPreviewData(data);
          setErrorMessage(null);
        },
        onError: (err: any) => {
          const msg =
            err.response?.data?.message ||
            err.message ||
            "Failed to allocate stock according to FEFO.";
          setErrorMessage(msg);
          setPreviewData(null);
        },
      }
    );
  };

  const handleConfirm = () => {
    if (!previewData || previewData.allocations.length === 0) {
      setErrorMessage("No valid FEFO preview to confirm.");
      return;
    }

    setErrorMessage(null);

    confirmConsumptionMutation.mutate(
      {
        jobId: previewData.jobId || selectedJobId,
        payload: {
          itemId: previewData.itemId || selectedItemId,
          requestedQuantity: previewData.requestedQuantity || parseFloat(requestedQuantity),
          allocations: previewData.allocations.map((a) => ({
            lotId: a.lotId,
            quantity: a.quantity,
            expectedRowVersion: a.rowVersion,
          })),
        },
      },
      {
        onSuccess: (res) => {
          setSuccessMessage(
            `Successfully consumed ${res.totalConsumedQuantity} ${res.baseUnit || ""}. Lot stock has been atomically updated.`
          );
          setPreviewData(null);
          setRequestedQuantity("");
        },
        onError: (err: any) => {
          const msg =
            err.response?.data?.message ||
            err.message ||
            "Failed to confirm consumption. Stock may have changed.";
          setErrorMessage(msg);
        },
      }
    );
  };

  const selectedItem = items.find((i) => i.id === selectedItemId);

  return (
    <AppLayout
      title="Consume"
      description="Active jobs use lot allocation based on FEFO."
    >
      <div className="space-y-6 pb-12">
        <div>
          <h1 className="text-[26px] font-bold tracking-tight text-slate-900">
            Confirm stock consumption
          </h1>
          <p className="mt-1 text-[13px] text-slate-500">
            Active jobs use lot allocation based on FEFO.
          </p>
        </div>

        <div className="flex items-center gap-3 rounded-xl border border-[#b9e8eb] bg-[#eafafd] px-4 py-3 text-[12px] text-[#14546a]">
          <Info className="h-4 w-4 shrink-0 text-[#11a9ae]" />
          <span>
            FEFO preview does not reserve stock. Stock and lot order will be
            checked again at confirmation.
          </span>
        </div>

        {errorMessage && (
          <InlineAlert
            tone="error"
            title="Action could not be completed"
            onDismiss={() => setErrorMessage(null)}
          >
            {errorMessage}
          </InlineAlert>
        )}


        {successMessage && (
          <InlineAlert
            tone="success"
            title="Consumption confirmed"
            onDismiss={() => setSuccessMessage(null)}
          >
            <div className="flex flex-col gap-2">
              <p>{successMessage}</p>
              <div>
                <button
                  type="button"
                  onClick={() => navigate("/app/jobs")}
                  className="text-xs font-semibold text-emerald-800 underline hover:text-emerald-900"
                >
                  Return to Job List →
                </button>
              </div>
            </div>
          </InlineAlert>
        )}

        <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
          <JobItemSection
            jobs={jobs}
            items={items}
            selectedJobId={selectedJobId}
            onSelectJobId={handleSelectJobId}
            selectedItemId={selectedItemId}
            onSelectItemId={handleSelectItemId}
            requestedQuantity={requestedQuantity}
            onRequestedQuantityChange={handleQuantityChange}
            onPreview={handlePreview}
            isPreviewLoading={fefoPreviewMutation.isPending}
            isLoadingJobs={isLoadingJobs}
            isLoadingItems={isLoadingItems}
          />

          <FefoAllocationPreview
            preview={previewData}
            isLoading={fefoPreviewMutation.isPending}
            selectedItem={selectedItem}
          />
        </div>

        <ConsumptionConfirmation
          canConfirm={
            Boolean(previewData) &&
            (previewData?.allocations?.length ?? 0) > 0 &&
            !fefoPreviewMutation.isPending
          }
          isSubmitting={confirmConsumptionMutation.isPending}
          onConfirm={handleConfirm}
          onBack={() => navigate("/app/jobs")}
        />
      </div>
    </AppLayout>
  );
};
