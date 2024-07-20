## Wikipedia Quiz Generator

This project will generate quizes and store previous quizes based on any topic the user requests using Wikipedia and ChatGPT.
It exposes an API that can then be used in another app I will make.

Requirements:
- OpenAI API key

Instructions:
- Set up .env file with Open AI key
- Run docker-compose up --build
- Test API at localhost:5000

Just finished question generation.
Todo:
Now I want to use the links to generate an entire quiz. Page length will determine the number of questions I can generate from a page before going to a random link.
Add a basic user class
Store the quizes in the database
Make the web app

Considerations:
The questions generation is considerably better than I expected.
Gpt 4o mini is really good, and the Wikipedia content ensures quality and accuracy.
It might start to get costly. I would have to use a request to the openai api each page.
I need to find the right balance of cost and quality in the quiz generation.