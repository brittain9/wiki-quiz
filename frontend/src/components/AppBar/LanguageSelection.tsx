import LanguageIcon from '@mui/icons-material/Language';
import { IconButton, Menu, MenuItem, Grow } from '@mui/material';
import React, { useCallback } from 'react';
import { useTranslation } from 'react-i18next';

import { useQuizOptions } from '../../context';

const LanguageToggle: React.FC = () => {
  const { quizOptions, setLanguage } = useQuizOptions();
  const { i18n } = useTranslation();
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const [previewLanguage, setPreviewLanguage] = React.useState<string | null>(
    null,
  );
  const [isTransitioning, setIsTransitioning] = React.useState(false);

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
    setPreviewLanguage(null);
  };

  // The context class will handle the change in i18n
  const changeLanguage = useCallback(
    (lang: string) => {
      setIsTransitioning(true);
      setLanguage(lang);

      // Reset transitioning state after animation completes
      setTimeout(() => {
        setIsTransitioning(false);
      }, 350); // Slightly longer than the CSS transition

      handleClose();
    },
    [setLanguage],
  );

  // Preview language on hover
  const handleLanguageHover = (lang: string) => {
    if (lang !== previewLanguage && !isTransitioning) {
      setPreviewLanguage(lang);
      i18n.changeLanguage(lang);
    }
  };

  // Revert to selected language when mouse leaves
  const handleMenuMouseLeave = () => {
    if (
      previewLanguage !== null &&
      previewLanguage !== quizOptions.language &&
      !isTransitioning
    ) {
      setPreviewLanguage(null);
      i18n.changeLanguage(quizOptions.language);
    }
  };

  // Make sure we reset preview if menu closes
  React.useEffect(() => {
    if (
      !anchorEl &&
      previewLanguage !== null &&
      previewLanguage !== quizOptions.language &&
      !isTransitioning
    ) {
      setPreviewLanguage(null);
      i18n.changeLanguage(quizOptions.language);
    }
  }, [anchorEl, previewLanguage, quizOptions.language, i18n, isTransitioning]);

  return (
    <>
      <IconButton
        color="primary"
        aria-label="change language"
        onClick={handleClick}
        size="small"
        disabled={isTransitioning}
      >
        <LanguageIcon fontSize="small" />
      </IconButton>
      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleClose}
        onMouseLeave={handleMenuMouseLeave}
        TransitionComponent={Grow}
        TransitionProps={{
          timeout: 250,
        }}
        PaperProps={{
          style: {
            maxHeight: 48 * 4.5,
            width: '120px',
          },
          elevation: 6,
          sx: {
            backgroundColor: 'var(--bg-color)',
            color: 'var(--text-color)',
            borderRadius: 1,
            border: '1px solid var(--sub-alt-color)',
          },
        }}
      >
        <MenuItem
          onClick={() => changeLanguage('en')}
          onMouseEnter={() => handleLanguageHover('en')}
          selected={quizOptions.language === 'en'}
          disabled={isTransitioning}
          sx={{
            backgroundColor:
              quizOptions.language === 'en'
                ? 'var(--main-color-10)'
                : 'transparent',
            '&:hover': {
              backgroundColor: 'var(--bg-color-secondary)',
            },
          }}
        >
          English
        </MenuItem>
        <MenuItem
          onClick={() => changeLanguage('de')}
          onMouseEnter={() => handleLanguageHover('de')}
          selected={quizOptions.language === 'de'}
          disabled={isTransitioning}
          sx={{
            backgroundColor:
              quizOptions.language === 'de'
                ? 'var(--main-color-10)'
                : 'transparent',
            '&:hover': {
              backgroundColor: 'var(--bg-color-secondary)',
            },
          }}
        >
          Deutsch
        </MenuItem>
        <MenuItem
          onClick={() => changeLanguage('es')}
          onMouseEnter={() => handleLanguageHover('es')}
          selected={quizOptions.language === 'es'}
          disabled={isTransitioning}
          sx={{
            backgroundColor:
              quizOptions.language === 'es'
                ? 'var(--main-color-10)'
                : 'transparent',
            '&:hover': {
              backgroundColor: 'var(--bg-color-secondary)',
            },
          }}
        >
          Español
        </MenuItem>
        <MenuItem
          onClick={() => changeLanguage('zh')}
          onMouseEnter={() => handleLanguageHover('zh')}
          selected={quizOptions.language === 'zh'}
          disabled={isTransitioning}
          sx={{
            backgroundColor:
              quizOptions.language === 'zh'
                ? 'var(--main-color-10)'
                : 'transparent',
            '&:hover': {
              backgroundColor: 'var(--bg-color-secondary)',
            },
          }}
        >
          中文
        </MenuItem>
        <MenuItem
          onClick={() => changeLanguage('ja')}
          onMouseEnter={() => handleLanguageHover('ja')}
          selected={quizOptions.language === 'ja'}
          disabled={isTransitioning}
          sx={{
            backgroundColor:
              quizOptions.language === 'ja'
                ? 'var(--main-color-10)'
                : 'transparent',
            '&:hover': {
              backgroundColor: 'var(--bg-color-secondary)',
            },
          }}
        >
          日本語
        </MenuItem>
        <MenuItem
          onClick={() => changeLanguage('ru')}
          onMouseEnter={() => handleLanguageHover('ru')}
          selected={quizOptions.language === 'ru'}
          disabled={isTransitioning}
          sx={{
            backgroundColor:
              quizOptions.language === 'ru'
                ? 'var(--main-color-10)'
                : 'transparent',
            '&:hover': {
              backgroundColor: 'var(--bg-color-secondary)',
            },
          }}
        >
          Русский
        </MenuItem>
        <MenuItem
          onClick={() => changeLanguage('fr')}
          onMouseEnter={() => handleLanguageHover('fr')}
          selected={quizOptions.language === 'fr'}
          disabled={isTransitioning}
          sx={{
            backgroundColor:
              quizOptions.language === 'fr'
                ? 'var(--main-color-10)'
                : 'transparent',
            '&:hover': {
              backgroundColor: 'var(--bg-color-secondary)',
            },
          }}
        >
          Français
        </MenuItem>
        <MenuItem
          onClick={() => changeLanguage('it')}
          onMouseEnter={() => handleLanguageHover('it')}
          selected={quizOptions.language === 'it'}
          disabled={isTransitioning}
          sx={{
            backgroundColor:
              quizOptions.language === 'it'
                ? 'var(--main-color-10)'
                : 'transparent',
            '&:hover': {
              backgroundColor: 'var(--bg-color-secondary)',
            },
          }}
        >
          Italiano
        </MenuItem>
        <MenuItem
          onClick={() => changeLanguage('pt')}
          onMouseEnter={() => handleLanguageHover('pt')}
          selected={quizOptions.language === 'pt'}
          disabled={isTransitioning}
          sx={{
            backgroundColor:
              quizOptions.language === 'pt'
                ? 'var(--main-color-10)'
                : 'transparent',
            '&:hover': {
              backgroundColor: 'var(--bg-color-secondary)',
            },
          }}
        >
          Português
        </MenuItem>
      </Menu>
    </>
  );
};

export default LanguageToggle;
