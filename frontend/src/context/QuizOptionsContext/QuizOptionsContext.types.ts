export interface QuizOptions {
  topic: string;
  numQuestions: number;
  numOptions: number;
  extractLength: number;
  language: string;
  selectedService: string | null;
  selectedModel: string | null;
  availableServices: string[];
  availableModels: string[];
}

export interface QuizOptionsContextType {
  quizOptions: QuizOptions;
  setTopic: (topic: string) => void;
  setNumQuestions: (numQuestions: number) => void;
  setNumOptions: (numOptions: number) => void;
  setExtractLength: (extractLength: number) => void;
  setLanguage: (language: string) => void;
  setSelectedService: (serviceId: string | null) => void;
  setSelectedModel: (modelId: string | null) => void;
}
