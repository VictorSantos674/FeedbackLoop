import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  build: {
    sourcemap: true,
    lib: {
      entry: 'src/main.tsx',
      name: 'FeedbackLoop',
      fileName: () => 'widget.umd.js',
      formats: ['umd']
    },
    rollupOptions: {
      external: []
    }
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['src/__tests__/setup.ts']
  }
});
