import React, { useState, useEffect, useRef } from 'react';
import { useTheme } from '@mui/material/styles';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Radio from '@mui/material/Radio';
import RadioGroup from '@mui/material/RadioGroup';
import FormControlLabel from '@mui/material/FormControlLabel';
import FormControl from '@mui/material/FormControl';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import { quizService } from '../services/quizService';
import { Quiz, Question } from '../types/quiz.types';
import { UserAnswer } from '../types/quizSubmission.types';
import { QuizResult } from '../types/quizResult.types';

interface QuizComponentProps {
  topic: string;
}

const QuizComponent: React.FC<QuizComponentProps> = ({ topic }) => {
  const [quiz, setQuiz] = useState<Quiz | null>(null);
  const [userAnswers, setUserAnswers] = useState<UserAnswer[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [quizResult, setQuizResult] = useState<QuizResult | null>(null);
  const theme = useTheme();
  const fetchedRef = useRef(false);

  useEffect(() => {
    const loadQuiz = async () => {
      if (fetchedRef.current) return;
      fetchedRef.current = true;

      setLoading(true);
      setError(null);
      try {
        console.log('Fetching quiz for topic:', topic);
        const fetchedQuiz = await quizService.fetchQuiz({ topic, numQuestions: 5 });
        console.log('Fetched quiz:', fetchedQuiz);
        setQuiz(fetchedQuiz);
      } catch (err) {
        console.error('Error fetching quiz:', err);
        setError(`Failed to load quiz. Error: ${err instanceof Error ? err.message : String(err)}`);
      } finally {
        setLoading(false);
      }
    };

    loadQuiz();
  }, [topic]);

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

  const handleSubmit = async () => {
    if (quiz) {
      try {
        setError(null);
        console.log('Submitting quiz with answers:', userAnswers);
        const result = await quizService.submitQuiz(quiz.id, userAnswers);
        console.log('Quiz submission result:', result);
        setQuizResult(result);
      } catch (err) {
        console.error('Error submitting quiz:', err);
        setError(`Failed to submit quiz. ${err instanceof Error ? err.message : String(err)}`);
      }
    }
  };
  
  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
        <Typography color="error">{error}</Typography>
      </Box>
    );
  }

  if (!quiz) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
        <Typography>No quiz available. Please try a different topic.</Typography>
      </Box>
    );
  }

  if (quizResult) {
    return (
      <Box sx={{ maxWidth: 800, margin: 'auto', padding: 3 }}>
        <Typography variant="h4" gutterBottom>
          Quiz Results: {quizResult.title}
        </Typography>
        <Typography variant="h5" gutterBottom>
          Score: {quizResult.correctAnswers} / {quizResult.totalQuestions}
        </Typography>
        {quizResult.aiResponses.map((response, responseIndex) => (
          <Box key={responseIndex} sx={{ marginBottom: 4 }}>
            <Typography variant="h6" gutterBottom>
              {response.responseTopic}
            </Typography>
            {response.questions.map((question, questionIndex) => (
              <Paper 
                key={question.id} 
                elevation={3} 
                sx={{ 
                  padding: 2, 
                  marginBottom: 2, 
                  backgroundColor: questionIndex % 2 === 0 
                    ? theme.palette.background.default 
                    : theme.palette.background.paper 
                }}
              >
                <Typography variant="subtitle1" gutterBottom>
                  {question.text}
                </Typography>
                {question.options.map((option, optionIndex) => (
                  <Typography 
                    key={optionIndex}
                    sx={{ 
                      color: optionIndex + 1 === question.correctOptionNumber ? 'green' : 
                             optionIndex === question.userSelectedOptionIndex && optionIndex + 1 !== question.correctOptionNumber ? 'red' : 'inherit'
                    }}
                  >
                    {option}
                    {optionIndex + 1 === question.correctOptionNumber && ' ✓'}
                    {optionIndex === question.userSelectedOptionIndex && optionIndex + 1 !== question.correctOptionNumber && ' ✗'}
                  </Typography>
                ))}
              </Paper>
            ))}
          </Box>
        ))}
      </Box>
    );
  }

  return (
    <Box sx={{ maxWidth: 800, margin: 'auto', padding: 3 }}>
      <Typography variant="h4" gutterBottom>
        Quiz: {quiz.title}
      </Typography>
      {quiz.aiResponses.map((response, responseIndex) => (
        <Box key={responseIndex} sx={{ marginBottom: 4 }}>
          <Typography variant="h5" gutterBottom>
            {response.responseTopic}
          </Typography>
          {response.questions.map((question: Question, questionIndex) => (
            <Paper 
              key={question.id} 
              elevation={3} 
              sx={{ 
                padding: 2, 
                marginBottom: 2, 
                backgroundColor: questionIndex % 2 === 0 
                  ? theme.palette.background.default 
                  : theme.palette.background.paper 
              }}
            >
              <Typography variant="h6" gutterBottom>
                {question.text}
              </Typography>
              <FormControl component="fieldset">
                <RadioGroup
                  value={userAnswers.find(a => a.questionId === question.id)?.selectedOptionNumber.toString() || ''}
                  onChange={(e) => handleAnswerChange(question.id, parseInt(e.target.value, 10))}
                >
                  {question.options.map((option, optionIndex) => (
                    <FormControlLabel
                      key={optionIndex}
                      value={(optionIndex + 1).toString()}
                      control={<Radio />}
                      label={option}
                    />
                  ))}
                </RadioGroup>
              </FormControl>
            </Paper>
          ))}
        </Box>
      ))}
      <Button variant="contained" color="primary" onClick={handleSubmit}>
        Submit Quiz
      </Button>
    </Box>
  );
};

export default QuizComponent;
