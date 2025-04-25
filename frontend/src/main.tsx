// main.tsx
import CssBaseline from '@mui/material/CssBaseline';
import * as React from 'react';
import * as ReactDOM from 'react-dom/client';
import { I18nextProvider } from 'react-i18next';

// Import global styles
import './global.css';

import App from './App';
import {
  AuthProvider,
  OverlayProvider,
  QuizOptionsProvider,
  QuizStateProvider,
} from './context';
import i18n from './i18n';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <CssBaseline />
    <I18nextProvider i18n={i18n}>
      <AuthProvider>
        <OverlayProvider>
          <QuizOptionsProvider>
            <QuizStateProvider>
              <App />
            </QuizStateProvider>
          </QuizOptionsProvider>
        </OverlayProvider>
      </AuthProvider>
    </I18nextProvider>
  </React.StrictMode>,
);
