import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { NewPostForm } from '../components/NewPostForm';
import type { Post, WidgetConfig } from '../types';

describe('NewPostForm', () => {
  it('botao desabilitado com titulo menor que 10 caracteres', async () => {
    renderForm();

    await userEvent.type(screen.getByLabelText('Titulo'), 'Curto');

    expect(screen.getByRole('button', { name: 'Enviar' })).toBeDisabled();
  });

  it('validacao exibe mensagem de erro no blur', async () => {
    renderForm();

    const input = screen.getByLabelText('Titulo');
    await userEvent.type(input, 'Curto');
    await userEvent.tab();

    expect(screen.getByText('O titulo precisa ter pelo menos 10 caracteres.')).toBeInTheDocument();
  });

  it('submit bem-sucedido limpa o form', async () => {
    const postsState = createPostsState();
    renderForm(postsState);

    await userEvent.type(screen.getByLabelText('Titulo'), 'Adicionar exportacao CSV');
    await userEvent.type(screen.getByLabelText('Descricao'), 'Exportar dados ajuda o time.');
    await userEvent.click(screen.getByRole('button', { name: 'Enviar' }));

    await waitFor(() => expect(postsState.addPost).toHaveBeenCalled());
    expect(screen.getByLabelText('Titulo')).toHaveValue('');
    expect(screen.getByLabelText('Descricao')).toHaveValue('');
  });
});

function renderForm(postsState = createPostsState()) {
  render(
    <NewPostForm
      config={config}
      postsState={postsState}
      onCreated={vi.fn()}
      onToast={vi.fn()}
    />
  );
}

function createPostsState() {
  return {
    posts: [],
    setPosts: vi.fn(),
    updatePost: vi.fn(),
    addPost: vi.fn(async () => post),
    isLoading: false,
    isLoadingMore: false,
    error: null,
    hasMore: false,
    loadMore: vi.fn(),
    refresh: vi.fn(),
    invalidate: vi.fn()
  };
}

const config: WidgetConfig = {
  boardSlug: 'roadmap',
  endUserToken: 'user-token',
  endUserName: 'Cliente',
  apiBaseUrl: 'http://localhost:5000'
};

const post: Post = {
  id: 'post-1',
  title: 'Adicionar exportacao CSV',
  description: 'Exportar dados ajuda o time.',
  status: 'Open',
  voteCount: 0,
  commentCount: 0,
  hasVoted: false,
  endUserName: 'Cliente',
  createdAt: new Date().toISOString()
};
