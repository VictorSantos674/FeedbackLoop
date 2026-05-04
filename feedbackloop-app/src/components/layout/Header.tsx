import { useAuthStore } from '../../store/authStore';

export function Header() {
  const user = useAuthStore((state) => state.user);
  const initials = user?.name
    .split(' ')
    .map((part) => part[0])
    .join('')
    .slice(0, 2)
    .toUpperCase();

  return (
    <header className="sticky top-0 z-10 flex h-16 items-center justify-between border-b border-slate-200 bg-white/95 px-8 backdrop-blur">
      <div>
        <p className="text-xs font-bold uppercase tracking-wide text-slate-400">Workspace</p>
        <h1 className="font-bold text-slate-900">FeedbackLoop Admin</h1>
      </div>
      <div className="flex items-center gap-3">
        <div className="text-right">
          <p className="text-sm font-bold">{user?.name}</p>
          <p className="text-xs text-slate-500">{user?.role}</p>
        </div>
        <div className="grid h-10 w-10 place-items-center rounded-full bg-brand-600 text-sm font-black text-white">{initials}</div>
      </div>
    </header>
  );
}
