import { Link } from 'react-router-dom';
import type { BoardSummary } from '../../types';
import { Card } from '../ui/Card';

export function BoardCard({ board }: { board: BoardSummary }) {
  return (
    <Link to={`/boards/${board.id}`}>
      <Card className="transition hover:border-brand-300 hover:shadow-md">
        <div className="flex items-start justify-between">
          <div>
            <h3 className="font-bold text-slate-900">{board.name}</h3>
            <p className="mt-1 text-sm text-slate-500">/{board.slug}</p>
          </div>
          <span className="rounded-full bg-brand-50 px-3 py-1 text-xs font-bold text-brand-700">{board.postCount} posts</span>
        </div>
      </Card>
    </Link>
  );
}
