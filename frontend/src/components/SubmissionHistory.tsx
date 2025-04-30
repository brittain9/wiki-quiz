import CloseIcon from '@mui/icons-material/Close';
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
  IconButton,
} from '@mui/material';
import { format } from 'date-fns';
import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';

import QuizResultOverlay from './Overlays/QuizResultOverlay';
import { useAuth } from '../context/AuthContext/AuthContext';
import { useQuizState } from '../context/QuizStateContext/QuizStateContext';
import useAuthCheck from '../hooks/useAuthCheck';
import { submissionApi } from '../services';
import { QuizResult } from '../types/quizResult.types';
import { SubmissionResponse, SubmissionDetail, QuestionAnswer } from '../types/quizSubmission.types';

// Helper to map SubmissionDetail to QuizResult
function mapSubmissionDetailToQuizResult(detail: SubmissionDetail): QuizResult {
  // Build a map of correct answers from quiz object
  const correctAnswers: Record<number, number> = {};
  detail.quiz.aiResponses.forEach((aiResp) => {
    aiResp.questions.forEach((q) => {
      // Assume correct answer is always the first option (index 0) unless you have a better way
      correctAnswers[q.id] = 0;
    });
  });
  return {
    quiz: detail.quiz,
    answers: detail.questionAnswers.map((qa: QuestionAnswer) => ({
      questionId: qa.questionId,
      correctAnswerChoice: correctAnswers[qa.questionId] ?? 0,
      selectedAnswerChoice: qa.selectedOptionNumber,
    })),
    submissionTime: detail.submissionTime,
    score: detail.score,
  };
}

const SubmissionHistory: React.FC = React.memo(() => {
  const { submissionHistory: newSubmissions } = useQuizState();
  const [allSubmissions, setAllSubmissions] = useState<SubmissionResponse[]>(
    [],
  );
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { isLoggedIn } = useAuth();
  const { t } = useTranslation();

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

      setIsResultLoading(true);
      setResultError(null);
      try {
        const result = await submissionApi.getQuizSubmissionById(id);
        setSelectedQuizResult(mapSubmissionDetailToQuizResult(result));
        setIsOverlayOpen(true);
      } catch {
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

      <Modal
        open={isOverlayOpen}
        onClose={closeOverlay}
        aria-labelledby="quiz-result-modal"
      >
        <Box
          sx={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            width: { xs: '95%', sm: '80%', md: '70%' },
            maxWidth: 800,
            maxHeight: '90vh',
            overflow: 'auto',
            bgcolor: 'var(--bg-color)',
            color: 'var(--text-color)',
            borderRadius: 2,
            boxShadow: 24,
            p: 4,
            outline: 'none',
          }}
        >
          <IconButton
            aria-label="close"
            onClick={closeOverlay}
            sx={{
              position: 'absolute',
              right: 12,
              top: 12,
              color: 'var(--sub-color)',
              '&:hover': {
                color: 'var(--main-color)',
                backgroundColor: 'var(--bg-color-secondary)',
              },
            }}
          >
            <CloseIcon />
          </IconButton>
          <QuizResultOverlay
            quizResult={selectedQuizResult}
            isLoading={isResultLoading}
            error={resultError}
            standalone={true}
          />
        </Box>
      </Modal>
    </>
  );
});

SubmissionHistory.displayName = 'SubmissionHistory';

export default SubmissionHistory;