import CloseRoundedIcon from '@mui/icons-material/CloseRounded';
import MenuIcon from '@mui/icons-material/Menu';
import { Typography, IconButton } from '@mui/material';
import AppBar from '@mui/material/AppBar';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Container from '@mui/material/Container';
import Divider from '@mui/material/Divider';
import Drawer from '@mui/material/Drawer';
import MenuItem from '@mui/material/MenuItem';
import Toolbar from '@mui/material/Toolbar';
import * as React from 'react';
import { useTranslation } from 'react-i18next';

import AuthControls from './Account/AuthControls';
import LanguageToggle from './LanguageSelection';
import QuizOptionsComponent from './QuizOptionsComponent';

const QuizAppBar: React.FC = () => {
  const [open, setOpen] = React.useState(false);
  const [appBarState, setAppBarState] = React.useState<'docked' | 'visible'>(
    'docked',
  );
  const { t } = useTranslation();

  const timerRef = React.useRef<number | null>(null);

  const showAppBar = React.useCallback(() => {
    setAppBarState('visible');
    if (timerRef.current !== null) {
      clearTimeout(timerRef.current);
    }
    timerRef.current = window.setTimeout(() => {
      setAppBarState('docked');
    }, 3000);
  }, []);

  React.useEffect(() => {
    const handleScroll = () => {
      showAppBar();
    };

    const handleMouseMove = (e: MouseEvent) => {
      if (e.clientY < 100) {
        showAppBar();
      }
    };

    window.addEventListener('scroll', handleScroll);
    window.addEventListener('mousemove', handleMouseMove);

    return () => {
      if (timerRef.current !== null) {
        clearTimeout(timerRef.current);
      }
      window.removeEventListener('scroll', handleScroll);
      window.removeEventListener('mousemove', handleMouseMove);
    };
  }, [showAppBar]);

  const toggleDrawer = (newOpen: boolean) => () => {
    setOpen(newOpen);
  };

  const scrollToSection = (sectionId: string) => {
    const sectionElement = document.getElementById(sectionId);
    const offset = 128;
    if (sectionElement) {
      const targetScroll = sectionElement.offsetTop - offset;
      sectionElement.scrollIntoView({ behavior: 'smooth' });
      window.scrollTo({
        top: targetScroll,
        behavior: 'smooth',
      });
      setOpen(false);
    }
  };

  const getTransformValue = () => {
    switch (appBarState) {
      case 'visible':
        return 'translateY(0)';
      case 'docked':
      default:
        return 'translateY(-90%)';
    }
  };

  return (
    <AppBar
      position="fixed"
      sx={{
        boxShadow: 0,
        bgcolor: 'transparent',
        backgroundImage: 'none',
        mt: 2,
        transition: 'transform 0.3s cubic-bezier(.4,2,.6,1)',
        transform: getTransformValue(),
        '&:hover': {
          transform: 'translateY(0)',
        },
      }}
    >
      <Container maxWidth="lg">
        <Toolbar
          variant="regular"
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            flexShrink: 0,
            borderRadius: '999px',
            backdropFilter: 'blur(24px)',
            height: 40,
            border: '1px solid',
            borderColor: 'divider',
            bgcolor: 'hsla(220, 60%, 99%, 0.6)',
            boxShadow:
              '0 1px 2px hsla(210, 0%, 0%, 0.05), 0 2px 12px hsla(210, 100%, 80%, 0.5)',
          }}
        >
          <Box
            sx={{ flexGrow: 1, display: 'flex', alignItems: 'center', px: 0 }}
          >
            <LanguageToggle />
            <Typography color="text.primary" sx={{ mr: 2, ml: 2 }}>
              {t('appBar.title')}
            </Typography>
            <Box sx={{ display: { xs: 'none', md: 'flex' } }}>
              <Button
                variant="text"
                color="primary"
                size="small"
                onClick={() => scrollToSection('highlights')}
              >
                {t('appBar.highlights')}
              </Button>
            </Box>
          </Box>
          <Box
            sx={{
              display: { xs: 'none', md: 'flex' },
              gap: 0.5,
              alignItems: 'center',
            }}
          >
            <QuizOptionsComponent />
            <AuthControls />
          </Box>
          <Box sx={{ display: { sm: 'flex', md: 'none' } }}>
            <IconButton
              aria-label="Menu button"
              onClick={toggleDrawer(true)}
              color="primary"
            >
              <MenuIcon />
            </IconButton>
            <Drawer anchor="top" open={open} onClose={toggleDrawer(false)}>
              <Box sx={{ p: 2, backgroundColor: 'background.default' }}>
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                  }}
                >
                  <IconButton onClick={toggleDrawer(false)}>
                    <CloseRoundedIcon />
                  </IconButton>
                </Box>
                <Divider sx={{ my: 2 }} />
                <MenuItem onClick={() => scrollToSection('highlights')}>
                  {t('appBar.highlights')}
                </MenuItem>
                <MenuItem>
                  <AuthControls />
                </MenuItem>
              </Box>
            </Drawer>
          </Box>
        </Toolbar>
      </Container>
    </AppBar>
  );
};

QuizAppBar.displayName = 'QuizAppBar';

export default QuizAppBar;
