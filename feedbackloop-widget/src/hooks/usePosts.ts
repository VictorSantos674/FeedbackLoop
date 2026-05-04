import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { createPost, fetchPosts, WidgetApiError } from '../api/widgetApi';
import type { CreatePostPayload, Post, PostFilter, WidgetConfig } from '../types';

type PostsCacheEntry = {
  posts: Post[];
  totalCount: number;
  page: number;
  totalPages: number;
};

const postsCache = new Map<string, PostsCacheEntry>();

export function clearPostsCache() {
  postsCache.clear();
}

export function usePosts(config: WidgetConfig, filter: PostFilter) {
  const normalizedFilter = useMemo(
    () => ({
      status: filter.status,
      sortBy: filter.sortBy ?? 'MostVoted',
      pageSize: filter.pageSize ?? 20
    }),
    [filter.status, filter.sortBy, filter.pageSize]
  );
  const [posts, setPosts] = useState<Post[]>([]);
  const [page, setPage] = useState(filter.page ?? 1);
  const [totalPages, setTotalPages] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const cacheKey = useMemo(
    () => buildCacheKey(config.boardSlug, config.endUserToken, normalizedFilter),
    [config.boardSlug, config.endUserToken, normalizedFilter]
  );
  const requestIdRef = useRef(0);

  const loadPage = useCallback(
    async (targetPage: number, mode: 'replace' | 'append') => {
      const requestId = requestIdRef.current + 1;
      requestIdRef.current = requestId;
      mode === 'append' ? setIsLoadingMore(true) : setIsLoading(true);
      setError(null);

      try {
        const result = await fetchPosts(
          { apiBaseUrl: config.apiBaseUrl, boardSlug: config.boardSlug },
          config.endUserToken,
          { ...normalizedFilter, page: targetPage }
        );

        if (requestIdRef.current !== requestId) return;

        setPosts((current) => {
          const next = mode === 'append' ? mergePosts(current, result.items) : result.items;
          postsCache.set(cacheKey, {
            posts: next,
            totalCount: result.totalCount,
            page: result.page,
            totalPages: result.totalPages
          });
          return next;
        });
        setPage(result.page);
        setTotalPages(result.totalPages);
      } catch (caught) {
        if (requestIdRef.current !== requestId) return;
        setError(caught instanceof Error ? caught.message : 'Nao foi possivel carregar as sugestoes.');
      } finally {
        if (requestIdRef.current === requestId) {
          setIsLoading(false);
          setIsLoadingMore(false);
        }
      }
    },
    [cacheKey, config.apiBaseUrl, config.boardSlug, config.endUserToken, normalizedFilter]
  );

  useEffect(() => {
    const cached = postsCache.get(cacheKey);
    if (cached) {
      setPosts(cached.posts);
      setPage(cached.page);
      setTotalPages(cached.totalPages);
      setIsLoading(false);
      setError(null);
      return;
    }

    setPosts([]);
    setPage(1);
    setTotalPages(0);
    void loadPage(1, 'replace');
  }, [cacheKey, loadPage]);

  const loadMore = useCallback(async () => {
    if (isLoadingMore || page >= totalPages) return;
    await loadPage(page + 1, 'append');
  }, [isLoadingMore, loadPage, page, totalPages]);

  const refresh = useCallback(async () => {
    postsCache.delete(cacheKey);
    await loadPage(1, 'replace');
  }, [cacheKey, loadPage]);

  const invalidate = useCallback(() => {
    postsCache.delete(cacheKey);
  }, [cacheKey]);

  const addPost = useCallback(
    async (payload: CreatePostPayload) => {
      try {
        const created = await createPost(
          { apiBaseUrl: config.apiBaseUrl, boardSlug: config.boardSlug },
          payload
        );
        setPosts((current) => {
          const next = [created, ...current.filter((post) => post.id !== created.id)];
          postsCache.delete(cacheKey);
          return next;
        });
        return created;
      } catch (caught) {
        if (caught instanceof WidgetApiError && (caught.status === 429 || caught.message.toLowerCase().includes('limit'))) {
          throw new Error('Você atingiu o limite de sugestões por hoje');
        }
        throw caught;
      }
    },
    [cacheKey, config.apiBaseUrl, config.boardSlug]
  );

  const updatePost = useCallback((postId: string, updater: (post: Post) => Post) => {
    setPosts((current) => current.map((post) => (post.id === postId ? updater(post) : post)));
  }, []);

  return {
    posts,
    setPosts,
    updatePost,
    addPost,
    isLoading,
    isLoadingMore,
    error,
    hasMore: page < totalPages,
    loadMore,
    refresh,
    invalidate
  };
}

function buildCacheKey(boardSlug: string, endUserToken: string, filter: PostFilter) {
  return JSON.stringify({
    boardSlug,
    endUserToken,
    status: filter.status ?? null,
    sortBy: filter.sortBy ?? 'MostVoted',
    pageSize: filter.pageSize ?? 20
  });
}

function mergePosts(current: Post[], incoming: Post[]) {
  const seen = new Set(current.map((post) => post.id));
  return [...current, ...incoming.filter((post) => !seen.has(post.id))];
}
