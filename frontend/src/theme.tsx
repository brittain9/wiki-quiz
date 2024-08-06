import { createTheme } from '@mui/material/styles';
import getLPTheme from './getLPTheme';

const theme = createTheme(getLPTheme('light'));

export default theme;
