import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { teacherService } from '../../services/api'
import TeacherLayout from '../../components/TeacherLayout'
import styled from 'styled-components'
import {
  DashboardGrid,
  StatCard,
  QuickActionCard,
  Alert,
  COLORS,
} from '../../styles/TeacherStyles'

const WelcomeSection = styled.div`
  background: #2c3e50;
  color: white;
  padding: 40px;
  border-radius: 8px;
  margin-bottom: 32px;
  box-shadow: 0 4px 12px rgba(44, 62, 80, 0.15);

  h1 {
    font-size: 28px;
    font-weight: 600;
    margin: 0 0 12px 0;
    letter-spacing: -0.3px;
  }

  p {
    font-size: 15px;
    margin: 0;
    opacity: 0.9;
    font-weight: 400;
    letter-spacing: 0px;
  }
`

const SectionTitle = styled.h2`
  font-size: 22px;
  font-weight: 600;
  color: ${COLORS.text};
  margin: 0 0 20px 0;
  padding-bottom: 12px;
  border-bottom: 2px solid ${COLORS.border};
`

const QuickActionsContainer = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 16px;
  margin-bottom: 32px;
`

const LoadingSpinner = styled.div`
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;

  .spinner {
    width: 40px;
    height: 40px;
    border: 4px solid ${COLORS.border};
    border-top-color: ${COLORS.primary};
    border-radius: 50%;
    animation: spin 0.6s linear infinite;
  }

  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }
`

export default function TeacherDashboard() {
  const navigate = useNavigate()
  const { user } = useAuth()
  const [stats, setStats] = useState({
    totalExams: 0,
    activeExams: 0,
    upcomingExams: 0,
    endedExams: 0,
  })
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    loadDashboardStats()
  }, [])

  const loadDashboardStats = async () => {
    try {
      setLoading(true)
      const examsData = await teacherService.getExams()
      const examsList = examsData.data || []

      // Calculate statistics only
      let active = 0, upcoming = 0, ended = 0
      examsList.forEach((exam) => {
        const now = new Date()
        const start = new Date(exam.scheduleStart)
        const end = new Date(exam.scheduleEnd)

        if (now < start) {
          upcoming++
        } else if (now > end) {
          ended++
        } else {
          active++
        }
      })

      setStats({
        totalExams: examsList.length,
        activeExams: active,
        upcomingExams: upcoming,
        endedExams: ended,
      })

      setError('')
    } catch (err) {
      setError(err.message || 'Failed to load dashboard')
    } finally {
      setLoading(false)
    }
  }

  const handleCreateExam = () => {
    navigate('/teacher/create-exam')
  }

  if (loading) {
    return (
      <TeacherLayout pageTitle="Dashboard">
        <LoadingSpinner>
          <div className="spinner" />
        </LoadingSpinner>
      </TeacherLayout>
    )
  }

  return (
    <TeacherLayout pageTitle="Dashboard">
      <WelcomeSection>
        <h1>Welcome back, {user?.fullName}</h1>
        <p>Manage your exams, track student progress, and grade responses</p>
      </WelcomeSection>

      {error && (
        <Alert type="error">
          <div className="icon">⚠</div>
          <div className="content">
            <p>{error}</p>
          </div>
        </Alert>
      )}

      {/* Statistics */}
      <DashboardGrid>
        <StatCard gradientFrom="#1e40af" gradientTo="#1e3a8a">
          <h3>Total Exams</h3>
          <div className="value">{stats.totalExams}</div>
          <div className="subtitle">All time exams created</div>
        </StatCard>
        <StatCard gradientFrom="#0891b2" gradientTo="#0e7490">
          <h3>Active Exams</h3>
          <div className="value">{stats.activeExams}</div>
          <div className="subtitle">Exams in progress</div>
        </StatCard>
        <StatCard gradientFrom="#f59e0b" gradientTo="#d97706">
          <h3>Upcoming Exams</h3>
          <div className="value">{stats.upcomingExams}</div>
          <div className="subtitle">Scheduled to start</div>
        </StatCard>
        <StatCard gradientFrom="#10b981" gradientTo="#059669">
          <h3>Completed Exams</h3>
          <div className="value">{stats.endedExams}</div>
          <div className="subtitle">Ready for grading</div>
        </StatCard>
      </DashboardGrid>

      {/* Quick Actions */}
      <SectionTitle>Quick Actions</SectionTitle>
      <QuickActionsContainer>
        <QuickActionCard onClick={handleCreateExam}>
          <div className="icon">■</div>
          <h4>Create New Exam</h4>
          <p>Set up a new exam with questions and schedule</p>
        </QuickActionCard>
        <QuickActionCard onClick={() => navigate('/teacher/dashboard')}>
          <div className="icon">▤</div>
          <h4>View Statistics</h4>
          <p>Check exam performance and student results</p>
        </QuickActionCard>
        <QuickActionCard onClick={() => navigate('/teacher/grading')}>
          <div className="icon">✓</div>
          <h4>Grade Responses</h4>
          <p>Evaluate and grade student responses</p>
        </QuickActionCard>
      </QuickActionsContainer>
    </TeacherLayout>
  )
}
