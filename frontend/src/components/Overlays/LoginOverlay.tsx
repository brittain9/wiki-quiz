import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Fade from '@mui/material/Fade';
import IconButton from '@mui/material/IconButton';
import Modal from '@mui/material/Modal';
import Paper from '@mui/material/Paper';
import Typography from '@mui/material/Typography';
import CloseIcon from '@mui/icons-material/Close';
import GoogleIcon from '@mui/icons-material/Google';
import React from 'react';
import { useTranslation } from 'react-i18next';

import { useAuth } from '../../context/AuthProvider';
import { useOverlay } from '../../context/OverlayContext';

interface LoginOverlayProps {
  onSuccess?: () => void;
  message?: string;
}

const LoginOverlay: React.FC = () => {
  const { loginWithGoogle, error, clearError } = useAuth();
  const { currentOverlay, hideOverlay, overlayProps } = useOverlay();
  const { t } = useTranslation();
  
  const { onSuccess, message } = overlayProps as LoginOverlayProps;

  const isOpen = currentOverlay === 'login';

  const handleLoginSuccess = () => {
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
              p: 4,
              borderRadius: 2,
            }}
          >
            <IconButton
              aria-label="close"
              onClick={hideOverlay}
              sx={{
                position: 'absolute',
                right: 12,
                top: 12,
                color: 'text.secondary',
              }}
            >
              <CloseIcon />
            </IconButton>
            
            <Typography component="h2" variant="h5" align="center" gutterBottom>
              {t('login.title')}
            </Typography>
            
            <Typography variant="body1" align="center" sx={{ mb: 3 }}>
              {message || t('login.subtitle')}
            </Typography>

            {error && (
              <Box sx={{ mb: 3, width: '100%' }}>
                <Card sx={{ bgcolor: 'error.light' }}>
                  <CardContent>
                    <Typography color="error.dark">{error}</Typography>
                    <Button size="small" onClick={clearError} sx={{ mt: 1 }}>
                      {t('login.dismiss')}
                    </Button>
                  </CardContent>
                </Card>
              </Box>
            )}

            <Box sx={{ display: 'flex', justifyContent: 'center' }}>
              <Button
                variant="contained"
                color="primary"
                size="large"
                startIcon={<GoogleIcon />}
                onClick={handleLogin}
                sx={{ mt: 2 }}
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