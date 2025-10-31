import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import ProtectedRoute from './components/ProtectedRoute'

// Auth pages
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'

// Teacher pages
import TeacherDashboard from './pages/Teacher/Dashboard'
import ExamForm from './pages/Teacher/ExamForm'
import ExamDetail from './pages/Teacher/ExamDetail'
import QuestionForm from './pages/Teacher/QuestionForm'
import StudentResponses from './pages/Teacher/StudentResponses'
import GradingDashboard from './pages/Teacher/GradingDashboard'
import GradingHub from './pages/Teacher/GradingHub'
import EnhancedGradingDashboard from './pages/Teacher/EnhancedGradingDashboard'
import StudentGradingPortal from './pages/Teacher/StudentGradingPortal'
import TeacherGradingDashboard from './pages/Teacher/TeacherGradingDashboard'
import ResultPublishDashboard from './pages/Teacher/ResultPublishDashboard'

// Student pages
import StudentDashboard from './pages/Student/Dashboard'
import ExamInterface from './pages/Student/ExamInterface'
import MyResults from './pages/Student/MyResults'
import ResultDetail from './pages/Student/ResultDetail'

// Admin pages
import AdminDashboard from './pages/Admin/Dashboard'

function App() {
  return (
    <Router>
      <AuthProvider>
        <Routes>
          {/* Public routes */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* Teacher routes */}
          <Route
            path="/teacher/dashboard"
            element={
              <ProtectedRoute requiredRole="Teacher">
                <TeacherDashboard />
              </ProtectedRoute>
            }
          />
          <Route
            path="/teacher/create-exam"
            element={
              <ProtectedRoute requiredRole="Teacher">
                <ExamForm />
              </ProtectedRoute>
            }
          />
          <Route
            path="/teacher/edit-exam/:examId"
            element={
              <ProtectedRoute requiredRole="Teacher">
                <ExamForm />
              </ProtectedRoute>
            }
          />
          <Route
            path="/teacher/exam/:examId"
            element={
              <ProtectedRoute requiredRole="Teacher">
                <ExamDetail />
              </ProtectedRoute>
            }
          />
          <Route
            path="/teacher/exam/:examId/add-question"
            element={
              <ProtectedRoute requiredRole="Teacher">
                <QuestionForm />
              </ProtectedRoute>
            }
          />
          <Route
            path="/teacher/exam/:examId/question/:questionId/edit"
            element={
              <ProtectedRoute requiredRole="Teacher">
                <QuestionForm />
              </ProtectedRoute>
            }
          />
          <Route
            path="/teacher/exam/:examId/student/:studentId"
            element={
              <ProtectedRoute requiredRole="Teacher">
                <StudentResponses />
              </ProtectedRoute>
            }
          />
          <Route
            path="/teacher/grading/:examId"
            element={
              <ProtectedRoute requiredRole="Teacher">
                <GradingHub />
              </ProtectedRoute>
            }
          />
          <Route
            path="/teacher/exam/:examId/grading"
            element={
              <ProtectedRoute requiredRole="Teacher">
                <EnhancedGradingDashboard />
              </ProtectedRoute>
            }
          />
          <Route
            path="/teacher/exam/:examId/grading-legacy"
            element={
              <ProtectedRoute requiredRole="Teacher">
                <GradingDashboard />
              </ProtectedRoute>
            }
          />
          <Route
            path="/teacher/exam/:examId/grade-students"
            element={
              <ProtectedRoute requiredRole="Teacher">
                <StudentGradingPortal />
              </ProtectedRoute>
            }
          />
          <Route
            path="/teacher/exam/:examId/grade-by-student"
            element={
              <ProtectedRoute requiredRole="Teacher">
                <TeacherGradingDashboard />
              </ProtectedRoute>
            }
          />
          <Route
            path="/teacher/exam/:examId/results"
            element={
              <ProtectedRoute requiredRole="Teacher">
                <ResultPublishDashboard />
              </ProtectedRoute>
            }
          />

          {/* Student routes */}
          <Route
            path="/student/dashboard"
            element={
              <ProtectedRoute requiredRole="Student">
                <StudentDashboard />
              </ProtectedRoute>
            }
          />
          <Route
            path="/student/exam/:examId"
            element={
              <ProtectedRoute requiredRole="Student">
                <ExamInterface />
              </ProtectedRoute>
            }
          />
          <Route
            path="/student/results"
            element={
              <ProtectedRoute requiredRole="Student">
                <MyResults />
              </ProtectedRoute>
            }
          />
          <Route
            path="/student/result/:examId"
            element={
              <ProtectedRoute requiredRole="Student">
                <ResultDetail />
              </ProtectedRoute>
            }
          />

          {/* Admin routes */}
          <Route
            path="/admin/dashboard"
            element={
              <ProtectedRoute requiredRole="Admin">
                <AdminDashboard />
              </ProtectedRoute>
            }
          />

          {/* Default redirect */}
          <Route path="/" element={<Navigate to="/login" replace />} />
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </AuthProvider>
    </Router>
  )
}

export default App
