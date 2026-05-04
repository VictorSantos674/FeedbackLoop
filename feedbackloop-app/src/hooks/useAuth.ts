import { useMutation } from '@tanstack/react-query';
import { authApi } from '../api/auth';
import { useAuthStore } from '../store/authStore';
import type { LoginRequest, RegisterRequest } from '../types';

export function useLogin() {
  const setAuth = useAuthStore((state) => state.setAuth);
  return useMutation({
    mutationFn: (data: LoginRequest) => authApi.login(data),
    onSuccess: setAuth
  });
}

export function useRegister() {
  return useMutation({
    mutationFn: (data: RegisterRequest) => authApi.register(data)
  });
}

export function useLogout() {
  const refreshToken = useAuthStore((state) => state.refreshToken);
  const logout = useAuthStore((state) => state.logout);

  return async () => {
    if (refreshToken) {
      try {
        await authApi.logout(refreshToken);
      } catch {
        // Logout must clear local state even when the API is unavailable.
      }
    }
    logout();
  };
}
