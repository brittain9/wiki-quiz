import axios from 'axios';
import { Quiz } from '../types/quiz.types';
import { QuizSubmission } from '../types/quizSubmission.types';
import { QuizResult } from '../types/quizResult.types';

const API_BASE_URL = 'http://localhost:5543';

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

  getBasicQuiz: async (params: BasicQuizParams): Promise<Quiz> => {
    try {
      const response = await axios.get<Quiz>(`${API_BASE_URL}/basicquiz`, { params });
      return response.data;
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

  postQuiz: async (submission: QuizSubmission): Promise<QuizResult> => {
    try {
      const response = await axios.post<QuizResult>(`${API_BASE_URL}/submitquiz`, submission);
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
      const response = await axios.get<Record<number, string>>(`${API_BASE_URL}/getAiServices`);
      return response.data;
    } catch (error) {
      console.error('Error fetching AI services:', error);
      throw error;
    }
  },

  getAiModels: async (serviceId: number): Promise<Record<number, string>> => {
    try {
      const response = await axios.get<Record<number, string>>(`${API_BASE_URL}/getModels`, {
        params: { aiServiceId: serviceId }
      });
      return response.data;
    } catch (error) {
      console.error(`Error fetching AI models for service ${serviceId}:`, error);
      throw error;
    }
  }

};
export default api;