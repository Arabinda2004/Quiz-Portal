import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { teacherService } from '../../services/api'
import TeacherLayout from '../../components/TeacherLayout'
import styled from 'styled-components'
import {
  FormContainer,
  FormSection,
  FormSectionTitle,
  FormGroup,
  Label,
  Input,
  TextArea,
  Select,
  ButtonGroup,
  PrimaryButton,
  SecondaryButton,
  FormError,
  FormSuccess,
  Alert,
  COLORS,
} from '../../styles/TeacherStyles'

const OptionsContainer = styled.div`
  background-color: rgba(30, 64, 175, 0.03);
  padding: 20px;
  border-radius: 12px;
  margin-top: 16px;
  border: 2px dashed ${COLORS.border};
`

const OptionItem = styled.div`
  display: flex;
  gap: 12px;
  margin-bottom: 12px;
  align-items: flex-start;
  padding: 16px;
  background: white;
  border-radius: 8px;
  border: 1px solid ${COLORS.border};
  transition: all 0.3s ease;

  &:hover {
    border-color: ${COLORS.primary};
    box-shadow: 0 2px 8px rgba(30, 64, 175, 0.1);
  }

  &:last-child {
    margin-bottom: 0;
  }
`

const RadioInput = styled.input`
  margin-top: 4px;
  cursor: pointer;
  width: 18px;
  height: 18px;
  accent-color: ${COLORS.success};

  &:disabled {
    cursor: not-allowed;
    opacity: 0.5;
  }
`

const OptionInput = styled(Input)`
  margin: 0;
  flex: 1;
`

const OptionLabel = styled.div`
  font-size: 12px;
  font-weight: 600;
  color: ${COLORS.textMuted};
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 4px;
`

const Helper = styled.p`
  color: ${COLORS.textSecondary};
  font-size: 13px;
  margin-top: 6px;
`

const QuestionTypeCard = styled.div`
  padding: 16px;
  border: 2px solid ${(props) => (props.$selected ? COLORS.primary : COLORS.border)};
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
  text-align: center;
  background-color: ${(props) => (props.$selected ? 'rgba(30, 64, 175, 0.05)' : 'white')};

  &:hover {
    border-color: ${COLORS.primary};
    box-shadow: 0 4px 12px rgba(30, 64, 175, 0.1);
  }

  .icon {
    font-size: 28px;
    margin-bottom: 8px;
  }

  h4 {
    font-size: 14px;
    font-weight: 600;
    color: ${COLORS.text};
    margin: 0 0 4px 0;
  }

  p {
    font-size: 12px;
    color: ${COLORS.textSecondary};
    margin: 0;
  }
`

const QuestionTypeGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
  margin-bottom: 20px;

  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }
`

const MarksPreview = styled.div`
  background-color: rgba(16, 185, 129, 0.05);
  border-left: 4px solid ${COLORS.success};
  border-radius: 8px;
  padding: 12px 14px;
  font-size: 13px;
  color: ${COLORS.text};
  margin-top: 12px;

  strong {
    color: ${COLORS.success};
  }
`

const LoadingSpinner = styled.div`
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;

  .spinner {
    width: 40px;
    height: 40px;
    border: 4px solid ${COLORS.border};
    border-top-color: ${COLORS.primary};
    border-radius: 50%;
    animation: spin 0.6s linear infinite;
  }

  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }
`

export default function QuestionForm() {
  const navigate = useNavigate()
  const { examId, questionId } = useParams()
  const { user, logout } = useAuth()
  const isEditMode = Boolean(questionId)

  const [loading, setLoading] = useState(isEditMode || true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [exam, setExam] = useState(null)
  const [examStatus, setExamStatus] = useState(null)
  const [allQuestions, setAllQuestions] = useState([])

  const [formData, setFormData] = useState({
    questionText: '',
    questionType: 'MCQ', // MCQ, Short Answer, Long Answer
    marks: 1,
    options: [
      { optionID: 1, optionText: '', isCorrect: true },
      { optionID: 2, optionText: '', isCorrect: false },
      { optionID: 3, optionText: '', isCorrect: false },
      { optionID: 4, optionText: '', isCorrect: false },
    ],
    correctAnswer: '', // For short/long answers
  })

  const [validationErrors, setValidationErrors] = useState({})

  useEffect(() => {
    loadExamData()
  }, [examId])

  useEffect(() => {
    if (isEditMode && exam) {
      loadQuestionData()
    }
  }, [questionId, isEditMode, exam?.examID])

  // Calculate exam status based on schedule times
  const calculateExamStatus = (examData) => {
    const now = new Date()
    const start = new Date(examData.scheduleStart)
    const end = new Date(examData.scheduleEnd)

    if (now < start) return 'Upcoming'
    if (now > end) return 'Ended'
    return 'Active'
  }

  const loadExamData = async () => {
    try {
      const response = await teacherService.getExamById(examId)
      const examData = response.data
      setExam(examData)
      const status = calculateExamStatus(examData)
      setExamStatus(status)

      // Load all questions to calculate total marks
      const questionsResponse = await teacherService.getQuestions(examId)
      const questions = questionsResponse.data || []
      setAllQuestions(questions)

      if (!isEditMode) {
        setLoading(false)
      }
    } catch (err) {
      setError('Failed to load exam information')
      console.error(err)
      setLoading(false)
    }
  }

  const loadQuestionData = async () => {
    try {
      setLoading(true)
      const response = await teacherService.getQuestionById(examId, questionId)
      const question = response.data

      if (question) {
        setFormData({
          questionText: question.questionText || '',
          questionType: question.questionType === 0 ? 'MCQ' : question.questionType === 1 ? 'ShortAnswer' : 'LongAnswer',
          marks: question.marks || 1,
          options: question.options && question.options.length > 0
            ? question.options.map((opt, idx) => ({
              optionID: opt.optionID,
              optionText: opt.optionText,
              isCorrect: opt.isCorrect
            }))
            : [
              { optionID: 1, optionText: '', isCorrect: true },
              { optionID: 2, optionText: '', isCorrect: false },
              { optionID: 3, optionText: '', isCorrect: false },
              { optionID: 4, optionText: '', isCorrect: false },
            ],
          correctAnswer: question.correctAnswer || '',
          explanation: question.explanation || '',
        })
      }
    } catch (err) {
      setError('Failed to load question data')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  const validateForm = () => {
    const errors = {}

    if (!formData.questionText.trim()) {
      errors.questionText = 'Question text is required'
    }

    const marksValue = parseFloat(formData.marks)
    if (isNaN(marksValue) || marksValue <= 0) {
      errors.marks = 'Marks must be a number greater than 0'
    }

    // Note: No validation for total marks limit since TotalMarks is now dynamically calculated from questions

    if (formData.questionType === 'MCQ') {
      // MCQ must have exactly 4 options
      const validOptions = formData.options.filter((opt) => opt.optionText.trim())
      if (validOptions.length !== 4) {
        errors.options = 'MCQ questions must have exactly 4 options with text'
      }

      // Exactly 1 correct option required
      const correctOptions = formData.options.filter((opt) => opt.isCorrect && opt.optionText.trim())
      if (correctOptions.length !== 1) {
        errors.options = 'MCQ questions must have exactly 1 correct option'
      }
    }
    // Note: No validation for correctAnswer for SAQ/LAQ as per requirement

    setValidationErrors(errors)
    return Object.keys(errors).length === 0
  }

  const handleInputChange = (e) => {
    const { name, value } = e.target
    setFormData((prev) => ({
      ...prev,
      [name]: name === 'marks' ? (value === '' ? '' : parseFloat(value)) : value,
    }))
    if (validationErrors[name]) {
      setValidationErrors((prev) => ({ ...prev, [name]: '' }))
    }
  }


  const handleQuestionTypeChange = (e) => {
    setFormData((prev) => ({
      ...prev,
      questionType: e.target.value,
    }))
  }

  const handleOptionChange = (index, field, value) => {
    const newOptions = [...formData.options]
    
    // If setting isCorrect to true, deselect all other options (radio button behavior)
    if (field === 'isCorrect' && value === true) {
      newOptions.forEach((opt, idx) => {
        opt.isCorrect = idx === index
      })
    } else {
      newOptions[index] = { ...newOptions[index], [field]: value }
    }
    
    setFormData((prev) => ({ ...prev, options: newOptions }))
  }

  const handleCancel = () => {
    navigate(`/teacher/exam/${examId}`)
  }

  const handleSubmit = async (e) => {
    e.preventDefault()

    // Check if exam is upcoming before allowing edit
    if (examStatus !== 'Upcoming') {
      setError(`Cannot modify questions for ${examStatus} exams. Questions can only be edited for upcoming exams.`)
      return
    }

    if (!validateForm()) {
      return
    }

    try {
      setSaving(true)
      setError('')
      setSuccess('')

      // Convert question type string to numeric value
      // MCQ = 0, SAQ = 1, Subjective = 2
      let questionTypeValue = 0
      if (formData.questionType === 'ShortAnswer') {
        questionTypeValue = 1
      } else if (formData.questionType === 'LongAnswer') {
        questionTypeValue = 2
      }

      // Prepare data based on question type
      const submitData = {
        questionText: formData.questionText,
        questionType: questionTypeValue,
        marks: parseFloat(formData.marks),
        negativeMarks: 0,
      }

      // Add options for MCQ only (type 0)
      if (questionTypeValue === 0) {
        submitData.options = formData.options.map((opt) => ({
          optionID: opt.optionID || null,  // Include optionID for updates (null for new options)
          optionText: opt.optionText,
          isCorrect: opt.isCorrect,
        }))
      }

      console.log('Submitting question data:', submitData)

      if (isEditMode) {
        // Update question
        await teacherService.updateQuestion(examId, questionId, submitData)
        setSuccess('Question updated successfully!')
        setTimeout(() => navigate(`/teacher/exam/${examId}`), 1500)
      } else {
        // Create question
        await teacherService.createQuestion(examId, submitData)
        setSuccess('Question created successfully!')
        setTimeout(() => navigate(`/teacher/exam/${examId}`), 1500)
      }
    } catch (err) {
      setError(err.message || 'Failed to save question')
      console.error(err)
    } finally {
      setSaving(false)
    }
  }

  const handleLogout = async () => {
    try {
      navigate('/login')
    } catch (err) {
      console.error('Logout failed:', err)
    }
  }

  if (loading) {
    return (
      <TeacherLayout pageTitle={isEditMode ? 'Edit Question' : 'Add Question'}>
        <FormContainer>
          <LoadingSpinner>
            <div className="spinner" />
          </LoadingSpinner>
        </FormContainer>
      </TeacherLayout>
    )
  }

  return (
    <TeacherLayout pageTitle={isEditMode ? 'Edit Question' : 'Add Question'}>
      <FormContainer maxWidth="900px">
        {/* Exam Status Alert */}
        {exam && examStatus && (
          <>
            {examStatus === 'Active' && (
              <Alert type="error">
                <span>✕</span>
                <div className="content">
                  <p>
                    <strong>This exam is ACTIVE.</strong> Questions cannot be modified while the exam is in progress. Students are currently taking the exam.
                  </p>
                </div>
              </Alert>
            )}
            {examStatus === 'Ended' && (
              <Alert type="error">
                <span>✕</span>
                <div className="content">
                  <p>
                    <strong>This exam has ENDED.</strong> Questions cannot be modified after the exam has concluded.
                  </p>
                </div>
              </Alert>
            )}
            {examStatus === 'Upcoming' && isEditMode && (
              <Alert type="warning">
                <span>✓</span>
                <div className="content">
                  <p>This exam is upcoming. You can modify this question before the exam starts.</p>
                </div>
              </Alert>
            )}
          </>
        )}

        {error && (
          <FormError>
            <span>⚠</span>
            <span>{error}</span>
          </FormError>
        )}
        {success && (
          <FormSuccess>
            <span>✓</span>
            <span>{success}</span>
          </FormSuccess>
        )}

        <form onSubmit={handleSubmit} disabled={examStatus !== 'Upcoming'}>
          {/* Section 1: Question */}
          <FormSection>
            <FormSectionTitle>
              Question Details
            </FormSectionTitle>

            <FormGroup>
              <Label htmlFor="questionText">
                Question <span style={{ color: COLORS.danger }}>*</span>
              </Label>
              <TextArea
                id="questionText"
                name="questionText"
                value={formData.questionText}
                onChange={handleInputChange}
                placeholder="Enter the question text. Be clear and concise..."
                disabled={examStatus !== 'Upcoming'}
                $error={!!validationErrors.questionText}
              />
              {validationErrors.questionText && (
                <FormError>{validationErrors.questionText}</FormError>
              )}
            </FormGroup>
          </FormSection>

          {/* Section 2: Question Type & Marks */}
          <FormSection>
            <FormSectionTitle>
              Question Type & Scoring
            </FormSectionTitle>

            <FormGroup>
              <Label>Question Type *</Label>
              <QuestionTypeGrid>
                <QuestionTypeCard
                  $selected={formData.questionType === 'MCQ'}
                  onClick={() => !disableChanges && handleQuestionTypeChange({ target: { value: 'MCQ' } })}
                  style={{ opacity: examStatus !== 'Upcoming' ? 0.5 : 1, cursor: examStatus !== 'Upcoming' ? 'not-allowed' : 'pointer' }}
                >
                  <div className="icon">✓</div>
                  <h4>Multiple Choice</h4>
                  <p>Select correct option</p>
                </QuestionTypeCard>
                <QuestionTypeCard
                  $selected={formData.questionType === 'ShortAnswer'}
                  onClick={() => !disableChanges && handleQuestionTypeChange({ target: { value: 'ShortAnswer' } })}
                  style={{ opacity: examStatus !== 'Upcoming' ? 0.5 : 1, cursor: examStatus !== 'Upcoming' ? 'not-allowed' : 'pointer' }}
                >
                  <div className="icon">T</div>
                  <h4>Short Answer</h4>
                  <p>Student provides answer</p>
                </QuestionTypeCard>
                <QuestionTypeCard
                  $selected={formData.questionType === 'LongAnswer'}
                  onClick={() => !disableChanges && handleQuestionTypeChange({ target: { value: 'LongAnswer' } })}
                  style={{ opacity: examStatus !== 'Upcoming' ? 0.5 : 1, cursor: examStatus !== 'Upcoming' ? 'not-allowed' : 'pointer' }}
                >
                  <div className="icon">¶</div>
                  <h4>Long Answer</h4>
                  <p>Detailed answer required</p>
                </QuestionTypeCard>
              </QuestionTypeGrid>
            </FormGroup>

            <FormGroup>
              <Label htmlFor="marks">
                Marks <span style={{ color: COLORS.danger }}>*</span>
              </Label>
              <Input
                id="marks"
                type="number"
                name="marks"
                value={formData.marks || ''}
                onChange={handleInputChange}
                min="1"
                step="1"
                disabled={examStatus !== 'Upcoming'}
                $error={!!validationErrors.marks}
              />
              {validationErrors.marks && <FormError>{validationErrors.marks}</FormError>}
              {exam && !validationErrors.marks && (
                <MarksPreview>
                  <strong>Current Question Marks:</strong> {formData.marks} | <strong>Other Questions:</strong> {allQuestions
                    .filter((q) => !isEditMode || parseInt(q.questionID) !== parseInt(questionId))
                    .reduce((sum, q) => sum + (parseFloat(q.marks) || 0), 0)
                    .toFixed(2)} | <strong>Total if saved:</strong> {(allQuestions
                      .filter((q) => !isEditMode || parseInt(q.questionID) !== parseInt(questionId))
                      .reduce((sum, q) => sum + (parseFloat(q.marks) || 0), 0) + parseFloat(formData.marks || 0))
                      .toFixed(2)}
                </MarksPreview>
              )}
            </FormGroup>
          </FormSection>

          {/* Section 3: MCQ Options */}
          {formData.questionType === 'MCQ' && (
            <FormSection>
              <FormSectionTitle>
                Options (Exactly 4 required)
              </FormSectionTitle>
              <Helper>Select the correct answer using the radio button. Students will see all 4 options in random order.</Helper>

              <OptionsContainer>
                {formData.options.map((option, index) => (
                  <OptionItem key={index}>
                    <RadioInput
                      type="radio"
                      name="correctOption"
                      checked={option.isCorrect}
                      onChange={(e) => handleOptionChange(index, 'isCorrect', e.target.checked)}
                      title="Mark as correct answer"
                      disabled={examStatus !== 'Upcoming'}
                    />
                    <div style={{ flex: 1 }}>
                      <OptionLabel>Option {String.fromCharCode(65 + index)}</OptionLabel>
                      <OptionInput
                        type="text"
                        value={option.optionText}
                        onChange={(e) => handleOptionChange(index, 'optionText', e.target.value)}
                        placeholder={`Enter option ${index + 1}`}
                        disabled={examStatus !== 'Upcoming'}
                      />
                    </div>
                  </OptionItem>
                ))}
              </OptionsContainer>

              {validationErrors.options && (
                <FormError style={{ marginTop: '12px' }}>
                  <span>⚠️</span>
                  <span>{validationErrors.options}</span>
                </FormError>
              )}
            </FormSection>
          )}

          {/* Section: Short/Long Answer Info */}
          {(formData.questionType === 'ShortAnswer' || formData.questionType === 'LongAnswer') && (
            <FormSection>
              <Alert type="info">
                <span>ℹ️</span>
                <div className="content">
                  <p>
                    Students will provide their own answers for {formData.questionType === 'ShortAnswer' ? 'short answer' : 'long answer'} questions. You can grade them manually after the exam ends.
                  </p>
                </div>
              </Alert>
            </FormSection>
          )}

          {/* Buttons */}
          <ButtonGroup>
            <SecondaryButton type="button" onClick={handleCancel}>
              ← Back to Exam
            </SecondaryButton>
            <PrimaryButton
              type="submit"
              disabled={saving || examStatus !== 'Upcoming'}
              title={examStatus !== 'Upcoming' ? `Cannot modify questions for ${examStatus} exams` : ''}
            >
              {saving ? 'Saving...' : isEditMode ? 'Update Question' : 'Add Question'}
            </PrimaryButton>
          </ButtonGroup>
        </form>
      </FormContainer>
    </TeacherLayout>
  )
}

const disableChanges = false
