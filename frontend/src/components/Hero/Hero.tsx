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

import AnimatedTopics from './AnimatedTopics';
import { useCustomTheme } from '../../context/CustomThemeContext';
import { useQuizOptions } from '../../context/QuizOptionsContext';
import { useQuizState } from '../../context/QuizStateContext';
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
  const { currentTheme } = useCustomTheme();

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
          aiService: quizOptions.selectedService,
          model: quizOptions.selectedModel,
          language: quizOptions.language,
          numQuestions: quizOptions.numQuestions,
          numOptions: quizOptions.numOptions,
          extractLength: quizOptions.extractLength,
        };

        const quiz = await quizApi.createBasicQuiz(requestData);

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

  return (
    <Box
      id="hero"
      className={`hero-section theme-${currentTheme}`}
      sx={backgroundStyle}
    >
      <Container
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          pt: { xs: 14, sm: 20 },
          pb: { xs: 8, sm: 12 },
        }}
      >
        <Stack
          spacing={2}
          useFlexGap
          sx={{ alignItems: 'center', width: { xs: '100%', sm: '70%' } }}
        >
          <Typography
            variant="h1"
            sx={{
              display: 'flex',
              flexDirection: { xs: 'column', sm: 'row' },
              alignItems: 'center',
              fontSize: 'clamp(3rem, 10vw, 3.5rem)',
              color: 'var(--text-color)',
            }}
          >
            <span style={{ whiteSpace: 'nowrap' }}>
              {t('hero.generateQuiz')}&nbsp;
            </span>
            <AnimatedTopics />
          </Typography>
          <Typography
            sx={{
              textAlign: 'center',
              color: 'var(--sub-color)',
              width: { sm: '100%', md: '80%' },
            }}
          >
            {t('hero.topicInfo')}
          </Typography>
          <form
            onSubmit={handleSubmit}
            style={{
              display: 'flex',
              alignItems: 'center',
              width: '60%',
              maxWidth: 600,
            }}
          >
            <Box
              sx={{ position: 'relative', flexGrow: 1, mr: 1 }}
              ref={dropdownRef}
            >
              <TextField
                id="quiz-topic"
                hiddenLabel
                size="small"
                variant="outlined"
                aria-label="Enter your quiz topic"
                placeholder={t('hero.placeholder')}
                value={topicInput}
                onChange={handleTopicChange}
                error={!!error}
                helperText={error}
                disabled={isGenerating}
                fullWidth
                autoComplete="off"
                inputProps={{
                  style: {
                    textAlign: 'left',
                    backgroundColor: 'var(--bg-color-secondary)',
                    color: 'var(--text-color)',
                  },
                }}
                sx={{
                  '& .MuiOutlinedInput-root': {
                    '& fieldset': {
                      borderColor: 'var(--sub-color)',
                    },
                    '&:hover fieldset': {
                      borderColor: 'var(--main-color)',
                    },
                    '&.Mui-focused fieldset': {
                      borderColor: 'var(--main-color)',
                    },
                  },
                  '& .MuiFormHelperText-root': {
                    color: 'var(--error-color)',
                  },
                }}
              />
              {suggestedTopics}
            </Box>
            <Button
              disabled={isGenerating}
              type="submit"
              variant="contained"
              sx={{
                backgroundColor: 'var(--main-color)',
                color: 'var(--bg-color)',
                '&:hover': {
                  backgroundColor: 'var(--caret-color)',
                },
                whiteSpace: 'nowrap',
              }}
            >
              {isGenerating ? t('hero.generating') : t('hero.generate')}
            </Button>
          </form>
        </Stack>
      </Container>
    </Box>
  );
});

// Add display name
Hero.displayName = 'Hero';

export default Hero;
