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
import React, { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';

import QuizResultOverlay from './QuizResultOverlay';
import { useAuth } from '../context/AuthProvider';
import { useQuizState } from '../context/QuizStateContext';
import useAuthCheck from '../hooks/useAuthCheck';
import { submissionApi } from '../services';
import { QuizResult } from '../types/quizResult.types';
import { SubmissionResponse } from '../types/quizSubmission.types';

const SubmissionHistory: React.FC = () => {
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
    const fetchSubmissions = async () => {
      // Only fetch if logged in
      if (!isLoggedIn) {
        setLoading(false);
        return;
      }
      
      try {
        const recentSubmissions = await submissionApi.getRecentSubmissions();
        setAllSubmissions(recentSubmissions);
      } catch (err) {
        setError('Failed to fetch recent submissions. Please try again later.');
      } finally {
        setLoading(false);
      }
    };

    fetchSubmissions();
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
  const handleSubmissionClick = async (id: number) => {
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
  };

  // Close the overlay
  const closeOverlay = () => {
    setIsOverlayOpen(false);
    setSelectedQuizResult(null);
  };

  // Render the component
  if (loading) {
    return (
      <Box
        display="flex"
        justifyContent="center"
        alignItems="center"
        minHeight="200px"
      >
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Typography color="error" align="center">
        {error}
      </Typography>
    );
  }
  
  // If not logged in, show message to log in
  if (!isLoggedIn) {
    return (
      <Paper
        elevation={3}
        sx={{ padding: 4, maxWidth: 800, margin: 'auto', mt: 5, mb: 5 }}
      >
        <Typography variant="h5" gutterBottom align="center">
          {t('recentSubmissions.title')}
        </Typography>
        <Typography align="center" sx={{ mb: 2 }}>
          {t('recentSubmissions.loginRequired')}
        </Typography>
        <Box display="flex" justifyContent="center">
          <Button 
            variant="contained" 
            color="primary"
            onClick={() => checkAuth()}
          >
            {t('login.loginToView')}
          </Button>
        </Box>
      </Paper>
    );
  }

  return (
    <>
      <Paper
        elevation={3}
        sx={{ padding: 2, maxWidth: 800, margin: 'auto', mt: 5, mb: 5 }}
      >
        <Typography variant="h5" gutterBottom align="center">
          {t('recentSubmissions.title')}
        </Typography>
        {allSubmissions.length === 0 ? (
          <Typography align="center">
            {' '}
            {t('recentSubmissions.noHistory')}
          </Typography>
        ) : (
          <List>
            {allSubmissions.map((submission) => (
              <ListItem key={submission.id} disablePadding>
                <ListItemButton
                  onClick={() => handleSubmissionClick(submission.id)}
                >
                  <ListItemText
                    primary={submission.title}
                    secondary={format(
                      new Date(submission.submissionTime),
                      'PPpp',
                    )}
                    primaryTypographyProps={{ noWrap: true }}
                    secondaryTypographyProps={{ noWrap: true }}
                  />
                  <Chip
                    label={`${submission.score}%`}
                    color={
                      submission.score >= 70
                        ? 'success'
                        : submission.score >= 50
                          ? 'warning'
                          : 'error'
                    }
                    size="small"
                    sx={{ ml: 2, minWidth: 60 }}
                  />
                </ListItemButton>
              </ListItem>
            ))}
          </List>
        )}
      </Paper>

      <Modal
        open={isOverlayOpen}
        onClose={closeOverlay}
        aria-labelledby="submission-history-modal"
        aria-describedby="submission-history-description"
      >
        <Box
          sx={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            width: '80%',
            maxWidth: 800,
            bgcolor: 'background.paper',
            boxShadow: 24,
            p: 4,
            maxHeight: '90vh',
            overflowY: 'auto',
          }}
        >
          <QuizResultOverlay
            quizResult={selectedQuizResult}
            isLoading={isResultLoading}
            error={resultError}
          />
          <Button onClick={closeOverlay} sx={{ mt: 2 }}>
            {t('button.close')}
          </Button>
        </Box>
      </Modal>
    </>
  );
};

export default SubmissionHistory;
