import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { Modal } from '../ui/Modal';
import { Input } from '../ui/Input';
import { Button } from '../ui/Button';
import { useCreateBoard } from '../../hooks/useBoards';

const schema = z.object({
  name: z.string().min(3, 'Minimo 3 caracteres'),
  description: z.string().optional(),
  isPublic: z.boolean()
});

type FormData = z.infer<typeof schema>;

export function CreateBoardModal({ onClose }: { onClose: () => void }) {
  const createBoard = useCreateBoard();
  const form = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', description: '', isPublic: true }
  });

  async function onSubmit(data: FormData) {
    await createBoard.mutateAsync(data);
    onClose();
  }

  return (
    <Modal title="Novo board" onClose={onClose}>
      <form className="grid gap-4" onSubmit={form.handleSubmit(onSubmit)}>
        <Input label="Nome" error={form.formState.errors.name?.message} {...form.register('name')} />
        <label className="grid gap-1.5 text-sm font-medium text-slate-700">
          Descricao
          <textarea className="min-h-24 rounded-md border border-slate-200 px-3 py-2" {...form.register('description')} />
        </label>
        <label className="flex items-center gap-2 text-sm font-medium">
          <input type="checkbox" {...form.register('isPublic')} /> Publico
        </label>
        <Button type="submit" disabled={createBoard.isPending}>Criar board</Button>
      </form>
    </Modal>
  );
}
