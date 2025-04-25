import React from 'react';

import { AccountOverlay, LoginOverlay, QuizResultOverlay } from './';
import { useOverlay } from '../../context';

/**
 * OverlayManager renders the appropriate overlay based on the current overlay state.
 * This centralized approach makes it easy to add new overlays in the future.
 */
const OverlayManager: React.FC = () => {
  const { currentOverlay } = useOverlay();

  return (
    <>
      <LoginOverlay />
      <AccountOverlay />
      <QuizResultOverlay />

      {/* Future overlays can be added here */}
    </>
  );
};

export default OverlayManager;
