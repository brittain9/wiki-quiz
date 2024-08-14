// QuizOptionsComponent.tsx
import React from 'react';
import { Box, IconButton, Tooltip, Select, MenuItem, InputLabel, FormControl } from '@mui/material';
import KeyboardDoubleArrowLeftIcon from '@mui/icons-material/KeyboardDoubleArrowLeft';
import KeyboardDoubleArrowRightIcon from '@mui/icons-material/KeyboardDoubleArrowRight';
import { useGlobalQuiz } from '../../context/GlobalQuizContext';

const QuizOptionsComponent: React.FC = () => {
  const [expanded, setExpanded] = React.useState(false);
  const { quizOptions, setNumQuestions, setNumOptions, setExtractLength } = useGlobalQuiz();

  const toggleExpanded = () => {
    setExpanded(!expanded);
  };

  const varietyOptions = [
    { label: 'Low', value: 1000 },
    { label: 'Medium', value: 2000 },
    { label: 'High', value: 5000 },
    { label: 'Very High', value: 10000 },
  ];

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', height: '100%' }}>
      <Tooltip title={expanded ? "Hide options" : "Show options"}>
        <IconButton onClick={toggleExpanded} color="primary" size="small">
          {expanded ? <KeyboardDoubleArrowRightIcon /> : <KeyboardDoubleArrowLeftIcon />}
        </IconButton>
      </Tooltip>
      {expanded && (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, height: '100%' }}>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel id="questions-label">Questions</InputLabel>
            <Select
              labelId="questions-label"
              value={quizOptions.numQuestions}
              label="Questions"
              onChange={(e) => setNumQuestions(Number(e.target.value))}
            >
              {[5, 10, 15, 20].map((num) => (
                <MenuItem key={num} value={num}>{num}</MenuItem>
              ))}
            </Select>
          </FormControl>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel id="options-label">Options</InputLabel>
            <Select
              labelId="options-label"
              value={quizOptions.numOptions}
              label="Options"
              onChange={(e) => setNumOptions(Number(e.target.value))}
            >
              {[2, 3, 4, 5].map((num) => (
                <MenuItem key={num} value={num}>{num}</MenuItem>
              ))}
            </Select>
          </FormControl>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel id="variety-label">Variety</InputLabel>
            <Select
              labelId="variety-label"
              value={quizOptions.extractLength}
              label="Variety"
              onChange={(e) => setExtractLength(Number(e.target.value))}
            >
              {varietyOptions.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {option.label}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>
      )}
    </Box>
  );
};

export default QuizOptionsComponent;
