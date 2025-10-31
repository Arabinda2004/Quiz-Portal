import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { teacherService } from '../../services/api'
import styled from 'styled-components'

// ============================================================================
// STYLED COMPONENTS
// ============================================================================

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
  transition: opacity 0.3s;

  &:hover {
    opacity: 0.8;
  }
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
  font-weight: 600;
  transition: background-color 0.3s;

  &:hover {
    background-color: #b91c1c;
  }
`

const MainContent = styled.div`
  padding: 2rem;
  max-width: 1600px;
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
  font-weight: 600;

  &:hover {
    text-decoration: underline;
  }
`

const PageTitle = styled.h1`
  font-size: 2rem;
  color: #1f2937;
  margin-bottom: 0.5rem;
  font-weight: 700;
`

const PageSubtitle = styled.p`
  color: #6b7280;
  margin: 0 0 2rem 0;
  font-size: 1rem;
`

const DashboardGrid = styled.div`
  display: grid;
  grid-template-columns: 350px 1fr;
  gap: 2rem;
  margin-bottom: 2rem;

  @media (max-width: 1024px) {
    grid-template-columns: 1fr;
  }
`

const LeftPanel = styled.div`
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
`

const RightPanel = styled.div`
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
`

const Card = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  overflow: hidden;
`

const CardHeader = styled.div`
  background-color: #f3f4f6;
  padding: 1rem 1.5rem;
  border-bottom: 1px solid #e5e7eb;
  font-weight: 600;
  color: #374151;
  display: flex;
  justify-content: space-between;
  align-items: center;
`

const CardTitle = styled.div`
  font-size: 1rem;
  font-weight: 700;
  color: #1f2937;
`

const CardBody = styled.div`
  padding: 1.5rem;
  max-height: ${(props) => props.$maxHeight || '500px'};
  overflow-y: auto;
`

const StudentListItem = styled.div`
  padding: 1rem;
  border-bottom: 1px solid #e5e7eb;
  cursor: pointer;
  transition: all 0.3s;
  background-color: ${(props) => (props.$selected ? '#dbeafe' : 'transparent')};
  border-left: 4px solid ${(props) => (props.$selected ? '#3b82f6' : 'transparent')};

  &:hover {
    background-color: #f3f4f6;
  }

  &:last-child {
    border-bottom: none;
  }
`

const StudentName = styled.div`
  font-weight: 600;
  color: #1f2937;
  margin-bottom: 0.25rem;
  font-size: 0.95rem;
`

const StudentEmail = styled.div`
  font-size: 0.8rem;
  color: #6b7280;
  margin-bottom: 0.5rem;
`

const StudentStats = styled.div`
  display: flex;
  gap: 0.75rem;
  font-size: 0.75rem;
`

const StatBadge = styled.span`
  padding: 0.25rem 0.5rem;
  border-radius: 3px;
  font-weight: 600;
  background-color: ${(props) => {
    switch (props.$type) {
      case 'pending':
        return '#fef3c7'
      case 'graded':
        return '#d1fae5'
      case 'total':
        return '#dbeafe'
      default:
        return '#e5e7eb'
    }
  }};
  color: ${(props) => {
    switch (props.$type) {
      case 'pending':
        return '#92400e'
      case 'graded':
        return '#065f46'
      case 'total':
        return '#1e40af'
      default:
        return '#374151'
    }
  }};
`

const EmptyState = styled.div`
  text-align: center;
  padding: 2rem 1rem;
  color: #9ca3af;
  font-size: 0.95rem;
`

const LoadingSpinner = styled.div`
  text-align: center;
  padding: 2rem;
  color: #6b7280;
`

const DetailHeader = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
`

const DetailTitle = styled.h2`
  margin: 0;
  color: #1f2937;
  font-size: 1.25rem;
`

const DetailStats = styled.div`
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
  margin-bottom: 1.5rem;
`

const StatBox = styled.div`
  background-color: #f9fafb;
  padding: 1rem;
  border-radius: 6px;
  border-left: 4px solid #3b82f6;
`

const StatLabel = styled.div`
  font-size: 0.8rem;
  color: #6b7280;
  font-weight: 600;
  margin-bottom: 0.25rem;
`

const StatValue = styled.div`
  font-size: 1.5rem;
  font-weight: 700;
  color: #1f2937;
`

const ResponseItem = styled.div`
  background-color: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 6px;
  padding: 1.25rem;
  margin-bottom: 1rem;
  transition: all 0.3s;

  &:hover {
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  }

  &:last-child {
    margin-bottom: 0;
  }
`

const QuestionText = styled.div`
  font-weight: 600;
  color: #1f2937;
  margin-bottom: 0.75rem;
  font-size: 0.95rem;
`

const AnswerBox = styled.div`
  background-color: white;
  border: 1px solid #e5e7eb;
  border-radius: 4px;
  padding: 0.75rem;
  margin-bottom: 0.75rem;
  max-height: 100px;
  overflow-y: auto;
  font-size: 0.9rem;
  color: #4b5563;
`

const ResponseFooter = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 0.75rem;
  padding-top: 0.75rem;
  border-top: 1px solid #e5e7eb;
`

const MarksDisplay = styled.div`
  font-weight: 600;
  color: #1f2937;
`

const StatusBadge = styled.span`
  display: inline-block;
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  background-color: ${(props) => {
    switch (props.$status) {
      case 'graded':
        return '#d1fae5'
      case 'pending':
        return '#fef3c7'
      default:
        return '#e5e7eb'
    }
  }};
  color: ${(props) => {
    switch (props.$status) {
      case 'graded':
        return '#065f46'
      case 'pending':
        return '#92400e'
      default:
        return '#374151'
    }
  }};
`

const Button = styled.button`
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 6px;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
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

const GradingModal = styled.div`
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
`

const ModalContent = styled.div`
  background: white;
  border-radius: 8px;
  padding: 2rem;
  max-width: 600px;
  width: 90%;
  max-height: 80vh;
  overflow-y: auto;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
`

const ModalTitle = styled.h2`
  margin: 0 0 1.5rem 0;
  color: #1f2937;
  font-size: 1.5rem;
`

const FormGroup = styled.div`
  margin-bottom: 1.5rem;
`

const Label = styled.label`
  display: block;
  font-weight: 600;
  color: #374151;
  margin-bottom: 0.5rem;
  font-size: 0.95rem;
`

const Input = styled.input`
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 4px;
  font-size: 0.95rem;
  font-family: inherit;

  &:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }
`

const Textarea = styled.textarea`
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 4px;
  font-size: 0.95rem;
  font-family: inherit;
  resize: vertical;
  min-height: 100px;

  &:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }
`

const PreviewBox = styled.div`
  background-color: #f3f4f6;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1rem;
  border-left: 4px solid #3b82f6;
`

const PreviewLabel = styled.div`
  font-size: 0.875rem;
  color: #6b7280;
  font-weight: 600;
  margin-bottom: 0.5rem;
`

const PreviewText = styled.div`
  color: #1f2937;
  word-break: break-word;
  font-size: 0.95rem;
`

const ErrorMessage = styled.div`
  background-color: #fee;
  border-left: 4px solid #991b1b;
  color: #991b1b;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
  font-size: 0.95rem;
`

const SuccessMessage = styled.div`
  background-color: #ecfdf5;
  border-left: 4px solid #065f46;
  color: #065f46;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
  font-size: 0.95rem;
`

const ModalButtonGroup = styled.div`
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  margin-top: 2rem;
  border-top: 1px solid #e5e7eb;
  padding-top: 1.5rem;
`

const ProgressContainer = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 1rem;
  margin-bottom: 1.5rem;
`

const ProgressCard = styled.div`
  background: linear-gradient(135deg, ${(props) => props.$bgStart} 0%, ${(props) => props.$bgEnd} 100%);
  color: white;
  padding: 1rem;
  border-radius: 6px;
  text-align: center;
`

const ProgressCardLabel = styled.div`
  font-size: 0.75rem;
  opacity: 0.9;
  margin-bottom: 0.25rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  font-weight: 600;
`

const ProgressCardValue = styled.div`
  font-size: 1.75rem;
  font-weight: 700;
`

// ============================================================================
// MAIN COMPONENT
// ============================================================================

export default function TeacherGradingDashboard() {
  const navigate = useNavigate()
  const { examId } = useParams()
  const { user, logout } = useAuth()

  // Data states
  const [students, setStudents] = useState([])
  const [selectedStudentData, setSelectedStudentData] = useState(null)
  const [selectedStudentId, setSelectedStudentId] = useState(null)

  // UI states
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  // Modal states
  const [showGradingModal, setShowGradingModal] = useState(false)
  const [editingResponseId, setEditingResponseId] = useState(null)
  const [gradingData, setGradingData] = useState({
    marksObtained: '',
    feedback: '',
  })

  // Load students on mount
  useEffect(() => {
    loadStudentsList()
  }, [examId])

  // Load student responses when selected student changes
  useEffect(() => {
    if (selectedStudentId) {
      loadStudentResponses(selectedStudentId)
    }
  }, [selectedStudentId])

  const loadStudentsList = async () => {
    try {
      setLoading(true)
      setError('')
      const response = await teacherService.getStudentAttempts(examId)
      console.log('Students response:', response)
      console.log('Students response data:', response?.data)
      
      // Extract students data from response
      let studentsData = []
      
      if (response?.data?.data) {
        studentsData = response.data.data || []
      } else if (response?.data && Array.isArray(response.data)) {
        studentsData = response.data || []
      } else if (Array.isArray(response)) {
        studentsData = response || []
      }
      
      // Log first student to debug properties
      if (studentsData.length > 0) {
        console.log('First student object:', studentsData[0])
        console.log('Student keys:', Object.keys(studentsData[0]))
      }
      
      setStudents(studentsData)
    } catch (err) {
      console.error('Error loading students:', err)
      setError(err?.message || 'Failed to load students')
      setStudents([])
    } finally {
      setLoading(false)
    }
  }

  const loadStudentResponses = async (studentId) => {
    try {
      setError('')
      const response = await teacherService.getStudentResponses(examId, studentId)
      console.log('Student responses:', response)
      
      // The API returns: { examId, examName, studentId, totalQuestions, data: [...] }
      // We need to preserve all metadata while passing data array
      if (response?.data) {
        setSelectedStudentData(response)
      } else {
        setSelectedStudentData(response)
      }
    } catch (err) {
      console.error('Error loading responses:', err)
      setError('Failed to load student responses')
      setSelectedStudentData(null)
    }
  }

  const handleSelectStudent = (student) => {
    setSelectedStudentId(student.studentId)
    setEditingResponseId(null)
    setGradingData({ marksObtained: '', feedback: '' })
  }

  const handleOpenGradingModal = (responseId) => {
    // Find the response to get current marks and feedback
    const response = selectedStudentData?.data?.find((r) => r.responseID === responseId)
    
    setEditingResponseId(responseId)
    setGradingData({
      marksObtained: response?.marksObtained ? response.marksObtained.toString() : '',
      feedback: response?.feedback || '',
    })
    setShowGradingModal(true)
  }

  const handleCloseGradingModal = () => {
    setShowGradingModal(false)
    setEditingResponseId(null)
    setGradingData({ marksObtained: '', feedback: '' })
  }

  const handleGradeResponse = async () => {
    console.log('=== GRADING DEBUG ===')
    console.log('Editing Response ID:', editingResponseId)
    console.log('Grading Data:', gradingData)
    console.log('Selected Student Data:', selectedStudentData)

    if (!gradingData.marksObtained && gradingData.marksObtained !== 0) {
      setError('Please enter marks for this response')
      return
    }

    const marks = parseInt(gradingData.marksObtained)
    console.log('Parsed Marks:', marks)

    const response = selectedStudentData?.data?.find((r) => r.responseID === editingResponseId)
    console.log('Found Response:', response)

    if (!response) {
      setError('Response not found')
      return
    }

    // Get max marks from the response object (may vary by question)
    // Default to 5 if not specified
    const maxMarks = response.questionMarks || response.marks || 5
    console.log('Max Marks:', maxMarks)

    if (marks < 0 || marks > maxMarks) {
      setError(`Marks must be between 0 and ${maxMarks}`)
      return
    }

    const payload = {
      marksObtained: marks,
      feedback: gradingData.feedback,
    }
    console.log('Payload being sent:', payload)

    try {
      setSaving(true)
      setError('')
      console.log('Calling gradeSingleResponse...')
      const result = await teacherService.gradeSingleResponse(editingResponseId, payload)
      console.log('Grade result:', result)
      setSuccess('Response graded successfully!')
      handleCloseGradingModal()
      loadStudentResponses(selectedStudentId)
      setTimeout(() => setSuccess(''), 3000)
    } catch (err) {
      console.error('Error grading response:', err)
      console.error('Error details:', { 
        message: err?.message,
        data: err?.data,
        response: err?.response,
        fullError: err
      })
      setError(err?.message || JSON.stringify(err) || 'Failed to grade response')
    } finally {
      setSaving(false)
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
          <LoadingSpinner>Loading students and their responses...</LoadingSpinner>
        </MainContent>
      </Container>
    )
  }

  const currentStudent = students.find((s) => s.studentId === selectedStudentId)

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

        <PageTitle>📝 Student-Wise Grading Dashboard</PageTitle>
        <PageSubtitle>Grade responses organized by student - view all answers and provide feedback</PageSubtitle>

        {error && <ErrorMessage>{error}</ErrorMessage>}
        {success && <SuccessMessage>{success}</SuccessMessage>}

        {students.length === 0 ? (
          <Card>
            <CardBody>
              <EmptyState>
                <div style={{ fontSize: '2rem', marginBottom: '0.5rem' }}>📭</div>
                No students have attempted this exam yet
              </EmptyState>
            </CardBody>
          </Card>
        ) : (
          <DashboardGrid>
            {/* LEFT PANEL - STUDENT LIST */}
            <LeftPanel>
              <Card>
                <CardHeader>
                  <CardTitle>Students ({students.length})</CardTitle>
                </CardHeader>
                <CardBody $maxHeight="600px">
                  {students.map((student) => (
                    <StudentListItem
                      key={student.studentId}
                      $selected={selectedStudentId === student.studentId}
                      onClick={() => {
                        console.log('Selected student:', student)
                        handleSelectStudent(student)
                      }}
                    >
                      <StudentName>{student.studentName}</StudentName>
                      <StudentEmail>{student.studentEmail}</StudentEmail>
                      <StudentStats>
                        <StatBadge $type="total">
                          Attempts: {student.totalAttempts ?? student.totalResponses ?? student.answeredQuestions ?? student.totalQuestions ?? '?'}
                        </StatBadge>
                        {/* <StatBadge $type="graded">
                          Info: {student.gradedAttempts ?? student.gradedQuestions ?? student.marksObtained ?? '?'}
                        </StatBadge> */}
                      </StudentStats>
                    </StudentListItem>
                  ))}
                </CardBody>
              </Card>
            </LeftPanel>

            {/* RIGHT PANEL - STUDENT DETAILS AND RESPONSES */}
            <RightPanel>
              {selectedStudentId && currentStudent ? (
                <>
                  {/* Student Summary */}
                  <Card>
                    <CardBody>
                      <DetailHeader>
                        <DetailTitle>{currentStudent.studentName}</DetailTitle>
                      </DetailHeader>
                      <div style={{ marginBottom: '1rem', color: '#6b7280', fontSize: '0.9rem' }}>
                        {currentStudent.studentEmail}
                      </div>

                      <DetailStats>
                        <ProgressCard $bgStart="#3b82f6" $bgEnd="#1e40af">
                          <ProgressCardLabel>Total Questions</ProgressCardLabel>
                          <ProgressCardValue>
                            {selectedStudentData?.totalQuestions || 0}
                          </ProgressCardValue>
                        </ProgressCard>
                        <ProgressCard $bgStart="#10b981" $bgEnd="#059669">
                          <ProgressCardLabel>Answered</ProgressCardLabel>
                          <ProgressCardValue>
                            {selectedStudentData?.answeredQuestions || 0}
                          </ProgressCardValue>
                        </ProgressCard>
                        <ProgressCard $bgStart="#f59e0b" $bgEnd="#d97706">
                          <ProgressCardLabel>Unanswered</ProgressCardLabel>
                          <ProgressCardValue>
                            {selectedStudentData?.unansweredQuestions || 0}
                          </ProgressCardValue>
                        </ProgressCard>
                      </DetailStats>
                    </CardBody>
                  </Card>

                  {/* Responses */}
                  <Card>
                    <CardHeader>
                      <CardTitle>
                        Responses ({selectedStudentData?.data?.length || 0})
                      </CardTitle>
                    </CardHeader>
                    <CardBody $maxHeight="700px">
                      {selectedStudentData?.data && selectedStudentData.data.length > 0 ? (
                        selectedStudentData.data.map((response, index) => {
                          console.log('Response object:', response)
                          return (
                          <ResponseItem key={response.responseID || index}>
                            <QuestionText>
                              Q{index + 1}. {response.questionText || 'Question text not available'}
                            </QuestionText>

                            <AnswerBox>
                              <strong>Student's Answer:</strong>
                              <div style={{ marginTop: '0.5rem' }}>
                                {response.answerText || <em>Not answered</em>}
                              </div>
                            </AnswerBox>

                            <ResponseFooter>
                              <div>
                                <StatusBadge $status={response.marksObtained > 0 ? 'graded' : 'pending'}>
                                  {response.marksObtained > 0 ? 'Graded' : 'Pending'}
                                </StatusBadge>
                                {response.marksObtained > 0 && (
                                  <MarksDisplay style={{ marginLeft: '0.75rem', display: 'inline' }}>
                                    {response.marksObtained} marks
                                  </MarksDisplay>
                                )}
                              </div>
                              <PrimaryButton
                                onClick={() => handleOpenGradingModal(response.responseID)}
                              >
                                {response.marksObtained > 0 ? 'Edit' : 'Grade'}
                              </PrimaryButton>
                            </ResponseFooter>
                          </ResponseItem>
                        )})
                      ) : (
                        <EmptyState>No responses available</EmptyState>
                      )}
                    </CardBody>
                  </Card>
                </>
              ) : (
                <Card>
                  <CardBody>
                    <EmptyState>Select a student to view their responses</EmptyState>
                  </CardBody>
                </Card>
              )}
            </RightPanel>
          </DashboardGrid>
        )}
      </MainContent>

      {/* GRADING MODAL */}
      {showGradingModal && selectedStudentData && editingResponseId && (
        <GradingModal onClick={handleCloseGradingModal}>
          <ModalContent onClick={(e) => e.stopPropagation()}>
            {(() => {
              const response = selectedStudentData.data.find((r) => r.responseID === editingResponseId)
              if (!response) return null

              const maxMarks = response.questionMarks || response.marks || 5

              return (
                <>
                  <ModalTitle>Grade Response</ModalTitle>

                  <PreviewBox>
                    <PreviewLabel>Question</PreviewLabel>
                    <PreviewText>{response.questionText || 'Question not available'}</PreviewText>
                  </PreviewBox>

                  <PreviewBox>
                    <PreviewLabel>Student's Answer</PreviewLabel>
                    <PreviewText>{response.answerText || <em>Not answered</em>}</PreviewText>
                  </PreviewBox>

                  <FormGroup>
                    <Label>Marks Obtained (Max: {maxMarks})</Label>
                    <Input
                      type="number"
                      min="0"
                      max={maxMarks}
                      value={gradingData.marksObtained}
                      onChange={(e) => {
                        console.log('Marks input changed:', e.target.value)
                        setGradingData((prev) => {
                          const updated = {
                            ...prev,
                            marksObtained: e.target.value,
                          }
                          console.log('Updated grading data:', updated)
                          return updated
                        })
                      }}
                      placeholder="Enter marks"
                    />
                  </FormGroup>

                  <FormGroup>
                    <Label>Feedback (Optional)</Label>
                    <Textarea
                      value={gradingData.feedback}
                      onChange={(e) => {
                        console.log('Feedback changed:', e.target.value)
                        setGradingData((prev) => ({
                          ...prev,
                          feedback: e.target.value,
                        }))
                      }}
                      placeholder="Provide feedback to the student..."
                    />
                  </FormGroup>

                  {error && <ErrorMessage>{error}</ErrorMessage>}

                  <ModalButtonGroup>
                    <SecondaryButton onClick={handleCloseGradingModal}>Cancel</SecondaryButton>
                    <PrimaryButton 
                      onClick={() => {
                        console.log('Grade button clicked!')
                        handleGradeResponse()
                      }} 
                      disabled={saving}
                    >
                      {saving ? 'Saving...' : 'Submit Grade'}
                    </PrimaryButton>
                  </ModalButtonGroup>
                </>
              )
            })()}
          </ModalContent>
        </GradingModal>
      )}
    </Container>
  )
}
