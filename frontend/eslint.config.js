// eslint.config.js
import globals from 'globals';
import pluginJs from '@eslint/js';
import tseslint from 'typescript-eslint';
import pluginReact from 'eslint-plugin-react';
import pluginReactHooks from 'eslint-plugin-react-hooks';
import pluginJsxA11y from 'eslint-plugin-jsx-a11y';
import pluginImport from 'eslint-plugin-import';
import pluginPrettier from 'eslint-plugin-prettier'; // For running prettier as a rule
import configPrettier from 'eslint-config-prettier'; // For disabling conflicting rules

export default tseslint.config(
  // ----- Global Ignores -----
  {
    ignores: [
      'node_modules/',
      'dist/',
      'build/',
      'vite.config.ts',
      '*.config.js',
      '*.config.cjs',
      '.prettierrc.cjs',
    ],
  },

  // ----- Base ESLint & JS Rules -----
  pluginJs.configs.recommended,

  // ----- TypeScript Rules -----
  ...tseslint.configs.recommended,
  // Optional stricter rule sets:
  // ...tseslint.configs.recommendedTypeChecked,
  // ...tseslint.configs.stylisticTypeChecked,
  {
    languageOptions: {
      parserOptions: {
        project: true, // Automatically finds tsconfig.json
        tsconfigRootDir: import.meta.dirname, // Root dir for tsconfig.json search
      },
    },
    rules: {
      '@typescript-eslint/no-unused-vars': [
        'warn',
        { argsIgnorePattern: '^_', varsIgnorePattern: '^_' },
      ],
      '@typescript-eslint/explicit-function-return-type': 'off', // Allow implicit return types
      '@typescript-eslint/explicit-module-boundary-types': 'off', // Allow implicit boundary types
      '@typescript-eslint/no-explicit-any': 'warn', // Warn on 'any'
      // Add other TS overrides here
    },
  },

  // ----- React Rules -----
  {
    files: ['**/*.{ts,tsx}'], // Apply React rules only to TS/TSX files
    plugins: {
      react: pluginReact,
      'react-hooks': pluginReactHooks,
    },
    languageOptions: {
      parserOptions: {
        ecmaFeatures: { jsx: true }, // Enable JSX parsing
      },
      globals: {
        // Add browser and ES environment globals
        ...globals.browser,
        ...globals.es2021,
      },
    },
    settings: {
      react: {
        version: 'detect', // Automatically detect React version
      },
    },
    rules: {
      ...pluginReact.configs.recommended.rules,
      ...pluginReact.configs['jsx-runtime'].rules, // For new JSX transform (React 17+)
      ...pluginReactHooks.configs.recommended.rules,
      'react/prop-types': 'off', // Disable prop-types as TypeScript handles this
      'react/display-name': 'warn',
      // Add other React overrides here
    },
  },

  // ----- JSX Accessibility Rules -----
  {
    files: ['**/*.{jsx,tsx}'], // Apply only to JSX/TSX files
    plugins: {
      'jsx-a11y': pluginJsxA11y,
    },
    rules: {
      ...pluginJsxA11y.configs.recommended.rules,
      // Add specific a11y overrides here if needed
    },
  },

  // ----- Import Rules -----
  {
    files: ['**/*.{js,jsx,ts,tsx}'], // Apply to all relevant script files
    plugins: {
      import: pluginImport,
    },
    settings: {
      'import/resolver': {
        typescript: {
          alwaysTryTypes: true, // Helps with type imports
          project: './tsconfig.json',
        },
        node: true, // Fallback to node resolution
      },
    },
    rules: {
      ...pluginImport.configs.recommended.rules,
      ...pluginImport.configs.typescript.rules, // TS specific import rules
      'import/order': [
        // Enforce consistent import order
        'warn',
        {
          groups: [
            'builtin',
            'external',
            'internal',
            ['parent', 'sibling', 'index'],
            'type',
            'object',
          ],
          'newlines-between': 'always',
          alphabetize: { order: 'asc', caseInsensitive: true },
        },
      ],
      'import/no-unresolved': 'error', // Catch unresolved imports
      'import/prefer-default-export': 'off', // Allow named exports without default
      // Add other import overrides here
    },
  },

  // ----- Prettier Integration (MUST BE LAST) -----
  // This setup uses eslint-plugin-prettier to run Prettier as an ESLint rule
  // and eslint-config-prettier to disable any ESLint rules that conflict with Prettier.
  {
    plugins: {
      prettier: pluginPrettier,
    },
    rules: {
      ...configPrettier.rules, // Disable conflicting ESLint rules
      'prettier/prettier': ['warn', {}, { usePrettierrc: true }], // Show Prettier differences as warnings
    },
  },
);
