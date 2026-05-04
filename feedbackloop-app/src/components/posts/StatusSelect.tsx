import type { PostStatus } from '../../types';
import { useUpdateStatus } from '../../hooks/usePosts';

const statuses: PostStatus[] = ['Open', 'UnderReview', 'Planned', 'InProgress', 'Done', 'Declined'];

export function StatusSelect({ boardId, postId, status }: { boardId: string; postId: string; status: PostStatus }) {
  const updateStatus = useUpdateStatus();

  return (
    <select
      className="rounded-md border border-slate-200 bg-white px-2 py-1 text-sm font-semibold"
      value={status}
      onClick={(event) => event.stopPropagation()}
      onChange={(event) => updateStatus.mutate({ boardId, postId, status: event.target.value as PostStatus })}
    >
      {statuses.map((item) => (
        <option key={item} value={item}>{item}</option>
      ))}
    </select>
  );
}
