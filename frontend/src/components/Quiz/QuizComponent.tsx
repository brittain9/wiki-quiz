import React, { useEffect } from 'react';
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
import { useGlobalQuiz } from '../../context/GlobalQuizContext';
import { QuestionAnswer } from '../../types/quizSubmission.types';
import { useQuizService } from '../../services/quizService';
import { QuizSubmission } from '../../types/quizSubmission.types';

const Quiz: React.FC = () => {
  const theme = useTheme();
  const { quizOptions, setCurrentQuizResult } = useGlobalQuiz();
  const { submitQuiz } = useQuizService();
  const [userAnswers, setUserAnswers] = React.useState<QuestionAnswer[]>([]);

    // Reset userAnswers when a new quiz is loaded
    useEffect(() => {
      if (quizOptions.currentQuiz) {
        setUserAnswers([]);
      }
    }, [quizOptions.currentQuiz]);
  
    if (!quizOptions.currentQuiz && !quizOptions.currentQuizResult) {
      return null;
    }
  
    if (quizOptions.isGenerating) {
      return (
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
          <CircularProgress />
        </Box>
      );
    }

  if (!quizOptions.currentQuiz && !quizOptions.currentQuizResult) {
    return null;
  }

  if (quizOptions.isGenerating) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

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
    if (quizOptions.currentQuiz) {
      try {
        const quizSubmission: QuizSubmission = {
          quizId: quizOptions.currentQuiz.id,
          questionAnswers: userAnswers.map(answer => ({
            questionId: answer.questionId,
            selectedOptionNumber: answer.selectedOptionNumber
          }))
        };
  
        const result = await submitQuiz(quizSubmission);
        setCurrentQuizResult(result);
      } catch (error) {
        console.error('Failed to submit quiz:', error);
        // Handle error (e.g., show error message to user)
      }
    }
  };
  

  if (quizOptions.currentQuizResult) {
    // Render Quiz Result
    return (
      <Box sx={{ maxWidth: 800, margin: 'auto', padding: 3 }}>
        <Typography variant="h4" gutterBottom>
          Quiz Results: {quizOptions.currentQuizResult.title}
        </Typography>
        <Typography variant="h5" gutterBottom>
          Score: {quizOptions.currentQuizResult.correctAnswers} / {quizOptions.currentQuizResult.totalQuestions}
        </Typography>
        {quizOptions.currentQuizResult.aiResponses.map((response, responseIndex) => (
          <Box key={responseIndex} sx={{ marginBottom: 4 }}>
            <Typography variant="h5" gutterBottom>
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
                <Typography variant="h6" gutterBottom>
                  {question.text}
                </Typography>
                <FormControl component="fieldset">
                  <RadioGroup
                    value={question.userSelectedOption?.toString() || ''}
                  >
                    {question.options.map((option, optionIndex) => {
                      const optionNumber = optionIndex + 1;
                      const isCorrect = optionNumber === question.correctOptionNumber;
                      const isSelected = optionNumber === question.userSelectedOption;
                      const isIncorrectSelection = isSelected && !isCorrect;

                      return (
                        <FormControlLabel
                          key={optionIndex}
                          value={optionNumber.toString()}
                          control={
                            <Radio 
                              checked={isSelected || isCorrect}
                              sx={{
                                color: isCorrect ? 'green' : isIncorrectSelection ? 'red' : 'inherit',
                                '&.Mui-checked': {
                                  color: isCorrect ? 'green' : isIncorrectSelection ? 'red' : 'inherit',
                                },
                              }}
                            />
                          }
                          label={
                            <Typography
                              sx={{
                                color: isCorrect ? 'green' : isIncorrectSelection ? 'red' : 'inherit',
                              }}
                            >
                              {option}
                              {isCorrect && ' ✓'}
                              {isIncorrectSelection && ' ✗'}
                            </Typography>
                          }
                          sx={{
                            pointerEvents: 'none',
                          }}
                        />
                      );
                    })}
                  </RadioGroup>
                </FormControl>
              </Paper>
            ))}
          </Box>
        ))}
      </Box>
    );
  }

  // Render Quiz In Progress
  return (
    <Box sx={{ maxWidth: 800, margin: 'auto', padding: 3 }}>
      <Typography variant="h4" gutterBottom>
        Quiz: {quizOptions.currentQuiz?.title}
      </Typography>
      {quizOptions.currentQuiz?.aiResponses.map((response, responseIndex) => (
        <Box key={responseIndex} sx={{ marginBottom: 4 }}>
          <Typography variant="h5" gutterBottom>
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

export default Quiz;
