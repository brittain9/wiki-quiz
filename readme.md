# Wikipedia Quiz Generator

This project generates high-quality quizes based on nearly any topic using Wikipedia and AI.

## Features:
- Dynamic quiz generation utilizing semantic kernel prompt templates  
- Multilingual support for English, Spanish, French, Japenese, Chinese, and more
- Flexible search bar (Entering Apple Company will give you a quiz about Apple Inc.)
- Easy integration with various AI APIs, currently supporting OpenAI API and the Perplexity API
- ASP.NET Core minimal api for quiz generation and submission (coming soon)

## Requirements:
- AI Service API Key of your choosing
- Docker

## Instructions:
- Set up the .env file with your API key (use the .env.example file) and uncomment your service extension in program.cs of the API
- Run docker-compose up --build
- Test API at localhost:5543

## Todo:
- Add a basic user class
- Store the quizes in the database
- Add support for Google Gemini and Mistral
- Add logic to handle Wikipedia disambiguation pages: https://en.wikipedia.org/wiki/Category:Disambiguation_pages