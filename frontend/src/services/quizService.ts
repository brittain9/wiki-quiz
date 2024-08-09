import { useGlobalQuiz } from '../context/GlobalQuizContext';
import { BasicQuizParams } from './api';
import { Quiz } from '../types/quiz.types';
import { QuizSubmission } from '../types/quizSubmission.types';
import { QuizResult } from '../types/quizResult.types';
import api from './api';

export const useQuizService = () => {
  const { quizOptions, setCurrentQuiz, setIsGenerating, setIsQuizReady } = useGlobalQuiz();

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