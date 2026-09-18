import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router';
import { useLogin } from '../hooks/useLogin';
import { ErrorAlert } from '../components/ui/ErrorAlert';
import { PasswordInput } from '../components/ui/PasswordInput';
import FormulatrixLogo from '../assets/logo_formulatrix.png';
import loginHero from '../assets/hero.png';

const GENERIC_LOGIN_ERROR = 'Invalid username or password.';

export const LoginPage = () => {
  const [userName, setUserName] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [showError, setShowError] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string>('');

  const navigate = useNavigate();
  const location = useLocation();
  const { mutate: login, isPending } = useLogin();

  const from =
    (location.state as { from?: { pathname?: string } } | null)?.from?.pathname ??
    '/dashboard';

  const submit = () => {
    setShowError(false);
    setErrorMessage('');

    console.log('[LoginPage] Submitting login for:', userName);

    login(
      { username: userName.trim(), password, rememberMe },
      {
        onSuccess: (data) => {
          console.log('[LoginPage] Login success, user data:', data);
          navigate(from, { replace: true });
        },
        onError: (err: any) => {
          console.error('[LoginPage] Login error response:', err);
          const message =
            err.response?.data?.message ||
            err.message ||
            GENERIC_LOGIN_ERROR;
          setErrorMessage(message);
          setShowError(true);
        },
      },
    );
  };

  return (
    <main className="flex min-h-screen items-center justify-center bg-brand-bg px-4 py-8 sm:px-6 lg:px-10">
      <div className="grid w-full max-w-6xl overflow-hidden rounded-2xl bg-brand-surface shadow-2xl lg:grid-cols-2">

        {/* Login form */}
        <section className="flex flex-col px-6 py-8 sm:px-10 lg:px-14 lg:py-10">

          {/* Logo */}
          <img
            src={FormulatrixLogo}
            alt="Formulatrix"
            className="h-6 w-auto object-contain self-start sm:h-7"
          />

          <div className="mx-auto flex w-full max-w-md flex-1 flex-col justify-center py-10">

            {/* Header */}
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-brand-teal">
                Lab Consumable Tracker
              </p>

              <h1 className="mt-3 text-3xl font-semibold tracking-tight text-slate-900 sm:text-4xl">
                Welcome back
              </h1>

              <p className="mt-2 text-sm leading-6 text-slate-500">
                Sign in to manage laboratory consumables and inventory.
              </p>
            </div>

            {/* Error */}
            {showError && (
              <div className="mt-6">
                <ErrorAlert
                  title="Sign-in failed"
                  message={errorMessage || GENERIC_LOGIN_ERROR}
                />
              </div>
            )}

            {/* Form */}
            <form
              className="mt-8 space-y-5"
              onSubmit={(event) => {
                event.preventDefault();
                submit();
              }}
            >
              {/* Username */}
              <div>
                <label
                  htmlFor="username"
                  className="mb-2 block text-sm font-medium text-slate-700"
                >
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
                  placeholder="Enter your username"
                  className="w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm text-slate-800 outline-none transition placeholder:text-slate-400 focus:border-brand-teal focus:ring-2 focus:ring-brand-teal/20"
                />
              </div>

              {/* Password */}
              <PasswordInput
                label="Password"
                name="password"
                autoComplete="current-password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />

              {/* Remember me */}
              <div className="flex items-center gap-2.5">
                <input
                  id="remember"
                  type="checkbox"
                  checked={rememberMe}
                  onChange={(e) => setRememberMe(e.target.checked)}
                  className="h-4 w-4 rounded border-slate-300 accent-brand-teal"
                />

                <label
                  htmlFor="remember"
                  className="text-sm text-slate-600"
                >
                  Remember me
                </label>
              </div>

              {/* Submit */}
              <button
                type="submit"
                disabled={isPending}
                className="w-full rounded-lg bg-brand-green py-3.5 text-sm font-semibold text-white shadow-sm transition hover:bg-brand-green-dark focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-green focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {isPending ? 'Signing in...' : 'Sign in'}
              </button>
            </form>

            {/* Register */}
            <p className="mt-7 text-center text-sm text-slate-500">
              Don't have an account?{' '}
              <Link
                to="/register"
                className="font-semibold text-brand-link hover:underline"
              >
                Create an account
              </Link>
            </p>
          </div>

          {/* Footer */}
          <p className="text-center text-[11px] text-slate-400">
            © 2026 Consumable Tracker
          </p>
        </section>

        {/* Hero */}
        <section className="relative hidden bg-slate-300 lg:block">
          <img
            src={loginHero}
            alt=""
            aria-hidden="true"
            className="absolute inset-0 h-full w-full object-cover"
          />

          <div className="absolute inset-0 bg-black/10" />
        </section>
      </div>
    </main>
  );
};