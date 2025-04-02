import CloseIcon from '@mui/icons-material/Close';
import Avatar from '@mui/material/Avatar';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Fade from '@mui/material/Fade';
import IconButton from '@mui/material/IconButton';
import Modal from '@mui/material/Modal';
import Paper from '@mui/material/Paper';
import Typography from '@mui/material/Typography';
import React from 'react';
import { useTranslation } from 'react-i18next';

import { useAuth } from '../../context/AuthProvider';
import { useCustomTheme } from '../../context/CustomThemeContext';
import { useOverlay } from '../../context/OverlayContext';

const AccountOverlay: React.FC = () => {
  const { userInfo } = useAuth();
  const { currentOverlay, hideOverlay } = useOverlay();
  const { t } = useTranslation();
  const { currentTheme } = useCustomTheme();

  const isOpen = currentOverlay === 'account';

  // Calculate the avatar initials from first and last name
  const getInitials = () => {
    if (!userInfo) return '?';

    const firstName = userInfo.firstName || '';
    const lastName = userInfo.lastName || '';

    return (
      `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase() ||
      firstName.charAt(0).toUpperCase() ||
      '?'
    );
  };

  if (!userInfo) {
    return null;
  }

  return (
    <Modal
      open={isOpen}
      onClose={hideOverlay}
      closeAfterTransition
      aria-labelledby="account-modal-title"
      className={`theme-${currentTheme}`}
    >
      <Fade in={isOpen}>
        <Box
          sx={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            width: { xs: '90%', sm: 500 },
            maxWidth: 500,
            bgcolor: 'var(--bg-color)',
            borderRadius: 2,
            boxShadow: 24,
            p: 0,
            outline: 'none',
          }}
          className={`theme-${currentTheme}`}
        >
          <Paper
            elevation={0}
            sx={{
              position: 'relative',
              p: 0,
              borderRadius: 2,
              overflow: 'hidden',
              backgroundColor: 'var(--bg-color)',
              color: 'var(--text-color)',
            }}
          >
            <IconButton
              aria-label="close"
              onClick={hideOverlay}
              sx={{
                position: 'absolute',
                right: 12,
                top: 12,
                color: 'white',
                bgcolor: 'rgba(0,0,0,0.2)',
                '&:hover': {
                  bgcolor: 'rgba(0,0,0,0.4)',
                },
                zIndex: 10,
              }}
            >
              <CloseIcon />
            </IconButton>

            {/* Header with user info */}
            <Box
              sx={{
                p: 5,
                background:
                  'linear-gradient(120deg, var(--main-color), var(--caret-color))',
                color: 'var(--bg-color)',
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
              }}
            >
              {userInfo.profilePicture ? (
                <Avatar
                  src={userInfo.profilePicture}
                  alt={userInfo.firstName}
                  sx={{ width: 120, height: 120, mb: 3, boxShadow: 2 }}
                />
              ) : (
                <Avatar
                  sx={{
                    width: 120,
                    height: 120,
                    mb: 3,
                    fontSize: '3rem',
                    bgcolor: 'var(--main-color-light)',
                    color: 'var(--bg-color)',
                    boxShadow: 2,
                  }}
                >
                  {getInitials()}
                </Avatar>
              )}
              <Typography variant="h4" component="h2" gutterBottom>
                {userInfo.firstName} {userInfo.lastName}
              </Typography>
              <Typography variant="body1" sx={{ opacity: 0.8 }}>
                {userInfo.email}
              </Typography>
            </Box>

            {/* Footer */}
            <Box sx={{ p: 3, textAlign: 'center' }}>
              <Typography
                variant="body2"
                sx={{ mb: 2, color: 'var(--sub-color)' }}
              >
                {t('account.profileMessage')}
              </Typography>
              <Button
                variant="outlined"
                onClick={hideOverlay}
                sx={{
                  minWidth: 120,
                  borderColor: 'var(--main-color)',
                  color: 'var(--main-color)',
                  '&:hover': {
                    borderColor: 'var(--caret-color)',
                    backgroundColor: 'rgba(var(--main-color-rgb), 0.1)',
                  },
                }}
              >
                {t('button.close')}
              </Button>
            </Box>
          </Paper>
        </Box>
      </Fade>
    </Modal>
  );
};

export default AccountOverlay;
