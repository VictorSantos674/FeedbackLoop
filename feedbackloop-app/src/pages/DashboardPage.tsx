import { useMemo, useState } from 'react';
import { useQueries } from '@tanstack/react-query';
import { Plus } from 'lucide-react';
import { postsApi } from '../api/posts';
import { BoardCard } from '../components/boards/BoardCard';
import { CreateBoardModal } from '../components/boards/CreateBoardModal';
import { StatsCards } from '../components/dashboard/StatsCards';
import { VotesChart } from '../components/dashboard/VotesChart';
import { Button } from '../components/ui/Button';
import { Skeleton } from '../components/ui/Skeleton';
import { useBoards } from '../hooks/useBoards';
import { useAuthStore } from '../store/authStore';

export function DashboardPage() {
  const { data: boards = [], isLoading } = useBoards();
  const user = useAuthStore((state) => state.user);
  const [showCreateBoard, setShowCreateBoard] = useState(false);
  const postQueries = useQueries({
    queries: boards.map((board) => ({
      queryKey: ['posts', board.id, { pageSize: 100, sortBy: 'Newest' }],
      queryFn: () => postsApi.getByBoard(board.id, { pageSize: 100, sortBy: 'Newest' as const })
    }))
  });
  const stats = useMemo(() => ({
    totalPosts: boards.reduce((sum, board) => sum + board.postCount, 0),
    openPosts: boards.reduce((sum, board) => sum + board.openPostCount, 0),
    totalVotes: boards.reduce((sum, board) => sum + board.voteCount, 0),
    resolvedPosts: boards.reduce((sum, board) => sum + board.donePostCount, 0)
  }), [boards]);
  const posts = postQueries.flatMap((query) => query.data?.items ?? []);

  return (
    <section className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black">Dashboard</h1>
          <p className="text-slate-500">Acompanhe feedbacks, votos e boards do workspace.</p>
        </div>
        {user?.role === 'Admin' ? <Button onClick={() => setShowCreateBoard(true)}><Plus size={16} /> Novo board</Button> : null}
      </div>
      <StatsCards stats={stats} />
      <VotesChart posts={posts} />
      <div>
        <h2 className="mb-3 font-bold">Boards</h2>
        {isLoading ? <Skeleton className="h-28" /> : <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">{boards.map((board) => <BoardCard key={board.id} board={board} />)}</div>}
      </div>
      {showCreateBoard ? <CreateBoardModal onClose={() => setShowCreateBoard(false)} /> : null}
    </section>
  );
}
