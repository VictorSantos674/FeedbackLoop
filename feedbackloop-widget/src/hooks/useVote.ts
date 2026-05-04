import { useCallback, useState } from 'react';
import { toggleVote as toggleVoteRequest } from '../api/widgetApi';
import type { Post, WidgetConfig } from '../types';

type UpdatePost = (postId: string, updater: (post: Post) => Post) => void;

export function useVote(config: WidgetConfig, updatePost: UpdatePost, onError?: (message: string) => void) {
  const [loadingPostId, setLoadingPostId] = useState<string | null>(null);

  const toggle = useCallback(
    async (postId: string, currentVoteState: boolean, currentCount: number) => {
      const optimistic = {
        hasVoted: !currentVoteState,
        voteCount: currentVoteState ? Math.max(0, currentCount - 1) : currentCount + 1
      };

      updatePost(postId, (post) => ({ ...post, ...optimistic }));
      setLoadingPostId(postId);

      try {
        const result = await toggleVoteRequest(
          { apiBaseUrl: config.apiBaseUrl, boardSlug: config.boardSlug },
          postId,
          config.endUserToken
        );
        updatePost(postId, (post) => ({ ...post, hasVoted: result.voted, voteCount: result.voteCount }));
      } catch (caught) {
        updatePost(postId, (post) => ({ ...post, hasVoted: currentVoteState, voteCount: currentCount }));
        onError?.(caught instanceof Error ? caught.message : 'Nao foi possivel registrar seu voto.');
      } finally {
        setLoadingPostId((current) => (current === postId ? null : current));
      }
    },
    [config.apiBaseUrl, config.boardSlug, config.endUserToken, onError, updatePost]
  );

  return { toggle, loadingPostId };
}
