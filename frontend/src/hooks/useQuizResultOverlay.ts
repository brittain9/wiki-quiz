// hooks/useQuizResultOverlay.ts
import { useState } from 'react';
import { QuizResult } from '../types/quizResult.types';
import api from '../services/api';

export const useQuizResultOverlay = () => {
  const [isOverlayOpen, setIsOverlayOpen] = useState(false);
  const [quizResult, setQuizResult] = useState<QuizResult | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const openOverlay = async (submissionId: number) => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await api.getSubmissionById(submissionId);
      setQuizResult(result);
      setIsOverlayOpen(true);
    } catch (err) {
      setError('Failed to fetch quiz result');
      console.error('Error fetching quiz result:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const closeOverlay = () => {
    setIsOverlayOpen(false);
    setQuizResult(null);
  };

  return {
    isOverlayOpen,
    quizResult,
    isLoading,
    error,
    openOverlay,
    closeOverlay,
  };
};