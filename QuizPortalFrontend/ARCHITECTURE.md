# Quiz Portal Frontend - Complete Documentation

## Project Overview

This is a professional React frontend for the Quiz Portal application. It provides role-based access for three user types: Students, Teachers, and Admins. The first milestone implements authentication and basic dashboards for each role.

## Architecture

### Technology Stack
- **Framework**: React 18
- **Build Tool**: Vite
- **Styling**: styled-components (CSS-in-JS)
- **Routing**: React Router v6
- **HTTP Client**: Axios
- **Language**: JavaScript (ES6+)

### Design Principles
- **Minimal & Professional**: Clean, distraction-free UI
- **Solid Colors**: No gradients, consistent color scheme (Blues, Grays)
- **No Emojis**: White backgrounds, professional appearance
- **Responsive**: Works on desktop and mobile
- **User-Centric**: Clear navigation and feedback

## Project Structure

```
QuizPortalFrontend/
├── src/
│   ├── App.jsx                    # Main app router
│   ├── main.jsx                   # React entry point
│   │
│   ├── components/
│   │   └── ProtectedRoute.jsx     # Route protection with role checks
│   │
│   ├── context/
│   │   └── AuthContext.jsx        # Authentication context provider
│   │
│   ├── pages/
│   │   ├── LoginPage.jsx          # Login page
│   │   ├── RegisterPage.jsx       # Registration page
│   │   ├── Teacher/
│   │   │   └── Dashboard.jsx      # Teacher dashboard
│   │   ├── Student/
│   │   │   └── Dashboard.jsx      # Student dashboard
│   │   └── Admin/
│   │       └── Dashboard.jsx      # Admin dashboard
│   │
│   ├── services/
│   │   └── api.js                 # Axios instance & API methods
│   │
│   └── styles/
│       ├── SharedStyles.js        # Common styled components
│       └── DashboardStyles.js     # Dashboard styled components
│
├── index.html                     # HTML entry point
├── vite.config.js                # Vite configuration
├── package.json                  # Dependencies
├── .env.example                  # Environment template
├── .gitignore                    # Git ignore rules
├── README.md                     # Project README
├── SETUP.md                      # Setup instructions
└── ARCHITECTURE.md               # This file
```

## Key Components & Features

### 1. Authentication Context (AuthContext.jsx)

**Purpose**: Manages global authentication state

**Features**:
- User data persistence
- Automatic auth check on app load
- Login/logout state management
- Role information storage

**Usage**:
```javascript
const { user, isAuthenticated, loading, login, logout } = useAuth()
```

### 2. Protected Routes (ProtectedRoute.jsx)

**Purpose**: Ensures only authenticated users with correct roles can access pages

**Features**:
- Checks authentication status
- Verifies user role
- Redirects unauthorized users to login
- Shows loading state

**Usage**:
```javascript
<ProtectedRoute requiredRole="Teacher">
  <TeacherDashboard />
</ProtectedRoute>
```

### 3. API Service (api.js)

**Purpose**: Centralized API communication with the backend

**Features**:
- Axios instance with base URL
- Credentials support for cookies
- Error handling
- Organized methods by feature (auth, teacher, student, admin)

**Methods**:
```javascript
authService.register()
authService.login()
authService.logout()

userService.getProfile()
userService.updateProfile()
userService.changePassword()

teacherService.createExam()
teacherService.getExams()
teacherService.getQuestions()

studentService.validateAccess()
studentService.submitAnswer()

adminService.getAllUsers()
adminService.getAllExams()
```

### 4. Styled Components

**SharedStyles.js**:
- Form elements (Input, Select, Button)
- Auth page styles (AuthContainer, AuthCard)
- Messages (ErrorMessage, SuccessMessage)
- Common utilities (LoadingSpinner, etc.)

**DashboardStyles.js**:
- Dashboard layout (DashboardContainer, NavBar)
- Content area styles
- Table and card components
- Action buttons

## User Flows

### Registration Flow
1. User navigates to `/register`
2. Fills registration form with name, email, password, and role
3. Form validates passwords match
4. API registers user and returns auth tokens
5. User is logged in and redirected to dashboard

### Login Flow
1. User navigates to `/login`
2. Enters email and password
3. API validates credentials
4. On success, stores auth info and redirects to role-based dashboard:
   - Teacher → `/teacher/dashboard`
   - Student → `/student/dashboard`
   - Admin → `/admin/dashboard`

### Teacher Dashboard Flow
1. Shows welcome message and statistics
2. Displays list of created exams
3. Allows creating, editing, viewing, and deleting exams
4. Shows exam status (Draft, Upcoming, Active, Ended)
5. Quick access to exam management features

### Student Dashboard Flow
1. Shows welcome message
2. Provides form to access exam using code + password
3. On validation success, redirects to exam taking interface
4. Shows exam statistics

### Admin Dashboard Flow
1. Shows system statistics (users, exams)
2. Tabbed interface for Users and Exams
3. User management: create, edit, delete
4. View all exams with creator information
5. Role-based user display

## Authentication & Security

### Cookie-Based Authentication
- Backend sets HTTP-only cookies with JWT tokens
- Frontend sends credentials: 'include' with every request
- Automatic token refresh (handled by backend)

### Protected Routes
- All protected pages check authentication status
- Routes verify user role matches requirement
- Unauthorized users redirected to login

### Input Validation
- Email format validation
- Password strength requirements
- Confirm password matching
- Role selection constraint

## Styling Approach

### Color Scheme
- **Primary Blue**: #1e40af (links, buttons, active states)
- **Dark Blue**: #1e3a8a (hover states)
- **Gray**: #1f2937, #6b7280 (text, borders)
- **Light Gray**: #f5f5f5, #f9fafb (backgrounds)
- **Red**: #dc2626 (danger actions)
- **Light Red**: #fee (error backgrounds)

### Components
- All components use styled-components
- Consistent spacing and typography
- Smooth transitions and hover effects
- Mobile-responsive layout

## API Integration

### Base URL
```javascript
const API_BASE_URL = 'http://localhost:5242/api'
```

### Request Examples

**Register**:
```javascript
POST /auth/register
{
  fullName: "John Doe",
  email: "john@example.com",
  password: "Pass123",
  confirmPassword: "Pass123",
  role: "Student"
}
```

**Login**:
```javascript
POST /auth/login
{
  email: "john@example.com",
  password: "Pass123"
}
```

**Get Teacher Exams**:
```javascript
GET /exams
Authorization: Bearer {token}
```

## Error Handling

### Frontend Error Handling
- Try-catch blocks on all API calls
- User-friendly error messages
- Form validation before submission
- Network error handling

### Error Display
- Error messages shown in red box
- Loading states prevent duplicate submissions
- Disabled buttons during API calls

## Development Workflow

### Setup
```bash
npm install
npm run dev
```

### Build
```bash
npm run build
npm run preview
```

### Testing
1. Register new users with different roles
2. Login with existing credentials
3. Test logout functionality
4. Verify role-based access control
5. Check error handling with invalid data

## Code Examples

### Using Authentication Context
```javascript
import { useAuth } from '../context/AuthContext'

export function MyComponent() {
  const { user, isAuthenticated, login, logout } = useAuth()
  
  return (
    <div>
      {isAuthenticated && <p>Welcome {user.fullName}</p>}
    </div>
  )
}
```

### Making API Calls
```javascript
import { teacherService } from '../services/api'

async function loadExams() {
  try {
    const response = await teacherService.getExams()
    setExams(response.data)
  } catch (error) {
    setError(error.message)
  }
}
```

### Protected Route Usage
```javascript
import ProtectedRoute from './ProtectedRoute'
import TeacherDashboard from './pages/Teacher/Dashboard'

<Route
  path="/teacher/dashboard"
  element={
    <ProtectedRoute requiredRole="Teacher">
      <TeacherDashboard />
    </ProtectedRoute>
  }
/>
```

## Troubleshooting

### Issue: CORS Errors
**Solution**: Ensure backend CORS includes frontend URL in Program.cs

### Issue: 401 Unauthorized
**Solution**: Check if token is expired or credentials are invalid

### Issue: Components not re-rendering
**Solution**: Check if state updates are triggering correctly, verify useEffect dependencies

### Issue: Styled components not working
**Solution**: Ensure styled-components is properly installed and imported

## Future Enhancements (Milestone 2+)

### Teacher Features
- Create/edit exam interface with validation
- Question management (MCQ, descriptive)
- Question option management
- Exam scheduling interface
- Student response review interface
- Grading interface for manual questions
- Analytics dashboard

### Student Features
- Full exam interface with timer
- Question navigation
- Answer save mechanism
- Review and submit functionality
- Result viewing
- Performance analytics

### Admin Features
- User creation and bulk import
- Role assignment and management
- System settings management
- Audit logs viewing
- Analytics dashboard

### General
- Real-time notifications
- Search and filter functionality
- Profile management page
- Two-factor authentication
- Export reports (CSV/PDF)
- Dark mode support

## Performance Optimization

### Current Optimizations
- Lazy loading with React Router
- Component memoization where needed
- Conditional rendering
- Event delegation

### Future Optimizations
- Code splitting
- Image optimization
- API response caching
- Virtual scrolling for large lists

## Browser Support

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## License

MIT License - See LICENSE file for details

## Contributing

When adding new features:
1. Follow the existing folder structure
2. Use styled-components for styling
3. Add error handling for API calls
4. Update this documentation
5. Test with all user roles
