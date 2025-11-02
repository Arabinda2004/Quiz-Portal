import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { studentService, authService } from '../../services/api'
import {
  ExamContainer,
  ExamHeader,
  ExamTitle,
  TimerSection,
  Timer,
  SubmitButton,
  ExamContent,
  LeftPanel,
  SectionTitle,
  QuestionsGrid,
  QuestionButton,
  LegendContainer,
  LegendItem,
  LegendBox,
  CenterPanel,
  QuestionHeader,
  QuestionNumber,
  QuestionMetadata,
  MetadataBadge,
  QuestionText,
  QuestionBody,
  QuestionInstruction,
  OptionsContainer,
  OptionLabel,
  RadioInput,
  OptionText,
  AnswerTextArea,
  ControlsContainer,
  PreviousButton,
  ClearButton,
  SaveNextButton,
  LoadingContainer,
  ErrorMessage,
  SuccessMessage,
  SubmissionModal,
  SubmissionModalContent,
  ModalTitle,
  ModalText,
  ModalStats,
  StatItem,
  StatLabel,
  StatValue,
  ModalButtons,
  CancelButton,
  ConfirmButton,
} from '../../styles/ExamStyles'

const QuestionType = {
  MCQ: 0,
  SAQ: 1,
  Subjective: 2,
}

const getQuestionTypeName = (type) => {
  switch (type) {
    case QuestionType.MCQ:
      return 'Multiple Choice'
    case QuestionType.SAQ:
      return 'Short Answer'
    case QuestionType.Subjective:
      return 'Subjective'
    default:
      return 'Unknown'
  }
}

export default function ExamInterface() {
  const navigate = useNavigate()
  const { examId } = useParams()
  const { user, logout } = useAuth()

  // State management
  const [exam, setExam] = useState(null)
  const [questions, setQuestions] = useState([])
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0)
  const [answers, setAnswers] = useState({}) // questionId -> answer
  const [questionStatus, setQuestionStatus] = useState({}) // questionId -> 'not-visited', 'answered', 'skipped'
  const [timeRemaining, setTimeRemaining] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [showSubmitModal, setShowSubmitModal] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [examCompleted, setExamCompleted] = useState(false)

  // Prevent browser back/navigation during exam
  useEffect(() => {
    const handleBeforeUnload = (e) => {
      if (!examCompleted) {
        e.preventDefault()
        e.returnValue = 'Are you sure you want to leave? Your exam progress may be lost.'
        return e.returnValue
      }
    }

    const handlePopState = (e) => {
      if (!examCompleted) {
        const confirmLeave = window.confirm('Are you sure you want to leave the exam? Your progress may be lost.')
        if (!confirmLeave) {
          window.history.pushState(null, '', window.location.pathname)
        } else {
          navigate('/student/dashboard')
        }
      }
    }

    // Add event listeners
    window.addEventListener('beforeunload', handleBeforeUnload)
    window.addEventListener('popstate', handlePopState)
    
    // Push a state to prevent accidental back navigation
    window.history.pushState(null, '', window.location.pathname)

    // Cleanup
    return () => {
      window.removeEventListener('beforeunload', handleBeforeUnload)
      window.removeEventListener('popstate', handlePopState)
    }
  }, [examCompleted, navigate])

  // Load exam and questions
  useEffect(() => {
    loadExamData()
  }, [examId])

  const loadExamData = async () => {
    try {
      setLoading(true)
      const response = await studentService.getExamWithQuestions(examId)
      
      if (response && response.data) {
        const examData = response.data
        setExam(examData)
        
        const examQuestions = examData.questions || []
        setQuestions(examQuestions)
        
        // Initialize answers and status
        const initialAnswers = {}
        const initialStatus = {}
        
        examQuestions.forEach(q => {
          initialAnswers[q.questionID] = ''
          initialStatus[q.questionID] = 'not-visited'
        })
        
        setAnswers(initialAnswers)
        setQuestionStatus(initialStatus)
        
        // Set timer
        setTimeRemaining(examData.durationMinutes * 60) // Convert to seconds
      }
    } catch (err) {
      const errorMessage = err.message || err || 'Failed to load exam'
      setError(errorMessage)
      console.error('Error loading exam:', err)
      
      // If the error indicates already submitted, redirect to dashboard after 3 seconds
      if (errorMessage.includes('already submitted')) {
        setTimeout(() => {
          navigate('/student/dashboard')
        }, 3000)
      }
    } finally {
      setLoading(false)
    }
  }

  // Timer effect
  useEffect(() => {
    if (timeRemaining <= 0 || !exam) return

    const timer = setInterval(() => {
      setTimeRemaining(prev => {
        if (prev <= 1) {
          handleAutoSubmit()
          return 0
        }
        return prev - 1
      })
    }, 1000)

    return () => clearInterval(timer)
  }, [timeRemaining, exam])

  const formatTime = (seconds) => {
    const hours = Math.floor(seconds / 3600)
    const minutes = Math.floor((seconds % 3600) / 60)
    const secs = seconds % 60

    return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(secs).padStart(2, '0')}`
  }

  const isLowTime = timeRemaining < 300 // Less than 5 minutes

  const currentQuestion = questions[currentQuestionIndex]

  // Get sections and group questions
  const getGroupedQuestions = () => {
    const sections = {}
    questions.forEach((q, index) => {
      const sectionKey = `Section ${Math.floor(index / 6) + 1}`
      if (!sections[sectionKey]) {
        sections[sectionKey] = []
      }
      sections[sectionKey].push({ ...q, index })
    })
    return sections
  }

  const handleAnswerChange = (value) => {
    const questionId = currentQuestion.questionID
    setAnswers(prev => ({
      ...prev,
      [questionId]: value,
    }))
  }

  const handleClearAnswer = () => {
    const questionId = currentQuestion.questionID
    setAnswers(prev => ({
      ...prev,
      [questionId]: '',
    }))
    setQuestionStatus(prev => ({
      ...prev,
      [questionId]: 'skipped',
    }))
  }

  const handleSave = async () => {
    const questionId = currentQuestion.questionID
    const answer = answers[questionId]

    if (answer && answer.trim()) {
      setQuestionStatus(prev => ({
        ...prev,
        [questionId]: 'answered',
      }))
      setSuccess('Answer saved!')
      setTimeout(() => setSuccess(''), 2000)
    } else {
      setQuestionStatus(prev => ({
        ...prev,
        [questionId]: 'skipped',
      }))
    }
  }

  const handleSaveAndNext = async () => {
    const questionId = currentQuestion.questionID
    const answer = answers[questionId]

    if (answer && answer.trim()) {
      setQuestionStatus(prev => ({
        ...prev,
        [questionId]: 'answered',
      }))
      setSuccess('Answer saved!')
      setTimeout(() => setSuccess(''), 2000)
    } else {
      setQuestionStatus(prev => ({
        ...prev,
        [questionId]: 'skipped',
      }))
    }

    // Move to next question
    if (currentQuestionIndex < questions.length - 1) {
      setCurrentQuestionIndex(prev => prev + 1)
    }
  }

  const handlePrevious = () => {
    if (currentQuestionIndex > 0) {
      setCurrentQuestionIndex(prev => prev - 1)
    }
  }

  const handleQuestionClick = (index) => {
    setCurrentQuestionIndex(index)
  }

  const countAnsweredQuestions = () => {
    return Object.values(questionStatus).filter(s => s === 'answered').length
  }

  const countSkippedQuestions = () => {
    return Object.values(questionStatus).filter(s => s === 'skipped').length
  }

  const handleSubmit = async () => {
    setShowSubmitModal(true)
  }

  const handleAutoSubmit = async () => {
    setShowSubmitModal(false)
    await submitExam()
  }

  const submitExam = async () => {
    setIsSubmitting(true)
    try {
      // Save the current question's status before submitting
      const currentQuestionId = currentQuestion?.questionID
      if (currentQuestionId) {
        const currentAnswer = answers[currentQuestionId]
        if (currentAnswer && currentAnswer.trim()) {
          setQuestionStatus(prev => ({
            ...prev,
            [currentQuestionId]: 'answered',
          }))
        }
      }

      const responses = []
      
      Object.entries(answers).forEach(([questionId, answer]) => {
        if (answer && answer.trim()) {
          responses.push({
            questionID: parseInt(questionId),
            answerText: answer,
          })
        }
      })

      // Submit all responses (if any)
      console.log(`Submitting ${responses.length} responses...`)
      if (responses.length > 0) {
        for (const response of responses) {
          await studentService.submitAnswer(examId, response)
        }
        console.log('All responses submitted successfully')
      } else {
        console.log('No responses to submit')
      }

      // Finalize exam submission - creates a Result record to prevent re-access
      console.log('Finalizing exam submission...')
      await studentService.finalizeExamSubmission(examId)
      console.log('Exam finalized successfully')

      // Mark exam as completed to allow navigation
      setExamCompleted(true)

      setSuccess('Exam submitted successfully!')
    //   console.log("Exam submitted successfully!")
      setTimeout(() => {
        navigate('/student/dashboard')
      }, 2000)
    } catch (err) {
      console.error('Exam submission error:', err)
      const errorMessage = err?.message || err?.error || err || 'Failed to submit exam'
      setError(errorMessage)
    } finally {
      setIsSubmitting(false)
    }
  }

  if (loading) {
    return (
      <ExamContainer>
        <LoadingContainer>Loading exam...</LoadingContainer>
      </ExamContainer>
    )
  }

  if (!exam || questions.length === 0) {
    return (
      <ExamContainer>
        <ExamHeader>
          <ExamTitle>Exam Not Available</ExamTitle>
        </ExamHeader>
        <ExamContent>
          <ErrorMessage>Unable to load exam. Please try again or contact support.</ErrorMessage>
        </ExamContent>
      </ExamContainer>
    )
  }

  const groupedQuestions = getGroupedQuestions()

  return (
    <ExamContainer>
      <ExamHeader>
        <ExamTitle>{exam.title}</ExamTitle>
        <TimerSection>
          <Timer isLowTime={isLowTime}>{formatTime(timeRemaining)}</Timer>
          <SubmitButton onClick={handleSubmit} disabled={isSubmitting}>
            Submit
          </SubmitButton>
        </TimerSection>
      </ExamHeader>

      <ExamContent>
        {/* Left Panel - Questions Navigator */}
        <LeftPanel>
          {Object.entries(groupedQuestions).map(([section, sectionQuestions]) => (
            <div key={section}>
              <SectionTitle>{section}</SectionTitle>
              <QuestionsGrid>
                {sectionQuestions.map(q => (
                  <QuestionButton
                    key={q.questionID}
                    isActive={currentQuestionIndex === q.index}
                    status={questionStatus[q.questionID]}
                    onClick={() => handleQuestionClick(q.index)}
                    title={`Question ${q.index + 1}`}
                  >
                    {q.index + 1}
                  </QuestionButton>
                ))}
              </QuestionsGrid>
              <div style={{ marginTop: '16px' }} />
            </div>
          ))}

          <LegendContainer>
            <SectionTitle style={{ marginBottom: '12px' }}>Legend</SectionTitle>
            <LegendItem>
              <LegendBox color="#10b981" bgColor="#ecfdf5" />
              <span>Answered</span>
            </LegendItem>
            <LegendItem>
              <LegendBox color="#f59e0b" bgColor="#fef3c7" />
              <span>Skipped</span>
            </LegendItem>
            <LegendItem>
              <LegendBox color="#e5e7eb" bgColor="white" />
              <span>Not Visited</span>
            </LegendItem>
          </LegendContainer>
        </LeftPanel>

        {/* Center Panel - Question and Answer */}
        <CenterPanel>
          {error && <ErrorMessage>{error}</ErrorMessage>}
          {success && <SuccessMessage>{success}</SuccessMessage>}

          {currentQuestion && (
            <>
              <QuestionHeader>
                <QuestionNumber>Question {currentQuestionIndex + 1} of {questions.length}</QuestionNumber>
                
                <QuestionMetadata>
                  <MetadataBadge>
                    <span>📝</span>
                    {getQuestionTypeName(currentQuestion.questionType)}
                  </MetadataBadge>
                  
                  <MetadataBadge>
                    <span>⭐</span>
                    {currentQuestion.marks} {currentQuestion.marks === 1 ? 'Mark' : 'Marks'}
                  </MetadataBadge>
                  
                  {currentQuestion.questionType === QuestionType.MCQ && currentQuestion.options && (
                    <MetadataBadge>
                      <span>🔢</span>
                      {currentQuestion.options.length} Options
                    </MetadataBadge>
                  )}
                </QuestionMetadata>
                
                <QuestionText>{currentQuestion.questionText}</QuestionText>
              </QuestionHeader>

              <QuestionBody>
                {/* Instruction based on question type */}
                {currentQuestion.questionType === QuestionType.MCQ && (
                  <QuestionInstruction>
                    <span>💡 Instruction:</span> Select the most appropriate answer from the options below.
                  </QuestionInstruction>
                )}
                {currentQuestion.questionType === QuestionType.SAQ && (
                  <QuestionInstruction>
                    <span>💡 Instruction:</span> Provide a brief and concise answer in the text area below.
                  </QuestionInstruction>
                )}
                {currentQuestion.questionType === QuestionType.Subjective && (
                  <QuestionInstruction>
                    <span>💡 Instruction:</span> Write a detailed answer with proper explanation and reasoning.
                  </QuestionInstruction>
                )}

                {/* MCQ Options */}
                {currentQuestion.questionType === QuestionType.MCQ && currentQuestion.options && (
                  <OptionsContainer>
                    {currentQuestion.options.map(option => (
                      <OptionLabel
                        key={option.optionID}
                        isSelected={answers[currentQuestion.questionID] === option.optionText}
                      >
                        <RadioInput
                          type="radio"
                          name={`question-${currentQuestion.questionID}`}
                          value={option.optionText}
                          checked={answers[currentQuestion.questionID] === option.optionText}
                          onChange={e => handleAnswerChange(e.target.value)}
                        />
                        <OptionText>{option.optionText}</OptionText>
                      </OptionLabel>
                    ))}
                  </OptionsContainer>
                )}

                {/* Descriptive/SAQ Answer */}
                {(currentQuestion.questionType === QuestionType.SAQ ||
                  currentQuestion.questionType === QuestionType.Subjective) && (
                  <AnswerTextArea
                    value={answers[currentQuestion.questionID] || ''}
                    onChange={e => handleAnswerChange(e.target.value)}
                    placeholder="Type your answer here..."
                  />
                )}
              </QuestionBody>

              {/* Controls */}
              <ControlsContainer>
                <PreviousButton
                  onClick={handlePrevious}
                  disabled={currentQuestionIndex === 0}
                >
                  ← Previous
                </PreviousButton>
                <ClearButton onClick={handleClearAnswer}>
                  Clear
                </ClearButton>
                <SaveNextButton 
                  onClick={currentQuestionIndex === questions.length - 1 ? handleSave : handleSaveAndNext}
                >
                  {currentQuestionIndex === questions.length - 1 ? 'Save' : 'Save & Next →'}
                </SaveNextButton>
              </ControlsContainer>
            </>
          )}
        </CenterPanel>
      </ExamContent>

      {/* Submit Confirmation Modal */}
      {showSubmitModal && (
        <SubmissionModal>
          <SubmissionModalContent>
            <ModalTitle>Submit Exam?</ModalTitle>
            <ModalText>
              Are you sure you want to submit your exam? You won't be able to make any changes after submission.
            </ModalText>

            <ModalStats>
              <StatItem>
                <StatLabel>Answered</StatLabel>
                <StatValue>{countAnsweredQuestions()}</StatValue>
              </StatItem>
              <StatItem>
                <StatLabel>Skipped</StatLabel>
                <StatValue>{countSkippedQuestions()}</StatValue>
              </StatItem>
            </ModalStats>

            <ModalButtons>
              <CancelButton onClick={() => setShowSubmitModal(false)} disabled={isSubmitting}>
                Cancel
              </CancelButton>
              <ConfirmButton onClick={submitExam} disabled={isSubmitting}>
                {isSubmitting ? 'Submitting...' : 'Submit'}
              </ConfirmButton>
            </ModalButtons>
          </SubmissionModalContent>
        </SubmissionModal>
      )}
    </ExamContainer>
  )
}
