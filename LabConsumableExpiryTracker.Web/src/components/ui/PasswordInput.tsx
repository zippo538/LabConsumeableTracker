// src/components/ui/PasswordInput.tsx
import { useId, useState } from 'react';
import type { InputHTMLAttributes, ReactNode } from 'react';

interface PasswordInputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  action?: ReactNode; // contoh: link "Lupa password?"
}

export const PasswordInput = ({ label, action, id, ...props }: PasswordInputProps) => {
  const [visible, setVisible] = useState(false);
  const autoId = useId();
  const inputId = id ?? autoId;

  return (
    <div>
      <div className="mb-1.5 flex items-end justify-between gap-2">
        <label htmlFor={inputId} className="text-xs font-semibold text-slate-600">
          {label}
        </label>
        {action}
      </div>

      <div className="relative">
        <input
          id={inputId}
          type={visible ? 'text' : 'password'}
          className="w-full rounded-md border border-slate-300 bg-white px-4 py-3 pr-11 text-sm text-slate-800 placeholder:text-slate-400 focus:border-brand-teal focus:outline-none focus:ring-2 focus:ring-brand-teal/25"
          {...props}
        />
        <button
          type="button"
          onClick={() => setVisible((v) => !v)}
          aria-label={visible ? 'Sembunyikan password' : 'Tampilkan password'}
          className="absolute inset-y-0 right-0 flex w-11 items-center justify-center rounded-r-md text-slate-500 hover:text-slate-700 focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-teal"
        >
          {visible ? (
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
              <path d="M3 3l18 18" />
              <path d="M10.6 5.1A9.8 9.8 0 0 1 12 5c6.5 0 10 7 10 7a17.6 17.6 0 0 1-3 3.9" />
              <path d="M6.6 6.6C3.8 8.5 2 12 2 12s3.5 7 10 7c1.9 0 3.6-.6 5-1.4" />
              <path d="M9.9 9.9a3 3 0 0 0 4.2 4.2" />
            </svg>
          ) : (
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
              <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7Z" />
              <circle cx="12" cy="12" r="3" />
            </svg>
          )}
        </button>
      </div>
    </div>
  );
};