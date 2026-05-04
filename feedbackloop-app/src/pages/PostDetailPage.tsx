import { useParams } from 'react-router-dom';
import { Vote } from 'lucide-react';
import { Card } from '../components/ui/Card';
import { Skeleton } from '../components/ui/Skeleton';
import { StatusSelect } from '../components/posts/StatusSelect';
import { StatusHistoryList } from '../components/posts/StatusHistoryList';
import { usePost, useStatusHistory } from '../hooks/usePosts';

export function PostDetailPage() {
  const { boardId, postId } = useParams();
  const { data: post, isLoading } = usePost(boardId, postId);
  const { data: history = [] } = useStatusHistory(boardId, postId);

  if (isLoading) return <Skeleton className="h-96" />;
  if (!post || !boardId || !postId) return <p>Post nao encontrado.</p>;

  return (
    <section className="grid gap-6 xl:grid-cols-[1fr_360px]">
      <Card>
        <div className="flex items-start justify-between gap-4">
          <div>
            <h1 className="text-2xl font-black">{post.title}</h1>
            <p className="mt-2 text-sm text-slate-500">Criado por {post.endUserName} em {new Date(post.createdAt).toLocaleDateString()}</p>
          </div>
          <StatusSelect boardId={boardId} postId={postId} status={post.status} />
        </div>
        <p className="mt-6 whitespace-pre-wrap text-slate-700">{post.description || 'Sem descricao.'}</p>
        <div className="mt-6 inline-flex items-center gap-2 rounded-md bg-brand-50 px-3 py-2 font-bold text-brand-700">
          <Vote size={16} /> {post.voteCount} votos
        </div>
      </Card>
      <Card>
        <h2 className="mb-4 font-bold">Historico de status</h2>
        <StatusHistoryList entries={history} />
      </Card>
    </section>
  );
}
