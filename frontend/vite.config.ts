import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import eslint from 'vite-plugin-eslint';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    react(),
    eslint({
      cache: false,
      include: ['src/**/*.ts', 'src/**/*.tsx'],
      // Corrected line using glob patterns (strings)
      exclude: [
        'node_modules/**', // Exclude everything inside node_modules
        'dist/**', // Exclude everything inside dist
        'build/**', // Exclude everything inside build
        '.vite/**', // Exclude Vite's internal cache/build artifacts
        // Add any other specific directories or patterns as strings
      ],
      failOnError: false,
    }),
  ],
});
