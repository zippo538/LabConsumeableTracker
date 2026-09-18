import type { FC } from 'react';
import type { UserProfile } from '../types/auth.types';

interface DashboardProps {
  user: UserProfile | null;
}

export const WarehouseAdminDashboard: FC<DashboardProps> = ({ user }) => {
  return (
    <div className="space-y-6">
      {/* Header Info */}
      <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-xs">
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <span className="inline-flex items-center rounded-md bg-emerald-50 px-2.5 py-1 text-xs font-semibold text-emerald-700 ring-1 ring-emerald-600/20 ring-inset">
              Warehouse Admin Workspace
            </span>
            <h1 className="mt-2 text-2xl font-bold text-slate-900">Dashboard</h1>
            <p className="mt-1 text-sm text-slate-500">
              Welcome back, <span className="font-semibold text-slate-700">{user?.fullName || user?.userName}</span>! Manage consumable stocks, monitor expiry dates, and manage users.
            </p>
          </div>
        </div>
      </div>

      {/* Metric Cards */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-xs">
          <p className="text-xs font-medium text-slate-500 uppercase tracking-wider">Total Consumables</p>
          <p className="mt-2 text-3xl font-bold text-slate-800">--</p>
          <p className="mt-1 text-xs text-slate-400">Registered catalogue items</p>
        </div>

        <div className="rounded-xl border border-amber-200 bg-amber-50/50 p-5 shadow-xs">
          <p className="text-xs font-medium text-amber-700 uppercase tracking-wider">Expiring Soon Lots</p>
          <p className="mt-2 text-3xl font-bold text-amber-900">--</p>
          <p className="mt-1 text-xs text-amber-600">Requires attention</p>
        </div>

        <div className="rounded-xl border border-rose-200 bg-rose-50/50 p-5 shadow-xs">
          <p className="text-xs font-medium text-rose-700 uppercase tracking-wider">Expired Lots</p>
          <p className="mt-2 text-3xl font-bold text-rose-900">--</p>
          <p className="mt-1 text-xs text-rose-600">Pending disposal / quarantine</p>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-xs">
          <p className="text-xs font-medium text-slate-500 uppercase tracking-wider">Low Stock Items</p>
          <p className="mt-2 text-3xl font-bold text-slate-800">--</p>
          <p className="mt-1 text-xs text-slate-400">Below minimum threshold</p>
        </div>
      </div>

      {/* Admin Modules Grid */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-xs">
          <h3 className="font-semibold text-slate-900">Consumable Management</h3>
          <p className="mt-1 text-xs text-slate-500">
            Create, update, and manage consumable items and lots in the inventory.
          </p>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-xs">
          <h3 className="font-semibold text-slate-900">User Management</h3>
          <p className="mt-1 text-xs text-slate-500">
            Activate, deactivate, and assign roles for laboratory staff.
          </p>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-xs">
          <h3 className="font-semibold text-slate-900">Audit & Disposals</h3>
          <p className="mt-1 text-xs text-slate-500">
            Review stock adjustments, expired lot disposal logs, and FEFO history.
          </p>
        </div>
      </div>
    </div>
  );
};
