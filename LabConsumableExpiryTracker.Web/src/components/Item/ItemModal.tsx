import React, { useState, useEffect } from 'react';
import { X, Upload, AlertCircle } from 'lucide-react';
import type {
  ItemDto,
  CreateItemPayload,
  UpdateItemPayload,
} from '../../types/item.types';

interface ItemModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (
    payload: CreateItemPayload | UpdateItemPayload
  ) => Promise<void>;
  item?: ItemDto | null;
  isLoading?: boolean;
}

const UNIT_OPTIONS = [
  { value: 'Milliliter', label: 'Milliliter (mL)' },
  { value: 'Gram', label: 'Gram (g)' },
  { value: 'Unit', label: 'Unit' },
  { value: 'Vial', label: 'Vial' },
  { value: 'Box', label: 'Box' },
];

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
const IMAGE_BASE_URL = API_BASE_URL.replace(/\/api$/, '');

export const ItemModal: React.FC<ItemModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  item,
  isLoading = false,
}) => {
  const isEdit = Boolean(item);

  const [code, setCode] = useState('');
  const [name, setName] = useState('');
  const [baseUnit, setBaseUnit] = useState('Milliliter');
  const [minimumStock, setMinimumStock] = useState<number>(0);
  const [expiringSoonDays, setExpiringSoonDays] = useState<number>(14);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

useEffect(() => {
  if (item) {
    setCode(item.code);
    setName(item.name);
    setBaseUnit(item.baseUnit);
    setMinimumStock(item.minimumStock);
    setExpiringSoonDays(item.expiringSoonDays ?? 30);

    setImagePreview(
      item.imagePath
        ? `${IMAGE_BASE_URL}/${item.imagePath}`
        : null
    );

    setImageFile(null);
  } else {
    setCode('');
    setName('');
    setBaseUnit('Unit');
    setMinimumStock(0);
    setExpiringSoonDays(30);
    setImagePreview(null);
    setImageFile(null);
  }
}, [item, isOpen]);

  if (!isOpen) return null;

  const handleImageChange = (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    if (e.target.files && e.target.files[0]) {
      const file = e.target.files[0];

      setImageFile(file);
      setImagePreview(URL.createObjectURL(file));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!isEdit && !code.trim()) {
      setError('Item code is required.');
      return;
    }

    if (!name.trim()) {
      setError('Item name is required.');
      return;
    }

    if (minimumStock < 0) {
      setError('Minimum stock cannot be negative.');
      return;
    }

    if (expiringSoonDays < 0) {
      setError('Expiry warning threshold cannot be negative.');
      return;
    }

    try {
      if (isEdit && item) {
        await onSubmit({
          id: item.id,
          name: name.trim(),
          baseUnit,
          minimumStock: Number(minimumStock),
          expiringSoonDays: Number(expiringSoonDays),
          image: imageFile,
        });
      } else {
        await onSubmit({
          code: code.trim(),
          name: name.trim(),
          baseUnit,
          minimumStock: Number(minimumStock),
          expiringSoonDays: Number(expiringSoonDays),
          image: imageFile,
        });
      }

      onClose();
    } catch (err: unknown) {
      const msg =
        err instanceof Error
          ? err.message
          : 'An error occurred while saving the item.';

      setError(msg);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-xs animate-in fade-in duration-200">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg overflow-hidden border border-slate-200">
        {/* Header */}
        <div className="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
          <h2 className="text-lg font-bold text-slate-900">
            {isEdit ? 'Edit Item' : 'Add New Item'}
          </h2>

          <button
            onClick={onClose}
            disabled={isLoading}
            className="p-1 rounded-lg text-slate-400 hover:text-slate-600 hover:bg-slate-100 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {error && (
            <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg flex items-center gap-2 text-xs font-medium text-rose-700">
              <AlertCircle className="w-4 h-4 shrink-0" />
              <span>{error}</span>
            </div>
          )}

          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">
              Item Code {!isEdit && <span className="text-rose-500">*</span>}
            </label>

            <input
              type="text"
              required={!isEdit}
              disabled={isEdit || isLoading}
              value={code}
              onChange={(e) => setCode(e.target.value)}
              placeholder="Example: REAG-001"
              className={`w-full px-3.5 py-2.5 rounded-lg border text-sm transition-colors outline-hidden ${
                isEdit
                  ? 'bg-slate-100 text-slate-500 border-slate-200 cursor-not-allowed'
                  : 'bg-white border-slate-300 text-slate-900 focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20'
              }`}
            />

            {isEdit && (
              <p className="text-[11px] text-slate-400 mt-1">
                Item code cannot be changed after creation.
              </p>
            )}
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">
              Item Name <span className="text-rose-500">*</span>
            </label>

            <input
              type="text"
              required
              disabled={isLoading}
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Example: Buffer Reagent A"
              className="w-full px-3.5 py-2.5 rounded-lg border border-slate-300 bg-white text-sm text-slate-900 focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 outline-hidden transition-colors"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">
                Base Unit <span className="text-rose-500">*</span>
              </label>

              <select
                value={baseUnit}
                disabled={isLoading}
                onChange={(e) => setBaseUnit(e.target.value)}
                className="w-full px-3 py-2.5 rounded-lg border border-slate-300 bg-white text-sm text-slate-900 focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 outline-hidden transition-colors"
              >
                {UNIT_OPTIONS.map((opt) => (
                  <option key={opt.value} value={opt.value}>
                    {opt.label}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">
                Minimum Stock <span className="text-rose-500">*</span>
              </label>

              <input
                type="number"
                min="0"
                step="any"
                required
                disabled={isLoading}
                value={minimumStock}
                onChange={(e) =>
                  setMinimumStock(
                    parseFloat(e.target.value) || 0
                  )
                }
                className="w-full px-3.5 py-2.5 rounded-lg border border-slate-300 bg-white text-sm text-slate-900 focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 outline-hidden transition-colors"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">
              Expiry Warning Threshold (Days){' '}
              <span className="text-rose-500">*</span>
            </label>

            <input
              type="number"
              min="0"
              required
              disabled={isLoading}
              value={expiringSoonDays}
              onChange={(e) =>
                setExpiringSoonDays(
                  parseInt(e.target.value, 10) || 0
                )
              }
              placeholder="14"
              className="w-full px-3.5 py-2.5 rounded-lg border border-slate-300 bg-white text-sm text-slate-900 focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 outline-hidden transition-colors"
            />

            <p className="text-[11px] text-slate-400 mt-1">
              A lot is marked as "Expiring Soon" when it expires
              within this number of days.
            </p>
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">
              Item Image (Optional)
            </label>

            <div className="flex items-center gap-3">
              {imagePreview && (
                <div className="w-12 h-12 rounded-lg border border-slate-200 overflow-hidden bg-slate-50 shrink-0">
                  <img
                    src={imagePreview}
                    alt="Preview"
                    className="w-full h-full object-cover"
                  />
                </div>
              )}

              <label className="flex-1 flex items-center justify-center gap-2 px-3.5 py-2.5 rounded-lg border border-dashed border-slate-300 hover:border-emerald-500 hover:bg-emerald-50/20 text-xs font-medium text-slate-600 cursor-pointer transition-colors">
                <Upload className="w-4 h-4 text-slate-400" />

                <span>
                  {imageFile
                    ? imageFile.name
                    : 'Choose image file (.jpg, .png)'}
                </span>

                <input
                  type="file"
                  accept="image/jpeg,image/png,image/webp"
                  onChange={handleImageChange}
                  className="hidden"
                />
              </label>
            </div>
          </div>

          <div className="pt-4 flex items-center justify-end gap-3 border-t border-slate-100">
            <button
              type="button"
              onClick={onClose}
              disabled={isLoading}
              className="px-4 py-2.5 rounded-lg border border-slate-200 text-xs font-semibold text-slate-600 hover:bg-slate-50 transition-colors"
            >
              Cancel
            </button>

            <button
              type="submit"
              disabled={isLoading}
              className="px-5 py-2.5 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-semibold shadow-xs transition-colors disabled:opacity-60"
            >
              {isLoading
                ? 'Saving...'
                : isEdit
                  ? 'Save Changes'
                  : 'Add Item'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};