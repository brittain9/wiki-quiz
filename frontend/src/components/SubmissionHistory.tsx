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
  Pagination,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Stack,
} from '@mui/material';
import { format } from 'date-fns';
import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';

import { useAuth } from '../context/AuthContext/AuthContext';
import { useOverlay } from '../context/OverlayContext/OverlayContext';
import { useQuizState } from '../context/QuizStateContext/QuizStateContext';
import useAuthCheck from '../hooks/useAuthCheck';
import { submissionApi } from '../services';
import { SubmissionResponse, PaginatedResponse } from '../types/quizSubmission.types';

const SubmissionHistory: React.FC = React.memo(() => {
  const { submissionHistory: newSubmissions } = useQuizState();
  const [paginatedData, setPaginatedData] = useState<PaginatedResponse<SubmissionResponse>>({
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 10,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false,
  });
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { isLoggedIn } = useAuth();
  const { t } = useTranslation();
  const { showOverlay } = useOverlay();

  const { checkAuth } = useAuthCheck({
    message: t('login.viewSubmissionsMessage'),
  });

  // Fetch paginated submissions from the backend
  const fetchSubmissions = useCallback(async (page: number, size: number) => {
    if (!isLoggedIn) {
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const response = await submissionApi.getMySubmissionsPaginated(page, size);
      setPaginatedData(response);
    } catch {
      setError('Failed to fetch submissions. Please try again later.');
    } finally {
      setLoading(false);
    }
  }, [isLoggedIn]);

  // Fetch submissions when component mounts or when page/pageSize changes
  useEffect(() => {
    fetchSubmissions(currentPage, pageSize);
  }, [fetchSubmissions, currentPage, pageSize]);

  // Handle new submissions from quiz state (for real-time updates)
  useEffect(() => {
    if (newSubmissions.length > 0 && currentPage === 1) {
      // Only update if we're on the first page to avoid confusion
      setPaginatedData(prev => {
        const existingIds = new Set(prev.items.map(item => item.id));
        const newItems = newSubmissions.filter(item => !existingIds.has(item.id));
        
        if (newItems.length === 0) return prev;

        const updatedItems = [...newItems, ...prev.items].slice(0, pageSize);
        return {
          ...prev,
          items: updatedItems,
          totalCount: prev.totalCount + newItems.length,
        };
      });
    }
  }, [newSubmissions, currentPage, pageSize]);

  // Handle page change
  const handlePageChange = useCallback((_: React.ChangeEvent<unknown>, page: number) => {
    setCurrentPage(page);
  }, []);

  // Handle page size change
  const handlePageSizeChange = useCallback((event: any) => {
    const newPageSize = event.target.value as number;
    setPageSize(newPageSize);
    setCurrentPage(1); // Reset to first page when changing page size
  }, []);

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

  // Page size options
  const pageSizeOptions = [5, 10, 20, 50];

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

          {/* Page size selector */}
          <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="body2" sx={{ color: 'var(--sub-color)' }}>
              {paginatedData.totalCount > 0 
                ? `Showing ${((currentPage - 1) * pageSize) + 1}-${Math.min(currentPage * pageSize, paginatedData.totalCount)} of ${paginatedData.totalCount} submissions`
                : 'No submissions found'
              }
            </Typography>
            <FormControl size="small" sx={{ minWidth: 120 }}>
              <InputLabel 
                sx={{ 
                  color: 'var(--sub-color)',
                  '&.Mui-focused': { color: 'var(--main-color)' }
                }}
              >
                Per Page
              </InputLabel>
              <Select
                value={pageSize}
                label="Per Page"
                onChange={handlePageSizeChange}
                sx={{
                  color: 'var(--text-color)',
                  '& .MuiOutlinedInput-notchedOutline': {
                    borderColor: 'var(--sub-alt-color)',
                  },
                  '&:hover .MuiOutlinedInput-notchedOutline': {
                    borderColor: 'var(--main-color)',
                  },
                  '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
                    borderColor: 'var(--main-color)',
                  },
                  '& .MuiSvgIcon-root': {
                    color: 'var(--text-color)',
                  },
                }}
              >
                {pageSizeOptions.map((option) => (
                  <MenuItem 
                    key={option} 
                    value={option}
                    sx={{
                      color: 'var(--text-color)',
                      backgroundColor: 'var(--bg-color)',
                      '&:hover': {
                        backgroundColor: 'var(--bg-color-secondary)',
                      },
                    }}
                  >
                    {option}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Box>

          {paginatedData.items.length === 0 ? (
            <Typography sx={{ color: 'var(--sub-color)' }}>
              {t('recentSubmissions.noSubmissions')}
            </Typography>
          ) : (
            <>
              <List
                sx={{
                  width: '100%',
                  bgcolor: 'var(--bg-color)',
                  borderRadius: 1,
                  border: '1px solid var(--sub-alt-color)',
                  mb: 2,
                }}
              >
                {paginatedData.items.map((submission) => (
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

              {/* Pagination controls */}
              {paginatedData.totalPages > 1 && (
                <Stack spacing={2} alignItems="center">
                  <Pagination
                    count={paginatedData.totalPages}
                    page={currentPage}
                    onChange={handlePageChange}
                    color="primary"
                    sx={{
                      '& .MuiPaginationItem-root': {
                        color: 'var(--text-color)',
                        borderColor: 'var(--sub-alt-color)',
                        '&:hover': {
                          backgroundColor: 'var(--bg-color-secondary)',
                        },
                        '&.Mui-selected': {
                          backgroundColor: 'var(--main-color)',
                          color: 'var(--bg-color)',
                          '&:hover': {
                            backgroundColor: 'var(--caret-color)',
                          },
                        },
                      },
                    }}
                  />
                </Stack>
              )}
            </>
          )}
        </Paper>
      </Box>
    </>
  );
});

SubmissionHistory.displayName = 'SubmissionHistory';

export default SubmissionHistory;
