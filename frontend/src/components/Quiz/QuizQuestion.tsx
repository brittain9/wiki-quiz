import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import FormControl from '@mui/material/FormControl';
import FormControlLabel from '@mui/material/FormControlLabel';
import LinearProgress from '@mui/material/LinearProgress';
import Radio from '@mui/material/Radio';
import RadioGroup from '@mui/material/RadioGroup';
import Typography from '@mui/material/Typography';
import React from 'react';

import { QuestionAnswer, Question } from '../../types';

interface QuizQuestionProps {
  currentQuestion: Question;
  currentQuestionIndex: number;
  totalQuestions: number;
  currentAnswer: QuestionAnswer | undefined;
  isLastQuestion: boolean;
  onAnswerChange: (questionId: number, selectedOptionNumber: number) => void;
  onPrevious: () => void;
  onNext: () => void;
}

const QuizQuestion: React.FC<QuizQuestionProps> = ({
  currentQuestion,
  currentQuestionIndex,
  totalQuestions,
  currentAnswer,
  isLastQuestion,
  onAnswerChange,
  onPrevious,
  onNext,
}) => {
  if (!currentQuestion) return null;

  return (
    <Box sx={{ p: { xs: 2, sm: 4 } }}>
      {/* Question position progress */}
      <LinearProgress
        variant="determinate"
        value={((currentQuestionIndex + 1) / Math.max(1, totalQuestions)) * 100}
        sx={{
          height: 6,
          borderRadius: 3,
          mb: 2,
          backgroundColor: 'var(--bg-color-tertiary)',
          '.MuiLinearProgress-bar': {
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
            onAnswerChange(currentQuestion.id, parseInt(e.target.value, 10));
          }}
        >
          {currentQuestion.options.map((option: string, index: number) => (
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
          onClick={onPrevious}
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
          onClick={onNext}
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
  );
};

export default QuizQuestion;
