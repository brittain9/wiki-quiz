import * as React from 'react';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Container from '@mui/material/Container';
import InputLabel from '@mui/material/InputLabel';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';

import { visuallyHidden } from '@mui/utils';
import { styled } from '@mui/material/styles';

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

const AnimatedTopics = () => {
  const topics = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H'];
  const [currentTopic, setCurrentTopic] = React.useState(0);

  React.useEffect(() => {
    const interval = setInterval(() => {
      setCurrentTopic((prev) => (prev + 1) % topics.length);
    }, 2000);
    return () => clearInterval(interval);
  }, []);

  return (
    <Typography
      component="span"
      variant="h1"
      sx={(theme) => ({
        fontSize: 'inherit',
        color: 'primary.main',
        transition: 'color 0.5s ease',
        ...theme.applyStyles('dark', {
          color: 'primary.light',
        }),
      })}
    >
      {topics[currentTopic]}
    </Typography>
  );
};

interface HeroProps {
  onStartQuiz: (topic: string) => void;
}

export default function Hero({ onStartQuiz }: HeroProps) {
  const [topic, setTopic] = React.useState('');
  const [isSubmitted, setIsSubmitted] = React.useState(false);

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    if (topic && !isSubmitted) {
      onStartQuiz(topic);
      setIsSubmitted(true);
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
            Generate a quiz about&nbsp;
            <AnimatedTopics />
          </Typography>
          <Typography
            sx={{
              textAlign: 'center',
              color: 'text.secondary',
              width: { sm: '100%', md: '80%' },
            }}
          >
            Enter any topic you're curious about, and we'll generate a custom quiz just for you. 
            From history to science, literature to pop culture - the possibilities are endless!
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
              placeholder="Your quiz topic"
              value={topic}
              onChange={(e) => setTopic(e.target.value)}
              disabled={isSubmitted}
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
              disabled={isSubmitted || !topic}
            >
              {isSubmitted ? 'Quiz Started' : 'Start now'}
            </Button>
          </Stack>
        </Stack>
      </Container>
    </Box>
  );
}