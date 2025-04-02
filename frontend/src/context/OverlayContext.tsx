import React, { createContext, useContext, useState, ReactNode } from 'react';

// Define overlay types that can be shown in the application
export type OverlayType = 'login' | 'quiz' | 'quizResult' | 'account' | null;

interface OverlayContextType {
  currentOverlay: OverlayType;
  showOverlay: (overlay: OverlayType) => void;
  hideOverlay: () => void;
  overlayProps: Record<string, any>;
  setOverlayProps: (props: Record<string, any>) => void;
}

const OverlayContext = createContext<OverlayContextType | undefined>(undefined);

interface OverlayProviderProps {
  children: ReactNode;
}

export const OverlayProvider: React.FC<OverlayProviderProps> = ({ children }) => {
  const [currentOverlay, setCurrentOverlay] = useState<OverlayType>(null);
  const [overlayProps, setOverlayProps] = useState<Record<string, any>>({});

  const showOverlay = (overlay: OverlayType) => {
    setCurrentOverlay(overlay);
  };

  const hideOverlay = () => {
    setCurrentOverlay(null);
    setOverlayProps({});
  };

  return (
    <OverlayContext.Provider
      value={{
        currentOverlay,
        showOverlay,
        hideOverlay,
        overlayProps,
        setOverlayProps,
      }}
    >
      {children}
    </OverlayContext.Provider>
  );
};

export const useOverlay = () => {
  const context = useContext(OverlayContext);
  if (context === undefined) {
    throw new Error('useOverlay must be used within an OverlayProvider');
  }
  return context;
}; 