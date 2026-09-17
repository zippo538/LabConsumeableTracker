import React from 'react';

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
}

export const Input: React.FC<InputProps> = ({ label, error, id, ...props }) => {
  return (
    <div className="mb-4">
      <label htmlFor={id} className="block text-sm font-medium text-gray-700 mb-1">
        {label}
      </label>
      <input
        id={id}
        className={`w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors ${
          error ? 'border-red-500 focus:ring-red-500' : 'border-gray-300'
        }`}
        {...props}
      />
      {error && <p className="mt-1 text-sm text-red-600" role="alert">{error}</p>}
    </div>
  );
};