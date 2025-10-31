import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { teacherService, resultService, authService } from '../../services/api'
import ResultPublishModal from './ResultPublishModal'
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

const PageTitle = styled.h1`
  margin: 0 0 2rem 0;
  color: #1f2937;
  font-size: 1.875rem;
`

const Header = styled.div`
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  margin-bottom: 2rem;
`

const ExamInfo = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
`

const InfoCard = styled.div`
  display: flex;
  flex-direction: column;
`

const InfoLabel = styled.span`
  color: #6b7280;
  font-size: 0.875rem;
  margin-bottom: 0.25rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
`

const InfoValue = styled.span`
  color: #1f2937;
  font-size: 1.1rem;
  font-weight: 600;
`

const StatusBadge = styled.span`
  display: inline-block;
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.875rem;
  font-weight: 500;
  width: fit-content;

  ${(props) => {
    if (props.variant === 'published') {
      return `
        background-color: #dcfce7;
        color: #15803d;
      `
    } else if (props.variant === 'draft') {
      return `
        background-color: #e0e7ff;
        color: #3730a3;
      `
    } else {
      return `
        background-color: #fef3c7;
        color: #92400e;
      `
    }
  }}
`

const ContentGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 2rem;
  margin-bottom: 2rem;
`

const Card = styled.div`
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
`

const CardTitle = styled.h3`
  margin: 0 0 1.5rem 0;
  color: #1f2937;
  font-size: 1.125rem;
`

const StatItem = styled.div`
  display: flex;
  justify-content: space-between;
  padding: 1rem 0;
  border-bottom: 1px solid #e5e7eb;

  &:last-child {
    border-bottom: none;
  }
`

const StatLabel = styled.span`
  color: #6b7280;
  font-size: 0.95rem;
`

const StatValue = styled.span`
  color: #1f2937;
  font-weight: 600;
  font-size: 0.95rem;
`

const ProgressBar = styled.div`
  width: 100%;
  height: 8px;
  background-color: #e5e7eb;
  border-radius: 4px;
  overflow: hidden;
  margin: 1rem 0;
`

const ProgressFill = styled.div`
  height: 100%;
  background-color: #3b82f6;
  width: ${(props) => props.percentage}%;
  transition: width 0.3s ease;
`

const TableContainer = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  margin-bottom: 2rem;
`

const Table = styled.table`
  width: 100%;
  border-collapse: collapse;
`

const TableHeader = styled.thead`
  background-color: #f9fafb;
  border-bottom: 2px solid #e5e7eb;
`

const TableRow = styled.tr`
  border-bottom: 1px solid #e5e7eb;

  &:hover {
    background-color: #f9fafb;
  }
`

const TableCell = styled.td`
  padding: 1rem;
  font-size: 0.95rem;
  color: #374151;

  &:first-child {
    padding-left: 1.5rem;
  }

  &:last-child {
    padding-right: 1.5rem;
  }
`

const TableHeaderCell = styled.th`
  text-align: left;
  padding: 1rem;
  font-weight: 600;
  color: #374151;
  font-size: 0.875rem;

  &:first-child {
    padding-left: 1.5rem;
  }

  &:last-child {
    padding-right: 1.5rem;
  }
`

const Button = styled.button`
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  font-size: 0.95rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;

  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
`

const PrimaryButton = styled(Button)`
  background-color: #3b82f6;
  color: white;

  &:hover:not(:disabled) {
    background-color: #2563eb;
  }
`

const SecondaryButton = styled(Button)`
  background-color: #f3f4f6;
  color: #374151;

  &:hover:not(:disabled) {
    background-color: #e5e7eb;
  }
`

const DangerButton = styled(Button)`
  background-color: #dc2626;
  color: white;

  &:hover:not(:disabled) {
    background-color: #b91c1c;
  }
`

const ButtonGroup = styled.div`
  display: flex;
  gap: 1rem;
  margin-top: 2rem;
`

const LoadingSpinner = styled.div`
  display: inline-block;
  width: 20px;
  height: 20px;
  border: 3px solid #f3f3f3;
  border-top: 3px solid #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;

  @keyframes spin {
    0% {
      transform: rotate(0deg);
    }
    100% {
      transform: rotate(360deg);
    }
  }
`

const ErrorMessage = styled.div`
  background-color: #fee;
  border: 1px solid #dc2626;
  color: #991b1b;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
`

const SuccessMessage = styled.div`
  background-color: #dcfce7;
  border: 1px solid #16a34a;
  color: #15803d;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
`

export default function ResultPublishDashboard() {
  const navigate = useNavigate()
  const { examId } = useParams()
  const { user, logout } = useAuth()
  const [exam, setExam] = useState(null)
  const [results, setResults] = useState([])
  const [publicationStatus, setPublicationStatus] = useState(null)
  const [gradingProgress, setGradingProgress] = useState(null)
  const [resultStats, setResultStats] = useState(null)
  const [examStats, setExamStats] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [showPublishModal, setShowPublishModal] = useState(false)

  useEffect(() => {
    loadData()
  }, [examId])

  const loadData = async () => {
    try {
      setLoading(true)
      setError('')

      const [examData, resultsData, statusData, progressData, statsData] = await Promise.all([
        teacherService.getExamById(examId),
        resultService.getExamResults(examId, 1, 100),
        resultService.getPublicationStatus(examId),
        resultService.getGradingProgress(examId),
        teacherService.getExamStatistics(examId),
      ])

      console.log("=== DASHBOARD DATA ===")
      console.log("Exam Data:", examData)
      console.log("Results Data:", resultsData)
      console.log("Status Data:", statusData)
      console.log("Progress Data:", progressData)
      console.log("Statistics Data:", statsData)

      // examData has .data property (from teacherService)
      // resultsData is the array directly (from resultService - already extracted)
      // statusData is the object directly (from resultService - already extracted)
      // progressData is the object directly (from resultService - already extracted)
      // statsData has .data property (from teacherService)
      
      setExam(examData.data)
      setResults(Array.isArray(resultsData) ? resultsData : [])
      setPublicationStatus(statusData)

      // Store exam statistics
      if (statsData && statsData.data) {
        setExamStats(statsData.data)
      }

      // Calculate grading progress from results data (always calculate for consistency)
      if (Array.isArray(resultsData)) {
        if (resultsData.length > 0) {
          const totalStudents = resultsData.length
          const gradedStudents = resultsData.filter(r => r.status === 'Graded').length
          const pendingStudents = totalStudents - gradedStudents
          const gradingProgressPercentage = totalStudents > 0 ? (gradedStudents / totalStudents) * 100 : 0

          console.log("Grading Progress Calculation:", {
            totalStudents,
            gradedStudents,
            pendingStudents,
            gradingProgressPercentage
          })

          setGradingProgress({
            totalStudents,
            gradedStudents,
            pendingStudents,
            gradingProgress: gradingProgressPercentage
          })
        } else {
          // No results yet - set to zero
          setGradingProgress({
            totalStudents: 0,
            gradedStudents: 0,
            pendingStudents: 0,
            gradingProgress: 0
          })
        }
      } else {
        // Fallback to API data if resultsData is not an array
        setGradingProgress(progressData || {
          totalStudents: 0,
          gradedStudents: 0,
          pendingStudents: 0,
          gradingProgress: 0
        })
      }

      // Calculate statistics directly from results data for accuracy
      if (Array.isArray(resultsData) && resultsData.length > 0 && examData.data) {
        const totalMarks = resultsData.reduce((sum, r) => sum + (r.totalMarks || 0), 0)
        const avgMarks = resultsData.length > 0 ? totalMarks / resultsData.length : 0
        const passingMarks = examData.data.passingMarks || 0
        const passedCount = resultsData.filter(
          (r) => (r.totalMarks || 0) >= passingMarks
        ).length

        console.log("Statistics calculation from actual results:", { 
          totalStudents: resultsData.length,
          passingMarks,
          passedCount,
          failedCount: resultsData.length - passedCount,
          averageMarks: avgMarks.toFixed(2)
        })

        setResultStats({
          totalStudents: resultsData.length,
          passedCount,
          failedCount: resultsData.length - passedCount,
          averageMarks: avgMarks.toFixed(2),
        })
      } else {
        setResultStats({
          totalStudents: 0,
          passedCount: 0,
          failedCount: 0,
          averageMarks: '0.00',
        })
      }
    } catch (err) {
      console.error("Error loading data:", err)
      setError(err.message || 'Failed to load data')
    } finally {
      setLoading(false)
    }
  }

  const handlePublishSuccess = () => {
    setSuccess('Results published successfully! Students can now view their results.')
    setTimeout(() => {
      loadData()
    }, 1000)
  }

  const handleLogout = async () => {
    try {
      await authService.logout()
      logout()
      navigate('/login')
    } catch (err) {
      setError('Logout failed')
    }
  }

  if (loading) {
    return (
      <Container>
        <NavBar>
          <NavLeft>
            <Logo onClick={() => navigate('/teacher/dashboard')}>Quiz Portal</Logo>
          </NavLeft>
          <NavMenu>
            <UserInfo>{user?.fullName}</UserInfo>
            <LogoutButton onClick={handleLogout}>Logout</LogoutButton>
          </NavMenu>
        </NavBar>
        <MainContent>
          <div style={{ textAlign: 'center', padding: '3rem' }}>
            <LoadingSpinner /> Loading...
          </div>
        </MainContent>
      </Container>
    )
  }

  return (
    <Container>
      <NavBar>
        <NavLeft>
          <Logo onClick={() => navigate('/teacher/dashboard')}>Quiz Portal</Logo>
        </NavLeft>
        <NavMenu>
          <UserInfo>{user?.fullName}</UserInfo>
          <LogoutButton onClick={handleLogout}>Logout</LogoutButton>
        </NavMenu>
      </NavBar>
      <MainContent>
        <BackButton onClick={() => navigate(`/teacher/exam/${examId}`)}>← Back to Exam</BackButton>
        <PageTitle>Results & Publication</PageTitle>

        {error && <ErrorMessage>{error}</ErrorMessage>}
        {success && <SuccessMessage>✓ {success}</SuccessMessage>}

        {/* Exam & Publication Info */}
        <Header>
          <ExamInfo>
            <InfoCard>
              <InfoLabel>Exam Title</InfoLabel>
              <InfoValue>{exam?.title}</InfoValue>
            </InfoCard>
            <InfoCard>
              <InfoLabel>Publication Status</InfoLabel>
              <StatusBadge variant={publicationStatus?.isPublished ? 'published' : 'draft'}>
                {publicationStatus?.isPublished ? '✓ Published' : 'Not Published'}
              </StatusBadge>
            </InfoCard>
            <InfoCard>
              <InfoLabel>Passing Percentage</InfoLabel>
              <InfoValue>
                {exam?.passingPercentage?.toFixed(2) || publicationStatus?.passingPercentage || 40}%
              </InfoValue>
            </InfoCard>
            <InfoCard>
              <InfoLabel>Published Date</InfoLabel>
              <InfoValue>
                {publicationStatus?.publishedAt
                  ? new Date(publicationStatus.publishedAt).toLocaleDateString()
                  : 'Not yet published'}
              </InfoValue>
            </InfoCard>
          </ExamInfo>
        </Header>

        {/* Content Grid */}
        <ContentGrid>
          {/* Grading Progress Card */}
          <Card>
            <CardTitle>Grading Progress</CardTitle>
            <StatItem>
              <StatLabel>Total Students</StatLabel>
              <StatValue>{gradingProgress?.totalStudents || 0}</StatValue>
            </StatItem>
            <StatItem>
              <StatLabel>Graded Responses</StatLabel>
              <StatValue>{gradingProgress?.gradedStudents || 0}</StatValue>
            </StatItem>
            <StatItem>
              <StatLabel>Pending Responses</StatLabel>
              <StatValue>{gradingProgress?.totalPending || 0}</StatValue>
            </StatItem>
            <ProgressBar>
              <ProgressFill percentage={gradingProgress?.gradingProgress || 0} />
            </ProgressBar>
            <StatItem>
              <StatLabel>Progress</StatLabel>
              <StatValue>{Math.round(gradingProgress?.gradingProgress || 0)}%</StatValue>
            </StatItem>
          </Card>

          {/* Result Statistics Card */}
          <Card>
            <CardTitle>Result Statistics</CardTitle>
            <StatItem>
              <StatLabel>Total Students</StatLabel>
              <StatValue>{resultStats?.totalStudents || examStats?.studentsAttempted || 0}</StatValue>
            </StatItem>
            <StatItem>
              <StatLabel>Passed</StatLabel>
              <StatValue style={{ color: '#16a34a' }}>{resultStats?.passedCount || 0}</StatValue>
            </StatItem>
            <StatItem>
              <StatLabel>Failed</StatLabel>
              <StatValue style={{ color: '#dc2626' }}>{resultStats?.failedCount || 0}</StatValue>
            </StatItem>
            <StatItem>
              <StatLabel>Average Marks</StatLabel>
              <StatValue>{resultStats?.averageMarks || examStats?.averageScore?.toFixed(2) || 0}</StatValue>
            </StatItem>
          </Card>

          {/* Exam Statistics Card */}
          {examStats && (
            <Card>
              <CardTitle>Exam Statistics</CardTitle>
              <StatItem>
                <StatLabel>Students Attempted</StatLabel>
                <StatValue>{examStats.studentsAttempted || 0}</StatValue>
              </StatItem>
              <StatItem>
                <StatLabel>Average Score</StatLabel>
                <StatValue>{examStats.averageScore?.toFixed(2) || 0}</StatValue>
              </StatItem>
              <StatItem>
                <StatLabel>Highest Score</StatLabel>
                <StatValue style={{ color: '#16a34a' }}>{examStats.highestScore || 0}</StatValue>
              </StatItem>
              <StatItem>
                <StatLabel>Lowest Score</StatLabel>
                <StatValue style={{ color: '#dc2626' }}>{examStats.lowestScore || 0}</StatValue>
              </StatItem>
            </Card>
          )}

          {/* Action Card */}
          <Card>
            <CardTitle>Publication</CardTitle>
            <StatItem>
              <StatLabel>Current Status</StatLabel>
              <StatusBadge variant={publicationStatus?.isPublished ? 'published' : 'draft'}>
                {publicationStatus?.isPublished ? 'Published' : 'Draft'}
              </StatusBadge>
            </StatItem>
            <ButtonGroup>
              <PrimaryButton
                onClick={() => setShowPublishModal(true)}
                disabled={publicationStatus?.isPublished}
              >
                {publicationStatus?.isPublished ? 'Already Published' : 'Publish Results'}
              </PrimaryButton>
              {publicationStatus?.isPublished && (
                <DangerButton
                  onClick={async () => {
                    if (window.confirm('Are you sure you want to unpublish these results?')) {
                      try {
                        await resultService.unpublishExam(examId, 'Manually unpublished by teacher')
                        setSuccess('Results unpublished successfully')
                        loadData()
                      } catch (err) {
                        setError(err.message || 'Failed to unpublish results')
                      }
                    }
                  }}
                >
                  Unpublish
                </DangerButton>
              )}
            </ButtonGroup>
          </Card>
        </ContentGrid>

        {/* Results Table */}
        <TableContainer>
          <CardTitle style={{ padding: '1.5rem 1.5rem 0 1.5rem' }}>Student Results</CardTitle>
          <Table>
            <TableHeader>
              <tr>
                <TableHeaderCell>Student Name</TableHeaderCell>
                <TableHeaderCell>Email</TableHeaderCell>
                <TableHeaderCell>Marks</TableHeaderCell>
                <TableHeaderCell>Percentage</TableHeaderCell>
                <TableHeaderCell>Status</TableHeaderCell>
                <TableHeaderCell>Result</TableHeaderCell>
              </tr>
            </TableHeader>
            <tbody>
              {results.length > 0 ? (
                results.map((result) => (
                  <TableRow key={result.resultID}>
                    <TableCell>{result.studentName || 'N/A'}</TableCell>
                    <TableCell>{result.studentEmail || 'N/A'}</TableCell>
                    <TableCell>{result.totalMarks?.toFixed(2) || 0}</TableCell>
                    <TableCell>{result.percentage?.toFixed(2) || 0}%</TableCell>
                    <TableCell>
                      <StatusBadge
                        variant={
                          result.status === 'Graded' ? 'published' : result.status === 'Pending' ? 'draft' : 'draft'
                        }
                      >
                        {result.status}
                      </StatusBadge>
                    </TableCell>
                    <TableCell>
                      <StatusBadge
                        variant={
                          (result.totalMarks || 0) >= (exam?.passingMarks || 0)
                            ? 'published'
                            : 'draft'
                        }
                      >
                        {(result.totalMarks || 0) >= (exam?.passingMarks || 0)
                          ? 'Pass'
                          : 'Fail'}
                      </StatusBadge>
                    </TableCell>
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell colSpan="6" style={{ textAlign: 'center', padding: '2rem' }}>
                    No results available
                  </TableCell>
                </TableRow>
              )}
            </tbody>
          </Table>
        </TableContainer>
      </MainContent>

      {/* Publish Modal */}
      {showPublishModal && (
        <ResultPublishModal
          examId={examId}
          examTitle={exam?.title}
          onClose={() => setShowPublishModal(false)}
          onPublishSuccess={handlePublishSuccess}
        />
      )}
    </Container>
  )
}
