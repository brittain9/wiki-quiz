import { createTheme } from '@mui/material/styles';
import getTheme from './getTheme';

const theme = createTheme(getTheme('light'));

export default theme;
