import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { authService, studentService, resultService } from '../../services/api'
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
  const [completedExams, setCompletedExams] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [stats, setStats] = useState({
    completedExams: 0,
  })

  useEffect(() => {
    loadDashboard()
  }, [])

  const loadDashboard = async () => {
    try {
      // Fetch all completed exams (including unpublished)
      const results = await resultService.getMyCompletedExams(1, 100)
      setCompletedExams(results)
      console.log("Printing completed exams: ", results)
      // Count only published exams for stats
      const publishedCount = results.filter(r => r.status === 'Graded').length
      setStats({
        completedExams: publishedCount,
      })
    } catch (err) {
      console.error('Failed to load dashboard:', err)
      // Silent fail - student can still access exams
    }
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
            <StatLabel>Completed Exams</StatLabel>
            <StatValue>{stats.completedExams}</StatValue>
          </StatCard>
          <StatCard 
            style={{ cursor: 'pointer' }}
            onClick={() => navigate('/student/results')}
          >
            <StatLabel>View Results</StatLabel>
            <StatValue style={{ fontSize: '1rem' }}>📊</StatValue>
            <p style={{ margin: '0.5rem 0 0 0', fontSize: '0.875rem', color: '#6b7280' }}>
              Click to view your published results
            </p>
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

        {completedExams.filter(result => result.status === 'Graded').length > 0 && (
          <Card>
            <PageTitle style={{ marginTop: 0 }}>Completed Exams</PageTitle>
            <Table>
              <thead>
                <tr>
                  <th>Exam</th>
                  <th>Date Submitted</th>
                  <th>Score</th>
                  <th>Percentage</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {completedExams
                  .filter(result => result.status === 'Graded')
                  .map((result) => (
                    <tr key={result.resultID}>
                      <td>
                        <strong>{result.examName}</strong>
                      </td>
                      <td>
                        {result.submittedAt 
                          ? new Date(result.submittedAt).toLocaleDateString()
                          : 'N/A'
                        }
                      </td>
                      <td>
                        {result.totalMarks?.toFixed(2) || '0'} / {result.examTotalMarks || '0'}
                      </td>
                      <td>
                        <span style={{ 
                          color: result.percentage >= result.passingPercentage ? '#10b981' : '#ef4444',
                          fontWeight: 'bold'
                        }}>
                          {result.percentage?.toFixed(2) || '0'}%
                        </span>
                      </td>
                      <td>
                        <span style={{ 
                          padding: '4px 12px',
                          borderRadius: '12px',
                          fontSize: '0.875rem',
                          fontWeight: '500',
                          backgroundColor: result.percentage >= result.passingPercentage ? '#d1fae5' : '#fee2e2',
                          color: result.percentage >= result.passingPercentage ? '#065f46' : '#991b1b'
                        }}>
                          {result.percentage >= result.passingPercentage ? 'Passed' : 'Failed'}
                        </span>
                      </td>
                      <td>
                        <ActionButton onClick={() => navigate(`/student/results/${result.examID}`)}>
                          View Details
                        </ActionButton>
                      </td>
                    </tr>
                  ))}
              </tbody>
            </Table>
          </Card>
        )}

        {completedExams.filter(result => result.status !== 'Graded').length > 0 && (
          <Card>
            <PageTitle style={{ marginTop: 0 }}>Pending Results</PageTitle>
            <div style={{ padding: '20px', backgroundColor: '#fef3c7', borderRadius: '8px', border: '1px solid #fde047' }}>
              <p style={{ margin: '0 0 15px 0', fontSize: '14px', color: '#92400e', fontWeight: '500' }}>
                📋 You have {completedExams.filter(result => result.status !== 'Graded').length} exam(s) that have been completed but results are not yet published:
              </p>
              <ul style={{ margin: '0', paddingLeft: '20px' }}>
                {completedExams
                  .filter(result => result.status !== 'Graded')
                  .map((result) => (
                    <li key={result.resultID} style={{ marginBottom: '8px', color: '#92400e', fontSize: '14px' }}>
                      <strong>{result.examName}</strong> - Submitted on {result.submittedAt ? new Date(result.submittedAt).toLocaleDateString() : 'N/A'}
                      <span style={{ 
                        marginLeft: '10px',
                        padding: '2px 8px',
                        borderRadius: '8px',
                        fontSize: '0.75rem',
                        fontWeight: '500',
                        backgroundColor: '#fbbf24',
                        color: '#78350f'
                      }}>
                        Not Published Yet
                      </span>
                    </li>
                  ))}
              </ul>
              <p style={{ margin: '15px 0 0 0', fontSize: '13px', color: '#92400e', fontStyle: 'italic' }}>
                Your instructor will publish the results once grading is complete.
              </p>
            </div>
          </Card>
        )}
      </MainContent>
    </DashboardContainer>
  )
}
