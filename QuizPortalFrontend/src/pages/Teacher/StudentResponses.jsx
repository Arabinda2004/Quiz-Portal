import { useState, useEffect } from 'react'
import { useNavigate, useParams, useLocation } from 'react-router-dom'
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
  max-width: 1000px;
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
  margin: 0;
  font-size: 2rem;
  color: #1f2937;
  margin-bottom: 1rem;
`

const SubtitleText = styled.p`
  color: #6b7280;
  margin: 0.5rem 0 0 0;
  font-size: 1rem;
`

const ProgressInfo = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
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
  font-size: 1.25rem;
  font-weight: bold;
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

const Card = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  padding: 2rem;
  margin-bottom: 1.5rem;
`

const QuestionContainer = styled.div`
  margin-bottom: 2rem;
  padding-bottom: 2rem;
  border-bottom: 1px solid #e5e7eb;

  &:last-child {
    border-bottom: none;
    margin-bottom: 0;
    padding-bottom: 0;
  }
`

const QuestionText = styled.h3`
  color: #1f2937;
  margin: 0 0 1rem 0;
  font-size: 1.1rem;
`

const QuestionNumber = styled.span`
  color: #9ca3af;
  font-weight: 600;
  margin-right: 0.5rem;
`

const ResponseBox = styled.div`
  background-color: #f9fafb;
  border-left: 4px solid #3b82f6;
  padding: 1rem;
  border-radius: 4px;
  margin-top: 0.5rem;
`

const ResponseLabel = styled.span`
  display: block;
  color: #6b7280;
  font-size: 0.875rem;
  font-weight: 600;
  margin-bottom: 0.25rem;
`

const ResponseValue = styled.span`
  display: block;
  color: #1f2937;
  font-size: 0.95rem;
  word-break: break-word;
`

const StatusBadge = styled.span`
  display: inline-block;
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  margin-left: 0.5rem;
  background-color: ${(props) => {
    switch (props.status) {
      case 'Correct':
        return '#d1fae5'
      case 'Incorrect':
        return '#fee2e2'
      case 'Pending':
        return '#fef3c7'
      case 'NotAnswered':
        return '#e5e7eb'
      default:
        return '#e5e7eb'
    }
  }};
  color: ${(props) => {
    switch (props.status) {
      case 'Correct':
        return '#065f46'
      case 'Incorrect':
        return '#991b1b'
      case 'Pending':
        return '#92400e'
      case 'NotAnswered':
        return '#374151'
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

const SuccessMessage = styled.div`
  background-color: #f0fdf4;
  border-left: 4px solid #16a34a;
  color: #16a34a;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
`

const GradingButton = styled.button`
  padding: 0.5rem 1rem;
  background-color: #3b82f6;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.875rem;
  font-weight: 600;
  margin-left: 0.5rem;

  &:hover {
    background-color: #2563eb;
  }

  &:disabled {
    background-color: #9ca3af;
    cursor: not-allowed;
  }
`

const Modal = styled.div`
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

const ButtonGroup = styled.div`
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  margin-top: 2rem;
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

export default function StudentResponses() {
  const navigate = useNavigate()
  const location = useLocation()
  const { examId, studentId } = useParams()
  const { user, logout } = useAuth()
  const studentName = location.state?.studentName || 'Student'

  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [responses, setResponses] = useState(null)
  const [gradingModal, setGradingModal] = useState(null)
  const [gradingData, setGradingData] = useState({
    marksObtained: '',
    feedback: '',
  })
  const [savingGrade, setSavingGrade] = useState(false)

  useEffect(() => {
    loadStudentResponses()
  }, [examId, studentId])

  const loadStudentResponses = async () => {
    try {
      setLoading(true)
      const response = await teacherService.getStudentResponses(examId, studentId)
      console.log("Response: ", response)
      console.log("Responses data:", response?.data)
      if (response?.data) {
        response.data.forEach((r, idx) => {
          console.log(`Response ${idx}: marksObtained=${r.marksObtained}, feedback=${r.feedback}`)
        })
      }
      setResponses(response)
      // console.log(response)
      setError('')
    } catch (err) {
      setError('Failed to load student responses')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  // const handleOpenGradeModal = (response) => {
  //   setGradingModal(response)
  //   setGradingData({
  //     marksObtained: '',
  //     feedback: '',
  //   })
  //   setError('')
  // }

  const handleCloseGradeModal = () => {
    setGradingModal(null)
    setGradingData({
      marksObtained: '',
      feedback: '',
    })
  }

  const handleGradeInputChange = (field, value) => {
    setGradingData((prev) => ({
      ...prev,
      [field]: value,
    }))
  }

  const handleSubmitGrade = async () => {
    if (gradingData.marksObtained === '') {
      setError('Please enter marks for this response')
      return
    }

    const marks = parseInt(gradingData.marksObtained)
    const maxMarks = gradingModal.questionMarks
    if (marks < 0 || marks > maxMarks) {
      setError(`Marks must be between 0 and ${maxMarks}`)
      return
    }

    try {
      setSavingGrade(true)
      setError('')
      const responseId = gradingModal.responseID
      await teacherService.gradeSingleResponse(responseId, {
        marksObtained: marks,
        feedback: gradingData.feedback,
      })
      setSuccess('Response graded successfully!')
      handleCloseGradeModal()
      loadStudentResponses()
      // Clear success message after 3 seconds
      setTimeout(() => setSuccess(''), 3000)
    } catch (err) {
      setError('Failed to grade response: ' + (err.message || 'Unknown error'))
      console.error(err)
    } finally {
      setSavingGrade(false)
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
          <LoadingSpinner>Loading student responses...</LoadingSpinner>
        </MainContent>
      </Container>
    )
  }

  if (!responses) {
    return (
      <Container>
        <NavBar>
          <NavLeft>
            <Logo onClick={() => navigate('/teacher/dashboard')}>Quiz Portal</Logo>
          </NavLeft>
        </NavBar>
        <MainContent>
          <ErrorMessage>No responses found for this student</ErrorMessage>
          <BackButton onClick={() => navigate(-1)}>← Go Back</BackButton>
        </MainContent>
      </Container>
    )
  }

  const progressPercentage = responses.totalQuestions > 0
    ? ((responses.answeredQuestions / responses.totalQuestions) * 100).toFixed(0)
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
        <BackButton onClick={() => navigate(-1)}>← Back</BackButton>

        {error && <ErrorMessage>{error}</ErrorMessage>}
        {success && <SuccessMessage>{success}</SuccessMessage>}

        <Header>
          <PageTitle>{studentName}'s Responses</PageTitle>
          <SubtitleText>Exam: {responses.examName}</SubtitleText>

          <ProgressInfo>
            <InfoItem>
              <InfoLabel>Questions</InfoLabel>
              <InfoValue>{responses.totalQuestions}</InfoValue>
            </InfoItem>
            <InfoItem>
              <InfoLabel>Answered</InfoLabel>
              <InfoValue>{responses.answeredQuestions}</InfoValue>
            </InfoItem>
            <InfoItem>
              <InfoLabel>Unanswered</InfoLabel>
              <InfoValue>{responses.unansweredQuestions}</InfoValue>
            </InfoItem>
            <InfoItem>
              <InfoLabel>Progress</InfoLabel>
              <InfoValue>{progressPercentage}%</InfoValue>
              <ProgressBar>
                <ProgressFill percentage={progressPercentage} />
              </ProgressBar>
            </InfoItem>
          </ProgressInfo>
        </Header>

        <Card>
          {responses.data && responses.data.length > 0 ? (
            <>
              {responses.data.map((response, index) => {
                return (
                  <QuestionContainer key={response.responseID}>
                    <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
                      <QuestionText>
                        <QuestionNumber>Q{index + 1}.</QuestionNumber>
                        {response.questionText.substring(0, 80)}...
                      </QuestionText>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                        <StatusBadge status="Pending">
                          {response.questionType === 1 ? 'Multiple Choice' : 'Short Answer'}
                        </StatusBadge>
                        {/* <GradingButton onClick={() => handleOpenGradeModal(response)}>
                          Grade
                        </GradingButton> */}
                      </div>
                    </div>

                    <ResponseBox>
                      <ResponseLabel>Student's Answer:</ResponseLabel>
                      <ResponseValue>
                        {response.answerText || <em>Not answered</em>} 
                      </ResponseValue>

                      <ResponseLabel style={{ marginTop: '1rem' }}>
                        Marks: {response.isGraded 
                          ? `${response.marksObtained} / ${response.questionMarks}` 
                          : <em style={{ color: '#f59e0b', fontWeight: 600 }}>Not Graded Yet</em>
                        }
                      </ResponseLabel>
                      {/* {response.feedback && (
                        <>
                          <ResponseLabel style={{ marginTop: '1rem' }}>Feedback:</ResponseLabel>
                          <ResponseValue>{response.feedback}</ResponseValue>
                        </>
                      )} */}
                      {/* {response.isCorrect !== null && response.isCorrect !== undefined && (
                        <>
                          <ResponseLabel style={{ marginTop: '1rem' }}>Status:</ResponseLabel>
                          <StatusBadge status={response.isCorrect ? 'Correct' : 'Incorrect'}>
                            {response.isCorrect ? 'Correct' : 'Incorrect'}
                          </StatusBadge>
                        </>
                      )} */}
                    </ResponseBox>
                  </QuestionContainer>
                )
              })}
            </>
          ) : (
            <div style={{ textAlign: 'center', padding: '2rem', color: '#6b7280' }}>
              No responses available for this exam
            </div>
          )}
        </Card>

        {/* Grading Modal */}
        {gradingModal && (
          <Modal onClick={handleCloseGradeModal}>
            <ModalContent onClick={(e) => e.stopPropagation()}>
              <ModalTitle>Grade Response</ModalTitle>

              <FormGroup>
                <Label>Question</Label>
                <div style={{ 
                  backgroundColor: '#f9fafb', 
                  padding: '1rem', 
                  borderRadius: '4px',
                  marginTop: '0.5rem'
                }}>
                  {gradingModal.questionText}
                </div>
              </FormGroup>

              <FormGroup>
                <Label>Student's Answer</Label>
                <div style={{ 
                  backgroundColor: '#f9fafb', 
                  padding: '1rem', 
                  borderRadius: '4px',
                  marginTop: '0.5rem',
                  maxHeight: '120px',
                  overflowY: 'auto'
                }}>
                  {gradingModal.answerText || <em>Not answered</em>}
                </div>
              </FormGroup>

              <FormGroup>
                <Label>Marks Obtained (Max: {gradingModal.questionMarks})</Label>
                <Input
                  type="number"
                  min="0"
                  max={gradingModal.questionMarks}
                  value={gradingData.marksObtained}
                  onChange={(e) => handleGradeInputChange('marksObtained', e.target.value)}
                  placeholder="Enter marks"
                />
              </FormGroup>

              <FormGroup>
                <Label>Feedback (Optional)</Label>
                <Textarea
                  value={gradingData.feedback}
                  onChange={(e) => handleGradeInputChange('feedback', e.target.value)}
                  placeholder="Provide feedback to the student..."
                />
              </FormGroup>

              {error && (
                <div style={{
                  backgroundColor: '#fee',
                  border: '1px solid #fca5a5',
                  color: '#991b1b',
                  padding: '0.75rem',
                  borderRadius: '4px',
                  marginBottom: '1rem',
                  fontSize: '0.875rem'
                }}>
                  {error}
                </div>
              )}

              <ButtonGroup>
                <SecondaryButton onClick={handleCloseGradeModal}>
                  Cancel
                </SecondaryButton>
                <PrimaryButton 
                  onClick={handleSubmitGrade}
                  disabled={savingGrade}
                >
                  {savingGrade ? 'Saving...' : 'Submit Grade'}
                </PrimaryButton>
              </ButtonGroup>
            </ModalContent>
          </Modal>
        )}
      </MainContent>
    </Container>
  )
}
