import React, { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useAtomValue } from 'jotai';
import {
  Search,
  Plus,
  MoreHorizontal,
  Layers,
  Edit2,
  Trash2,
  AlertTriangle,
  PackageCheck,
  Info,
} from 'lucide-react';
import { itemApi } from '../api/itemApi';
import type { ItemDto, CreateItemPayload, UpdateItemPayload } from '../types/item.types';
import { AppLayout } from '../components/layout/AppLayout';
import { ItemModal } from './../components/Item/ItemModal';
import { userAtom } from '../lib/auth/authAtom';

export const ItemListPage: React.FC = () => {
  const queryClient = useQueryClient();
  const user = useAtomValue(userAtom);

  const isWarehouseAdmin = user?.roles?.includes('WarehouseAdmin') ?? false;
  const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
  const IMAGE_BASE_URL = API_BASE_URL.replace(/\/api$/, '');
  // Filters
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedUnit, setSelectedUnit] = useState<string>('ALL');
  const [selectedStatus, setSelectedStatus] = useState<string>('ALL');

  // Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedItemForEdit, setSelectedItemForEdit] = useState<ItemDto | null>(null);
  const [itemToDelete, setItemToDelete] = useState<ItemDto | null>(null);

  // Active action dropdown
  const [openDropdownId, setOpenDropdownId] = useState<string | null>(null);

  // Fetch Items via TanStack Query
  const {
    data: items = [],
    isLoading,
    isError,
    error,
  } = useQuery<ItemDto[]>({
    queryKey: ['items'],
    queryFn: () => itemApi.getAll(),
  });

  // Create Mutation
  const createMutation = useMutation({
    mutationFn: (payload: CreateItemPayload) => itemApi.create(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['items'] });
      setIsModalOpen(false);
    },
  });

  // Update Mutation
  const updateMutation = useMutation({
    mutationFn: (payload: UpdateItemPayload) => itemApi.update(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['items'] });
      setIsModalOpen(false);
    },
  });

  // Delete Mutation
  const deleteMutation = useMutation({
    mutationFn: (id: string) => itemApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['items'] });
      setItemToDelete(null);
    },
  });

  // KPI Calculations
  const stats = useMemo(() => {
    const totalItems = items.length;
    const lowStockCount = items.filter((item) => item.isLowStock).length;
    const uniqueUnits = new Set(items.map((item) => item.baseUnit)).size;

    return {
      totalActive: totalItems,
      lowStock: lowStockCount,
      uniqueUnitsCount: uniqueUnits || 4,
    };
  }, [items]);

  // Unit filter options derived from items or standard units
  const availableUnits = useMemo(() => {
    const units = Array.from(new Set(items.map((i) => i.baseUnit).filter(Boolean)));
    return units.length ? units : ['Milliliter', 'Gram', 'Unit', 'Vial', 'Box'];
  }, [items]);

  // Filtered Items
  const filteredItems = useMemo(() => {
    return items.filter((item) => {
      const matchQuery =
        item.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
        item.code.toLowerCase().includes(searchQuery.toLowerCase());

      const matchUnit =
        selectedUnit === 'ALL' ||
        item.baseUnit.toLowerCase() === selectedUnit.toLowerCase();

      const matchStatus =
        selectedStatus === 'ALL' ||
        (selectedStatus === 'ACTIVE' && true) ||
        (selectedStatus === 'LOW_STOCK' && item.isLowStock);

      return matchQuery && matchUnit && matchStatus;
    });
  }, [items, searchQuery, selectedUnit, selectedStatus]);

  // Handlers
  const handleOpenCreate = () => {
    setSelectedItemForEdit(null);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (item: ItemDto) => {
    setSelectedItemForEdit(item);
    setIsModalOpen(true);
    setOpenDropdownId(null);
  };

  const handleConfirmDelete = async () => {
    if (itemToDelete) {
      await deleteMutation.mutateAsync(itemToDelete.id);
    }
  };

  const handleModalSubmit = async (payload: CreateItemPayload | UpdateItemPayload) => {
    if ('id' in payload) {
      await updateMutation.mutateAsync(payload);
    } else {
      await createMutation.mutateAsync(payload);
    }
  };

  return (
      <AppLayout
          title="Item Master"
          description="Manage Laboratory Master Items"
      >
        <div
            className="space-y-6 pb-12"
            onClick={() => setOpenDropdownId(null)}
        >
          {/* Page Title & Subtitle */}
          <div>
            <h2 className="text-2xl font-bold text-slate-900 tracking-tight">
              Daftar Master Item
            </h2>
            <p className="text-xs text-slate-500 mt-1">
              Kelola kode, unit dasar, batas stok minimum, dan ambang peringatan expiry.
            </p>
          </div>

          {/* Top Metric Cards */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {/* Card 1 */}
            <div className="bg-white rounded-2xl border border-slate-200/80 p-5 shadow-2xs">
              <div className="flex items-center gap-2 mb-2">
                <span className="w-2.5 h-2.5 rounded-xs bg-blue-600" />
                <span className="text-2xl font-bold text-slate-900">
              {stats.totalActive}
            </span>
              </div>
              <p className="text-xs font-medium text-slate-500">
                Item aktif
              </p>
            </div>

            {/* Card 2 */}
            <div className="bg-white rounded-2xl border border-slate-200/80 p-5 shadow-2xs">
              <div className="flex items-center gap-2 mb-2">
                <span className="w-2.5 h-2.5 rounded-xs bg-amber-500" />
                <span className="text-2xl font-bold text-slate-900">
              {stats.lowStock}
            </span>
              </div>
              <p className="text-xs font-medium text-slate-500">
                Low stock
              </p>
            </div>

            {/* Card 3 */}
            <div className="bg-white rounded-2xl border border-slate-200/80 p-5 shadow-2xs">
              <div className="flex items-center gap-2 mb-2">
                <span className="w-2.5 h-2.5 rounded-xs bg-teal-500" />
                <span className="text-2xl font-bold text-slate-900">
              {stats.uniqueUnitsCount}
            </span>
              </div>
              <p className="text-xs font-medium text-slate-500">
                Jenis unit
              </p>
            </div>

            {/* Card 4 */}
            <div className="bg-sky-50/50 rounded-2xl border border-sky-100 p-4 shadow-2xs flex flex-col justify-center">
              <p className="text-xs font-bold text-sky-900 flex items-center gap-1.5">
                <Info className="w-3.5 h-3.5 text-sky-600 shrink-0" />
                <span>Item ≠ Lot</span>
              </p>

              <p className="text-[11px] text-slate-500 mt-1 leading-relaxed">
                Tidak ada quantity per Lot di halaman ini.
                <br />
                <span className="font-medium text-sky-700">
              Stok dikelola pada halaman Lot.
            </span>
              </p>
            </div>
          </div>

          {/* Toolbar */}
          <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-3">
            {/* Search */}
            <div className="relative flex-1">
              <Search className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />

              <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Cari kode atau nama item..."
                  className="w-full pl-9 pr-4 py-2.5 bg-white border border-slate-200 rounded-xl text-xs text-slate-800 placeholder:text-slate-400 focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/10 outline-hidden shadow-2xs transition-colors"
              />
            </div>

            {/* Unit Filter */}
            <select
                value={selectedUnit}
                onChange={(e) => setSelectedUnit(e.target.value)}
                className="px-3.5 py-2.5 bg-white border border-slate-200 rounded-xl text-xs font-medium text-slate-700 outline-hidden shadow-2xs cursor-pointer focus:border-emerald-500"
            >
              <option value="ALL">Semua unit</option>

              {availableUnits.map((unit) => (
                  <option key={unit} value={unit}>
                    {unit}
                  </option>
              ))}
            </select>

            {/* Status Filter */}
            <select
                value={selectedStatus}
                onChange={(e) => setSelectedStatus(e.target.value)}
                className="px-3.5 py-2.5 bg-white border border-slate-200 rounded-xl text-xs font-medium text-slate-700 outline-hidden shadow-2xs cursor-pointer focus:border-emerald-500"
            >
              <option value="ALL">Status aktif</option>
              <option value="ACTIVE">Hanya Aktif</option>
              <option value="LOW_STOCK">Stok Rendah</option>
            </select>

            {/* Add Button */}
            {isWarehouseAdmin && (
                <button
                    onClick={handleOpenCreate}
                    className="flex items-center justify-center gap-2 px-5 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-bold rounded-xl shadow-xs transition-colors shrink-0"
                >
                  <Plus className="w-4 h-4" />
                  <span>+ Tambah Master Item</span>
                </button>
            )}
          </div>

          {/* Main Table */}
          <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
            {isLoading ? (
                <div className="py-20 text-center">
                  <div className="inline-block animate-spin rounded-full h-8 w-8 border-4 border-slate-200 border-t-emerald-600" />

                  <p className="mt-3 text-xs text-slate-500 font-medium">
                    Memuat data master item...
                  </p>
                </div>
            ) : isError ? (
                <div className="py-16 text-center text-rose-600 px-4">
                  <AlertTriangle className="w-8 h-8 mx-auto mb-2 text-rose-500" />

                  <p className="text-sm font-semibold">
                    Gagal memuat data master item
                  </p>

                  <p className="text-xs text-slate-500 mt-1">
                    {(error as Error)?.message ||
                        'Periksa koneksi backend Anda.'}
                  </p>
                </div>
            ) : filteredItems.length === 0 ? (
                <div className="py-20 text-center text-slate-400">
                  <PackageCheck className="w-10 h-10 mx-auto mb-2 opacity-50" />

                  <p className="text-sm font-medium text-slate-600">
                    Belum ada item yang sesuai
                  </p>

                  <p className="text-xs text-slate-400 mt-0.5">
                    Coba gunakan kata kunci pencarian atau filter yang lain.
                  </p>
                </div>
            ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-left border-collapse">
                    <thead>
                    <tr className="border-b border-slate-100 text-[11px] font-bold uppercase tracking-wider text-slate-400 bg-slate-50/50">
                      <th className="py-3.5 pl-6 pr-4">
                        Kode / Nama
                      </th>

                      <th className="py-3.5 px-4">
                        Unit Dasar
                      </th>

                      <th className="py-3.5 px-4">
                        Min. Stok
                      </th>

                      <th className="py-3.5 px-4">
                        Warning Expiry
                      </th>

                      <th className="py-3.5 px-4">
                        Status
                      </th>

                      <th className="py-3.5 pr-6 pl-4 text-right">
                        Aksi
                      </th>
                    </tr>
                    </thead>

                    <tbody className="divide-y divide-slate-100 text-xs">
                    {filteredItems.map((item) => (
                        <tr
                            key={item.id}
                            className="hover:bg-slate-50/80 transition-colors group"
                        >
                          {/* Item */}
                          <td className="py-4 pl-6 pr-4">
                            <div className="flex items-center gap-3">
                              <div className="w-14 h-14 rounded-lg bg-teal-50 border border-teal-100 flex items-center justify-center text-teal-700 shrink-0 overflow-hidden">
                                {item.imagePath ? (
                                    <img
                                        src={`${IMAGE_BASE_URL}/${item.imagePath}`}
                                        alt={item.name}
                                        className="w-full h-full object-cover"
                                    />
                                ) : (
                                    <Layers className="w-4 h-4 opacity-75" />
                                )}
                              </div>

                              <div>
                                <p className="font-bold text-slate-900 leading-snug">
                                  {item.code}
                                </p>

                                <p className="text-[11px] text-slate-400 leading-tight">
                                  {item.name}
                                </p>
                              </div>
                            </div>
                          </td>

                          {/* Unit */}
                          <td className="py-4 px-4 font-medium text-slate-700">
                            {item.baseUnit}
                          </td>

                          {/* Min Stock */}
                          <td className="py-4 px-4 font-semibold text-slate-800">
                            {item.minimumStock}
                          </td>

                          {/* Expiry */}
                          <td className="py-4 px-4 text-slate-600 font-medium">
                            {item.expiringSoonDays
                                ? `${item.expiringSoonDays} hari`
                                : '14 hari'}
                          </td>

                          {/* Status */}
                          <td className="py-4 px-4">
                      <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[11px] font-semibold bg-emerald-50 text-emerald-700 border border-emerald-100">
                        <span className="w-1.5 h-1.5 rounded-full bg-emerald-500" />
                        Aktif
                      </span>
                          </td>

                          {/* Actions */}
                          <td className="py-4 pr-6 pl-4 text-right relative">
                            <div
                                className="inline-block"
                                onClick={(e) => e.stopPropagation()}
                            >
                              <button
                                  onClick={() =>
                                      setOpenDropdownId(
                                          openDropdownId === item.id
                                              ? null
                                              : item.id,
                                      )
                                  }
                                  className="p-1.5 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 transition-colors"
                              >
                                <MoreHorizontal className="w-4 h-4" />
                              </button>

                              {openDropdownId === item.id && (
                                  <div className="absolute right-6 top-12 z-20 w-36 bg-white rounded-xl shadow-lg border border-slate-100 py-1.5 text-left animate-in fade-in zoom-in-95 duration-100">
                                    {isWarehouseAdmin ? (
                                        <>
                                          <button
                                              onClick={() => handleOpenEdit(item)}
                                              className="w-full flex items-center gap-2 px-3.5 py-2 text-xs font-medium text-slate-700 hover:bg-slate-50 hover:text-emerald-700 transition-colors"
                                          >
                                            <Edit2 className="w-3.5 h-3.5" />
                                            <span>Ubah</span>
                                          </button>

                                          <button
                                              onClick={() => {
                                                setItemToDelete(item);
                                                setOpenDropdownId(null);
                                              }}
                                              className="w-full flex items-center gap-2 px-3.5 py-2 text-xs font-medium text-rose-600 hover:bg-rose-50 transition-colors"
                                          >
                                            <Trash2 className="w-3.5 h-3.5" />
                                            <span>Hapus</span>
                                          </button>
                                        </>
                                    ) : (
                                        <div className="px-3.5 py-2 text-[11px] text-slate-400">
                                          Hanya lihat
                                        </div>
                                    )}
                                  </div>
                              )}
                            </div>
                          </td>
                        </tr>
                    ))}
                    </tbody>
                  </table>
                </div>
            )}
          </div>

          {/* Footer */}
          <p className="text-[11px] text-slate-400 pt-2">
            WarehouseAdmin dapat membuat, mengubah, dan menonaktifkan Item.
            Scientist hanya melihat data.
          </p>

          {/* Add / Edit Modal */}
          <ItemModal
              isOpen={isModalOpen}
              onClose={() => setIsModalOpen(false)}
              onSubmit={handleModalSubmit}
              item={selectedItemForEdit}
              isLoading={
                  createMutation.isPending ||
                  updateMutation.isPending
              }
          />

          {/* Delete Confirmation */}
          {itemToDelete && (
              <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-xs">
                <div className="bg-white rounded-2xl shadow-xl w-full max-w-sm p-6 border border-slate-200">
                  <h3 className="text-base font-bold text-slate-900">
                    Hapus Master Item?
                  </h3>

                  <p className="text-xs text-slate-500 mt-2">
                    Apakah Anda yakin ingin menghapus item{' '}
                    <span className="font-semibold text-slate-800">
                {itemToDelete.name}
              </span>{' '}
                    ({itemToDelete.code})? Tindakan ini tidak dapat
                    dibatalkan.
                  </p>

                  <div className="mt-6 flex items-center justify-end gap-3">
                    <button
                        type="button"
                        onClick={() => setItemToDelete(null)}
                        disabled={deleteMutation.isPending}
                        className="px-4 py-2 rounded-lg border border-slate-200 text-xs font-semibold text-slate-600 hover:bg-slate-50 transition-colors"
                    >
                      Batal
                    </button>

                    <button
                        type="button"
                        onClick={handleConfirmDelete}
                        disabled={deleteMutation.isPending}
                        className="px-4 py-2 rounded-lg bg-rose-600 hover:bg-rose-700 text-white text-xs font-semibold shadow-xs transition-colors disabled:opacity-60"
                    >
                      {deleteMutation.isPending
                          ? 'Menghapus...'
                          : 'Hapus Item'}
                    </button>
                  </div>
                </div>
              </div>
          )}
        </div>
      </AppLayout>
  );
};
