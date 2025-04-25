import React, { createContext, useContext, useState } from 'react';

import {
  OverlayContextType,
  OverlayData,
  OverlayType,
} from './OverlayContext.types';

const OverlayContext = createContext<OverlayContextType | null>(null);

export const OverlayProvider: React.FC<React.PropsWithChildren> = ({
  children,
}) => {
  const [currentOverlay, setCurrentOverlay] = useState<OverlayType>(null);
  const [overlayData, setOverlayData] = useState<OverlayData | null>(null);

  const showOverlay = (type: OverlayType, data?: OverlayData) => {
    setCurrentOverlay(type);
    setOverlayData(data || null);
  };

  const hideOverlay = () => {
    setCurrentOverlay(null);
    setOverlayData(null);
  };

  return (
    <OverlayContext.Provider
      value={{
        currentOverlay,
        overlayData,
        showOverlay,
        hideOverlay,
      }}
    >
      {children}
    </OverlayContext.Provider>
  );
};

export const useOverlay = () => {
  const context = useContext(OverlayContext);
  if (!context) {
    throw new Error('useOverlay must be used within an OverlayProvider');
  }
  return context;
};
