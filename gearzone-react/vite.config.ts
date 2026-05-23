import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // Proxy API requests to ASP.NET backend during development
      '/api': {
        target: 'https://localhost:7000',
        changeOrigin: true,
        secure: false,
      },
      '/hubs': {
        target: 'https://localhost:7000',
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
});
