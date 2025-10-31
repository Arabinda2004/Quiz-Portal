# Quiz Portal Frontend

A modern React frontend for the Quiz Portal application built with Vite, styled-components, and Axios.

## Features (Milestone 1)

- **Authentication**: Register and login with role-based access
- **Three Role-Based Dashboards**:
  - **Teacher Dashboard**: View and manage exams, questions, and student responses
  - **Student Dashboard**: Access exams using access codes, view exam details
  - **Admin Dashboard**: Manage users and view all exams in the system
- **Minimal, Professional Design**: Clean UI with solid colors, no emojis
- **Responsive Layout**: Works on desktop and mobile devices

## Project Structure

```
src/
├── components/          # Reusable components
│   └── ProtectedRoute.jsx
├── context/            # Context providers
│   └── AuthContext.jsx
├── pages/              # Page components
│   ├── LoginPage.jsx
│   ├── RegisterPage.jsx
│   ├── Teacher/
│   │   └── Dashboard.jsx
│   ├── Student/
│   │   └── Dashboard.jsx
│   └── Admin/
│       └── Dashboard.jsx
├── services/           # API service calls
│   └── api.js
├── styles/             # Styled components
│   ├── SharedStyles.js
│   └── DashboardStyles.js
├── App.jsx            # Main app component
└── main.jsx           # Entry point
```

## Installation & Setup

### Prerequisites
- Node.js 16+ and npm

### Steps

1. Install dependencies:
```bash
npm install
```

2. Start the development server:
```bash
npm run dev
```

The app will open at `http://localhost:5173`

3. Build for production:
```bash
npm run build
```

## API Configuration

The frontend is configured to connect to the backend API at `http://localhost:5242`. 

To change the API base URL, edit `src/services/api.js`:
```javascript
const API_BASE_URL = 'http://localhost:5242/api'
```

## Test Credentials

### Admin Account
- Email: `admin@quizportal.com`
- Password: `Admin123!`

### Demo Teacher
- Register as Teacher role during signup

### Demo Student
- Register as Student role during signup

## Available Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `POST /api/auth/logout` - Logout user

### User Management
- `GET /api/users/profile` - Get current user profile
- `PUT /api/users/profile` - Update profile
- `PUT /api/users/change-password` - Change password

### Teacher Features
- `POST /api/exams` - Create exam
- `GET /api/exams` - Get teacher's exams
- `PUT /api/exams/{id}` - Update exam
- `DELETE /api/exams/{id}` - Delete exam
- `POST /api/exams/{examId}/questions` - Create question
- `GET /api/exams/{examId}/questions` - Get exam questions
- `GET /api/teacher/exams/{examId}/responses/students` - View student attempts
- `GET /api/teacher/exams/{examId}/responses/statistics` - View exam statistics

### Student Features
- `POST /api/exams/validate-access` - Validate exam access
- `POST /api/exams/{examId}/responses` - Submit answer
- `GET /api/exams/{examId}/responses` - Get exam responses
- `GET /api/exams/{examId}/responses/status` - Get submission status

### Admin Features
- `GET /api/admin/users` - Get all users
- `POST /api/admin/users` - Create user
- `PUT /api/admin/users/{id}` - Update user
- `DELETE /api/admin/users/{id}` - Delete user
- `GET /api/exams/all` - Get all exams

## Technology Stack

- **React 18** - UI library
- **Vite** - Build tool
- **styled-components** - CSS-in-JS styling
- **React Router v6** - Routing
- **Axios** - HTTP client
- **JavaScript** - Programming language

## Design Principles

- **Minimal & Professional**: Clean interface with focus on functionality
- **No Emojis**: White background maintained throughout
- **Solid Colors**: Consistent color scheme using blue and neutral tones
- **Responsive**: Mobile-first approach
- **Accessibility**: Semantic HTML and proper contrast ratios

## Future Enhancements (Milestone 2+)

- Create and edit exams page
- Question management interface
- Student exam taking interface with timer
- Result viewing and grading interface
- Real-time notifications
- Analytics and reporting
- User profile management
- Search and filter functionality

## License

MIT
