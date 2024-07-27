### Wikipedia Quiz Generator

This project will generate high quality quizes based on any Wikipedia topic the user requests using AI.

# Requirements:
- AI Service API Key of your choosing (currently supports OpenAI and Perplexity)
- Docker

# Instructions:
- Set up .env file with your api key (use the .env.example file)
- Run docker-compose up --build
- Test API at localhost:5543

# Todo:
- Add a basic user class
- Store the quizes in the database
- Add support for Google Gemini and Mistral
- Add logic to handle Wikipedia disambiguation pages: https://en.wikipedia.org/wiki/Category:Disambiguation_pages
- Use more semantic kernel functionality like prompt templates possibly according to topic category (a quiz about a book would have a different prompt that a person for example)