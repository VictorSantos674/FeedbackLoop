import { apiClient } from './client';
import type { UpdateWorkspaceRequest, Workspace } from '../types';

export const workspacesApi = {
  async getCurrent() {
    const response = await apiClient.get<Workspace>('/api/workspaces/current');
    return response.data;
  },
  async updateCurrent(data: UpdateWorkspaceRequest) {
    const response = await apiClient.patch<Workspace>('/api/workspaces/current', data);
    return response.data;
  }
};
