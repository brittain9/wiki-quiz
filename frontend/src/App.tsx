import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import QuizPage from './pages/QuizPage';
import SubmissionDetailPage from './pages/SubmissionDetailPage';

export default function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<QuizPage />} />
        <Route path="/submission/:id" element={<SubmissionDetailPage />} />

      </Routes>
    </Router>
  );
}