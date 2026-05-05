import axios, { AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from 'axios';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { apiClient, authRedirect } from '../api/client';
import { useAuthStore } from '../store/authStore';
import type { AuthResponse } from '../types';

describe('apiClient auth interceptor', () => {
  afterEach(() => {
    apiClient.defaults.adapter = undefined;
    vi.restoreAllMocks();
    useAuthStore.getState().logout();
  });

  it('refreshes token only once when multiple requests fail with 401', async () => {
    useAuthStore.getState().setAuth(authResponse('old-access', 'refresh-token'));
    const refresh = deferred<AuthResponse>();
    const refreshSpy = vi.spyOn(axios, 'post').mockReturnValue(refresh.promise as never);
    const attempts = new Map<string, number>();

    apiClient.defaults.adapter = async (config) => {
      const url = config.url ?? '';
      const currentAttempts = attempts.get(url) ?? 0;
      attempts.set(url, currentAttempts + 1);

      if (currentAttempts === 0) {
        throw unauthorized(config);
      }

      expect(config.headers?.Authorization).toBe('Bearer new-access');
      return ok(config, { url });
    };

    const requests = Promise.all([
      apiClient.get('/resource-a'),
      apiClient.get('/resource-b'),
      apiClient.get('/resource-c')
    ]);

    await Promise.resolve();
    refresh.resolve({ data: authResponse('new-access', 'new-refresh') });
    const responses = await requests;

    expect(refreshSpy).toHaveBeenCalledTimes(1);
    expect(responses.map((response) => response.data.url)).toEqual(['/resource-a', '/resource-b', '/resource-c']);
    expect(useAuthStore.getState().accessToken).toBe('new-access');
  });

  it('logs out when refresh token is expired', async () => {
    useAuthStore.getState().setAuth(authResponse('old-access', 'expired-refresh'));
    vi.spyOn(axios, 'post').mockRejectedValue(unauthorized({ url: '/api/auth/refresh' } as InternalAxiosRequestConfig));
    const logoutSpy = vi.spyOn(useAuthStore.getState(), 'logout');
    const redirectSpy = vi.spyOn(authRedirect, 'toLogin').mockImplementation(() => undefined);

    apiClient.defaults.adapter = async (config) => {
      throw unauthorized(config);
    };

    await expect(apiClient.get('/protected')).rejects.toBeInstanceOf(AxiosError);

    expect(logoutSpy).toHaveBeenCalledTimes(1);
    expect(redirectSpy).toHaveBeenCalledTimes(1);
  });
});

function authResponse(accessToken: string, refreshToken: string): AuthResponse {
  return {
    accessToken,
    refreshToken,
    expiresAt: new Date(Date.now() + 60_000).toISOString(),
    user: {
      id: 'user-1',
      name: 'Admin',
      email: 'admin@example.com',
      role: 'Admin'
    }
  };
}

function ok(config: InternalAxiosRequestConfig, data: unknown): AxiosResponse {
  return {
    data,
    status: 200,
    statusText: 'OK',
    headers: {},
    config
  };
}

function unauthorized(config: InternalAxiosRequestConfig) {
  return new AxiosError(
    'Unauthorized',
    AxiosError.ERR_BAD_REQUEST,
    config,
    undefined,
    {
      data: { message: 'Unauthorized' },
      status: 401,
      statusText: 'Unauthorized',
      headers: {},
      config
    }
  );
}

function deferred<T>() {
  let resolve!: (value: { data: T }) => void;
  let reject!: (reason?: unknown) => void;
  const promise = new Promise<{ data: T }>((promiseResolve, promiseReject) => {
    resolve = promiseResolve;
    reject = promiseReject;
  });

  return { promise, resolve, reject };
}
