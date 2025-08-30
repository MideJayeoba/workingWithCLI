
// filepath: c:\Users\USER\codes\workingWithCLI\vault-frontend\vite.config.js
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/postcss';

export default defineConfig({
  plugins: [react()],
  css: {
    postcss: {
      plugins: [tailwindcss(),
      ],
    },
  },
});