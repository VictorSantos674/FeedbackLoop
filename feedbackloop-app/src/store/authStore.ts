import { create } from 'zustand';
import { createJSONStorage, persist } from 'zustand/middleware';
import type { AuthResponse, User } from '../types';

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  setAuth: (data: AuthResponse) => void;
  setAccessToken: (token: string | null) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      setAuth: (data) =>
        set({
          user: data.user,
          accessToken: data.accessToken,
          refreshToken: data.refreshToken
        }),
      setAccessToken: (token) => set({ accessToken: token }),
      logout: () => {
        set({ user: null, accessToken: null, refreshToken: null });
        localStorage.removeItem('feedbackloop-auth');
      }
    }),
    {
      name: 'feedbackloop-auth',
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({
        user: state.user,
        refreshToken: state.refreshToken
      })
    }
  )
);
