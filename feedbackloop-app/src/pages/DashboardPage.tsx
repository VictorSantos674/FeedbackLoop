import { useMemo, useState } from 'react';
import { Plus } from 'lucide-react';
import { BoardCard } from '../components/boards/BoardCard';
import { CreateBoardModal } from '../components/boards/CreateBoardModal';
import { StatsCards } from '../components/dashboard/StatsCards';
import { VotesChart } from '../components/dashboard/VotesChart';
import { Button } from '../components/ui/Button';
import { Skeleton } from '../components/ui/Skeleton';
import { useBoards } from '../hooks/useBoards';
import { usePosts } from '../hooks/usePosts';
import { useAuthStore } from '../store/authStore';

export function DashboardPage() {
  const { data: boards = [], isLoading } = useBoards();
  const user = useAuthStore((state) => state.user);
  const [showCreateBoard, setShowCreateBoard] = useState(false);
  const firstBoard = boards[0];
  const { data: postsResult } = usePosts(firstBoard?.id, { pageSize: 100, sortBy: 'Newest' });
  const posts = useMemo(() => postsResult?.items ?? [], [postsResult]);

  return (
    <section className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black">Dashboard</h1>
          <p className="text-slate-500">Acompanhe feedbacks, votos e boards do workspace.</p>
        </div>
        {user?.role === 'Admin' ? <Button onClick={() => setShowCreateBoard(true)}><Plus size={16} /> Novo board</Button> : null}
      </div>
      <StatsCards posts={posts} />
      <VotesChart posts={posts} />
      <div>
        <h2 className="mb-3 font-bold">Boards</h2>
        {isLoading ? <Skeleton className="h-28" /> : <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">{boards.map((board) => <BoardCard key={board.id} board={board} />)}</div>}
      </div>
      {showCreateBoard ? <CreateBoardModal onClose={() => setShowCreateBoard(false)} /> : null}
    </section>
  );
}
