import KeyboardDoubleArrowLeftIcon from '@mui/icons-material/KeyboardDoubleArrowLeft';
import KeyboardDoubleArrowRightIcon from '@mui/icons-material/KeyboardDoubleArrowRight';
import {
  Box,
  IconButton,
  Tooltip,
  Select,
  MenuItem,
  InputLabel,
  FormControl,
  SelectChangeEvent,
} from '@mui/material';
import React, { useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';

import { useQuizOptions } from '../../context/QuizOptionsContext/QuizOptionsContext';

const QuizOptionsComponent: React.FC = () => {
  const { t } = useTranslation();
  const [expanded, setExpanded] = React.useState(false);

  const {
    quizOptions,
    setNumQuestions,
    setNumOptions,
    setExtractLength,
    setSelectedService,
    setSelectedModel,
  } = useQuizOptions();

  useEffect(() => {
    if (
      quizOptions.selectedService !== null &&
      quizOptions.selectedModel === null
    ) {
      // If a service is selected but no model, select the first available model
      const firstModelId = Object.keys(quizOptions.availableModels)[0];
      if (firstModelId) {
        setSelectedModel(firstModelId);
      }
    }
  }, [
    quizOptions.selectedService,
    quizOptions.availableModels,
    quizOptions.selectedModel,
    setSelectedModel,
  ]);

  const handleServiceChange = useCallback(
    (event: SelectChangeEvent) => {
      const serviceId = event.target.value;
      setSelectedService(serviceId);
    },
    [setSelectedService],
  );

  const handleModelChange = useCallback(
    (event: SelectChangeEvent) => {
      const modelId = event.target.value;
      setSelectedModel(modelId);
    },
    [setSelectedModel],
  );

  const toggleExpanded = useCallback(() => {
    setExpanded((prev) => !prev);
  }, []);

  const varietyOptions = [
    { label: t('quizOptions.varietyLevels.low'), value: 1000 },
    { label: t('quizOptions.varietyLevels.medium'), value: 2000 },
    { label: t('quizOptions.varietyLevels.high'), value: 5000 },
    { label: t('quizOptions.varietyLevels.veryHigh'), value: 10000 },
  ];

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', height: '100%' }}>
      <Tooltip
        title={
          expanded ? t('quizOptions.hideOptions') : t('quizOptions.showOptions')
        }
      >
        <IconButton onClick={toggleExpanded} color="primary" size="small">
          {expanded ? (
            <KeyboardDoubleArrowRightIcon />
          ) : (
            <KeyboardDoubleArrowLeftIcon />
          )}
        </IconButton>
      </Tooltip>
      {expanded && (
        <Box
          sx={{ display: 'flex', alignItems: 'center', gap: 1, height: '100%' }}
        >
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel id="questions-label">
              {t('quizOptions.questions')}
            </InputLabel>
            <Select
              labelId="questions-label"
              value={quizOptions.numQuestions}
              label={t('quizOptions.questions')}
              onChange={(e) => setNumQuestions(Number(e.target.value))}
            >
              {[5, 10, 15, 20].map((num) => (
                <MenuItem key={num} value={num}>
                  {num}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel id="options-label">
              {t('quizOptions.options')}
            </InputLabel>
            <Select
              labelId="options-label"
              value={quizOptions.numOptions}
              label={t('quizOptions.options')}
              onChange={(e) => setNumOptions(Number(e.target.value))}
            >
              {[2, 3, 4, 5].map((num) => (
                <MenuItem key={num} value={num}>
                  {num}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel id="variety-label">
              {t('quizOptions.variety')}
            </InputLabel>
            <Select
              labelId="variety-label"
              value={quizOptions.extractLength || 10000}
              label={t('quizOptions.variety')}
              onChange={(e) => setExtractLength(Number(e.target.value))}
            >
              {varietyOptions.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {option.label}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel id="service-label">
              {t('quizOptions.aiService')}
            </InputLabel>
            <Select
              labelId="service-label"
              value={quizOptions.selectedService?.toString() ?? ''}
              label={t('quizOptions.aiService')}
              onChange={handleServiceChange}
            >
              {quizOptions.availableServices.length > 0 ? (
                quizOptions.availableServices.map((service) => (
                  <MenuItem key={service} value={service}>
                    {service}
                  </MenuItem>
                ))
              ) : (
                <MenuItem value="" disabled>
                  {t('quizOptions.noServicesAvailable')}
                </MenuItem>
              )}
            </Select>
          </FormControl>

          {quizOptions.selectedService !== null && (
            <FormControl size="small" sx={{ minWidth: 120 }}>
              <InputLabel id="model-label">
                {t('quizOptions.aiModel')}
              </InputLabel>
              <Select
                labelId="model-label"
                value={quizOptions.selectedModel?.toString() ?? ''}
                label={t('quizOptions.aiModel')}
                onChange={handleModelChange}
              >
                {quizOptions.availableModels.map((model) => (
                  <MenuItem key={model} value={model}>
                    {model}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          )}
        </Box>
      )}
    </Box>
  );
};

QuizOptionsComponent.displayName = 'QuizOptionsComponent';

export default QuizOptionsComponent;
