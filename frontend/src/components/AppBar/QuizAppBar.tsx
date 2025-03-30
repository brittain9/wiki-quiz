import CloseRoundedIcon from '@mui/icons-material/CloseRounded';
import MenuIcon from '@mui/icons-material/Menu';
import { PaletteMode, Typography, IconButton } from '@mui/material';
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

import LanguageToggle from './LanguageSelection';
import QuizOptionsComponent from './QuizOptionsComponent';
import ToggleColorMode from './ToggleColorMode';
import LoginButton from '../Auth/LoginButton';

interface AppBarProps {
  mode: PaletteMode;
  toggleColorMode: () => void;
}

const QuizAppBar: React.FC<AppBarProps> = ({ mode, toggleColorMode }) => {
  const [open, setOpen] = React.useState(false);
  const [visible, setVisible] = React.useState(true);
  const { t } = useTranslation();

  const timerRef = React.useRef<number | null>(null);

  const showAppBar = React.useCallback(() => {
    setVisible(true);
    if (timerRef.current !== null) {
      clearTimeout(timerRef.current);
    }
    timerRef.current = window.setTimeout(() => {
      setVisible(false);
    }, 3000);
  }, []);

  React.useEffect(() => {
    showAppBar();

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

  return (
    <AppBar
      position="fixed"
      sx={{
        boxShadow: 0,
        bgcolor: 'transparent',
        backgroundImage: 'none',
        mt: 2,
        transition: 'transform 0.3s ease-in-out',
        transform: visible ? 'translateY(0)' : 'translateY(-100%)',
      }}
    >
      <Container maxWidth="lg">
        <Toolbar
          variant="regular"
          sx={(theme) => ({
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
            ...theme.applyStyles('dark', {
              bgcolor: 'hsla(220, 0%, 0%, 0.7)',
              boxShadow:
                '0 1px 2px hsla(210, 0%, 0%, 0.5), 0 2px 12px hsla(210, 100%, 25%, 0.3)',
            }),
          })}
        >
          <Box
            sx={{ flexGrow: 1, display: 'flex', alignItems: 'center', px: 0 }}
          >
            <Typography color="text.primary" sx={{ mr: 2 }}>
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
            <LanguageToggle />
            <ToggleColorMode mode={mode} toggleColorMode={toggleColorMode} />
            <LoginButton />
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
                  <ToggleColorMode
                    mode={mode}
                    toggleColorMode={toggleColorMode}
                  />
                  <IconButton onClick={toggleDrawer(false)}>
                    <CloseRoundedIcon />
                  </IconButton>
                </Box>
                <Divider sx={{ my: 2 }} />
                <MenuItem onClick={() => scrollToSection('highlights')}>
                  {t('appBar.highlights')}
                </MenuItem>
                <MenuItem>
                  <LoginButton />
                </MenuItem>
              </Box>
            </Drawer>
          </Box>
        </Toolbar>
      </Container>
    </AppBar>
  );
};

export default QuizAppBar;
