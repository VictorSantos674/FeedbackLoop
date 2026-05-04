import type { PostFilter, PostStatus } from '../../types';

type PostFiltersProps = {
  filter: PostFilter;
  onChange: (filter: PostFilter) => void;
};

export function PostFilters({ filter, onChange }: PostFiltersProps) {
  return (
    <div className="flex flex-wrap gap-3">
      <select
        className="rounded-md border border-slate-200 bg-white px-3 py-2 text-sm"
        value={filter.status ?? ''}
        onChange={(event) => onChange({ ...filter, status: event.target.value ? (event.target.value as PostStatus) : undefined, page: 1 })}
      >
        <option value="">Todos os status</option>
        <option value="Open">Aberto</option>
        <option value="UnderReview">Em analise</option>
        <option value="Planned">Planejado</option>
        <option value="InProgress">Em progresso</option>
        <option value="Done">Concluido</option>
        <option value="Declined">Recusado</option>
      </select>
      <select
        className="rounded-md border border-slate-200 bg-white px-3 py-2 text-sm"
        value={filter.sortBy ?? 'MostVoted'}
        onChange={(event) => onChange({ ...filter, sortBy: event.target.value as PostFilter['sortBy'], page: 1 })}
      >
        <option value="MostVoted">Mais votadas</option>
        <option value="Newest">Mais recentes</option>
        <option value="Oldest">Mais antigas</option>
        <option value="RecentlyUpdated">Atualizadas</option>
      </select>
    </div>
  );
}
