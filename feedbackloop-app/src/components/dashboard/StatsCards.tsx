import { CheckCircle, MessageSquare, Vote } from 'lucide-react';
import { Card } from '../ui/Card';
import type { Post } from '../../types';

export function StatsCards({ posts }: { posts: Post[] }) {
  const totalVotes = posts.reduce((sum, post) => sum + post.voteCount, 0);
  const resolved = posts.filter((post) => post.status === 'Done').length;
  const open = posts.filter((post) => post.status === 'Open').length;

  return (
    <div className="grid gap-4 md:grid-cols-4">
      <Stat label="Total de posts" value={posts.length} icon={<MessageSquare size={18} />} />
      <Stat label="Posts abertos" value={open} icon={<MessageSquare size={18} />} />
      <Stat label="Total de votos" value={totalVotes} icon={<Vote size={18} />} />
      <Stat label="Resolvidos" value={resolved} icon={<CheckCircle size={18} />} />
    </div>
  );
}

function Stat({ label, value, icon }: { label: string; value: number; icon: React.ReactNode }) {
  return (
    <Card>
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm text-slate-500">{label}</p>
          <p className="mt-2 text-2xl font-black">{value}</p>
        </div>
        <div className="rounded-md bg-brand-50 p-2 text-brand-700">{icon}</div>
      </div>
    </Card>
  );
}
