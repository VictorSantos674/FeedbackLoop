import { describe, expect, it } from 'vitest';
import { useAuthStore } from '../store/authStore';
import type { AuthResponse } from '../types';

describe('authStore', () => {
  it('setAuth salva user e refreshToken', () => {
    useAuthStore.getState().setAuth(authResponse);

    expect(useAuthStore.getState().user?.email).toBe('admin@example.com');
    expect(useAuthStore.getState().refreshToken).toBe('refresh-token');
  });

  it('logout limpa estado e localStorage', () => {
    useAuthStore.getState().setAuth(authResponse);
    useAuthStore.getState().logout();

    expect(useAuthStore.getState().user).toBeNull();
    expect(useAuthStore.getState().refreshToken).toBeNull();
    expect(localStorage.getItem('feedbackloop-auth')).toBeNull();
  });

  it('accessToken nunca vai para localStorage', () => {
    useAuthStore.getState().setAuth(authResponse);

    expect(localStorage.getItem('feedbackloop-auth')).not.toContain('access-token');
  });
});

const authResponse: AuthResponse = {
  accessToken: 'access-token',
  refreshToken: 'refresh-token',
  expiresAt: new Date().toISOString(),
  user: {
    id: 'user-1',
    name: 'Admin',
    email: 'admin@example.com',
    role: 'Admin'
  }
};
