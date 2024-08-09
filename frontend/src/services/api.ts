import axios, { AxiosError } from 'axios';
import { Quiz } from '../types/quiz.types';
import { QuizSubmission } from '../types/quizSubmission.types';
import { QuizResult } from '../types/quizResult.types';

// used in the quiz service

const API_BASE_URL = 'http://localhost:5543';

export interface BasicQuizParams {
  topic: string;
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

  submitQuiz: async (submission: QuizSubmission): Promise<QuizResult> => {
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
        // Handle other Axios-specific errors
      }
      console.error('Error submitting quiz:', error);
      throw error; // re-throw error to be handled by component
    }
  }
};
export default api;