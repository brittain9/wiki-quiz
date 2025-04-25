// components/Overlays/QuizResultOverlay.tsx
import CloseIcon from '@mui/icons-material/Close';
import {
  Box,
  Typography,
  List,
  ListItem,
  Radio,
  RadioGroup,
  FormControlLabel,
  CircularProgress,
  Button,
  Paper,
  Modal,
  IconButton,
  Fade,
} from '@mui/material';
import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { PieChart, Pie, Cell, ResponsiveContainer } from 'recharts';

import { useOverlay } from '../../context/OverlayContext/OverlayContext';
import { submissionApi } from '../../services';
import { Question, QuizResult, ResultOption } from '../../types';

interface QuizResultOverlayProps {
  quizResult?: QuizResult | null;
  isLoading?: boolean;
  error?: string | null;
  standalone?: boolean; // New prop to determine if this is a standalone component
}

const QuizResultOverlay: React.FC<QuizResultOverlayProps> = (props) => {
  const { t } = useTranslation();
  const { currentOverlay, overlayData, hideOverlay } = useOverlay();

  const [internalQuizResult, setInternalQuizResult] =
    useState<QuizResult | null>(null);
  const [internalIsLoading, setInternalIsLoading] = useState(false);
  const [internalError, setInternalError] = useState<string | null>(null);

  // Determine if we're using props or internal state
  const quizResult = props.quizResult || internalQuizResult;
  const isLoading =
    props.isLoading !== undefined ? props.isLoading : internalIsLoading;
  const error = props.error !== undefined ? props.error : internalError;

  const isOpen = currentOverlay === 'quiz_result';

  // Fetch quiz result when overlay opens (only if not using props)
  useEffect(() => {
    if (isOpen && overlayData?.resultId && !props.quizResult) {
      fetchQuizResult(overlayData.resultId);
    }
  }, [isOpen, overlayData, props.quizResult]);

  // Reset state when overlay closes
  useEffect(() => {
    if (!isOpen) {
      setInternalQuizResult(null);
    }
  }, [isOpen]);

  const fetchQuizResult = async (submissionId: number) => {
    setInternalIsLoading(true);
    setInternalError(null);
    try {
      const result = await submissionApi.getSubmissionById(submissionId);
      setInternalQuizResult(result);
    } catch {
      setInternalError('Failed to fetch quiz result');
    } finally {
      setInternalIsLoading(false);
    }
  };

  const getQuestionById = (questionId: number): Question | undefined => {
    if (!quizResult) return undefined;

    return quizResult.quiz.aiResponses
      .flatMap((response) => response.questions)
      .find((question) => question.id === questionId);
  };

  // Prepare chart data for score visualization
  const renderContent = () => {
    if (isLoading) {
      return (
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
      );
    }

    if (error) {
      return (
        <Typography sx={{ color: 'var(--error-color)' }}>{error}</Typography>
      );
    }

    if (!quizResult) {
      return (
        <Typography sx={{ color: 'var(--sub-color)' }}>
          No quiz result data available
        </Typography>
      );
    }

    const score = quizResult.score;
    const remainingScore = 100 - score;
    const data = [
      { name: 'Score', value: score },
      { name: 'Remaining', value: remainingScore },
    ];

    // Determine color based on score
    const getScoreColor = () => {
      if (score >= 80) return 'var(--success-color)';
      if (score >= 50) return 'var(--warning-color)';
      return 'var(--error-color)';
    };

    return (
      <>
        <Typography
          variant="h4"
          gutterBottom
          align="center"
          sx={{
            color: 'var(--main-color)',
            fontWeight: 600,
            mb: 3,
          }}
        >
          {quizResult.quiz.title} {t('quizresultoverlay.title')}
        </Typography>

        <Paper
          elevation={1}
          sx={{
            mb: 4,
            p: 3,
            backgroundColor: 'var(--bg-color-secondary)',
            borderRadius: 2,
            border: '1px solid var(--sub-alt-color)',
          }}
        >
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'center',
              alignItems: 'center',
              flexDirection: 'column',
            }}
          >
            <Box sx={{ position: 'relative', width: 200, height: 200 }}>
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={data}
                    cx="50%"
                    cy="50%"
                    startAngle={90}
                    endAngle={-270}
                    innerRadius={60}
                    outerRadius={80}
                    paddingAngle={0}
                    dataKey="value"
                    strokeWidth={0}
                  >
                    <Cell key="cell-0" fill={getScoreColor()} />
                    <Cell key="cell-1" fill="var(--bg-color-tertiary)" />
                  </Pie>
                </PieChart>
              </ResponsiveContainer>
              <Box
                sx={{
                  position: 'absolute',
                  top: '50%',
                  left: '50%',
                  transform: 'translate(-50%, -50%)',
                  textAlign: 'center',
                }}
              >
                <Typography
                  variant="h3"
                  component="div"
                  fontWeight="bold"
                  sx={{ color: getScoreColor() }}
                >
                  {score.toFixed(0)}%
                </Typography>
                <Typography
                  variant="subtitle2"
                  sx={{ color: 'var(--sub-color)' }}
                >
                  {t('quizresultoverlay.score')}
                </Typography>
              </Box>
            </Box>
          </Box>
        </Paper>

        <Typography
          variant="h5"
          sx={{
            mb: 2,
            color: 'var(--text-color)',
            fontWeight: 600,
            borderLeft: '4px solid var(--main-color)',
            pl: 2,
          }}
        >
          Quiz Questions
        </Typography>

        <List
          sx={{
            backgroundColor: 'var(--bg-color)',
            border: '1px solid var(--sub-alt-color)',
            borderRadius: 2,
            overflow: 'hidden',
          }}
        >
          {quizResult.answers.length > 0 ? (
            quizResult.answers.map((result: ResultOption, index: number) => {
              const question = getQuestionById(result.questionId);

              if (!question) {
                return null;
              }

              return (
                <ListItem
                  key={result.questionId}
                  sx={{
                    borderBottom: '1px solid var(--sub-alt-color)',
                    py: 3,
                    px: 3,
                    flexDirection: 'column',
                    alignItems: 'flex-start',
                    backgroundColor:
                      index % 2 === 0
                        ? 'var(--bg-color)'
                        : 'var(--bg-color-secondary)',
                  }}
                >
                  <Box sx={{ width: '100%', mb: 2 }}>
                    <Typography
                      variant="subtitle1"
                      gutterBottom
                      sx={{
                        fontWeight: 600,
                        color: 'var(--text-color)',
                      }}
                    >
                      {index + 1}. {question.text}
                    </Typography>
                  </Box>

                  <RadioGroup
                    name={`question-${result.questionId}`}
                    value={result.selectedAnswerChoice}
                    sx={{ width: '100%' }}
                  >
                    {question.options.map((optionText, optionIndex) => {
                      // Determine option appearance based on correctness
                      // Option indexes are 0-based, but answer choices are 1-based
                      const optionNumber = optionIndex + 1;
                      const isCorrect =
                        optionNumber === result.correctAnswerChoice;
                      const isSelected =
                        optionNumber === result.selectedAnswerChoice;

                      // Style by case
                      let optionColor = 'var(--text-color)';
                      let optionBgColor = 'transparent';
                      let borderColor = 'var(--sub-alt-color)';

                      if (isSelected && isCorrect) {
                        // Correct answer - user selected
                        optionColor = 'var(--success-color)';
                        optionBgColor = 'var(--success-color-10)';
                        borderColor = 'var(--success-color)';
                      } else if (isSelected && !isCorrect) {
                        // Wrong answer - user selected
                        optionColor = 'var(--error-color)';
                        optionBgColor = 'var(--error-color-10)';
                        borderColor = 'var(--error-color)';
                      } else if (isCorrect) {
                        // Correct answer - not selected
                        optionColor = 'var(--success-color)';
                        borderColor = 'var(--success-color)';
                      }

                      return (
                        <FormControlLabel
                          key={optionNumber}
                          value={optionNumber}
                          disabled
                          control={
                            <Radio
                              sx={{
                                color: optionColor,
                                '&.Mui-checked': {
                                  color: optionColor,
                                },
                              }}
                            />
                          }
                          label={optionText}
                          sx={{
                            mb: 1,
                            p: 1,
                            borderRadius: 1,
                            border: `1px solid ${borderColor}`,
                            backgroundColor: optionBgColor,
                            color: optionColor,
                            width: '100%',
                          }}
                        />
                      );
                    })}
                  </RadioGroup>
                </ListItem>
              );
            })
          ) : (
            <ListItem>
              <Typography sx={{ color: 'var(--sub-color)' }}>
                No answers available
              </Typography>
            </ListItem>
          )}
        </List>

        {!props.standalone && (
          <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
            <Button
              variant="contained"
              onClick={hideOverlay}
              sx={{
                backgroundColor: 'var(--main-color)',
                color: 'var(--bg-color)',
                '&:hover': {
                  backgroundColor: 'var(--caret-color)',
                },
              }}
            >
              {t('quizresultoverlay.close')}
            </Button>
          </Box>
        )}
      </>
    );
  };

  // When used as a direct child in another component's modal
  if (props.standalone) {
    return renderContent();
  }

  // When used via the overlay system
  return (
    <Modal
      open={isOpen}
      onClose={hideOverlay}
      closeAfterTransition
      className="quiz-result-modal"
      sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        zIndex: 1300,
      }}
    >
      <Fade in={isOpen}>
        <Box
          sx={{
            width: { xs: '90%', sm: '80%', md: '70%' },
            maxWidth: 900,
            maxHeight: '90vh',
            overflow: 'auto',
            bgcolor: 'var(--bg-color)',
            color: 'var(--text-color)',
            borderRadius: 2,
            boxShadow: 24,
            p: { xs: 2, sm: 4 },
            outline: 'none',
            position: 'relative',
            m: 2,
          }}
        >
          <IconButton
            onClick={hideOverlay}
            sx={{
              position: 'absolute',
              right: 8,
              top: 8,
              color: 'var(--sub-color)',
              zIndex: 2,
              '&:hover': {
                color: 'var(--main-color)',
                backgroundColor: 'var(--bg-color-secondary)',
              },
            }}
          >
            <CloseIcon />
          </IconButton>

          {renderContent()}
        </Box>
      </Fade>
    </Modal>
  );
};

export default QuizResultOverlay;
