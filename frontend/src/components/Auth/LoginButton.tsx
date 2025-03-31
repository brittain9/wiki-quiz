// frontend/src/components/Auth/LoginButton.tsx
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import GoogleIcon from '@mui/icons-material/Google';
import {
  Button,
  Box,
  Snackbar,
  Alert,
  Tooltip,
  Typography,
  CircularProgress,
} from '@mui/material';
import React, { useEffect } from 'react';

import useAuthActions from '../../hooks/useAuthActions';
import useAuthStatus from '../../hooks/useAuthStatus';
import { createLogger } from '../../utils/logger';

// Create a specialized logger for LoginButton
const logLoginButton = createLogger('LoginButton', '🔘', true);

const LoginButton: React.FC = () => {
  // Use specialized hooks instead of the full context
  const { isAuthenticated, isLoading, user } = useAuthStatus();
  const { login, logout, clearError, error } = useAuthActions();

  // Log component state changes
  useEffect(() => {
    logLoginButton('Auth state changed', {
      isAuthenticated,
      isLoading,
      user: user
        ? {
            id: user.id,
            email: user.email,
            firstName: user.firstName,
          }
        : null,
      hasError: !!error,
    });
  }, [isAuthenticated, isLoading, user, error]);

  // Log error when it appears
  useEffect(() => {
    if (error) {
      logLoginButton('Authentication error', { error });
    }
  }, [error]);

  // Enhanced login handler
  const handleLoginWithGoogle = () => {
    logLoginButton('Google login button clicked');
    login();
  };

  // Enhanced logout handler
  const handleLogout = async () => {
    logLoginButton('Logout button clicked');

    try {
      await logout();
      logLoginButton('Logout completed');
    } catch (err) {
      logLoginButton('Logout failed', { error: err });
    }
  };

  // Display loading indicator while checking auth status
  if (isLoading) {
    logLoginButton('Rendering loading state');
    return (
      <Box display="flex" alignItems="center" sx={{ minHeight: 40 }}>
        <CircularProgress size={24} sx={{ mr: 1 }} />
        <Typography variant="body2">Checking status...</Typography>
      </Box>
    );
  }

  // Display user info and logout button if logged in
  if (isAuthenticated) {
    logLoginButton('Rendering logged-in state', {
      email: user?.email,
      firstName: user?.firstName,
    });

    return (
      <Box display="flex" alignItems="center" sx={{ minHeight: 40 }}>
        <Tooltip title={`Logged in as ${user?.email || 'User'}`}>
          <AccountCircleIcon color="primary" sx={{ mr: 1 }} />
        </Tooltip>
        <Typography
          variant="body2"
          sx={{ mr: 2, display: { xs: 'none', sm: 'block' } }}
        >
          Hi, {user?.firstName || 'User'}
        </Typography>
        <Button
          variant="outlined"
          color="secondary"
          size="small"
          onClick={handleLogout}
        >
          Log out
        </Button>
      </Box>
    );
  }

  // Display login button and error snackbar if logged out
  logLoginButton('Rendering logged-out state');

  return (
    <Box sx={{ minHeight: 40 }}>
      <Button
        variant="contained"
        color="primary"
        size="small"
        startIcon={<GoogleIcon />}
        onClick={handleLoginWithGoogle}
      >
        Sign in with Google
      </Button>
      <Snackbar
        open={!!error}
        autoHideDuration={6000}
        onClose={clearError}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert
          onClose={clearError}
          severity="error"
          sx={{ width: '100%' }}
          variant="filled"
        >
          {error}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default LoginButton;
