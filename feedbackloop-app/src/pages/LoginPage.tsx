import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { Input } from '../components/ui/Input';
import { Button } from '../components/ui/Button';
import { Card } from '../components/ui/Card';
import { useLogin } from '../hooks/useAuth';

const loginSchema = z.object({
  email: z.string().email('E-mail invalido'),
  password: z.string().min(6, 'Minimo 6 caracteres')
});

type LoginForm = z.infer<typeof loginSchema>;

export function LoginPage() {
  const navigate = useNavigate();
  const login = useLogin();
  const form = useForm<LoginForm>({ resolver: zodResolver(loginSchema), defaultValues: { email: '', password: '' } });

  async function onSubmit(data: LoginForm) {
    await login.mutateAsync(data);
    navigate('/');
  }

  return (
    <main className="grid min-h-screen place-items-center bg-slate-50 p-6">
      <Card className="w-full max-w-md">
        <h1 className="text-2xl font-black text-slate-900">Entrar no FeedbackLoop</h1>
        <form className="mt-6 grid gap-4" noValidate onSubmit={form.handleSubmit(onSubmit)}>
          <Input label="E-mail" type="email" error={form.formState.errors.email?.message} {...form.register('email')} />
          <Input label="Senha" type="password" error={form.formState.errors.password?.message} {...form.register('password')} />
          {login.isError ? <p className="text-sm font-semibold text-red-600">Credenciais invalidas.</p> : null}
          <Button type="submit" disabled={login.isPending}>{login.isPending ? 'Entrando...' : 'Entrar'}</Button>
        </form>
        <p className="mt-4 text-sm text-slate-500">
          Ainda nao tem conta? <Link className="font-bold text-brand-700" to="/register">Criar workspace</Link>
        </p>
      </Card>
    </main>
  );
}
