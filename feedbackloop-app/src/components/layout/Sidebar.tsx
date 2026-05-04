import { Link, NavLink } from 'react-router-dom';
import { LayoutDashboard, Settings } from 'lucide-react';
import { useBoards } from '../../hooks/useBoards';
import { useAuthStore } from '../../store/authStore';

export function Sidebar() {
  const { data: boards = [] } = useBoards();
  const user = useAuthStore((state) => state.user);

  return (
    <aside className="fixed inset-y-0 left-0 flex w-72 flex-col border-r border-slate-200 bg-white">
      <Link to="/" className="border-b border-slate-200 px-6 py-5 text-xl font-black text-brand-700">
        FeedbackLoop
      </Link>
      <nav className="flex-1 space-y-1 overflow-y-auto p-4">
        <NavLink to="/" className={({ isActive }) => navClass(isActive)}>
          <LayoutDashboard size={18} /> Dashboard
        </NavLink>
        <div className="px-3 pt-5 text-xs font-bold uppercase tracking-wide text-slate-400">Boards</div>
        {boards.map((board) => (
          <NavLink key={board.id} to={`/boards/${board.id}`} className={({ isActive }) => navClass(isActive)}>
            <span className="h-2 w-2 rounded-full bg-brand-500" /> {board.name}
          </NavLink>
        ))}
        {user?.role === 'Admin' ? (
          <NavLink to="/settings" className={({ isActive }) => navClass(isActive)}>
            <Settings size={18} /> Settings
          </NavLink>
        ) : null}
      </nav>
    </aside>
  );
}

function navClass(isActive: boolean) {
  return `flex items-center gap-3 rounded-md px-3 py-2 text-sm font-semibold ${
    isActive ? 'bg-brand-50 text-brand-700' : 'text-slate-600 hover:bg-slate-50'
  }`;
}
