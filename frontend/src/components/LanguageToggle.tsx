import React from 'react';
import { useTranslation } from 'react-i18next';
import { IconButton, Menu, MenuItem } from '@mui/material';
import LanguageIcon from '@mui/icons-material/Language';

const LanguageToggle: React.FC = () => {
  const { i18n } = useTranslation();
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const changeLanguage = (lang: string) => {
    i18n.changeLanguage(lang);
    handleClose();
  };

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
        PaperProps={{
          style: {
            maxHeight: 48 * 4.5,
            width: '120px',
          },
        }}
      >
        <MenuItem onClick={() => changeLanguage('en')}>English</MenuItem>
        <MenuItem onClick={() => changeLanguage('de')}>Deutsch</MenuItem>
        <MenuItem onClick={() => changeLanguage('es')}>Español</MenuItem>
        <MenuItem onClick={() => changeLanguage('zh')}>中文</MenuItem>
        <MenuItem onClick={() => changeLanguage('ja')}>日本語</MenuItem>
        <MenuItem onClick={() => changeLanguage('ru')}>Русский</MenuItem>
        <MenuItem onClick={() => changeLanguage('fr')}>Français</MenuItem>
        <MenuItem onClick={() => changeLanguage('it')}>Italiano</MenuItem>
        <MenuItem onClick={() => changeLanguage('pt')}>Português</MenuItem>

      </Menu>
    </>
  );
};

export default LanguageToggle;
