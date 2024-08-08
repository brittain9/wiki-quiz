import React, { createContext, useContext, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Quiz } from '../types/quiz.types';
import { QuizResult } from '../types/quizResult.types';

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
