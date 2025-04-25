import CloseIcon from '@mui/icons-material/Close';
import GoogleIcon from '@mui/icons-material/Google';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Fade from '@mui/material/Fade';
import IconButton from '@mui/material/IconButton';
import Modal from '@mui/material/Modal';
import Paper from '@mui/material/Paper';
import Typography from '@mui/material/Typography';
import React from 'react';
import { useTranslation } from 'react-i18next';

import { useAuth, useOverlay } from '../../context';

interface _LoginOverlayProps {
  onSuccess?: () => void;
  message?: string;
}

const LoginOverlay: React.FC = () => {
  const { loginWithGoogle, error, clearError } = useAuth();
  const { currentOverlay, hideOverlay, overlayData } = useOverlay();
  const { t } = useTranslation();

  const message = overlayData?.message || t('login.defaultMessage');
  const onSuccess = overlayData?.onSuccess;

  const isOpen = currentOverlay === 'login';

  const _handleLoginSuccess = () => {
    if (onSuccess) {
      onSuccess();
    }
    hideOverlay();
  };

  const handleLogin = async () => {
    try {
      await loginWithGoogle();
      // Login is handled asynchronously by redirects, so we don't call handleLoginSuccess here
    } catch (error) {
      console.error('Login failed:', error);
    }
  };

  return (
    <Modal
      open={isOpen}
      onClose={hideOverlay}
      closeAfterTransition
      aria-labelledby="login-modal-title"
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
        >
          <Paper
            elevation={0}
            sx={{
              position: 'relative',
              p: 4,
              borderRadius: 2,
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
                color: 'var(--sub-color)',
                '&:hover': {
                  color: 'var(--main-color)',
                  backgroundColor: 'var(--bg-color-secondary)',
                },
              }}
            >
              <CloseIcon />
            </IconButton>

            <Typography
              component="h2"
              variant="h5"
              align="center"
              gutterBottom
              sx={{ color: 'var(--text-color)' }}
            >
              {t('login.title')}
            </Typography>

            <Typography
              variant="body1"
              align="center"
              sx={{ mb: 3, color: 'var(--sub-color)' }}
            >
              {message}
            </Typography>

            {error && (
              <Box sx={{ mb: 3, width: '100%' }}>
                <Card sx={{ bgcolor: 'var(--error-color-light)' }}>
                  <CardContent>
                    <Typography sx={{ color: 'var(--error-color)' }}>
                      {error}
                    </Typography>
                    <Button
                      size="small"
                      onClick={clearError}
                      sx={{
                        mt: 1,
                        color: 'var(--error-color)',
                        '&:hover': {
                          backgroundColor: 'rgba(var(--error-color-rgb), 0.1)',
                        },
                      }}
                    >
                      {t('login.dismiss')}
                    </Button>
                  </CardContent>
                </Card>
              </Box>
            )}

            <Box sx={{ display: 'flex', justifyContent: 'center' }}>
              <Button
                variant="contained"
                size="large"
                startIcon={<GoogleIcon />}
                onClick={handleLogin}
                sx={{
                  mt: 2,
                  backgroundColor: 'var(--main-color)',
                  color: 'var(--bg-color)',
                  '&:hover': {
                    backgroundColor: 'var(--caret-color)',
                  },
                }}
              >
                {t('login.loginWithGoogle')}
              </Button>
            </Box>
          </Paper>
        </Box>
      </Fade>
    </Modal>
  );
};

export default LoginOverlay;
