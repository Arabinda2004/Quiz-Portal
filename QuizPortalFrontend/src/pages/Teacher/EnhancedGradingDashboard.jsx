import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { teacherService } from '../../services/api'
import styled from 'styled-components'
import GradingResponseDetail from '../../components/GradingResponseDetail'
import BatchGradingModal from '../../components/BatchGradingModal'
import GradingProgress from '../../components/GradingProgress'

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
  margin-bottom: 1rem;
  padding: 0;

  &:hover {
    text-decoration: underline;
  }
`

const PageTitle = styled.h1`
  font-size: 2rem;
  color: #1f2937;
  margin-bottom: 1.5rem;
`

const TabBar = styled.div`
  display: flex;
  gap: 0.5rem;
  margin-bottom: 2rem;
  background: white;
  padding: 0.5rem;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  flex-wrap: wrap;
`

const Tab = styled.button`
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  background-color: ${(props) => (props.$active ? '#3b82f6' : '#e5e7eb')};
  color: ${(props) => (props.$active ? 'white' : '#374151')};
  cursor: pointer;
  font-weight: 600;
  font-size: 0.95rem;
  transition: all 0.3s ease;

  &:hover {
    background-color: ${(props) => (props.$active ? '#2563eb' : '#d1d5db')};
  }
`

const FilterBar = styled.div`
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
`

const FilterGroup = styled.div`
  display: flex;
  flex-direction: column;
`

const FilterLabel = styled.label`
  font-weight: 600;
  color: #374151;
  margin-bottom: 0.5rem;
  font-size: 0.875rem;
`

const Select = styled.select`
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 4px;
  font-size: 0.95rem;
  background-color: white;

  &:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }
`

const Card = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  margin-bottom: 1.5rem;
`

const CardHeader = styled.div`
  padding: 1.5rem;
  background-color: #f3f4f6;
  border-bottom: 1px solid #e5e7eb;
  display: flex;
  justify-content: space-between;
  align-items: center;
`

const CardTitle = styled.h3`
  margin: 0;
  color: #1f2937;
  font-size: 1.1rem;
`

const CardBody = styled.div`
  padding: 1.5rem;
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
    font-size: 0.95rem;
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

const SecondaryButton = styled(Button)`
  background-color: #e5e7eb;
  color: #1f2937;

  &:hover {
    background-color: #d1d5db;
  }
`

const Badge = styled.span`
  display: inline-block;
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  background-color: ${(props) => {
    switch (props.$status) {
      case 'Graded':
        return '#d1fae5'
      case 'Pending':
        return '#fef3c7'
      default:
        return '#e5e7eb'
    }
  }};
  color: ${(props) => {
    switch (props.$status) {
      case 'Graded':
        return '#065f46'
      case 'Pending':
        return '#92400e'
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

const EmptyState = styled.div`
  text-align: center;
  padding: 3rem 1rem;
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

const SuccessMessage = styled.div`
  background-color: #ecfdf5;
  border-left: 4px solid #065f46;
  color: #065f46;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
`

const ActionCell = styled.div`
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
`

export default function EnhancedGradingDashboard() {
  const navigate = useNavigate()
  const { examId } = useParams()
  const { user, logout } = useAuth()

  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  // Data state
  const [pendingResponses, setPendingResponses] = useState([])
  const [gradingStats, setGradingStats] = useState(null)

  // UI state
  const [activeTab, setActiveTab] = useState('overview') // overview, byStudent, byQuestion, batchGrade
  const [selectedStudent, setSelectedStudent] = useState(null)
  const [selectedQuestion, setSelectedQuestion] = useState(null)
  const [page, setPage] = useState(1)
  const [pageSize] = useState(10)

  // Modal state
  const [selectedResponse, setSelectedResponse] = useState(null)
  const [showResponseDetail, setShowResponseDetail] = useState(false)
  const [showBatchModal, setShowBatchModal] = useState(false)
  const [batchResponses, setBatchResponses] = useState([])

  useEffect(() => {
    loadGradingData()
  }, [examId, activeTab, page, selectedStudent, selectedQuestion])

  const loadGradingData = async () => {
    try {
      setLoading(true)
      setError('')

      // Load statistics
      const statsResponse = await teacherService.getGradingStatistics(examId)
      setGradingStats(statsResponse.data)

      // Load pending responses based on active tab
      let responsesData
      if (activeTab === 'byStudent' && selectedStudent) {
        responsesData = await teacherService.getPendingResponsesByStudent(
          examId,
          selectedStudent,
          page,
          pageSize
        )
      } else if (activeTab === 'byQuestion' && selectedQuestion) {
        responsesData = await teacherService.getPendingResponsesByQuestion(
          examId,
          selectedQuestion,
          page,
          pageSize
        )
      } else {
        responsesData = await teacherService.getPendingResponses(examId, page, pageSize)
      }

      console.log('API Response:', responsesData)

      if (responsesData.data && responsesData.data.responses) {
        console.log('Found responses in data.responses')
        console.log('First response object:', responsesData.data.responses[0])
        setPendingResponses(responsesData.data.responses)
      } else if (responsesData.responses) {
        console.log('Found responses in responses property')
        console.log('First response object:', responsesData.responses[0])
        setPendingResponses(responsesData.responses)
      } else if (responsesData.data && Array.isArray(responsesData.data)) {
        console.log('Found responses array in data')
        console.log('First response object:', responsesData.data[0])
        setPendingResponses(responsesData.data)
      } else {
        console.log('No responses found, setting empty array')
        setPendingResponses([])
      }
    } catch (err) {
      console.error('Error loading data:', err)
      if (err.message && err.message.includes('has not ended yet')) {
        setError('This exam has not ended yet. Grading will be available after exam completion.')
      } else {
        setError(err.message || 'Failed to load grading data')
      }
      setPendingResponses([])
    } finally {
      setLoading(false)
    }
  }

  const handleOpenResponseDetail = (responseId) => {
    console.log('Opening response detail for ID:', responseId)
    setSelectedResponse(responseId)
    setShowResponseDetail(true)
  }

  const handleCloseResponseDetail = () => {
    setShowResponseDetail(false)
    setSelectedResponse(null)
  }

  const handleOpenBatchModal = (responses) => {
    setBatchResponses(responses)
    setShowBatchModal(true)
  }

  const handleCloseBatchModal = () => {
    setShowBatchModal(false)
    setBatchResponses([])
  }

  const handleLogout = async () => {
    try {
      logout()
      navigate('/login')
    } catch (err) {
      console.error('Logout failed:', err)
    }
  }

  const onGradingComplete = () => {
    loadGradingData()
    setSuccess('Response graded successfully!')
    setTimeout(() => setSuccess(''), 3000)
  }

  // Get unique students and questions
  const getStudentList = () => {
    const students = new Set()
    pendingResponses.forEach((r) => {
      students.add(JSON.stringify({ id: r.studentId, name: r.studentName }))
    })
    return Array.from(students).map((s) => JSON.parse(s))
  }

  const getQuestionList = () => {
    const questions = new Set()
    pendingResponses.forEach((r) => {
      questions.add(JSON.stringify({ id: r.questionId, text: r.questionText }))
    })
    return Array.from(questions).map((q) => JSON.parse(q))
  }

  const getFilteredResponses = () => {
    let filtered = pendingResponses
    if (activeTab === 'byStudent' && selectedStudent) {
      filtered = filtered.filter((r) => r.studentId === selectedStudent)
    } else if (activeTab === 'byQuestion' && selectedQuestion) {
      filtered = filtered.filter((r) => r.questionId === selectedQuestion)
    }
    return filtered
  }

  if (loading && !gradingStats) {
    return (
      <Container>
        <NavBar>
          <NavLeft>
            <Logo onClick={() => navigate('/teacher/dashboard')}>Quiz Portal</Logo>
          </NavLeft>
        </NavBar>
        <MainContent>
          <LoadingSpinner>Loading grading interface...</LoadingSpinner>
        </MainContent>
      </Container>
    )
  }

  const filteredResponses = getFilteredResponses()
  const studentList = getStudentList()
  const questionList = getQuestionList()

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
        <BackButton onClick={() => navigate(`/teacher/grading/${examId}`)}>← Back to Overview</BackButton>

        <PageTitle>📝 Teacher Grading Dashboard</PageTitle>

        {error && <ErrorMessage>{error}</ErrorMessage>}
        {success && <SuccessMessage>{success}</SuccessMessage>}

        {/* Grading Progress Stats */}
        {gradingStats && <GradingProgress stats={gradingStats} />}

        {/* Tab Navigation */}
        <TabBar>
          <Tab $active={activeTab === 'overview'} onClick={() => setActiveTab('overview')}>
            📊 Overview
          </Tab>
          <Tab $active={activeTab === 'byStudent'} onClick={() => setActiveTab('byStudent')}>
            👤 By Student
          </Tab>
          <Tab $active={activeTab === 'byQuestion'} onClick={() => setActiveTab('byQuestion')}>
            ❓ By Question
          </Tab>
        </TabBar>

        {/* Filters */}
        {activeTab === 'byStudent' && (
          <FilterBar>
            <FilterGroup>
              <FilterLabel>Select Student</FilterLabel>
              <Select value={selectedStudent || ''} onChange={(e) => setSelectedStudent(e.target.value ? parseInt(e.target.value) : null)}>
                <option key="all-students" value="">All Students</option>
                {studentList.map((student) => (
                  <option key={`student-${student.id}`} value={student.id}>
                    {student.name}
                  </option>
                ))}
              </Select>
            </FilterGroup>
          </FilterBar>
        )}

        {activeTab === 'byQuestion' && (
          <FilterBar>
            <FilterGroup>
              <FilterLabel>Select Question</FilterLabel>
              <Select value={selectedQuestion || ''} onChange={(e) => setSelectedQuestion(e.target.value ? parseInt(e.target.value) : null)}>
                <option key="all-questions" value="">All Questions</option>
                {questionList.map((question) => (
                  <option key={`question-${question.id}`} value={question.id}>
                    Q{question.id}: {question.text?.substring(0, 50)}...
                  </option>
                ))}
              </Select>
            </FilterGroup>
          </FilterBar>
        )}

        {/* Responses Table */}
        {filteredResponses.length === 0 ? (
          <Card>
            <CardBody>
              <EmptyState>
                {activeTab === 'overview' && '🎉 No pending responses! All responses have been graded.'}
                {activeTab === 'byStudent' && 'Select a student to view their responses'}
                {activeTab === 'byQuestion' && 'Select a question to view responses'}
              </EmptyState>
            </CardBody>
          </Card>
        ) : (
          <>
            <Card>
              <CardHeader>
                <CardTitle>
                  {activeTab === 'overview' && `Pending Responses (${filteredResponses.length})`}
                  {activeTab === 'byStudent' && `Responses for ${studentList.find((s) => s.id === selectedStudent)?.name || 'Selected Student'} (${filteredResponses.length})`}
                  {activeTab === 'byQuestion' && `Responses for ${questionList.find((q) => q.id === selectedQuestion)?.text?.substring(0, 40) || 'Selected Question'}... (${filteredResponses.length})`}
                </CardTitle>
                {filteredResponses.length > 1 && (
                  <PrimaryButton onClick={() => handleOpenBatchModal(filteredResponses)}>
                    ⚡ Batch Grade
                  </PrimaryButton>
                )}
              </CardHeader>
              <CardBody>
                <div style={{ overflowX: 'auto' }}>
                  <Table>
                    <thead>
                      <tr key="header-row">
                        <th>Student</th>
                        <th>Question</th>
                        <th>Answer Preview</th>
                        <th>Max Marks</th>
                        <th>Status</th>
                        <th>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {filteredResponses.map((response) => (
                        <tr key={`${response.studentId}-${response.questionId}-${response.responseId}`}>
                          <td>
                            <strong>{response.studentName}</strong>
                            <div style={{ fontSize: '0.85rem', color: '#6b7280' }}>
                              {response.studentEmail}
                            </div>
                          </td>
                          <td>
                            <div style={{ maxWidth: '200px', wordWrap: 'break-word' }}>
                              {response.questionText}
                            </div>
                          </td>
                          <td>
                            <div
                              style={{
                                maxWidth: '250px',
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                                whiteSpace: 'nowrap',
                                fontSize: '0.9rem',
                              }}
                              title={response.studentAnswer}
                            >
                              {response.studentAnswer}
                            </div>
                          </td>
                          <td>{response.maxMarks}</td>
                          <td>
                            <Badge $status={'Pending'}>Pending</Badge>
                          </td>
                          <td>
                            <ActionCell>
                              <PrimaryButton onClick={() => handleOpenResponseDetail(response.responseId)}>
                                Grade
                              </PrimaryButton>
                            </ActionCell>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </Table>
                </div>
              </CardBody>
            </Card>
          </>
        )}
      </MainContent>

      {/* Modals */}
      <GradingResponseDetail
        responseId={selectedResponse}
        isOpen={showResponseDetail}
        onClose={handleCloseResponseDetail}
        onGraded={onGradingComplete}
      />

      <BatchGradingModal
        isOpen={showBatchModal}
        onClose={handleCloseBatchModal}
        responses={batchResponses}
        examId={examId}
        questionId={selectedQuestion}
        onGraded={onGradingComplete}
      />
    </Container>
  )
}
