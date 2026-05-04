import { useState } from 'react';
import type { PostFilter, WidgetConfig } from '../types';
import { usePosts } from '../hooks/usePosts';
import { NewPostForm } from './NewPostForm';
import { PostList } from './PostList';

type PanelProps = {
  isOpen: boolean;
  config: WidgetConfig;
  onClose: () => void;
  onToast: (message: string) => void;
};

export function Panel({ isOpen, config, onClose, onToast }: PanelProps) {
  const [activeTab, setActiveTab] = useState<'list' | 'new'>('list');
  const [filter, setFilter] = useState<PostFilter>({ sortBy: 'MostVoted', pageSize: 20 });
  const postsState = usePosts(config, filter);

  return (
    <aside className={`fl-panel ${isOpen ? 'is-open' : ''}`} aria-hidden={!isOpen}>
      <header className="fl-panel__header">
        <h2 className="fl-panel__title">Sugestoes</h2>
        <button className="fl-icon-button" type="button" aria-label="Fechar" onClick={onClose}>
          ×
        </button>
      </header>
      <div className="fl-tabs" role="tablist">
        <button className={`fl-tab ${activeTab === 'list' ? 'is-active' : ''}`} type="button" onClick={() => setActiveTab('list')}>
          Ver sugestoes
        </button>
        <button className={`fl-tab ${activeTab === 'new' ? 'is-active' : ''}`} type="button" onClick={() => setActiveTab('new')}>
          Nova sugestao
        </button>
      </div>
      <div className="fl-panel__body">
        {activeTab === 'list' ? (
          <PostList config={config} postsState={postsState} filter={filter} onFilterChange={setFilter} onToast={onToast} />
        ) : (
          <NewPostForm config={config} postsState={postsState} onCreated={() => setActiveTab('list')} onToast={onToast} />
        )}
      </div>
    </aside>
  );
}
