import React from 'react';
import { useTheme } from '@mui/material/styles';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Radio from '@mui/material/Radio';
import RadioGroup from '@mui/material/RadioGroup';
import FormControlLabel from '@mui/material/FormControlLabel';
import FormControl from '@mui/material/FormControl';
import { QuizResult } from '../../types/quizResult.types';

interface QuizResultComponentProps {
  quizResult: QuizResult;
}

const QuizResultComponent: React.FC<QuizResultComponentProps> = ({ quizResult }) => {
  const theme = useTheme();

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
};

export default QuizResultComponent;
