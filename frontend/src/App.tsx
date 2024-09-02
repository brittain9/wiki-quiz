import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import QuizPage from './pages/QuizPage';

export default function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<QuizPage />} />
      </Routes>
    </Router>
  );
}