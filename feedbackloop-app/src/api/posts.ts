import { apiClient } from './client';
import type { PagedResult, Post, PostFilter, PostStatus, StatusHistoryEntry } from '../types';

export const postsApi = {
  async getByBoard(boardId: string, filter: PostFilter) {
    const response = await apiClient.get<PagedResult<Post>>(`/api/boards/${boardId}/posts`, { params: filter });
    return response.data;
  },
  async getById(boardId: string, postId: string) {
    const response = await apiClient.get<Post>(`/api/boards/${boardId}/posts/${postId}`);
    return response.data;
  },
  async updateStatus(boardId: string, postId: string, status: PostStatus) {
    const response = await apiClient.patch<Post>(`/api/boards/${boardId}/posts/${postId}/status`, { status });
    return response.data;
  },
  async getHistory(boardId: string, postId: string) {
    const response = await apiClient.get<StatusHistoryEntry[]>(`/api/boards/${boardId}/posts/${postId}/history`);
    return response.data;
  }
};
