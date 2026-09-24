// src/pages/LotPage.tsx
import { useState, useMemo } from "react";
import { useAtomValue } from "jotai";
import { useNavigate } from "react-router";
import { userAtom } from "../lib/auth/authAtom";
import { Grid2x2, MoreHorizontal, Edit2, Trash2 } from "lucide-react";
import {
  useLots,
  useLotSummary,
  useItems,
  useCreateLot,
  useUpdateLot,
  useDeleteLot,
} from "../hooks/useLot";
import type {
  Lot,
  CreateLotRequest,
  UpdateLotRequest,
} from "../types/lot.types";
import { DataTable, ItemBadge } from "../components/DataTable";
import { LotStatusBadge } from "../components/lots/LotStatusBadge";
import { LotSummaryCards } from "../components/lots/LotSummaryCard";
import { LotFilterBar } from "../components/lots/LotFilterbar";
import { CreateLotModal } from "../components/lots/CreateLotModal";
import { UpdateLotModal } from "../components/lots/UpdateLotModal";
import { DeleteLotModal } from "../components/lots/DeleteLotModal";
import { AppLayout } from "../components/layout/AppLayout";
import { EmptyState, ErrorState } from "../components/FeedBackStates";

export const LotsPage = () => {
  const user = useAtomValue(userAtom);
  const isWarehouseAdmin = user?.roles?.includes("WarehouseAdmin") ?? false;
  const navigate = useNavigate();
  const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
  const IMAGE_BASE_URL = API_BASE_URL.replace(/\/api$/, '');

  // State untuk filter
  const [search, setSearch] = useState("");
  const [filterItem, setFilterItem] = useState("");
  const [filterAdministrativeStatus, setFilterAdministrativeStatus] =
    useState("");
  const [filterExpiryCondition, setFilterExpiryCondition] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  // 2. State untuk Dropdown Menu
  const [openDropdownId, setOpenDropdownId] = useState<string | null>(null);

  const {
    data: lotsData,
    isLoading: isLotsLoading,
    isError: isLotsError,
    refetch: refetchLots,
  } = useLots({
    page,
    pageSize,
    search: search || undefined,
    itemId: filterItem || undefined,
    administrativeStatus: filterAdministrativeStatus || undefined,
    expiryCondition: filterExpiryCondition || undefined,
  });

  const { data: summary } = useLotSummary();
  const { data: items } = useItems();
  const createMutation = useCreateLot();
  const updateMutation = useUpdateLot();
  const deleteMutation = useDeleteLot();

  const lots = lotsData?.items ?? [];
  const totalItems = lotsData?.totalCount ?? 0;

  const itemImageMap = useMemo(() => {
    const map = new Map<string, string>();

    items?.forEach((item) => {
      if (item.imageId) {
        map.set(item.id, item.imageId);
      }
    });

    return map;
  }, [items]);
  
  const handleSearchChange = (value: string) => {
    setSearch(value);
    setPage(1);
  };
  const handleFilterItemChange = (value: string) => {
    setFilterItem(value);
    setPage(1);
  };
  const handleFilterAdministrativeStatusChange = (value: string) => {
    setFilterAdministrativeStatus(value);
    setPage(1);
  };
  const handleFilterExpiryConditionChange = (value: string) => {
    setFilterExpiryCondition(value);
    setPage(1);
  };

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
    setOpenDropdownId(null); // Tutup dropdown setelah klik
  };

  const handleDeleteLot = (lot: Lot) => {
    if (!isWarehouseAdmin) return;
    setSelectedLot(lot);
    setIsDeleteModalOpen(true);
    setOpenDropdownId(null); // Tutup dropdown setelah klik
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
      await updateMutation.mutateAsync({ id: selectedLot.id, payload });
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
    } catch (err) {
      /* Handle error */
    }
  };

  const columns = [
    {
      id: "item",
      header: "Code / Name",
      cell: (lot: Lot) => (
        <div className="flex items-center gap-3">
          <ItemBadge
            imageId={
              itemImageMap.get(lot.itemId) ?? null
            }
            imageAlt={`${lot.itemName} image`}
            fallbackText={lot.lotNumber.slice(0, 2)}
            tone="cyan"
            label={`View details for ${lot.itemName} - ${lot.lotNumber}`}
            onClick={() => navigate(`/app/lots/${lot.id}`)}
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
      id: "administrativeStatus",
      header: "Admin Status",
      cell: (lot: Lot) => <LotStatusBadge status={lot.administrativeStatus} />,
    },
    {
      id: "expiryCondition",
      header: "Expiry Condition",
      cell: (lot: Lot) => <LotStatusBadge status={lot.expiryCondition} />,
    },
    // 3. Kolom Actions dengan Dropdown Menu
    ...(isWarehouseAdmin
      ? [
          {
            id: "actions",
            header: "Actions",
            cell: (lot: Lot) => (
              <div className="relative text-right">
                <div
                  className="inline-block"
                  onClick={(e) => e.stopPropagation()}
                >
                  <button
                    type="button"
                    onClick={() =>
                      setOpenDropdownId(
                        openDropdownId === lot.id ? null : lot.id,
                      )
                    }
                    className="p-1.5 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 transition-colors"
                    aria-label="Open actions menu"
                  >
                    <MoreHorizontal className="w-4 h-4" />
                  </button>

                  {openDropdownId === lot.id && (
                    <div className="absolute right-0 top-10 z-20 w-36 bg-white rounded-xl shadow-lg border border-slate-100 py-1.5 text-left animate-in fade-in zoom-in-95 duration-100">
                      <button
                        type="button"
                        onClick={() => handleUpdateLot(lot)}
                        className="w-full flex items-center gap-2 px-3.5 py-2 text-xs font-medium text-slate-700 hover:bg-slate-50 hover:text-emerald-700 transition-colors"
                      >
                        <Edit2 className="w-3.5 h-3.5" />
                        <span>Edit</span>
                      </button>

                      <button
                        type="button"
                        onClick={() => handleDeleteLot(lot)}
                        className="w-full flex items-center gap-2 px-3.5 py-2 text-xs font-medium text-rose-600 hover:bg-rose-50 transition-colors"
                      >
                        <Trash2 className="w-3.5 h-3.5" />
                        <span>Delete</span>
                      </button>
                    </div>
                  )}
                </div>
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

        <LotFilterBar
          search={search}
          onSearchChange={handleSearchChange}
          filterItem={filterItem}
          onFilterItemChange={handleFilterItemChange}
          filterAdministrativeStatus={filterAdministrativeStatus}
          onFilterAdministrativeStatusChange={
            handleFilterAdministrativeStatusChange
          }
          filterExpiryCondition={filterExpiryCondition}
          onFilterExpiryConditionChange={handleFilterExpiryConditionChange}
          items={items ?? []}
          onRegisterNew={handleCreateLot}
          canRegister={isWarehouseAdmin}
        />

        {isLotsError ? (
          <ErrorState
            title="Failed to load Lot data"
            description="An error occurred while retrieving lot data. Please try again."
            onRetry={() => refetchLots()}
          />
        ) : (
          <DataTable
            columns={columns}
            rows={lots}
            getRowId={(lot) => lot.id}
            loading={isLotsLoading}
            emptyState={
              <EmptyState
                title={
                  search ||
                  filterItem ||
                  filterAdministrativeStatus ||
                  filterExpiryCondition
                    ? "No lots match the filter."
                    : "No lots have been listed yet."
                }
                description={
                  search ||
                  filterItem ||
                  filterAdministrativeStatus ||
                  filterExpiryCondition
                    ? "Try changing the search keywords or other filters."
                    : "Click 'Register New Lot' to add a new lot."
                }
                icon={<Grid2x2 />}
              />
            }
            density="comfortable"
            page={page}
            pageSize={pageSize}
            totalItems={totalItems}
            onPageChange={setPage}
            onPageSizeChange={setPageSize}
          />
        )}

        <p className="text-xs text-slate-400">
          Eligible: Active × balance &gt; 0 × not expired. Expiring Soon remains
          eligible.
        </p>

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
