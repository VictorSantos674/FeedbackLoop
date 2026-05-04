import { useMemo, useState } from 'react';
import { Card } from '../components/ui/Card';
import { useBoards } from '../hooks/useBoards';
import { useAuthStore } from '../store/authStore';

export function SettingsPage() {
  const user = useAuthStore((state) => state.user);
  const { data: boards = [] } = useBoards();
  const [selectedBoardId, setSelectedBoardId] = useState('');
  const board = boards.find((item) => item.id === selectedBoardId) ?? boards[0];
  const snippet = useMemo(() => board ? createSnippet(board.slug) : '', [board]);

  return (
    <section className="space-y-6">
      <h1 className="text-2xl font-black">Settings</h1>
      <Card>
        <h2 className="font-bold">Workspace</h2>
        <p className="mt-2 text-sm text-slate-500">Edicao de nome vira no proximo ciclo da API.</p>
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
