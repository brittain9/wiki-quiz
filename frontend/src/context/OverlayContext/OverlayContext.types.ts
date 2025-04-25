export type OverlayType = 'login' | 'account' | 'quiz_result' | null;

export interface OverlayData {
  message?: string;
  onSuccess?: () => void;
  onCancel?: () => void;
  resultId?: number;
  [key: string]: unknown;
}

export interface OverlayContextType {
  currentOverlay: OverlayType;
  overlayData: OverlayData | null;

  showOverlay: (type: OverlayType, data?: OverlayData) => void;
  hideOverlay: () => void;
}
