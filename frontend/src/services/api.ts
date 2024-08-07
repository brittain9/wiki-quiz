import axios from 'axios';
import { Quiz } from '../types/quiz.types';
import { QuizSubmission } from '../types/quizSubmission.types';
import { QuizResult } from '../types/quizResult.types';

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
    } catch (error) {
      console.error('Error fetching basic quiz:', error);
      throw error;
    }
  },

  submitQuiz: async (submission: QuizSubmission): Promise<QuizResult> => {
    try {
      const response = await axios.post<QuizResult>(`${API_BASE_URL}/submitquiz`, submission);
      return response.data;
    } catch (error) {
      console.error('Error submitting quiz:', error);
      throw error;
    }
  }
};

export default api;
