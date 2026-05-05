import { CheckCircle, MessageSquare, Vote } from 'lucide-react';
import { Card } from '../ui/Card';

export type DashboardStats = {
  totalPosts: number;
  openPosts: number;
  totalVotes: number;
  resolvedPosts: number;
};

export function StatsCards({ stats }: { stats: DashboardStats }) {
  return (
    <div className="grid gap-4 md:grid-cols-4">
      <Stat label="Total de posts" value={stats.totalPosts} icon={<MessageSquare size={18} />} />
      <Stat label="Posts abertos" value={stats.openPosts} icon={<MessageSquare size={18} />} />
      <Stat label="Total de votos" value={stats.totalVotes} icon={<Vote size={18} />} />
      <Stat label="Resolvidos" value={stats.resolvedPosts} icon={<CheckCircle size={18} />} />
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
