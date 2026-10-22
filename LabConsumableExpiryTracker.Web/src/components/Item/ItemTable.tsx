import React, { useEffect, useState } from "react";
import {
  MoreHorizontal,
  Layers,
  Edit2,
  Trash2,
  AlertTriangle,
  PackageCheck,
} from "lucide-react";
import type { ItemDto } from "../../types/item.types";
import { useImage } from "../../hooks/useImage";

export interface ItemTableProps {
  items: ItemDto[];
  isLoading?: boolean;
  isError?: boolean;

  /** Jika false, dropdown hanya menampilkan "View only" */
  canManage?: boolean;
  /** Jika tidak diisi, kolom Actions disembunyikan */
  showActions?: boolean;

  onRowClick?: (item: ItemDto) => void;
  onEdit?: (item: ItemDto) => void;
  onDelete?: (item: ItemDto) => void;
}
const ItemRowImage: React.FC<{
  imageId: string | null | undefined;
  alt: string;
}> = ({ imageId, alt }) => {
  const { imageUrl, isLoading, error } = useImage(imageId);

  // Jika tidak ada ID, sedang loading, atau error, tampilkan ikon placeholder (Layers).
  // Ini sangat penting agar layout tabel tidak berantakan karena spinner atau teks error.
  if (!imageId || isLoading || error || !imageUrl) {
    return <Layers className="w-5 h-5 opacity-50 text-teal-600" />;
  }

  return (
    <img
      src={imageUrl}
      alt={alt}
      className="w-full h-full object-cover"
      onError={(event) => {
        event.currentTarget.style.display = "none";
      }}
    />
  );
};

export const ItemTable: React.FC<ItemTableProps> = ({
  items,
  isLoading = false,
  isError = false,
  canManage = false,
  showActions = true,
  onRowClick,
  onEdit,
  onDelete,
}) => {
  const [openDropdownId, setOpenDropdownId] = useState<string | null>(null);

  // Tutup dropdown saat klik di mana pun (menggantikan onClick di wrapper halaman)
  useEffect(() => {
    if (!openDropdownId) return;
    const close = () => setOpenDropdownId(null);
    document.addEventListener("click", close);
    return () => document.removeEventListener("click", close);
  }, [openDropdownId]);

  if (isLoading) {
    return (
      <div className="py-20 text-center">
        <div className="inline-block animate-spin rounded-full h-8 w-8 border-4 border-slate-200 border-t-emerald-600" />
        <p className="mt-3 text-xs text-slate-500 font-medium">
          Loading master items...
        </p>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="py-16 text-center text-rose-600 px-4">
        <AlertTriangle className="w-8 h-8 mx-auto mb-2 text-rose-500" />
        <p className="text-sm font-semibold">Failed to load master items</p>
        <p className="text-xs text-slate-500 mt-1">
          Please check your backend connection.
        </p>
      </div>
    );
  }

  if (items.length === 0) {
    return (
      <div className="py-20 text-center text-slate-400">
        <PackageCheck className="w-10 h-10 mx-auto mb-2 opacity-50" />
        <p className="text-sm font-medium text-slate-600">
          No matching items found
        </p>
        <p className="text-xs text-slate-400 mt-0.5">
          Try using a different search keyword or filter.
        </p>
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-left border-collapse">
        <thead>
          <tr className="border-b border-slate-100 text-[11px] font-bold uppercase tracking-wider text-slate-400 bg-slate-50/50">
            <th className="py-3.5 pl-6 pr-4">Code / Name</th>
            <th className="py-3.5 px-4">Base Unit</th>
            <th className="py-3.5 px-4">Min. Stock</th>
            <th className="py-3.5 px-4">Expiry Warning</th>
            {showActions && (
              <th className="py-3.5 pr-6 pl-4 text-right">Actions</th>
            )}
          </tr>
        </thead>

        <tbody className="divide-y divide-slate-100">
          {items.map((item) => (
            <tr
              key={item.id}
              onClick={() => onRowClick?.(item)}
              className={`hover:bg-slate-50/80 transition-colors group ${
                onRowClick ? "cursor-pointer" : ""
              }`}
            >
              <td className="py-4 pl-6 pr-4">
                <div className="flex items-center gap-3">
                  <div className="w-14 h-14 rounded-lg bg-teal-50 border border-teal-100 flex items-center justify-center text-teal-700 shrink-0 overflow-hidden">
                    {<ItemRowImage imageId={item.imageId} alt={item.name} />}
                  </div>

                  <div>
                    <p className=" font-semibold text-[14px] text-slate-900">
                      {item.code}
                    </p>
                    <p className="text-xs text-slate-500 font-bold">
                      {item.name}
                    </p>
                  </div>
                </div>
              </td>

              <td className="py-4 px-4 text-sm font-medium text-slate-800">
                {item.baseUnit}
              </td>

              <td className="py-4 px-4 text-sm font-medium text-slate-800">
                {item.minimumStock}
              </td>

              <td className="py-4 px-4 text-sm font-medium text-slate-800">
                {item.expiringSoonDays
                  ? `${item.expiringSoonDays} days`
                  : "14 days"}
              </td>

              {showActions && (
                <td className="py-4 pr-6 pl-4 text-right relative">
                  <div
                    className="inline-block"
                    onClick={(e) => e.stopPropagation()}
                  >
                    <button
                      type="button"
                      onClick={() =>
                        setOpenDropdownId(
                          openDropdownId === item.id ? null : item.id,
                        )
                      }
                      className="p-1.5 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 transition-colors"
                    >
                      <MoreHorizontal className="w-4 h-4" />
                    </button>

                    {openDropdownId === item.id && (
                      <div className="absolute right-6 top-12 z-20 w-36 bg-white rounded-xl shadow-lg border border-slate-100 py-1.5 text-left animate-in fade-in zoom-in-95 duration-100">
                        {canManage ? (
                          <>
                            <button
                              type="button"
                              onClick={() => {
                                onEdit?.(item);
                                setOpenDropdownId(null);
                              }}
                              className="w-full flex items-center gap-2 px-3.5 py-2 text-xs font-medium text-slate-700 hover:bg-slate-50 hover:text-emerald-700 transition-colors"
                            >
                              <Edit2 className="w-3.5 h-3.5" />
                              <span>Edit</span>
                            </button>

                            <button
                              type="button"
                              onClick={() => {
                                onDelete?.(item);
                                setOpenDropdownId(null);
                              }}
                              className="w-full flex items-center gap-2 px-3.5 py-2 text-xs font-medium text-rose-600 hover:bg-rose-50 transition-colors"
                            >
                              <Trash2 className="w-3.5 h-3.5" />
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
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
