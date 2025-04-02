// main.tsx
import CssBaseline from '@mui/material/CssBaseline';
import * as React from 'react';
import * as ReactDOM from 'react-dom/client';
import { I18nextProvider } from 'react-i18next';

// Import base theme CSS first
import '../public/themes/base.css';

import App from './App';
import { AuthProvider } from './context/AuthProvider';
import { CustomThemeProvider } from './context/CustomThemeContext';
import { OverlayProvider } from './context/OverlayContext';
import { QuizOptionsProvider } from './context/QuizOptionsContext';
import { QuizStateProvider } from './context/QuizStateContext';
import i18n from './i18n';

// Global styles to ensure theme variables can override MUI
const globalStylesElement = document.createElement('style');
globalStylesElement.innerHTML = `
  :root {
    --font-family-main: 'Roboto Mono', monospace;
    --font-family-alt: 'Lexend Deca', sans-serif;
  }
  
  body * {
    transition: background-color 0.3s ease, color 0.3s ease;
  }
  
  /* Force MUI to use our theme variables */
  .MuiPaper-root, .MuiAppBar-root, .MuiDrawer-paper, .MuiButton-root, 
  .MuiTextField-root, .MuiInputBase-root, .MuiTypography-root, 
  .MuiDivider-root, .MuiIconButton-root, .MuiMenu-paper, .MuiMenuItem-root,
  .MuiList-root, .MuiListItem-root, .MuiListItemButton-root, .MuiCard-root {
    transition: background-color 0.3s ease, color 0.3s ease, border-color 0.3s ease !important;
  }
`;
document.head.appendChild(globalStylesElement);

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
