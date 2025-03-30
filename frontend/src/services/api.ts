import axios from 'axios';
import apiClient from './apiClient';
import { Quiz } from '../types/quiz.types';
import { QuizSubmission, SubmissionDetail, SubmissionResponse } from '../types/quizSubmission.types';
import { QuizResult } from '../types/quizResult.types';

const API_BASE_URL = 'http://localhost:5543/api';

export interface BasicQuizParams {
  topic: string;
  aiService: number | null;
  model: number | null;
  language?: string;
  numQuestions?: number;
  numOptions?: number;
  extractLength?: number;
}

const api = {

  getBasicQuiz: async (
    quizOptions: {
      topic: string;
      selectedService: number | null;
      selectedModel: number | null;
      language?: string;
      numQuestions?: number;
      numOptions?: number;
      extractLength?: number;
    },
  ): Promise<Quiz> => {
    
    try {
      const params: BasicQuizParams = {
        topic: quizOptions.topic,
        aiService: quizOptions.selectedService,
        model: quizOptions.selectedModel,
        language: quizOptions.language,
        numQuestions: quizOptions.numQuestions,
        numOptions: quizOptions.numOptions,
        extractLength: quizOptions.extractLength,
      };

      const { data: quiz } = await axios.get<Quiz>(`${API_BASE_URL}/quiz/basicquiz`, { params });

      return quiz;
    } 
    catch (error) {
      if (axios.isAxiosError(error)) {
        if (error.response?.status === 404) {
          throw new Error(`Please enter a valid Wikipedia topic.`); // this gets printed in our hero
        }
        if (error.code === 'ERR_NETWORK') {
          console.error('Network error:', error);
          throw new Error('Network error: Unable to connect to the server. Please check if the API is running.'); // also printed in hero
        }
      }      
      console.error('Error fetching basic quiz:', error);
      throw error; // re-throw error to be handled by component
    }
  },

  postQuiz: async (submission: QuizSubmission): Promise<SubmissionResponse> => {
    try {
      const response = await apiClient.post<SubmissionResponse>('/quiz/submitquiz', submission);
      return response.data;
    }
    catch (error) {
      if (axios.isAxiosError(error)) {
        if (error.code === 'ERR_NETWORK') {
          console.error('Network error:', error);
          throw new Error('Network error: Unable to connect to the server. Please check if the API is running.');
        }
      }
      console.error('Error submitting quiz:', error);
      throw error; // re-throw error to be handled by component
    }
  },

  getAiServices: async (): Promise<Record<number, string>> => {
    try {
      const response = await axios.get<Record<number, string>>(`${API_BASE_URL}/ai/getAiServices`);
      return response.data;
    } catch (error) {
      console.error('Error fetching AI services:', error);
      throw error;
    }
  },

  getAiModels: async (serviceId: number): Promise<Record<number, string>> => {
    try {
      const response = await axios.get<Record<number, string>>(`${API_BASE_URL}/ai/getModels`, {
        params: { aiServiceId: serviceId }
      });
      return response.data;
    } catch (error) {
      console.error(`Error fetching AI models for service ${serviceId}:`, error);
      throw error;
    }
  },

  getRecentSubmissions: async (): Promise<SubmissionResponse[]> => {
    try {
      const response = await apiClient.get<SubmissionResponse[]>('/submission/quizsubmission/recent');
      return response.data;
    } catch (error) {
      console.error('Error fetching recent submissions:', error);
      throw error;
    }
  },

  getSubmissionById: async (id: number): Promise<QuizResult> => {
    try {
      const response = await apiClient.get<QuizResult>(`/submission/quizsubmission/${id}`);
      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error)) {
        if (error.response?.status === 404) {
          throw new Error(`Submission with ID ${id} not found.`);
        }
      }
      console.error(`Error fetching submission with ID ${id}:`, error);
      throw error;
    }
  },

};
export default api;

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