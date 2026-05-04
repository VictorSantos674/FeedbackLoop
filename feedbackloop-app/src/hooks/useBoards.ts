import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { boardsApi } from '../api/boards';
import type { CreateBoardRequest } from '../types';

export function useBoards() {
  return useQuery({
    queryKey: ['boards'],
    queryFn: boardsApi.getAll
  });
}

export function useBoard(boardId?: string) {
  return useQuery({
    queryKey: ['board', boardId],
    queryFn: () => boardsApi.getById(boardId!),
    enabled: Boolean(boardId)
  });
}

export function useCreateBoard() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateBoardRequest) => boardsApi.create(data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['boards'] })
  });
}
