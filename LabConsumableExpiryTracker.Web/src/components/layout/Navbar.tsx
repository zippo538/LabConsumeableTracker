import React from 'react';
import { useAtomValue } from 'jotai';
import { useNavigate } from 'react-router';
import { userAtom } from '../../lib/auth/authAtom';
import { useLogout } from '../../hooks/useLogin';
import { LogOut } from 'lucide-react';

interface NavbarProps {
  title?: string;
  subtitle?: string;
}

export const Navbar: React.FC<NavbarProps> = ({
  title = 'Master Item',
  subtitle = 'Definisi bahan dan kebijakan stok minimum',
}) => {
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

  return (
    <header className="h-20 bg-white border-b border-slate-200/80 px-8 flex items-center justify-between shrink-0">
      <div>
        <h1 className="text-xl font-bold text-slate-900 leading-tight">{title}</h1>
        {subtitle && <p className="text-xs text-slate-500 mt-0.5">{subtitle}</p>}
      </div>

      <div className="flex items-center gap-3">
        {/* Role Pill */}
        <span className="inline-flex items-center px-4 py-1.5 rounded-full text-xs font-semibold text-sky-700 bg-sky-50 border border-sky-100 shadow-2xs">
          {user?.roles?.includes('WarehouseAdmin') ? 'Admin Gudang' : 'Scientist'}
        </span>

        {/* Sign out button */}
        <button
          onClick={handleLogout}
          disabled={isPending}
          title="Keluar"
          className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium text-slate-500 hover:text-rose-600 hover:bg-rose-50 border border-slate-200 transition-colors disabled:opacity-50"
        >
          <LogOut className="w-3.5 h-3.5" />
          <span>Keluar</span>
        </button>
      </div>
    </header>
  );
};
