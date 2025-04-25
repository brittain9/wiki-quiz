// App.tsx
import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import React, { useMemo, lazy, Suspense } from 'react';

import {
  QuizAppBar,
  Footer,
  Hero,
  OverlayManager,
  ThemeSelector,
} from './components';
import { useCustomTheme } from './context/CustomThemeContext/CustomThemeContext';

// Lazy load components that aren't needed for initial render
const LazyHighlights = lazy(() => import('./components/Highlights'));
const LazyQuiz = lazy(() => import('./components/Quiz/Quiz'));
const LazySubmissionHistory = lazy(
  () => import('./components/SubmissionHistory'),
);

// Loading fallback component
const LoadingFallback = () => (
  <Box
    sx={{
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      py: 6,
    }}
  >
    <CircularProgress sx={{ color: 'var(--main-color)' }} />
  </Box>
);

// Main App component
const App: React.FC = React.memo(() => {
  const { themeToDisplay } = useCustomTheme();

  // Memoize the theme class
  const themeClass = useMemo(() => `theme-${themeToDisplay}`, [themeToDisplay]);

  return (
    <Box
      className={`app-container ${themeClass}`}
      sx={{
        minHeight: '100vh',
        backgroundColor: 'var(--bg-color)',
        color: 'var(--text-color)',
        transition: 'background-color 0.3s ease, color 0.3s ease',
      }}
    >
      <QuizAppBar />
      <Hero />
      <Suspense fallback={<LoadingFallback />}>
        <LazyQuiz />
        <LazySubmissionHistory />
        <LazyHighlights />
      </Suspense>
      <Footer />
      <ThemeSelector />
      <OverlayManager />
    </Box>
  );
});

App.displayName = 'App';
LoadingFallback.displayName = 'LoadingFallback';

export default App;
