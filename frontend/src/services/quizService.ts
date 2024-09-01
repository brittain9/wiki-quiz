import { useGlobalQuiz } from '../context/GlobalQuizContext';
import { BasicQuizParams } from './api';
import { Quiz } from '../types/quiz.types';
import { QuizSubmission, SubmissionResponse } from '../types/quizSubmission.types';
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

  const submitQuiz = async (submission: QuizSubmission): Promise<SubmissionResponse> => {
    try {
      return await api.postQuiz(submission);
    } catch (error) {
      throw error;
    }
  };

  return {
    generateQuiz,
    submitQuiz
  };
};

// exported separately to avoid circular dependency with global context.
export const fetchAvailableServices = async () => {
  try {
    const services = await api.getAiServices();
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