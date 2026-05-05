import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { workspacesApi } from '../api/workspaces';
import type { UpdateWorkspaceRequest } from '../types';

export function useCurrentWorkspace() {
  return useQuery({
    queryKey: ['workspace', 'current'],
    queryFn: workspacesApi.getCurrent
  });
}

export function useUpdateWorkspace() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateWorkspaceRequest) => workspacesApi.updateCurrent(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workspace', 'current'] });
    }
  });
}
