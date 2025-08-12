import CloseIcon from '@mui/icons-material/Close';
import EmojiEventsIcon from '@mui/icons-material/EmojiEvents';
import StarIcon from '@mui/icons-material/Star';
import Avatar from '@mui/material/Avatar';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import Fade from '@mui/material/Fade';
import IconButton from '@mui/material/IconButton';
import LinearProgress from '@mui/material/LinearProgress';
import Modal from '@mui/material/Modal';
import Paper from '@mui/material/Paper';
import Typography from '@mui/material/Typography';
import React from 'react';
import { useTranslation } from 'react-i18next';

import { useAuth } from '../../context/AuthContext/AuthContext';
import { useOverlay } from '../../context/OverlayContext/OverlayContext';
import { aiApi } from '../../services';

const AccountOverlay: React.FC = () => {
  const { userInfo } = useAuth();
  const { currentOverlay, hideOverlay } = useOverlay();
  const { t } = useTranslation();
  const isOpen = currentOverlay === 'account';

  const [userCost, setUserCost] = React.useState<number | null>(null);
  const [loadingCost, setLoadingCost] = React.useState(false);
  const [costError, setCostError] = React.useState<string | null>(null);
  const COST_LIMIT = 0.25;

  React.useEffect(() => {
    if (isOpen) {
      setLoadingCost(true);
      setCostError(null);
      aiApi
        .getUserCost()
        .then((cost) => {
          setUserCost(cost);
          setLoadingCost(false);
        })
        .catch((err) => {
          setCostError(err.message || 'Error fetching cost');
          setLoadingCost(false);
        });
    }
  }, [isOpen]);

  // Calculate the avatar initials from first and last name
  const getInitials = () => {
    if (!userInfo) return '?';

    const firstName = userInfo.firstName || '';
    const lastName = userInfo.lastName || '';

    return (
      `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase() ||
      firstName.charAt(0).toUpperCase() ||
      '?'
    );
  };

  // Format cost display
  const getFormattedCost = () => {
    if (userCost === null) return '0.00';
    return typeof userCost === 'number' && !isNaN(userCost)
      ? userCost.toFixed(2)
      : '0.00';
  };

  // Calculate progress percentage
  const getProgressPercentage = () => {
    if (userCost === null) return 0;
    return typeof userCost === 'number' && !isNaN(userCost)
      ? Math.min((userCost / COST_LIMIT) * 100, 100)
      : 0;
  };

  // Determine if cost exceeds limit
  const isCostExceeded = () => {
    return (
      typeof userCost === 'number' && !isNaN(userCost) && userCost >= COST_LIMIT
    );
  };

  if (!userInfo) {
    return null;
  }

  return (
    <Modal
      open={isOpen}
      onClose={hideOverlay}
      closeAfterTransition
      aria-labelledby="account-modal-title"
    >
      <Fade in={isOpen}>
        <Box
          sx={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            width: { xs: '90%', sm: 500 },
            maxWidth: 500,
            bgcolor: 'var(--bg-color)',
            borderRadius: 2,
            boxShadow: 24,
            p: 0,
            outline: 'none',
          }}
        >
          <Paper
            elevation={0}
            sx={{
              position: 'relative',
              p: 0,
              borderRadius: 2,
              overflow: 'hidden',
              backgroundColor: 'var(--bg-color)',
              color: 'var(--text-color)',
            }}
          >
            <IconButton
              aria-label="close"
              onClick={hideOverlay}
              sx={{
                position: 'absolute',
                right: 12,
                top: 12,
                color: 'white',
                bgcolor: 'rgba(0,0,0,0.2)',
                '&:hover': {
                  bgcolor: 'rgba(0,0,0,0.4)',
                },
                zIndex: 10,
              }}
            >
              <CloseIcon />
            </IconButton>

            {/* Header with user info */}
            <Box
              sx={{
                p: 5,
                background:
                  'linear-gradient(120deg, var(--main-color), var(--caret-color))',
                color: 'var(--bg-color)',
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
              }}
            >
              <Avatar
                sx={{
                  width: 120,
                  height: 120,
                  mb: 3,
                  fontSize: '3rem',
                  bgcolor: 'var(--main-color-light)',
                  color: 'var(--bg-color)',
                  boxShadow: 2,
                }}
              >
                {getInitials()}
              </Avatar>
              <Typography variant="h4" component="h2" gutterBottom>
                {userInfo.firstName} {userInfo.lastName}
              </Typography>
              <Typography variant="body1" sx={{ opacity: 0.8 }}>
                {userInfo.email}
              </Typography>

              {/* Points and Level Display */}
              <Box sx={{ display: 'flex', gap: 2, mt: 2, mb: 1 }}>
                <Chip
                  icon={<StarIcon />}
                  label={`Level ${userInfo.level}`}
                  sx={{
                    bgcolor: 'rgba(255, 255, 255, 0.2)',
                    color: 'inherit',
                    fontWeight: 'bold',
                  }}
                />
                <Chip
                  icon={<EmojiEventsIcon />}
                  label={`${(userInfo.totalPoints || 0).toLocaleString()} pts`}
                  sx={{
                    bgcolor: 'rgba(255, 255, 255, 0.2)',
                    color: 'inherit',
                    fontWeight: 'bold',
                  }}
                />
              </Box>

              {/* Cost usage progress bar */}
              <Box sx={{ width: '100%', mt: 3 }}>
                {loadingCost ? (
                  <LinearProgress color="error" />
                ) : costError ? (
                  <Typography color="error" variant="body2">
                    {costError}
                  </Typography>
                ) : userCost !== null ? (
                  <>
                    <Box
                      sx={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        mb: 0.5,
                      }}
                    >
                      <Typography variant="body2" color="inherit">
                        {t('account.usage') || 'Usage'}
                      </Typography>
                      <Typography
                        variant="body2"
                        color={isCostExceeded() ? 'error' : 'inherit'}
                      >
                        ${getFormattedCost()} / ${COST_LIMIT.toFixed(2)}
                      </Typography>
                    </Box>
                    <LinearProgress
                      variant="determinate"
                      value={getProgressPercentage()}
                      color="error"
                      sx={{
                        height: 10,
                        borderRadius: 5,
                        backgroundColor: 'rgba(255,0,0,0.1)',
                      }}
                    />
                  </>
                ) : null}
              </Box>
            </Box>

            {/* Footer */}
            <Box sx={{ p: 3, textAlign: 'center' }}>
              <Typography
                variant="body2"
                sx={{ mb: 2, color: 'var(--sub-color)' }}
              >
                {t('account.profileMessage')}
              </Typography>
              <Button
                variant="outlined"
                onClick={hideOverlay}
                sx={{
                  minWidth: 120,
                  borderColor: 'var(--main-color)',
                  color: 'var(--main-color)',
                  '&:hover': {
                    borderColor: 'var(--caret-color)',
                    backgroundColor: 'rgba(var(--main-color-rgb), 0.1)',
                  },
                }}
              >
                {t('button.close')}
              </Button>
            </Box>
          </Paper>
        </Box>
      </Fade>
    </Modal>
  );
};

export default AccountOverlay;
