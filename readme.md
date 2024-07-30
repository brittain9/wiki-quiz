# Wikipedia Quiz Generator

This project generates high quality quizes based on any Wikipedia topic the user requests using AI.

## Features:
- Prompt templates using semantic kernel
- Multilingual support for English, Spanish, French, Japenese, Chinese, and more
- Dynamic quiz generation with many features being added soon!
- A forgiving search bar (Entering Apple Company will give you a quiz about Apple Inc.)
- Easy support for various AI API currently supporting OpenAI ChatGPT and the Perplexity API
- A ASP.NET Core minimal api for quiz generation and submission (coming soon)

## Requirements:
- AI Service API Key of your choosing
- Docker

## Instructions:
- Set up .env file with your api key (use the .env.example file) (and uncomment your service extension in program.cs of the api)
- Run docker-compose up --build
- Test API at localhost:5543

## Todo:
- Add a basic user class
- Store the quizes in the database
- Add support for Google Gemini and Mistral
- Add logic to handle Wikipedia disambiguation pages: https://en.wikipedia.org/wiki/Category:Disambiguation_pages