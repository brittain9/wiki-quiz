import React, { useState, useEffect } from 'react';
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
import api from '../services/api';
import { SubmissionResponse } from '../types/quizSubmission.types';
import { useQuizResultOverlay } from '../hooks/useQuizResultOverlay';
import QuizResultOverlay from './QuizResultOverlay';

const SubmissionHistory: React.FC = () => {
  const [submissions, setSubmissions] = useState<SubmissionResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const {
    isOverlayOpen,
    quizResult,
    isLoading: isResultLoading,
    error: resultError,
    openOverlay,
    closeOverlay,
  } = useQuizResultOverlay();

  useEffect(() => {
    const fetchSubmissions = async () => {
      try {
        const recentSubmissions = await api.getRecentSubmissions();
        setSubmissions(recentSubmissions);
      } catch (err) {
        setError('Failed to fetch recent submissions. Please try again later.');
      } finally {
        setLoading(false);
      }
    };

    fetchSubmissions();
  }, []);

  const handleSubmissionClick = (id: number) => {
    openOverlay(id);
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
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

  return (
    <>
      <Paper elevation={3} sx={{ padding: 2, maxWidth: 800, margin: 'auto', mt: 5, mb: 5 }}>
        <Typography variant="h5" gutterBottom>
          Recent Submissions
        </Typography>
        {submissions.length === 0 ? (
          <Typography>No recent submissions found.</Typography>
        ) : (
          <List>
            {submissions.map((submission) => (
              <ListItem key={submission.id} disablePadding>
                <ListItemButton onClick={() => handleSubmissionClick(submission.id)}>
                  <ListItemText
                    primary={submission.title}
                    secondary={format(new Date(submission.submissionTime), 'PPpp')}
                    primaryTypographyProps={{ noWrap: true }}
                    secondaryTypographyProps={{ noWrap: true }}
                  />
                  <Chip
                    label={`${submission.score}%`}
                    color={submission.score >= 70 ? 'success' : submission.score >= 50 ? 'warning' : 'error'}
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
        <Box sx={{
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
        }}>
          <QuizResultOverlay quizResult={quizResult} isLoading={isResultLoading} error={resultError} />
          <Button onClick={closeOverlay} sx={{ mt: 2 }}>Close</Button>
        </Box>
      </Modal>
    </>
  );
};

export default SubmissionHistory;