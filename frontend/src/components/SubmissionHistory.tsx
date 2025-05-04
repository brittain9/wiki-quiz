import {
  Typography,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  Paper,
  CircularProgress,
  Box,
  Chip,
  Button,
} from '@mui/material';
import { format } from 'date-fns';
import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';

import { useAuth } from '../context/AuthContext/AuthContext';
import { useOverlay } from '../context/OverlayContext/OverlayContext';
import { useQuizState } from '../context/QuizStateContext/QuizStateContext';
import useAuthCheck from '../hooks/useAuthCheck';
import { submissionApi } from '../services';
import { SubmissionResponse } from '../types/quizSubmission.types';

const SubmissionHistory: React.FC = React.memo(() => {
  const { submissionHistory: newSubmissions } = useQuizState();
  const [allSubmissions, setAllSubmissions] = useState<SubmissionResponse[]>(
    [],
  );
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { isLoggedIn } = useAuth();
  const { t } = useTranslation();
  const { showOverlay } = useOverlay();

  const { checkAuth } = useAuthCheck({
    message: t('login.viewSubmissionsMessage'),
  });

  // Fetch submissions from the backend when authenticated
  useEffect(() => {
    let isMounted = true;
    const fetchSubmissions = async () => {
      // Only fetch if logged in
      if (!isLoggedIn) {
        setLoading(false);
        return;
      }

      try {
        const recentSubmissions = await submissionApi.getRecentSubmissions();
        if (isMounted) {
          setAllSubmissions(recentSubmissions);
        }
      } catch {
        if (isMounted) {
          setError(
            'Failed to fetch recent submissions. Please try again later.',
          );
        }
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    };

    fetchSubmissions();
    return () => {
      isMounted = false;
    };
  }, [isLoggedIn]);

  // Merge new submissions into the existing list and remove duplicates
  useEffect(() => {
    if (newSubmissions.length > 0) {
      setAllSubmissions((prevSubmissions) => {
        const updatedSubmissions = [...newSubmissions, ...prevSubmissions];
        return Array.from(
          new Map(updatedSubmissions.map((item) => [item.id, item])).values(),
        );
      });
    }
  }, [newSubmissions]);

  // Handle the click on a submission to view details
  const handleSubmissionClick = useCallback(
    async (id: number) => {
      // Check if user is authenticated before showing submission details
      const isAuthenticated = checkAuth();
      if (!isAuthenticated) {
        return;
      }

      try {
        // Instead of managing our own state and modal, just pass the ID to the overlay system
        showOverlay('quiz_result', { resultId: id });
      } catch (error) {
        console.error('Failed to handle submission click:', error);
      }
    },
    [checkAuth, showOverlay],
  );

  // Get chip color based on score
  const getScoreColor = useCallback((score: number) => {
    if (score >= 70) return 'var(--success-color)';
    if (score >= 50) return 'var(--warning-color)';
    return 'var(--error-color)';
  }, []);

  // Memoize the login button to prevent re-renders
  const loginButton = useMemo(
    () => (
      <Button
        variant="contained"
        onClick={() => checkAuth()}
        sx={{
          backgroundColor: 'var(--main-color)',
          color: 'var(--bg-color)',
          '&:hover': {
            backgroundColor: 'var(--caret-color)',
          },
        }}
      >
        {t('login.loginToView')}
      </Button>
    ),
    [checkAuth, t],
  );

  // Render the component
  if (loading) {
    return (
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          minHeight: '200px',
        }}
      >
        <CircularProgress sx={{ color: 'var(--main-color)' }} />
      </Box>
    );
  }

  if (error) {
    return (
      <Typography align="center" sx={{ color: 'var(--error-color)' }}>
        {error}
      </Typography>
    );
  }

  // If not logged in, show message to log in
  if (!isLoggedIn) {
    return (
      <Paper
        elevation={3}
        sx={{
          padding: 4,
          maxWidth: 800,
          margin: 'auto',
          mt: 5,
          mb: 5,
          backgroundColor: 'var(--bg-color)',
          color: 'var(--text-color)',
          border: '1px solid var(--sub-alt-color)',
          borderRadius: 2,
        }}
      >
        <Typography
          variant="h5"
          gutterBottom
          align="center"
          sx={{ color: 'var(--text-color)' }}
        >
          {t('recentSubmissions.title')}
        </Typography>
        <Typography align="center" sx={{ mb: 2, color: 'var(--sub-color)' }}>
          {t('recentSubmissions.loginRequired')}
        </Typography>
        <Box sx={{ display: 'flex', justifyContent: 'center' }}>
          {loginButton}
        </Box>
      </Paper>
    );
  }

  return (
    <>
      <Box
        sx={{
          backgroundColor: 'var(--bg-color)',
          py: 4,
          mt: 3,
        }}
      >
        <Paper
          elevation={3}
          sx={{
            padding: 3,
            maxWidth: 800,
            margin: 'auto',
            backgroundColor: 'var(--bg-color)',
            color: 'var(--text-color)',
            border: '1px solid var(--sub-alt-color)',
            borderRadius: 2,
            position: 'relative',
            overflow: 'hidden',
            '&::before': {
              content: '""',
              position: 'absolute',
              top: 0,
              left: 0,
              right: 0,
              height: '4px',
              backgroundColor: 'var(--main-color)',
              opacity: 0.8,
            },
          }}
        >
          <Typography
            variant="h5"
            gutterBottom
            sx={{
              color: 'var(--main-color)',
              fontWeight: 600,
              mb: 3,
            }}
          >
            {t('recentSubmissions.title')}
          </Typography>

          {allSubmissions.length === 0 ? (
            <Typography sx={{ color: 'var(--sub-color)' }}>
              {t('recentSubmissions.noSubmissions')}
            </Typography>
          ) : (
            <List
              sx={{
                width: '100%',
                maxHeight: 400,
                overflow: 'auto',
                bgcolor: 'var(--bg-color)',
                borderRadius: 1,
                border: '1px solid var(--sub-alt-color)',
              }}
            >
              {allSubmissions.map((submission) => (
                <ListItem
                  key={submission.id}
                  divider
                  disablePadding
                  sx={{
                    borderBottom: '1px solid var(--sub-alt-color)',
                    '&:last-child': {
                      borderBottom: 'none',
                    },
                  }}
                >
                  <ListItemButton
                    onClick={() => handleSubmissionClick(submission.id)}
                    sx={{
                      py: 2,
                      '&:hover': {
                        backgroundColor: 'var(--bg-color-secondary)',
                      },
                    }}
                  >
                    <ListItemText
                      primary={
                        <Box
                          sx={{
                            display: 'flex',
                            justifyContent: 'space-between',
                            alignItems: 'center',
                          }}
                        >
                          <Typography
                            sx={{
                              color: 'var(--text-color)',
                              fontWeight: 500,
                              fontSize: '1rem',
                            }}
                          >
                            {submission.title || 'Quiz'}
                          </Typography>
                          <Chip
                            label={`${submission.score.toFixed(0)}%`}
                            size="small"
                            sx={{
                              backgroundColor: 'var(--bg-color-secondary)',
                              color: getScoreColor(submission.score),
                              border: `1px solid ${getScoreColor(
                                submission.score,
                              )}`,
                              fontWeight: 'bold',
                            }}
                          />
                        </Box>
                      }
                      secondary={
                        <Typography
                          variant="body2"
                          sx={{ color: 'var(--sub-color)' }}
                        >
                          {submission.submissionTime
                            ? format(new Date(submission.submissionTime), 'PPp')
                            : 'No date'}
                        </Typography>
                      }
                    />
                  </ListItemButton>
                </ListItem>
              ))}
            </List>
          )}
        </Paper>
      </Box>
    </>
  );
});

SubmissionHistory.displayName = 'SubmissionHistory';

export default SubmissionHistory;
