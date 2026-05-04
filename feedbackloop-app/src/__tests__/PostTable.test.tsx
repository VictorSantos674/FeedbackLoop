import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import { PostTable } from '../components/posts/PostTable';
import { postsApi } from '../api/posts';
import type { Post } from '../types';

describe('PostTable', () => {
  it('renderiza lista de posts mockada', () => {
    renderTable();

    expect(screen.getByText('Adicionar exportacao CSV')).toBeInTheDocument();
  });

  it('clique em linha navega para detalhe', async () => {
    renderTable();

    await userEvent.click(screen.getByText('Adicionar exportacao CSV'));

    expect(screen.getByTestId('location')).toHaveTextContent('/boards/board-1/posts/post-1');
  });

  it('StatusSelect dispara mutation correta', async () => {
    const spy = vi.spyOn(postsApi, 'updateStatus').mockResolvedValue({ ...post, status: 'Done' });
    renderTable();

    await userEvent.selectOptions(screen.getByDisplayValue('Open'), 'Done');

    expect(spy).toHaveBeenCalledWith('board-1', 'post-1', 'Done');
  });
});

function renderTable() {
  const queryClient = new QueryClient({
    defaultOptions: { mutations: { retry: false }, queries: { retry: false } }
  });

  render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={['/boards/board-1']}>
        <Routes>
          <Route
            path="/boards/:boardId"
            element={
              <>
                <LocationProbe />
                <PostTable boardId="board-1" posts={[post]} />
              </>
            }
          />
          <Route path="/boards/:boardId/posts/:postId" element={<LocationProbe />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>
  );
}

function LocationProbe() {
  const location = useLocation();
  return <span data-testid="location">{location.pathname}</span>;
}

const post: Post = {
  id: 'post-1',
  title: 'Adicionar exportacao CSV',
  description: 'Exportar dados',
  status: 'Open',
  voteCount: 3,
  commentCount: 0,
  hasVoted: null,
  endUserName: 'Cliente',
  createdAt: new Date().toISOString()
};
