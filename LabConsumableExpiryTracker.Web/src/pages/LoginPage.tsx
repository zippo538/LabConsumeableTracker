// src/pages/LoginPage.tsx
import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router';
import { useLogin } from '../hooks/useLogin';
import { ErrorAlert } from '../components/ui/ErrorAlert';
import { PasswordInput } from '../components/ui/PasswordInput';
import FormulatrixLogo  from '../assets/logo_formulatrix.png';
import loginHero from '../assets/hero.png'


// UI-02 & AC-F13-02: pesan umum, tidak mengungkap keberadaan akun
const GENERIC_LOGIN_ERROR = 'Periksa kembali username dan password.';

export const LoginPage = () => {
  const [userName, setUserName] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [showError, setShowError] = useState(false);

  const navigate = useNavigate();
  const location = useLocation();
  const { mutate: login, isPending } = useLogin();

  // UI-02: hanya return path internal yang diizinkan
  const from =
    (location.state as { from?: { pathname?: string } } | null)?.from?.pathname ??
    '/app/dashboard';

  const submit = () => {
    setShowError(false);
    login(
      { userName, password, rememberMe },
      {
        onSuccess: () => navigate(from, { replace: true }),
        onError: () => setShowError(true),
      },
    );
  };

  return (
    <main className="flex min-h-screen items-center justify-center bg-brand-bg px-4 py-8 sm:px-6 lg:px-10">
      <div className="grid w-full max-w-6xl overflow-hidden rounded-2xl bg-brand-surface shadow-2xl lg:grid-cols-2">
        {/* ===== Panel kiri: form ===== */}
        <section className="flex flex-col px-6 py-8 sm:px-10 lg:px-14 lg:py-10">
          <img
            src={FormulatrixLogo}
            alt="Logo Formulatrix"
            className="h-6 w-auto object-contain sm:h-7"
          />

          <div className="mx-auto flex w-full max-w-md flex-1 flex-col justify-center py-8">
            <h1 className="text-center text-3xl font-extrabold tracking-tight text-brand-teal sm:text-4xl">
              LABCONSUMABLE TRACKER
            </h1>
            <h2 className="mt-4 text-xl font-bold text-slate-800 sm:text-2xl">
              Selamat datang kembali
            </h2>
            <p className="mt-1.5 text-sm text-slate-500">
              Masuk menggunakan akun laboratorium/Admin Anda.
            </p>

            {showError && (
              <div className="mt-6">
                <ErrorAlert title="Tidak dapat masuk" message={GENERIC_LOGIN_ERROR} />
              </div>
            )}

            <form
              className="mt-6 space-y-5"
              onSubmit={(event) => {
                event.preventDefault();
                submit();
              }}
            >
              <div>
                <label htmlFor="username" className="mb-1.5 block text-xs font-semibold text-slate-600">
                  Username
                </label>
                <input
                  id="username"
                  name="username"
                  type="text"
                  autoComplete="username"
                  required
                  value={userName}
                  onChange={(e) => setUserName(e.target.value)}
                  placeholder="nama.pengguna"
                  className="w-full rounded-md border border-slate-300 bg-white px-4 py-3 text-sm text-slate-800 placeholder:text-slate-400 focus:border-brand-teal focus:outline-none focus:ring-2 focus:ring-brand-teal/25"
                />
              </div>

              <PasswordInput
                label="Password"
                name="password"
                autoComplete="current-password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                action={
                  <a href="#" className="text-xs font-semibold text-brand-link hover:underline">
                    Lupa password?
                  </a>
                }
              />

              <div className="flex items-center gap-2.5">
                <input
                  id="remember"
                  type="checkbox"
                  checked={rememberMe}
                  onChange={(e) => setRememberMe(e.target.checked)}
                  className="h-4 w-4 rounded border-slate-300 accent-brand-teal"
                />
                <label htmlFor="remember" className="text-xs text-slate-600">
                  Ingat sesi masuk
                </label>
              </div>

              {/* UI-08: cegah double submit saat pending */}
              <button
                type="submit"
                disabled={isPending}
                className="w-full rounded-md bg-brand-green py-3.5 text-sm font-bold tracking-wider text-white transition-colors hover:bg-brand-green-dark focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-green focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {isPending ? 'MEMPROSES...' : 'MASUK KE SISTEM'}
              </button>
            </form>

            <p className="mt-8 text-center text-xs text-slate-500">
              Belum punya akun?{' '}
              <Link to="/register" className="font-semibold text-brand-link hover:underline">
                Daftar
              </Link>
            </p>
          </div>

          <p className="text-center text-[11px] text-slate-400">
            © 2026 ConsumableTracker · Sistem gudang laboratorium
          </p>
        </section>

        {/* ===== Panel kanan: foto hero (UI-18: hidden di viewport sempit) ===== */}
        <section className="relative hidden bg-slate-300 lg:block">
          <img
            src={loginHero}
            alt=""
            aria-hidden="true"
            className="absolute inset-0 h-full w-full object-cover"
          />
        </section>
      </div>
    </main>
  );
};