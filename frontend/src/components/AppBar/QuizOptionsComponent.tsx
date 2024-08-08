import React from 'react';
import {
  Box,
  IconButton,
  Tooltip,
  Menu,
  MenuItem,
  TextField,
  Typography,
  Slider,
} from '@mui/material';
import SettingsIcon from '@mui/icons-material/Settings';
import { useGlobalQuiz } from '../../context/GlobalQuizContext';

const QuizOptionsMenu: React.FC = () => {
  const { quizOptions, setNumQuestions, setNumOptions, setExtractLength } = useGlobalQuiz();
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  return (
    <>
      <Tooltip title="Quiz Settings">
        <IconButton
          onClick={handleClick}
          size="large"
          edge="end"
          color="inherit"
          aria-label="quiz settings"
        >
          <SettingsIcon />
        </IconButton>
      </Tooltip>
      <Menu
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
        PaperProps={{
          elevation: 0,
          sx: {
            overflow: 'visible',
            filter: 'drop-shadow(0px 2px 8px rgba(0,0,0,0.32))',
            mt: 1.5,
            '& .MuiAvatar-root': {
              width: 32,
              height: 32,
              ml: -0.5,
              mr: 1,
            },
          },
        }}
        transformOrigin={{ horizontal: 'right', vertical: 'top' }}
        anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
      >
        <Box sx={{ p: 2, width: 300 }}>
          <Typography variant="h6" gutterBottom>
            Quiz Settings
          </Typography>
          <TextField
            label="Number of Questions"
            type="number"
            value={quizOptions.numQuestions}
            onChange={(e) => setNumQuestions(Number(e.target.value))}
            fullWidth
            margin="normal"
            InputProps={{ inputProps: { min: 1, max: 20 } }}
          />
          <TextField
            label="Options per Question"
            type="number"
            value={quizOptions.numOptions}
            onChange={(e) => setNumOptions(Number(e.target.value))}
            fullWidth
            margin="normal"
            InputProps={{ inputProps: { min: 2, max: 5 } }}
          />
          <Typography gutterBottom>
            Extract Length: {quizOptions.extractLength}
          </Typography>
          <Slider
            value={quizOptions.extractLength}
            onChange={(_, newValue) => setExtractLength(newValue as number)}
            aria-labelledby="extract-length-slider"
            valueLabelDisplay="auto"
            step={250}
            marks
            min={500}
            max={10000}
          />
        </Box>
      </Menu>
    </>
  );
};

export default QuizOptionsMenu;
