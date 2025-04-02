// App.tsx
import Box from '@mui/material/Box';
import Divider from '@mui/material/Divider';
import React from 'react';

import QuizAppBar from './components/AppBar/QuizAppBar';
import Footer from './components/Footer';
import Hero from './components/Hero/Hero';
import Highlights from './components/Highlights';
import OverlayManager from './components/Overlays/OverlayManager';
import QuizComponent from './components/QuizComponent';
import SubmissionHistory from './components/SubmissionHistory';
import { useTheme } from './context/ThemeContext';

const App: React.FC = () => {
  const { mode, toggleColorMode } = useTheme();

  return (
    <>
      <QuizAppBar mode={mode} toggleColorMode={toggleColorMode} />
      <Hero />
      <Box 
        id="quiz-section"
        sx={{ bgcolor: 'background.default', py: 4 }}
      >
        <QuizComponent />
        <Divider />
        <SubmissionHistory />
        <Divider />
        <Highlights />
        <Divider />
        <Footer />
      </Box>
      
      {/* Render overlay manager to handle all modal overlays */}
      <OverlayManager />
    </>
  );
};

export default App;
