import React, { useState } from 'react';
import { useTheme } from '@mui/material/styles';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Radio from '@mui/material/Radio';
import RadioGroup from '@mui/material/RadioGroup';
import FormControlLabel from '@mui/material/FormControlLabel';
import FormControl from '@mui/material/FormControl';
import Button from '@mui/material/Button';
import { Quiz, Question } from '../../types/quiz.types';
import { QuestionAnswer } from '../../types/quizSubmission.types';

interface QuizInProgressProps {
  quiz: Quiz;
  onSubmit: (quizId: number, userAnswers: QuestionAnswer[]) => Promise<void>;
}

const QuizInProgressComponent: React.FC<QuizInProgressProps> = ({ quiz, onSubmit }) => {
  const theme = useTheme();
  const [userAnswers, setUserAnswers] = useState<QuestionAnswer[]>([]);

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

  const handleSubmit = () => {
    console.log('User answers before submission:', JSON.stringify(userAnswers, null, 2));
    onSubmit(quiz.id, userAnswers);
  };

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

export default QuizInProgressComponent;
