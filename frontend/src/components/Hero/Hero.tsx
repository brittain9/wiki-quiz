import { Paper, List, ListItem, ListItemButton } from '@mui/material';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Container from '@mui/material/Container';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import React, {
  useEffect,
  useState,
  useRef,
  useCallback,
  useMemo,
} from 'react';
import { useTranslation } from 'react-i18next';
import { motion, HTMLMotionProps } from 'framer-motion';

import AnimatedTopics from './AnimatedTopics';
import { useQuizOptions } from '../../context/QuizOptionsContext/QuizOptionsContext';
import { useQuizState } from '../../context/QuizStateContext/QuizStateContext';
import useAuthCheck from '../../hooks/useAuthCheck';
import { quizApi, wikiApi } from '../../services';

const Hero = React.memo(() => {
  const { quizOptions, setTopic } = useQuizOptions();
  const { setIsQuizReady, setCurrentQuiz, setIsGenerating, isGenerating } =
    useQuizState();
  const { t } = useTranslation();
  const [topicInput, setTopicInput] = useState('');
  const [topics, setTopics] = useState<string[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [debounceTimeout, setDebounceTimeout] = useState<number | null>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Use our auth check hook
  const { checkAuth } = useAuthCheck({
    message: t('login.quizCreationMessage'),
    onAuthSuccess: () => {
      // This will run after successful login from the login overlay
      if (quizOptions.topic && !isGenerating) {
        handleSubmitQuiz();
      }
    },
  });

  useEffect(() => {
    // Add event listener to close the dropdown when clicking outside
    const handleClickOutside = (event: MouseEvent) => {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setTopics([]);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  const handleSubmitQuiz = useCallback(async () => {
    if (quizOptions.topic && !isGenerating) {
      try {
        setIsGenerating(true);
        setIsQuizReady(false);

        // Convert from QuizOptions to CreateBasicQuizRequest
        const requestData = {
          topic: quizOptions.topic,
          aiService: quizOptions.selectedService || undefined,
          model: quizOptions.selectedModel || undefined,
          language: quizOptions.language,
          numQuestions: quizOptions.numQuestions,
          numOptions: quizOptions.numOptions,
          extractLength: quizOptions.extractLength,
        };

        const quiz = await quizApi.generateBasicQuiz(requestData);

        setCurrentQuiz(quiz);
        setIsQuizReady(true);
        setError(null);

        // Scroll to quiz section
        const quizElement = document.getElementById('quiz-section');
        if (quizElement) {
          quizElement.scrollIntoView({ behavior: 'smooth' });
        }
      } catch (error: unknown) {
        // Handle error with proper type checking
        if (error instanceof Error) {
          setError(error.message);
        } else {
          setError('An unknown error occurred');
        }
      } finally {
        setIsGenerating(false);
      }
    }
  }, [
    quizOptions,
    isGenerating,
    setIsGenerating,
    setIsQuizReady,
    setCurrentQuiz,
    setError,
  ]);

  const handleSubmit = useCallback(
    async (event: React.FormEvent) => {
      event.preventDefault(); // Prevent default form submission

      // Check if user is authenticated before proceeding
      const isAuthenticated = checkAuth();
      if (isAuthenticated) {
        handleSubmitQuiz();
      }
      // If not authenticated, the login overlay will show automatically
    },
    [checkAuth, handleSubmitQuiz],
  );

  const handleTopicChange = useCallback(
    (event: React.ChangeEvent<HTMLInputElement>) => {
      setTopic(event.target.value);
      setTopicInput(event.target.value);
      setError(null);

      if (debounceTimeout) {
        clearTimeout(debounceTimeout);
      }

      const timeout = window.setTimeout(() => {
        if (event.target.value) {
          wikiApi
            .fetchWikipediaTopics(event.target.value)
            .then(setTopics)
            .catch((err) => setError(err.message));
        } else {
          setTopics([]);
        }
      }, 1000);

      setDebounceTimeout(timeout);
    },
    [setTopic, debounceTimeout, setError, setTopics, setDebounceTimeout],
  );

  const handleTopicSelect = useCallback(
    (selectedTopic: string) => {
      if (!isGenerating) {
        setTopic(selectedTopic);
        setTopicInput(selectedTopic);
        setTopics([]);
      }
    },
    [isGenerating, setTopic],
  );

  // Memoize UI components that won't change often
  const backgroundStyle = useMemo(
    () => ({
      width: '100%',
      backgroundRepeat: 'no-repeat',
      backgroundImage:
        'radial-gradient(ellipse 80% 50% at 50% -20%, hsla(var(--main-color-rgb), 0.15), transparent)',
      backgroundColor: 'var(--bg-color)',
      color: 'var(--text-color)',
    }),
    [],
  );

  const suggestedTopics = useMemo(() => {
    if (topics.length === 0) return null;

    return (
      <Paper
        elevation={3}
        sx={{
          position: 'absolute',
          top: '100%',
          left: 0,
          right: 0,
          zIndex: 1,
          maxHeight: 200,
          overflow: 'auto',
          backgroundColor: 'var(--bg-color-secondary)',
          border: '1px solid var(--sub-color)',
        }}
      >
        <List>
          {topics.map((topic, i) => (
            <ListItem key={i} disablePadding>
              <ListItemButton
                onClick={() => handleTopicSelect(topic)}
                sx={{
                  py: 1,
                  '&:hover': {
                    backgroundColor: 'var(--main-color-10)',
                  },
                }}
              >
                {topic}
              </ListItemButton>
            </ListItem>
          ))}
        </List>
      </Paper>
    );
  }, [topics, handleTopicSelect]);

  // Animated background motion config
  const bgMotion = {
    initial: { scale: 1, opacity: 0.7, y: 0 },
    animate: { scale: [1, 1.04, 1], opacity: [0.7, 1, 0.7], y: [0, 10, 0] },
    transition: { duration: 12, repeat: Infinity, ease: 'easeInOut' },
  };

  return (
    <Box
      id="hero"
      className="hero-section"
      sx={{
        position: 'relative',
        width: '100%',
        minHeight: { xs: 480, sm: 600 },
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        overflow: 'hidden',
        background: 'radial-gradient(ellipse 80% 50% at 50% -20%, hsla(var(--main-color-rgb), 0.15), transparent)',
        backgroundColor: 'var(--bg-color)',
        color: 'var(--text-color)',
      }}
    >
      {/* Animated background blob */}
      <Box
        component={motion.div}
        {...bgMotion}
        sx={{
          position: 'absolute',
          top: { xs: -120, sm: -180 },
          left: '50%',
          transform: 'translateX(-50%)',
          width: { xs: 420, sm: 700 },
          height: { xs: 320, sm: 500 },
          borderRadius: '50%',
          background: 'radial-gradient(circle at 60% 40%, var(--main-color-10) 0%, var(--main-color) 60%, transparent 100%)',
          filter: 'blur(60px)',
          opacity: 0.25,
          zIndex: 0,
        }}
      />
      <Container
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          pt: { xs: 10, sm: 16 },
          pb: { xs: 6, sm: 10 },
          zIndex: 1,
          width: '100%'
        }}
      >
        <Box
          component={motion.div}
          initial={{ opacity: 0, y: 30 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ 
            duration: 0.8, 
            ease: "easeOut",
            delay: 0.2
          }}
          sx={{ 
            width: '100%',
            display: 'flex',
            justifyContent: 'center'
          }}
        >
          <Stack
            spacing={3}
            useFlexGap
            sx={{ 
              alignItems: 'center', 
              width: '100%',
              maxWidth: { xs: '100%', sm: '70%' },
              mx: 'auto'
            }}
          >
            <Typography
              component={motion.h1}
              variant="h1"
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.6, delay: 0.4 }}
              sx={{
                display: 'flex',
                flexDirection: { xs: 'column', sm: 'row' },
                alignItems: 'center',
                justifyContent: { xs: 'center', sm: 'center' },
                fontSize: 'clamp(2.6rem, 8vw, 3.5rem)',
                color: 'var(--text-color)',
                fontWeight: 700,
                letterSpacing: '-0.01em',
                textAlign: 'center',
                mx: 'auto',
                width: '100%',
                maxWidth: 680,
              }}
            >
              <span style={{ 
                whiteSpace: 'nowrap',
                display: 'flex', 
                justifyContent: 'center',
                marginRight: '0.3em'
              }}>
                {t('hero.generateQuiz')}
              </span>
              <AnimatedTopics />
            </Typography>
            <Typography
              component={motion.p}
              variant="subtitle1"
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.6, delay: 0.6 }}
              sx={{
                color: 'var(--sub-color)',
                fontSize: { xs: '1.1rem', sm: '1.25rem' },
                textAlign: 'center',
                maxWidth: 600,
                mb: 1,
                fontWeight: 400,
              }}
            >
              {t('hero.topicInfo')}
            </Typography>
            <Paper
              component={motion.div}
              initial={{ opacity: 0, y: 20, scale: 0.98 }}
              animate={{ opacity: 1, y: 0, scale: 1 }}
              transition={{ 
                duration: 0.7, 
                delay: 0.8,
                ease: [0.25, 1, 0.5, 1]
              }}
              elevation={6}
              sx={{
                width: '100%',
                maxWidth: 520,
                mx: 'auto',
                px: { xs: 2, sm: 4 },
                py: { xs: 3, sm: 4 },
                borderRadius: 4,
                background: 'rgba(40, 40, 60, 0.55)',
                boxShadow: '0 8px 32px 0 rgba(124,58,237,0.10)',
                backdropFilter: 'blur(12px)',
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                zIndex: 2,
                alignSelf: 'center',
              }}
            >
              <form
                onSubmit={handleSubmit}
                style={{ width: '100%' }}
                autoComplete="off"
              >
                <Box
                  sx={{
                    display: 'flex',
                    flexDirection: { xs: 'column', sm: 'row' },
                    gap: 2,
                    position: 'relative',
                    alignItems: 'center',
                    width: '100%',
                  }}
                  ref={dropdownRef}
                >
                  <TextField
                    fullWidth
                    value={topicInput}
                    onChange={handleTopicChange}
                    disabled={isGenerating}
                    placeholder={t('hero.placeholder')}
                    variant="outlined"
                    error={!!error}
                    helperText={error}
                    inputProps={{
                      'aria-label': t('hero.placeholder'),
                      style: { fontSize: '1.15rem', fontWeight: 500 },
                    }}
                    sx={{
                      bgcolor: 'rgba(30,30,40,0.85)',
                      borderRadius: 2,
                      fontSize: '1.15rem',
                      fontWeight: 500,
                      boxShadow: '0 2px 8px 0 rgba(124,58,237,0.04)',
                      '& .MuiOutlinedInput-root': {
                        borderRadius: 2,
                      },
                      '& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline': {
                        borderColor: 'var(--main-color)',
                      },
                    }}
                  />
                  {topics.length > 0 && (
                    <Box
                      component={motion.div}
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      exit={{ opacity: 0, y: 10 }}
                      transition={{ duration: 0.25, ease: 'easeOut' }}
                      sx={{ width: '100%', position: 'absolute', top: '100%', left: 0, zIndex: 10 }}
                    >
                      {suggestedTopics}
                    </Box>
                  )}
                  <Button
                    component={motion.button}
                    initial={{ opacity: 0, scale: 0.9 }}
                    animate={{ opacity: 1, scale: 1 }}
                    transition={{ 
                      duration: 0.5, 
                      delay: 1.0,
                      type: "spring",
                      stiffness: 400,
                      damping: 15
                    }}
                    type="submit"
                    variant="contained"
                    disabled={!topicInput || isGenerating}
                    sx={{
                      bgcolor: 'var(--main-color)',
                      color: 'var(--bg-color)',
                      px: 5,
                      py: 1.5,
                      fontSize: '1.1rem',
                      fontWeight: 600,
                      borderRadius: 2,
                      minWidth: { xs: '100%', sm: 140 },
                      boxShadow: '0 2px 8px 0 rgba(124,58,237,0.10)',
                      transition: 'transform 0.18s cubic-bezier(.4,2,.6,1), background 0.18s',
                      '&:hover': {
                        bgcolor: 'var(--caret-color)',
                        transform: 'scale(1.045)',
                      },
                      '&:disabled': {
                        bgcolor: 'var(--sub-alt-color)',
                        color: 'var(--sub-color)',
                      },
                    }}
                  >
                    {isGenerating ? t('hero.generating') : t('hero.generate')}
                  </Button>
                </Box>
              </form>
            </Paper>
          </Stack>
        </Box>
      </Container>
    </Box>
  );
});

Hero.displayName = 'Hero';

export default Hero;
