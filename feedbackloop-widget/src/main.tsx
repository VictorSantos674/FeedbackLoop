import React from 'react';
import { createRoot, type Root } from 'react-dom/client';
import { Widget } from './Widget';
import type { WidgetConfig } from './types';
import widgetCss from './styles/widget.css?inline';

type FeedbackLoopPublicApi = {
  init(config: WidgetConfig): void;
  destroy(): void;
};

let root: Root | null = null;
let hostElement: HTMLDivElement | null = null;
let cleanupThemeListener: (() => void) | null = null;

export function init(config: WidgetConfig) {
  validateConfig(config);
  destroy();

  hostElement = document.createElement('div');
  hostElement.setAttribute('data-feedbackloop-widget', 'true');
  document.body.appendChild(hostElement);

  const shadowRoot = hostElement.attachShadow({ mode: 'open' });
  const style = document.createElement('style');
  style.textContent = widgetCss;
  shadowRoot.appendChild(style);

  const mountNode = document.createElement('div');
  shadowRoot.appendChild(mountNode);
  applyTheme(shadowRoot, config.theme ?? 'light');

  root = createRoot(mountNode);
  root.render(
    <React.StrictMode>
      <Widget config={{ ...config, position: config.position ?? 'bottom-right', theme: config.theme ?? 'light' }} />
    </React.StrictMode>
  );
}

export function destroy() {
  cleanupThemeListener?.();
  cleanupThemeListener = null;
  root?.unmount();
  root = null;
  hostElement?.remove();
  hostElement = null;
}

function validateConfig(config: WidgetConfig) {
  if (!config?.boardSlug) {
    throw new Error('FeedbackLoop.init requires boardSlug.');
  }

  if (!config.endUserToken) {
    throw new Error('FeedbackLoop.init requires endUserToken.');
  }

  if (!config.apiBaseUrl) {
    throw new Error('FeedbackLoop.init requires apiBaseUrl.');
  }
}

function applyTheme(shadowRoot: ShadowRoot, theme: WidgetConfig['theme']) {
  const host = shadowRoot.host;
  host.classList.remove('dark');

  if (theme === 'dark') {
    host.classList.add('dark');
    return;
  }

  if (theme === 'auto') {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    const syncTheme = (matches: boolean) => {
      host.classList.toggle('dark', matches);
    };

    syncTheme(mediaQuery.matches);
    const listener = (event: MediaQueryListEvent) => syncTheme(event.matches);
    mediaQuery.addEventListener('change', listener);
    cleanupThemeListener = () => mediaQuery.removeEventListener('change', listener);
  }
}

declare global {
  interface Window {
    FeedbackLoop?: FeedbackLoopPublicApi;
  }
}

window.FeedbackLoop = {
  init,
  destroy
};
