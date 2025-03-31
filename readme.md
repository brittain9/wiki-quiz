# Wiki Quiz

An interactive quiz application that lets users test their knowledge on topics sourced from Wikipedia.

## Project Structure

The project is organized into frontend and backend directories:

```
wiki-quiz/
├── frontend/           # React frontend application
│   ├── src/
│   │   ├── components/ # UI components
│   │   ├── context/    # React context providers
│   │   ├── hooks/      # Custom React hooks
│   │   ├── pages/      # Page components
│   │   ├── services/   # API client and services
│   │   ├── types/      # TypeScript type definitions
│   │   └── utils/      # Utility functions
└── backend/            # Node.js backend application
```

## Features

- User authentication with Google OAuth
- Quiz generation from Wikipedia articles
- Multiple-choice questions with explanations
- Score tracking and history
- Responsive design for mobile and desktop

## Development Setup

### Prerequisites

- Node.js (v18+)
- npm or yarn

### Frontend

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

### Backend

```bash
# Navigate to backend directory
cd backend

# Install dependencies
npm install

# Start development server
npm run dev
```

## Authentication

The application uses Google OAuth for authentication. Users can:

- Sign in with their Google account
- View their quiz history
- Create custom quizzes

## Logging

The application includes a comprehensive logging system:

- Client-side logging of user interactions
- Authentication event logging
- API request/response logging
- Navigation tracking

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
