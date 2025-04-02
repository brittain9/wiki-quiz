// components/QuizResultOverlay.tsx
import {
  Box,
  Typography,
  List,
  ListItem,
  Radio,
  RadioGroup,
  FormControlLabel,
  CircularProgress,
  Button,
  Paper,
} from '@mui/material';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { PieChart, Pie, Cell, ResponsiveContainer } from 'recharts';

import { useCustomTheme } from '../context/CustomThemeContext';
import { Question } from '../types/quiz.types';
import { QuizResult, ResultOption } from '../types/quizResult.types';

interface QuizResultOverlayProps {
  quizResult: QuizResult | null;
  isLoading: boolean;
  error: string | null;
}

const QuizResultOverlay: React.FC<QuizResultOverlayProps> = ({
  quizResult,
  isLoading,
  error,
}) => {
  const { t } = useTranslation();
  const { currentTheme } = useCustomTheme();

  if (isLoading) {
    return (
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          height: '100%',
        }}
        className={`theme-${currentTheme}`}
      >
        <CircularProgress sx={{ color: 'var(--main-color)' }} />
      </Box>
    );
  }

  if (error) {
    return (
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          height: '100%',
        }}
        className={`theme-${currentTheme}`}
      >
        <Typography sx={{ color: 'var(--error-color)' }}>{error}</Typography>
      </Box>
    );
  }

  if (!quizResult) {
    return null;
  }

  const getQuestionById = (questionId: number): Question | undefined => {
    return quizResult.quiz.aiResponses
      .flatMap((response) => response.questions)
      .find((question) => question.id === questionId);
  };

  // Prepare chart data for score visualization
  const score = quizResult.score;
  const remainingScore = 100 - score;
  const data = [
    { name: 'Score', value: score },
    { name: 'Remaining', value: remainingScore },
  ];

  // Determine color based on score
  const getScoreColor = () => {
    if (score >= 80) return 'var(--success-color)';
    if (score >= 50) return 'var(--warning-color)';
    return 'var(--error-color)';
  };

  return (
    <Box className={`theme-${currentTheme}`}>
      <Typography
        variant="h4"
        gutterBottom
        align="center"
        sx={{
          color: 'var(--main-color)',
          fontWeight: 600,
          mb: 3,
        }}
      >
        {quizResult.quiz.title} {t('quizresultoverlay.title')}
      </Typography>

      <Paper
        elevation={1}
        sx={{
          mb: 4,
          p: 3,
          backgroundColor: 'var(--bg-color-secondary)',
          borderRadius: 2,
          border: '1px solid var(--sub-alt-color)',
        }}
      >
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            flexDirection: 'column',
          }}
        >
          <Box sx={{ position: 'relative', width: 200, height: 200 }}>
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
                  <Cell key="cell-0" fill={getScoreColor()} />
                  <Cell key="cell-1" fill="var(--bg-color-tertiary)" />
                </Pie>
              </PieChart>
            </ResponsiveContainer>
            <Box
              sx={{
                position: 'absolute',
                top: '50%',
                left: '50%',
                transform: 'translate(-50%, -50%)',
                textAlign: 'center',
              }}
            >
              <Typography
                variant="h3"
                component="div"
                fontWeight="bold"
                sx={{ color: getScoreColor() }}
              >
                {score.toFixed(0)}%
              </Typography>
              <Typography
                variant="subtitle2"
                sx={{ color: 'var(--sub-color)' }}
              >
                {t('quizresultoverlay.score')}
              </Typography>
            </Box>
          </Box>
        </Box>
      </Paper>

      <Typography
        variant="h5"
        sx={{
          mb: 2,
          color: 'var(--text-color)',
          fontWeight: 600,
          borderLeft: '4px solid var(--main-color)',
          pl: 2,
        }}
      >
        Quiz Questions
      </Typography>

      <List
        sx={{
          backgroundColor: 'var(--bg-color)',
          border: '1px solid var(--sub-alt-color)',
          borderRadius: 2,
          overflow: 'hidden',
        }}
      >
        {quizResult.answers.map((result: ResultOption, index: number) => {
          const question = getQuestionById(result.questionId);
          return (
            <ListItem
              key={result.questionId}
              sx={{
                borderBottom: '1px solid var(--sub-alt-color)',
                py: 3,
                px: 3,
                flexDirection: 'column',
                alignItems: 'flex-start',
                backgroundColor:
                  index % 2 === 0
                    ? 'var(--bg-color)'
                    : 'var(--bg-color-secondary)',
              }}
            >
              <Box sx={{ width: '100%', mb: 2 }}>
                <Typography
                  variant="subtitle1"
                  gutterBottom
                  sx={{
                    color: 'var(--text-color)',
                    fontWeight: 600,
                    display: 'flex',
                    alignItems: 'center',
                    '&::before': {
                      content: '""',
                      display: 'inline-block',
                      width: 12,
                      height: 12,
                      marginRight: 1.5,
                      backgroundColor: 'var(--main-color)',
                      borderRadius: '50%',
                    },
                  }}
                >
                  {t('quizresultoverlay.question')} {index + 1}:{' '}
                  {question?.text}
                </Typography>
              </Box>

              <Box sx={{ width: '100%', pl: 2 }}>
                <RadioGroup value={result.selectedAnswerChoice}>
                  {question?.options.map((option, optionIndex) => {
                    // Determine colors based on correctness
                    const isCorrect =
                      optionIndex + 1 === result.correctAnswerChoice;
                    const isSelected =
                      optionIndex + 1 === result.selectedAnswerChoice;
                    const isWrong = isSelected && !isCorrect;

                    let textColor = 'var(--text-color)';
                    let radioColor = 'var(--sub-color)';
                    let bgColor = 'transparent';
                    let borderColor = 'transparent';

                    if (isCorrect) {
                      textColor = 'var(--success-color)';
                      radioColor = 'var(--success-color)';
                      bgColor = 'rgba(var(--success-color-rgb), 0.1)';
                      borderColor = 'var(--success-color)';
                    } else if (isWrong) {
                      textColor = 'var(--error-color)';
                      radioColor = 'var(--error-color)';
                      bgColor = 'rgba(var(--error-color-rgb), 0.1)';
                      borderColor = 'var(--error-color)';
                    }

                    return (
                      <FormControlLabel
                        key={optionIndex}
                        value={optionIndex + 1}
                        control={
                          <Radio
                            sx={{
                              color: radioColor,
                              '&.Mui-checked': {
                                color: radioColor,
                              },
                            }}
                          />
                        }
                        label={option}
                        sx={{
                          color: textColor,
                          width: '100%',
                          mb: 1,
                          p: 1.5,
                          borderRadius: 1,
                          backgroundColor: bgColor,
                          ...(isSelected || isCorrect
                            ? {
                                border: `1px solid ${borderColor}`,
                              }
                            : {}),
                        }}
                        disabled
                      />
                    );
                  })}
                </RadioGroup>
              </Box>
            </ListItem>
          );
        })}
      </List>

      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
        <Button
          variant="contained"
          onClick={() => window.location.reload()}
          sx={{
            backgroundColor: 'var(--main-color)',
            color: 'var(--bg-color)',
            '&:hover': {
              backgroundColor: 'var(--caret-color)',
            },
            py: 1.5,
            px: 4,
            fontSize: '1rem',
            fontWeight: 500,
          }}
        >
          {t('button.close')}
        </Button>
      </Box>
    </Box>
  );
};

export default QuizResultOverlay;
