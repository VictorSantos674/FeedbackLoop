import React from 'react';
import ReactDOM from 'react-dom/client';
import { Widget } from './Widget';
import './styles.css';

export type FeedbackLoopWidgetOptions = {
  boardSlug: string;
  endUserToken: string;
  apiBaseUrl: string;
  container: string | HTMLElement;
};

export function mountFeedbackLoopWidget(options: FeedbackLoopWidgetOptions) {
  const container =
    typeof options.container === 'string'
      ? document.querySelector(options.container)
      : options.container;

  if (!container) {
    throw new Error('FeedbackLoop widget container not found.');
  }

  ReactDOM.createRoot(container).render(
    <React.StrictMode>
      <Widget options={options} />
    </React.StrictMode>
  );
}

declare global {
  interface Window {
    FeedbackLoopWidget?: {
      mount: typeof mountFeedbackLoopWidget;
    };
  }
}

window.FeedbackLoopWidget = {
  mount: mountFeedbackLoopWidget
};

const sandboxContainer = document.getElementById('feedbackloop-widget');
if (sandboxContainer) {
  mountFeedbackLoopWidget({
    apiBaseUrl: 'http://localhost:5000',
    boardSlug: 'demo',
    endUserToken: crypto.randomUUID(),
    container: sandboxContainer
  });
}
