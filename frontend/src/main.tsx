// main.tsx
import CssBaseline from '@mui/material/CssBaseline';
import * as React from 'react';
import * as ReactDOM from 'react-dom/client';
import { I18nextProvider } from 'react-i18next';

// Import global styles
import './global.css';

import App from './App';
import { AuthProvider } from './context/AuthContext/AuthContext';
import { CustomThemeProvider } from './context/CustomThemeContext/CustomThemeContext';
import { OverlayProvider } from './context/OverlayContext/OverlayContext';
import { QuizOptionsProvider } from './context/QuizOptionsContext/QuizOptionsContext';
import { QuizStateProvider } from './context/QuizStateContext/QuizStateContext';
import i18n from './i18n';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <CssBaseline />
    <I18nextProvider i18n={i18n}>
      <CustomThemeProvider>
        <AuthProvider>
          <OverlayProvider>
            <QuizOptionsProvider>
              <QuizStateProvider>
                <App />
              </QuizStateProvider>
            </QuizOptionsProvider>
          </OverlayProvider>
        </AuthProvider>
      </CustomThemeProvider>
    </I18nextProvider>
  </React.StrictMode>,
);
