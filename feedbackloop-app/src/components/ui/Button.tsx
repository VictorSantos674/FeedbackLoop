import type { ButtonHTMLAttributes, ReactNode } from 'react';

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: 'primary' | 'secondary' | 'ghost';
  children: ReactNode;
};

export function Button({ variant = 'primary', className = '', children, ...props }: ButtonProps) {
  const variants = {
    primary: 'bg-brand-600 text-white hover:bg-brand-700 disabled:bg-brand-300',
    secondary: 'border border-slate-200 bg-white text-slate-900 hover:bg-slate-50',
    ghost: 'text-slate-600 hover:bg-slate-100'
  };

  return (
    <button
      className={`inline-flex items-center justify-center gap-2 rounded-md px-3 py-2 text-sm font-semibold transition disabled:cursor-not-allowed disabled:opacity-60 ${variants[variant]} ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}
