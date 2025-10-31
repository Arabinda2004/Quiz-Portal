import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { teacherService } from '../../services/api'
import styled from 'styled-components'

const Container = styled.div`
  min-height: 100vh;
  background-color: #f3f4f6;
`

const NavBar = styled.nav`
  background-color: #1f2937;
  color: white;
  padding: 1rem 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
`

const NavLeft = styled.div`
  display: flex;
  align-items: center;
  gap: 2rem;
`

const Logo = styled.div`
  font-size: 1.5rem;
  font-weight: bold;
  cursor: pointer;
`

const NavMenu = styled.div`
  display: flex;
  align-items: center;
  gap: 1rem;
`

const LogoutButton = styled.button`
  padding: 0.5rem 1rem;
  background-color: #dc2626;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.875rem;

  &:hover {
    background-color: #b91c1c;
  }
`

const MainContent = styled.div`
  padding: 2rem;
  max-width: 1400px;
  margin: 0 auto;
`

const BackButton = styled.button`
  background: none;
  border: none;
  color: #3b82f6;
  cursor: pointer;
  font-size: 0.95rem;
  margin-bottom: 1.5rem;
  padding: 0;

  &:hover {
    text-decoration: underline;
  }
`

const Header = styled.div`
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  margin-bottom: 2rem;
`

const Title = styled.h1`
  margin: 0 0 1rem 0;
  font-size: 2rem;
  color: #1f2937;
`

const StatsGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-top: 1.5rem;
`

const StatItem = styled.div`
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 1.5rem;
  border-radius: 8px;
  text-align: center;
`

const StatLabel = styled.div`
  font-size: 0.875rem;
  opacity: 0.9;
  margin-bottom: 0.5rem;
`

const StatValue = styled.div`
  font-size: 2rem;
  font-weight: bold;
`

const StatPercent = styled.div`
  font-size: 0.95rem;
  opacity: 0.85;
  margin-top: 0.5rem;
`

const Card = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  padding: 2rem;
  margin-bottom: 1.5rem;
`

const Button = styled.button`
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
`

const PrimaryButton = styled(Button)`
  background-color: #3b82f6;
  color: white;

  &:hover {
    background-color: #2563eb;
  }

  &:disabled {
    background-color: #9ca3af;
    cursor: not-allowed;
  }
`

const InfoBox = styled.div`
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
  font-size: 0.95rem;
  font-weight: 500;
  border-left: 4px solid;
  background-color: #ecfdf5;
  border-left-color: #065f46;
  color: #065f46;
`

const Table = styled.table`
  width: 100%;
  border-collapse: collapse;

  thead {
    background-color: #f3f4f6;
    border-bottom: 1px solid #e5e7eb;
  }

  th {
    padding: 1rem;
    text-align: left;
    font-weight: 600;
    color: #374151;
  }

  td {
    padding: 1rem;
    border-bottom: 1px solid #e5e7eb;
    color: #1f2937;
  }

  tbody tr:hover {
    background-color: #f9fafb;
  }
`

const Badge = styled.span`
  display: inline-block;
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  background-color: ${(props) => {
    switch (props.status) {
      case 'Graded':
        return '#d1fae5'
      case 'Pending':
        return '#fef3c7'
      case 'PartialGraded':
        return '#dbeafe'
      default:
        return '#e5e7eb'
    }
  }};
  color: ${(props) => {
    switch (props.status) {
      case 'Graded':
        return '#065f46'
      case 'Pending':
        return '#92400e'
      case 'PartialGraded':
        return '#1e40af'
      default:
        return '#374151'
    }
  }};
`

const LoadingSpinner = styled.div`
  text-align: center;
  padding: 2rem;
  color: #6b7280;
`

const ErrorMessage = styled.div`
  background-color: #fee;
  border-left: 4px solid #991b1b;
  color: #991b1b;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
`

const EmptyState = styled.div`
  text-align: center;
  padding: 3rem 1rem;
  color: #6b7280;
`

const ProgressBar = styled.div`
  width: 100%;
  height: 8px;
  background-color: #e5e7eb;
  border-radius: 4px;
  overflow: hidden;
  margin-top: 0.5rem;
`

const ProgressFill = styled.div`
  height: 100%;
  background-color: #3b82f6;
  width: ${(props) => props.percentage}%;
  transition: width 0.3s ease;
`

export default function GradingHub() {
  const navigate = useNavigate()
  const { examId } = useParams()
  const { user, logout } = useAuth()

  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [gradingStats, setGradingStats] = useState(null)
  const [examInfo, setExamInfo] = useState(null)

  useEffect(() => {
    loadGradingData()
  }, [examId])

  const loadGradingData = async () => {
    try {
      setLoading(true)
      setError('')

      // Load exam info
      const examResponse = await teacherService.getExamById(examId)
      setExamInfo(examResponse.data)

      // Load grading statistics
      const statsResponse = await teacherService.getGradingStatistics(examId)
      setGradingStats(statsResponse.data)
    } catch (err) {
      if (err.message && err.message.includes('has not ended yet')) {
        setError('❌ This exam has not ended yet. Grading will be available after the exam concludes.')
      } else {
        setError('Failed to load grading data. ' + (err.message || 'Please try again.'))
      }
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  const handleLogout = async () => {
    try {
      logout()
      navigate('/login')
    } catch (err) {
      console.error('Logout failed:', err)
    }
  }

  if (loading) {
    return (
      <Container>
        <NavBar>
          <NavLeft>
            <Logo onClick={() => navigate('/teacher/dashboard')}>Quiz Portal</Logo>
          </NavLeft>
        </NavBar>
        <MainContent>
          <LoadingSpinner>Loading grading data...</LoadingSpinner>
        </MainContent>
      </Container>
    )
  }

  if (error) {
    return (
      <Container>
        <NavBar>
          <NavLeft>
            <Logo onClick={() => navigate('/teacher/dashboard')}>Quiz Portal</Logo>
          </NavLeft>
          <NavMenu>
            <span>{user?.fullName} ({user?.role})</span>
            <LogoutButton onClick={handleLogout}>Logout</LogoutButton>
          </NavMenu>
        </NavBar>
        <MainContent>
          <BackButton onClick={() => navigate(`/teacher/exam/${examId}`)}>← Back to Exam</BackButton>
          <ErrorMessage>{error}</ErrorMessage>
        </MainContent>
      </Container>
    )
  }

  if (!gradingStats || !examInfo) {
    return (
      <Container>
        <NavBar>
          <NavLeft>
            <Logo onClick={() => navigate('/teacher/dashboard')}>Quiz Portal</Logo>
          </NavLeft>
        </NavBar>
        <MainContent>
          <ErrorMessage>Unable to load grading data. Please try again.</ErrorMessage>
          <BackButton onClick={() => navigate(-1)}>← Go Back</BackButton>
        </MainContent>
      </Container>
    )
  }

  const gradingPercentage = gradingStats.totalResponses > 0
    ? ((gradingStats.gradedResponses / gradingStats.totalResponses) * 100).toFixed(0)
    : 0

  return (
    <Container>
      <NavBar>
        <NavLeft>
          <Logo onClick={() => navigate('/teacher/dashboard')}>Quiz Portal</Logo>
        </NavLeft>
        <NavMenu>
          <span>{user?.fullName} ({user?.role})</span>
          <LogoutButton onClick={handleLogout}>Logout</LogoutButton>
        </NavMenu>
      </NavBar>

      <MainContent>
        <BackButton onClick={() => navigate(`/teacher/exam/${examId}`)}>← Back to Exam</BackButton>

        <Header>
          <Title>📝 Grading Interface: {examInfo.title}</Title>
          <StatsGrid>
            <StatItem>
              <StatLabel>Total Responses</StatLabel>
              <StatValue>{gradingStats.totalStudents}</StatValue>
            </StatItem>
            <StatItem>
              <StatLabel>Graded</StatLabel>
              <StatValue style={{ color: '#d1fae5' }}>
                {gradingStats.totalStudents > 0 
                  ? Math.ceil(gradingStats.gradedResponses / (gradingStats.totalResponses / gradingStats.totalStudents))
                  : 0
                }
              </StatValue>
            </StatItem>
            <StatItem>
              <StatLabel>Pending</StatLabel>
              <StatValue style={{ color: '#fef3c7' }}>
                {gradingStats.totalStudents > 0 
                  ? gradingStats.totalStudents - Math.ceil(gradingStats.gradedResponses / (gradingStats.totalResponses / gradingStats.totalStudents))
                  : 0
                }
              </StatValue>
            </StatItem>
            <StatItem>
              <StatLabel>Progress</StatLabel>
              <StatValue>{gradingPercentage}%</StatValue>
              <ProgressBar>
                <ProgressFill percentage={gradingPercentage} />
              </ProgressBar>
            </StatItem>
          </StatsGrid>
        </Header>

        {gradingStats.pendingResponses === 0 ? (
          <Card>
            <InfoBox>
              ✅ All responses for this exam have been graded! You can view the results or regrade if needed.
            </InfoBox>
          </Card>
        ) : (
          <Card>
            <InfoBox>
              ⏱️ {gradingStats.pendingResponses} responses are still pending grading. Use the grading dashboard to grade them.
            </InfoBox>
          </Card>
        )}

        <Card>
          <h3 style={{ marginTop: 0 }}>Question-wise Statistics</h3>
          {gradingStats.questionStats && gradingStats.questionStats.length > 0 ? (
            <Table>
              <thead>
                <tr>
                  <th>#</th>
                  <th>Question</th>
                  <th>Type</th>
                  <th>Total</th>
                  <th>Graded</th>
                  <th>Pending</th>
                  <th>Avg Marks</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {gradingStats.questionStats.map((q, idx) => {
                  const gradingPercent = q.totalResponses > 0
                    ? ((q.gradedResponses / q.totalResponses) * 100).toFixed(0)
                    : 0

                  return (
                    <tr key={q.questionID}>
                      <td>{idx + 1}</td>
                      <td title={q.questionText} style={{ maxWidth: '300px' }}>
                        {q.questionText?.substring(0, 40)}...
                      </td>
                      <td>{q.questionType}</td>
                      <td>{q.totalResponses}</td>
                      <td style={{ color: '#16a34a', fontWeight: 600 }}>{q.gradedResponses}</td>
                      <td style={{ color: '#dc2626', fontWeight: 600 }}>{q.pendingResponses}</td>
                      <td>
                        {q.averageMarks.toFixed(1)} / {q.maxMarks}
                      </td>
                      <td>
                        <Badge status={q.gradedResponses === q.totalResponses ? 'Graded' : 'Pending'}>
                          {gradingPercent}%
                        </Badge>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </Table>
          ) : (
            <EmptyState>No question statistics available</EmptyState>
          )}
        </Card>

        <Card>
          <h3 style={{ marginTop: 0, marginBottom: '1.5rem' }}>Next Steps</h3>
          <PrimaryButton onClick={() => navigate(`/teacher/exam/${examId}/grading`)}>
            📊 Open Grading Dashboard
          </PrimaryButton>
          <p style={{ marginTop: '1rem', color: '#6b7280', fontSize: '0.95rem' }}>
            Click the button above to access the full grading interface where you can:
          </p>
          <ul style={{ color: '#6b7280', marginLeft: '1.5rem' }}>
            <li>View all pending responses by student or question</li>
            <li>Grade individual responses with feedback</li>
            <li>Batch grade multiple responses</li>
            <li>Regrade responses with reason tracking</li>
            <li>Track grading progress</li>
          </ul>
        </Card>
      </MainContent>
    </Container>
  )
}
