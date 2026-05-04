import { useEffect, useState } from 'react';
import type { WidgetConfig } from './types';
import { FloatingButton } from './components/FloatingButton';
import { Panel } from './components/Panel';

type WidgetProps = {
  config: WidgetConfig;
};

export function Widget({ config }: WidgetProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [toast, setToast] = useState<string | null>(null);
  const position = config.position ?? 'bottom-right';

  useEffect(() => {
    if (!toast) return;
    const timeout = window.setTimeout(() => setToast(null), 3000);
    return () => window.clearTimeout(timeout);
  }, [toast]);

  return (
    <div className={`fl-root fl-position-${position}`}>
      <FloatingButton onClick={() => setIsOpen((current) => !current)} />
      <Panel isOpen={isOpen} config={config} onClose={() => setIsOpen(false)} onToast={setToast} />
      {toast ? <div className="fl-toast">{toast}</div> : null}
    </div>
  );
}
