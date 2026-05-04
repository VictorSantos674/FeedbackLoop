import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { postsApi } from '../api/posts';
import type { PostFilter, PostStatus } from '../types';

export function usePosts(boardId: string | undefined, filter: PostFilter) {
  return useQuery({
    queryKey: ['posts', boardId, filter],
    queryFn: () => postsApi.getByBoard(boardId!, filter),
    enabled: Boolean(boardId)
  });
}

export function usePost(boardId?: string, postId?: string) {
  return useQuery({
    queryKey: ['post', boardId, postId],
    queryFn: () => postsApi.getById(boardId!, postId!),
    enabled: Boolean(boardId && postId)
  });
}

export function useStatusHistory(boardId?: string, postId?: string) {
  return useQuery({
    queryKey: ['post-history', boardId, postId],
    queryFn: () => postsApi.getHistory(boardId!, postId!),
    enabled: Boolean(boardId && postId)
  });
}

export function useUpdateStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ boardId, postId, status }: { boardId: string; postId: string; status: PostStatus }) =>
      postsApi.updateStatus(boardId, postId, status),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['posts', variables.boardId] });
      queryClient.invalidateQueries({ queryKey: ['post', variables.boardId, variables.postId] });
      queryClient.invalidateQueries({ queryKey: ['post-history', variables.boardId, variables.postId] });
    }
  });
}
