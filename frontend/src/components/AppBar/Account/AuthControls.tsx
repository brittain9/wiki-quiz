import { Box } from '@mui/material';
import { memo } from 'react';

import AccountMenu from './AccountMenu';
import LoginButton from './LoginButton';

const AuthControls = memo(() => {
  return (
    <Box display="flex" alignItems="center">
      <LoginButton />
      <AccountMenu />
    </Box>
  );
});

// Add display name
AuthControls.displayName = 'AuthControls';

export default AuthControls;
