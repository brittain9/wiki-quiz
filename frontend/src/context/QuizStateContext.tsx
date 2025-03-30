import React, { createContext, useContext, useState } from 'react';

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

export const QuizStateProvider: React.FC<React.PropsWithChildren<{}>> = ({
  children,
}) => {
  const [isGenerating, setIsGenerating] = useState(false);
  const [isQuizReady, setIsQuizReady] = useState(false);
  const [currentQuiz, setCurrentQuiz] = useState<Quiz | null>(null);
  const [currentSubmission, setCurrentSubmission] =
    useState<SubmissionResponse | null>(null);
  const [submissionHistory, setSubmissionHistory] = useState<
    SubmissionResponse[]
  >([]);

  const handleSetCurrentQuiz = (quiz: Quiz | null) => {
    setCurrentQuiz(quiz);
    setCurrentSubmission(null);
    setIsQuizReady(!!quiz);
  };

  const handleSetCurrentSubmission = (
    submissionResponse: SubmissionResponse | null,
  ) => {
    setCurrentSubmission(submissionResponse);
    setIsQuizReady(false);
  };

  const addSubmissionToHistory = (submission: SubmissionResponse) => {
    setSubmissionHistory((prevHistory) => [submission, ...prevHistory]);
  };

  return (
    <QuizStateContext.Provider
      value={{
        isGenerating,
        isQuizReady,
        currentQuiz,
        currentSubmission,
        submissionHistory,
        setIsGenerating,
        setIsQuizReady,
        setCurrentQuiz,
        setCurrentSubmission,
        addSubmissionToHistory,
      }}
    >
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
