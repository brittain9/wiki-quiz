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
} from '@mui/material';
import { format } from 'date-fns';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';
import { SubmissionResponse } from '../types/quizSubmission.types';

const SubmissionHistory: React.FC = () => {
  const [submissions, setSubmissions] = useState<SubmissionResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchSubmissions = async () => {
      try {
        const recentSubmissions = await api.getRecentSubmissions();
        setSubmissions(recentSubmissions);
        setLoading(false);
      } catch (err) {
        setError('Failed to fetch recent submissions. Please try again later.');
        setLoading(false);
      }
    };

    fetchSubmissions();
  }, []);

  const handleSubmissionClick = (id: number) => {
    navigate(`/submission/${id}`);
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
  );
};

export default SubmissionHistory;