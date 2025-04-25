// App.tsx
import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import React, { useMemo, lazy, Suspense } from 'react';

import { QuizAppBar, Footer, Hero, OverlayManager } from './components';

// Lazy load components that aren't needed for initial render
const LazyHighlights = lazy(() => import('./components/Highlights'));
const LazyQuiz = lazy(() => import('./components/Quiz/QuizContainer'));
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
    <CircularProgress />
  </Box>
);

// Main App component
const App: React.FC = React.memo(() => {
  return (
    <Box
      className="app-container"
      sx={{
        minHeight: '100vh',
        backgroundColor: '#323437',
        color: '#d1d0c5',
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
      <OverlayManager />
    </Box>
  );
});

App.displayName = 'App';
LoadingFallback.displayName = 'LoadingFallback';

export default App;
