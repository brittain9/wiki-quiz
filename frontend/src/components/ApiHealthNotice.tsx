import React from 'react';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import CircularProgress from '@mui/material/CircularProgress';
import { waitForApiReady } from '../services/apiService';

const ApiHealthNotice: React.FC = () => {
  const [open, setOpen] = React.useState(false);
  const [checking, setChecking] = React.useState(false);
  const intervalRef = React.useRef<number | null>(null);

  const clearPoll = () => {
    if (intervalRef.current !== null) {
      window.clearInterval(intervalRef.current);
      intervalRef.current = null;
    }
  };

  const checkOnce = React.useCallback(async () => {
    if (checking) return;
    setChecking(true);
    try {
      await waitForApiReady();
      setOpen(false);
      clearPoll();
    } catch {
      setOpen(true);
    } finally {
      setChecking(false);
    }
  }, [checking]);

  React.useEffect(() => {
    let isMounted = true;
    (async () => {
      try {
        await waitForApiReady();
        if (!isMounted) return;
        setOpen(false);
      } catch {
        if (!isMounted) return;
        setOpen(true);
        // Start polling every 5s until backend is ready
        if (intervalRef.current === null) {
          intervalRef.current = window.setInterval(() => {
            checkOnce();
          }, 5000);
        }
      }
    })();
    return () => {
      isMounted = false;
      clearPoll();
    };
  }, [checkOnce]);

  return (
    <Snackbar
      open={open}
      anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
    >
      <Alert
        severity="info"
        variant="filled"
        sx={{ width: '100%' }}
        action={
          <Button
            color="inherit"
            size="small"
            onClick={checkOnce}
            disabled={checking}
          >
            {checking ? 'Checking…' : 'Retry now'}
          </Button>
        }
      >
        <Stack direction="row" spacing={1} alignItems="center">
          {checking && <CircularProgress color="inherit" size={16} />}
          <span>The server is starting up. Please wait a moment…</span>
        </Stack>
      </Alert>
    </Snackbar>
  );
};

export default ApiHealthNotice;


