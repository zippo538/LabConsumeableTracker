// src/pages/LotPage.tsx
import { useState } from "react";
import { useAtomValue } from "jotai";
import { userAtom } from "../lib/auth/authAtom";
import { Grid2x2 } from "lucide-react";
import {
  useLots,
  useLotSummary,
  useItems,
  useCreateLot,
  useUpdateLot,
  useDeleteLot,
} from "../hooks/useLots";
import type {
  Lot,
  CreateLotRequest,
  UpdateLotRequest,
} from "../types/lot.types";
import { DataTable, ItemBadge } from "../components/DataTable";
import { LotStatusBadge } from "../components/lots/LotStatusBadge";
import { LotSummaryCards } from "../components/lots/LotSummaryCard";
import { LotFilterBar } from "../components/lots/LotFilterbar";
import { FefoInfoBox } from "../components/lots/FefoInfoBox";
import { CreateLotModal } from "../components/lots/CreateLotModal";
import { UpdateLotModal } from "../components/lots/UpdateLotModal";
import { DeleteLotModal } from "../components/lots/DeleteLotModal";
import { AppLayout } from "../components/layout/AppLayout";

export const LotsPage = () => {
  const user = useAtomValue(userAtom);
  const isWarehouseAdmin = user?.roles?.includes("WarehouseAdmin") ?? false;

  const [search, setSearch] = useState("");
  const [filterItem, setFilterItem] = useState("");
  const [filterStatus, setFilterStatus] = useState("");

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const { data: lotsData, isLoading: isLotsLoading } = useLots({
    page,
    pageSize,
    search: search || undefined,
    itemId: filterItem || undefined,
    status: filterStatus || undefined,
  });

  const { data: summary } = useLotSummary();
  const { data: items } = useItems();

  const createMutation = useCreateLot();
  const updateMutation = useUpdateLot();
  const deleteMutation = useDeleteLot();

  const lots = lotsData?.items ?? [];
  const totalItems = lotsData?.totalCount ?? 0;

  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isUpdateModalOpen, setIsUpdateModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [selectedLot, setSelectedLot] = useState<Lot | null>(null);

  const [createError, setCreateError] = useState<string | undefined>();
  const [updateError, setUpdateError] = useState<string | undefined>();

  const handleCreateLot = () => {
    if (!isWarehouseAdmin) return;
    setCreateError(undefined);
    setIsCreateModalOpen(true);
  };

  const handleUpdateLot = (lot: Lot) => {
    if (!isWarehouseAdmin) return;
    setSelectedLot(lot);
    setUpdateError(undefined);
    setIsUpdateModalOpen(true);
  };

  const handleDeleteLot = (lot: Lot) => {
    if (!isWarehouseAdmin) return;
    setSelectedLot(lot);
    setIsDeleteModalOpen(true);
  };

  const handleCreateSubmit = async (payload: CreateLotRequest) => {
    try {
      await createMutation.mutateAsync(payload);
      setIsCreateModalOpen(false);
    } catch (err) {
      setCreateError(
        err instanceof Error ? err.message : "Failed to create lot.",
      );
    }
  };

  const handleUpdateSubmit = async (payload: UpdateLotRequest) => {
    if (!selectedLot) return;
    try {
      await updateMutation.mutateAsync({
        id: selectedLot.id,
        payload,
      });
      setIsUpdateModalOpen(false);
      setSelectedLot(null);
    } catch (err) {
      setUpdateError(
        err instanceof Error ? err.message : "Failed to update lot.",
      );
    }
  };

  const handleDeleteConfirm = async () => {
    if (!selectedLot) return;
    try {
      await deleteMutation.mutateAsync(selectedLot.id);
      setIsDeleteModalOpen(false);
      setSelectedLot(null);
    } catch (err) {}
  };

  const columns = [
    {
      id: "item",
      header: "KODE / NAMA",
      cell: (lot: Lot) => (
        <div className="flex items-center gap-3">
          <ItemBadge
            icon={Grid2x2}
            fallbackText={lot.lotNumber.slice(0, 2)}
            tone="cyan"
            label={`View details for ${lot.itemName} - ${lot.lotNumber}`}
            onClick={() => {
              console.log("Badge clicked:", lot.id);
            }}
          />
          <div className="grid min-w-0 gap-0.5">
            <span className="truncate font-semibold text-slate-900">
              {lot.lotNumber}
            </span>
            <span className="truncate text-xs text-slate-500">
              {lot.itemName ?? "Unknown"}
            </span>
          </div>
        </div>
      ),
      width: "30%",
    },
    {
      id: "quantity",
      header: "Remaining / Initial",
      cell: (lot: Lot) => (
        <p className="text-sm font-medium text-slate-800">
          {lot.remainingQuantity} / {lot.initialQuantity} {lot.baseUnit}
        </p>
      ),
    },
    {
      id: "expiry",
      header: "Expiry",
      cell: (lot: Lot) => (
        <p className="text-sm text-slate-800">
          {new Date(lot.expiryDate).toLocaleDateString("en-US", {
            day: "numeric",
            month: "short",
            year: "numeric",
          })}
        </p>
      ),
      sortable: true,
      sortValue: (lot: Lot) => new Date(lot.expiryDate),
    },
    {
      id: "location",
      header: "Location",
      cell: (lot: Lot) => (
        <p className="text-sm text-slate-800">{lot.storageLocation}</p>
      ),
    },
    {
      id: "status",
      header: "Status",
      cell: (lot: Lot) => <LotStatusBadge status={lot.expiryCondition} />,
    },
    ...(isWarehouseAdmin
      ? [
          {
            id: "actions",
            header: "Actions",
            cell: (lot: Lot) => (
              <div className="flex gap-2">
                <button
                  onClick={() => handleUpdateLot(lot)}
                  className="rounded px-2 py-1 text-xs font-medium text-teal-600 hover:bg-teal-50"
                >
                  Edit
                </button>
                <button
                  onClick={() => handleDeleteLot(lot)}
                  className="rounded px-2 py-1 text-xs font-medium text-red-600 hover:bg-red-50"
                >
                  Delete
                </button>
              </div>
            ),
          },
        ]
      : []),
  ];

  return (
    <AppLayout title="Lot Master" description="Manage Laboratory Master Lots">
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-bold text-slate-800">Lot List</h1>
          <p className="mt-1 text-sm text-slate-500">
            Each lot has its own expiry, balance, location, and administrative
            status.
          </p>
        </div>

        {summary && <LotSummaryCards summary={summary} />}

        <FefoInfoBox />

        <LotFilterBar
          search={search}
          onSearchChange={setSearch}
          filterItem={filterItem}
          onFilterItemChange={setFilterItem}
          filterStatus={filterStatus}
          onFilterStatusChange={setFilterStatus}
          items={items ?? []}
          onRegisterNew={handleCreateLot}
          canRegister={isWarehouseAdmin}
        />

        <DataTable
          columns={columns}
          rows={lots}
          getRowId={(lot) => lot.id}
          loading={isLotsLoading}
          emptyState={
            <div className="flex flex-col items-center justify-center py-8">
              <p className="text-sm font-medium text-slate-600">
                Belum ada lot yang sesuai
              </p>
              <p className="text-xs text-slate-400 mt-0.5">
                Coba gunakan kata kunci pencarian atau filter yang lain.
              </p>
            </div>
          }
          density="comfortable"
          page={page}
          pageSize={pageSize}
          totalItems={totalItems}
          onPageChange={setPage}
          onPageSizeChange={setPageSize}
        />

        <p className="text-xs text-slate-400">
          Eligible: Active × balance &gt; 0 × not expired. Expiring Soon remains
          eligible.
        </p>

        {/* Modals — menggunakan komponen reusable */}
        <CreateLotModal
          isOpen={isCreateModalOpen}
          onClose={() => {
            setIsCreateModalOpen(false);
            setCreateError(undefined);
          }}
          onSubmit={handleCreateSubmit}
          items={items ?? []}
          isLoading={createMutation.isPending}
          error={createError}
        />

        <UpdateLotModal
          isOpen={isUpdateModalOpen}
          onClose={() => {
            setIsUpdateModalOpen(false);
            setSelectedLot(null);
            setUpdateError(undefined);
          }}
          onSubmit={handleUpdateSubmit}
          lot={selectedLot}
          isLoading={updateMutation.isPending}
          error={updateError}
        />

        <DeleteLotModal
          isOpen={isDeleteModalOpen}
          onClose={() => {
            setIsDeleteModalOpen(false);
            setSelectedLot(null);
          }}
          onConfirm={handleDeleteConfirm}
          lot={selectedLot}
          isLoading={deleteMutation.isPending}
        />
      </div>
    </AppLayout>
  );
};
