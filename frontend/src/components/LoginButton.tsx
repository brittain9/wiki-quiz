// src/components/Auth/LoginButton.tsx
import GoogleIcon from '@mui/icons-material/Google';
import LogoutIcon from '@mui/icons-material/Logout';
import PersonIcon from '@mui/icons-material/Person';
import {
  Box,
  Snackbar,
  Alert,
  Tooltip,
  Typography,
  CircularProgress,
  Menu,
  MenuItem,
  Divider,
  IconButton,
  Avatar,
  Button,
} from '@mui/material';
import React, { useEffect, useState, useCallback, memo } from 'react';
import { useTranslation } from 'react-i18next';

import { useAuth } from '../context/AuthContext/AuthContext';
import { useOverlay } from '../context/OverlayContext/OverlayContext';

const LoginButton = memo(() => {
  const {
    isLoggedIn,
    isChecking,
    userInfo,
    loginWithGoogle,
    logout,
    error,
    clearError,
  } = useAuth();
  const { showOverlay } = useOverlay();
  const { t } = useTranslation();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  // Log error when it appears
  useEffect(() => {
    if (error) {
      console.error('Authentication error:', error);
    }
  }, [error]);

  // Login handler
  const handleLoginWithGoogle = useCallback(() => {
    loginWithGoogle();
  }, [loginWithGoogle]);

  // Show account overlay
  const handleAccountClick = useCallback(() => {
    setAnchorEl(null); // Close menu
    showOverlay('account');
  }, [showOverlay]);

  // Handle logout
  const handleLogout = useCallback(async () => {
    setAnchorEl(null); // Close menu
    try {
      await logout();
    } catch (err) {
      console.error('Logout failed:', err);
    }
  }, [logout]);

  // Menu handling
  const handleMenuOpen = useCallback((event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  }, []);

  const handleMenuClose = useCallback(() => {
    setAnchorEl(null);
  }, []);

  // Display loading indicator while checking auth status
  if (isChecking) {
    return (
      <Box display="flex" alignItems="center" sx={{ minHeight: 40 }}>
        <CircularProgress size={24} sx={{ mr: 1 }} />
        <Typography variant="body2">Checking status...</Typography>
      </Box>
    );
  }

  // Display user avatar button if logged in
  if (isLoggedIn) {
    return (
      <Box display="flex" alignItems="center" sx={{ minHeight: 40 }}>
        <Tooltip title={t('account.title')}>
          <IconButton
            onClick={handleMenuOpen}
            size="small"
            aria-controls={open ? 'account-menu' : undefined}
            aria-haspopup="true"
            aria-expanded={open ? 'true' : undefined}
            sx={{ ml: 1 }}
          >
            {userInfo?.profilePicture ? (
              <Avatar
                src={userInfo.profilePicture}
                alt={userInfo.firstName}
                sx={{
                  width: 32,
                  height: 32,
                }}
              />
            ) : (
              <Avatar
                sx={{
                  width: 32,
                  height: 32,
                  bgcolor: 'primary.main',
                }}
              >
                {userInfo?.firstName?.charAt(0) || 'U'}
              </Avatar>
            )}
          </IconButton>
        </Tooltip>
        <Menu
          id="account-menu"
          anchorEl={anchorEl}
          open={open}
          onClose={handleMenuClose}
          onClick={handleMenuClose}
          PaperProps={{
            elevation: 0,
            sx: {
              overflow: 'visible',
              filter: 'drop-shadow(0px 2px 8px rgba(0,0,0,0.15))',
              mt: 1.5,
              minWidth: 200,
              '& .MuiAvatar-root': {
                width: 32,
                height: 32,
                ml: -0.5,
                mr: 1,
              },
            },
          }}
          transformOrigin={{ horizontal: 'right', vertical: 'top' }}
          anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
        >
          <Box sx={{ px: 2, py: 1 }}>
            <Typography variant="subtitle1">
              {userInfo?.firstName} {userInfo?.lastName || ''}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {userInfo?.email}
            </Typography>
          </Box>
          <Divider />
          <MenuItem onClick={handleAccountClick}>
            <PersonIcon fontSize="small" sx={{ mr: 1 }} />
            {t('account.viewProfile')}
          </MenuItem>
          <Divider />
          <MenuItem onClick={handleLogout}>
            <LogoutIcon fontSize="small" sx={{ mr: 1 }} />
            {t('account.logout')}
          </MenuItem>
        </Menu>
      </Box>
    );
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
        variant="contained"
        startIcon={<GoogleIcon />}
        onClick={handleLoginWithGoogle}
        sx={{
          mr: 1,
          backgroundColor: 'var(--main-color)',
          color: 'var(--bg-color)',
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
