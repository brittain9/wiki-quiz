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

import { useCustomTheme } from '../../context/CustomThemeContext/CustomThemeContext';
import { useOverlay } from '../../context/OverlayContext/OverlayContext';
import { submissionApi } from '../../services';
import { Question, QuizResult, ResultOption } from '../../types';

interface QuizResultOverlayProps {
  quizResult?: QuizResult | null;
  isLoading?: boolean;
  error?: string | null;
}

const QuizResultOverlay: React.FC<QuizResultOverlayProps> = (props) => {
  const { t } = useTranslation();
  const { themeToDisplay } = useCustomTheme();
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
    } catch (err) {
      setInternalError('Failed to fetch quiz result');
      console.error('Error fetching quiz result:', err);
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
      return null;
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
          {quizResult.answers.map((result: ResultOption, index: number) => {
            const question = getQuestionById(result.questionId);
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
                      color: 'var(--text-color)',
                      fontWeight: 600,
                      display: 'flex',
                      alignItems: 'center',
                      '&::before': {
                        content: '""',
                        display: 'inline-block',
                        width: 12,
                        height: 12,
                        marginRight: 1.5,
                        backgroundColor: 'var(--main-color)',
                        borderRadius: '50%',
                      },
                    }}
                  >
                    {t('quizresultoverlay.question')} {index + 1}:{' '}
                    {question?.text}
                  </Typography>
                </Box>

                <Box sx={{ width: '100%', pl: 2 }}>
                  <RadioGroup value={result.selectedAnswerChoice}>
                    {question?.options.map((option, optionIndex) => {
                      // Determine colors based on correctness
                      const isCorrect =
                        optionIndex + 1 === result.correctAnswerChoice;
                      const isSelected =
                        optionIndex + 1 === result.selectedAnswerChoice;
                      const isWrong = isSelected && !isCorrect;

                      let textColor, radioColor, bgColor, borderColor;
                      if (isCorrect) {
                        textColor = 'var(--success-color)';
                        radioColor = 'var(--success-color)';
                        bgColor = 'var(--success-color-10)';
                        borderColor = 'var(--success-color)';
                      } else if (isWrong) {
                        textColor = 'var(--error-color)';
                        radioColor = 'var(--error-color)';
                        bgColor = 'var(--error-color-10)';
                        borderColor = 'var(--error-color)';
                      } else {
                        textColor = 'var(--sub-color)';
                        radioColor = 'var(--sub-color)';
                        bgColor = 'transparent';
                        borderColor = 'var(--sub-alt-color)';
                      }

                      return (
                        <FormControlLabel
                          key={optionIndex}
                          value={optionIndex + 1}
                          control={
                            <Radio
                              sx={{
                                color: radioColor,
                                '&.Mui-checked': {
                                  color: radioColor,
                                },
                              }}
                              disabled
                            />
                          }
                          label={option}
                          sx={{
                            margin: '4px 0',
                            padding: '8px 16px',
                            borderRadius: 1,
                            width: '100%',
                            backgroundColor:
                              isSelected || isCorrect ? bgColor : 'transparent',
                            border: '1px solid',
                            borderColor:
                              isSelected || isCorrect
                                ? borderColor
                                : 'var(--sub-alt-color)',
                            color:
                              isSelected || isCorrect
                                ? textColor
                                : 'var(--text-color)',
                            transition: 'all 0.2s ease',
                            '.MuiFormControlLabel-label': {
                              width: '100%',
                              fontWeight: isSelected || isCorrect ? 500 : 400,
                            },
                          }}
                          disabled
                        />
                      );
                    })}
                  </RadioGroup>
                </Box>
              </ListItem>
            );
          })}
        </List>
      </>
    );
  };

  // If used as a direct component (not in modal), just render the content
  if (props.quizResult) {
    return renderContent();
  }

  return (
    <Modal
      open={isOpen}
      onClose={hideOverlay}
      closeAfterTransition
      aria-labelledby="quiz-result-modal"
      className={`theme-${themeToDisplay}`}
    >
      <Fade in={isOpen}>
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
            boxShadow: 'rgba(0, 0, 0, 0.25) 0px 15px 45px',
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
          <IconButton
            aria-label="close"
            onClick={hideOverlay}
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

          {renderContent()}
        </Paper>
      </Fade>
    </Modal>
  );
};

export default QuizResultOverlay;
