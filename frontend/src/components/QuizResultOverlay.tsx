// components/QuizResultOverlay.tsx
import React from 'react';
import { Box, Typography, List, ListItem, Radio, RadioGroup, FormControlLabel, CircularProgress } from '@mui/material';
import { QuizResult, ResultOption } from '../types/quizResult.types';
import { Question } from '../types/quiz.types';
import { useTranslation } from 'react-i18next';
import { PieChart, Pie, Cell, ResponsiveContainer } from 'recharts';

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

  // Prepare chart data for score visualization
  const score = quizResult.score;
  const remainingScore = 100 - score;
  const data = [
    { name: 'Score', value: score },
    { name: 'Remaining', value: remainingScore }
  ];

  // Determine color based on score
  const getScoreColor = () => {
    if (score >= 80) return '#4caf50'; // success.main
    if (score >= 50) return '#ff9800'; // warning.main
    return '#f44336'; // error.main
  };

  const COLORS = [getScoreColor(), '#e0e0e0'];

  return (
    <Box>
      <Typography variant="h4" gutterBottom align="center">{quizResult.quiz.title} {t('quizresultoverlay.title')} </Typography>
      
      <Box display="flex" justifyContent="center" alignItems="center" flexDirection="column" mb={4}>
        <Box position="relative" width={200} height={200}>
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie
                data={data}
                cx="50%"
                cy="50%"
                startAngle={90}
                endAngle={-270}
                innerRadius={60}
                outerRadius={80}
                paddingAngle={0}
                dataKey="value"
                strokeWidth={0}
              >
                {data.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                ))}
              </Pie>
            </PieChart>
          </ResponsiveContainer>
          <Box
            position="absolute"
            top="50%"
            left="50%"
            sx={{
              transform: 'translate(-50%, -50%)',
              textAlign: 'center'
            }}
          >
            <Typography variant="h3" component="div" fontWeight="bold">
              {score.toFixed(0)}%
            </Typography>
            <Typography variant="subtitle2" color="text.secondary">
              {t('quizresultoverlay.score')}
            </Typography>
          </Box>
        </Box>
      </Box>
      
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