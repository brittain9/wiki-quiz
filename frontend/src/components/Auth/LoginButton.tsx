import GoogleIcon from '@mui/icons-material/Google';
import { Button, Box, Snackbar, Alert } from '@mui/material';
import React, { useState, useEffect } from 'react';

const LoginButton: React.FC = () => {
  const [error, setError] = useState<string | null>(null);

  // Check for error params passed back from backend redirect
  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const errorParam = params.get('error');

    if (errorParam) {
      let errorMessage = errorParam.replace(/_/g, ' ');
      // Make specific error messages more user-friendly if needed
      if (errorMessage === 'email exists different provider') {
           errorMessage = 'An account with this email already exists, possibly created directly or via a different method. Please log in using that method.';
      } else if (errorMessage === 'google auth failed') {
           errorMessage = 'Authentication with Google failed. Please try again.';
      } else if (errorMessage === 'google claims missing') {
           errorMessage = 'Could not retrieve necessary information from Google. Please try again.';
      }
      setError(errorMessage);

      // Clean up the URL (remove the ?error=... part)
      const cleanUrl = window.location.pathname + window.location.hash; // Keep hash if any
      window.history.replaceState({}, document.title, cleanUrl);
    }
  }, []);

  const handleLoginWithGoogle = () => {
    setError(null); // Clear previous errors
    try {
        window.location.href = 'http://localhost:5543/api/auth/login/google?returnUrl=http://localhost:5173';
    } catch (e) {
        console.error('Error initiating navigation:', e);
        setError('Failed to start the login process.');
    }
  };

  const handleCloseError = () => {
    setError(null);
  };

  // Display login button
  return (
    <Box>
      <Button
        variant="outlined"
        color="primary"
        size="small"
        startIcon={<GoogleIcon />}
        onClick={handleLoginWithGoogle}
      >
        Sign in
      </Button>

      <Snackbar
        open={!!error}
        autoHideDuration={8000} // Allow more time for longer messages
        onClose={handleCloseError}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert
          onClose={handleCloseError}
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