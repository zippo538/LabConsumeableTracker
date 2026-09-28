import React, { useState } from "react";
import { useNavigate, useParams } from "react-router";
import {
  Edit2,
  Plus,
  Package,
  AlertTriangle,
  CalendarDays,
  Layers,
  ArrowLeft,
} from "lucide-react";

import { AppLayout } from "../components/layout/AppLayout";
import { ItemModal } from "../components/Item/ItemModal";
import { useItemDetail } from "../hooks/useItemDetail";
import { useItem } from "../hooks/useItem";

import type {
  CreateItemPayload,
  UpdateItemPayload,
} from "../types/item.types";
import { useImage } from "../hooks/useImage";

export const ItemDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const { data: item, isLoading, isError, refetch } = useItemDetail(id);

  const { updateItem, isUpdating } = useItem();

  const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
  const IMAGE_BASE_URL = API_BASE_URL.replace(/\/api$/, "");

  const ItemRowImage: React.FC<{
    imageId: string | null | undefined;
    alt: string;
  }> = ({ imageId, alt }) => {
    const { imageUrl, isLoading, error } = useImage(imageId);

    if (!imageId || isLoading || error || !imageUrl) {
      return <Layers className="w-5 h-5 opacity-50 text-teal-600" />;
    }

    return (
      <img
        src={imageUrl}
        alt={alt}
        className="w-full h-full object-cover"
        onError={() => {
          <Package className="w-5 h-5" />;
        }}
      />
    );
  };

  const formatDate = (date: string) => {
    const parsedDate = new Date(date);

    if (Number.isNaN(parsedDate.getTime())) {
      return date;
    }

    return parsedDate.toLocaleDateString("en-GB", {
      day: "2-digit",
      month: "short",
      year: "numeric",
    });
  };

  const getStockStatusLabel = (status: string | number) => {
    switch (status) {
      case "Sufficient":
        return "Sufficient";
      case "AtMinimum":
        return "At Minimum";
      case "LowStock":
        return "Low Stock";
      case "OutOfStock":
        return "Out of Stock";
      default:
        return String(status);
    }
  };

  const getStockStatusClass = (status: string | number) => {
    switch (status) {
      case "Sufficient":
        return "bg-emerald-50 text-emerald-700 border-emerald-100";
      case "AtMinimum":
        return "bg-amber-50 text-amber-700 border-amber-100";
      case "LowStock":
        return "bg-orange-50 text-orange-700 border-orange-100";
      case "OutOfStock":
        return "bg-rose-50 text-rose-700 border-rose-100";
      default:
        return "bg-slate-50 text-slate-600 border-slate-100";
    }
  };

  const handleUpdateItem = async (
    payload: CreateItemPayload | UpdateItemPayload,
  ) => {
    if (!("id" in payload)) {
      return;
    }

    setSuccessMessage(null);

    await updateItem(payload);
    await refetch();

    setIsEditModalOpen(false);
    setSuccessMessage("Item updated successfully.");
  };

  if (isLoading) {
    return (
      <AppLayout
        title="Item Details"
        description="Master item, usable stock, and associated lots"
      >
        <div className="flex items-center justify-center py-32">
          <div className="text-center">
            <div className="inline-block animate-spin rounded-full h-8 w-8 border-4 border-slate-200 border-t-emerald-600" />

            <p className="mt-3 text-xs font-medium text-slate-500">
              Loading item details...
            </p>
          </div>
        </div>
      </AppLayout>
    );
  }

  if (isError || !item) {
    return (
      <AppLayout
        title="Item Details"
        description="Master item, usable stock, and associated lots"
      >
        <button
          type="button"
          onClick={() => navigate("/app/items")}
          className="mb-5 inline-flex items-center gap-2 text-[12px] font-medium text-slate-500 transition-colors hover:text-[#0da7ad]"
        >
          <ArrowLeft className="h-3.5 w-3.5" />
          Back to Item List
        </button>
      </AppLayout>
    );
  }

  const lots = item.lots ?? [];

  return (
    <AppLayout
      title="Item Details"
      description="Master item, usable stock, and associated lots"
    >
      <div className="space-y-5 pb-12">
        <button
          type="button"
          onClick={() => navigate("/app/items")}
          className="mb-5 inline-flex items-center gap-2 text-[12px] font-medium text-slate-500 transition-colors hover:text-[#0da7ad]"
        >
          <ArrowLeft className="h-3.5 w-3.5" />
          Back to Item List
        </button>

        {successMessage && (
          <div className="rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
            {successMessage}
          </div>
        )}

        <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs px-5 py-4">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-teal-50 border border-teal-100 flex items-center justify-center text-teal-600 overflow-hidden shrink-0">
                <ItemRowImage imageId={item.imageId} alt={item.name} />
              </div>

              <div>
                <h2 className="text-lg font-bold text-slate-900">
                  {item.name}
                </h2>

                <div className="flex items-center gap-2 mt-1">
                  <span className="px-2.5 py-1 rounded-full bg-emerald-50 border border-emerald-100 text-[10px] font-bold text-emerald-700">
                    ACTIVE
                  </span>

                  <span className="px-2.5 py-1 rounded-full bg-sky-50 border border-sky-100 text-[10px] font-semibold text-sky-700">
                    {item.code}
                  </span>
                </div>
              </div>
            </div>

            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={() => {
                  setSuccessMessage(null);
                  setIsEditModalOpen(true);
                }}
                className="flex items-center gap-2 px-4 py-2 rounded-lg bg-slate-100 border-[#c3d3df] text-black text-[11px] font-semibold hover:bg-slate-300 transition-colors"
              >
                <Edit2 className="w-3.5 h-3.5" />
                Edit Item
              </button>
            </div>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <div className="bg-[#FCEBEB] rounded-2xl border border-[#C93838] p-4 shadow-2xs">
            <div className="flex items-center gap-3">
              <div className="w-9 h-9 rounded-lg bg-rose-50 flex items-center justify-center">
                <Package className="w-4 h-4 text-rose-600" />
              </div>

              <div>
                <p className="text-lg font-bold text-slate-900">
                  {item.totalRemainingQuantity} {item.baseUnit}
                </p>

                <p className="text-[10px] text-slate-500">Usable Stock</p>
              </div>
            </div>

            <p className="text-[10px] text-slate-400 mt-4">
              {item.isLowStock ? "Below the minimum" : "Stock is sufficient"}
            </p>
          </div>

          <div className="bg-[#FFF5DB] rounded-2xl border border-[#B86E00] p-4 shadow-2xs">
            <div className="flex items-center gap-3">
              <div className="w-9 h-9 rounded-lg bg-amber-50 flex items-center justify-center">
                <AlertTriangle className="w-4 h-4 text-amber-600" />
              </div>

              <div>
                <p className="text-lg font-bold text-slate-900">
                  {item.minimumStock} {item.baseUnit}
                </p>

                <p className="text-[10px] text-slate-500">Minimum Stock</p>
              </div>
            </div>

            <p className="text-[10px] text-slate-400 mt-4">
              Status: {getStockStatusLabel(item.stockStatus)}
            </p>
          </div>

          <div className="bg-[#E5F7F7] rounded-2xl border border-[#0EA5A5] p-4 shadow-2xs">
            <div className="flex items-center gap-3">
              <div className="w-9 h-9 rounded-lg bg-teal-50 flex items-center justify-center">
                <CalendarDays className="w-4 h-4 text-teal-600" />
              </div>

              <div>
                <p className="text-lg font-bold text-slate-900">
                  {item.expiringSoonDays ?? 14} days
                </p>

                <p className="text-[10px] text-slate-500">Expiry warning</p>
              </div>
            </div>

            <p className="text-[10px] text-slate-400 mt-4">Server-managed</p>
          </div>

          <div className="bg-[#E6F7EF] rounded-2xl border border-[#168A58] p-4 shadow-2xs">
            <div className="flex items-center gap-3">
              <div className="w-9 h-9 rounded-lg bg-emerald-50 flex items-center justify-center">
                <Layers className="w-4 h-4 text-emerald-600" />
              </div>

              <div>
                <p className="text-lg font-bold text-slate-900">
                  {lots.length} lots
                </p>

                <p className="text-[10px] text-slate-500">Associated lots</p>
              </div>
            </div>

            <p className="text-[10px] text-slate-400 mt-4">
              Available for FEFO
            </p>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-[minmax(0,1fr)_280px] gap-5">
          <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
            <div className="px-5 py-4 flex items-center justify-between">
              <div>
                <h3 className="text-sm font-bold text-slate-800">
                  Lot Details
                </h3>

                <p className="text-[10px] text-slate-400 mt-1">
                  Sorted by FEFO • lot data for this item only
                </p>
              </div>

              <button
                type="button"
                onClick={() => navigate(`/items/${item.id}/lots/new`)}
                className="flex items-center gap-1.5 px-3 py-2 rounded-lg border border-slate-200 text-[10px] font-semibold text-emerald-700 hover:bg-emerald-50 transition-colors"
              >
                <Plus className="w-3 h-3" />
                Add Lot
              </button>
            </div>

            {lots.length === 0 ? (
              <div className="py-16 text-center">
                <Layers className="w-8 h-8 mx-auto text-slate-300" />

                <p className="text-xs font-semibold text-slate-600 mt-3">
                  No lots found
                </p>

                <p className="text-[10px] text-slate-400 mt-1">
                  This item does not have any registered lots.
                </p>
              </div>
            ) : (
              <div className="px-5 pb-5 overflow-x-auto">
                <table className="w-full text-left">
                  <thead>
                    <tr className="bg-slate-50 text-[9px] font-bold uppercase tracking-wider text-slate-400">
                      <th className="px-3 py-3 rounded-l-lg">Lot Number</th>
                      <th className="px-3 py-3">Remains</th>
                      <th className="px-3 py-3">Expiry</th>
                      <th className="px-3 py-3">Status</th>
                    </tr>
                  </thead>

                  <tbody className="divide-y divide-slate-100">
                    {lots.map((lot) => (
                      <tr
                        key={lot.lotId}
                        className="hover:bg-slate-50/70 transition-colors"
                      >
                        <td className="px-3 py-4">
                          <p className="text-[11px] font-bold text-slate-800">
                            {lot.lotNumber}
                          </p>
                        </td>

                        <td className="px-3 py-4">
                          <span className="text-[11px] font-semibold text-slate-800">
                            {lot.remainingQuantity} {item.baseUnit}
                          </span>
                        </td>

                        <td className="px-3 py-4">
                          <span className="text-[11px] text-slate-600">
                            {formatDate(lot.expiryDate)}
                          </span>
                        </td>

                        <td className="px-3 py-4">
                          <span
                            className={`inline-flex px-2.5 py-1 rounded-full border text-[9px] font-semibold ${getStockStatusClass(
                              lot.status,
                            )}`}
                          >
                            {lot.status}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}

            <div className="px-5 pb-4">
              <p className="text-[9px] text-slate-400">
                Expired, depleted, blocked, and quarantined items cannot be
                allocated.
              </p>
            </div>
          </div>

          <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
            <div className="px-5 py-4">
              <h3 className="text-sm font-bold text-slate-800">
                Item Configuration
              </h3>

              <p className="text-[10px] text-slate-400 mt-1">
                Item Master Details
              </p>
            </div>

            <div className="divide-y divide-slate-100">
              <div className="px-5 py-4">
                <p className="text-[9px] uppercase font-bold text-slate-400">
                  Base Unit
                </p>

                <p className="text-[11px] font-semibold text-slate-800 mt-1">
                  {item.baseUnit}
                </p>
              </div>

              <div className="px-5 py-4">
                <p className="text-[9px] uppercase font-bold text-slate-400">
                  Minimum Stock
                </p>

                <p className="text-[11px] font-semibold text-slate-800 mt-1">
                  {item.minimumStock} {item.baseUnit}
                </p>
              </div>

              <div className="px-5 py-4">
                <p className="text-[9px] uppercase font-bold text-slate-400">
                  Expiry Warning
                </p>

                <p className="text-[11px] font-semibold text-slate-800 mt-1">
                  {item.expiringSoonDays ?? 14} days
                </p>
              </div>

              <div className="px-5 py-4">
                <p className="text-[9px] uppercase font-bold text-slate-400">
                  Stock Status
                </p>

                <span
                  className={`inline-flex mt-1 px-2.5 py-1 rounded-full border text-[9px] font-semibold ${getStockStatusClass(
                    item.stockStatus,
                  )}`}
                >
                  {getStockStatusLabel(item.stockStatus)}
                </span>
              </div>
            </div>

            <div className="px-5 py-5 bg-slate-50/50">
              <p className="text-[10px] leading-relaxed text-slate-400">
                Usable stock is calculated by the server from eligible lots.
              </p>
            </div>
          </div>
        </div>

        <p className="text-[10px] text-slate-400">
          Data from API • fetch failures are not displayed as zero stock
        </p>

        <ItemModal
          isOpen={isEditModalOpen}
          onClose={() => setIsEditModalOpen(false)}
          item={item}
          onSubmit={handleUpdateItem}
          isLoading={isUpdating}
        />
      </div>
    </AppLayout>
  );
};