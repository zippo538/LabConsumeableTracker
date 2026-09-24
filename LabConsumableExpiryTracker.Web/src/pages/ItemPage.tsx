import React, { useState } from "react";
import { useAtomValue } from "jotai";
import { Search, Plus, AlertTriangle, PackageCheck, Info } from "lucide-react";

import type {
  ItemDto,
  CreateItemPayload,
  UpdateItemPayload,
} from "../types/item.types";

import { AppLayout } from "../components/layout/AppLayout";
import { useNavigate } from "react-router";
import { ItemModal } from "../components/Item/ItemModal";
import { userAtom } from "../lib/auth/authAtom";
import { useItem } from "../hooks/useItem";
import { ItemTable } from "../components/Item/ItemTable";
import { Pagination } from "../components/Pagination";
import { SummaryStatsCard } from "../components/SummaryStatsCard";

export const ItemListPage: React.FC = () => {
  const user = useAtomValue(userAtom);

  const isWarehouseAdmin = user?.roles?.includes("WarehouseAdmin") ?? false;

  const navigate = useNavigate();
  const {
    filteredItems,
    page,
    totalPages,
    setPage,
    isLoading,
    isError,
    stats,
    availableUnits,
    searchQuery,
    setSearchQuery,
    selectedUnit,
    setSelectedUnit,
    selectedStatus,
    setSelectedStatus,
    createItem,
    updateItem,
    deleteItem,
    isCreating,
    isUpdating,
    isDeleting,
  } = useItem();

  const [isModalOpen, setIsModalOpen] = useState(false);

  const [selectedItemForEdit, setSelectedItemForEdit] =
    useState<ItemDto | null>(null);

  const [itemToDelete, setItemToDelete] = useState<ItemDto | null>(null);

  const [deleteError, setDeleteError] = useState<string | null>(null);
  const [deleteSuccess, setDeleteSuccess] = useState<string | null>(null);
  const [createSuccess, setCreateSuccess] = useState<string | null>(null);
  const handleOpenCreate = () => {
    setSelectedItemForEdit(null);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (item: ItemDto) => {
    setSelectedItemForEdit(item);
    setIsModalOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!itemToDelete) return;

    setDeleteError(null);
    setDeleteSuccess(null);

    try {
      await deleteItem(itemToDelete.id);
      setDeleteSuccess("Item deleted successfully.");
      setItemToDelete(null);
    } catch (error: any) {
      const status = error?.response?.status;

      if (status === 409) {
        setDeleteError(
          error.response?.data?.message ??
            "This item cannot be deleted because it is already used by one or more lots.",
        );
      } else if (status === 404) {
        setDeleteError("Item not found.");
      } else {
        setDeleteError("An unexpected error occurred while deleting the item.");
      }
      setItemToDelete(null);
    }
  };

  const handleModalSubmit = async (
    payload: CreateItemPayload | UpdateItemPayload,
  ) => {
    setCreateSuccess(null);

    try {
      if ("id" in payload) {
        await updateItem(payload);
      } else {
        await createItem(payload);
        setCreateSuccess("Item added successfully.");
      }

      setIsModalOpen(false);
    } catch (error) {
      throw error;
    }
  };
  return (
    <AppLayout
      title="Items"
      description="Manage laboratory master items and lots"
    >
      <div className="space-y-6 pb-12">
        <div>
          <h2 className="text-2xl font-bold text-slate-900 tracking-tight">
            Master Item List
          </h2>

          <p className="text-xs text-slate-500 mt-1">
            Manage item codes, base units, minimum stock levels, and expiry
            warning thresholds.
          </p>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <SummaryStatsCard
            className="col-span-1 sm:col-span-2 lg:col-span-3 mt-0 grid-cols-3"
            stats={[
              {
                value: stats.totalActive,
                title: "Active items",
                color: "blue",
              },
              { value: stats.lowStock, title: "Low stock", color: "amber" },
              {
                value: stats.uniqueUnitsCount,
                title: "Unit types",
                color: "teal",
              },
            ]}
          />

          <div className="bg-[#F0FAFC] rounded-2xl border border-[#BDE5E8] p-4 shadow-2xs flex flex-col justify-center">
            <p className="text-xs font-bold text-sky-900 flex items-center gap-1.5">
              <Info className="w-3.5 h-3.5 text-sky-600 shrink-0" />
              <span>Item ≠ Lot</span>
            </p>

            <p className="text-[11px] text-slate-500 mt-1 leading-relaxed">
              Quantity is not managed at the item level.
              <br />
              <span className="font-medium text-sky-700">
                Stock is managed on the Lot page.
              </span>
            </p>
          </div>
        </div>

        <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-3">
          <div className="relative flex-1">
            <Search className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />

            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Search by item code or name..."
              className="w-full pl-9 pr-4 py-2.5 bg-white border border-slate-200 rounded-xl text-xs text-slate-800 placeholder:text-slate-400 focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/10 outline-hidden shadow-2xs transition-colors"
            />
          </div>

          <select
            value={selectedUnit}
            onChange={(e) => setSelectedUnit(e.target.value)}
            className="px-3.5 py-2.5 bg-white border border-slate-200 rounded-xl text-xs font-medium text-slate-700 outline-hidden shadow-2xs cursor-pointer focus:border-emerald-500"
          >
            <option value="ALL">All units</option>

            {availableUnits.map((unit) => (
              <option key={unit} value={unit}>
                {unit}
              </option>
            ))}
          </select>

          <select
            value={selectedStatus}
            onChange={(e) => setSelectedStatus(e.target.value)}
            className="px-3.5 py-2.5 bg-white border border-slate-200 rounded-xl text-xs font-medium text-slate-700 outline-hidden shadow-2xs cursor-pointer focus:border-emerald-500"
          >
            <option value="ALL">Active status</option>
            <option value="ACTIVE">Active only</option>
            <option value="LOW_STOCK">Low stock</option>
          </select>

          {isWarehouseAdmin && (
            <button
              type="button"
              onClick={handleOpenCreate}
              className="flex items-center justify-center gap-2 px-5 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-bold rounded-xl shadow-xs transition-colors shrink-0"
            >
              <Plus className="w-4 h-4" />
              <span>Add Master Item</span>
            </button>
          )}
        </div>
        {deleteError && (
          <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {deleteError}
          </div>
        )}

        {deleteSuccess && (
          <div className="rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
            {deleteSuccess}
          </div>
        )}
        {createSuccess && (
          <div className="rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
            {createSuccess}
          </div>
        )}

        <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
          {isLoading ? (
            <div className="py-20 text-center">
              <div className="inline-block animate-spin rounded-full h-8 w-8 border-4 border-slate-200 border-t-emerald-600" />

              <p className="mt-3 text-xs text-slate-500 font-medium">
                Loading master items...
              </p>
            </div>
          ) : isError ? (
            <div className="py-16 text-center text-rose-600 px-4">
              <AlertTriangle className="w-8 h-8 mx-auto mb-2 text-rose-500" />

              <p className="text-sm font-semibold">
                Failed to load master items
              </p>

              <p className="text-xs text-slate-500 mt-1">
                {"Please check your backend connection."}
              </p>
            </div>
          ) : filteredItems.length === 0 ? (
            <div className="py-20 text-center text-slate-400">
              <PackageCheck className="w-10 h-10 mx-auto mb-2 opacity-50" />

              <p className="text-sm font-medium text-slate-600">
                No matching items found
              </p>

              <p className="text-xs text-slate-400 mt-0.5">
                Try using a different search keyword or filter.
              </p>
            </div>
          ) : (
            <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
              <ItemTable
                items={filteredItems}
                isLoading={isLoading}
                isError={isError}
                canManage={isWarehouseAdmin}
                onRowClick={(item) => navigate(`/items/${item.id}`)}
                onEdit={handleOpenEdit}
                onDelete={setItemToDelete}
              />

              <Pagination
                page={page}
                totalPages={totalPages}
                onPageChange={setPage}
              />
            </div>
          )}
        </div>

        <p className="text-[11px] text-slate-400 pt-2">
          WarehouseAdmin can create, edit, and deactivate items. Scientist has
          read-only access.
        </p>

        <ItemModal
          isOpen={isModalOpen}
          onClose={() => setIsModalOpen(false)}
          onSubmit={handleModalSubmit}
          item={selectedItemForEdit}
          isLoading={isCreating || isUpdating}
        />

        {itemToDelete && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-xs">
            <div className="bg-white rounded-2xl shadow-xl w-full max-w-sm p-6 border border-slate-200">
              <h3 className="text-base font-bold text-slate-900">
                Delete Master Item?
              </h3>

              <p className="text-xs text-slate-500 mt-2">
                Are you sure you want to delete{" "}
                <span className="font-semibold text-slate-800">
                  {itemToDelete.name}
                </span>{" "}
                ({itemToDelete.code})? This action cannot be undone.
              </p>

              <div className="mt-6 flex items-center justify-end gap-3">
                <button
                  type="button"
                  onClick={() => setItemToDelete(null)}
                  disabled={isDeleting}
                  className="px-4 py-2 rounded-lg border border-slate-200 text-xs font-semibold text-slate-600 hover:bg-slate-50 transition-colors"
                >
                  Cancel
                </button>

                <button
                  type="button"
                  onClick={handleConfirmDelete}
                  disabled={isDeleting}
                  className="px-4 py-2 rounded-lg bg-rose-600 hover:bg-rose-700 text-white text-xs font-semibold shadow-xs transition-colors disabled:opacity-60"
                >
                  {isDeleting ? "Deleting..." : "Delete Item"}
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </AppLayout>
  );
};
