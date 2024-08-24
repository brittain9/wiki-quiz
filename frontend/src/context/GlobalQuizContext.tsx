import React, { createContext, useContext, useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Quiz } from '../types/quiz.types';
import { QuizResult } from '../types/quizResult.types';
import { fetchAvailableServices, fetchAvailableModels } from '../services/quizService';

export interface QuizOptions {
  topic: string;
  numQuestions: number;
  numOptions: number;
  extractLength: number;
  language: string;
  isGenerating: boolean;
  isQuizReady: boolean;
  currentQuiz: Quiz | null;
  currentQuizResult: QuizResult | null;

  // the numbers will correspond with dictionary received from the endpoint
  selectedService: number | null;
  selectedModel: number | null;
  availableServices: Record<number, string>;
  availableModels: Record<number, string>;
}

interface GlobalQuizContextType {
  quizOptions: QuizOptions;
  setTopic: (topic: string) => void;
  setNumQuestions: (numQuestions: number) => void;
  setNumOptions: (numOptions: number) => void;
  setExtractLength: (extractLength: number) => void;
  setLanguage: (language: string) => void;
  setIsGenerating: (isGenerating: boolean) => void;
  setIsQuizReady: (isQuizReady: boolean) => void;
  setCurrentQuiz: (quiz: Quiz | null) => void;
  setCurrentQuizResult: (quizResult: QuizResult | null) => void;

  setSelectedService: (serviceId: number | null) => void;
  setSelectedModel: (modelId: number | null) => void;
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
    isGenerating: false,
    isQuizReady: false,
    currentQuiz: null,
    currentQuizResult: null,

    selectedService: null,
    selectedModel: null,
    availableServices: {},
    availableModels: {},
  });

  const debugStateChange = <K extends keyof QuizOptions>(
    key: K,
    oldValue: QuizOptions[K],
    newValue: QuizOptions[K]
  ) => {
    console.log(`Updating ${key}:`);
    console.log(`  Old value: ${oldValue}`);
    console.log(`  New value: ${newValue}`);
  };

  const setTopic = (topic: string) => {
    setQuizOptions(prev => {
      debugStateChange('topic', prev.topic, topic);
      return { ...prev, topic };
    });
  };

  const setNumQuestions = (numQuestions: number) => {
    setQuizOptions(prev => {
      debugStateChange('numQuestions', prev.numQuestions, numQuestions);
      return { ...prev, numQuestions };
    });
  };

  const setNumOptions = (numOptions: number) => {
    setQuizOptions(prev => {
      debugStateChange('numOptions', prev.numOptions, numOptions);
      return { ...prev, numOptions };
    });
  };

  const setExtractLength = (extractLength: number) => {
    setQuizOptions(prev => {
      debugStateChange('extractLength', prev.extractLength, extractLength);
      return { ...prev, extractLength };
    });
  };

  const setLanguage = (language: string) => {
    setQuizOptions(prev => {
      debugStateChange('language', prev.language, language);
      const newState = { ...prev, language };
      i18n.changeLanguage(language);
      return newState;
    });
  };

  const setIsGenerating = (isGenerating: boolean) => {
    setQuizOptions(prev => {
      debugStateChange('isGenerating', prev.isGenerating, isGenerating);
      return { ...prev, isGenerating };
    });
  };

  const setIsQuizReady = (isQuizReady: boolean) => {
    setQuizOptions(prev => {
      debugStateChange('isQuizReady', prev.isQuizReady, isQuizReady);
      return { ...prev, isQuizReady };
    });
  };
  
  const setCurrentQuiz = (quiz: Quiz | null) => {
    setQuizOptions(prev => {
      debugStateChange('currentQuiz', prev.currentQuiz, quiz);
      debugStateChange('currentQuizResult', prev.currentQuizResult, null);
      debugStateChange('isQuizReady', prev.isQuizReady, !!quiz);
      return { 
        ...prev, 
        currentQuiz: quiz,
        currentQuizResult: null,
        isQuizReady: !!quiz
      };
    });
  };

  const setCurrentQuizResult = (quizResult: QuizResult | null) => {
    setQuizOptions(prev => {
      debugStateChange('currentQuizResult', prev.currentQuizResult, quizResult);
      debugStateChange('isQuizReady', prev.isQuizReady, false);
      return { 
        ...prev, 
        currentQuizResult: quizResult,
        isQuizReady: false
      };
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

  useEffect(() => {
    const loadServices = async () => {
      try {
        const services = await fetchAvailableServices();
        console.log('Services loaded:', services);
        
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
      setIsGenerating,
      setIsQuizReady,
      setCurrentQuiz,
      setCurrentQuizResult,
      setSelectedModel,
      setSelectedService
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
