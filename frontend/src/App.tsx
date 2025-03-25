// App.tsx
import React from 'react';
import Box from '@mui/material/Box';
import Divider from '@mui/material/Divider';
import Hero from './components/Hero/Hero';
import Highlights from './components/Highlights';
import Footer from './components/Footer';
import QuizComponent from './components/QuizComponent';
import SubmissionHistory from './components/SubmissionHistory';
import QuizAppBar from './components/AppBar/QuizAppBar';
import { useTheme } from './context/ThemeContext';

const App: React.FC = () => {
  const { mode, toggleColorMode } = useTheme();

  return (
    <>
      <QuizAppBar mode={mode} toggleColorMode={toggleColorMode} />
      <Hero />
      <Box sx={{ bgcolor: 'background.default' }}>
        <QuizComponent />
        <Divider />
        <SubmissionHistory />
        <Divider />
        <Highlights />
        <Divider />
        <Footer />
      </Box>
    </>
  );
};

export default App;