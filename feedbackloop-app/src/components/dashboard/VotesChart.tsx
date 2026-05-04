import { Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { Card } from '../ui/Card';
import type { Post } from '../../types';

export function VotesChart({ posts }: { posts: Post[] }) {
  const data = Array.from({ length: 30 }, (_, index) => {
    const date = new Date();
    date.setDate(date.getDate() - (29 - index));
    const key = date.toISOString().slice(0, 10);
    return {
      date: key.slice(5),
      votes: posts
        .filter((post) => post.createdAt.slice(0, 10) === key)
        .reduce((sum, post) => sum + post.voteCount, 0)
    };
  });

  return (
    <Card>
      <h2 className="mb-4 font-bold">Votos por dia</h2>
      <div className="h-72">
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={data}>
            <XAxis dataKey="date" />
            <YAxis allowDecimals={false} />
            <Tooltip />
            <Line type="monotone" dataKey="votes" stroke="#6366f1" strokeWidth={2} dot={false} />
          </LineChart>
        </ResponsiveContainer>
      </div>
    </Card>
  );
}
