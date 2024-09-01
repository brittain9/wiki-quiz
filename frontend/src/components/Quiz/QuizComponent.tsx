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
import { useGlobalQuiz } from '../../context/GlobalQuizContext';
import { useQuizService } from '../../services/quizService';
import { QuizSubmission, QuestionAnswer } from '../../types/quizSubmission.types';

const Quiz: React.FC = () => {
  const theme = useTheme();
  const { quizOptions, setCurrentSubmission } = useGlobalQuiz();
  const { submitQuiz } = useQuizService();
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [userAnswers, setUserAnswers] = useState<QuestionAnswer[]>([]);
  const [quizSubmitted, setQuizSubmitted] = useState(false);
  const [score, setScore] = useState<number | null>(null);

  const currentQuiz = quizOptions.currentQuiz;
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

  if (quizOptions.isGenerating) {
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
        const result = await submitQuiz(quizSubmission);
        setCurrentSubmission(result);
        setQuizSubmitted(true);
        setScore(result.score);
      } catch (error) {
        console.error('Failed to submit quiz:', error);
      }
    }
  };

  return (
    <Box sx={{ maxWidth: 800, mx: 'auto', mb:4 }}>
      <Typography variant="h4" gutterBottom align="center" sx={{ mb: 3 }}>
        {currentQuiz.title}
      </Typography>
      <Paper elevation={3} sx={{ p: 4 }}>
        {quizSubmitted ? (
          <Box>
            <Typography variant="h5" gutterBottom>Quiz Completed</Typography>
            <Typography variant="h6" gutterBottom>Your Score: {score?.toFixed(2)}%</Typography>
            <Button variant="contained" color="primary" onClick={() => setQuizSubmitted(false)}>
              Review Questions
            </Button>
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
    </Box>
  );
};

export default Quiz;