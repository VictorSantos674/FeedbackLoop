import { apiClient } from './client';
import type { AuthResponse, LoginRequest, RegisterRequest } from '../types';

export const authApi = {
  async login(data: LoginRequest) {
    const response = await apiClient.post<AuthResponse>('/api/auth/login', data);
    return response.data;
  },
  async register(data: RegisterRequest) {
    const response = await apiClient.post<AuthResponse>('/api/auth/register', data);
    return response.data;
  },
  async logout(refreshToken: string) {
    await apiClient.post('/api/auth/logout', { refreshToken });
  }
};
