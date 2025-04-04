import FacebookIcon from '@mui/icons-material/GitHub';
import Box from '@mui/material/Box';
import Container from '@mui/material/Container';
import IconButton from '@mui/material/IconButton';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import React from 'react';
import { useTranslation } from 'react-i18next';

import { useCustomTheme } from '../context/CustomThemeContext';

const Footer = React.memo(() => {
  const { t } = useTranslation();
  const { currentTheme } = useCustomTheme();

  return (
    <Box
      component="footer"
      className={`theme-${currentTheme}`}
      sx={{
        width: '100%',
        py: { xs: 3, sm: 4 },
        color: 'var(--text-color)',
        backgroundColor: 'var(--bg-color)',
      }}
    >
      <Container
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: { xs: 2, sm: 3 },
        }}
      >
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            width: '100%',
          }}
        >
          <Typography variant="body1" sx={{ color: 'var(--sub-color)' }}>
            {t('footer.title')}
          </Typography>
          <Stack
            direction="row"
            spacing={1}
            useFlexGap
            sx={{ justifyContent: 'left' }}
          >
            <IconButton
              href="https://github.com/brittain9/wiki-quiz"
              aria-label="GitHub"
              sx={{
                alignSelf: 'center',
                color: 'var(--sub-color)',
                '&:hover': {
                  color: 'var(--main-color)',
                  backgroundColor: 'var(--main-color-10)',
                },
              }}
            >
              <FacebookIcon />
            </IconButton>
          </Stack>
        </Box>
      </Container>
    </Box>
  );
});

Footer.displayName = 'Footer';

export default Footer;
