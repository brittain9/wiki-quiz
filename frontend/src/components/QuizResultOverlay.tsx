// components/QuizResultOverlay.tsx
import React from 'react';
import { Box, Typography, List, ListItem, ListItemText, CircularProgress } from '@mui/material';
import { QuizResult, ResultOption } from '../types/quizResult.types';
import { Question } from '../types/quiz.types';

interface QuizResultOverlayProps {
  quizResult: QuizResult | null;
  isLoading: boolean;
  error: string | null;
}

const QuizResultOverlay: React.FC<QuizResultOverlayProps> = ({ quizResult, isLoading, error }) => {
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
      <Typography variant="h5" gutterBottom>Quiz Results</Typography>
      <Typography variant="h6" gutterBottom>Score: {quizResult.score.toFixed(2)}%</Typography>
      <List>
        {quizResult.answers.map((result: ResultOption, index: number) => {
          const question = getQuestionById(result.questionId);
          return (
            <ListItem key={result.questionId}>
              <ListItemText
                primary={`Question ${index + 1}: ${question?.text}`}
                secondary={
                  <>
                    <Typography component="span" variant="body2" color="text.primary">
                      Correct Answer: {question?.options[result.correctAnswerChoice - 1]}
                    </Typography>
                    <br />
                    <Typography component="span" variant="body2" color={result.correctAnswerChoice === result.selectedAnswerChoice ? "success.main" : "error.main"}>
                      Your Answer: {question?.options[result.selectedAnswerChoice - 1]}
                    </Typography>
                  </>
                }
              />
            </ListItem>
          );
        })}
      </List>
    </Box>
  );
};

export default QuizResultOverlay;