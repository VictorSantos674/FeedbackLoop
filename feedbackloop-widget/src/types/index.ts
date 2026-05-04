export interface WidgetConfig {
  boardSlug: string;
  endUserToken: string;
  endUserName: string;
  apiBaseUrl: string;
  position?: 'bottom-right' | 'bottom-left';
  theme?: 'light' | 'dark' | 'auto';
}

export type PostStatus =
  | 'Open'
  | 'UnderReview'
  | 'Planned'
  | 'InProgress'
  | 'Done'
  | 'Declined';

export interface Post {
  id: string;
  title: string;
  description?: string;
  status: PostStatus;
  voteCount: number;
  commentCount: number;
  hasVoted: boolean;
  endUserName: string;
  createdAt: string;
}

export interface PostFilter {
  status?: PostStatus;
  sortBy?: 'MostVoted' | 'Newest' | 'Oldest' | 'RecentlyUpdated';
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreatePostPayload {
  title: string;
  description?: string;
  endUserToken: string;
  endUserName: string;
}

export interface VoteResult {
  voted: boolean;
  voteCount: number;
}
