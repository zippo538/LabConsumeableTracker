// src/pages/LotsPage.tsx
import { useState, useEffect } from "react";
import { useAuth } from "../features/auth/AuthContext";
import type { Lot, LotSummary, ItemOption } from "../types/lot.types";
import { LotSummaryCards } from "../components/lots/LotSummaryCard";
import { LotTable } from "../components/lots/LotTable";
import { LotFilterBar } from "../components/lots/LotFilterbar";
import { FefoInfoBox } from "../components/lots/FefoInfoBox";
import { CreateLotModal } from "../components/lots/CreateLotModal";
import { UpdateLotModal } from "../components/lots/UpdateLotModal";
import { DeleteLotModal } from "../components/lots/DeleteLotModal";

export const LotsPage = () => {
  const { session } = useAuth();
  const isWarehouseAdmin = session?.user.roles?.includes("WarehouseAdmin") ?? false;
  

  const [lots] = useState<Lot[]>([]);
  const [summary] = useState<LotSummary | null>(null);
  const [items] = useState<ItemOption[]>([]);
  const [search, setSearch] = useState("");
  const [filterItem, setFilterItem] = useState("");
  const [filterStatus, setFilterStatus] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  // Modal states
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isUpdateModalOpen, setIsUpdateModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [selectedLot, setSelectedLot] = useState<Lot | null>(null);

  useEffect(() => {
    // TODO: Fetch data from API
    // fetchLots();
    // fetchSummary();
    // fetchItems();
    setIsLoading(false);
  }, [search, filterItem, filterStatus]);

  const handleCreateLot = () => {
    if (!isWarehouseAdmin) return;
    setIsCreateModalOpen(true);
  };

  const handleUpdateLot = (lot: Lot) => {
    if (!isWarehouseAdmin) return;
    setSelectedLot(lot);
    setIsUpdateModalOpen(true);
  };

  const handleDeleteLot = (lot: Lot) => {
    if (!isWarehouseAdmin) return;
    setSelectedLot(lot);
    setIsDeleteModalOpen(true);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-slate-800">Lot List</h1>
        <p className="mt-1 text-sm text-slate-500">
          Each lot has its own expiry, balance, location, and administrative
          status.
        </p>
      </div>

      {/* Summary Cards */}
      {summary && <LotSummaryCards summary={summary} />}

      {/* FEFO Info Box */}
      <FefoInfoBox />

      {/* Filter Bar */}
      <LotFilterBar
        search={search}
        onSearchChange={setSearch}
        filterItem={filterItem}
        onFilterItemChange={setFilterItem}
        filterStatus={filterStatus}
        onFilterStatusChange={setFilterStatus}
        items={items}
        onRegisterNew={handleCreateLot}
        canRegister={isWarehouseAdmin}
      />

      {/* Lot Table */}
      <LotTable
        lots={lots}
        isLoading={isLoading}
        onUpdate={handleUpdateLot}
        onDelete={handleDeleteLot}
        canEdit={isWarehouseAdmin}
      />

      {/* Footer Info */}
      <p className="text-xs text-slate-400">
        Eligible: Active × balance &gt; 0 × not expired. Expiring Soon remains
        eligible.
      </p>

      {/* Modals */}
      <CreateLotModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        items={items}
      />

      <UpdateLotModal
        isOpen={isUpdateModalOpen}
        onClose={() => setIsUpdateModalOpen(false)}
        lot={selectedLot}
        items={items}
      />

      <DeleteLotModal
        isOpen={isDeleteModalOpen}
        onClose={() => setIsDeleteModalOpen(false)}
        lot={selectedLot}
      />
    </div>
  );
};
