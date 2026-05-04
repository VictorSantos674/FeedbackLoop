import { useState } from 'react';
import type { WidgetConfig } from '../types';
import { usePosts } from '../hooks/usePosts';

type NewPostFormProps = {
  config: WidgetConfig;
  postsState: ReturnType<typeof usePosts>;
  onCreated: () => void;
  onToast: (message: string) => void;
};

export function NewPostForm({ config, postsState, onCreated, onToast }: NewPostFormProps) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [touched, setTouched] = useState({ title: false, description: false });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const titleError = getTitleError(title);
  const descriptionError = description.length > 500 ? 'A descricao deve ter ate 500 caracteres.' : null;
  const isValid = !titleError && !descriptionError;

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setTouched({ title: true, description: true });
    if (!isValid) return;

    setIsSubmitting(true);
    try {
      await postsState.addPost({
        title,
        description: description.trim() || undefined,
        endUserToken: config.endUserToken,
        endUserName: config.endUserName
      });
      setTitle('');
      setDescription('');
      setTouched({ title: false, description: false });
      onCreated();
      onToast('Sugestao enviada com sucesso.');
    } catch (caught) {
      onToast(caught instanceof Error ? caught.message : 'Nao foi possivel enviar sua sugestao.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form className="fl-form" onSubmit={handleSubmit}>
      <div className="fl-form__group">
        <label className="fl-form__label" htmlFor="fl-title">
          Titulo
        </label>
        <input
          id="fl-title"
          className="fl-field"
          value={title}
          maxLength={100}
          onBlur={() => setTouched((current) => ({ ...current, title: true }))}
          onChange={(event) => setTitle(event.target.value)}
          placeholder="Ex: Adicionar exportacao CSV"
        />
        <div className="fl-form__hint">
          <span>{touched.title && titleError ? <span className="fl-error">{titleError}</span> : '10 a 100 caracteres'}</span>
          <span>{title.length}/100</span>
        </div>
      </div>

      <div className="fl-form__group">
        <label className="fl-form__label" htmlFor="fl-description">
          Descricao
        </label>
        <textarea
          id="fl-description"
          className="fl-field fl-textarea"
          value={description}
          maxLength={500}
          onBlur={() => setTouched((current) => ({ ...current, description: true }))}
          onChange={(event) => setDescription(event.target.value)}
          placeholder="Conte um pouco mais sobre a ideia"
        />
        <div className="fl-form__hint">
          <span>{touched.description && descriptionError ? <span className="fl-error">{descriptionError}</span> : 'Opcional'}</span>
          <span>{description.length}/500</span>
        </div>
      </div>

      <button className="fl-primary-button" type="submit" disabled={!isValid || isSubmitting}>
        {isSubmitting ? 'Enviando...' : 'Enviar'}
      </button>
    </form>
  );
}

function getTitleError(title: string) {
  const length = title.trim().length;
  if (length === 0) return 'Informe um titulo.';
  if (length < 10) return 'O titulo precisa ter pelo menos 10 caracteres.';
  if (length > 100) return 'O titulo deve ter ate 100 caracteres.';
  return null;
}
