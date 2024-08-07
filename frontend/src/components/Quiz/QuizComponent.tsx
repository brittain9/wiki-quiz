import React, { useState, useEffect } from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import CircularProgress from '@mui/material/CircularProgress';
import { quizService } from '../../services/quizService';
import { Quiz } from '../../types/quiz.types';
import { QuizResult } from '../../types/quizResult.types';
import { QuestionAnswer } from '../../types/quizSubmission.types';
import QuizInProgressComponent from './QuizInProgressComponent';
import QuizResultComponent from './QuizResultComponent';

interface QuizComponentProps {
  topic: string;
}

const QuizComponent: React.FC<QuizComponentProps> = ({ topic }) => {
  const [quiz, setQuiz] = useState<Quiz | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [quizResult, setQuizResult] = useState<QuizResult | null>(null);

  useEffect(() => {
    const loadQuiz = async () => {
      setLoading(true);
      setError(null);
      try {
        const fetchedQuiz = await quizService.fetchQuiz({ topic, numQuestions: 5 });
        setQuiz(fetchedQuiz);
      } catch (err) {
        setError(`Failed to load quiz. Error: ${err instanceof Error ? err.message : String(err)}`);
      } finally {
        setLoading(false);
      }
    };

    loadQuiz();
  }, [topic]);

  const handleQuizSubmit = async (quizId: number, questionAnswers: QuestionAnswer[]) => {
    setLoading(true);
    setError(null);
    try {
      const result = await quizService.submitQuiz(quizId, questionAnswers);
      setQuizResult(result);
    } catch (err) {
      setError(`Failed to submit quiz. Error: ${err instanceof Error ? err.message : String(err)}`);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
        <Typography color="error">{error}</Typography>
      </Box>
    );
  }

  if (!quiz) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
        <Typography>No quiz available. Please try a different topic.</Typography>
      </Box>
    );
  }

  if (quizResult) {
    return <QuizResultComponent quizResult={quizResult} />;
  }

  return <QuizInProgressComponent quiz={quiz} onSubmit={handleQuizSubmit} />;
};

export default QuizComponent;
