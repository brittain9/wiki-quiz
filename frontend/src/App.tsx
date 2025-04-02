// App.tsx
import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import React, { useEffect, useMemo, lazy, Suspense } from 'react';

import QuizAppBar from './components/AppBar/QuizAppBar';
import Footer from './components/Footer';
import Hero from './components/Hero/Hero';
import OverlayManager from './components/Overlays/OverlayManager';
import ThemeSelector from './components/ThemeSelector';
import { useCustomTheme } from './context/CustomThemeContext';

// Lazy load components that aren't needed for initial render
const Highlights = lazy(() => import('./components/Highlights'));
const QuizComponent = lazy(() => import('./components/QuizComponent'));
const SubmissionHistory = lazy(() => import('./components/SubmissionHistory'));

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
  const { currentTheme } = useCustomTheme();

  // Set theme class on root when theme changes
  useEffect(() => {
    document.documentElement.className = `theme-${currentTheme}`;
    document.body.className = `theme-${currentTheme}`;
  }, [currentTheme]);

  // Memoize the theme class
  const themeClass = useMemo(() => `theme-${currentTheme}`, [currentTheme]);

  return (
    <Box
      className={`app-container ${themeClass}`}
      sx={{
        minHeight: '100vh',
        backgroundColor: 'var(--bg-color)',
        transition: 'background-color 0.3s ease, color 0.3s ease',
      }}
    >
      <QuizAppBar />
      <Hero />

      <Suspense fallback={<LoadingFallback />}>
        <QuizComponent />
        <SubmissionHistory />
        <Highlights />
      </Suspense>

      <Footer />

      {/* Theme Selector */}
      <ThemeSelector />

      {/* Render overlay manager to handle all modal overlays */}
      <OverlayManager />
    </Box>
  );
});

// Add display name
App.displayName = 'App';
LoadingFallback.displayName = 'LoadingFallback';

export default App;
