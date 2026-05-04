import { useMemo, useState } from 'react';
import { useParams, useSearchParams } from 'react-router-dom';
import { Copy } from 'lucide-react';
import { PostFilters } from '../components/posts/PostFilters';
import { PostTable } from '../components/posts/PostTable';
import { Button } from '../components/ui/Button';
import { Card } from '../components/ui/Card';
import { Modal } from '../components/ui/Modal';
import { Skeleton } from '../components/ui/Skeleton';
import { useBoard } from '../hooks/useBoards';
import { usePosts } from '../hooks/usePosts';
import type { PostFilter, PostStatus } from '../types';

export function BoardPage() {
  const { boardId } = useParams();
  const [params, setParams] = useSearchParams();
  const [showSnippet, setShowSnippet] = useState(false);
  const filter = useMemo<PostFilter>(() => ({
    status: (params.get('status') as PostStatus | null) ?? undefined,
    sortBy: (params.get('sortBy') as PostFilter['sortBy']) ?? 'MostVoted',
    page: Number(params.get('page') ?? 1),
    pageSize: 20
  }), [params]);
  const { data: board } = useBoard(boardId);
  const { data, isLoading } = usePosts(boardId, filter);

  function updateFilter(next: PostFilter) {
    const nextParams = new URLSearchParams();
    if (next.status) nextParams.set('status', next.status);
    if (next.sortBy) nextParams.set('sortBy', next.sortBy);
    if (next.page && next.page > 1) nextParams.set('page', String(next.page));
    setParams(nextParams);
  }

  const snippet = board ? createSnippet(board.slug) : '';

  return (
    <section className="space-y-5">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black">{board?.name ?? 'Board'}</h1>
          <p className="text-slate-500">Gerencie sugestoes recebidas pelo widget.</p>
        </div>
        <Button variant="secondary" onClick={() => setShowSnippet(true)}><Copy size={16} /> Copiar snippet</Button>
      </div>
      <Card className="space-y-4">
        <PostFilters filter={filter} onChange={updateFilter} />
        {isLoading ? <Skeleton className="h-80" /> : <PostTable boardId={boardId!} posts={data?.items ?? []} />}
        <div className="flex items-center justify-between text-sm text-slate-500">
          <span>Pagina {data?.page ?? 1} de {data?.totalPages ?? 0}</span>
          <div className="flex gap-2">
            <Button variant="secondary" disabled={(filter.page ?? 1) <= 1} onClick={() => updateFilter({ ...filter, page: (filter.page ?? 1) - 1 })}>Anterior</Button>
            <Button variant="secondary" disabled={!data || data.page >= data.totalPages} onClick={() => updateFilter({ ...filter, page: (filter.page ?? 1) + 1 })}>Proxima</Button>
          </div>
        </div>
      </Card>
      {showSnippet ? (
        <Modal title="Snippet de instalacao" onClose={() => setShowSnippet(false)}>
          <pre className="overflow-x-auto rounded-md bg-slate-950 p-4 text-xs text-slate-100">{snippet}</pre>
        </Modal>
      ) : null}
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
