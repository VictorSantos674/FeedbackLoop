import type { PostStatus } from '../types';

const labels: Record<PostStatus, string> = {
  Open: 'Aberto',
  UnderReview: 'Em analise',
  Planned: 'Planejado',
  InProgress: 'Em progresso',
  Done: 'Concluido',
  Declined: 'Recusado'
};

export function StatusBadge({ status }: { status: PostStatus }) {
  return <span className={`fl-badge fl-status-${status.toLowerCase()}`}>{labels[status]}</span>;
}
