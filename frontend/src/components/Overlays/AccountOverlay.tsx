import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Fade from '@mui/material/Fade';
import IconButton from '@mui/material/IconButton';
import Modal from '@mui/material/Modal';
import Paper from '@mui/material/Paper';
import Typography from '@mui/material/Typography';
import Avatar from '@mui/material/Avatar';
import CloseIcon from '@mui/icons-material/Close';
import React from 'react';
import { useTranslation } from 'react-i18next';

import { useAuth } from '../../context/AuthProvider';
import { useOverlay } from '../../context/OverlayContext';

const AccountOverlay: React.FC = () => {
  const { userInfo } = useAuth();
  const { currentOverlay, hideOverlay } = useOverlay();
  const { t } = useTranslation();

  const isOpen = currentOverlay === 'account';

  // Calculate the avatar initials from first and last name
  const getInitials = () => {
    if (!userInfo) return '?';
    
    const firstName = userInfo.firstName || '';
    const lastName = userInfo.lastName || '';
    
    return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase() || firstName.charAt(0).toUpperCase() || '?';
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
            bgcolor: 'background.paper',
            borderRadius: 2,
            boxShadow: 24,
            p: 0,
            outline: 'none',
          }}
        >
          <Paper
            elevation={0}
            sx={{
              position: 'relative',
              p: 0,
              borderRadius: 2,
              overflow: 'hidden',
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
              }}
            >
              <CloseIcon />
            </IconButton>

            {/* Header with user info */}
            <Box
              sx={{
                p: 5,
                background: 'linear-gradient(120deg, primary.main, primary.dark)',
                color: 'white',
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
                    bgcolor: 'primary.light',
                    color: 'white',
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
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                {t('account.profileMessage')}
              </Typography>
              <Button
                variant="outlined"
                onClick={hideOverlay}
                sx={{ minWidth: 120 }}
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