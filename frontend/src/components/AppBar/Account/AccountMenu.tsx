import LogoutIcon from '@mui/icons-material/Logout';
import PersonIcon from '@mui/icons-material/Person';
import {
  Box,
  Tooltip,
  Typography,
  Menu,
  MenuItem,
  Divider,
  IconButton,
  Avatar,
} from '@mui/material';
import React, { useState, useCallback, memo } from 'react';
import { useTranslation } from 'react-i18next';

import { useAuth } from '../../../context/AuthContext/AuthContext';
import { useOverlay } from '../../../context/OverlayContext/OverlayContext';

const AccountMenu = memo(() => {
  const { isLoggedIn, userInfo, logout } = useAuth();
  const { showOverlay } = useOverlay();
  const { t } = useTranslation();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

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

  // Don't render anything if not logged in
  if (!isLoggedIn) {
    return null;
  }

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
          <Avatar
            sx={{
              width: 32,
              height: 32,
              bgcolor: 'var(--main-color-light)',
              color: 'var(--bg-color)',
              fontWeight: 600,
              fontSize: '1.1rem',
            }}
          >
            {userInfo?.firstName && userInfo?.lastName
              ? `${userInfo.firstName.charAt(0)}${userInfo.lastName.charAt(0)}`.toUpperCase()
              : userInfo?.firstName
                ? userInfo.firstName.charAt(0).toUpperCase()
                : 'U'}
          </Avatar>
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
});

// Add display name
AccountMenu.displayName = 'AccountMenu';

export default AccountMenu;
