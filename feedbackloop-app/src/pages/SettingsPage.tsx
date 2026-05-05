import { useEffect, useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { Card } from '../components/ui/Card';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { Toast } from '../components/ui/Toast';
import { useBoards } from '../hooks/useBoards';
import { useCurrentWorkspace, useUpdateWorkspace } from '../hooks/useWorkspace';
import { useAuthStore } from '../store/authStore';

const workspaceSchema = z.object({
  name: z.string().min(2, 'Minimo 2 caracteres').max(50, 'Maximo 50 caracteres')
});

type WorkspaceForm = z.infer<typeof workspaceSchema>;

export function SettingsPage() {
  const user = useAuthStore((state) => state.user);
  const { data: boards = [] } = useBoards();
  const { data: workspace } = useCurrentWorkspace();
  const updateWorkspace = useUpdateWorkspace();
  const [selectedBoardId, setSelectedBoardId] = useState('');
  const [toast, setToast] = useState<string | null>(null);
  const form = useForm<WorkspaceForm>({
    resolver: zodResolver(workspaceSchema),
    defaultValues: { name: '' }
  });
  const board = boards.find((item) => item.id === selectedBoardId) ?? boards[0];
  const snippet = useMemo(() => board ? createSnippet(board.slug) : '', [board]);

  useEffect(() => {
    if (workspace) {
      form.reset({ name: workspace.name });
    }
  }, [form, workspace]);

  async function onSubmit(data: WorkspaceForm) {
    try {
      await updateWorkspace.mutateAsync(data);
      setToast('Workspace atualizado com sucesso.');
    } catch {
      form.setError('root', {
        type: 'manual',
        message: 'Nao foi possivel atualizar o workspace.'
      });
    }
  }

  return (
    <section className="space-y-6">
      <h1 className="text-2xl font-black">Settings</h1>
      <Card>
        <h2 className="font-bold">Workspace</h2>
        <form className="mt-4 grid max-w-md gap-4" onSubmit={form.handleSubmit(onSubmit)}>
          <Input label="Nome do workspace" error={form.formState.errors.name?.message} {...form.register('name')} />
          {form.formState.errors.root?.message ? <p className="text-sm font-semibold text-red-600">{form.formState.errors.root.message}</p> : null}
          <Button type="submit" disabled={updateWorkspace.isPending || !form.formState.isDirty} className="w-fit">
            {updateWorkspace.isPending ? (
              <>
                <span className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
                Salvando...
              </>
            ) : (
              'Salvar workspace'
            )}
          </Button>
        </form>
      </Card>
      <Card>
        <div className="mb-4 flex items-center justify-between">
          <h2 className="font-bold">Snippet de instalacao</h2>
          <select className="rounded-md border border-slate-200 px-3 py-2 text-sm" value={board?.id ?? ''} onChange={(event) => setSelectedBoardId(event.target.value)}>
            {boards.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}
          </select>
        </div>
        <pre className="overflow-x-auto rounded-md bg-slate-950 p-4 text-xs text-slate-100">{snippet}</pre>
      </Card>
      <Card>
        <h2 className="font-bold">Membros</h2>
        <div className="mt-3 rounded-md border border-slate-200 p-3 text-sm">
          <strong>{user?.name}</strong>
          <p className="text-slate-500">{user?.email} · {user?.role}</p>
        </div>
      </Card>
      <Toast message={toast} />
    </section>
  );
}

function createSnippet(slug: string) {
  return `<script src="https://cdn.feedbackloop.app/widget.umd.js"></script>
<script>
  FeedbackLoop.init({
    boardSlug: "${slug}",
    endUserToken: "SEU_USER_TOKEN_AQUI",
    endUserName: "Nome do Usuario",
    apiBaseUrl: "https://api.feedbackloop.app"
  });
</script>`;
}
