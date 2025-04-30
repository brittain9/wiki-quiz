import React, { createContext, useContext, useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';

import {
  QuizOptions,
  QuizOptionsContextType,
} from './QuizOptionsContext.types';
import { aiApi } from '../../services';

const QuizOptionsContext = createContext<QuizOptionsContextType | null>(null);

export const QuizOptionsProvider: React.FC<React.PropsWithChildren> = ({
  children,
}) => {
  const { i18n } = useTranslation();

  const [quizOptions, setQuizOptions] = useState<QuizOptions>({
    topic: '',
    numQuestions: 5,
    numOptions: 4,
    extractLength: 5000,
    language: i18n.language,
    selectedService: null,
    selectedModel: null,
    availableServices: [],
    availableModels: [],
  });

  const setTopic = (topic: string) =>
    setQuizOptions((prev) => ({ ...prev, topic }));
  const setNumQuestions = (numQuestions: number) =>
    setQuizOptions((prev) => ({ ...prev, numQuestions }));
  const setNumOptions = (numOptions: number) =>
    setQuizOptions((prev) => ({ ...prev, numOptions }));
  const setExtractLength = (extractLength: number) =>
    setQuizOptions((prev) => ({ ...prev, extractLength }));
  const setLanguage = (language: string) => {
    setQuizOptions((prev) => ({ ...prev, language }));
    i18n.changeLanguage(language);
  };

  const setSelectedService = async (serviceId: string | null) => {
    setQuizOptions((prev) => ({
      ...prev,
      selectedService: serviceId,
      selectedModel: null,
      availableModels: [],
    }));
    if (serviceId !== null) {
      try {
        const modelsArr = await aiApi.getAiModels(serviceId);
        const firstModel = modelsArr.length > 0 ? modelsArr[0] : null;
        setQuizOptions((prev) => ({
          ...prev,
          availableModels: modelsArr,
          selectedModel: firstModel,
        }));
      } catch (error) {
        console.error(`Error fetching models for service ${serviceId}:`, error);
      }
    }
  };

  const setSelectedModel = (modelId: string | null) => {
    setQuizOptions((prev) => ({
      ...prev,
      selectedModel: modelId,
    }));
  };

  useEffect(() => {
    const loadServices = async () => {
      try {
        const servicesArr = await aiApi.getAiServices();
        const firstServiceId = servicesArr.length > 0 ? servicesArr[0] : null;
        setQuizOptions((prev) => ({
          ...prev,
          availableServices: servicesArr,
          selectedService: firstServiceId,
        }));
        
        if (firstServiceId) {
          const modelsArr = await aiApi.getAiModels(firstServiceId);
          const firstModel = modelsArr.length > 0 ? modelsArr[0] : null;
          setQuizOptions((prev) => ({
            ...prev,
            availableModels: modelsArr,
            selectedModel: firstModel,
          }));
        }
      } catch (error) {
        console.error('Error fetching available services:', error);
      }
    };
    loadServices();
  }, []);

  return (
    <QuizOptionsContext.Provider
      value={{
        quizOptions,
        setTopic,
        setNumQuestions,
        setNumOptions,
        setExtractLength,
        setLanguage,
        setSelectedService,
        setSelectedModel,
      }}
    >
      {children}
    </QuizOptionsContext.Provider>
  );
};

export const useQuizOptions = () => {
  const context = useContext(QuizOptionsContext);
  if (!context) {
    throw new Error('useQuizOptions must be used within a QuizOptionsProvider');
  }
  return context;
};
