import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import FormControl from '@mui/material/FormControl';
import FormControlLabel from '@mui/material/FormControlLabel';
import LinearProgress from '@mui/material/LinearProgress';
import Modal from '@mui/material/Modal';
import Paper from '@mui/material/Paper';
import Radio from '@mui/material/Radio';
import RadioGroup from '@mui/material/RadioGroup';
import Typography from '@mui/material/Typography';
import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer } from 'recharts';

import QuizResultOverlay from './QuizResultOverlay';
import { useCustomTheme } from '../context/CustomThemeContext';
import { useQuizState } from '../context/QuizStateContext';
import { submissionApi } from '../services';
import { QuizResult } from '../types/quizResult.types';
import { QuizSubmission, QuestionAnswer } from '../types/quizSubmission.types';

const Quiz: React.FC = React.memo(() => {
  const {
    isGenerating,
    currentQuiz,
    currentSubmission,
    setCurrentSubmission,
    addSubmissionToHistory,
  } = useQuizState();
  const { currentTheme } = useCustomTheme();

  // State
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [userAnswers, setUserAnswers] = useState<QuestionAnswer[]>([]);
  const [quizSubmitted, setQuizSubmitted] = useState(false);
  const [score, setScore] = useState<number | null>(null);
  const [isOverlayOpen, setIsOverlayOpen] = useState(false);
  const [quizResult, setQuizResult] = useState<QuizResult | null>(null);
  const [isLoadingResult, setIsLoadingResult] = useState(false);
  const [resultError, setResultError] = useState<string | null>(null);

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
  const fetchQuizResult = useCallback(async (submissionId: number) => {
    setIsLoadingResult(true);
    setResultError(null);
    try {
      const result = await submissionApi.getSubmissionById(submissionId);
      setQuizResult(result);
    } catch (error) {
      console.error('Failed to fetch quiz result:', error);
      setResultError('Failed to load quiz result. Please try again.');
    } finally {
      setIsLoadingResult(false);
    }
  }, []);

  const handleViewDetailedSubmission = useCallback(() => {
    if (currentSubmission) {
      setIsOverlayOpen(true);
      fetchQuizResult(currentSubmission.id);
    }
  }, [currentSubmission, fetchQuizResult]);

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
        className={`theme-${currentTheme}`}
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
      className={`theme-${currentTheme}`}
    >
      <Typography
        variant="h4"
        gutterBottom
        align="center"
        sx={{ mb: 3, fontWeight: 600, color: 'var(--main-color)' }}
      >
        {currentQuiz.title} Quiz
      </Typography>

      <Paper
        elevation={3}
        sx={{
          p: 4,
          minHeight: 400,
          maxWidth: 700,
          mx: 'auto',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          borderRadius: 2,
          transition: 'all 0.3s ease',
          boxShadow: 'rgba(0, 0, 0, 0.1) 0px 10px 30px',
          backgroundColor: 'var(--bg-color)',
          color: 'var(--text-color)',
          border: '1px solid var(--sub-alt-color)',
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
        {quizSubmitted ? (
          <Box sx={{ textAlign: 'center' }}>
            <Typography
              variant="h5"
              gutterBottom
              sx={{ fontWeight: 600, color: 'var(--main-color)' }}
            >
              Quiz Completed!
            </Typography>

            <Box
              sx={{
                display: 'flex',
                justifyContent: 'center',
                my: 4,
                height: 200,
              }}
            >
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={getScoreChartData()}
                    innerRadius={60}
                    outerRadius={80}
                    paddingAngle={2}
                    dataKey="value"
                  >
                    <Cell fill={getScoreColor()} />
                    <Cell fill="var(--bg-color-tertiary)" />
                  </Pie>
                </PieChart>
              </ResponsiveContainer>
            </Box>

            <Typography
              variant="h4"
              sx={{
                fontWeight: 'bold',
                color: getScoreColor(),
                mb: 3,
              }}
            >
              {score}%
            </Typography>

            <Button
              variant="contained"
              onClick={handleViewDetailedSubmission}
              sx={{
                mt: 2,
                backgroundColor: 'var(--main-color)',
                color: 'var(--bg-color)',
                '&:hover': {
                  backgroundColor: 'var(--caret-color)',
                },
                px: 4,
                py: 1.5,
                fontSize: '1rem',
                fontWeight: 500,
              }}
            >
              View Detailed Results
            </Button>
          </Box>
        ) : (
          <Box>
            {/* Progress bar */}
            <LinearProgress
              variant="determinate"
              value={((currentQuestionIndex + 1) / totalQuestions) * 100}
              sx={{
                mb: 3,
                height: 8,
                borderRadius: 4,
                backgroundColor: 'var(--bg-color-tertiary)',
                '& .MuiLinearProgress-bar': {
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

      <Modal
        open={isOverlayOpen}
        onClose={() => setIsOverlayOpen(false)}
        aria-labelledby="quiz-result-modal"
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
          {isLoadingResult ? (
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
            quizResult && (
              <QuizResultOverlay
                quizResult={quizResult}
                isLoading={false}
                error={null}
              />
            )
          )}
        </Paper>
      </Modal>
    </Box>
  );
});

// Add display name
Quiz.displayName = 'Quiz';

export default Quiz;
