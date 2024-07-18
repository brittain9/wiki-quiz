## Wikipedia Quiz Generator

This project will generate quizes and store previous quizes based on any topic the user requests using Wikipedia and ChatGPT.
It exposes an API that can then be used in another app I will make.

Requirements:
- OpenAI API key

Instructions:
- Set up .env file with Open AI key
- Run docker-compose up --build
- Test API at localhost:5000

TODO:
- Wiki.NET query result previews are random and inadequate for reliably generating questions; Find better way to do this.
- Set up tests (for fun :)

Considerations:
- Downloading all of Wikipedia (21gb)
- Scrapping Wikipedia?