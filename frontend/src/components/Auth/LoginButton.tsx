import GoogleIcon from '@mui/icons-material/Google';
import { Button, Box, Typography, Snackbar, Alert } from '@mui/material';
import React, { useState, useEffect } from 'react';

import { useAuth } from '../../context/AuthContext';

const API_BASE_URL = 'http://localhost:5543';

const LoginButton: React.FC = () => {
  const { isAuthenticated, logout, user } = useAuth();
  const [error, setError] = useState<string | null>(null);

  // Check for error params in URL when component mounts
  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const errorParam = params.get('error');

    if (errorParam) {
      setError(errorParam.replace(/_/g, ' '));

      // Clean up the URL
      const newUrl = window.location.pathname;
      window.history.replaceState({}, document.title, newUrl);
    }
  }, []);

  const handleGoogleLogin = () => {
    // Open the auth endpoint in a popup window
    const width = 500;
    const height = 600;
    const left = window.screenX + (window.outerWidth - width) / 2;
    const top = window.screenY + (window.outerHeight - height) / 2;

    try {
      const popup = window.open(
        `${API_BASE_URL}/api/auth/google-login`,
        'googleLoginPopup',
        `width=${width},height=${height},left=${left},top=${top},resizable=yes,scrollbars=yes,status=yes`,
      );

      if (!popup) {
        setError('Popup blocked. Please allow popups for this site.');
        return;
      }

      // Poll to check if popup is closed
      const checkPopup = setInterval(() => {
        try {
          if (!popup || popup.closed) {
            clearInterval(checkPopup);
            console.log('Auth popup closed, refreshing user data');

            // Check for any URL parameters in the popup before refreshing
            try {
              // This might throw a cross-origin error if the popup redirected to a different domain
              const popupUrl = popup?.location?.href;
              console.log('Popup final URL:', popupUrl);
            } catch (e) {
              console.log(
                'Could not access popup URL due to cross-origin restrictions',
              );
            }

            // Wait a moment before refreshing to allow backend session to be fully established
            setTimeout(() => {
              console.log('Refreshing page to update auth state');
              window.location.reload();
            }, 500);
          }
        } catch (e) {
          clearInterval(checkPopup);
          console.error('Error monitoring popup:', e);
        }
      }, 1000);
    } catch (e) {
      console.error('Error opening popup:', e);
      setError('Failed to open login popup');
    }
  };

  const handleCloseError = () => {
    setError(null);
  };

  if (isAuthenticated && user) {
    return (
      <Box display="flex" alignItems="center">
        {user.picture && (
          <Box
            component="img"
            src={user.picture}
            alt={user.name}
            sx={{
              width: 32,
              height: 32,
              borderRadius: '50%',
              marginRight: 1,
            }}
          />
        )}
        <Typography variant="body2" sx={{ marginRight: 2 }}>
          {user.name}
        </Typography>
        <Button
          variant="outlined"
          color="inherit"
          size="small"
          onClick={logout}
        >
          Logout
        </Button>
      </Box>
    );
  }

  return (
    <>
      <Button
        variant="outlined"
        color="primary"
        size="small"
        startIcon={<GoogleIcon />}
        onClick={handleGoogleLogin}
      >
        Sign in
      </Button>

      <Snackbar
        open={!!error}
        autoHideDuration={6000}
        onClose={handleCloseError}
      >
        <Alert
          onClose={handleCloseError}
          severity="error"
          sx={{ width: '100%' }}
        >
          Authentication error: {error}
        </Alert>
      </Snackbar>
    </>
  );
};

export default LoginButton;
