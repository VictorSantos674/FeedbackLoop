import type { PostStatus } from '../../types';

const statusClasses: Record<PostStatus, string> = {
  Open: 'bg-gray-100 text-gray-700',
  UnderReview: 'bg-blue-100 text-blue-700',
  Planned: 'bg-violet-100 text-violet-700',
  InProgress: 'bg-orange-100 text-orange-700',
  Done: 'bg-green-100 text-green-700',
  Declined: 'bg-red-100 text-red-700'
};

const labels: Record<PostStatus, string> = {
  Open: 'Aberto',
  UnderReview: 'Em analise',
  Planned: 'Planejado',
  InProgress: 'Em progresso',
  Done: 'Concluido',
  Declined: 'Recusado'
};

export function Badge({ status }: { status: PostStatus }) {
  return <span className={`rounded-full px-2 py-1 text-xs font-bold ${statusClasses[status]}`}>{labels[status]}</span>;
}
