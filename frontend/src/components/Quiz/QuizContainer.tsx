import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import Paper from '@mui/material/Paper';
import Typography from '@mui/material/Typography';
import React, {
  useState,
  useEffect,
  useCallback,
  useMemo,
  useRef,
} from 'react';

import QuizQuestion from './QuizQuestion';
import QuizResult from './QuizResult';
import { useQuizState } from '../../context';
import { submissionApi, quizApi } from '../../services';
import { QuizSubmission, QuestionAnswer } from '../../types';

const QuizContainer: React.FC = React.memo(() => {
  const {
    isGenerating,
    currentQuiz,
    currentSubmission,
    setCurrentSubmission,
    addSubmissionToHistory,
  } = useQuizState();

  const containerRef = useRef<HTMLDivElement>(null);

  // State
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [userAnswers, setUserAnswers] = useState<QuestionAnswer[]>([]);
  const [quizSubmitted, setQuizSubmitted] = useState(false);
  const [score, setScore] = useState<number | null>(null);
  const [totalPoints, setTotalPoints] = useState(0);
  const [showResult, setShowResult] = useState(false);
  const [selectedOption, setSelectedOption] = useState<number | null>(null);
  const [currentQuestionPoints, setCurrentQuestionPoints] = useState(0);
  const [correctAnswer, setCorrectAnswer] = useState<number | undefined>(
    undefined,
  );
  const [correctAnswerText, setCorrectAnswerText] = useState<
    string | undefined
  >(undefined);

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
      setTotalPoints(0);
      setShowResult(false);
      setSelectedOption(null);
      setCurrentQuestionPoints(0);
    }
  }, [currentQuiz]);

  // Handle answer selection
  const handleAnswerSelected = useCallback(
    async (selectedOptionNumber: number) => {
      try {
        // Call the backend to validate the answer
        const response = await fetch('/api/quiz/validateanswer', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            // Add authorization header if needed
          },
          body: JSON.stringify({
            questionId: currentQuestion.id,
            selectedOptionNumber,
          }),
        });

        if (!response.ok) {
          throw new Error('Failed to validate answer');
        }

        const result = await response.json();

        setSelectedOption(selectedOptionNumber);
        setCurrentQuestionPoints(result.pointsEarned);
        setTotalPoints((prev) => prev + result.pointsEarned);
        setCorrectAnswer(result.correctOptionNumber);
        setCorrectAnswerText(result.correctAnswerText);
        setShowResult(true);

        // Add to user answers
        setUserAnswers((prev) => {
          const existingAnswerIndex = prev.findIndex(
            (a) => a.questionId === currentQuestion.id,
          );
          const newAnswer = {
            questionId: currentQuestion.id,
            selectedOptionNumber,
          };

          if (existingAnswerIndex > -1) {
            const newAnswers = [...prev];
            newAnswers[existingAnswerIndex] = newAnswer;
            return newAnswers;
          } else {
            return [...prev, newAnswer];
          }
        });

        // Auto-advance to next question after showing result
        setTimeout(() => {
          if (currentQuestionIndex < totalQuestions - 1) {
            setCurrentQuestionIndex((prev) => prev + 1);
            setShowResult(false);
            setSelectedOption(null);
            setCurrentQuestionPoints(0);
            setCorrectAnswer(undefined);
            setCorrectAnswerText(undefined);
          } else {
            // Last question - submit quiz
            handleSubmit();
          }
        }, 3000); // Show result for 3 seconds
      } catch (error) {
        console.error('Error validating answer:', error);
        // Handle error - maybe show a fallback or retry
      }
    },
    [currentQuestion, currentQuestionIndex, totalQuestions],
  );

  const handleSubmit = useCallback(async () => {
    if (!currentQuiz) return;

    try {
      const quizSubmission: QuizSubmission = {
        quizId: currentQuiz.id,
        questionAnswers: userAnswers,
      };
      const result = await quizApi.submitQuiz(quizSubmission);
      setCurrentSubmission(result);
      addSubmissionToHistory(result);
      setQuizSubmitted(true);
      setScore(result.score);
    } catch (error) {
      console.error('Failed to submit quiz:', error);
    }
  }, [currentQuiz, userAnswers, setCurrentSubmission, addSubmissionToHistory]);

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

  return (
    <Box
      ref={containerRef}
      sx={{
        maxWidth: 800,
        mx: 'auto',
        py: 4,
        backgroundColor: 'var(--bg-color)',
        mt: 4,
      }}
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
          <>
            {/* Points Display */}
            <Box
              sx={{
                p: 2,
                backgroundColor: 'var(--bg-color-secondary)',
                borderBottom: '1px solid var(--sub-alt-color)',
                textAlign: 'center',
              }}
            >
              <Typography
                variant="h6"
                sx={{
                  color: 'var(--main-color)',
                  fontWeight: 'bold',
                }}
              >
                Total Points: {totalPoints.toLocaleString()}
              </Typography>
            </Box>

            <QuizQuestion
              currentQuestion={currentQuestion}
              currentQuestionIndex={currentQuestionIndex}
              totalQuestions={totalQuestions}
              onAnswerSelected={handleAnswerSelected}
              showResult={showResult}
              selectedOption={selectedOption}
              pointsEarned={currentQuestionPoints}
              correctAnswer={correctAnswer}
              correctAnswerText={correctAnswerText}
            />
          </>
        )}
      </Paper>
    </Box>
  );
});

QuizContainer.displayName = 'QuizContainer';

export default QuizContainer;
