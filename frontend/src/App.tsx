// App.tsx
import Box from '@mui/material/Box';
import Divider from '@mui/material/Divider';
import React from 'react';

import QuizAppBar from './components/AppBar/QuizAppBar';
import AuthContainer from './components/Auth/AuthContainer';
import Footer from './components/Footer';
import Hero from './components/Hero/Hero';
import Highlights from './components/Highlights';
import QuizComponent from './components/QuizComponent';
import SubmissionHistory from './components/SubmissionHistory';
import { AuthProvider } from './context/AuthProvider';
import { useTheme } from './context/ThemeContext';

const App: React.FC = () => {
  const { mode, toggleColorMode } = useTheme();

  return (
    <AuthProvider>
      <QuizAppBar mode={mode} toggleColorMode={toggleColorMode} />
      <Hero />
      <Box sx={{ bgcolor: 'background.default', py: 4 }}>
        {/* Auth Demo Component */}
        <AuthContainer />

        <Divider sx={{ my: 4 }} />

        <QuizComponent />
        <Divider />
        <SubmissionHistory />
        <Divider />
        <Highlights />
        <Divider />
        <Footer />
      </Box>
    </AuthProvider>
  );
};

export default App;
