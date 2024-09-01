import React, { createContext, useContext, useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Quiz } from '../types/quiz.types';
import { SubmissionResponse } from '../types/quizSubmission.types';
import { fetchAvailableServices, fetchAvailableModels } from '../services/quizService';

export interface QuizOptions {
  // Quiz Options
  topic: string;
  numQuestions: number;
  numOptions: number;
  extractLength: number;
  language: string;
  // AI options
  selectedService: number | null;
  selectedModel: number | null;
  availableServices: Record<number, string>;
  availableModels: Record<number, string>;

  // Loading and state
  isGenerating: boolean;
  isQuizReady: boolean;
  currentQuiz: Quiz | null;
  currentSubmission: SubmissionResponse | null;
}

interface GlobalQuizContextType {
  quizOptions: QuizOptions;

  setTopic: (topic: string) => void;
  setNumQuestions: (numQuestions: number) => void;
  setNumOptions: (numOptions: number) => void;
  setExtractLength: (extractLength: number) => void;
  setLanguage: (language: string) => void;

  setSelectedService: (serviceId: number | null) => void;
  setSelectedModel: (modelId: number | null) => void;

  setIsGenerating: (isGenerating: boolean) => void;
  setIsQuizReady: (isQuizReady: boolean) => void;
  setCurrentQuiz: (quiz: Quiz | null) => void;
  setCurrentSubmission: (submissionResponse: SubmissionResponse | null) => void;

}

const GlobalQuizContext = createContext<GlobalQuizContextType | undefined>(undefined);

export const GlobalQuizProvider: React.FC<React.PropsWithChildren<{}>> = ({ children }) => {
  const { i18n } = useTranslation();
  
  const [quizOptions, setQuizOptions] = useState<QuizOptions>({
    topic: '',
    numQuestions: 5,
    numOptions: 4,
    extractLength: 1000,
    language: i18n.language,

    selectedService: null,
    selectedModel: null,
    availableServices: {},
    availableModels: {},

    isGenerating: false,
    isQuizReady: false,
    currentQuiz: null,
    currentSubmission: null,
  });

  const setTopic = (topic: string) => {
    setQuizOptions(prev => ({ ...prev, topic }));
  };
  
  const setNumQuestions = (numQuestions: number) => {
    setQuizOptions(prev => ({ ...prev, numQuestions }));
  };
  
  const setNumOptions = (numOptions: number) => {
    setQuizOptions(prev => ({ ...prev, numOptions }));
  };
  
  const setExtractLength = (extractLength: number) => {
    setQuizOptions(prev => ({ ...prev, extractLength }));
  };

  const setLanguage = (language: string) => {
    setQuizOptions(prev => {
      const newState = { ...prev, language };
      i18n.changeLanguage(language);
      return newState;
    });
  };

  const setSelectedService = async (serviceId: number | null) => {
    setQuizOptions(prev => ({
      ...prev,
      selectedService: serviceId,
      selectedModel: null,
      availableModels: {},
    }));
    if (serviceId !== null) {
      try {
        const models = await fetchAvailableModels(serviceId);
        setQuizOptions(prev => ({
          ...prev,
          availableModels: models,
        }));
      } catch (error) {
        console.error(`Error fetching models for service ${serviceId}:`, error);
      }
    }
  };
  
  const setSelectedModel = (modelId: number | null) => {
    setQuizOptions(prev => ({
      ...prev,
      selectedModel: modelId,
    }));
  };

  const setIsGenerating = (isGenerating: boolean) => {
    setQuizOptions(prev => ({ ...prev, isGenerating }));
  };
  
  const setIsQuizReady = (isQuizReady: boolean) => {
    setQuizOptions(prev => ({ ...prev, isQuizReady }));
  };
  
  const setCurrentQuiz = (quiz: Quiz | null) => {
    setQuizOptions(prev => ({ 
      ...prev, 
      currentQuiz: quiz,
      currentSubmission: null,
      isQuizReady: !!quiz
    }));
  };
  
  const setCurrentSubmission = (submissionResponse: SubmissionResponse | null) => {
    setQuizOptions(prev => ({ 
      ...prev, 
      currentSubmission: submissionResponse,
      isQuizReady: false
    }));
  };

  useEffect(() => {
    const loadServices = async () => {
      try {
        const services = await fetchAvailableServices();        
        // Get the first service ID
        const firstServiceId = Object.keys(services)[0] ? parseInt(Object.keys(services)[0]) : null;
        
        setQuizOptions(prev => ({
          ...prev,
          availableServices: services,
          selectedService: firstServiceId,
        }));

        // If we have a service, fetch and set the first model
        if (firstServiceId !== null) {
          const models = await fetchAvailableModels(firstServiceId);
          const firstModelId = Object.keys(models)[0] ? parseInt(Object.keys(models)[0]) : null;
          
          setQuizOptions(prev => ({
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
    <GlobalQuizContext.Provider value={{
      quizOptions,
      setTopic,
      setNumQuestions,
      setNumOptions,
      setExtractLength,
      setLanguage,
      setSelectedModel,
      setSelectedService,
      setIsGenerating,
      setIsQuizReady,
      setCurrentQuiz,
      setCurrentSubmission
    }}>
      {children}
    </GlobalQuizContext.Provider>
  );
};

export const useGlobalQuiz = () => {
  const context = useContext(GlobalQuizContext);
  if (context === undefined) {
    throw new Error('useGlobalQuiz must be used within a GlobalQuizProvider');
  }
  return context;
};
