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
import React, { useState, useEffect } from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer } from 'recharts';

import QuizResultOverlay from './QuizResultOverlay';
import { useQuizState } from '../context/QuizStateContext';
import { submissionApi } from '../services';
import { QuizResult } from '../types/quizResult.types';
import { QuizSubmission, QuestionAnswer } from '../types/quizSubmission.types';

const Quiz: React.FC = () => {
  const {
    isGenerating,
    currentQuiz,
    currentSubmission,
    setCurrentSubmission,
    addSubmissionToHistory,
  } = useQuizState();

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
  const flatQuestions =
    currentQuiz?.aiResponses.flatMap((response) => response.questions) || [];
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
  const fetchQuizResult = async (submissionId: number) => {
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
  };

  const handleViewDetailedSubmission = () => {
    if (currentSubmission) {
      setIsOverlayOpen(true);
      fetchQuizResult(currentSubmission.id);
    }
  };

  const handleAnswerChange = (
    questionId: number,
    selectedOptionNumber: number,
  ) => {
    setUserAnswers((prev) => {
      const existingAnswerIndex = prev.findIndex(
        (a) => a.questionId === questionId,
      );
      if (existingAnswerIndex > -1) {
        const newAnswers = [...prev];
        newAnswers[existingAnswerIndex] = { questionId, selectedOptionNumber };
        return newAnswers;
      } else {
        return [...prev, { questionId, selectedOptionNumber }];
      }
    });
  };

  const handlePrevious = () => {
    setCurrentQuestionIndex((prev) => Math.max(0, prev - 1));
  };

  const handleNextQuestion = () => {
    if (currentQuestionIndex < totalQuestions - 1) {
      setCurrentQuestionIndex((prev) => prev + 1);
    } else {
      handleSubmit();
    }
  };

  const handleSubmit = async () => {
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
  };

  // Add score chart data calculation
  const getScoreChartData = () => {
    if (score === null) return [];

    const scoreValue = score;
    const remainingScore = 100 - scoreValue;

    return [
      { name: 'Score', value: scoreValue },
      { name: 'Remaining', value: remainingScore },
    ];
  };

  // Get score color based on percentage
  const getScoreColor = () => {
    if (score === null) return '#4caf50';
    if (score >= 80) return '#4caf50'; // success.main
    if (score >= 50) return '#ff9800'; // warning.main
    return '#f44336'; // error.main
  };

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
      >
        <CircularProgress size={50} thickness={4} />
        <Typography variant="h6" color="text.secondary">
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
    <Box sx={{ maxWidth: 800, mx: 'auto', mb: 4 }}>
      <Typography
        variant="h4"
        gutterBottom
        align="center"
        sx={{ mb: 3, fontWeight: 600, color: 'primary.main' }}
      >
        {currentQuiz.title} Quiz
      </Typography>

      <Paper
        elevation={3}
        sx={{
          p: 4,
          minHeight: 400,
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          borderRadius: 2,
          transition: 'all 0.3s ease',
          boxShadow: 4,
        }}
      >
        {quizSubmitted ? (
          <Box sx={{ textAlign: 'center' }}>
            <Typography variant="h5" gutterBottom sx={{ fontWeight: 600 }}>
              Quiz Completed
            </Typography>

            {/* Score Donut Chart */}
            <Box
              display="flex"
              justifyContent="center"
              alignItems="center"
              flexDirection="column"
              mb={3}
            >
              <Box position="relative" width={200} height={200}>
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={getScoreChartData()}
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
                      {getScoreChartData().map((entry, index) => (
                        <Cell
                          key={`cell-${index}`}
                          fill={index === 0 ? getScoreColor() : '#e0e0e0'}
                        />
                      ))}
                    </Pie>
                  </PieChart>
                </ResponsiveContainer>
                <Box
                  position="absolute"
                  top="50%"
                  left="50%"
                  sx={{
                    transform: 'translate(-50%, -50%)',
                    textAlign: 'center',
                  }}
                >
                  <Typography variant="h3" component="div" fontWeight="bold">
                    {score?.toFixed(0)}%
                  </Typography>
                  <Typography variant="subtitle2" color="text.secondary">
                    Your Score
                  </Typography>
                </Box>
              </Box>
            </Box>

            <Button
              variant="contained"
              color="primary"
              onClick={handleViewDetailedSubmission}
              sx={{
                borderRadius: 2,
                py: 1.2,
                mt: 2,
                fontWeight: 600,
                minWidth: 220,
              }}
            >
              View Detailed Results
            </Button>
          </Box>
        ) : (
          <>
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                mb: 1,
              }}
            >
              <Typography variant="h6" sx={{ fontWeight: 600 }}>
                Question {currentQuestionIndex + 1} of {totalQuestions}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Progress:{' '}
                {Math.round(
                  ((currentQuestionIndex + 1) / totalQuestions) * 100,
                )}
                %
              </Typography>
            </Box>

            <LinearProgress
              variant="determinate"
              value={((currentQuestionIndex + 1) / totalQuestions) * 100}
              sx={{ mb: 3, height: 8, borderRadius: 4 }}
            />

            <Typography
              variant="body1"
              sx={{
                mb: 3,
                fontWeight: 500,
                fontSize: '1.1rem',
                bgcolor: 'background.paper',
                p: 2,
                borderRadius: 1,
                boxShadow: 1,
                color: 'text.primary',
              }}
            >
              {currentQuestion.text}
            </Typography>

            <FormControl component="fieldset" sx={{ width: '100%' }}>
              <RadioGroup
                value={currentAnswer?.selectedOptionNumber.toString() || ''}
                onChange={(e) =>
                  handleAnswerChange(
                    currentQuestion.id,
                    parseInt(e.target.value, 10),
                  )
                }
              >
                {currentQuestion.options.map((option, optionIndex) => (
                  <FormControlLabel
                    key={optionIndex}
                    value={(optionIndex + 1).toString()}
                    control={<Radio color="primary" />}
                    label={option}
                    sx={{
                      mb: 1.5,
                      p: 1,
                      borderRadius: 1,
                      transition: 'all 0.2s',
                      '&:hover': {
                        bgcolor: 'action.hover',
                      },
                    }}
                  />
                ))}
              </RadioGroup>
            </FormControl>

            <Box
              sx={{ mt: 4, display: 'flex', justifyContent: 'space-between' }}
            >
              <Button
                variant="outlined"
                color="primary"
                onClick={handlePrevious}
                disabled={currentQuestionIndex === 0}
                sx={{
                  borderRadius: 2,
                  px: 3,
                }}
                startIcon={<span>←</span>}
              >
                Previous
              </Button>
              <Button
                variant="contained"
                color="primary"
                onClick={handleNextQuestion}
                disabled={!currentAnswer}
                sx={{
                  borderRadius: 2,
                  px: 3,
                  fontWeight: 600,
                }}
                endIcon={isLastQuestion ? null : <span>→</span>}
              >
                {isLastQuestion ? 'Submit Quiz' : 'Next Question'}
              </Button>
            </Box>
          </>
        )}
      </Paper>

      <Modal
        open={isOverlayOpen}
        onClose={() => setIsOverlayOpen(false)}
        aria-labelledby="detailed-submission-modal"
      >
        <Box
          sx={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            width: '90%',
            maxWidth: 800,
            bgcolor: 'background.paper',
            boxShadow: 24,
            p: 4,
            maxHeight: '90vh',
            overflowY: 'auto',
            borderRadius: 2,
          }}
        >
          <QuizResultOverlay
            quizResult={quizResult}
            isLoading={isLoadingResult}
            error={resultError}
          />
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 3 }}>
            <Button
              onClick={() => setIsOverlayOpen(false)}
              variant="contained"
              color="primary"
              sx={{ borderRadius: 2 }}
            >
              Close
            </Button>
          </Box>
        </Box>
      </Modal>
    </Box>
  );
};

export default React.memo(Quiz);
