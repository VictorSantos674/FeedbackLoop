import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { PostCard } from '../components/PostCard';
import type { Post } from '../types';

describe('PostCard', () => {
  it('renderiza voteCount corretamente', () => {
    render(<PostCard post={createPost({ voteCount: 42 })} onVote={vi.fn()} />);

    expect(screen.getByText('42')).toBeInTheDocument();
  });

  it('clique no voto dispara useVote.toggle', async () => {
    const onVote = vi.fn();
    const post = createPost();

    render(<PostCard post={post} onVote={onVote} />);
    await userEvent.click(screen.getByRole('button', { name: 'Votar' }));

    expect(onVote).toHaveBeenCalledWith(post);
  });

  it('optimistic update: voteCount sobe imediatamente antes da resposta', async () => {
    function Harness() {
      const post = createPost({ voteCount: 3, hasVoted: false });
      const optimisticPost = { ...post, voteCount: post.voteCount + 1, hasVoted: true };
      return <PostCard post={optimisticPost} onVote={vi.fn()} />;
    }

    render(<Harness />);

    expect(screen.getByText('4')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Remover voto' })).toBeInTheDocument();
  });
});

function createPost(overrides: Partial<Post> = {}): Post {
  return {
    id: 'post-1',
    title: 'Adicionar exportacao CSV',
    description: 'Exportar dados ajuda o time.',
    status: 'Open',
    voteCount: 3,
    commentCount: 0,
    hasVoted: false,
    endUserName: 'Cliente',
    createdAt: new Date().toISOString(),
    ...overrides
  };
}
