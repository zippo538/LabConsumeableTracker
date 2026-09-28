import { useState, useEffect } from "react";
import { FormModal, ModalField } from "../Modal";
import type { CreateLotRequest, ItemOption } from "../../types/lot.types";

interface CreateLotModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (payload: CreateLotRequest) => void;
  items: ItemOption[];
  isLoading?: boolean;
  error?: string;
}

const getInitialFormData = (): CreateLotRequest => ({
  itemId: "",
  lotNumber: "",
  initialQuantity: 0,
  expiryDate: "",
  storageLocation: "",
  receivedAt: new Date().toISOString().slice(0, 10),
  suppliername: "",
  supplierLotNumber: "",
});

export const CreateLotModal = ({
  isOpen,
  onClose,
  onSubmit,
  items,
  isLoading,
  error,
}: CreateLotModalProps) => {
  const [formData, setFormData] = useState<CreateLotRequest>(
    getInitialFormData()
  );

  const [itemSearch, setItemSearch] = useState("");
  const [isItemDropdownOpen, setIsItemDropdownOpen] = useState(false);

  useEffect(() => {
    if (isOpen) {
      setFormData(getInitialFormData());
      setItemSearch("");
      setIsItemDropdownOpen(false);
    }
  }, [isOpen]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  const filteredItems = items.filter((item) => {
    const search = itemSearch.toLowerCase();

    return (
      item.name.toLowerCase().includes(search) ||
      item.code.toLowerCase().includes(search)
    );
  });

  const selectedItem = items.find((item) => item.id === formData.itemId);

  return (
    <FormModal
      isOpen={isOpen}
      onClose={onClose}
      mode="create"
      title="Register New Lot"
      description="Fill in the required information."
      submitLabel="Save Lot"
      onSubmit={handleSubmit}
      busy={isLoading}
      error={error ? <p>{error}</p> : undefined}
      identity={
        selectedItem ? (
          <div className="flex items-center gap-3">
            <div className="grid h-10 w-10 shrink-0 place-items-center rounded-lg bg-teal-50 text-sm font-bold text-teal-700">
              {selectedItem.name.slice(0, 1).toUpperCase()}
            </div>
            <div>
              <p className="text-sm font-semibold text-slate-900">
                {selectedItem.name}
              </p>
              <p className="text-xs text-slate-500">
                {selectedItem.code} · {selectedItem.baseUnit}
              </p>
            </div>
          </div>
        ) : undefined
      }
    >
      <ModalField label="Master Item" htmlFor="create-itemId" required>
        <div className="relative">
          <input
            id="create-itemId"
            type="text"
            value={
              isItemDropdownOpen
                ? itemSearch
                : selectedItem
                  ? `${selectedItem.name} (${selectedItem.code})`
                  : ""
            }
            onChange={(e) => {
              setItemSearch(e.target.value);
              setIsItemDropdownOpen(true);

              if (formData.itemId) {
                setFormData({
                  ...formData,
                  itemId: "",
                });
              }
            }}
            onFocus={() => {
              setItemSearch("");
              setIsItemDropdownOpen(true);
            }}
            placeholder="Search item..."
            autoComplete="off"
            className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 pr-10 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
          />

          {selectedItem && !isItemDropdownOpen && (
            <button
              type="button"
              onClick={() => {
                setFormData({
                  ...formData,
                  itemId: "",
                });
                setItemSearch("");
                setIsItemDropdownOpen(true);
              }}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
            >
              ×
            </button>
          )}

          {isItemDropdownOpen && (
            <div className="absolute z-50 mt-1 max-h-60 w-full overflow-y-auto rounded-lg border border-slate-200 bg-white shadow-lg">
              {filteredItems.length > 0 ? (
                filteredItems.map((item) => (
                  <button
                    key={item.id}
                    type="button"
                    onClick={() => {
                      setFormData({
                        ...formData,
                        itemId: item.id,
                      });

                      setItemSearch("");
                      setIsItemDropdownOpen(false);
                    }}
                    className="flex w-full items-center justify-between px-3 py-2.5 text-left hover:bg-teal-50"
                  >
                    <div>
                      <p className="text-sm font-medium text-slate-800">
                        {item.name}
                      </p>

                      <p className="text-xs text-slate-500">
                        {item.code} · {item.baseUnit}
                      </p>
                    </div>
                  </button>
                ))
              ) : (
                <div className="px-3 py-3 text-sm text-slate-500">
                  No items found.
                </div>
              )}
            </div>
          )}
        </div>
      </ModalField>

      <ModalField label="Lot Number" htmlFor="create-lotNumber" required>
        <input
          id="create-lotNumber"
          type="text"
          value={formData.lotNumber}
          onChange={(e) =>
            setFormData({ ...formData, lotNumber: e.target.value })
          }
          placeholder="LOT-XXX-001"
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>

      <ModalField
        label="Initial Quantity"
        htmlFor="create-initialQuantity"
        required
      >
        <input
          id="create-initialQuantity"
          type="number"
          step="0.01"
          min="0"
          value={formData.initialQuantity || ""}
          onChange={(e) =>
            setFormData({
              ...formData,
              initialQuantity: parseFloat(e.target.value) || 0,
            })
          }
          placeholder="50"
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>

      <ModalField label="Expiry Date" htmlFor="create-expiryDate" required>
        <input
          id="create-expiryDate"
          type="date"
          value={formData.expiryDate}
          onChange={(e) =>
            setFormData({ ...formData, expiryDate: e.target.value })
          }
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>

      <ModalField label="Received At" htmlFor="create-receivedAt" required>
        <input
          id="create-receivedAt"
          type="date"
          value={formData.receivedAt}
          onChange={(e) =>
            setFormData({ ...formData, receivedAt: e.target.value })
          }
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>
      <ModalField label="Supplier Name" htmlFor="create-supplierName" required>
        <input
          id="create-supplierName"
          type="text"
          value={formData.suppliername}
          onChange={(e) =>
            setFormData({ ...formData, suppliername: e.target.value })
          }
          placeholder="Nama supplier"
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>

      <ModalField
        label="Supplier Lot Number"
        htmlFor="create-supplierLotNumber"
        required
      >
        <input
          id="create-supplierLotNumber"
          type="text"
          value={formData.supplierLotNumber}
          onChange={(e) =>
            setFormData({ ...formData, supplierLotNumber: e.target.value })
          }
          placeholder="Nomor lot dari supplier"
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>

      <ModalField
        label="Storage Location"
        htmlFor="create-storageLocation"
        required
      >
        <input
          id="create-storageLocation"
          type="text"
          value={formData.storageLocation}
          onChange={(e) =>
            setFormData({ ...formData, storageLocation: e.target.value })
          }
          placeholder="Rack A-02"
          className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none focus:border-teal-500 focus:ring-2 focus:ring-teal-100"
        />
      </ModalField>
    </FormModal>
  );
};