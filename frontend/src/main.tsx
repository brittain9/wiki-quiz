// main.tsx
import CssBaseline from '@mui/material/CssBaseline';
import {
  ThemeProvider,
  createTheme,
  responsiveFontSizes,
} from '@mui/material/styles';
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
import i18n from './i18n/i18n';
import { createAppTheme } from './theme/mui-theme';
import { initTheme } from './theme/themeService';

// First load the base theme style element
const linkElement = document.createElement('link');
linkElement.rel = 'stylesheet';
linkElement.id = 'currentTheme';
linkElement.href = '/themes/dark.css';
document.head.appendChild(linkElement);

const muiTheme = responsiveFontSizes(createTheme(createAppTheme()));

// Initialize the app with theme
const init = async () => {
  // Initialize CSS theme
  await initTheme();

  ReactDOM.createRoot(document.getElementById('root')!).render(
    <React.StrictMode>
      <ThemeProvider theme={muiTheme}>
        <CssBaseline enableColorScheme />
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
      </ThemeProvider>
    </React.StrictMode>,
  );
};

// Start the app
init().catch(console.error);
