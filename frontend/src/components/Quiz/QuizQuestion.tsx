import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import LinearProgress from '@mui/material/LinearProgress';
import Typography from '@mui/material/Typography';
import Fade from '@mui/material/Fade';
import Zoom from '@mui/material/Zoom';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelIcon from '@mui/icons-material/Cancel';
import React, { useState, useEffect } from 'react';

import { Question } from '../../types';

interface QuizQuestionProps {
  currentQuestion: Question;
  currentQuestionIndex: number;
  totalQuestions: number;
  onAnswerSelected: (selectedOption: number) => void;
  showResult: boolean;
  selectedOption: number | null;
  pointsEarned: number;
  correctAnswer?: number;
  correctAnswerText?: string;
}

const QuizQuestion: React.FC<QuizQuestionProps> = ({
  currentQuestion,
  currentQuestionIndex,
  totalQuestions,
  onAnswerSelected,
  showResult,
  selectedOption,
  pointsEarned,
  correctAnswer,
  correctAnswerText,
}) => {
  const [animatePoints, setAnimatePoints] = useState(false);

  useEffect(() => {
    if (showResult && pointsEarned > 0) {
      setAnimatePoints(true);
      const timer = setTimeout(() => setAnimatePoints(false), 2000);
      return () => clearTimeout(timer);
    }
  }, [showResult, pointsEarned]);

  if (!currentQuestion) return null;

  const handleOptionClick = (optionIndex: number) => {
    if (showResult) return; // Prevent clicking after answer is shown
    
    onAnswerSelected(optionIndex + 1);
  };

  const getOptionStyle = (optionIndex: number) => {
    const baseStyle = {
      mb: 2,
      p: 2,
      borderRadius: 2,
      width: '100%',
      cursor: showResult ? 'default' : 'pointer',
      transition: 'all 0.3s ease',
      border: '2px solid transparent',
      backgroundColor: 'var(--bg-color-secondary)',
      color: 'var(--text-color)',
      textAlign: 'left' as const,
      fontSize: '1rem',
      fontWeight: 500,
      '&:hover': showResult ? {} : {
        backgroundColor: 'var(--main-color-10)',
        border: '2px solid var(--main-color)',
        transform: 'translateY(-2px)',
        boxShadow: '0 4px 12px rgba(0,0,0,0.1)',
      },
    };

    if (!showResult) {
      return baseStyle;
    }

    // Show results
    const isSelected = selectedOption === optionIndex + 1;
    const isCorrect = optionIndex + 1 === correctAnswer;

    if (isCorrect) {
      return {
        ...baseStyle,
        backgroundColor: 'var(--success-color)',
        color: 'white',
        border: '2px solid var(--success-color)',
        boxShadow: '0 4px 12px rgba(76, 175, 80, 0.3)',
      };
    } else if (isSelected) {
      return {
        ...baseStyle,
        backgroundColor: 'var(--error-color)',
        color: 'white',
        border: '2px solid var(--error-color)',
        boxShadow: '0 4px 12px rgba(244, 67, 54, 0.3)',
      };
    }

    return {
      ...baseStyle,
      opacity: 0.6,
    };
  };

  const getOptionIcon = (optionIndex: number) => {
    if (!showResult) return null;
    
    const isSelected = selectedOption === optionIndex + 1;
    const isCorrect = optionIndex + 1 === correctAnswer;

    if (isCorrect) {
      return <CheckCircleIcon sx={{ ml: 1, color: 'white' }} />;
    } else if (isSelected) {
      return <CancelIcon sx={{ ml: 1, color: 'white' }} />;
    }

    return null;
  };

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

      {/* Points Animation */}
      {showResult && (
        <Fade in={true} timeout={500}>
          <Box sx={{ textAlign: 'center', mb: 3 }}>
            <Zoom in={animatePoints} timeout={800}>
              <Typography
                variant="h4"
                sx={{
                  color: pointsEarned > 0 ? 'var(--success-color)' : 'var(--error-color)',
                  fontWeight: 'bold',
                  textShadow: '0 2px 4px rgba(0,0,0,0.3)',
                }}
              >
                {pointsEarned > 0 ? `+${pointsEarned.toLocaleString()}` : '0'} Points!
              </Typography>
            </Zoom>
          </Box>
        </Fade>
      )}

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
      <Box sx={{ mb: 4 }}>
        {currentQuestion.options.map((option: string, index: number) => (
          <Button
            key={index}
            onClick={() => handleOptionClick(index)}
            sx={getOptionStyle(index)}
            disabled={showResult}
            endIcon={getOptionIcon(index)}
          >
            <Box sx={{ display: 'flex', alignItems: 'center', width: '100%' }}>
              <Typography sx={{ flexGrow: 1, textAlign: 'left' }}>
                {option}
              </Typography>
            </Box>
          </Button>
        ))}
      </Box>

      {/* Result message */}
      {showResult && (
        <Fade in={true} timeout={1000}>
          <Box sx={{ textAlign: 'center', mb: 3 }}>
            <Typography
              variant="h6"
              sx={{
                color: pointsEarned > 0 ? 'var(--success-color)' : 'var(--error-color)',
                fontWeight: 600,
              }}
            >
              {pointsEarned > 0 ? 'Correct!' : 'Incorrect!'}
            </Typography>
            {pointsEarned === 0 && correctAnswer && (
              <Typography
                variant="body1"
                sx={{ color: 'var(--sub-color)', mt: 1 }}
              >
                The correct answer was: {correctAnswerText || currentQuestion.options[correctAnswer - 1]}
              </Typography>
            )}
          </Box>
        </Fade>
      )}
    </Box>
  );
};

export default QuizQuestion;
