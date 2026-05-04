import { useNavigate } from 'react-router-dom';
import type { Post } from '../../types';
import { Badge } from '../ui/Badge';
import { StatusSelect } from './StatusSelect';

export function PostTable({ boardId, posts }: { boardId: string; posts: Post[] }) {
  const navigate = useNavigate();

  return (
    <div className="overflow-hidden rounded-lg border border-slate-200 bg-white">
      <table className="w-full border-collapse text-left text-sm">
        <thead className="bg-slate-50 text-xs uppercase tracking-wide text-slate-500">
          <tr>
            <th className="px-4 py-3">Titulo</th>
            <th className="px-4 py-3">Status</th>
            <th className="px-4 py-3">Votos</th>
            <th className="px-4 py-3">Data</th>
            <th className="px-4 py-3">Acoes</th>
          </tr>
        </thead>
        <tbody>
          {posts.map((post) => (
            <tr key={post.id} className="cursor-pointer border-t border-slate-100 hover:bg-slate-50" onClick={() => navigate(`/boards/${boardId}/posts/${post.id}`)}>
              <td className="px-4 py-3 font-semibold text-slate-900">{post.title}</td>
              <td className="px-4 py-3"><Badge status={post.status} /></td>
              <td className="px-4 py-3">{post.voteCount}</td>
              <td className="px-4 py-3 text-slate-500">{new Date(post.createdAt).toLocaleDateString()}</td>
              <td className="px-4 py-3"><StatusSelect boardId={boardId} postId={post.id} status={post.status} /></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
