import * as React from 'react';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Container from '@mui/material/Container';
import InputLabel from '@mui/material/InputLabel';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';
import { visuallyHidden } from '@mui/utils';
import { styled } from '@mui/material/styles';
import AnimatedTopics from './AnimatedTopics';
import { useGlobalQuiz } from '../../context/GlobalQuizContext';
import { useQuizService } from '../../services/quizService';

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
  const { quizOptions, setTopic, setIsQuizReady, setCurrentQuiz } = useGlobalQuiz();
  const { generateQuiz } = useQuizService();
  const { t } = useTranslation();
  const [topicInput, setTopicInput] = React.useState('');

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (topicInput && !quizOptions.isGenerating) {
      setTopic(topicInput);
      try {
        const quiz = await generateQuiz();
        setCurrentQuiz(quiz);
        setIsQuizReady(true);
      } catch (error) {
        console.error('Failed to generate quiz:', error);
        // Here show an error message to the user
      }
    }
  };

  const handleTopicChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setTopicInput(event.target.value);
    setTopic(event.target.value);
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
          <Stack
            component="form"
            onSubmit={handleSubmit}
            direction={{ xs: 'column', sm: 'row' }}
            spacing={1}
            useFlexGap
            sx={{ pt: 2, width: { xs: '100%', sm: 'auto' } }}
          >
            <InputLabel htmlFor="quiz-topic" sx={visuallyHidden}>
              Quiz Topic
            </InputLabel>
            <TextField
              id="quiz-topic"
              hiddenLabel
              size="small"
              variant="outlined"
              aria-label="Enter your quiz topic"
              placeholder={t('hero.placeholder')}
              value={topicInput}
              onChange={handleTopicChange}
              disabled={quizOptions.isGenerating}
              slotProps={{
                htmlInput: {
                  autoComplete: 'off',
                  'aria-label': 'Enter your quiz topic',
                },
              }}
            />
            <Button 
              type="submit" 
              variant="contained" 
              color="primary"
              disabled={quizOptions.isGenerating || !topicInput}
            >
              {quizOptions.isGenerating ? t('hero.generatingButtonText') : t('hero.startButtonText')}
            </Button>
          </Stack>
        </Stack>
      </Container>
    </Box>
  );
}