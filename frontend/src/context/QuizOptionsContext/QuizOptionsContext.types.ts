export interface QuizOptions {
  topic: string;
  numQuestions: number;
  numOptions: number;
  extractLength: number;
  language: string;
  selectedService: number | null;
  selectedModel: number | null;
  availableServices: Record<number, string>;
  availableModels: Record<number, string>;
}

export interface QuizOptionsContextType {
  quizOptions: QuizOptions;
  setTopic: (topic: string) => void;
  setNumQuestions: (numQuestions: number) => void;
  setNumOptions: (numOptions: number) => void;
  setExtractLength: (extractLength: number) => void;
  setLanguage: (language: string) => void;
  setSelectedService: (serviceId: number | null) => void;
  setSelectedModel: (modelId: number | null) => void;
}
