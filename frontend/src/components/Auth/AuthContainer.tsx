import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import GoogleIcon from '@mui/icons-material/Google';
import LogoutIcon from '@mui/icons-material/Logout';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Paper,
  Typography,
} from '@mui/material';
import React, { useEffect, useState } from 'react';

import { useAuthStatus, useAuthActions } from '../../hooks';
import { logAuth } from '../../utils/logger';

/**
 * Format a date string in a user-friendly format
 */
const formatDate = (dateString: string): string => {
  try {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(date);
  } catch (error) {
    console.error('Error formatting date:', error);
    return dateString; // Return original string if formatting fails
  }
};

/**
 * Container component that handles authentication state and actions
 */
const AuthContainer: React.FC = () => {
  const { isAuthenticated, isLoading, user } = useAuthStatus();
  const { login, logout, clearError, refreshToken, error } = useAuthActions();
  const [lastRefresh, setLastRefresh] = useState<string | null>(null);

  // Log component rendering
  useEffect(() => {
    logAuth('AuthContainer rendered', {
      isAuthenticated,
      isLoading,
      hasUser: !!user,
      hasError: !!error,
    });
  }, [isAuthenticated, isLoading, user, error]);

  // Handle login button click
  const handleLogin = () => {
    logAuth('Login button clicked');
    login();
  };

  // Handle logout button click
  const handleLogout = async () => {
    logAuth('Logout button clicked');
    await logout();
  };

  // Handle token refresh
  const handleRefreshToken = async () => {
    logAuth('Refresh token button clicked');
    const success = await refreshToken();
    if (success) {
      setLastRefresh(new Date().toISOString());
    }
  };

  if (isLoading) {
    return (
      <Paper elevation={3} sx={{ p: 3, maxWidth: 500, mx: 'auto', mt: 4 }}>
        <Box display="flex" flexDirection="column" alignItems="center" gap={2}>
          <CircularProgress />
          <Typography>Verifying authentication status...</Typography>
        </Box>
      </Paper>
    );
  }

  if (isAuthenticated && user) {
    return (
      <Paper elevation={3} sx={{ p: 3, maxWidth: 500, mx: 'auto', mt: 4 }}>
        <Box display="flex" flexDirection="column" gap={2}>
          <Box display="flex" alignItems="center" gap={1}>
            <AccountCircleIcon color="primary" fontSize="large" />
            <Typography variant="h5">Welcome, {user.firstName}!</Typography>
          </Box>

          <Box my={2}>
            <Typography variant="subtitle1">User Information</Typography>
            <Typography variant="body2">ID: {user.id}</Typography>
            <Typography variant="body2">Email: {user.email}</Typography>
            <Typography variant="body2">
              Name: {user.firstName} {user.lastName}
            </Typography>
          </Box>

          {lastRefresh && (
            <Alert severity="success" sx={{ mb: 2 }}>
              Token refreshed at {formatDate(lastRefresh)}
            </Alert>
          )}

          <Box display="flex" gap={2}>
            <Button
              variant="contained"
              color="secondary"
              startIcon={<LogoutIcon />}
              onClick={handleLogout}
            >
              Logout
            </Button>
            <Button variant="outlined" onClick={handleRefreshToken}>
              Refresh Token
            </Button>
          </Box>
        </Box>
      </Paper>
    );
  }

  return (
    <Paper elevation={3} sx={{ p: 3, maxWidth: 500, mx: 'auto', mt: 4 }}>
      <Box display="flex" flexDirection="column" gap={3} alignItems="center">
        <Typography variant="h5">Authentication Demo</Typography>

        <Button
          variant="contained"
          color="primary"
          startIcon={<GoogleIcon />}
          onClick={handleLogin}
          fullWidth
        >
          Sign in with Google
        </Button>

        {error && (
          <Alert severity="error" onClose={clearError} sx={{ width: '100%' }}>
            {error}
          </Alert>
        )}

        <Typography variant="body2" color="textSecondary" align="center">
          This demonstration uses Google OAuth for authentication. Your account
          information is handled securely.
        </Typography>
      </Box>
    </Paper>
  );
};

export default AuthContainer;
