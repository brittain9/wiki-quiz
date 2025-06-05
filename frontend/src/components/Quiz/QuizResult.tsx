import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import Typography from '@mui/material/Typography';
import React, { useCallback } from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer } from 'recharts';
import EmojiEventsIcon from '@mui/icons-material/EmojiEvents';

import { useOverlay } from '../../context';
import { Quiz, SubmissionResponse } from '../../types';

interface QuizResultProps {
  currentQuiz: Quiz;
  currentSubmission: SubmissionResponse | null;
  score: number | null;
  totalQuestions: number;
}

const QuizResult: React.FC<QuizResultProps> = ({
  currentQuiz,
  currentSubmission,
  score,
  totalQuestions,
}) => {
  const { showOverlay } = useOverlay();

  // Handlers
  const handleViewDetailedSubmission = useCallback(() => {
    if (currentSubmission) {
      showOverlay('quiz_result', { resultId: currentSubmission.id });
    } else {
      console.error('Cannot show quiz result: submission is null');
    }
  }, [currentSubmission, showOverlay]);

  // Add score chart data calculation
  const getScoreChartData = useCallback(() => {
    if (score === null) return [];

    return [
      { name: 'Score', value: score },
      { name: 'Remaining', value: 100 - score },
    ];
  }, [score]);

  // Get score color based on percentage
  const getScoreColor = useCallback(() => {
    if (score === null) return 'var(--success-color)';
    if (score >= 80) return 'var(--success-color)';
    if (score >= 50) return 'var(--warning-color)';
    return 'var(--error-color)';
  }, [score]);

  return (
    <Box sx={{ p: 4 }}>
      <Typography
        variant="h5"
        gutterBottom
        sx={{
          color: 'var(--text-color)',
          fontWeight: 600,
          textAlign: 'center',
          mb: 2,
        }}
      >
        Quiz Complete!
      </Typography>

      {/* Points Earned Display */}
      {currentSubmission && currentSubmission.pointsEarned > 0 && (
        <Box sx={{ display: 'flex', justifyContent: 'center', mb: 3 }}>
          <Chip
            icon={<EmojiEventsIcon />}
            label={`+${currentSubmission.pointsEarned.toLocaleString()} points earned!`}
            sx={{
              bgcolor: 'var(--success-color)',
              color: 'white',
              fontWeight: 'bold',
              fontSize: '1rem',
              padding: '8px 16px',
              '& .MuiChip-icon': {
                color: 'white',
              },
            }}
          />
        </Box>
      )}

      <Box
        sx={{
          display: 'flex',
          flexDirection: { xs: 'column', sm: 'row' },
          justifyContent: 'center',
          alignItems: 'center',
          mb: 4,
          gap: { xs: 3, sm: 6 },
        }}
      >
        {/* Score chart */}
        <Box sx={{ position: 'relative', width: 160, height: 160 }}>
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie
                data={getScoreChartData()}
                cx="50%"
                cy="50%"
                startAngle={90}
                endAngle={-270}
                innerRadius={45}
                outerRadius={60}
                paddingAngle={0}
                dataKey="value"
                strokeWidth={0}
              >
                <Cell key="score-cell-0" fill={getScoreColor()} />
                <Cell key="score-cell-1" fill="var(--bg-color-tertiary)" />
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
              variant="h4"
              component="div"
              fontWeight="bold"
              sx={{ color: getScoreColor() }}
            >
              {score !== null ? `${score}%` : '--'}
            </Typography>
            <Typography variant="body2" sx={{ color: 'var(--sub-color)' }}>
              Score
            </Typography>
          </Box>
        </Box>

        {/* Quiz stats */}
        <Box>
          <Typography
            sx={{
              mb: 1,
              color: 'var(--text-color)',
              display: 'flex',
              alignItems: 'center',
              gap: 1,
            }}
          >
            <span
              style={{
                display: 'inline-block',
                width: 12,
                height: 12,
                backgroundColor: 'var(--main-color)',
                borderRadius: '50%',
              }}
            />
            Questions: {totalQuestions}
          </Typography>
          <Typography
            sx={{
              mb: 1,
              color: 'var(--text-color)',
              display: 'flex',
              alignItems: 'center',
              gap: 1,
            }}
          >
            <span
              style={{
                display: 'inline-block',
                width: 12,
                height: 12,
                backgroundColor: 'var(--success-color)',
                borderRadius: '50%',
              }}
            />
            Topic: {currentQuiz.aiResponses[0]?.responseTopic || 'General'}
          </Typography>
          <Typography
            sx={{
              color: 'var(--text-color)',
              display: 'flex',
              alignItems: 'center',
              gap: 1,
            }}
          >
            <span
              style={{
                display: 'inline-block',
                width: 12,
                height: 12,
                backgroundColor: 'var(--warning-color)',
                borderRadius: '50%',
              }}
            />
            Quiz Title: {currentQuiz.title}
          </Typography>
        </Box>
      </Box>

      <Box
        sx={{
          display: 'flex',
          justifyContent: 'center',
          mt: { xs: 2, sm: 4 },
        }}
      >
        <Button
          variant="contained"
          onClick={handleViewDetailedSubmission}
          sx={{
            backgroundColor: 'var(--main-color)',
            color: 'var(--bg-color)',
            padding: '10px 20px',
            fontSize: '1rem',
            fontWeight: 500,
            textTransform: 'none',
            '&:hover': {
              backgroundColor: 'var(--caret-color)',
            },
          }}
        >
          View Detailed Results
        </Button>
      </Box>
    </Box>
  );
};

export default QuizResult;
