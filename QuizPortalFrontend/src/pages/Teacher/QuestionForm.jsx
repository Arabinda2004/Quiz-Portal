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
  max-width: 900px;
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

const FormContainer = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  padding: 2rem;
`

const PageTitle = styled.h1`
  margin: 0 0 2rem 0;
  font-size: 2rem;
  color: #1f2937;
`

const FormGroup = styled.div`
  margin-bottom: 1.5rem;
`

const Label = styled.label`
  display: block;
  margin-bottom: 0.5rem;
  color: #374151;
  font-weight: 600;
  font-size: 0.95rem;
`

const Input = styled.input`
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 4px;
  font-size: 1rem;
  font-family: inherit;

  &:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }

  &:invalid {
    border-color: #ef4444;
  }

  &:disabled {
    background-color: #f3f4f6;
    color: #9ca3af;
    cursor: not-allowed;
  }
`

const TextArea = styled.textarea`
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 4px;
  font-size: 1rem;
  font-family: inherit;
  resize: vertical;
  min-height: 100px;

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

const Select = styled.select`
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 4px;
  font-size: 1rem;
  font-family: inherit;
  background-color: white;

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

const OptionsContainer = styled.div`
  background-color: #f9fafb;
  padding: 1.5rem;
  border-radius: 4px;
  margin-top: 1rem;
`

const OptionItem = styled.div`
  display: flex;
  gap: 0.75rem;
  margin-bottom: 1rem;
  align-items: flex-start;
  padding: 1rem;
  background: white;
  border-radius: 4px;
  border: 1px solid #e5e7eb;
`

const CheckboxInput = styled.input`
  margin-top: 0.5rem;
  cursor: pointer;
  width: 18px;
  height: 18px;

  &:disabled {
    cursor: not-allowed;
    opacity: 0.5;
  }
`

const OptionInput = styled(Input)`
  margin: 0;
`

const OptionControls = styled.div`
  display: flex;
  gap: 0.5rem;
`

const SmallButton = styled.button`
  padding: 0.5rem 0.75rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.875rem;
  font-weight: 600;
`

const RemoveButton = styled(SmallButton)`
  background-color: #ef4444;
  color: white;

  &:hover {
    background-color: #dc2626;
  }
`

const AddButton = styled(SmallButton)`
  background-color: #10b981;
  color: white;

  &:hover {
    background-color: #059669;
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
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;

  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
`

const SubmitButton = styled(Button)`
  background-color: #3b82f6;
  color: white;

  &:hover:not(:disabled) {
    background-color: #2563eb;
  }
`

const CancelButton = styled(Button)`
  background-color: #e5e7eb;
  color: #1f2937;

  &:hover {
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
  background-color: #ecfdf5;
  border-left: 4px solid #065f46;
  color: #065f46;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
`

const WarningMessage = styled.div`
  background-color: #fef3c7;
  border-left: 4px solid #92400e;
  color: #92400e;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
  font-weight: 500;
`

const RestrictedMessage = styled.div`
  background-color: #fee;
  border-left: 4px solid #991b1b;
  color: #991b1b;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
  font-weight: 600;
`

const Helper = styled.p`
  color: #6b7280;
  font-size: 0.875rem;
  margin-top: 0.25rem;
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

  const handleRemoveOption = (index) => {
    // MCQ must have exactly 4 options, prevent removing options for MCQ
    if (formData.questionType === 'MCQ' && formData.options.length <= 4) {
      setError('MCQ questions must have exactly 4 options')
      return
    }

    setFormData((prev) => ({
      ...prev,
      options: prev.options.filter((_, i) => i !== index),
    }))
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

  const handleCancel = () => {
    navigate(`/teacher/exam/${examId}`)
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
            <Logo>Quiz Portal</Logo>
          </NavLeft>
        </NavBar>
        <MainContent>
          <FormContainer>Loading...</FormContainer>
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
          <span>{user?.fullName} ({user?.role})</span>
          <LogoutButton onClick={handleLogout}>Logout</LogoutButton>
        </NavMenu>
      </NavBar>

      <MainContent>
        <BackButton onClick={handleCancel}>← Back to Exam</BackButton>

        <FormContainer>
          <PageTitle>{isEditMode ? 'Edit Question' : 'Add New Question'}</PageTitle>

          {error && <ErrorMessage>{error}</ErrorMessage>}
          {success && <SuccessMessage>{success}</SuccessMessage>}

          {/* Exam Status Warning */}
          {exam && examStatus && (
            <>
              {examStatus === 'Active' && (
                <RestrictedMessage>
                  ⛔ This exam is currently ACTIVE. Questions cannot be modified while the exam is in progress.
                </RestrictedMessage>
              )}
              {examStatus === 'Ended' && (
                <RestrictedMessage>
                  ⛔ This exam has ENDED. Questions cannot be modified after the exam has ended.
                </RestrictedMessage>
              )}
              {examStatus === 'Upcoming' && isEditMode && (
                <WarningMessage>
                  ✓ This exam is upcoming. You can modify this question before the exam starts.
                </WarningMessage>
              )}
            </>
          )}

          <form onSubmit={handleSubmit} disabled={examStatus !== 'Upcoming'}>
            {/* Question Text */}
            <FormGroup>
              <Label htmlFor="questionText">Question *</Label>
              <TextArea
                id="questionText"
                name="questionText"
                value={formData.questionText}
                onChange={handleInputChange}
                placeholder="Enter the question text..."
                disabled={examStatus !== 'Upcoming'}
              />
              {validationErrors.questionText && (
                <Helper style={{ color: '#ef4444' }}>{validationErrors.questionText}</Helper>
              )}
            </FormGroup>

            {/* Question Type */}
            <FormGroup>
              <Label htmlFor="questionType">Question Type *</Label>
              <Select
                value={formData.questionType}
                onChange={handleQuestionTypeChange}
                disabled={examStatus !== 'Upcoming'}
              >
                <option value="MCQ">Multiple Choice (MCQ)</option>
                <option value="ShortAnswer">Short Answer</option>
                <option value="LongAnswer">Long Answer</option>
              </Select>
              <Helper>Choose the type of question</Helper>
            </FormGroup>

            {/* Marks */}
            <FormGroup>
              <Label htmlFor="marks">Marks *</Label>
              <Input
                id="marks"
                type="number"
                name="marks"
                value={formData.marks || ''}
                onChange={handleInputChange}
                min="1"
                step="1"
                disabled={examStatus !== 'Upcoming'}
                style={{ appearance: 'textfield' }}
              />
              {validationErrors.marks && (
                <Helper style={{ color: '#ef4444' }}>{validationErrors.marks}</Helper>
              )}
              {exam && !validationErrors.marks && (
                <Helper style={{ color: '#059669' }}>
                  ℹ️ Exam total marks: {exam.totalMarks} | Other questions: {allQuestions
                    .filter(q => isEditMode ? parseInt(q.questionID) !== parseInt(questionId) : true)
                    .reduce((sum, q) => sum + (parseFloat(q.marks) || 0), 0).toFixed(2)} | This question: {formData.marks} | Total after save: {(allQuestions
                      .filter(q => isEditMode ? parseInt(q.questionID) !== parseInt(questionId) : true)
                      .reduce((sum, q) => sum + (parseFloat(q.marks) || 0), 0) + parseFloat(formData.marks || 0)).toFixed(2)}
                </Helper>
              )}
            </FormGroup>

            {/* Options for MCQ */}
            {formData.questionType === 'MCQ' && (
              <FormGroup>
                <Label>Options (exactly 4 required) *</Label>
                <OptionsContainer>
                  {formData.options.map((option, index) => (
                    <OptionItem key={index}>
                      <CheckboxInput
                        type="radio"
                        name="correctOption"
                        checked={option.isCorrect}
                        onChange={(e) => handleOptionChange(index, 'isCorrect', e.target.checked)}
                        title="Mark as correct answer (only one option can be correct)"
                        disabled={examStatus !== 'Upcoming'}
                      />
                      <div style={{ flex: 1 }}>
                        <OptionInput
                          type="text"
                          value={option.optionText}
                          onChange={(e) => handleOptionChange(index, 'optionText', e.target.value)}
                          placeholder={`Option ${index + 1}`}
                          disabled={examStatus !== 'Upcoming'}
                        />
                      </div>
                      {/* <OptionControls>
                        <RemoveButton
                          type="button"
                          onClick={() => handleRemoveOption(index)}
                          disabled={formData.options.length <= 4 || examStatus !== 'Upcoming'}
                          style={{
                            opacity: formData.options.length <= 4 || examStatus !== 'Upcoming' ? 0.5 : 1,
                            cursor: formData.options.length <= 4 || examStatus !== 'Upcoming' ? 'not-allowed' : 'pointer'
                          }}
                          title={formData.options.length <= 4 ? 'MCQ must have exactly 4 options' : examStatus !== 'Upcoming' ? 'Cannot modify questions for this exam' : 'Remove option'}
                        >
                          Remove
                        </RemoveButton>
                      </OptionControls> */}
                    </OptionItem>
                  ))}

                </OptionsContainer>
                {validationErrors.options && (
                  <Helper style={{ color: '#ef4444' }}>{validationErrors.options}</Helper>
                )}
              </FormGroup>
            )}

            {/* Note for Short/Long Answer - No model answer required */}
            {(formData.questionType === 'ShortAnswer' || formData.questionType === 'LongAnswer') && (
              <FormGroup>
                <Helper style={{ color: '#059669', backgroundColor: '#ecfdf5', padding: '1rem', borderRadius: '4px', marginBottom: '1rem' }}>
                  ℹ️ Students will provide their answers. You can grade them manually after the exam ends.
                </Helper>
              </FormGroup>
            )}

            {/* Buttons */}
            <ButtonGroup>
              <CancelButton type="button" onClick={handleCancel}>
                Cancel
              </CancelButton>
              <SubmitButton
                type="submit"
                disabled={saving || examStatus !== 'Upcoming'}
                title={examStatus !== 'Upcoming' ? `Cannot modify questions for ${examStatus} exams` : ''}
              >
                {saving ? 'Saving...' : isEditMode ? 'Update Question' : 'Add Question'}
              </SubmitButton>
            </ButtonGroup>
          </form>
        </FormContainer>
      </MainContent>
    </Container>
  )
}
