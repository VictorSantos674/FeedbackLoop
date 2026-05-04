import type { Post } from '../types';
import { StatusBadge } from './StatusBadge';

type PostCardProps = {
  post: Post;
  onVote: (post: Post) => void;
  isVoting?: boolean;
};

export function PostCard({ post, onVote, isVoting = false }: PostCardProps) {
  return (
    <article className="fl-card">
      <button
        type="button"
        className={`fl-vote ${post.hasVoted ? 'is-active' : ''}`}
        aria-label={post.hasVoted ? 'Remover voto' : 'Votar'}
        disabled={isVoting}
        onClick={() => onVote(post)}
      >
        <span aria-hidden="true">▲</span>
        <span>{post.voteCount}</span>
      </button>
      <div>
        <h3 className="fl-card__title">{post.title}</h3>
        {post.description ? <p className="fl-card__description">{post.description}</p> : null}
        <div className="fl-card__meta">
          <StatusBadge status={post.status} />
          <span>·</span>
          <span>{post.commentCount} comentarios</span>
        </div>
      </div>
    </article>
  );
}
