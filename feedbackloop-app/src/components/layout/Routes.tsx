import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';

export function ProtectedRoute() {
  const refreshToken = useAuthStore((state) => state.refreshToken);
  return refreshToken ? <Outlet /> : <Navigate to="/login" replace />;
}

export function AdminRoute() {
  const user = useAuthStore((state) => state.user);
  return user?.role === 'Admin' ? <Outlet /> : <Navigate to="/" replace />;
}
