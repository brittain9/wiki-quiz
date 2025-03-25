import React from 'react';
import { PaletteMode } from '@mui/material';
import CssBaseline from '@mui/material/CssBaseline';
import Box from '@mui/material/Box';
import Divider from '@mui/material/Divider';
import { ThemeProvider } from '@mui/material/styles';
import AppAppBar from './components/AppBar/AppAppBar';
import Hero from './components/Hero/Hero';
import Highlights from './components/Highlights';
import Footer from './components/Footer';
import { lightTheme, darkTheme } from './getTheme';
import QuizComponent from './components/QuizComponent';
import SubmissionHistory from './components/SubmissionHistory';
import { QuizOptionsProvider } from './context/QuizOptionsContext';
import { QuizStateProvider } from './context/QuizStateContext';

const App: React.FC = () => {
  const [mode, setMode] = React.useState<PaletteMode>('light');
  const theme = mode === 'dark' ? darkTheme : lightTheme;

  return (
    <QuizOptionsProvider>
      <QuizStateProvider>
        <ThemeProvider theme={theme}>
          <CssBaseline />
          <AppAppBar 
            mode={mode} 
            toggleColorMode={() => setMode(mode === 'dark' ? 'light' : 'dark')}
          />
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
        </ThemeProvider>
      </QuizStateProvider>
    </QuizOptionsProvider>
  );
};

export default App;