import type { ReactNode } from 'react';
import { Button } from './Button';

export function Modal({ title, children, onClose }: { title: string; children: ReactNode; onClose: () => void }) {
  return (
    <div className="fixed inset-0 z-50 grid place-items-center bg-slate-950/40 p-4">
      <div className="w-full max-w-lg rounded-lg bg-white shadow-xl">
        <header className="flex items-center justify-between border-b border-slate-200 px-5 py-4">
          <h2 className="text-lg font-bold">{title}</h2>
          <Button variant="ghost" onClick={onClose}>Fechar</Button>
        </header>
        <div className="p-5">{children}</div>
      </div>
    </div>
  );
}
