import GoogleIcon from '@mui/icons-material/Google';
import {
  Box,
  Snackbar,
  Alert,
  Typography,
  CircularProgress,
  Button,
} from '@mui/material';
import { useEffect, useCallback, memo } from 'react';
import { useTranslation } from 'react-i18next';

import { useAuth } from '../../../context';
import { authApi } from '../../../services';

const LoginButton = memo(() => {
  const { isLoggedIn, isChecking, error, clearError } = useAuth();
  const { t } = useTranslation();

  // Log error when it appears
  useEffect(() => {
    if (error) {
      console.error('Authentication error:', error);
    }
  }, [error]);

  // Login handler
  const handleLoginWithGoogle = useCallback(() => {
    authApi.initiateGoogleLogin();
  }, []);

  // Display loading indicator while checking auth status
  if (isChecking) {
    return (
      <Box display="flex" alignItems="center" sx={{ minHeight: 40 }}>
        <CircularProgress size={24} sx={{ mr: 1 }} />
        <Typography variant="body2">Logging in...</Typography>
      </Box>
    );
  }

  // If logged in, don't show the login button
  if (isLoggedIn) {
    return null;
  }

  // Display login button and error snackbar if logged out
  return (
    <Box display="flex" alignItems="center">
      {/* Display error in a snackbar if present */}
      <Snackbar
        open={!!error}
        autoHideDuration={6000}
        onClose={clearError}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
      >
        <Alert onClose={clearError} severity="error" sx={{ width: '100%' }}>
          {error}
        </Alert>
      </Snackbar>

      {/* Login button */}
      <Button
        variant="outlined"
        startIcon={<GoogleIcon />}
        onClick={handleLoginWithGoogle}
        sx={{
          mr: 1,
          borderRadius: '999px',
          px: 2,
          py: 0.5,
          minHeight: 32,
          fontWeight: 500,
          fontSize: '0.95rem',
          textTransform: 'none',
          letterSpacing: 0.2,
          borderColor: 'var(--main-color)',
          color: 'var(--main-color)',
          backgroundColor: 'transparent',
          boxShadow: 'none',
          transition: 'background 0.2s, border-color 0.2s, color 0.2s',
          '&:hover': {
            backgroundColor: 'var(--main-color-10)', // subtle tint on hover
            borderColor: 'var(--main-color)',
            color: 'var(--main-color)',
            boxShadow: 'none',
          },
        }}
      >
        {t('login.loginButton')}
      </Button>
    </Box>
  );
});

// Add display name
LoginButton.displayName = 'LoginButton';

export default LoginButton;
