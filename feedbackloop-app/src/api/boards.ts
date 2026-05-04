import { apiClient } from './client';
import type { BoardDetail, BoardSummary, CreateBoardRequest } from '../types';

export const boardsApi = {
  async getAll() {
    const response = await apiClient.get<BoardSummary[]>('/api/boards');
    return response.data;
  },
  async getById(id: string) {
    const response = await apiClient.get<BoardDetail>(`/api/boards/${id}`);
    return response.data;
  },
  async create(data: CreateBoardRequest) {
    const response = await apiClient.post<BoardDetail>('/api/boards', data);
    return response.data;
  },
  async update(id: string, data: CreateBoardRequest) {
    const response = await apiClient.put<BoardDetail>(`/api/boards/${id}`, data);
    return response.data;
  },
  async remove(id: string) {
    await apiClient.delete(`/api/boards/${id}`);
  }
};
