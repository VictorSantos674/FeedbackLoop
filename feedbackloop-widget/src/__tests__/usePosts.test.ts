import { act, renderHook, waitFor } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { clearPostsCache, usePosts } from '../hooks/usePosts';
import type { PagedResult, Post, WidgetConfig } from '../types';

describe('usePosts', () => {
  afterEach(() => {
    vi.restoreAllMocks();
    clearPostsCache();
  });

  it('fetch inicial popula lista', async () => {
    mockFetch(pageResult([createPost('1')], 1, 1));

    const { result } = renderHook(() => usePosts(config, { pageSize: 20 }));

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.posts).toHaveLength(1);
    expect(result.current.posts[0].id).toBe('1');
  });

  it('loadMore incrementa pagina e concatena resultados', async () => {
    mockFetchSequence([
      pageResult([createPost('1')], 1, 2),
      pageResult([createPost('2')], 2, 2)
    ]);

    const { result } = renderHook(() => usePosts(config, { pageSize: 1 }));
    await waitFor(() => expect(result.current.isLoading).toBe(false));

    await act(async () => {
      await result.current.loadMore();
    });

    expect(result.current.posts.map((post) => post.id)).toEqual(['1', '2']);
  });

  it('erro de rede seta error sem apagar posts existentes', async () => {
    mockFetchSequence([
      pageResult([createPost('1')], 1, 2),
      null
    ]);

    const { result } = renderHook(() => usePosts(config, { pageSize: 1 }));
    await waitFor(() => expect(result.current.isLoading).toBe(false));

    await act(async () => {
      await result.current.loadMore();
    });

    expect(result.current.error).not.toBeNull();
    expect(result.current.posts.map((post) => post.id)).toEqual(['1']);
  });
});

function mockFetch(result: PagedResult<Post>) {
  vi.stubGlobal('fetch', vi.fn(async () => okResponse(result)));
}

function mockFetchSequence(results: Array<PagedResult<Post> | null>) {
  const fetchMock = vi.fn();
  for (const result of results) {
    if (result) {
      fetchMock.mockResolvedValueOnce(okResponse(result));
    } else {
      fetchMock.mockRejectedValueOnce(new Error('network'));
    }
  }
  vi.stubGlobal('fetch', fetchMock);
}

function okResponse(body: unknown) {
  return {
    ok: true,
    json: async () => body
  };
}

function pageResult(items: Post[], page: number, totalPages: number): PagedResult<Post> {
  return {
    items,
    totalCount: totalPages,
    page,
    pageSize: 1,
    totalPages
  };
}

function createPost(id: string): Post {
  return {
    id,
    title: `Sugestao ${id}`,
    description: 'Descricao',
    status: 'Open',
    voteCount: 0,
    commentCount: 0,
    hasVoted: false,
    endUserName: 'Cliente',
    createdAt: new Date().toISOString()
  };
}

const config: WidgetConfig = {
  boardSlug: 'roadmap',
  endUserToken: 'user-token',
  endUserName: 'Cliente',
  apiBaseUrl: 'http://localhost:5000'
};
