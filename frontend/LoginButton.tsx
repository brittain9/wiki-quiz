// frontend/src/components/LoginButton.tsx (Assuming path)
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
import React from 'react';

import { useAuth } from '../../context/AuthProvider';

const LoginButton: React.FC = () => {
  const {
    isLoggedIn,
    isChecking,
    userInfo,
    error,
    loginWithGoogle,
    logout,
    clearError,
  } = useAuth();

  // Display loading indicator while checking auth status
  if (isChecking) {
    return (
      <Box display="flex" alignItems="center" sx={{ minHeight: 40 }}>
        {' '}
        <CircularProgress size={24} sx={{ mr: 1 }} />{' '}
        <Typography variant="body2">Checking status...</Typography>
      </Box>
    );
  }

  // Display user info and logout button if logged in
  if (isLoggedIn) {
    return (
      <Box display="flex" alignItems="center" sx={{ minHeight: 40 }}>
        {' '}
        <Tooltip title={`Logged in as ${userInfo?.email || 'User'}`}>
          <AccountCircleIcon color="primary" sx={{ mr: 1 }} />
        </Tooltip>
        <Typography
          variant="body2"
          sx={{ mr: 2, display: { xs: 'none', sm: 'block' } }}
        >
          {' '}
          Hi, {userInfo?.firstName || 'User'}
        </Typography>
        <Button
          variant="outlined"
          color="secondary"
          size="small"
          onClick={logout}
        >
          Log out
        </Button>
      </Box>
    );
  }

  // Display login button and error snackbar if logged out
  return (
    <Box sx={{ minHeight: 40 }}>
      <Button
        variant="contained"
        color="primary"
        size="small"
        startIcon={<GoogleIcon />}
        onClick={loginWithGoogle}
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
