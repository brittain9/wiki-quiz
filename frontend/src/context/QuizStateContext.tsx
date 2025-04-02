import React, {
  createContext,
  useContext,
  useState,
  useCallback,
  useMemo,
} from 'react';

import { Quiz } from '../types/quiz.types';
import { SubmissionResponse } from '../types/quizSubmission.types';

interface QuizStateContextType {
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

const QuizStateContext = createContext<QuizStateContextType | undefined>(
  undefined,
);

export const QuizStateProvider: React.FC<React.PropsWithChildren> = ({
  children,
}) => {
  const [isQuizReady, setIsQuizReady] = useState(false);
  const [isGenerating, setIsGenerating] = useState(false);
  const [currentQuiz, setCurrentQuiz] = useState<Quiz | null>(null);
  const [currentSubmission, setCurrentSubmission] =
    useState<SubmissionResponse | null>(null);
  const [submissionHistory, setSubmissionHistory] = useState<
    SubmissionResponse[]
  >([]);

  const setCurrentQuizHandler = useCallback((quiz: Quiz | null) => {
    setCurrentQuiz(quiz);
  }, []);

  const setCurrentSubmissionHandler = useCallback(
    (submission: SubmissionResponse | null) => {
      setCurrentSubmission(submission);
    },
    [],
  );

  const addSubmissionToHistory = useCallback(
    (submission: SubmissionResponse) => {
      setSubmissionHistory((prev) => {
        if (prev.find((s) => s.id === submission.id)) {
          return prev;
        }
        return [submission, ...prev];
      });
    },
    [],
  );

  const value = useMemo(
    () => ({
      isQuizReady,
      setIsQuizReady,
      isGenerating,
      setIsGenerating,
      currentQuiz,
      setCurrentQuiz: setCurrentQuizHandler,
      currentSubmission,
      setCurrentSubmission: setCurrentSubmissionHandler,
      submissionHistory,
      addSubmissionToHistory,
    }),
    [
      isQuizReady,
      isGenerating,
      currentQuiz,
      currentSubmission,
      submissionHistory,
      setCurrentQuizHandler,
      setCurrentSubmissionHandler,
      addSubmissionToHistory,
    ],
  );

  return (
    <QuizStateContext.Provider value={value}>
      {children}
    </QuizStateContext.Provider>
  );
};

export const useQuizState = () => {
  const context = useContext(QuizStateContext);
  if (context === undefined) {
    throw new Error('useQuizState must be used within a QuizStateProvider');
  }
  return context;
};
