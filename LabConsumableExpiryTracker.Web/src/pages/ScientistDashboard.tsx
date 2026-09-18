import type { FC } from 'react';
import type { UserProfile } from '../types/auth.types';

interface DashboardProps {
  user: UserProfile | null;
}

export const ScientistDashboard: FC<DashboardProps> = ({ user }) => {
  return (
    <div className="space-y-6">
      {/* Header Info */}
      <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-xs">
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <span className="inline-flex items-center rounded-md bg-blue-50 px-2.5 py-1 text-xs font-semibold text-blue-700 ring-1 ring-blue-600/20 ring-inset">
              Scientist Laboratory Workspace
            </span>
            <h1 className="mt-2 text-2xl font-bold text-slate-900">Dashboard</h1>
            <p className="mt-1 text-sm text-slate-500">
              Welcome back, <span className="font-semibold text-slate-700">{user?.fullName || user?.userName}</span>! Track job consumptions, request consumables with FEFO, and log disposals.
            </p>
          </div>
        </div>
      </div>

      {/* Metric Cards */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-xs">
          <p className="text-xs font-medium text-slate-500 uppercase tracking-wider">Active Jobs</p>
          <p className="mt-2 text-3xl font-bold text-slate-800">--</p>
          <p className="mt-1 text-xs text-slate-400">Assigned laboratory runs</p>
        </div>

        <div className="rounded-xl border border-emerald-200 bg-emerald-50/50 p-5 shadow-xs">
          <p className="text-xs font-medium text-emerald-700 uppercase tracking-wider">Eligible Consumables</p>
          <p className="mt-2 text-3xl font-bold text-emerald-900">Ready</p>
          <p className="mt-1 text-xs text-emerald-600">Active FEFO recommended lots</p>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-xs">
          <p className="text-xs font-medium text-slate-500 uppercase tracking-wider">Recent Consumptions</p>
          <p className="mt-2 text-3xl font-bold text-slate-800">--</p>
          <p className="mt-1 text-xs text-slate-400">Recorded this week</p>
        </div>
      </div>

      {/* Scientist Modules Grid */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-xs">
          <h3 className="font-semibold text-slate-900">FEFO Consumable Selection</h3>
          <p className="mt-1 text-xs text-slate-500">
            Pick eligible lots automatically prioritized by earliest expiry date.
          </p>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-xs">
          <h3 className="font-semibold text-slate-900">Job Consumptions</h3>
          <p className="mt-1 text-xs text-slate-500">
            Record consumable quantities used for laboratory jobs and tests.
          </p>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-xs">
          <h3 className="font-semibold text-slate-900">Log Disposal</h3>
          <p className="mt-1 text-xs text-slate-500">
            Report damaged, contaminated, or expired consumables for disposal.
          </p>
        </div>
      </div>
    </div>
  );
};
