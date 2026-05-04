import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { useAuthStore } from '../store/authStore';
import type { AuthResponse } from '../types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

export const apiClient = axios.create({
  baseURL: API_BASE_URL
});

let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
}> = [];

apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as (InternalAxiosRequestConfig & { _retry?: boolean }) | undefined;

    if (error.response?.status !== 401 || !originalRequest || originalRequest._retry) {
      return Promise.reject(error);
    }

    originalRequest._retry = true;
    const refreshToken = useAuthStore.getState().refreshToken;
    if (!refreshToken) {
      useAuthStore.getState().logout();
      window.location.assign('/login');
      return Promise.reject(error);
    }

    if (isRefreshing) {
      return new Promise<string>((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      }).then((token) => {
        originalRequest.headers.Authorization = `Bearer ${token}`;
        return apiClient(originalRequest);
      });
    }

    isRefreshing = true;

    try {
      const response = await axios.post<AuthResponse>(`${API_BASE_URL}/api/auth/refresh`, { refreshToken });
      useAuthStore.getState().setAuth(response.data);
      processQueue(null, response.data.accessToken);
      originalRequest.headers.Authorization = `Bearer ${response.data.accessToken}`;
      return apiClient(originalRequest);
    } catch (refreshError) {
      processQueue(refreshError, null);
      useAuthStore.getState().logout();
      window.location.assign('/login');
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  }
);

function processQueue(error: unknown, token: string | null) {
  failedQueue.forEach((request) => {
    if (error || !token) {
      request.reject(error);
      return;
    }
    request.resolve(token);
  });
  failedQueue = [];
}
