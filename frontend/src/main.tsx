// main.tsx
import CssBaseline from '@mui/material/CssBaseline';
import * as React from 'react';
import * as ReactDOM from 'react-dom/client';
import { I18nextProvider } from 'react-i18next';

import App from './App';
import { QuizOptionsProvider } from './context/QuizOptionsContext';
import { QuizStateProvider } from './context/QuizStateContext';
import { ThemeProvider } from './context/ThemeContext';
import i18n from './i18n';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <CssBaseline />
    <I18nextProvider i18n={i18n}>
      <ThemeProvider>
          <QuizOptionsProvider>
            <QuizStateProvider>
              <App />
            </QuizStateProvider>
          </QuizOptionsProvider>
      </ThemeProvider>
    </I18nextProvider>
  </React.StrictMode>,
);
