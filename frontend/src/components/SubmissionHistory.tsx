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
  Modal,
  Button,
} from '@mui/material';
import { format } from 'date-fns';
import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';

import QuizResultOverlay from './QuizResultOverlay';
import { useAuth } from '../context/AuthContext';
import { useCustomTheme } from '../context/CustomThemeContext/CustomThemeContext';
import { useQuizState } from '../context/QuizStateContext/QuizStateContext';
import useAuthCheck from '../hooks/useAuthCheck';
import { submissionApi } from '../services';
import { QuizResult } from '../types/quizResult.types';
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
  const { currentTheme } = useCustomTheme();

  const [isOverlayOpen, setIsOverlayOpen] = useState(false);
  const [selectedQuizResult, setSelectedQuizResult] =
    useState<QuizResult | null>(null);
  const [isResultLoading, setIsResultLoading] = useState(false);
  const [resultError, setResultError] = useState<string | null>(null);

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
      } catch (_error) {
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

      setIsResultLoading(true);
      setResultError(null);
      try {
        const result = await submissionApi.getSubmissionById(id);
        setSelectedQuizResult(result);
        setIsOverlayOpen(true);
      } catch (err) {
        console.error('Failed to fetch quiz result:', err);
        setResultError('Failed to load quiz result. Please try again.');
      } finally {
        setIsResultLoading(false);
      }
    },
    [checkAuth],
  );

  // Close the overlay
  const closeOverlay = useCallback(() => {
    setIsOverlayOpen(false);
    setSelectedQuizResult(null);
  }, []);

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
        className={`theme-${currentTheme}`}
      >
        <CircularProgress sx={{ color: 'var(--main-color)' }} />
      </Box>
    );
  }

  if (error) {
    return (
      <Typography
        align="center"
        sx={{ color: 'var(--error-color)' }}
        className={`theme-${currentTheme}`}
      >
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
        className={`theme-${currentTheme}`}
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
        className={`theme-${currentTheme}`}
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
          className={`theme-${currentTheme}`}
        >
          <Typography
            variant="h5"
            gutterBottom
            align="center"
            sx={{
              color: 'var(--main-color)',
              fontWeight: 600,
              mb: 3,
            }}
          >
            {t('recentSubmissions.title')}
          </Typography>
          {allSubmissions.length === 0 ? (
            <Typography
              align="center"
              sx={{ color: 'var(--sub-color)', py: 3 }}
            >
              {t('recentSubmissions.noHistory')}
            </Typography>
          ) : (
            <List
              sx={{
                backgroundColor: 'var(--bg-color)',
                border: '1px solid var(--sub-alt-color)',
                borderRadius: 1,
                overflow: 'hidden',
              }}
            >
              {allSubmissions.map((submission) => (
                <ListItem key={submission.id} disablePadding divider>
                  <ListItemButton
                    onClick={() => handleSubmissionClick(submission.id)}
                    sx={{
                      borderRadius: 0,
                      py: 1.5,
                      px: 2,
                      '&:hover': {
                        backgroundColor: 'var(--bg-color-secondary)',
                      },
                    }}
                  >
                    <ListItemText
                      primary={submission.title}
                      secondary={format(
                        new Date(submission.submissionTime),
                        'PPpp',
                      )}
                      primaryTypographyProps={{
                        noWrap: true,
                        sx: {
                          color: 'var(--text-color)',
                          fontWeight: 500,
                        },
                      }}
                      secondaryTypographyProps={{
                        noWrap: true,
                        sx: { color: 'var(--sub-color)' },
                      }}
                    />
                    <Chip
                      label={`${submission.score}%`}
                      size="small"
                      sx={{
                        ml: 2,
                        minWidth: 60,
                        backgroundColor: `${getScoreColor(submission.score)}20`, // 20 is hex for 12% opacity
                        color: getScoreColor(submission.score),
                        border: `1px solid ${getScoreColor(submission.score)}`,
                        fontWeight: 'bold',
                      }}
                    />
                  </ListItemButton>
                </ListItem>
              ))}
            </List>
          )}
        </Paper>
      </Box>

      <Modal
        open={isOverlayOpen}
        onClose={closeOverlay}
        aria-labelledby="quiz-result-details"
        className={`theme-${currentTheme}`}
      >
        <Paper
          sx={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            width: { xs: '95%', sm: '80%', md: '70%' },
            maxWidth: 800,
            maxHeight: '90vh',
            overflow: 'auto',
            p: 4,
            backgroundColor: 'var(--bg-color)',
            color: 'var(--text-color)',
            borderRadius: 2,
            outline: 'none',
            border: '1px solid var(--sub-alt-color)',
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
          {isResultLoading ? (
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                height: 200,
              }}
            >
              <CircularProgress sx={{ color: 'var(--main-color)' }} />
            </Box>
          ) : resultError ? (
            <Typography sx={{ color: 'var(--error-color)' }}>
              {resultError}
            </Typography>
          ) : (
            selectedQuizResult && (
              <>
                <QuizResultOverlay
                  quizResult={selectedQuizResult}
                  isLoading={false}
                  error={null}
                />
                <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3 }}>
                  <Button
                    variant="contained"
                    onClick={closeOverlay}
                    sx={{
                      backgroundColor: 'var(--main-color)',
                      color: 'var(--bg-color)',
                      '&:hover': {
                        backgroundColor: 'var(--caret-color)',
                      },
                    }}
                  >
                    {t('button.close')}
                  </Button>
                </Box>
              </>
            )
          )}
        </Paper>
      </Modal>
    </>
  );
});

// Add display name
SubmissionHistory.displayName = 'SubmissionHistory';

export default SubmissionHistory;
