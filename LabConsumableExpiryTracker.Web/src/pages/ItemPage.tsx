import React, {useState} from 'react';
import {useAtomValue} from 'jotai';
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

import type {
    ItemDto,
    CreateItemPayload,
    UpdateItemPayload,
} from '../types/item.types';

import {AppLayout} from '../components/layout/AppLayout';
import {useNavigate} from 'react-router';
import {ItemModal} from '../components/Item/ItemModal';
import {userAtom} from '../lib/auth/authAtom';
import {useItem} from '../hooks/useItem';

export const ItemListPage: React.FC = () => {
    const user = useAtomValue(userAtom);

    const isWarehouseAdmin =
        user?.roles?.includes('WarehouseAdmin') ?? false;

    const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
    const IMAGE_BASE_URL = API_BASE_URL.replace(/\/api$/, '');
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

    const [itemToDelete, setItemToDelete] =
        useState<ItemDto | null>(null);

    const [openDropdownId, setOpenDropdownId] =
        useState<string | null>(null);

    const [deleteError, setDeleteError] = useState<string | null>(null);
    const [deleteSuccess, setDeleteSuccess] = useState<string | null>(null);

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
        if (!itemToDelete) return;

        setDeleteError(null);
        setDeleteSuccess(null);

        try {
            await deleteItem(itemToDelete.id);
            setDeleteSuccess('Item deleted successfully.');
            setItemToDelete(null);
        } catch (error: any) {
            const status = error?.response?.status;

            if (status === 409) {
                setDeleteError(
                    error.response?.data?.message ??
                    'This item cannot be deleted because it is already used by one or more lots.',
                );
            } else if (status === 404) {
                setDeleteError('Item not found.');
            } else {
                setDeleteError(
                    'An unexpected error occurred while deleting the item.',
                );
            }
            setItemToDelete(null)
        }

    };

    const handleModalSubmit = async (
        payload: CreateItemPayload | UpdateItemPayload,
    ) => {
        if ('id' in payload) {
            await updateItem(payload);
        } else {
            await createItem(payload);
        }

        setIsModalOpen(false);
    };

    return (
        <AppLayout
            title="Items & Lots"
            description="Manage laboratory master items and lots"
        >
            <div
                className="space-y-6 pb-12"
                onClick={() => setOpenDropdownId(null)}
            >

                <div>
                    <h2 className="text-2xl font-bold text-slate-900 tracking-tight">
                        Master Item List
                    </h2>

                    <p className="text-xs text-slate-500 mt-1">
                        Manage item codes, base units, minimum stock levels,
                        and expiry warning thresholds.
                    </p>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                
                    <div className="bg-white rounded-2xl border border-slate-200/80 p-5 shadow-2xs">
                        <div className="flex items-center gap-2 mb-2">
                            <span className="w-2.5 h-2.5 rounded-xs bg-blue-600"/>

                            <span className="text-2xl font-bold text-slate-900">
                {stats.totalActive}
              </span>
                        </div>

                        <p className="text-xs font-medium text-slate-500">
                            Active items
                        </p>
                    </div>

                    <div className="bg-white rounded-2xl border border-slate-200/80 p-5 shadow-2xs">
                        <div className="flex items-center gap-2 mb-2">
                            <span className="w-2.5 h-2.5 rounded-xs bg-amber-500"/>

                            <span className="text-2xl font-bold text-slate-900">
                {stats.lowStock}
              </span>
                        </div>

                        <p className="text-xs font-medium text-slate-500">
                            Low stock
                        </p>
                    </div>

                    <div className="bg-white rounded-2xl border border-slate-200/80 p-5 shadow-2xs">
                        <div className="flex items-center gap-2 mb-2">
                            <span className="w-2.5 h-2.5 rounded-xs bg-teal-500"/>

                            <span className="text-2xl font-bold text-slate-900">
                {stats.uniqueUnitsCount}
              </span>
                        </div>

                        <p className="text-xs font-medium text-slate-500">
                            Unit types
                        </p>
                    </div>

                    <div
                        className="bg-[#F0FAFC] rounded-2xl border border-[#BDE5E8] p-4 shadow-2xs flex flex-col justify-center">
                        <p className="text-xs font-bold text-sky-900 flex items-center gap-1.5">
                            <Info className="w-3.5 h-3.5 text-sky-600 shrink-0"/>
                            <span>Item ≠ Lot</span>
                        </p>

                        <p className="text-[11px] text-slate-500 mt-1 leading-relaxed">
                            Quantity is not managed at the item level.
                            <br/>

                            <span className="font-medium text-sky-700">
                Stock is managed on the Lot page.
              </span>
                        </p>
                    </div>
                </div>

                <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-3">
              
                    <div className="relative flex-1">
                        <Search className="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2"/>

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
                            <Plus className="w-4 h-4"/>
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

                <div className="bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
                    {isLoading ? (
                        <div className="py-20 text-center">
                            <div
                                className="inline-block animate-spin rounded-full h-8 w-8 border-4 border-slate-200 border-t-emerald-600"/>

                            <p className="mt-3 text-xs text-slate-500 font-medium">
                                Loading master items...
                            </p>
                        </div>
                    ) : isError ? (
                        <div className="py-16 text-center text-rose-600 px-4">
                            <AlertTriangle className="w-8 h-8 mx-auto mb-2 text-rose-500"/>

                            <p className="text-sm font-semibold">
                                Failed to load master items
                            </p>

                            <p className="text-xs text-slate-500 mt-1">
                                {
                                    'Please check your backend connection.'}
                            </p>
                        </div>
                    ) : filteredItems.length === 0 ? (
                        <div className="py-20 text-center text-slate-400">
                            <PackageCheck className="w-10 h-10 mx-auto mb-2 opacity-50"/>

                            <p className="text-sm font-medium text-slate-600">
                                No matching items found
                            </p>

                            <p className="text-xs text-slate-400 mt-0.5">
                                Try using a different search keyword or filter.
                            </p>
                        </div>
                    ) : (
                        <div className="overflow-x-auto">
                            <table className="w-full text-left border-collapse">
                                <thead>
                                <tr className="border-b border-slate-100 text-[11px] font-bold uppercase tracking-wider text-slate-400 bg-slate-50/50">
                                    <th className="py-3.5 pl-6 pr-4">
                                        Code / Name
                                    </th>

                                    <th className="py-3.5 px-4">
                                        Base Unit
                                    </th>

                                    <th className="py-3.5 px-4">
                                        Min. Stock
                                    </th>

                                    <th className="py-3.5 px-4">
                                        Expiry Warning
                                    </th>

                                    <th className="py-3.5 px-4">
                                        Status
                                    </th>

                                    <th className="py-3.5 pr-6 pl-4 text-right">
                                        Actions
                                    </th>
                                </tr>
                                </thead>

                                <tbody className="divide-y divide-slate-100 text-xs">
                                {filteredItems.map((item) => (
                                    <tr
                                        key={item.id}
                                        onClick={() => navigate(`/items/${item.id}`)}
                                        className="hover:bg-slate-50/80 transition-colors group cursor-pointer"
                                    >
                                        <td className="py-4 pl-6 pr-4">
                                            <div className="flex items-center gap-3">
                                                <div
                                                    className="w-14 h-14 rounded-lg bg-teal-50 border border-teal-100 flex items-center justify-center text-teal-700 shrink-0 overflow-hidden">
                                                    {item.imagePath ? (
                                                        <img
                                                            src={`${IMAGE_BASE_URL}/${item.imagePath}`}
                                                            alt={item.name}
                                                            className="w-full h-full object-cover"
                                                        />
                                                    ) : (
                                                        <Layers className="w-4 h-4 opacity-75"/>
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

                                        <td className="py-4 px-4 font-medium text-slate-700">
                                            {item.baseUnit}
                                        </td>

                                        <td className="py-4 px-4 font-semibold text-slate-800">
                                            {item.minimumStock}
                                        </td>

                                        <td className="py-4 px-4 text-slate-600 font-medium">
                                            {item.expiringSoonDays
                                                ? `${item.expiringSoonDays} days`
                                                : '14 days'}
                                        </td>

                                        {/* Status */}
                                        <td className="py-4 px-4">
                        <span
                            className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[11px] font-semibold bg-emerald-50 text-emerald-700 border border-emerald-100">
                          <span className="w-1.5 h-1.5 rounded-full bg-emerald-500"/>
                          Active
                        </span>
                                        </td>

                                        <td className="py-4 pr-6 pl-4 text-right relative">
                                            <div
                                                className="inline-block"
                                                onClick={(e) => e.stopPropagation()}
                                            >
                                                <button
                                                    type="button"
                                                    onClick={() =>
                                                        setOpenDropdownId(
                                                            openDropdownId === item.id
                                                                ? null
                                                                : item.id,
                                                        )
                                                    }
                                                    className="p-1.5 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 transition-colors"
                                                >
                                                    <MoreHorizontal className="w-4 h-4"/>
                                                </button>

                                                {openDropdownId === item.id && (
                                                    <div
                                                        className="absolute right-6 top-12 z-20 w-36 bg-white rounded-xl shadow-lg border border-slate-100 py-1.5 text-left animate-in fade-in zoom-in-95 duration-100">
                                                        {isWarehouseAdmin ? (
                                                            <>
                                                                <button
                                                                    type="button"
                                                                    onClick={() =>
                                                                        handleOpenEdit(item)
                                                                    }
                                                                    className="w-full flex items-center gap-2 px-3.5 py-2 text-xs font-medium text-slate-700 hover:bg-slate-50 hover:text-emerald-700 transition-colors"
                                                                >
                                                                    <Edit2 className="w-3.5 h-3.5"/>
                                                                    <span>Edit</span>
                                                                </button>

                                                                <button
                                                                    type="button"
                                                                    onClick={() => {
                                                                        setItemToDelete(item);
                                                                        setOpenDropdownId(null);
                                                                    }}
                                                                    className="w-full flex items-center gap-2 px-3.5 py-2 text-xs font-medium text-rose-600 hover:bg-rose-50 transition-colors"
                                                                >
                                                                    <Trash2 className="w-3.5 h-3.5"/>
                                                                    <span>Delete</span>
                                                                </button>
                                                            </>
                                                        ) : (
                                                            <div className="px-3.5 py-2 text-[11px] text-slate-400">
                                                                View only
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

                    {totalPages > 1 && (
                        <div
                            className="flex items-center justify-between px-5 py-4 border-t border-slate-100 bg-slate-50/30">
                            <p className="text-[11px] text-slate-500">
                                Page <span className="font-semibold text-slate-700">{page}</span> of{' '}
                                <span className="font-semibold text-slate-700">{totalPages}</span>
                            </p>

                            <div className="flex items-center gap-1">
                                <button
                                    type="button"
                                    disabled={page <= 1}
                                    onClick={() => setPage((current) => current - 1)}
                                    className="px-3 py-2 rounded-lg border border-slate-200 bg-white text-[11px] font-semibold text-slate-600 hover:bg-slate-50 hover:text-slate-900 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
                                >
                                    Previous
                                </button>

                                <div className="flex items-center gap-1 mx-1">
                                    {Array.from({length: totalPages}, (_, index) => index + 1).map(
                                        (pageNumber) => (
                                            <button
                                                key={pageNumber}
                                                type="button"
                                                onClick={() => setPage(pageNumber)}
                                                className={`w-8 h-8 rounded-lg text-[11px] font-semibold transition-colors ${page === pageNumber
                                                    ? 'bg-emerald-600 text-white shadow-sm'
                                                    : 'text-slate-600 hover:bg-slate-100'
                                                }`}
                                            >
                                                {pageNumber}
                                            </button>
                                        ),
                                    )}
                                </div>

                                <button
                                    type="button"
                                    disabled={page >= totalPages}
                                    onClick={() => setPage((current) => current + 1)}
                                    className="px-3 py-2 rounded-lg border border-slate-200 bg-white text-[11px] font-semibold text-slate-600 hover:bg-slate-50 hover:text-slate-900 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
                                >
                                    Next
                                </button>
                            </div>
                        </div>
                    )}
                </div>


                <p className="text-[11px] text-slate-400 pt-2">
                    WarehouseAdmin can create, edit, and deactivate items.
                    Scientist has read-only access.
                </p>

                <ItemModal
                    isOpen={isModalOpen}
                    onClose={() => setIsModalOpen(false)}
                    onSubmit={handleModalSubmit}
                    item={selectedItemForEdit}
                    isLoading={isCreating || isUpdating}
                />

                {itemToDelete && (
                    <div
                        className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-xs">
                        <div className="bg-white rounded-2xl shadow-xl w-full max-w-sm p-6 border border-slate-200">
                            <h3 className="text-base font-bold text-slate-900">
                                Delete Master Item?
                            </h3>

                            <p className="text-xs text-slate-500 mt-2">
                                Are you sure you want to delete{' '}
                                <span className="font-semibold text-slate-800">
                  {itemToDelete.name}
                </span>{' '}
                                ({itemToDelete.code})? This action cannot be
                                undone.
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
                                    {isDeleting ? 'Deleting...' : 'Delete Item'}
                                </button>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </AppLayout>
    );
};