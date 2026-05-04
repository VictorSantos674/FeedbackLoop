import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import { LoginPage } from '../pages/LoginPage';
import { authApi } from '../api/auth';

describe('LoginPage', () => {
  it('renderiza formulario corretamente', () => {
    renderLogin();

    expect(screen.getByLabelText('E-mail')).toBeInTheDocument();
    expect(screen.getByLabelText('Senha')).toBeInTheDocument();
  });

  it('exibe erro de validacao com e-mail invalido', async () => {
    renderLogin();

    await userEvent.type(screen.getByLabelText('E-mail'), 'email-invalido');
    await userEvent.type(screen.getByLabelText('Senha'), '123456');
    await userEvent.click(screen.getByRole('button', { name: 'Entrar' }));

    expect(await screen.findByText('E-mail invalido')).toBeInTheDocument();
  });

  it('desabilita botao durante loading', async () => {
    vi.spyOn(authApi, 'login').mockImplementation(() => new Promise(() => undefined));
    renderLogin();

    await userEvent.type(screen.getByLabelText('E-mail'), 'admin@example.com');
    await userEvent.type(screen.getByLabelText('Senha'), '123456');
    await userEvent.click(screen.getByRole('button', { name: 'Entrar' }));

    await waitFor(() => expect(screen.getByRole('button')).toBeDisabled());
  });
});

function renderLogin() {
  const queryClient = new QueryClient({
    defaultOptions: { mutations: { retry: false }, queries: { retry: false } }
  });
  render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    </QueryClientProvider>
  );
}
