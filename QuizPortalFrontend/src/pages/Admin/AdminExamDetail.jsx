import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { adminService } from '../../services/api'
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
  max-width: 1200px;
  margin: 0 auto;
`

const BackButton = styled.button`
  background: none;
  border: none;
  color: #3b82f6;
  cursor: pointer;
  font-size: 0.95rem;
  margin-bottom: 1rem;
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

const PageTitle = styled.h1`
  margin: 0 0 1rem 0;
  font-size: 2rem;
  color: #1f2937;
`

const ExamInfo = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
  margin-top: 1.5rem;
`

const InfoItem = styled.div`
  display: flex;
  flex-direction: column;
`

const InfoLabel = styled.span`
  color: #6b7280;
  font-size: 0.875rem;
  font-weight: 600;
  margin-bottom: 0.25rem;
`

const InfoValue = styled.span`
  color: #1f2937;
  font-size: 1rem;
  font-weight: 500;
`

const ActionButtons = styled.div`
  display: flex;
  gap: 0.75rem;
  margin-top: 1.5rem;
  flex-wrap: wrap;
`

const Button = styled.button`
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;

  &:hover {
    opacity: 0.9;
  }
`

const PrimaryButton = styled(Button)`
  background-color: #3b82f6;
  color: white;

  &:hover {
    background-color: #2563eb;
  }
`

const SecondaryButton = styled(Button)`
  background-color: #e5e7eb;
  color: #1f2937;

  &:hover {
    background-color: #d1d5db;
  }
`

const Card = styled.div`
  background: white;
  border-radius: 8px;
  padding: 1.5rem;
  margin-bottom: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
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

const StatCard = styled.div`
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

const EmptyState = styled.div`
  text-align: center;
  padding: 3rem 1rem;
  color: #6b7280;
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

const Grid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
`

export default function AdminExamDetail() {
  const navigate = useNavigate()
  const { examId } = useParams()

  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [exam, setExam] = useState(null)
  const [statistics, setStatistics] = useState(null)
  const [statsLoading, setStatsLoading] = useState(false)

  useEffect(() => {
    loadExamData()
  }, [examId])

  useEffect(() => {
    // Load statistics when component mounts (after exam data is loaded) for the overview card
    if (exam && !statistics) {
      loadStatistics()
    }
  }, [exam])

  const loadExamData = async () => {
    try {
      setLoading(true)
      const response = await adminService.getExamById(examId)
      setExam(response.data)
      setError('')
    } catch (err) {
      setError('Failed to load exam details')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  const loadStatistics = async () => {
    try {
      setStatsLoading(true)
      const response = await adminService.getExamStatistics(examId)
      // The API returns { data: {...} } structure, so extract the data properly
      const statsData = response.data || response || {}
      setStatistics(statsData)
      setError('')
    } catch (err) {
      console.error('Error loading statistics:', err)
      setError('Failed to load statistics')
    } finally {
      setStatsLoading(false)
    }
  }

  // Calculate exam status
  const calculateExamStatus = () => {
    if (!exam) return 'Upcoming'
    const now = new Date()
    const start = new Date(exam.scheduleStart)
    const end = new Date(exam.scheduleEnd)

    if (now < start) return 'Upcoming'
    if (now > end) return 'Ended'
    return 'Active'
  }

  const examStatus = calculateExamStatus()

  if (loading) {
    return (
      <Container>
        <NavBar>
          <NavLeft>
            <Logo onClick={() => navigate('/admin/dashboard')}>Quiz Portal</Logo>
          </NavLeft>
        </NavBar>
        <MainContent>
          <LoadingSpinner>Loading exam details...</LoadingSpinner>
        </MainContent>
      </Container>
    )
  }

  if (!exam) {
    return (
      <Container>
        <NavBar>
          <NavLeft>
            <Logo onClick={() => navigate('/admin/dashboard')}>Quiz Portal</Logo>
          </NavLeft>
        </NavBar>
        <MainContent>
          <ErrorMessage>Exam not found</ErrorMessage>
          <BackButton onClick={() => navigate('/admin/exams')}>← Back to Exam Management</BackButton>
        </MainContent>
      </Container>
    )
  }

  const getStatusColor = (status) => {
    switch (status) {
      case 'Active':
        return '#065f46'
      case 'Upcoming':
        return '#92400e'
      case 'Ended':
        return '#991b1b'
      default:
        return '#6b7280'
    }
  }

  return (
    <Container>
      <NavBar>
        <NavLeft>
          <Logo onClick={() => navigate('/admin/dashboard')}>Quiz Portal</Logo>
        </NavLeft>
        <NavMenu>
          <span>Admin</span>
        </NavMenu>
      </NavBar>

      <MainContent>
        <BackButton onClick={() => navigate('/admin/exams')}>← Back to Exam Management</BackButton>

        {error && <ErrorMessage>{error}</ErrorMessage>}

        <Header>
          <PageTitle>{exam.title}</PageTitle>
          <ExamInfo>
            <InfoItem>
              <InfoLabel>Status</InfoLabel>
              <InfoValue style={{ color: getStatusColor(examStatus) }}>
                {examStatus}
              </InfoValue>
            </InfoItem>
            <InfoItem>
              <InfoLabel>Created By</InfoLabel>
              <InfoValue>{exam.createdByUserName || 'Unknown'}</InfoValue>
            </InfoItem>
            <InfoItem>
              <InfoLabel>Total Marks</InfoLabel>
              <InfoValue>{exam.totalMarks || 0}</InfoValue>
            </InfoItem>
            <InfoItem>
              <InfoLabel>Passing Percentage</InfoLabel>
              <InfoValue>{exam.passingPercentage?.toFixed(2)}%</InfoValue>
            </InfoItem>
            <InfoItem>
              <InfoLabel>Passing Marks</InfoLabel>
              <InfoValue>{exam.passingMarks?.toFixed(2) || 0}</InfoValue>
            </InfoItem>
            <InfoItem>
              <InfoLabel>Duration</InfoLabel>
              <InfoValue>{exam.durationMinutes} minutes</InfoValue>
            </InfoItem>
            <InfoItem>
              <InfoLabel>Access Code</InfoLabel>
              <InfoValue style={{ fontFamily: 'monospace', fontSize: '1.1rem' }}>{exam.accessCode}</InfoValue>
            </InfoItem>
            <InfoItem>
              <InfoLabel>Created At</InfoLabel>
              <InfoValue>{new Date(exam.createdAt).toLocaleString()}</InfoValue>
            </InfoItem>
            <InfoItem>
              <InfoLabel>Start Time</InfoLabel>
              <InfoValue>{new Date(exam.scheduleStart).toLocaleString()}</InfoValue>
            </InfoItem>
            <InfoItem>
              <InfoLabel>End Time</InfoLabel>
              <InfoValue>{new Date(exam.scheduleEnd).toLocaleString()}</InfoValue>
            </InfoItem>
          </ExamInfo>
        </Header>

        <Card>
          <h3>Quick Statistics</h3>
          {statsLoading ? (
            <LoadingSpinner style={{ padding: '2rem' }}>Loading statistics...</LoadingSpinner>
          ) : statistics && statistics.studentsAttempted > 0 ? (
            <>
              <ExamInfo>
                <InfoItem>
                  <InfoLabel>📊 Total Attempts</InfoLabel>
                  <InfoValue>{statistics.studentsAttempted || 0}</InfoValue>
                </InfoItem>
                <InfoItem>
                  <InfoLabel>📈 Average Score</InfoLabel>
                  <InfoValue>{statistics.averageScore !== undefined ? statistics.averageScore.toFixed(2) : 0}</InfoValue>
                </InfoItem>
                <InfoItem>
                  <InfoLabel>🏆 Highest Score</InfoLabel>
                  <InfoValue>{statistics.highestScore || 0}</InfoValue>
                </InfoItem>
                <InfoItem>
                  <InfoLabel>📉 Lowest Score</InfoLabel>
                  <InfoValue>{statistics.lowestScore || 0}</InfoValue>
                </InfoItem>
                <InfoItem>
                  <InfoLabel>✅ Pass Rate</InfoLabel>
                  <InfoValue style={{ color: '#10b981' }}>{statistics.passPercentage ? statistics.passPercentage.toFixed(2) : 0}%</InfoValue>
                </InfoItem>
              </ExamInfo>
            </>
          ) : (
            <p style={{ color: '#6b7280' }}>No statistics available yet. Statistics will appear once students start attempting the exam.</p>
          )}
        </Card>
      </MainContent>
    </Container>
  )
}
