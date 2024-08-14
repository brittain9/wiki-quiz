import * as React from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Grid from '@mui/material/Grid';
import { useTheme } from '@mui/system';
import { useTranslation } from 'react-i18next';

import microsoftLight from '../logos/MicrosoftLight.png';
import microsoftDark from '../logos/MicrosoftDark.png';

import openAILight from '../logos/openai-light.png';
import openAIDark from '../logos/openai-dark.png';

// the logos aren't sized well
const whiteLogos = [
  openAILight, microsoftLight
];

const darkLogos = [
  openAIDark, microsoftDark
];

const logoStyle = {
  width: '200px',
  height: '70px',
  margin: '0 32px',
  opacity: 1,
};

export default function LogoCollection() {
  const { t } = useTranslation();
  const theme = useTheme();
  const logos = theme.palette.mode === 'light' ? darkLogos : whiteLogos;

  return (
    <Box id="logoCollection" sx={{ py: 4 }}>
      <Typography
        component="p"
        variant="subtitle2"
        align="center"
        sx={{ color: 'text.secondary' }}
      >
        {t('logoCollection.qualityGuarantee')}
      </Typography>
      <Grid container sx={{ justifyContent: 'center', mt: 0.5, opacity: 0.6 }}>
        {logos.map((logo, index) => (
          <Grid item key={index}>
            <img
              src={logo}
              alt={`Fake company number ${index + 1}`}
              style={logoStyle}
            />
          </Grid>
        ))}
      </Grid>
    </Box>
  );
}
