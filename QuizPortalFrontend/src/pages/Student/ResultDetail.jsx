import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
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
  margin: 0 0 0.5rem 0;
  color: #1f2937;
  font-size: 1.875rem;
`

const SummaryCard = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  padding: 2rem;
  margin-bottom: 2rem;
`

const SummaryGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 2rem;
  margin-top: 1.5rem;
`

const SummaryItem = styled.div`
  text-align: center;
`

const SummaryLabel = styled.div`
  font-size: 0.875rem;
  color: #6b7280;
  margin-bottom: 0.5rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
`

const SummaryValue = styled.div`
  font-size: 2rem;
  font-weight: bold;
  color: #1f2937;
`

const SummarySubValue = styled.div`
  font-size: 0.875rem;
  color: #6b7280;
  margin-top: 0.25rem;
`

const StatusBadge = styled.span`
  display: inline-block;
  padding: 0.5rem 1rem;
  border-radius: 9999px;
  font-size: 0.875rem;
  font-weight: 600;
  background-color: ${props => props.isPassed ? '#dcfce7' : '#fee2e2'};
  color: ${props => props.isPassed ? '#15803d' : '#991b1b'};
`

const SectionTitle = styled.h2`
  margin: 2rem 0 1rem 0;
  color: #1f2937;
  font-size: 1.5rem;
`

const QuestionCard = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  padding: 1.5rem;
  margin-bottom: 1rem;
  border-left: 4px solid ${props => {
    if (props.isCorrect) return '#16a34a';
    if (props.isAnswered) return '#dc2626';
    return '#6b7280';
  }};
`

const QuestionHeader = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: start;
  margin-bottom: 1rem;
`

const QuestionNumber = styled.span`
  font-weight: 600;
  color: #374151;
  font-size: 0.875rem;
`

const QuestionBadge = styled.span`
  padding: 0.25rem 0.75rem;
  border-radius: 4px;
  font-size: 0.75rem;
  font-weight: 500;
  background-color: ${props => {
    if (props.isCorrect) return '#dcfce7';
    if (props.isAnswered) return '#fee2e2';
    return '#f3f4f6';
  }};
  color: ${props => {
    if (props.isCorrect) return '#15803d';
    if (props.isAnswered) return '#991b1b';
    return '#6b7280';
  }};
`

const QuestionText = styled.p`
  margin: 0 0 1rem 0;
  color: #1f2937;
  font-size: 1rem;
  line-height: 1.5;
`

const AnswerSection = styled.div`
  background-color: #f9fafb;
  padding: 1rem;
  border-radius: 4px;
  margin-top: 1rem;
`

const AnswerLabel = styled.div`
  font-size: 0.875rem;
  font-weight: 600;
  color: #374151;
  margin-bottom: 0.5rem;
`

const AnswerText = styled.div`
  color: #1f2937;
  font-size: 0.95rem;
  padding: 0.5rem;
  background-color: white;
  border-radius: 4px;
  border: 1px solid #e5e7eb;
`

const MarksDisplay = styled.div`
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e5e7eb;
`

const MarksLabel = styled.span`
  font-size: 0.875rem;
  color: #6b7280;
`

const MarksValue = styled.span`
  font-size: 0.95rem;
  font-weight: 600;
  color: ${props => props.isCorrect ? '#16a34a' : '#dc2626'};
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

const ErrorMessage = styled.div`
  background-color: #fee;
  border: 1px solid #dc2626;
  color: #991b1b;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
`

export default function ResultDetail() {
  const navigate = useNavigate()
  const { examId } = useParams()
  const { user, logout } = useAuth()
  const [resultDetails, setResultDetails] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    loadResultDetails()
  }, [examId])

  const loadResultDetails = async () => {
    try {
      setLoading(true)
      setError('')
      
      const data = await resultService.getExamResultDetails(examId)
      console.log('Result Details:', data)
      
      setResultDetails(data)
    } catch (err) {
      console.error('Error loading result details:', err)
      setError(err.message || 'Failed to load result details')
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

  const getGrade = (percentage) => {
    if (percentage >= 90) return 'A++'
    if (percentage >= 80) return 'A+'
    if (percentage >= 70) return 'B'
    if (percentage >= 60) return 'C'
    if (percentage >= 50) return 'D'
    return 'F'
  }

  if (loading) {
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
          <div style={{ textAlign: 'center', padding: '3rem' }}>
            <LoadingSpinner />
            <p style={{ marginTop: '1rem', color: '#6b7280' }}>Loading result details...</p>
          </div>
        </MainContent>
      </Container>
    )
  }

  if (error) {
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
          <BackButton onClick={() => navigate('/student/results')}>← Back to Results</BackButton>
          <ErrorMessage>{error}</ErrorMessage>
        </MainContent>
      </Container>
    )
  }

  const percentage = resultDetails?.percentage || 0
  const isPassed = percentage >= 50
  const correctCount = resultDetails?.correctAnswers || 0
  const totalQuestions = resultDetails?.totalQuestions || 0
  const unansweredCount = resultDetails?.unansweredCount || 0

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
        <BackButton onClick={() => navigate('/student/results')}>← Back to Results</BackButton>
        <PageTitle>{resultDetails?.examName || 'Exam Result'}</PageTitle>

        <SummaryCard>
          <div style={{ textAlign: 'center', marginBottom: '1rem' }}>
            <StatusBadge isPassed={isPassed}>
              {isPassed ? '✓ Passed' : '✗ Failed'}
            </StatusBadge>
          </div>

          <SummaryGrid>
            <SummaryItem>
              <SummaryLabel>Score</SummaryLabel>
              <SummaryValue>{percentage.toFixed(1)}%</SummaryValue>
              <SummarySubValue>Grade: {getGrade(percentage)}</SummarySubValue>
            </SummaryItem>

            <SummaryItem>
              <SummaryLabel>Marks Obtained</SummaryLabel>
              <SummaryValue>{resultDetails?.totalMarks?.toFixed(2) || 0}</SummaryValue>
              <SummarySubValue>out of {resultDetails?.examTotalMarks || 'N/A'}</SummarySubValue>
            </SummaryItem>

            <SummaryItem>
              <SummaryLabel>Correct Answers</SummaryLabel>
              <SummaryValue>{correctCount}</SummaryValue>
              <SummarySubValue>out of {totalQuestions}</SummarySubValue>
            </SummaryItem>

            <SummaryItem>
              <SummaryLabel>Status</SummaryLabel>
              <SummaryValue style={{ fontSize: '1.25rem' }}>{resultDetails?.status || 'N/A'}</SummaryValue>
              <SummarySubValue>
                {unansweredCount > 0 ? `${unansweredCount} unanswered` : 'All answered'}
              </SummarySubValue>
            </SummaryItem>
          </SummaryGrid>
        </SummaryCard>

        <SectionTitle>Question-wise Results</SectionTitle>

        {resultDetails?.questionResults && resultDetails.questionResults.length > 0 ? (
          resultDetails.questionResults.map((question, index) => (
            <QuestionCard 
              key={question.questionID} 
              isCorrect={question.isCorrect}
              isAnswered={question.isAnswered}
            >
              <QuestionHeader>
                <QuestionNumber>Question {index + 1} ({question.questionType})</QuestionNumber>
                <QuestionBadge 
                  isCorrect={question.isCorrect}
                  isAnswered={question.isAnswered}
                >
                  {question.isCorrect ? '✓ Correct' : question.isAnswered ? '✗ Incorrect' : 'Not Answered'}
                </QuestionBadge>
              </QuestionHeader>

              <QuestionText>{question.questionText}</QuestionText>

              {question.isAnswered && (
                <AnswerSection>
                  <AnswerLabel>Your Answer:</AnswerLabel>
                  <AnswerText>{question.studentAnswer || 'No answer'}</AnswerText>
                </AnswerSection>
              )}

              {question.questionType === 'MCQ' && question.correctAnswer && (
                <AnswerSection style={{ marginTop: '0.5rem' }}>
                  <AnswerLabel>Correct Answer:</AnswerLabel>
                  <AnswerText style={{ borderColor: '#16a34a', color: '#15803d' }}>
                    {question.correctAnswer}
                  </AnswerText>
                </AnswerSection>
              )}

              <MarksDisplay>
                <MarksLabel>Marks:</MarksLabel>
                <MarksValue isCorrect={question.isCorrect}>
                  {question.marksObtained?.toFixed(2) || 0} / {question.maxMarks}
                </MarksValue>
              </MarksDisplay>
            </QuestionCard>
          ))
        ) : (
          <div style={{ textAlign: 'center', padding: '2rem', backgroundColor: 'white', borderRadius: '8px' }}>
            <p style={{ color: '#6b7280' }}>No question results available</p>
          </div>
        )}
      </MainContent>
    </Container>
  )
}
