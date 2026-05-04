import { Outlet, useNavigate } from 'react-router-dom';
import { LogOut } from 'lucide-react';
import { Sidebar } from './Sidebar';
import { Header } from './Header';
import { Button } from '../ui/Button';
import { useLogout } from '../../hooks/useAuth';

export function AppLayout() {
  const logout = useLogout();
  const navigate = useNavigate();

  async function handleLogout() {
    await logout();
    navigate('/login');
  }

  return (
    <div className="min-h-screen bg-slate-50">
      <Sidebar />
      <div className="ml-72 min-h-screen">
        <Header />
        <main className="p-8">
          <Outlet />
        </main>
        <div className="fixed bottom-4 left-4 w-64">
          <Button variant="secondary" className="w-full" onClick={handleLogout}>
            <LogOut size={16} /> Sair
          </Button>
        </div>
      </div>
    </div>
  );
}
