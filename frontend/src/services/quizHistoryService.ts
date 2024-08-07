import { QuizResult, QuizHistory } from '../types/quiz.types';

export const quizHistoryService = {
  addToQuizHistory(result: QuizResult): void {
    const history = this.getQuizHistory();
    history.quizResults.push(result);
    localStorage.setItem('quizHistory', JSON.stringify(history));
  },

  getQuizHistory(): QuizHistory {
    const historyString = localStorage.getItem('quizHistory');
    return historyString ? JSON.parse(historyString) : { quizResults: [] };
  }
};