import React, { useState, useEffect } from 'react';
import { useTheme } from '@mui/material/styles';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Radio from '@mui/material/Radio';
import RadioGroup from '@mui/material/RadioGroup';
import FormControlLabel from '@mui/material/FormControlLabel';
import FormControl from '@mui/material/FormControl';
import Button from '@mui/material/Button';
import LinearProgress from '@mui/material/LinearProgress';
import CircularProgress from '@mui/material/CircularProgress';
import Modal from '@mui/material/Modal';
import { useQuizOptions } from '../context/QuizOptionsContext';
import { useQuizState } from '../context/QuizStateContext';
import { QuizSubmission, QuestionAnswer, SubmissionResponse } from '../types/quizSubmission.types';
import { QuizResult } from '../types/quizResult.types';
import api from '../services/api';
import QuizResultOverlay from './QuizResultOverlay';

const Quiz: React.FC = () => {
  const theme = useTheme();
  const { quizOptions } = useQuizOptions();
  const { 
    isGenerating, 
    isQuizReady, 
    currentQuiz, 
    currentSubmission, 
    setIsGenerating, 
    setIsQuizReady, 
    setCurrentQuiz, 
    setCurrentSubmission,
    addSubmissionToHistory
  } = useQuizState();

  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [userAnswers, setUserAnswers] = useState<QuestionAnswer[]>([]);
  const [quizSubmitted, setQuizSubmitted] = useState(false);
  const [score, setScore] = useState<number | null>(null);
  const [isOverlayOpen, setIsOverlayOpen] = useState(false);
  const [quizResult, setQuizResult] = useState<QuizResult | null>(null);
  const [isLoadingResult, setIsLoadingResult] = useState(false);
  const [resultError, setResultError] = useState<string | null>(null);

  const totalQuestions = currentQuiz?.aiResponses.reduce(
    (total, response) => total + response.questions.length,
    0
  ) || 0;

  useEffect(() => {
    if (currentQuiz) {
      setUserAnswers([]);
      setCurrentQuestionIndex(0);
      setQuizSubmitted(false);
      setScore(null);
    }
  }, [currentQuiz]);

  const fetchQuizResult = async (submissionId: number) => {
    setIsLoadingResult(true);
    setResultError(null);
    try {
      const result = await api.getSubmissionById(submissionId);
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

  if (isGenerating) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '400px' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!currentQuiz) return null;

  const flatQuestions = currentQuiz.aiResponses.flatMap(response => response.questions);
  const currentQuestion = flatQuestions[currentQuestionIndex];

  const handleAnswerChange = (questionId: number, selectedOptionNumber: number) => {
    setUserAnswers(prev => {
      const existingAnswerIndex = prev.findIndex(a => a.questionId === questionId);
      if (existingAnswerIndex > -1) {
        const newAnswers = [...prev];
        newAnswers[existingAnswerIndex] = { questionId, selectedOptionNumber };
        return newAnswers;
      } else {
        return [...prev, { questionId, selectedOptionNumber }];
      }
    });
  };

  const handleNextQuestion = () => {
    if (currentQuestionIndex < totalQuestions - 1) {
      setCurrentQuestionIndex(prev => prev + 1);
    } else {
      handleSubmit();
    }
  };

  const handleSubmit = async () => {
    if (currentQuiz) {
      try {
        const quizSubmission: QuizSubmission = {
          quizId: currentQuiz.id,
          questionAnswers: userAnswers
        };
        const result = await api.postQuiz(quizSubmission);
        setCurrentSubmission(result);
        addSubmissionToHistory(result);
        setQuizSubmitted(true);
        setScore(result.score);
      } catch (error) {
        console.error('Failed to submit quiz:', error);
      }
    }
  };

  return (
    <Box sx={{ maxWidth: 800, mx: 'auto', mb: 4 }}>
      <Typography variant="h4" gutterBottom align="center" sx={{ mb: 3 }}>
        {currentQuiz.title}
      </Typography>
      <Paper elevation={3} sx={{ p: 4, minHeight: 400, display: 'flex', flexDirection: 'column', justifyContent: 'center' }}>
        {quizSubmitted ? (
          <Box sx={{ textAlign: 'center' }}>
            <Typography variant="h5" gutterBottom>Quiz Completed</Typography>
            <Typography variant="h6" gutterBottom>Your Score: {score?.toFixed(2)}%</Typography>
            <Box sx={{ mt: 3, display: 'flex', justifyContent: 'center', gap: 2 }}>
              <Button variant="contained" color="secondary" onClick={handleViewDetailedSubmission}>
                View Detailed Submission
              </Button>
            </Box>
          </Box>
        ) : (
          <>
            <Typography variant="h6" gutterBottom>
              Question {currentQuestionIndex + 1} of {totalQuestions}
            </Typography>
            <LinearProgress 
              variant="determinate" 
              value={(currentQuestionIndex + 1) / totalQuestions * 100} 
              sx={{ mb: 3 }}
            />
            <Typography variant="body1" gutterBottom sx={{ mb: 2 }}>{currentQuestion.text}</Typography>
            <FormControl component="fieldset" sx={{ width: '100%' }}>
              <RadioGroup
                value={userAnswers.find(a => a.questionId === currentQuestion.id)?.selectedOptionNumber.toString() || ''}
                onChange={(e) => handleAnswerChange(currentQuestion.id, parseInt(e.target.value, 10))}
              >
                {currentQuestion.options.map((option, optionIndex) => (
                  <FormControlLabel
                    key={optionIndex}
                    value={(optionIndex + 1).toString()}
                    control={<Radio />}
                    label={option}
                    sx={{ mb: 1 }}
                  />
                ))}
              </RadioGroup>
            </FormControl>
            <Box sx={{ mt: 3, display: 'flex', justifyContent: 'space-between' }}>
              <Button 
                variant="outlined" 
                color="primary" 
                onClick={() => setCurrentQuestionIndex(prev => Math.max(0, prev - 1))}
                disabled={currentQuestionIndex === 0}
              >
                Previous
              </Button>
              <Button 
                variant="contained" 
                color="primary" 
                onClick={handleNextQuestion}
                disabled={!userAnswers.find(a => a.questionId === currentQuestion.id)}
              >
                {currentQuestionIndex === totalQuestions - 1 ? 'Submit Quiz' : 'Next Question'}
              </Button>
            </Box>
          </>
        )}
      </Paper>
      <Modal
        open={isOverlayOpen}
        onClose={() => setIsOverlayOpen(false)}
        aria-labelledby="detailed-submission-modal"
        aria-describedby="detailed-submission-description"
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
          <QuizResultOverlay quizResult={quizResult} isLoading={isLoadingResult} error={resultError} />
          <Button onClick={() => setIsOverlayOpen(false)}>Close</Button>
        </Box>
      </Modal>
    </Box>
  );
};

export default Quiz;