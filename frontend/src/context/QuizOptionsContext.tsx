import React, { createContext, useContext, useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';

import { fetchAvailableServices, fetchAvailableModels } from '../services/api';

interface QuizOptions {
  topic: string;
  numQuestions: number;
  numOptions: number;
  extractLength: number;
  language: string;
  selectedService: number | null;
  selectedModel: number | null;
  availableServices: Record<number, string>;
  availableModels: Record<number, string>;
}

interface QuizOptionsContextType {
  quizOptions: QuizOptions;
  setTopic: (topic: string) => void;
  setNumQuestions: (numQuestions: number) => void;
  setNumOptions: (numOptions: number) => void;
  setExtractLength: (extractLength: number) => void;
  setLanguage: (language: string) => void;
  setSelectedService: (serviceId: number | null) => void;
  setSelectedModel: (modelId: number | null) => void;
}

const QuizOptionsContext = createContext<QuizOptionsContextType | undefined>(
  undefined,
);

export const QuizOptionsProvider: React.FC<React.PropsWithChildren<{}>> = ({
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
    availableServices: {},
    availableModels: {},
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

  const setSelectedService = async (serviceId: number | null) => {
    setQuizOptions((prev) => ({
      ...prev,
      selectedService: serviceId,
      selectedModel: null,
      availableModels: {},
    }));
    if (serviceId !== null) {
      try {
        const models = await fetchAvailableModels(serviceId);
        setQuizOptions((prev) => ({
          ...prev,
          availableModels: models,
        }));
      } catch (error) {
        console.error(`Error fetching models for service ${serviceId}:`, error);
      }
    }
  };

  const setSelectedModel = (modelId: number | null) => {
    setQuizOptions((prev) => ({
      ...prev,
      selectedModel: modelId,
    }));
  };

  useEffect(() => {
    const loadServices = async () => {
      try {
        const services = await fetchAvailableServices();
        const firstServiceId = Object.keys(services)[0]
          ? parseInt(Object.keys(services)[0])
          : null;

        setQuizOptions((prev) => ({
          ...prev,
          availableServices: services,
          selectedService: firstServiceId,
        }));

        if (firstServiceId !== null) {
          const models = await fetchAvailableModels(firstServiceId);
          const firstModelId = Object.keys(models)[0]
            ? parseInt(Object.keys(models)[0])
            : null;

          setQuizOptions((prev) => ({
            ...prev,
            availableModels: models,
            selectedModel: firstModelId,
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
  if (context === undefined) {
    throw new Error('useQuizOptions must be used within a QuizOptionsProvider');
  }
  return context;
};
