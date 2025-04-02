import React from 'react';

import { useOverlay } from '../../context/OverlayContext';
import AccountOverlay from './AccountOverlay';
import LoginOverlay from './LoginOverlay';

/**
 * OverlayManager renders the appropriate overlay based on the current overlay state.
 * This centralized approach makes it easy to add new overlays in the future.
 */
const OverlayManager: React.FC = () => {
  const { currentOverlay } = useOverlay();

  return (
    <>
      {/* Login overlay */}
      <LoginOverlay />
      
      {/* Account overlay */}
      <AccountOverlay />
      
      {/* Add more overlays here as needed */}
      {/* Example: currentOverlay === 'quiz' && <QuizOverlay /> */}
      {/* Example: currentOverlay === 'quizResult' && <QuizResultOverlay /> */}
    </>
  );
};

export default OverlayManager; 