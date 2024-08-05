import { createTheme } from '@mui/material/styles';
import getLPTheme from './getLPTheme';

// A custom theme for this app
const theme = createTheme(getLPTheme('light')); // or 'dark' for dark mode

export default theme;
