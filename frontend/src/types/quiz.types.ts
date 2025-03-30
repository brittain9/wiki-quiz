export interface Quiz {
  id: number;
  title: string;
  createdAt: string;
  aiResponses: AIResponse[];
}

export interface AIResponse {
  id: number;
  responseTopic: string;
  topicUrl: string;
  responseTime: number;
  promptTokenUsage: number | null;
  completionTokenUsage: number | null;
  modelName: string | null;
  questions: Question[];
}

export interface Question {
  id: number;
  text: string;
  options: string[];
}
