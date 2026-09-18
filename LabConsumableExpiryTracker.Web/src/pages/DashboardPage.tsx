import { useAtomValue } from 'jotai';
import { useNavigate } from 'react-router';
import { userAtom } from '../lib/auth/authAtom';
import { useLogout } from '../hooks/useLogin';
import { WarehouseAdminDashboard } from './WarehouseAdminDashboard';
import { ScientistDashboard } from './ScientistDashboard';
import FormulatrixLogo from '../assets/logo_formulatrix.png';

export const DashboardPage = () => {
  const user = useAtomValue(userAtom);
  const navigate = useNavigate();
  const { mutate: logout, isPending } = useLogout();

  const handleLogout = () => {
    logout(undefined, {
      onSettled: () => {
        navigate('/login', { replace: true });
      },
    });
  };

  const isWarehouseAdmin =
    user?.role?.toLowerCase() === 'warehouseadmin' ||
    user?.role?.toLowerCase() === 'admin';

  return (
    <div className="min-h-screen bg-slate-50">
      {/* Top navigation bar */}
      <header className="border-b border-slate-200 bg-white sticky top-0 z-10 shadow-2xs">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3 sm:px-6 lg:px-8">
          <div className="flex items-center gap-4">
            <img
              src={FormulatrixLogo}
              alt="Formulatrix"
              className="h-6 w-auto object-contain"
            />
            <div className="h-4 w-px bg-slate-200 hidden sm:block" />
            <span className="text-xs font-semibold uppercase tracking-wider text-brand-teal hidden sm:inline">
              Lab Consumable Expiry Tracker
            </span>
          </div>

          <div className="flex items-center gap-4">
            {user && (
              <div className="text-right text-xs">
                <p className="font-semibold text-slate-800">{user.fullName || user.userName}</p>
                <span className="inline-flex items-center rounded-full bg-slate-100 px-2 py-0.5 text-[10px] font-medium text-slate-600">
                  {user.role}
                </span>
              </div>
            )}
            <button
              onClick={handleLogout}
              disabled={isPending}
              className="rounded-md border border-slate-300 bg-white px-3 py-1.5 text-xs font-semibold text-slate-700 shadow-2xs hover:bg-slate-50 disabled:opacity-50 transition-colors"
            >
              {isPending ? 'Signing out...' : 'Sign out'}
            </button>
          </div>
        </div>
      </header>

      {/* Role-based dashboard content */}
      <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        {isWarehouseAdmin ? (
          <WarehouseAdminDashboard user={user} />
        ) : (
          <ScientistDashboard user={user} />
        )}
      </main>
    </div>
  );
};

export default DashboardPage;

