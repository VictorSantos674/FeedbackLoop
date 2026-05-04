export type UserRole = 'Admin' | 'Member' | 'Viewer';

export type PostStatus = 'Open' | 'UnderReview' | 'Planned' | 'InProgress' | 'Done' | 'Declined';

export interface User {
  id: string;
  name: string;
  email: string;
  role: UserRole;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  workspaceName: string;
}

export interface BoardSummary {
  id: string;
  name: string;
  slug: string;
  postCount: number;
  createdAt: string;
}

export interface BoardDetail {
  id: string;
  name: string;
  slug: string;
  description?: string;
  isPublic: boolean;
  createdAt: string;
}

export interface CreateBoardRequest {
  name: string;
  description?: string;
  isPublic: boolean;
}

export interface Post {
  id: string;
  title: string;
  description?: string;
  status: PostStatus;
  voteCount: number;
  commentCount: number;
  hasVoted: boolean | null;
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

export interface StatusHistoryEntry {
  from?: PostStatus;
  to: PostStatus;
  changedBy: string;
  changedAt: string;
}
