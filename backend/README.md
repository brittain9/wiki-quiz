# Wikipedia Quiz Generator

This project generates high-quality quizes based on nearly any topic using Wikipedia and AI.

## Features:
- Dynamic quiz generation utilizing semantic kernel prompt templates  
- Multilingual support for English, Spanish, French, Japanese, Chinese, and more
- Flexible search bar (Entering Apple Company will give you a quiz about Apple Inc.)
- Easy integration with  various AI APIs, currently supporting OpenAI API and the Perplexity API
- ASP.NET Core minimal api for quiz generation and submission (coming soon)

## Requirements:
- Perplexity or OpenAI API key
- Docker

## Instructions:
- Set up the .env file with your API key (use the .env.example file) and uncomment your service extension in program.cs of the API
- Run docker-compose up --build
- Test API at localhost:5543/swagger

## TODO:
- Add a api endpoint for getting the supported models. 
- Fix ai service extension to allow for any service with a key to be valid.
- add model as a parameter to create the quiz

![ERD Diagram](src/WikiQuizGenerator.Data/WikiQuizGeneratorERD.png)