import { useGlobalQuiz } from '../context/GlobalQuizContext';
import { BasicQuizParams } from './api';
import { Quiz } from '../types/quiz.types';
import { QuizSubmission } from '../types/quizSubmission.types';
import { QuizResult } from '../types/quizResult.types';
import api from './api';

export const useQuizService = () => {
  const { 
    quizOptions, 
    setCurrentQuiz, 
    setIsGenerating, 
    setIsQuizReady,
  } = useGlobalQuiz();

  const generateQuiz = async (): Promise<Quiz> => {
    setIsGenerating(true);
    setIsQuizReady(false);

    try {
      const params: BasicQuizParams = {
        topic: quizOptions.topic,
        language: quizOptions.language,
        numQuestions: quizOptions.numQuestions,
        numOptions: quizOptions.numOptions,
        extractLength: quizOptions.extractLength,
        aiService: quizOptions.selectedService,
        model: quizOptions.selectedModel,
      };

      const quiz = await api.getBasicQuiz(params);
      setCurrentQuiz(quiz);
      setIsQuizReady(true);
      return quiz;
    } catch (error) {
      throw error;
    } finally {
      setIsGenerating(false);
    }
  };

  const submitQuiz = async (submission: QuizSubmission): Promise<QuizResult> => {
    try {
      return await api.submitQuiz(submission);
    } catch (error) {
      throw error;
    }
  };

  return {
    generateQuiz,
    submitQuiz,
  };
};

export const fetchAvailableServices = async () => {
  try {
    const services = await api.getAiServices();
    console.log('Fetched services:', services);
    return services;
  } catch (error) {
    console.error('Failed to fetch available AI services:', error);
    throw error;
  }
};

export const fetchAvailableModels = async (serviceId: number) => {
  try {
    const models = await api.getAiModels(serviceId);
    return models;
  } catch (error) {
    console.error(`Failed to fetch available models for service ${serviceId}:`, error);
    throw error;
  }
};