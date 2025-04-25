import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import FormControl from '@mui/material/FormControl';
import FormControlLabel from '@mui/material/FormControlLabel';
import LinearProgress from '@mui/material/LinearProgress';
import Paper from '@mui/material/Paper';
import Radio from '@mui/material/Radio';
import RadioGroup from '@mui/material/RadioGroup';
import Typography from '@mui/material/Typography';
import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer } from 'recharts';

import { useCustomTheme } from '../../context/CustomThemeContext/CustomThemeContext';
import { useOverlay } from '../../context/OverlayContext/OverlayContext';
import { useQuizState } from '../../context/QuizStateContext/QuizStateContext';
import { submissionApi } from '../../services';
import {
  QuizSubmission,
  QuestionAnswer,
} from '../../types/quizSubmission.types';

const Quiz: React.FC = React.memo(() => {
  const {
    isGenerating,
    currentQuiz,
    currentSubmission,
    setCurrentSubmission,
    addSubmissionToHistory,
  } = useQuizState();
  const { themeToDisplay } = useCustomTheme();
  const { showOverlay } = useOverlay();

  // State
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [userAnswers, setUserAnswers] = useState<QuestionAnswer[]>([]);
  const [quizSubmitted, setQuizSubmitted] = useState(false);
  const [score, setScore] = useState<number | null>(null);

  // Computed values
  const flatQuestions = useMemo(
    () =>
      currentQuiz?.aiResponses.flatMap((response) => response.questions) || [],
    [currentQuiz],
  );

  const totalQuestions = flatQuestions.length;
  const currentQuestion = flatQuestions[currentQuestionIndex];

  // Reset quiz state when quiz changes
  useEffect(() => {
    if (currentQuiz) {
      setUserAnswers([]);
      setCurrentQuestionIndex(0);
      setQuizSubmitted(false);
      setScore(null);
    }
  }, [currentQuiz]);

  // Handlers
  const handleViewDetailedSubmission = useCallback(() => {
    if (currentSubmission) {
      showOverlay('quiz_result', { resultId: currentSubmission.id });
    }
  }, [currentSubmission, showOverlay]);

  const handleAnswerChange = useCallback(
    (questionId: number, selectedOptionNumber: number) => {
      setUserAnswers((prev) => {
        const existingAnswerIndex = prev.findIndex(
          (a) => a.questionId === questionId,
        );
        if (existingAnswerIndex > -1) {
          const newAnswers = [...prev];
          newAnswers[existingAnswerIndex] = {
            questionId,
            selectedOptionNumber,
          };
          return newAnswers;
        } else {
          return [...prev, { questionId, selectedOptionNumber }];
        }
      });
    },
    [],
  );

  const handlePrevious = useCallback(() => {
    setCurrentQuestionIndex((prev) => Math.max(0, prev - 1));
  }, []);

  const handleSubmit = useCallback(async () => {
    if (!currentQuiz) return;

    try {
      const quizSubmission: QuizSubmission = {
        quizId: currentQuiz.id,
        questionAnswers: userAnswers,
      };
      const result = await submissionApi.submitQuiz(quizSubmission);
      setCurrentSubmission(result);
      addSubmissionToHistory(result);
      setQuizSubmitted(true);
      setScore(result.score);
    } catch (error) {
      console.error('Failed to submit quiz:', error);
    }
  }, [currentQuiz, userAnswers, setCurrentSubmission, addSubmissionToHistory]);

  const handleNextQuestion = useCallback(() => {
    if (currentQuestionIndex < totalQuestions - 1) {
      setCurrentQuestionIndex((prev) => prev + 1);
    } else {
      handleSubmit();
    }
  }, [currentQuestionIndex, totalQuestions, handleSubmit]);

  // Add score chart data calculation
  const getScoreChartData = useCallback(() => {
    if (score === null) return [];

    return [
      { name: 'Score', value: score },
      { name: 'Remaining', value: 100 - score },
    ];
  }, [score]);

  // Get score color based on percentage
  const getScoreColor = useCallback(() => {
    if (score === null) return 'var(--success-color)';
    if (score >= 80) return 'var(--success-color)';
    if (score >= 50) return 'var(--warning-color)';
    return 'var(--error-color)';
  }, [score]);

  // Loading state
  if (isGenerating) {
    return (
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          height: '400px',
          flexDirection: 'column',
          gap: 2,
        }}
        className={`theme-${themeToDisplay}`}
      >
        <CircularProgress
          size={50}
          thickness={4}
          sx={{ color: 'var(--main-color)' }}
        />
        <Typography variant="h6" sx={{ color: 'var(--sub-color)' }}>
          Generating Quiz...
        </Typography>
      </Box>
    );
  }

  if (!currentQuiz) return null;

  // Quiz content
  const currentAnswer = userAnswers.find(
    (a) => a.questionId === currentQuestion?.id,
  );
  const isLastQuestion = currentQuestionIndex === totalQuestions - 1;

  return (
    <Box
      sx={{
        maxWidth: 800,
        mx: 'auto',
        py: 4,
        backgroundColor: 'var(--bg-color)',
        mt: 4,
      }}
      className={`theme-${themeToDisplay}`}
    >
      {/* Progress Indicator */}
      <LinearProgress
        variant="determinate"
        value={
          quizSubmitted
            ? 100
            : Math.round(
                (userAnswers.length / Math.max(1, totalQuestions)) * 100,
              )
        }
        sx={{
          height: 8,
          borderRadius: 4,
          mb: 5,
          backgroundColor: 'var(--bg-color-tertiary)',
          '.MuiLinearProgress-bar': {
            backgroundColor: 'var(--main-color)',
          },
        }}
      />

      <Paper
        elevation={1}
        sx={{
          p: 0,
          borderRadius: 2,
          backgroundColor: 'var(--bg-color)',
          border: '1px solid var(--sub-alt-color)',
          overflow: 'hidden',
        }}
      >
        {quizSubmitted ? (
          /* Quiz Results Summary */
          <Box sx={{ p: 4 }}>
            <Typography
              variant="h5"
              gutterBottom
              sx={{
                color: 'var(--text-color)',
                fontWeight: 600,
                textAlign: 'center',
                mb: 3,
              }}
            >
              Quiz Complete!
            </Typography>

            <Box
              sx={{
                display: 'flex',
                flexDirection: { xs: 'column', sm: 'row' },
                justifyContent: 'center',
                alignItems: 'center',
                mb: 4,
                gap: { xs: 3, sm: 6 },
              }}
            >
              {/* Score chart */}
              <Box sx={{ position: 'relative', width: 160, height: 160 }}>
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={getScoreChartData()}
                      cx="50%"
                      cy="50%"
                      startAngle={90}
                      endAngle={-270}
                      innerRadius={45}
                      outerRadius={60}
                      paddingAngle={0}
                      dataKey="value"
                      strokeWidth={0}
                    >
                      <Cell key="score-cell-0" fill={getScoreColor()} />
                      <Cell
                        key="score-cell-1"
                        fill="var(--bg-color-tertiary)"
                      />
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
                    variant="h4"
                    component="div"
                    fontWeight="bold"
                    sx={{ color: getScoreColor() }}
                  >
                    {score !== null ? `${score}%` : '--'}
                  </Typography>
                  <Typography
                    variant="body2"
                    sx={{ color: 'var(--sub-color)' }}
                  >
                    Score
                  </Typography>
                </Box>
              </Box>

              {/* Quiz stats */}
              <Box>
                <Typography
                  sx={{
                    mb: 1,
                    color: 'var(--text-color)',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1,
                  }}
                >
                  <span
                    style={{
                      display: 'inline-block',
                      width: 12,
                      height: 12,
                      backgroundColor: 'var(--main-color)',
                      borderRadius: '50%',
                    }}
                  />
                  Questions: {totalQuestions}
                </Typography>
                <Typography
                  sx={{
                    mb: 1,
                    color: 'var(--text-color)',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1,
                  }}
                >
                  <span
                    style={{
                      display: 'inline-block',
                      width: 12,
                      height: 12,
                      backgroundColor: 'var(--success-color)',
                      borderRadius: '50%',
                    }}
                  />
                  Topic:{' '}
                  {currentQuiz.aiResponses[0]?.responseTopic || 'General'}
                </Typography>
                <Typography
                  sx={{
                    color: 'var(--text-color)',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1,
                  }}
                >
                  <span
                    style={{
                      display: 'inline-block',
                      width: 12,
                      height: 12,
                      backgroundColor: 'var(--warning-color)',
                      borderRadius: '50%',
                    }}
                  />
                  Quiz Title: {currentQuiz.title}
                </Typography>
              </Box>
            </Box>

            <Box
              sx={{
                display: 'flex',
                justifyContent: 'center',
                mt: { xs: 2, sm: 4 },
              }}
            >
              <Button
                variant="contained"
                onClick={handleViewDetailedSubmission}
                sx={{
                  backgroundColor: 'var(--main-color)',
                  color: 'var(--bg-color)',
                  padding: '10px 20px',
                  fontSize: '1rem',
                  fontWeight: 500,
                  textTransform: 'none',
                  '&:hover': {
                    backgroundColor: 'var(--caret-color)',
                  },
                }}
              >
                View Detailed Results
              </Button>
            </Box>
          </Box>
        ) : (
          /* Quiz Question */
          <Box sx={{ p: { xs: 2, sm: 4 } }}>
            {/* Progress bar */}
            <LinearProgress
              variant="determinate"
              value={
                ((currentQuestionIndex + 1) / Math.max(1, totalQuestions)) * 100
              }
              sx={{
                height: 6,
                borderRadius: 3,
                mb: 2,
                backgroundColor: 'var(--bg-color-tertiary)',
                '.MuiLinearProgress-bar': {
                  backgroundColor: 'var(--main-color)',
                },
              }}
            />

            <Typography
              variant="body2"
              align="right"
              sx={{ mb: 2, color: 'var(--sub-color)' }}
            >
              Question {currentQuestionIndex + 1} of {totalQuestions}
            </Typography>

            {/* Question */}
            <Typography
              variant="h6"
              sx={{
                mb: 3,
                fontWeight: 500,
                color: 'var(--text-color)',
                p: 2,
                borderLeft: '4px solid var(--main-color)',
                backgroundColor: 'var(--bg-color-secondary)',
                borderRadius: '0 4px 4px 0',
              }}
            >
              {currentQuestion.text}
            </Typography>

            {/* Options */}
            <FormControl component="fieldset" sx={{ width: '100%', mb: 4 }}>
              <RadioGroup
                value={currentAnswer?.selectedOptionNumber || ''}
                onChange={(e) => {
                  handleAnswerChange(
                    currentQuestion.id,
                    parseInt(e.target.value, 10),
                  );
                }}
              >
                {currentQuestion.options.map((option, index) => (
                  <FormControlLabel
                    key={index}
                    value={index + 1}
                    control={
                      <Radio
                        sx={{
                          color: 'var(--sub-color)',
                          '&.Mui-checked': {
                            color: 'var(--main-color)',
                          },
                        }}
                      />
                    }
                    label={option}
                    sx={{
                      mb: 1,
                      p: 1.5,
                      borderRadius: 1,
                      width: '100%',
                      transition: 'background-color 0.2s',
                      border: '1px solid transparent',
                      '&:hover': {
                        backgroundColor: 'var(--bg-color-secondary)',
                        border: '1px solid var(--sub-alt-color)',
                      },
                      ...(currentAnswer?.selectedOptionNumber === index + 1
                        ? {
                            backgroundColor: 'var(--main-color-10)',
                            border: '1px solid var(--main-color)',
                          }
                        : {}),
                    }}
                  />
                ))}
              </RadioGroup>
            </FormControl>

            {/* Navigation buttons */}
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                mt: 2,
              }}
            >
              <Button
                onClick={handlePrevious}
                disabled={currentQuestionIndex === 0}
                sx={{
                  color: 'var(--main-color)',
                  fontWeight: 500,
                  py: 1,
                  px: 3,
                  '&:hover': {
                    backgroundColor: 'var(--main-color-10)',
                  },
                  '&.Mui-disabled': {
                    color: 'var(--sub-alt-color)',
                  },
                }}
              >
                Previous
              </Button>
              <Button
                variant="contained"
                onClick={handleNextQuestion}
                disabled={!currentAnswer}
                sx={{
                  backgroundColor: 'var(--main-color)',
                  color: 'var(--bg-color)',
                  fontWeight: 500,
                  py: 1,
                  px: 3,
                  '&:hover': {
                    backgroundColor: 'var(--caret-color)',
                  },
                  '&.Mui-disabled': {
                    backgroundColor: 'var(--sub-alt-color)',
                    color: 'var(--sub-color)',
                  },
                }}
              >
                {isLastQuestion ? 'Submit' : 'Next'}
              </Button>
            </Box>
          </Box>
        )}
      </Paper>
    </Box>
  );
});

// Add display name
Quiz.displayName = 'Quiz';

export default Quiz;
