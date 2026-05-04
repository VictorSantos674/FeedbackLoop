import type { PostFilter, PostStatus, WidgetConfig } from '../types';
import { usePosts } from '../hooks/usePosts';
import { useVote } from '../hooks/useVote';
import { PostCard } from './PostCard';

type PostListProps = {
  config: WidgetConfig;
  postsState: ReturnType<typeof usePosts>;
  filter: PostFilter;
  onFilterChange: (filter: PostFilter) => void;
  onToast: (message: string) => void;
};

const statuses: Array<{ value: '' | PostStatus; label: string }> = [
  { value: '', label: 'Todos' },
  { value: 'Open', label: 'Aberto' },
  { value: 'UnderReview', label: 'Em analise' },
  { value: 'Planned', label: 'Planejado' },
  { value: 'InProgress', label: 'Em progresso' },
  { value: 'Done', label: 'Concluido' },
  { value: 'Declined', label: 'Recusado' }
];

export function PostList({ config, postsState, filter, onFilterChange, onToast }: PostListProps) {
  const vote = useVote(config, postsState.updatePost, onToast);

  return (
    <section className="fl-post-list">
      <div className="fl-filters">
        <select
          className="fl-select"
          aria-label="Filtrar por status"
          value={filter.status ?? ''}
          onChange={(event) =>
            onFilterChange({ ...filter, status: event.target.value ? (event.target.value as PostStatus) : undefined })
          }
        >
          {statuses.map((status) => (
            <option key={status.value || 'all'} value={status.value}>
              {status.label}
            </option>
          ))}
        </select>
        <select
          className="fl-select"
          aria-label="Ordenar sugestoes"
          value={filter.sortBy ?? 'MostVoted'}
          onChange={(event) => onFilterChange({ ...filter, sortBy: event.target.value as PostFilter['sortBy'] })}
        >
          <option value="MostVoted">Mais votadas</option>
          <option value="Newest">Mais recentes</option>
          <option value="Oldest">Mais antigas</option>
          <option value="RecentlyUpdated">Atualizadas</option>
        </select>
      </div>

      {postsState.isLoading ? <SkeletonList /> : null}

      {!postsState.isLoading && postsState.error ? (
        <div className="fl-state">
          <strong>Nao foi possivel carregar</strong>
          <span>{postsState.error}</span>
          <button className="fl-secondary-button" type="button" onClick={postsState.refresh}>
            Tentar novamente
          </button>
        </div>
      ) : null}

      {!postsState.isLoading && !postsState.error && postsState.posts.length === 0 ? <EmptyState /> : null}

      {!postsState.isLoading && !postsState.error
        ? postsState.posts.map((post) => (
            <PostCard
              key={post.id}
              post={post}
              isVoting={vote.loadingPostId === post.id}
              onVote={() => vote.toggle(post.id, post.hasVoted, post.voteCount)}
            />
          ))
        : null}

      {postsState.hasMore && !postsState.error ? (
        <button className="fl-secondary-button" type="button" disabled={postsState.isLoadingMore} onClick={postsState.loadMore}>
          {postsState.isLoadingMore ? 'Carregando...' : 'Carregar mais'}
        </button>
      ) : null}
    </section>
  );
}

function SkeletonList() {
  return (
    <>
      <div className="fl-skeleton" />
      <div className="fl-skeleton" />
      <div className="fl-skeleton" />
    </>
  );
}

function EmptyState() {
  return (
    <div className="fl-state">
      <svg viewBox="0 0 120 90" role="img" aria-label="Sem sugestoes">
        <rect x="18" y="14" width="84" height="56" rx="10" fill="var(--fl-surface)" stroke="var(--fl-border)" />
        <path d="M38 35h44M38 48h28" stroke="var(--fl-muted)" strokeWidth="5" strokeLinecap="round" />
        <circle cx="92" cy="64" r="13" fill="var(--fl-primary)" />
        <path d="M92 57v14M85 64h14" stroke="var(--fl-primary-contrast)" strokeWidth="4" strokeLinecap="round" />
      </svg>
      <strong>Nenhuma sugestao ainda.</strong>
      <span>Seja o primeiro!</span>
    </div>
  );
}
