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

const DangerButton = styled(Button)`
  background-color: #dc2626;
  color: white;

  &:hover {
    background-color: #b91c1c;
  }
`

const TabContainer = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
`

const TabButtons = styled.div`
  display: flex;
  border-bottom: 1px solid #e5e7eb;
  padding: 0 2rem;
  gap: 1rem;
`

const TabButton = styled.button`
  padding: 1rem 0;
  border: none;
  background: none;
  cursor: pointer;
  color: #6b7280;
  font-weight: 600;
  border-bottom: 2px solid transparent;
  transition: all 0.3s ease;

  ${(props) => props.$active && `
    color: #3b82f6;
    border-bottom-color: #3b82f6;
  `}

  &:hover {
    color: #1f2937;
  }
`

const TabContent = styled.div`
  padding: 2rem;
  ${(props) => !props.$active && 'display: none;'}
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

const InfoBox = styled.div`
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
  font-size: 0.95rem;
  font-weight: 500;
  border-left: 4px solid;
`

const UpcomingInfoBox = styled(InfoBox)`
  background-color: #fef3c7;
  border-left-color: #92400e;
  color: #92400e;
`

const ActiveWarningBox = styled(InfoBox)`
  background-color: #fecaca;
  border-left-color: #dc2626;
  color: #991b1b;
`

const EndedWarningBox = styled(InfoBox)`
  background-color: #fee;
  border-left-color: #991b1b;
  color: #991b1b;
`

export default function ExamDetail() {
  const navigate = useNavigate()
  const { examId } = useParams()
  const { user, logout } = useAuth()

  const [activeTab, setActiveTab] = useState('overview')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [exam, setExam] = useState(null)
  const [questions, setQuestions] = useState([])
  const [attempts, setAttempts] = useState([])
  const [statistics, setStatistics] = useState(null)

  useEffect(() => {
    loadExamData()
  }, [examId])

  useEffect(() => {
    if (activeTab === 'questions' && questions.length === 0) {
      loadQuestions()
    } else if (activeTab === 'attempts' && attempts.length === 0) {
      loadAttempts()
    } else if (activeTab === 'statistics' && !statistics) {
      loadStatistics()
    }
  }, [activeTab])

  const loadExamData = async () => {
    try {
      setLoading(true)
      const response = await teacherService.getExamById(examId)
      setExam(response.data)
      setError('')
    } catch (err) {
      setError('Failed to load exam details')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  const loadQuestions = async () => {
    try {
      const response = await teacherService.getQuestions(examId)
      setQuestions(response.data || [])
    } catch (err) {
      setError('Failed to load questions')
    }
  }

  const loadAttempts = async () => {
    try {
      const response = await teacherService.getStudentAttempts(examId)
      
      setAttempts(response.data || [])
    } catch (err) {
      setError('Failed to load attempts')
    }
  }

  const loadStatistics = async () => {
    try {
      const response = await teacherService.getExamStatistics(examId)
      setStatistics(response.data || {})
    } catch (err) {
      setError('Failed to load statistics')
    }
  }

  const handleEditExam = () => {
    navigate(`/teacher/edit-exam/${examId}`)
  }

  const handleAddQuestion = () => {
    navigate(`/teacher/exam/${examId}/add-question`)
  }

  const handleViewStudentResponses = (studentId, studentName) => {
    navigate(`/teacher/exam/${examId}/student/${studentId}`, { state: { studentName } })
  }

  const handleDeleteQuestion = async (questionId) => {
    try {
      await teacherService.deleteQuestion(examId, questionId)
      // Reload questions after deletion
      await loadQuestions()
      setError('')
    } catch (err) {
      setError('Failed to delete question: ' + (err.message || 'Unknown error'))
      console.error(err)
    }
  }

  const handleDeleteExam = async () => {
    if (!window.confirm('Are you sure you want to delete this exam? This action cannot be undone.')) {
      return
    }

    try {
      await teacherService.deleteExam(examId)
      navigate('/teacher/dashboard')
    } catch (err) {
      setError('Failed to delete exam')
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
  const isUpcoming = examStatus === 'Upcoming'

  if (loading) {
    return (
      <Container>
        <NavBar>
          <NavLeft>
            <Logo onClick={() => navigate('/teacher/dashboard')}>Quiz Portal</Logo>
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
            <Logo onClick={() => navigate('/teacher/dashboard')}>Quiz Portal</Logo>
          </NavLeft>
        </NavBar>
        <MainContent>
          <ErrorMessage>Exam not found</ErrorMessage>
          <BackButton onClick={() => navigate('/teacher/dashboard')}>← Back to Dashboard</BackButton>
        </MainContent>
      </Container>
    )
  }

  const getExamStatus = () => {
    const now = new Date()
    const start = new Date(exam.scheduleStart)
    const end = new Date(exam.scheduleEnd)

    if (now < start) return 'Upcoming'
    if (now > end) return 'Ended'
    return 'Active'
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
          <Logo onClick={() => navigate('/teacher/dashboard')}>Quiz Portal</Logo>
        </NavLeft>
        <NavMenu>
          <span>{user?.fullName} ({user?.role})</span>
          <LogoutButton onClick={handleLogout}>Logout</LogoutButton>
        </NavMenu>
      </NavBar>

      <MainContent>
        <BackButton onClick={() => navigate('/teacher/dashboard')}>← Back to Dashboard</BackButton>

        {error && <ErrorMessage>{error}</ErrorMessage>}

        <Header>
          <PageTitle>{exam.title}</PageTitle>
          <ExamInfo>
            <InfoItem>
              <InfoLabel>Status</InfoLabel>
              <InfoValue style={{ color: getStatusColor(getExamStatus()) }}>
                {getExamStatus()}
              </InfoValue>
            </InfoItem>
            <InfoItem>
              <InfoLabel>Passing Percentage</InfoLabel>
              <InfoValue>{exam.passingPercentage?.toFixed(2)}%</InfoValue>
            </InfoItem>
            <InfoItem>
              <InfoLabel>Total Marks</InfoLabel>
              <InfoValue>{exam.totalMarks || 0} (from questions)</InfoValue>
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
              <InfoValue>{exam.accessCode}</InfoValue>
            </InfoItem>
            <InfoItem>
              <InfoLabel>Start Time</InfoLabel>
              <InfoValue>{new Date(exam.scheduleStart).toLocaleString()}</InfoValue>
            </InfoItem>
          </ExamInfo>

          <ActionButtons>
            {isUpcoming ? (
              <>
                <PrimaryButton onClick={handleEditExam}>Edit Exam</PrimaryButton>
                <DangerButton onClick={handleDeleteExam}>Delete Exam</DangerButton>
              </>
            ) : (
              <div style={{ fontSize: '0.95rem', color: '#6b7280', fontWeight: 500 }}>
                Exam cannot be edited after it has started. Only upcoming exams can be modified.
              </div>
            )}
            {examStatus === 'Ended' && (
              <PrimaryButton onClick={() => navigate(`/teacher/exam/${examId}/results`)}>
                📊 View & Publish Results
              </PrimaryButton>
            )}
            <SecondaryButton onClick={() => navigate('/teacher/dashboard')}>Back to Dashboard</SecondaryButton>
          </ActionButtons>
        </Header>

        <TabContainer>
          <TabButtons>
            <TabButton $active={activeTab === 'overview'} onClick={() => setActiveTab('overview')}>
              Overview
            </TabButton>
            <TabButton $active={activeTab === 'questions'} onClick={() => setActiveTab('questions')}>
              Questions
            </TabButton>
            <TabButton $active={activeTab === 'attempts'} onClick={() => setActiveTab('attempts')}>
              Student Attempts
            </TabButton>
            <TabButton $active={activeTab === 'statistics'} onClick={() => setActiveTab('statistics')}>
              Statistics
            </TabButton>
            {examStatus === 'Ended' && (
              <>
                <TabButton $active={activeTab === 'grading'} onClick={() => setActiveTab('grading')}>
                  📝 Grade Responses
                </TabButton>
                <TabButton $active={activeTab === 'results'} onClick={() => setActiveTab('results')}>
                  📊 Results & Publishing
                </TabButton>
              </>
            )}
          </TabButtons>

          {/* Overview Tab */}
          <TabContent $active={activeTab === 'overview'}>
            <Card>
              <h3>Exam Description</h3>
              <p>{exam.description || 'No description provided'}</p>
            </Card>

            {exam.batchRemark && (
              <Card>
                <h3>Batch/Class Remark</h3>
                <p>{exam.batchRemark}</p>
              </Card>
            )}

            <Card>
              <h3>Exam Configuration</h3>
              <ExamInfo>
                <InfoItem>
                  <InfoLabel>Allow Negative Marks</InfoLabel>
                  <InfoValue>{exam.allowNegativeMarks ? 'Yes' : 'No'}</InfoValue>
                </InfoItem>
                {exam.allowNegativeMarks && (
                  <InfoItem>
                    <InfoLabel>Negative Mark %</InfoLabel>
                    <InfoValue>{exam.negativeMarkPercentage}%</InfoValue>
                  </InfoItem>
                )}
              </ExamInfo>
            </Card>
          </TabContent>

          {/* Questions Tab */}
          <TabContent $active={activeTab === 'questions'}>
            {/* Status-based warning/info box */}
            {examStatus === 'Upcoming' && (
              <UpcomingInfoBox>
                ✓ This exam is <strong>upcoming</strong>. You can add, edit, and delete questions until the exam starts.
              </UpcomingInfoBox>
            )}
            {examStatus === 'Active' && (
              <ActiveWarningBox>
                ⏱️ This exam is currently <strong>ACTIVE</strong>. Questions cannot be modified while the exam is in progress. Students are currently taking the exam.
              </ActiveWarningBox>
            )}
            {examStatus === 'Ended' && (
              <EndedWarningBox>
                ✓ This exam has <strong>ENDED</strong>. Questions cannot be modified after the exam has concluded. You can now review student responses and grade answers.
              </EndedWarningBox>
            )}

            <div style={{ marginBottom: '1.5rem', marginTop: '1.5rem' }}>
              <PrimaryButton 
                onClick={handleAddQuestion}
                disabled={!isUpcoming}
                title={!isUpcoming ? `Questions can only be added to upcoming exams (current status: ${examStatus})` : 'Add a new question to this exam'}
                style={{
                  opacity: !isUpcoming ? 0.5 : 1,
                  cursor: !isUpcoming ? 'not-allowed' : 'pointer'
                }}
              >
                + Add Question
              </PrimaryButton>
            </div>

            {questions.length === 0 ? (
              <EmptyState>
                <p>No questions added yet. Start by adding questions to this exam.</p>
                {isUpcoming && (
                  <PrimaryButton onClick={handleAddQuestion} style={{ marginTop: '1rem' }}>
                    Add First Question
                  </PrimaryButton>
                )}
              </EmptyState>
            ) : (
              <Card>
                <Table>
                  <thead>
                    <tr>
                      <th>Question</th>
                      <th>Type</th>
                      <th>Marks</th>
                      <th>Options</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {questions.map((q, index) => (
                      <tr key={q.questionID}>
                        <td title={q.questionText}>
                          {index + 1}. {q.questionText && q.questionText.substring(0, 50)}
                          {q.questionText && q.questionText.length > 50 ? '...' : ''}
                        </td>
                        <td>{q.questionType === 0 ? 'MCQ' : q.questionType === 1 ? 'SAQ' : 'Subjective'}</td>
                        <td>{q.marks}</td>
                        <td>{q.optionCount || 0}</td>
                        <td>
                          <Button 
                            onClick={() => navigate(`/teacher/exam/${examId}/question/${q.questionID}/edit`)}
                            disabled={!isUpcoming}
                            title={!isUpcoming ? `Cannot edit questions for ${examStatus} exams` : 'Edit this question'}
                            style={{
                              opacity: !isUpcoming ? 0.5 : 1,
                              cursor: !isUpcoming ? 'not-allowed' : 'pointer',
                              marginRight: '0.5rem'
                            }}
                          >
                            Edit
                          </Button>
                          <Button 
                            onClick={() => {
                              if (window.confirm('Are you sure you want to delete this question?')) {
                                handleDeleteQuestion(q.questionID)
                              }
                            }}
                            disabled={!isUpcoming}
                            title={!isUpcoming ? `Cannot delete questions for ${examStatus} exams` : 'Delete this question'}
                            style={{
                              opacity: !isUpcoming ? 0.5 : 1,
                              cursor: !isUpcoming ? 'not-allowed' : 'pointer',
                              backgroundColor: '#ef4444',
                              color: 'white'
                            }}
                          >
                            Delete
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </Card>
            )}
          </TabContent>

          {/* Attempts Tab */}
          <TabContent $active={activeTab === 'attempts'}>
            {attempts.length === 0 ? (
              <EmptyState>
                <p>No student attempts yet</p>
              </EmptyState>
            ) : (
              <Card>
                <Table>
                  <thead>
                    <tr>
                      <th>Student Name</th>
                      <th>Email</th>
                      <th>Attempted</th>
                      <th>Submitted</th>
                      <th>Score</th>
                      <th>Status</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {attempts.map((attempt) => (
                      <tr key={attempt.studentId}>
                        <td>{attempt.studentName}</td>
                        <td>{attempt.studentEmail}</td>
                        <td>{attempt.answeredQuestions > 0 ? 'Yes' : 'No'}</td>
                        <td>{attempt.isSubmitted ? 'Yes' : 'No'}</td>
                        <td>{attempt.score ? `${attempt.score}/${exam.totalMarks}` : 'N/A'}</td>
                        <td>{attempt.status || 'Pending'}</td>
                        <td>
                          <Button
                            onClick={() => handleViewStudentResponses(attempt.studentId, attempt.studentName)}
                          >
                            View Responses
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </Card>
            )}
          </TabContent>

          {/* Statistics Tab */}
          <TabContent $active={activeTab === 'statistics'}>
            {statistics ? (
              <>
                <Grid>
                  <StatCard>
                    <StatLabel>Total Attempts</StatLabel>
                    <StatValue>{statistics.studentsAttempted || 0}</StatValue>
                  </StatCard>
                  <StatCard>
                    <StatLabel>Average Score</StatLabel>
                    <StatValue>
                      {statistics.averageScore !== undefined ? statistics.averageScore.toFixed(2) : 0}
                    </StatValue>
                  </StatCard>
                  <StatCard>
                    <StatLabel>Highest Score</StatLabel>
                    <StatValue>{statistics.highestScore || 0}</StatValue>
                  </StatCard>
                  <StatCard>
                    <StatLabel>Lowest Score</StatLabel>
                    <StatValue>{statistics.lowestScore || 0}</StatValue>
                  </StatCard>
                </Grid>

                <Card>
                  <h3>Pass Statistics</h3>
                  <ExamInfo>
                    <InfoItem>
                      <InfoLabel>Students Passed</InfoLabel>
                      <InfoValue>
                        {statistics.studentsAttempted > 0 
                          ? Math.ceil((statistics.studentsAttempted * statistics.passPercentage) / 100)
                          : 0}
                      </InfoValue>
                    </InfoItem>
                    <InfoItem>
                      <InfoLabel>Students Failed</InfoLabel>
                      <InfoValue>
                        {statistics.studentsAttempted > 0 
                          ? statistics.studentsAttempted - Math.ceil((statistics.studentsAttempted * statistics.passPercentage) / 100)
                          : 0}
                      </InfoValue>
                    </InfoItem>
                    <InfoItem>
                      <InfoLabel>Pass Rate</InfoLabel>
                      <InfoValue>{statistics.passPercentage ? statistics.passPercentage.toFixed(2) : 0}%</InfoValue>
                    </InfoItem>
                  </ExamInfo>
                </Card>
              </>
            ) : (
              <LoadingSpinner>Loading statistics...</LoadingSpinner>
            )}
          </TabContent>

          {/* Grading Tab */}
          {examStatus === 'Ended' && (
            <TabContent $active={activeTab === 'grading'}>
              <Card>
                <h3>Grade Student Responses</h3>
                <p style={{ color: '#6b7280', marginBottom: '1.5rem' }}>
                  View all students who attempted the exam and grade their responses. Select a student to see all their answers organized by question and provide feedback.
                </p>
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '1rem' }}>
                  <div style={{ padding: '1.5rem', border: '2px solid #3b82f6', borderRadius: '8px', backgroundColor: '#eff6ff' }}>
                    <h4 style={{ margin: '0 0 0.5rem 0', color: '#1f2937' }}>👥 Grade Student Responses</h4>
                    <p style={{ color: '#6b7280', fontSize: '0.9rem', margin: '0.5rem 0 1rem 0' }}>
                      Access the comprehensive grading dashboard to review and grade all student responses for this exam.
                    </p>
                    <PrimaryButton 
                      onClick={() => navigate(`/teacher/exam/${examId}/grade-by-student`)}
                      style={{ width: '100%' }}
                    >
                      📋 Open Grading Dashboard
                    </PrimaryButton>
                  </div>
                </div>
              </Card>
            </TabContent>
          )}

          {/* Results & Publishing Tab */}
          {examStatus === 'Ended' && (
            <TabContent $active={activeTab === 'results'}>
              <Card>
                <h3>Results & Publishing</h3>
                <p style={{ color: '#6b7280', marginBottom: '1.5rem' }}>
                  View student results, grading progress, and publish results so students can view their marks and feedback.
                </p>
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '1rem' }}>
                  <div style={{ padding: '1.5rem', border: '2px solid #3b82f6', borderRadius: '8px', backgroundColor: '#eff6ff' }}>
                    <h4 style={{ margin: '0 0 0.5rem 0', color: '#1f2937' }}>📊 Manage Results</h4>
                    <p style={{ color: '#6b7280', fontSize: '0.9rem', margin: '0.5rem 0 1rem 0' }}>
                      View detailed results, publishing status, and grading progress. Publish results to make them visible to students.
                    </p>
                    <PrimaryButton 
                      onClick={() => navigate(`/teacher/exam/${examId}/results`)}
                      style={{ width: '100%' }}
                    >
                      📈 Results Dashboard
                    </PrimaryButton>
                  </div>
                </div>
              </Card>
            </TabContent>
          )}
        </TabContainer>
      </MainContent>
    </Container>
  )
}
