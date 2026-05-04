import { forwardRef, type InputHTMLAttributes } from 'react';

type InputProps = InputHTMLAttributes<HTMLInputElement> & {
  label?: string;
  error?: string;
};

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input({ label, error, className = '', ...props }, ref) {
  return (
    <label className="grid gap-1.5 text-sm font-medium text-slate-700">
      {label ? <span>{label}</span> : null}
      <input
        ref={ref}
        className={`rounded-md border border-slate-200 bg-white px-3 py-2 text-slate-900 outline-none ring-brand-500 transition focus:ring-2 ${className}`}
        {...props}
      />
      {error ? <span className="text-xs font-medium text-red-600">{error}</span> : null}
    </label>
  );
});
