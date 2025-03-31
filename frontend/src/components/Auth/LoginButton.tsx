import GoogleIcon from '@mui/icons-material/Google';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import { Button, Box, Snackbar, Alert, Tooltip, Typography, CircularProgress } from '@mui/material';
import React, { useState, useEffect } from 'react';

// Define the user info interface
interface UserInfo {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

const LoginButton: React.FC = () => {
  const [error, setError] = useState<string | null>(null);
  const [isLoggedIn, setIsLoggedIn] = useState<boolean>(false);
  const [isChecking, setIsChecking] = useState<boolean>(true);
  const [userInfo, setUserInfo] = useState<UserInfo | null>(null);

  // Function to check login status
  const checkLoginStatus = async () => {
    setIsChecking(true);
    try {
      // First try to get user info from the /me endpoint
      try {
        console.log('Attempting to fetch user info from /me endpoint');
        const response = await fetch('http://localhost:5543/api/auth/me', {
          credentials: 'include',
          headers: {
            'Cache-Control': 'no-cache',
            'Pragma': 'no-cache'
          }
        });
        
        if (response.ok) {
          const userData = await response.json();
          console.log('User info retrieved:', userData);
          setUserInfo(userData);
          setIsLoggedIn(true);
          setIsChecking(false);
          return;
        } else {
          console.log('Failed to fetch user info:', response.status);
        }
      } catch (err) {
        console.log('Error fetching user info:', err);
      }
      
      // If user info fetch fails, fall back to checking auth status
      const endpoints = [
        'http://localhost:5543/api/auth/protected',
        'http://localhost:5543/api/movies'
      ];
      
      let authenticated = false;
      for (const endpoint of endpoints) {
        try {
          console.log(`Trying to check auth at endpoint: ${endpoint}`);
          const response = await fetch(endpoint, {
            credentials: 'include',
            headers: {
              'Cache-Control': 'no-cache',
              'Pragma': 'no-cache'
            }
          });
          
          if (response.ok) {
            authenticated = true;
            console.log(`Authentication check successful via ${endpoint}`);
            break;
          } else {
            console.log(`Auth check failed for ${endpoint} with status: ${response.status}`);
          }
        } catch (err) {
          console.log(`Endpoint ${endpoint} failed:`, err);
        }
      }
      
      console.log(`Setting isLoggedIn to: ${authenticated}`);
      setIsLoggedIn(authenticated);
    } catch (error) {
      console.error('Error checking login status:', error);
      setIsLoggedIn(false);
    } finally {
      setIsChecking(false);
    }
  };

  // Check for login status and error params
  useEffect(() => {
    // Check for error params from redirect
    const params = new URLSearchParams(window.location.search);
    
    // Check for error in query params
    const errorParam = params.get('error');
    if (errorParam) {
      handleErrorParam(errorParam);
    }

    // Check for successful login redirect
    const fromAuth = params.get('fromAuth');
    if (fromAuth === 'true') {
      console.log('Detected fromAuth parameter, waiting before checking login status');
      
      // Clean up the URL first
      const cleanUrl = window.location.pathname + window.location.hash;
      window.history.replaceState({}, document.title, cleanUrl);
      
      // Add a small delay to allow cookies to be properly set before checking
      setTimeout(() => {
        console.log('Checking login status after redirect delay');
        checkLoginStatus();
      }, 1000);
    } else {
      // Regular login check on component mount
      checkLoginStatus();
    }
  }, []);
  
  // Helper function to handle error parameter
  const handleErrorParam = (errorParam: string) => {
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
  };

  const handleLoginWithGoogle = () => {
    setError(null); // Clear previous errors
    try {
      // Add a timestamp to prevent caching issues and fromAuth parameter to trigger recheck
      const timestamp = new Date().getTime();
      window.location.href = `http://localhost:5543/api/auth/login/google?returnUrl=http://localhost:5173%3FfromAuth=true&t=${timestamp}`;
    } catch (e) {
      console.error('Error initiating navigation:', e);
      setError('Failed to start the login process.');
    }
  };

  const handleLogout = async () => {
    try {
      // Try the logout endpoint if it exists
      try {
        await fetch('http://localhost:5543/api/auth/logout', {
          method: 'POST',
          credentials: 'include',
        });
      } catch (e) {
        console.log('Logout endpoint might not exist, clearing cookies manually');
      }
      
      // Also clear cookies manually as a backup
      document.cookie = "REFRESH_TOKEN=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
      document.cookie = "ACCESS_TOKEN=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
      
      setUserInfo(null);
      setIsLoggedIn(false);
    } catch (error) {
      console.error('Error logging out:', error);
      setError('Failed to log out. Please try again.');
    }
  };

  const handleCloseError = () => {
    setError(null);
  };

  // Show checking status
  if (isChecking) {
    return (
      <Box display="flex" alignItems="center">
        <CircularProgress size={20} sx={{ mr: 1 }} />
        <Typography variant="body2">Checking...</Typography>
      </Box>
    );
  }

  // Show logged in state
  if (isLoggedIn) {
    return (
      <Box display="flex" alignItems="center">
        <Tooltip title="You are logged in">
          <AccountCircleIcon color="primary" sx={{ mr: 1 }} />
        </Tooltip>
        <Typography variant="body2" sx={{ mr: 2 }}>
          {userInfo ? `Welcome, ${userInfo.firstName || 'User'}` : 'Logged in successfully'}
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
        autoHideDuration={8000} 
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