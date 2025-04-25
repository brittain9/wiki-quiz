import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import Paper from '@mui/material/Paper';
import Typography from '@mui/material/Typography';
import React, { useState, useEffect, useCallback, useMemo } from 'react';

import QuizQuestion from './QuizQuestion';
import QuizResult from './QuizResult';
import { useCustomTheme, useQuizState } from '../../context';
import { submissionApi } from '../../services';
import { QuizSubmission, QuestionAnswer } from '../../types';

const QuizContainer: React.FC = React.memo(() => {
  const {
    isGenerating,
    currentQuiz,
    currentSubmission,
    setCurrentSubmission,
    addSubmissionToHistory,
  } = useQuizState();
  const { themeToDisplay } = useCustomTheme();

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

  // Find current answer
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
          <QuizResult
            currentQuiz={currentQuiz}
            currentSubmission={currentSubmission}
            score={score}
            totalQuestions={totalQuestions}
          />
        ) : (
          <QuizQuestion
            currentQuestion={currentQuestion}
            currentQuestionIndex={currentQuestionIndex}
            totalQuestions={totalQuestions}
            currentAnswer={currentAnswer}
            isLastQuestion={isLastQuestion}
            onAnswerChange={handleAnswerChange}
            onPrevious={handlePrevious}
            onNext={handleNextQuestion}
          />
        )}
      </Paper>
    </Box>
  );
});

QuizContainer.displayName = 'QuizContainer';

export default QuizContainer;
