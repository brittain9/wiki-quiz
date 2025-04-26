// wikiApi.ts
import axios from 'axios';

import { parseApiError } from './utils';
import i18n from '../../i18n/i18n';

// Helper function to map complex language codes to simple ones
const getSimpleLanguageCode = (languageCode: string) => {
  return languageCode.split('-')[0];
};

export const wikiApi = {
  /**
   * Fetches Wikipedia topic suggestions based on a query
   */
  fetchWikipediaTopics: async (query: string): Promise<string[]> => {
    try {
      const currentLanguage = i18n.language || 'en';
      const simpleLanguageCode = getSimpleLanguageCode(currentLanguage);

      const WIKIPEDIA_API_URL = `https://${simpleLanguageCode}.wikipedia.org/w/api.php`;

      const response = await axios.get(WIKIPEDIA_API_URL, {
        params: {
          action: 'opensearch',
          search: query,
          limit: 20,
          format: 'json',
          origin: '*', // Necessary to avoid CORS issues
        },
      });
      return response.data[1]; // The second element contains the list of topics
    } catch (error) {
      console.error('Error fetching topics from Wikipedia:', error);
      throw new Error(`Failed to fetch topics: ${parseApiError(error)}`);
    }
  },

  // Add other Wikipedia API methods as needed
};
