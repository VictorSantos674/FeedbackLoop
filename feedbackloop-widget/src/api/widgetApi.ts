import type { CreatePostPayload, PagedResult, Post, PostFilter, VoteResult } from '../types';

type WidgetApiOptions = {
  apiBaseUrl: string;
  boardSlug: string;
};

export class WidgetApiError extends Error {
  constructor(
    message: string,
    public readonly status: number
  ) {
    super(message);
  }
}

export async function fetchPosts(
  options: WidgetApiOptions,
  endUserToken: string,
  filter: PostFilter
): Promise<PagedResult<Post>> {
  const url = createWidgetUrl(options, '/posts');
  url.searchParams.set('endUserToken', endUserToken);
  if (filter.status) url.searchParams.set('status', filter.status);
  if (filter.sortBy) url.searchParams.set('sortBy', filter.sortBy);
  if (filter.page) url.searchParams.set('page', String(filter.page));
  if (filter.pageSize) url.searchParams.set('pageSize', String(filter.pageSize));

  return request<PagedResult<Post>>(url);
}

export async function createPost(options: WidgetApiOptions, payload: CreatePostPayload): Promise<Post> {
  return request<Post>(createWidgetUrl(options, '/posts'), {
    method: 'POST',
    body: JSON.stringify(payload)
  });
}

export async function toggleVote(options: WidgetApiOptions, postId: string, endUserToken: string): Promise<VoteResult> {
  return request<VoteResult>(createWidgetUrl(options, `/posts/${postId}/vote`), {
    method: 'POST',
    body: JSON.stringify({ endUserToken })
  });
}

async function request<T>(url: URL, init?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...init?.headers
    }
  });

  if (!response.ok) {
    let message = 'Nao foi possivel concluir a operacao.';
    try {
      const body = (await response.json()) as { message?: string; error?: string };
      message = body.message ?? body.error ?? message;
    } catch {
      // The API may return an empty body for infrastructure failures.
    }

    throw new WidgetApiError(message, response.status);
  }

  return (await response.json()) as T;
}

function createWidgetUrl(options: WidgetApiOptions, path: string): URL {
  const base = options.apiBaseUrl.replace(/\/$/, '');
  return new URL(`/api/widget/${encodeURIComponent(options.boardSlug)}${path}`, base);
}
