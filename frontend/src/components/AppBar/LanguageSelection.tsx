import LanguageIcon from '@mui/icons-material/Language';
import { IconButton, Menu, MenuItem } from '@mui/material';
import React from 'react';
import { useTranslation } from 'react-i18next';

import { useQuizOptions } from '../../context';

const LanguageToggle: React.FC = () => {
  const { quizOptions, setLanguage } = useQuizOptions();
  const { i18n } = useTranslation();
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const [previewLanguage, setPreviewLanguage] = React.useState<string | null>(
    null,
  );

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
    setPreviewLanguage(null);
  };

  // The context class will handle the change in i18n
  const changeLanguage = (lang: string) => {
    setLanguage(lang);
    handleClose();
  };

  // Preview language on hover
  const handleLanguageHover = (lang: string) => {
    if (lang !== previewLanguage) {
      setPreviewLanguage(lang);
      i18n.changeLanguage(lang);
    }
  };

  // Revert to selected language when mouse leaves
  const handleMenuMouseLeave = () => {
    if (previewLanguage !== null && previewLanguage !== quizOptions.language) {
      setPreviewLanguage(null);
      i18n.changeLanguage(quizOptions.language);
    }
  };

  // Make sure we reset preview if menu closes
  React.useEffect(() => {
    if (
      !anchorEl &&
      previewLanguage !== null &&
      previewLanguage !== quizOptions.language
    ) {
      setPreviewLanguage(null);
      i18n.changeLanguage(quizOptions.language);
    }
  }, [anchorEl, previewLanguage, quizOptions.language, i18n]);

  return (
    <>
      <IconButton
        color="primary"
        aria-label="change language"
        onClick={handleClick}
        size="small"
      >
        <LanguageIcon fontSize="small" />
      </IconButton>
      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleClose}
        onMouseLeave={handleMenuMouseLeave}
        PaperProps={{
          style: {
            maxHeight: 48 * 4.5,
            width: '120px',
          },
        }}
      >
        <MenuItem
          onClick={() => changeLanguage('en')}
          onMouseEnter={() => handleLanguageHover('en')}
          selected={quizOptions.language === 'en'}
        >
          English
        </MenuItem>
        <MenuItem
          onClick={() => changeLanguage('de')}
          onMouseEnter={() => handleLanguageHover('de')}
          selected={quizOptions.language === 'de'}
        >
          Deutsch
        </MenuItem>
        <MenuItem
          onClick={() => changeLanguage('es')}
          onMouseEnter={() => handleLanguageHover('es')}
          selected={quizOptions.language === 'es'}
        >
          Español
        </MenuItem>
        <MenuItem
          onClick={() => changeLanguage('zh')}
          onMouseEnter={() => handleLanguageHover('zh')}
          selected={quizOptions.language === 'zh'}
        >
          中文
        </MenuItem>
        <MenuItem
          onClick={() => changeLanguage('ja')}
          onMouseEnter={() => handleLanguageHover('ja')}
          selected={quizOptions.language === 'ja'}
        >
          日本語
        </MenuItem>
        <MenuItem
          onClick={() => changeLanguage('ru')}
          onMouseEnter={() => handleLanguageHover('ru')}
          selected={quizOptions.language === 'ru'}
        >
          Русский
        </MenuItem>
        <MenuItem
          onClick={() => changeLanguage('fr')}
          onMouseEnter={() => handleLanguageHover('fr')}
          selected={quizOptions.language === 'fr'}
        >
          Français
        </MenuItem>
        <MenuItem
          onClick={() => changeLanguage('it')}
          onMouseEnter={() => handleLanguageHover('it')}
          selected={quizOptions.language === 'it'}
        >
          Italiano
        </MenuItem>
        <MenuItem
          onClick={() => changeLanguage('pt')}
          onMouseEnter={() => handleLanguageHover('pt')}
          selected={quizOptions.language === 'pt'}
        >
          Português
        </MenuItem>
      </Menu>
    </>
  );
};

export default LanguageToggle;
