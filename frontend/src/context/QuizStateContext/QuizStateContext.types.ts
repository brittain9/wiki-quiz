import { Quiz, SubmissionResponse } from '../../types';

export interface QuizStateContextType {
  isGenerating: boolean;
  isQuizReady: boolean;

  currentQuiz: Quiz | null;
  currentSubmission: SubmissionResponse | null;
  submissionHistory: SubmissionResponse[];

  setIsGenerating: (isGenerating: boolean) => void;
  setIsQuizReady: (isQuizReady: boolean) => void;
  setCurrentQuiz: (quiz: Quiz | null) => void;
  setCurrentSubmission: (submissionResponse: SubmissionResponse | null) => void;
  addSubmissionToHistory: (submission: SubmissionResponse) => void;
}
