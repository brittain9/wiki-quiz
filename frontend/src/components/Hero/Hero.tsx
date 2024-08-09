import React, { useEffect, useState, useRef } from 'react';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Container from '@mui/material/Container';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import { Paper, List, ListItem, ListItemButton } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { styled } from '@mui/material/styles';
import AnimatedTopics from './AnimatedTopics';
import { useGlobalQuiz } from '../../context/GlobalQuizContext';
import { useQuizService } from '../../services/quizService';
import { fetchWikipediaTopics } from '../../services/wikiApi';

const StyledBox = styled('div')(({ theme }) => ({
  alignSelf: 'center',
  width: '100%',
  height: 400,
  marginTop: theme.spacing(8),
  borderRadius: theme.shape.borderRadius,
  outline: '1px solid',
  boxShadow: '0 0 12px 8px hsla(220, 25%, 80%, 0.2)',
  backgroundImage: `url(${'/static/images/templates/templates-images/hero-light.png'})`,
  outlineColor: 'hsla(220, 25%, 80%, 0.5)',
  backgroundSize: 'cover',
  [theme.breakpoints.up('sm')]: {
    marginTop: theme.spacing(10),
    height: 700,
  },
  ...theme.applyStyles('dark', {
    boxShadow: '0 0 24px 12px hsla(210, 100%, 25%, 0.2)',
    backgroundImage: `url(${'/static/images/templates/templates-images/hero-dark.png'})`,
    outlineColor: 'hsla(210, 100%, 80%, 0.1)',
  }),
}));

export default function Hero() {
  const { quizOptions, setTopic, setIsQuizReady, setCurrentQuiz, setIsGenerating } = useGlobalQuiz();
  const { generateQuiz } = useQuizService();
  const { t } = useTranslation();
  const [topicInput, setTopicInput] = useState('');
  const [topics, setTopics] = useState<string[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [debounceTimeout, setDebounceTimeout] = useState<number | null>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    // Add event listener to close the dropdown when clicking outside
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setTopics([]);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault(); // Prevent default form submission
    if (quizOptions.topic && !quizOptions.isGenerating) {
      try {
        setIsGenerating(true);
        const quiz = await generateQuiz();
        setCurrentQuiz(quiz);
        setIsQuizReady(true);
        setError(null);
      } catch (error: any) {
        setError(error.message);
      } finally {
        setIsGenerating(false);
      }
    }
  };

  const handleTopicChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setTopic(event.target.value);
    setTopicInput(event.target.value);
    setError(null);

    if (debounceTimeout) {
      clearTimeout(debounceTimeout);
    }

    const timeout = window.setTimeout(() => {
      if (event.target.value) {
        fetchWikipediaTopics(event.target.value)
          .then(setTopics)
          .catch(err => setError(err.message));
      } else {
        setTopics([]);
      }
    }, 1000);

    setDebounceTimeout(timeout);
  };

  const handleTopicSelect = (selectedTopic: string) => {
    if (!quizOptions.isGenerating) {
      setTopic(selectedTopic);
      setTopicInput(selectedTopic);
      setTopics([]);
    }
  };

  return (
    <Box
      id="hero"
      sx={(theme) => ({
        width: '100%',
        backgroundRepeat: 'no-repeat',
        backgroundImage:
          'radial-gradient(ellipse 80% 50% at 50% -20%, hsl(210, 100%, 90%), transparent)',
        ...theme.applyStyles('dark', {
          backgroundImage:
            'radial-gradient(ellipse 80% 50% at 50% -20%, hsl(210, 100%, 16%), transparent)',
        }),
      })}
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
            }}
          >
            {t('hero.generateQuiz')}&nbsp;
            <AnimatedTopics />
          </Typography>
          <Typography
            sx={{
              textAlign: 'center',
              color: 'text.secondary',
              width: { sm: '100%', md: '80%' },
            }}
          >
            {t('hero.topicInfo')}
          </Typography>
          <form onSubmit={handleSubmit} style={{ display: 'flex', alignItems: 'center', width: '60%', maxWidth: 600 }}>
            <Box sx={{ position: 'relative', flexGrow: 1, mr: 1 }} ref={dropdownRef}>
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
                disabled={quizOptions.isGenerating}
                fullWidth
                autoComplete="off"
                inputProps={{ style: { textAlign: 'left' } }}
              />
              {topics.length > 0 && (
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
                  }}
                >
                  <List>
                    {topics.map((topic, index) => (
                      <ListItem key={index} disablePadding>
                        <ListItemButton onClick={() => handleTopicSelect(topic)}>
                          {topic}
                        </ListItemButton>
                      </ListItem>
                    ))}
                  </List>
                </Paper>
              )}
            </Box>
            <Button 
              type="submit" 
              variant="contained" 
              color="primary"
              disabled={quizOptions.isGenerating || !topicInput}
            >
              {quizOptions.isGenerating ? t('hero.generatingButtonText') : t('hero.startButtonText')}
            </Button>
          </form>
        </Stack>
      </Container>
    </Box>
  );
}
