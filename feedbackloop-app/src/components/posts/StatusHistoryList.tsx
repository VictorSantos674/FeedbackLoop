import type { StatusHistoryEntry } from '../../types';
import { Badge } from '../ui/Badge';

export function StatusHistoryList({ entries }: { entries: StatusHistoryEntry[] }) {
  if (entries.length === 0) {
    return <p className="text-sm text-slate-500">Nenhuma mudanca de status ainda.</p>;
  }

  return (
    <div className="space-y-4">
      {entries.map((entry, index) => (
        <div key={`${entry.changedAt}-${index}`} className="border-l-2 border-brand-100 pl-4">
          <Badge status={entry.to} />
          <p className="mt-2 text-sm font-semibold">Alterado por {entry.changedBy}</p>
          <p className="text-xs text-slate-500">{new Date(entry.changedAt).toLocaleString()}</p>
        </div>
      ))}
    </div>
  );
}
