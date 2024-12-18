// components/QuizResultOverlay.tsx
import React from 'react';
import { Box, Typography, List, ListItem, Radio, RadioGroup, FormControlLabel, CircularProgress } from '@mui/material';
import { QuizResult, ResultOption } from '../types/quizResult.types';
import { Question } from '../types/quiz.types';
import { useTranslation } from 'react-i18next';


interface QuizResultOverlayProps {
  quizResult: QuizResult | null;
  isLoading: boolean;
  error: string | null;
}

const QuizResultOverlay: React.FC<QuizResultOverlayProps> = ({ quizResult, isLoading, error }) => {

  const { t } = useTranslation();

  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" height="100%">
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" height="100%">
        <Typography color="error">{error}</Typography>
      </Box>
    );
  }

  if (!quizResult) {
    return null;
  }

  const getQuestionById = (questionId: number): Question | undefined => {
    return quizResult.quiz.aiResponses
      .flatMap(response => response.questions)
      .find(question => question.id === questionId);
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom align="center">{quizResult.quiz.title} {t('quizresultoverlay.title')} </Typography>
      <Typography variant="h5" gutterBottom align="center">{t('quizresultoverlay.score')}: {quizResult.score.toFixed(0)}%</Typography>
      <List>
        {quizResult.answers.map((result: ResultOption, index: number) => {
          const question = getQuestionById(result.questionId);
          return (
            <ListItem key={result.questionId}>
              <Box>
              <Typography variant="subtitle1" gutterBottom>
                <strong>
                {t('quizresultoverlay.question')} {index + 1}:
                </strong>{' '}
                {question?.text}
              </Typography>
                <RadioGroup value={result.selectedAnswerChoice}>
                  {question?.options.map((option, optionIndex) => (
                    <FormControlLabel
                      key={optionIndex}
                      value={optionIndex + 1}
                      control={<Radio />}
                      label={option}
                      sx={{
                        color: optionIndex + 1 === result.correctAnswerChoice ? 'success.main' :
                               (optionIndex + 1 === result.selectedAnswerChoice && 
                                optionIndex + 1 !== result.correctAnswerChoice) ? 'error.main' : 'text.primary',
                        '& .MuiRadio-root': {
                          color: optionIndex + 1 === result.correctAnswerChoice ? 'success.main' :
                                 (optionIndex + 1 === result.selectedAnswerChoice && 
                                  optionIndex + 1 !== result.correctAnswerChoice) ? 'error.main' : 'inherit',
                        },
                      }}
                      disabled
                    />
                  ))}
                </RadioGroup>
              </Box>
            </ListItem>
          );
        })}
      </List>
    </Box>
  );
};

export default QuizResultOverlay;