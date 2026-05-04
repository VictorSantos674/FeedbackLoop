import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { Input } from '../components/ui/Input';
import { Button } from '../components/ui/Button';
import { Card } from '../components/ui/Card';
import { useRegister } from '../hooks/useAuth';

const schema = z.object({
  name: z.string().min(2, 'Informe seu nome'),
  email: z.string().email('E-mail invalido'),
  password: z.string().min(8, 'Minimo 8 caracteres'),
  workspaceName: z.string().min(2, 'Informe o workspace')
});

type RegisterForm = z.infer<typeof schema>;

export function RegisterPage() {
  const navigate = useNavigate();
  const register = useRegister();
  const form = useForm<RegisterForm>({ resolver: zodResolver(schema), defaultValues: { name: '', email: '', password: '', workspaceName: '' } });

  async function onSubmit(data: RegisterForm) {
    await register.mutateAsync(data);
    navigate('/login', { state: { message: 'Workspace criado com sucesso.' } });
  }

  return (
    <main className="grid min-h-screen place-items-center bg-slate-50 p-6">
      <Card className="w-full max-w-md">
        <h1 className="text-2xl font-black text-slate-900">Criar workspace</h1>
        <form className="mt-6 grid gap-4" noValidate onSubmit={form.handleSubmit(onSubmit)}>
          <Input label="Nome" error={form.formState.errors.name?.message} {...form.register('name')} />
          <Input label="E-mail" type="email" error={form.formState.errors.email?.message} {...form.register('email')} />
          <Input label="Senha" type="password" error={form.formState.errors.password?.message} {...form.register('password')} />
          <Input label="Workspace" error={form.formState.errors.workspaceName?.message} {...form.register('workspaceName')} />
          <Button type="submit" disabled={register.isPending}>{register.isPending ? 'Criando...' : 'Criar conta'}</Button>
        </form>
        <p className="mt-4 text-sm text-slate-500">
          Ja tem conta? <Link className="font-bold text-brand-700" to="/login">Entrar</Link>
        </p>
      </Card>
    </main>
  );
}
