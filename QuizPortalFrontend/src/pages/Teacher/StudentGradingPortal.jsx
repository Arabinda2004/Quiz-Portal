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
  font-weight: 600;

  &:hover {
    text-decoration: underline;
  }
`

const PageTitle = styled.h1`
  font-size: 2rem;
  color: #1f2937;
  margin: 0 0 2rem 0;
`

const ContentGrid = styled.div`
  display: grid;
  grid-template-columns: 350px 1fr;
  gap: 2rem;
  
  @media (max-width: 1024px) {
    grid-template-columns: 1fr;
  }
`

const StudentListPanel = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  height: fit-content;
  position: sticky;
  top: 20px;
`

const PanelHeader = styled.div`
  background-color: #f3f4f6;
  padding: 1rem;
  border-bottom: 1px solid #e5e7eb;
  font-weight: 600;
  color: #374151;
  font-size: 0.95rem;
`

const StudentListContent = styled.div`
  max-height: 800px;
  overflow-y: auto;
`

const StudentListItem = styled.div`
  padding: 1rem;
  border-bottom: 1px solid #e5e7eb;
  cursor: pointer;
  transition: all 0.2s ease;
  background-color: ${(props) => (props.$selected ? '#eff6ff' : 'white')};
  border-left: 4px solid ${(props) => (props.$selected ? '#3b82f6' : 'transparent')};

  &:hover {
    background-color: #f9fafb;
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

const StudentMeta = styled.div`
  display: flex;
  gap: 0.75rem;
  font-size: 0.75rem;
`

const MetaBadge = styled.span`
  background-color: #f3f4f6;
  padding: 0.25rem 0.5rem;
  border-radius: 3px;
  color: #374151;
  font-weight: 600;
`

const DetailPanel = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  overflow: hidden;
`

const DetailHeader = styled.div`
  background-color: #f3f4f6;
  padding: 1.5rem;
  border-bottom: 1px solid #e5e7eb;
`

const StudentTitle = styled.h2`
  margin: 0 0 0.5rem 0;
  color: #1f2937;
  font-size: 1.5rem;
`

const StudentSubtitle = styled.p`
  margin: 0.5rem 0 0 0;
  color: #6b7280;
  font-size: 0.95rem;
`

const DetailStats = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 1rem;
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e5e7eb;
`

const StatItem = styled.div`
  display: flex;
  flex-direction: column;
`

const StatLabel = styled.span`
  font-size: 0.8rem;
  color: #6b7280;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
`

const StatValue = styled.span`
  font-size: 1.5rem;
  font-weight: bold;
  color: #1f2937;
  margin-top: 0.25rem;
`

const DetailContent = styled.div`
  padding: 1.5rem;
  max-height: 800px;
  overflow-y: auto;
`

const ResponseCard = styled.div`
  background-color: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 1.5rem;
  margin-bottom: 1.5rem;
  transition: all 0.2s ease;

  &:hover {
    border-color: #3b82f6;
    box-shadow: 0 4px 6px rgba(59, 130, 246, 0.1);
  }

  &:last-child {
    margin-bottom: 0;
  }
`

const ResponseHeader = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
`

const QuestionNumber = styled.span`
  font-size: 0.8rem;
  font-weight: 600;
  color: #9ca3af;
  text-transform: uppercase;
`

const QuestionText = styled.h4`
  margin: 0.5rem 0 0 0;
  color: #1f2937;
  font-size: 1rem;
`

const StatusBadge = styled.span`
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 0.75rem;
  border-radius: 20px;
  font-size: 0.8rem;
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

const ResponseBody = styled.div`
  margin-bottom: 1rem;
`

const AnswerSection = styled.div`
  margin-bottom: 1rem;
`

const SectionLabel = styled.label`
  display: block;
  font-weight: 600;
  color: #374151;
  margin-bottom: 0.5rem;
  font-size: 0.9rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: #6b7280;
`

const AnswerText = styled.div`
  background-color: white;
  padding: 0.75rem;
  border-left: 4px solid #3b82f6;
  border-radius: 4px;
  color: #1f2937;
  font-size: 0.95rem;
  line-height: 1.5;
  word-break: break-word;
`

const GradingSection = styled.div`
  background-color: white;
  padding: 1rem;
  border-radius: 6px;
  margin-top: 1rem;
  border: 2px solid #eff6ff;
`

const GradingTitle = styled.div`
  font-weight: 600;
  color: #1f2937;
  margin-bottom: 1rem;
  font-size: 0.95rem;
`

const FormGroup = styled.div`
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-bottom: 1rem;

  &.full-width {
    grid-template-columns: 1fr;
  }
`

const InputGroup = styled.div`
  display: flex;
  flex-direction: column;
`

const Label = styled.label`
  font-weight: 600;
  color: #374151;
  margin-bottom: 0.5rem;
  font-size: 0.9rem;
`

const Input = styled.input`
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

  &:disabled {
    background-color: #f3f4f6;
    color: #9ca3af;
    cursor: not-allowed;
  }
`

const TextArea = styled.textarea`
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 4px;
  font-size: 0.95rem;
  font-family: inherit;
  resize: vertical;
  min-height: 80px;

  &:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }

  &:disabled {
    background-color: #f3f4f6;
    color: #9ca3af;
    cursor: not-allowed;
  }
`

const MarksDisplay = styled.div`
  padding: 0.75rem;
  background-color: #f3f4f6;
  border-radius: 4px;
  font-weight: 600;
  color: #1f2937;
  text-align: center;
`

const CheckboxGroup = styled.div`
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem;
  background-color: #f3f4f6;
  border-radius: 4px;
`

const Checkbox = styled.input`
  width: 20px;
  height: 20px;
  cursor: pointer;

  &:disabled {
    cursor: not-allowed;
    opacity: 0.5;
  }
`

const ButtonGroup = styled.div`
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
  margin-top: 1rem;
`

const Button = styled.button`
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;

  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
`

const SaveButton = styled(Button)`
  background-color: #3b82f6;
  color: white;

  &:hover:not(:disabled) {
    background-color: #2563eb;
  }
`

const ResetButton = styled(Button)`
  background-color: #e5e7eb;
  color: #1f2937;

  &:hover:not(:disabled) {
    background-color: #d1d5db;
  }
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
  background-color: #f0fdf4;
  border-left: 4px solid #16a34a;
  color: #16a34a;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
`

const LoadingSpinner = styled.div`
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 400px;
  color: #6b7280;
  font-size: 1rem;
`

const EmptyState = styled.div`
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  color: #6b7280;
  text-align: center;
`

const EmptyStateIcon = styled.div`
  font-size: 3rem;
  margin-bottom: 1rem;
`

const EmptyStateText = styled.p`
  font-size: 1rem;
  margin: 0;
`

export default function StudentGradingPortal() {
  const navigate = useNavigate()
  const { examId } = useParams()
  const { user, logout } = useAuth()

  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const [students, setStudents] = useState([])
  const [selectedStudentId, setSelectedStudentId] = useState(null)
  const [responses, setResponses] = useState(null)
  const [gradingData, setGradingData] = useState({})
  const [editingResponseId, setEditingResponseId] = useState(null)

  // Load all pending responses to get student list
  useEffect(() => {
    loadStudents()
  }, [examId])

  const loadStudents = async () => {
    try {
      setLoading(true)
      setError('')
      
      // Get pending responses to extract unique students
      const response = await teacherService.getPendingResponses(examId)
      const pendingResponses = response.data?.responses || response.responses || []

      // Create unique student map
      const studentMap = new Map()
      pendingResponses.forEach((resp) => {
        if (!studentMap.has(resp.studentId)) {
          studentMap.set(resp.studentId, {
            studentId: resp.studentId,
            studentName: resp.studentName,
            studentEmail: resp.studentEmail,
            pendingCount: 1,
          })
        } else {
          const student = studentMap.get(resp.studentId)
          student.pendingCount += 1
        }
      })

      const studentList = Array.from(studentMap.values()).sort((a, b) =>
        a.studentName.localeCompare(b.studentName)
      )
      setStudents(studentList)

      if (studentList.length > 0) {
        setSelectedStudentId(studentList[0].studentId)
      }
    } catch (err) {
      console.error('Error loading students:', err)
      setError(err.message || 'Failed to load students')
    } finally {
      setLoading(false)
    }
  }

  // Load student responses when student is selected
  useEffect(() => {
    if (selectedStudentId) {
      loadStudentResponses()
    }
  }, [selectedStudentId])

  const loadStudentResponses = async () => {
    try {
      setLoading(true)
      setError('')
      setGradingData({})
      setEditingResponseId(null)

      // Get responses for selected student
      const response = await teacherService.getPendingResponsesByStudent(
        examId,
        selectedStudentId
      )
      
      const studentResponses = response.data?.responses || response.responses || []
      setResponses(studentResponses)
    } catch (err) {
      console.error('Error loading student responses:', err)
      setError(err.message || 'Failed to load student responses')
      setResponses(null)
    } finally {
      setLoading(false)
    }
  }

  const handleSelectStudent = (studentId) => {
    setSelectedStudentId(studentId)
  }

  const handleGradingInputChange = (responseId, field, value) => {
    setGradingData((prev) => ({
      ...prev,
      [responseId]: {
        ...prev[responseId],
        [field]: value,
      },
    }))
  }

  const handleSaveGrade = async (responseId) => {
    const data = gradingData[responseId]
    if (!data || data.marksObtained === '') {
      setError('Please enter marks for this response')
      return
    }

    const marks = parseFloat(data.marksObtained)
    if (isNaN(marks) || marks < 0) {
      setError('Please enter a valid marks value')
      return
    }

    const response = responses.find((r) => r.responseId === responseId)
    if (marks > response.maxMarks) {
      setError(`Marks cannot exceed ${response.maxMarks}`)
      return
    }

    try {
      setSaving(true)
      setError('')

      await teacherService.gradeSingleResponse(responseId, {
        marksObtained: marks,
        feedback: data.feedback || '',
        comment: data.comment || '',
        isPartialCredit: data.isPartialCredit || false,
      })

      setSuccess('Response graded successfully!')
      setEditingResponseId(null)
      loadStudentResponses()
      setTimeout(() => setSuccess(''), 3000)
    } catch (err) {
      console.error('Error saving grade:', err)
      setError(err.message || 'Failed to save grade')
    } finally {
      setSaving(false)
    }
  }

  const handleResetForm = (responseId) => {
    setGradingData((prev) => {
      const newData = { ...prev }
      delete newData[responseId]
      return newData
    })
    setEditingResponseId(null)
  }

  const handleLogout = async () => {
    try {
      logout()
      navigate('/login')
    } catch (err) {
      console.error('Logout failed:', err)
    }
  }

  const selectedStudent = students.find((s) => s.studentId === selectedStudentId)
  const totalResponses = responses ? responses.length : 0
  const gradedResponses = responses
    ? responses.filter((r) => r.marksObtained !== null && r.marksObtained !== 0).length
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
        <BackButton onClick={() => navigate(`/teacher/exam/${examId}`)}>
          ← Back to Exam
        </BackButton>

        <PageTitle>📝 Grade Student Responses</PageTitle>

        {error && <ErrorMessage>{error}</ErrorMessage>}
        {success && <SuccessMessage>{success}</SuccessMessage>}

        {loading ? (
          <LoadingSpinner>Loading student responses...</LoadingSpinner>
        ) : (
          <ContentGrid>
            {/* Student List */}
            <StudentListPanel>
              <PanelHeader>
                Students ({students.length})
              </PanelHeader>
              <StudentListContent>
                {students.length === 0 ? (
                  <div style={{ padding: '1.5rem', textAlign: 'center', color: '#9ca3af' }}>
                    No students with pending responses
                  </div>
                ) : (
                  students.map((student) => (
                    <StudentListItem
                      key={student.studentId}
                      $selected={selectedStudentId === student.studentId}
                      onClick={() => handleSelectStudent(student.studentId)}
                    >
                      <StudentName>{student.studentName}</StudentName>
                      <StudentEmail>{student.studentEmail}</StudentEmail>
                      <StudentMeta>
                        <MetaBadge>Pending: {student.pendingCount}</MetaBadge>
                      </StudentMeta>
                    </StudentListItem>
                  ))
                )}
              </StudentListContent>
            </StudentListPanel>

            {/* Detail Panel */}
            <DetailPanel>
              {selectedStudent ? (
                <>
                  <DetailHeader>
                    <StudentTitle>{selectedStudent.studentName}</StudentTitle>
                    <StudentSubtitle>{selectedStudent.studentEmail}</StudentSubtitle>
                    <DetailStats>
                      <StatItem>
                        <StatLabel>Total Questions</StatLabel>
                        <StatValue>{totalResponses}</StatValue>
                      </StatItem>
                      <StatItem>
                        <StatLabel>Graded</StatLabel>
                        <StatValue style={{ color: '#16a34a' }}>
                          {gradedResponses}
                        </StatValue>
                      </StatItem>
                      <StatItem>
                        <StatLabel>Pending</StatLabel>
                        <StatValue style={{ color: '#dc2626' }}>
                          {totalResponses - gradedResponses}
                        </StatValue>
                      </StatItem>
                    </DetailStats>
                  </DetailHeader>

                  <DetailContent>
                    {loading ? (
                      <LoadingSpinner>Loading responses...</LoadingSpinner>
                    ) : responses && responses.length > 0 ? (
                      responses.map((response, index) => {
                        const isEditing = editingResponseId === response.responseId
                        const isGraded = response.marksObtained !== null && response.marksObtained !== 0
                        const gradeData = gradingData[response.responseId] || {
                          marksObtained: response.marksObtained || '',
                          feedback: response.feedback || '',
                          comment: response.comment || '',
                          isPartialCredit: response.isPartialCredit || false,
                        }

                        return (
                          <ResponseCard key={response.responseId}>
                            <ResponseHeader>
                              <div>
                                <QuestionNumber>Question {index + 1}</QuestionNumber>
                                <QuestionText>
                                  {response.questionText}
                                </QuestionText>
                              </div>
                              <StatusBadge $status={isGraded ? 'Graded' : 'Pending'}>
                                {isGraded ? '✓ Graded' : '⏱ Pending'}
                              </StatusBadge>
                            </ResponseHeader>

                            <ResponseBody>
                              <AnswerSection>
                                <SectionLabel>Student's Answer</SectionLabel>
                                <AnswerText>
                                  {response.studentAnswer || <em>Not answered</em>}
                                </AnswerText>
                              </AnswerSection>

                              {isEditing ? (
                                <GradingSection>
                                  <GradingTitle>🎓 Assign Marks & Feedback</GradingTitle>

                                  <FormGroup>
                                    <InputGroup>
                                      <Label>
                                        Marks Obtained (Max: {response.maxMarks}) *
                                      </Label>
                                      <Input
                                        type="number"
                                        min="0"
                                        max={response.maxMarks}
                                        step="0.5"
                                        value={gradeData.marksObtained || ''}
                                        onChange={(e) =>
                                          handleGradingInputChange(
                                            response.responseId,
                                            'marksObtained',
                                            e.target.value
                                          )
                                        }
                                        placeholder="Enter marks"
                                      />
                                    </InputGroup>
                                    <InputGroup>
                                      <Label>Max Marks</Label>
                                      <MarksDisplay>{response.maxMarks}</MarksDisplay>
                                    </InputGroup>
                                  </FormGroup>

                                  <FormGroup className="full-width">
                                    <InputGroup>
                                      <Label>Feedback</Label>
                                      <TextArea
                                        value={gradeData.feedback || ''}
                                        onChange={(e) =>
                                          handleGradingInputChange(
                                            response.responseId,
                                            'feedback',
                                            e.target.value
                                          )
                                        }
                                        placeholder="Provide feedback to the student..."
                                      />
                                    </InputGroup>
                                  </FormGroup>

                                  <FormGroup className="full-width">
                                    <InputGroup>
                                      <Label>Internal Comments</Label>
                                      <TextArea
                                        value={gradeData.comment || ''}
                                        onChange={(e) =>
                                          handleGradingInputChange(
                                            response.responseId,
                                            'comment',
                                            e.target.value
                                          )
                                        }
                                        placeholder="Personal notes (not shown to student)..."
                                      />
                                    </InputGroup>
                                  </FormGroup>

                                  <FormGroup className="full-width">
                                    <CheckboxGroup>
                                      <Checkbox
                                        type="checkbox"
                                        id={`partial-${response.responseId}`}
                                        checked={gradeData.isPartialCredit || false}
                                        onChange={(e) =>
                                          handleGradingInputChange(
                                            response.responseId,
                                            'isPartialCredit',
                                            e.target.checked
                                          )
                                        }
                                      />
                                      <Label htmlFor={`partial-${response.responseId}`}>
                                        This is partial credit for a partially correct answer
                                      </Label>
                                    </CheckboxGroup>
                                  </FormGroup>

                                  <ButtonGroup>
                                    <ResetButton
                                      onClick={() => handleResetForm(response.responseId)}
                                    >
                                      Cancel
                                    </ResetButton>
                                    <SaveButton
                                      onClick={() => handleSaveGrade(response.responseId)}
                                      disabled={saving}
                                    >
                                      {saving ? 'Saving...' : 'Save Grade'}
                                    </SaveButton>
                                  </ButtonGroup>
                                </GradingSection>
                              ) : (
                                <div style={{ marginTop: '1rem' }}>
                                  {isGraded ? (
                                    <div
                                      style={{
                                        backgroundColor: '#f0fdf4',
                                        padding: '1rem',
                                        borderRadius: '6px',
                                        border: '1px solid #86efac',
                                      }}
                                    >
                                      <div style={{ fontSize: '0.9rem', fontWeight: 600, color: '#16a34a', marginBottom: '0.5rem' }}>
                                        ✓ Graded
                                      </div>
                                      <div style={{ color: '#1f2937', fontSize: '0.95rem' }}>
                                        Marks: {response.marksObtained} / {response.maxMarks}
                                      </div>
                                      {response.feedback && (
                                        <div style={{ marginTop: '0.5rem', color: '#6b7280', fontSize: '0.9rem' }}>
                                          <strong>Feedback:</strong> {response.feedback}
                                        </div>
                                      )}
                                      <Button
                                        onClick={() => {
                                          setEditingResponseId(response.responseId)
                                          setGradingData((prev) => ({
                                            ...prev,
                                            [response.responseId]: {
                                              marksObtained: response.marksObtained || '',
                                              feedback: response.feedback || '',
                                              comment: response.comment || '',
                                              isPartialCredit: response.isPartialCredit || false,
                                            },
                                          }))
                                        }}
                                        style={{
                                          marginTop: '0.75rem',
                                          backgroundColor: '#3b82f6',
                                          color: 'white',
                                        }}
                                      >
                                        Edit Grade
                                      </Button>
                                    </div>
                                  ) : (
                                    <SaveButton
                                      onClick={() => {
                                        setEditingResponseId(response.responseId)
                                        setGradingData((prev) => ({
                                          ...prev,
                                          [response.responseId]: {
                                            marksObtained: '',
                                            feedback: '',
                                            comment: '',
                                            isPartialCredit: false,
                                          },
                                        }))
                                      }}
                                    >
                                      Grade This Response
                                    </SaveButton>
                                  )}
                                </div>
                              )}
                            </ResponseBody>
                          </ResponseCard>
                        )
                      })
                    ) : (
                      <EmptyState>
                        <EmptyStateIcon>🎉</EmptyStateIcon>
                        <EmptyStateText>
                          All responses have been graded for this student!
                        </EmptyStateText>
                      </EmptyState>
                    )}
                  </DetailContent>
                </>
              ) : (
                <EmptyState>
                  <EmptyStateIcon>👈</EmptyStateIcon>
                  <EmptyStateText>Select a student to view their responses</EmptyStateText>
                </EmptyState>
              )}
            </DetailPanel>
          </ContentGrid>
        )}
      </MainContent>
    </Container>
  )
}
