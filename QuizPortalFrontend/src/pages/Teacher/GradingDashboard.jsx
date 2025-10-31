import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { teacherService, resultService } from '../../services/api'
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

const PageTitle = styled.h1`
  font-size: 2rem;
  color: #1f2937;
  margin-bottom: 1.5rem;
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
  padding: 2rem;
  margin-bottom: 1.5rem;
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
`

const SecondaryButton = styled(Button)`
  background-color: #e5e7eb;
  color: #1f2937;

  &:hover {
    background-color: #d1d5db;
  }
`

const QuestionPreview = styled.div`
  background-color: #f9fafb;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1rem;
  border-left: 4px solid #3b82f6;
`

const QuestionText = styled.p`
  margin: 0 0 0.5rem 0;
  color: #1f2937;
  font-weight: 600;
`

const StudentAnswer = styled.p`
  margin: 0.5rem 0;
  color: #6b7280;
  font-style: italic;
`

const GradingForm = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
  margin-top: 1.5rem;
  padding-top: 1.5rem;
  border-top: 1px solid #e5e7eb;
`

const FormGroup = styled.div`
  display: flex;
  flex-direction: column;
`

const Label = styled.label`
  font-weight: 600;
  color: #374151;
  margin-bottom: 0.5rem;
  font-size: 0.95rem;
`

const Input = styled.input`
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 4px;
  font-size: 0.95rem;

  &:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }
`

const TextArea = styled.textarea`
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 4px;
  font-size: 0.95rem;
  resize: vertical;
  min-height: 80px;
  font-family: inherit;

  &:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }
`

const ButtonGroup = styled.div`
  display: flex;
  gap: 0.75rem;
  margin-top: 1.5rem;
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

const WarningMessage = styled.div`
  background-color: #fffbeb;
  border-left: 4px solid #d97706;
  color: #92400e;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
`

const Badge = styled.span`
  display: inline-block;
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  background-color: #fef3c7;
  color: #92400e;
`

const StudentListContainer = styled.div`
  display: grid;
  grid-template-columns: 1fr 2fr;
  gap: 2rem;
  margin-bottom: 2rem;

  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }
`

const StudentList = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  overflow: hidden;
`

const StudentListHeader = styled.div`
  background-color: #f3f4f6;
  padding: 1rem;
  border-bottom: 1px solid #e5e7eb;
  font-weight: 600;
  color: #374151;
`

const StudentListContent = styled.div`
  max-height: 500px;
  overflow-y: auto;
`

const StudentListItem = styled.div`
  padding: 1rem;
  border-bottom: 1px solid #e5e7eb;
  cursor: pointer;
  transition: background-color 0.3s ease;
  background-color: ${(props) => (props.$selected ? '#dbeafe' : 'white')};
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
`

const StudentEmail = styled.div`
  font-size: 0.875rem;
  color: #6b7280;
  margin-bottom: 0.5rem;
`

const StudentMeta = styled.div`
  display: flex;
  gap: 0.5rem;
  font-size: 0.8rem;
  color: #9ca3af;

  span {
    padding: 0.125rem 0.5rem;
    background-color: #f3f4f6;
    border-radius: 3px;
  }
`

const StudentDetailPanel = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
`

const StudentDetailHeader = styled.div`
  padding: 1.5rem;
  border-bottom: 1px solid #e5e7eb;
`

const StudentDetailTitle = styled.h2`
  margin: 0 0 0.5rem 0;
  color: #1f2937;
  font-size: 1.25rem;
`

const StudentDetailMeta = styled.div`
  display: flex;
  gap: 2rem;
  flex-wrap: wrap;
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e5e7eb;
`

const MetaItem = styled.div`
  display: flex;
  flex-direction: column;
`

const MetaLabel = styled.span`
  font-size: 0.875rem;
  color: #6b7280;
  font-weight: 600;
  margin-bottom: 0.25rem;
`

const MetaValue = styled.span`
  font-size: 1.125rem;
  font-weight: bold;
  color: #1f2937;
`

const StudentDetailContent = styled.div`
  padding: 1.5rem;
  max-height: 600px;
  overflow-y: auto;
`

const ResponseItemInPanel = styled.div`
  padding: 1rem;
  border: 1px solid #e5e7eb;
  border-radius: 4px;
  margin-bottom: 1rem;
  background-color: #f9fafb;

  &:last-child {
    margin-bottom: 0;
  }
`

const ResponseItemQuestion = styled.div`
  font-weight: 600;
  color: #1f2937;
  margin-bottom: 0.5rem;
`

const ResponseItemAnswer = styled.div`
  color: #6b7280;
  font-size: 0.95rem;
  margin-bottom: 0.75rem;
  max-height: 60px;
  overflow-y: auto;
`

const ResponseItemMarks = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 0.75rem;
  border-top: 1px solid #e5e7eb;
`

const ResponseStatus = styled.span`
  font-size: 0.8rem;
  font-weight: 600;
  padding: 0.25rem 0.75rem;
  border-radius: 3px;
  background-color: ${(props) => {
    switch (props.$status) {
      case 'Pending':
        return '#fef3c7'
      case 'Graded':
        return '#d1fae5'
      default:
        return '#e5e7eb'
    }
  }};
  color: ${(props) => {
    switch (props.$status) {
      case 'Pending':
        return '#92400e'
      case 'Graded':
        return '#065f46'
      default:
        return '#374151'
    }
  }};
`

export default function GradingDashboard() {
  const navigate = useNavigate()
  const { examId } = useParams()
  const { user, logout } = useAuth()

  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [responses, setResponses] = useState([])
  const [gradingMode, setGradingMode] = useState('overview') // overview, byStudent, byQuestion
  const [selectedStudent, setSelectedStudent] = useState(null)
  const [filterBy, setFilterBy] = useState('all') // all, byStudent, byQuestion
  const [selectedFilter, setSelectedFilter] = useState('')
  const [gradingData, setGradingData] = useState({})
  const [editingResponseId, setEditingResponseId] = useState(null)
  const [isExamPublished, setIsExamPublished] = useState(false)

  useEffect(() => {
    loadPendingResponses()
    loadPublicationStatus()
  }, [examId])

  const loadPublicationStatus = async () => {
    try {
      const status = await resultService.getPublicationStatus(examId)
      setIsExamPublished(status?.isPublished || false)
    } catch (err) {
      console.error('Failed to load publication status:', err)
    }
  }

  const loadPendingResponses = async () => {
    try {
      setLoading(true)
      setError('')
      const response = await teacherService.getPendingResponses(examId)
      
      // Handle different response structures
      if (response.data) {
        setResponses(response.data.responses || response.data || [])
      } else if (response.responses) {
        setResponses(response.responses || [])
      } else if (Array.isArray(response)) {
        setResponses(response)
      } else {
        setResponses([])
      }
    } catch (err) {
      // Check if error is about exam not ended
      if (err.message && err.message.includes('has not ended yet')) {
        setError(err.message)
      } else if (err.message) {
        setError(err.message)
      } else {
        setError('Failed to load pending responses')
      }
      console.error(err)
      setResponses([])
    } finally {
      setLoading(false)
    }
  }

  const handleGradeResponse = async (responseId) => {
    if (!gradingData[responseId] || gradingData[responseId].marksObtained === '') {
      setError('Please enter marks for the response')
      return
    }

    try {
      setSaving(true)
      setError('')
      const marksValue = parseInt(gradingData[responseId].marksObtained)
      
      // Validate marks
      const response = filteredResponses.concat(studentResponses).find((r) => r.responseId === responseId)
      if (response && marksValue > response.maxMarks) {
        setError(`Marks cannot exceed ${response.maxMarks}`)
        setSaving(false)
        return
      }
      
      await teacherService.gradeSingleResponse(responseId, {
        marksObtained: marksValue,
        feedback: gradingData[responseId].feedback || '',
        comment: gradingData[responseId].comment || '',
        isPartialCredit: false,
      })
      setSuccess('Response graded successfully!')
      setEditingResponseId(null)
      loadPendingResponses()
      setGradingData({})
      // Clear success message after 3 seconds
      setTimeout(() => setSuccess(''), 3000)
    } catch (err) {
      console.error('Grading error:', err)
      // Handle different error formats
      if (typeof err === 'object' && err.message) {
        setError('Failed to grade response: ' + err.message)
      } else if (typeof err === 'string') {
        setError('Failed to grade response: ' + err)
      } else {
        setError('Failed to grade response: Unknown error occurred. Check console for details.')
      }
    } finally {
      setSaving(false)
    }
  }

  const handleUpdateGradingData = (responseId, field, value) => {
    setGradingData((prev) => ({
      ...prev,
      [responseId]: {
        ...prev[responseId],
        [field]: value,
      },
    }))
  }

  const handleSelectStudent = (studentId) => {
    setSelectedStudent(studentId)
    setEditingResponseId(null)
    setError('')
  }

  const handleLogout = async () => {
    try {
      logout()
      navigate('/login')
    } catch (err) {
      console.error('Logout failed:', err)
    }
  }

  // Get unique students with their pending response count
  const getStudentList = () => {
    const studentMap = new Map()
    responses.forEach((response) => {
      if (!studentMap.has(response.studentId)) {
        studentMap.set(response.studentId, {
          studentId: response.studentId,
          studentName: response.studentName,
          studentEmail: response.studentEmail,
          pendingCount: 0,
          gradedCount: 0,
          totalCount: 0,
        })
      }
      const student = studentMap.get(response.studentId)
      student.totalCount += 1
      if (response.isGraded === true) {
        student.gradedCount += 1
      } else {
        student.pendingCount += 1
      }
    })
    return Array.from(studentMap.values())
  }

  // Get responses for selected student
  const getStudentResponses = () => {
    if (!selectedStudent) return []
    return responses.filter((r) => r.studentId === selectedStudent)
  }

  // Filter responses based on current filter
  const getFilteredResponses = () => {
    let filtered = responses
    if (filterBy === 'byStudent' && selectedFilter) {
      filtered = filtered.filter((r) => r.studentId === parseInt(selectedFilter))
    } else if (filterBy === 'byQuestion' && selectedFilter) {
      filtered = filtered.filter((r) => r.questionId === parseInt(selectedFilter))
    }
    // console.log("")
    // console.log(filtered)
    return filtered
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
          <LoadingSpinner>Loading pending responses...</LoadingSpinner>
        </MainContent>
      </Container>
    )
  }

  const studentList = getStudentList()
  const studentResponses = getStudentResponses()
  const filteredResponses = getFilteredResponses()
  const currentStudent = studentList.find((s) => s.studentId === selectedStudent)

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

        <PageTitle>Grade Responses</PageTitle>

        {error && <ErrorMessage>{error}</ErrorMessage>}
        {error && <ErrorMessage>{error}</ErrorMessage>}
        {isExamPublished && (
          <WarningMessage>
            ⚠️ This exam has been published. You cannot edit marks while the exam is published. Please unpublish the exam first to make changes.
          </WarningMessage>
        )}

        {responses.length === 0 ? (
          <Card>
            <EmptyState>
              <p>🎉 No pending responses! All responses have been graded.</p>
            </EmptyState>
          </Card>
        ) : (
          <>
            {/* Mode Selection */}
            <Card>
              <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', marginBottom: '1rem' }}>
                <div style={{ fontSize: '0.95rem', fontWeight: 600, color: '#6b7280', alignSelf: 'center' }}>
                  Grading Mode:
                </div>
                <button
                  onClick={() => {
                    setGradingMode('overview')
                    setSelectedStudent(null)
                    setFilterBy('all')
                  }}
                  style={{
                    padding: '0.5rem 1rem',
                    border: gradingMode === 'overview' ? '2px solid #3b82f6' : '1px solid #d1d5db',
                    borderRadius: '4px',
                    backgroundColor: gradingMode === 'overview' ? '#eff6ff' : 'white',
                    color: gradingMode === 'overview' ? '#3b82f6' : '#6b7280',
                    cursor: 'pointer',
                    fontWeight: 600,
                    fontSize: '0.95rem',
                  }}
                >
                  📋 Overview
                </button>
                <button
                  onClick={() => {
                    setGradingMode('byStudent')
                    setFilterBy('all')
                  }}
                  style={{
                    padding: '0.5rem 1rem',
                    border: gradingMode === 'byStudent' ? '2px solid #3b82f6' : '1px solid #d1d5db',
                    borderRadius: '4px',
                    backgroundColor: gradingMode === 'byStudent' ? '#eff6ff' : 'white',
                    color: gradingMode === 'byStudent' ? '#3b82f6' : '#6b7280',
                    cursor: 'pointer',
                    fontWeight: 600,
                    fontSize: '0.95rem',
                  }}
                >
                  👤 By Student
                </button>
                <button
                  onClick={() => {
                    setGradingMode('byQuestion')
                    setSelectedStudent(null)
                  }}
                  style={{
                    padding: '0.5rem 1rem',
                    border: gradingMode === 'byQuestion' ? '2px solid #3b82f6' : '1px solid #d1d5db',
                    borderRadius: '4px',
                    backgroundColor: gradingMode === 'byQuestion' ? '#eff6ff' : 'white',
                    color: gradingMode === 'byQuestion' ? '#3b82f6' : '#6b7280',
                    cursor: 'pointer',
                    fontWeight: 600,
                    fontSize: '0.95rem',
                  }}
                >
                  ❓ By Question
                </button>
              </div>
            </Card>

            {/* By Student Mode */}
            {gradingMode === 'byStudent' && (
              <StudentListContainer>
                <StudentList>
                  <StudentListHeader>
                    Students Pending Grading ({studentList.filter((s) => s.pendingCount > 0).length})
                  </StudentListHeader>
                  <StudentListContent>
                    {studentList.length === 0 ? (
                      <div style={{ padding: '1.5rem', textAlign: 'center', color: '#9ca3af' }}>
                        No students found
                      </div>
                    ) : (
                      studentList
                        .filter((s) => s.pendingCount > 0)
                        .map((student) => (
                          <StudentListItem
                            key={student.studentId}
                            $selected={selectedStudent === student.studentId}
                            onClick={() => handleSelectStudent(student.studentId)}
                          >
                            <StudentName>{student.studentName}</StudentName>
                            <StudentEmail>{student.studentEmail}</StudentEmail>
                            <StudentMeta>
                              <span>Pending: {student.pendingCount}</span>
                              <span>Graded: {student.gradedCount}</span>
                            </StudentMeta>
                          </StudentListItem>
                        ))
                    )}
                  </StudentListContent>
                </StudentList>

                {selectedStudent && currentStudent && (
                  <StudentDetailPanel>
                    <StudentDetailHeader>
                      <StudentDetailTitle>{currentStudent.studentName}</StudentDetailTitle>
                      <StudentDetailMeta>
                        <MetaItem>
                          <MetaLabel>Total Responses</MetaLabel>
                          <MetaValue>{currentStudent.totalCount}</MetaValue>
                        </MetaItem>
                        <MetaItem>
                          <MetaLabel>Graded</MetaLabel>
                          <MetaValue style={{ color: '#16a34a' }}>{currentStudent.gradedCount}</MetaValue>
                        </MetaItem>
                        <MetaItem>
                          <MetaLabel>Pending</MetaLabel>
                          <MetaValue style={{ color: '#dc2626' }}>{currentStudent.pendingCount}</MetaValue>
                        </MetaItem>
                      </StudentDetailMeta>
                    </StudentDetailHeader>

                    <StudentDetailContent>
                      {studentResponses.length === 0 ? (
                        <div style={{ textAlign: 'center', color: '#6b7280', padding: '2rem' }}>
                          No responses from this student
                        </div>
                      ) : (
                        studentResponses.map((response) => (
                          <ResponseItemInPanel key={response.responseId}>
                            <ResponseItemQuestion>
                              Q{response.questionId}. {response.questionText?.substring(0, 60)}...
                            </ResponseItemQuestion>
                            <ResponseItemAnswer>
                              <strong>Answer:</strong> {response.studentAnswer?.substring(0, 100)}...
                            </ResponseItemAnswer>
                            <ResponseItemMarks>
                              <div>
                                <ResponseStatus $status={response.isGraded ? 'Graded' : 'Pending'}>
                                  {response.isGraded ? 'Graded' : 'Pending'}
                                </ResponseStatus>
                              </div>
                              <PrimaryButton
                                onClick={() => setEditingResponseId(response.responseId)}
                                disabled={isExamPublished}
                                title={isExamPublished ? 'Cannot edit marks - exam is published' : ''}
                              >
                                {response.isGraded ? 'Edit' : 'Grade'}
                              </PrimaryButton>
                            </ResponseItemMarks>
                          </ResponseItemInPanel>
                        ))
                      )}
                    </StudentDetailContent>
                  </StudentDetailPanel>
                )}
              </StudentListContainer>
            )}

            {/* Overview or By Question Mode */}
            {(gradingMode === 'overview' || gradingMode === 'byQuestion') && (
              <>
                {gradingMode === 'byQuestion' && (
                  <FilterBar>
                    <FilterGroup>
                      <FilterLabel>Select Question</FilterLabel>
                      <Select value={selectedFilter} onChange={(e) => setSelectedFilter(e.target.value)}>
                        <option value="">All Questions</option>
                        {[...new Set(responses.map((r) => r.questionId))].map((questionId) => (
                          <option key={questionId} value={questionId}>
                            Question {questionId}
                          </option>
                        ))}
                      </Select>
                    </FilterGroup>
                  </FilterBar>
                )}

                <Card>
                  <Table>
                    <thead>
                      <tr>
                        <th>Student</th>
                        <th>Question</th>
                        <th>Student's Answer</th>
                        <th>Marks</th>
                        <th>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {filteredResponses.map((response) => (
                        <tr key={response.responseId}>
                          <td>{response.studentName}</td>
                          <td>{response.questionText}</td>
                          <td>
                            <div style={{ maxWidth: '300px', wordWrap: 'break-word' }}>
                              {response.studentAnswer?.substring(0, 50)}...
                            </div>
                          </td>
                          <td>
                            <span>{response.marksObtained || 0}</span>
                            <span style={{ color: '#9ca3af' }}> / {response.maxMarks}</span>
                          </td>
                          <td>
                            <PrimaryButton
                              onClick={() => setEditingResponseId(response.responseId)}
                              disabled={isExamPublished}
                              title={isExamPublished ? 'Cannot edit marks - exam is published' : ''}
                            >
                              {response.isGraded ? 'Edit' : 'Grade'}
                            </PrimaryButton>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </Table>
                </Card>
              </>
            )}

            {/* Grading Modal */}
            {editingResponseId && (
              <Card>
                {filteredResponses.concat(studentResponses).find((r) => r.responseId === editingResponseId) && (
                  <div key={editingResponseId}>
                    {(() => {
                      const response = filteredResponses.concat(studentResponses).find((r) => r.responseId === editingResponseId)
                      return (
                        <>
                          <QuestionPreview>
                            <QuestionText>{response.questionText}</QuestionText>
                            <StudentAnswer>
                              <strong>Student's Answer:</strong> {response.studentAnswer}
                            </StudentAnswer>
                          </QuestionPreview>

                          <GradingForm>
                            <FormGroup style={{ gridColumn: '1 / -1' }}>
                              <Label htmlFor="marks">Marks Obtained (Max: {response.maxMarks}) *</Label>
                              <Input
                                id="marks"
                                type="number"
                                min="0"
                                max={response.maxMarks}
                                value={gradingData[editingResponseId]?.marksObtained || ''}
                                onChange={(e) =>
                                  handleUpdateGradingData(editingResponseId, 'marksObtained', e.target.value)
                                }
                                placeholder="Enter marks"
                              />
                            </FormGroup>

                            <FormGroup style={{ gridColumn: '1 / -1' }}>
                              <Label htmlFor="feedback">Feedback</Label>
                              <TextArea
                                id="feedback"
                                value={gradingData[editingResponseId]?.feedback || ''}
                                onChange={(e) =>
                                  handleUpdateGradingData(editingResponseId, 'feedback', e.target.value)
                                }
                                placeholder="Provide feedback to the student (optional)"
                              />
                            </FormGroup>

                            <ButtonGroup style={{ gridColumn: '1 / -1' }}>
                              <SecondaryButton onClick={() => setEditingResponseId(null)}>
                                Cancel
                              </SecondaryButton>
                              <PrimaryButton
                                onClick={() => handleGradeResponse(editingResponseId)}
                                disabled={saving}
                              >
                                {saving ? 'Saving...' : 'Submit Grade'}
                              </PrimaryButton>
                            </ButtonGroup>
                          </GradingForm>
                        </>
                      )
                    })()}
                  </div>
                )}
              </Card>
            )}
          </>
        )}
      </MainContent>
    </Container>
  )
}
