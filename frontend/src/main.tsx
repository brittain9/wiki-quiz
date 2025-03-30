// main.tsx
import * as React from 'react';
import * as ReactDOM from 'react-dom/client';
import CssBaseline from '@mui/material/CssBaseline';
import { I18nextProvider } from 'react-i18next';
import i18n from './i18n';
import App from './App';
import { QuizOptionsProvider } from './context/QuizOptionsContext';
import { QuizStateProvider } from './context/QuizStateContext';
import { ThemeProvider } from './context/ThemeContext';
import { AuthProvider } from './context/AuthContext';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <CssBaseline />
    <I18nextProvider i18n={i18n}>
      <ThemeProvider>
        <AuthProvider>
          <QuizOptionsProvider>
            <QuizStateProvider>
              <App />
            </QuizStateProvider>
          </QuizOptionsProvider>
        </AuthProvider>
      </ThemeProvider>
    </I18nextProvider>
  </React.StrictMode>
);