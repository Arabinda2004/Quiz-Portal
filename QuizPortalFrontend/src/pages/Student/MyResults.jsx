import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { resultService, authService } from '../../services/api'
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

const UserInfo = styled.span`
  font-size: 0.95rem;
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

const PageTitle = styled.h1`
  margin: 0 0 0.5rem 0;
  color: #1f2937;
  font-size: 1.875rem;
`

const PageSubtitle = styled.p`
  margin: 0 0 2rem 0;
  color: #6b7280;
  font-size: 1rem;
`

const ResultsGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1.5rem;
  margin-top: 2rem;
`

const ResultCard = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  padding: 1.5rem;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  cursor: pointer;

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  }
`

const CardHeader = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: start;
  margin-bottom: 1rem;
`

const ExamTitle = styled.h3`
  margin: 0;
  color: #1f2937;
  font-size: 1.125rem;
  font-weight: 600;
`

const StatusBadge = styled.span`
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  background-color: ${props => {
    if (props.variant === 'pass') return '#dcfce7';
    if (props.variant === 'fail') return '#fee2e2';
    return '#e0e7ff';
  }};
  color: ${props => {
    if (props.variant === 'pass') return '#15803d';
    if (props.variant === 'fail') return '#991b1b';
    return '#3730a3';
  }};
`

const ScoreCircle = styled.div`
  width: 120px;
  height: 120px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  margin: 1.5rem auto;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
`

const ScorePercentage = styled.div`
  font-size: 2rem;
  font-weight: bold;
  color: white;
`

const ScoreLabel = styled.div`
  font-size: 0.75rem;
  color: rgba(255, 255, 255, 0.9);
  margin-top: 0.25rem;
`

const ResultDetails = styled.div`
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e5e7eb;
`

const DetailItem = styled.div`
  display: flex;
  flex-direction: column;
`

const DetailLabel = styled.span`
  font-size: 0.75rem;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-bottom: 0.25rem;
`

const DetailValue = styled.span`
  font-size: 0.95rem;
  color: #1f2937;
  font-weight: 600;
`

const ViewButton = styled.button`
  width: 100%;
  padding: 0.75rem;
  margin-top: 1rem;
  background-color: #3b82f6;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.2s ease;

  &:hover {
    background-color: #2563eb;
  }
`

const LoadingSpinner = styled.div`
  display: inline-block;
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;

  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
`

const EmptyState = styled.div`
  text-align: center;
  padding: 3rem 2rem;
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
`

const EmptyStateIcon = styled.div`
  font-size: 3rem;
  margin-bottom: 1rem;
`

const EmptyStateTitle = styled.h3`
  margin: 0 0 0.5rem 0;
  color: #1f2937;
  font-size: 1.25rem;
`

const EmptyStateText = styled.p`
  margin: 0;
  color: #6b7280;
  font-size: 0.95rem;
`

const ErrorMessage = styled.div`
  background-color: #fee;
  border: 1px solid #dc2626;
  color: #991b1b;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
`

export default function MyResults() {
  const navigate = useNavigate()
  const { user, logout } = useAuth()
  const [results, setResults] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    loadResults()
  }, [])

  const loadResults = async () => {
    try {
      setLoading(true)
      setError('')
      
      const data = await resultService.getPublishedResults()
      console.log('Published Results:', data)
      console.log('Published Results - Full Data:', JSON.stringify(data, null, 2))
      
      // Validate data is array
      if (!Array.isArray(data)) {
        console.warn('Expected array but got:', typeof data)
      }
      
      setResults(Array.isArray(data) ? data : [])
    } catch (err) {
      console.error('Error loading results:', err)
      setError(err.message || 'Failed to load results')
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
    }
  }

  const handleViewDetails = (examId) => {
    navigate(`/student/result/${examId}`)
  }

  const getGrade = (percentage) => {
    if (percentage >= 90) return 'A++'
    if (percentage >= 80) return 'A+'
    if (percentage >= 70) return 'B'
    if (percentage >= 60) return 'C'
    if (percentage >= 50) return 'D'
    return 'F'
  }

  return (
    <Container>
      <NavBar>
        <NavLeft>
          <Logo onClick={() => navigate('/student/dashboard')}>Quiz Portal</Logo>
        </NavLeft>
        <NavMenu>
          <UserInfo>{user?.fullName}</UserInfo>
          <LogoutButton onClick={handleLogout}>Logout</LogoutButton>
        </NavMenu>
      </NavBar>

      <MainContent>
        <PageTitle>My Results</PageTitle>
        <PageSubtitle>View your published exam results and performance</PageSubtitle>

        {error && <ErrorMessage>{error}</ErrorMessage>}

        {loading ? (
          <div style={{ textAlign: 'center', padding: '3rem' }}>
            <LoadingSpinner />
            <p style={{ marginTop: '1rem', color: '#6b7280' }}>Loading your results...</p>
          </div>
        ) : results.length === 0 ? (
          <EmptyState>
            <EmptyStateIcon>📊</EmptyStateIcon>
            <EmptyStateTitle>No Results Available</EmptyStateTitle>
            <EmptyStateText>
              You don't have any published results yet. Results will appear here once your teacher publishes them.
            </EmptyStateText>
          </EmptyState>
        ) : (
          <ResultsGrid>
            {results.map((result) => {
              const percentage = result.percentage || 0
              const totalMarks = result.totalMarks || 0
              const examTotalMarks = result.examTotalMarks || 0
              const isPassed = percentage >= result.passingPercentage
              
              // Debug log for each result
              console.log(`Result ${result.resultID}:`, {
                examName: result.examName,
                percentage,
                totalMarks,
                examTotalMarks,
                status: result.status,
                isPassed
              })
              
              return (
                <ResultCard key={result.resultID} onClick={() => handleViewDetails(result.examID)}>
                  <CardHeader>
                    <ExamTitle>{result.examName || 'Untitled Exam'}</ExamTitle>
                    <StatusBadge variant={isPassed ? 'pass' : 'fail'}>
                      {isPassed ? '✓ Passed' : '✗ Failed'}
                    </StatusBadge>
                  </CardHeader>

                  <ScoreCircle>
                    <ScorePercentage>{percentage.toFixed(1)}%</ScorePercentage>
                    <ScoreLabel>Grade: {getGrade(percentage)}</ScoreLabel>
                  </ScoreCircle>

                  <ResultDetails>
                    <DetailItem>
                      <DetailLabel>Total Marks</DetailLabel>
                      <DetailValue>
                        {totalMarks.toFixed(2)} / {examTotalMarks || 'N/A'}
                      </DetailValue>
                    </DetailItem>
                    <DetailItem>
                      <DetailLabel>Rank</DetailLabel>
                      <DetailValue>#{result.rank || 'N/A'}</DetailValue>
                    </DetailItem>
                    <DetailItem>
                      <DetailLabel>Status</DetailLabel>
                      <DetailValue>{result.status || 'N/A'}</DetailValue>
                    </DetailItem>
                    <DetailItem>
                      <DetailLabel>Published</DetailLabel>
                      <DetailValue>
                        {result.publishedAt 
                          ? new Date(result.publishedAt).toLocaleDateString() 
                          : 'N/A'}
                      </DetailValue>
                    </DetailItem>
                  </ResultDetails>

                  <ViewButton onClick={(e) => {
                    e.stopPropagation()
                    handleViewDetails(result.examID)
                  }}>
                    View Detailed Results
                  </ViewButton>
                </ResultCard>
              )
            })}
          </ResultsGrid>
        )}
      </MainContent>
    </Container>
  )
}
