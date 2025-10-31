import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { authService, studentService } from '../../services/api'
import {
  DashboardContainer,
  NavBar,
  NavLeft,
  Logo,
  NavMenu,
  UserInfo,
  LogoutButton,
  MainContent,
  PageTitle,
  WelcomeSection,
  Grid,
  StatCard,
  StatLabel,
  StatValue,
  Card,
  Table,
  ActionButton,
} from '../../styles/DashboardStyles'
import {
  FormGroup,
  Label,
  Input,
  Button,
  ErrorMessage,
  SuccessMessage,
} from '../../styles/SharedStyles'

export default function StudentDashboard() {
  const navigate = useNavigate()
  const { user, logout } = useAuth()
  const [accessCode, setAccessCode] = useState('')
  const [accessPassword, setAccessPassword] = useState('')
  const [exams, setExams] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [stats, setStats] = useState({
    totalExams: 0,
    completedExams: 0,
  })

  useEffect(() => {
    loadDashboard()
  }, [])

  const loadDashboard = () => {
    // Initialize with empty data - students can access exams by code
    setStats({
      totalExams: 0,
      completedExams: 0,
    })
  }

  const handleAccessExam = async (e) => {
    e.preventDefault()
    setError('')
    setSuccess('')
    setLoading(true)

    try {
      const response = await studentService.validateAccess(accessCode, accessPassword)

      if (response.canAttempt) {
        setSuccess('Access granted! Redirecting to exam...')
        setTimeout(() => {
          navigate(`/student/exam/${response.data.examID}`)
        }, 1500)
      } else {
        setError(response.message || 'Access denied')
      }
    } catch (err) {
      setError(err.message || 'Failed to validate exam access')
    } finally {
      setLoading(false)
    }
  }

  const handleLogout = async () => {
    try {
      await authService.logout()
      logout()
      navigate('/login')
    } catch (err) {
      console.error('Logout failed:', err)
      logout()
      navigate('/login')
    }
  }

  return (
    <DashboardContainer>
      <NavBar>
        <NavLeft>
          <Logo>Quiz Portal</Logo>
        </NavLeft>
        <NavMenu>
          <UserInfo>
            <span>{user?.fullName} ({user?.role})</span>
          </UserInfo>
          <LogoutButton onClick={handleLogout}>Logout</LogoutButton>
        </NavMenu>
      </NavBar>

      <MainContent>
        <WelcomeSection>
          <h1>Welcome, {user?.fullName}</h1>
          <p>Access and take exams using the access code provided by your instructor</p>
        </WelcomeSection>

        <Grid>
          <StatCard>
            <StatLabel>Exams Available</StatLabel>
            <StatValue>{stats.totalExams}</StatValue>
          </StatCard>
          <StatCard>
            <StatLabel>Completed Exams</StatLabel>
            <StatValue>{stats.completedExams}</StatValue>
          </StatCard>
        </Grid>

        <Card>
          <PageTitle style={{ marginTop: 0 }}>Access Exam</PageTitle>

          {error && <ErrorMessage>{error}</ErrorMessage>}
          {success && <SuccessMessage>{success}</SuccessMessage>}

          <form onSubmit={handleAccessExam}>
            <FormGroup>
              <Label htmlFor="accessCode">Exam Access Code</Label>
              <Input
                id="accessCode"
                type="text"
                value={accessCode}
                onChange={(e) => setAccessCode(e.target.value.toUpperCase())}
                placeholder="Enter access code"
                required
                disabled={loading}
              />
            </FormGroup>

            <FormGroup>
              <Label htmlFor="accessPassword">Access Password</Label>
              <Input
                id="accessPassword"
                type="password"
                value={accessPassword}
                onChange={(e) => setAccessPassword(e.target.value)}
                placeholder="Enter access password"
                required
                disabled={loading}
              />
            </FormGroup>

            <Button type="submit" disabled={loading}>
              {loading ? 'Validating...' : 'Access Exam'}
            </Button>
          </form>

          <div style={{ marginTop: '20px', paddingTop: '20px', borderTop: '1px solid #e5e7eb' }}>
            <p style={{ color: '#6b7280', fontSize: '14px' }}>
              <strong>How to access an exam:</strong><br />
              1. Get the access code and password from your instructor<br />
              2. Enter them above<br />
              3. Click "Access Exam" to start
            </p>
          </div>
        </Card>
      </MainContent>
    </DashboardContainer>
  )
}
