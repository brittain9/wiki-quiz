import AutoFixHighRoundedIcon from '@mui/icons-material/AutoFixHighRounded';
import ConstructionRoundedIcon from '@mui/icons-material/ConstructionRounded';
import QueryStatsRoundedIcon from '@mui/icons-material/QueryStatsRounded';
import SettingsSuggestRoundedIcon from '@mui/icons-material/SettingsSuggestRounded';
import SupportAgentRoundedIcon from '@mui/icons-material/SupportAgentRounded';
import ThumbUpAltRoundedIcon from '@mui/icons-material/ThumbUpAltRounded';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Container from '@mui/material/Container';
import Grid from '@mui/material/Grid';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { motion } from 'framer-motion';
import * as React from 'react';
import { useMemo } from 'react';
import { useTranslation } from 'react-i18next';

// Define a type for the highlight item
interface HighlightItem {
  icon: React.ReactNode;
  title: string;
  description: string;
}

const HighlightItem = React.memo(
  ({ item, index }: { item: HighlightItem; index: number }) => {
    return (
      <Grid item xs={12} sm={6} md={4} key={index}>
        <Box
          component={motion.div}
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{
            duration: 0.3,
            delay: 0.1 + index * 0.05,
            ease: 'easeOut',
          }}
          sx={{ height: '100%' }}
        >
          <Stack
            direction="column"
            component={Card}
            spacing={2}
            useFlexGap
            sx={{
              color: 'var(--text-color)',
              p: 3,
              height: '100%',
              border: '1px solid var(--sub-alt-color)',
              backgroundColor: 'var(--bg-color)',
              boxShadow: 'none',
              borderRadius: 2,
              position: 'relative',
              overflow: 'hidden',
              '&:hover': {
                borderColor: 'var(--main-color)',
                transform: 'translateY(-4px)',
                boxShadow: '0 8px 20px rgba(0,0,0,0.1)',
                '& .highlight-icon': {
                  color: 'var(--main-color)',
                  opacity: 0.9,
                },
              },
              '&::before': {
                content: '""',
                position: 'absolute',
                top: 0,
                left: 0,
                width: '4px',
                height: '100%',
                backgroundColor: 'var(--main-color)',
                opacity: 0.6,
              },
            }}
          >
            <Box
              className="highlight-icon"
              sx={{
                color: 'var(--main-color)',
                opacity: 0.7,
                fontSize: '2rem',
                '& svg': {
                  fontSize: '2rem',
                },
              }}
            >
              {item.icon}
            </Box>
            <div>
              <Typography
                gutterBottom
                sx={{
                  fontWeight: 600,
                  color: 'var(--text-color)',
                  fontSize: '1.1rem',
                }}
              >
                {item.title}
              </Typography>
              <Typography variant="body2" sx={{ color: 'var(--sub-color)' }}>
                {item.description}
              </Typography>
            </div>
          </Stack>
        </Box>
      </Grid>
    );
  },
);

// Add display name
HighlightItem.displayName = 'HighlightItem';

const Highlights = React.memo(() => {
  const { t } = useTranslation();

  const items = useMemo(
    () => [
      {
        icon: <SettingsSuggestRoundedIcon />,
        title: t('highlights.items.dynamic_quiz.title'),
        description: t('highlights.items.dynamic_quiz.desc'),
      },
      {
        icon: <ConstructionRoundedIcon />,
        title: t('highlights.items.inclusive_design.title'),
        description: t('highlights.items.inclusive_design.desc'),
      },
      {
        icon: <ThumbUpAltRoundedIcon />,
        title: t('highlights.items.multilingual_support.title'),
        description: t('highlights.items.multilingual_support.desc'),
      },
      {
        icon: <AutoFixHighRoundedIcon />,
        title: t('highlights.items.intelligent_search.title'),
        description: t('highlights.items.intelligent_search.desc'),
      },
      {
        icon: <SupportAgentRoundedIcon />,
        title: t('highlights.items.ai_integration.title'),
        description: t('highlights.items.ai_integration.desc'),
      },
      {
        icon: <QueryStatsRoundedIcon />,
        title: t('highlights.items.meticulous_design.title'),
        description: t('highlights.items.meticulous_design.desc'),
      },
    ],
    [t],
  );

  return (
    <Box
      id="highlights"
      component={motion.div}
      initial={{ opacity: 0.3 }}
      animate={{ opacity: 1 }}
      transition={{ duration: 0.4, ease: 'easeOut' }}
      sx={{
        pt: { xs: 4, sm: 8 },
        pb: { xs: 6, sm: 10 },
        backgroundColor: 'var(--bg-color-secondary)',
        color: 'var(--text-color)',
        mt: 4,
        width: '100%',
      }}
    >
      <Container
        sx={{
          position: 'relative',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: { xs: 3, sm: 6 },
        }}
      >
        <Box
          component={motion.div}
          initial={{ opacity: 0.3, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3, delay: 0.1 }}
          sx={{
            width: { sm: '100%', md: '60%' },
            textAlign: { sm: 'left', md: 'center' },
          }}
        >
          <Typography
            component="h2"
            variant="h4"
            sx={{
              color: 'var(--main-color)',
              fontWeight: 600,
              mb: 2,
            }}
          >
            {t('highlights.title')}
          </Typography>
          <Typography variant="body1" sx={{ color: 'var(--sub-color)' }}>
            {t('highlights.info')}
          </Typography>
        </Box>
        <Grid container spacing={3}>
          {items.map((item, index) => (
            <HighlightItem key={index} item={item} index={index} />
          ))}
        </Grid>
      </Container>
    </Box>
  );
});

// Add display name
Highlights.displayName = 'Highlights';

export default Highlights;
