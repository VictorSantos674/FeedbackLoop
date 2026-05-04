import type { FeedbackLoopWidgetOptions } from './main';

type WidgetProps = {
  options: FeedbackLoopWidgetOptions;
};

export function Widget({ options }: WidgetProps) {
  return (
    <section className="feedbackloop-widget">
      <header>
        <strong>FeedbackLoop</strong>
        <span>{options.boardSlug}</span>
      </header>
      <form>
        <input type="text" placeholder="Sugira uma melhoria" aria-label="Titulo da sugestao" />
        <textarea placeholder="Conte um pouco mais" aria-label="Descricao da sugestao" />
        <button type="button">Enviar</button>
      </form>
    </section>
  );
}
