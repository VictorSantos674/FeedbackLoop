import { Navigate, Route, Routes } from 'react-router-dom';
import { AppLayout } from './components/layout/AppLayout';
import { AdminRoute, ProtectedRoute } from './components/layout/Routes';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { DashboardPage } from './pages/DashboardPage';
import { BoardPage } from './pages/BoardPage';
import { PostDetailPage } from './pages/PostDetailPage';
import { SettingsPage } from './pages/SettingsPage';
import { NotFoundPage } from './pages/NotFoundPage';

export function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>
          <Route index element={<DashboardPage />} />
          <Route path="boards/:boardId" element={<BoardPage />} />
          <Route path="boards/:boardId/posts/:postId" element={<PostDetailPage />} />
          <Route element={<AdminRoute />}>
            <Route path="settings" element={<SettingsPage />} />
          </Route>
        </Route>
      </Route>
      <Route path="/404" element={<NotFoundPage />} />
      <Route path="*" element={<Navigate to="/404" replace />} />
    </Routes>
  );
}
