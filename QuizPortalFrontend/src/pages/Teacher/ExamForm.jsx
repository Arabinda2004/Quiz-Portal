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
`

const TwoColumnGrid = styled.div`
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
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

const Helper = styled.p`
  color: #6b7280;
  font-size: 0.875rem;
  margin-top: 0.25rem;
`

export default function ExamForm() {
  const navigate = useNavigate()
  const { examId } = useParams()
  const { user, logout } = useAuth()
  const isEditMode = Boolean(examId)

  const [loading, setLoading] = useState(isEditMode)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const [formData, setFormData] = useState({
    title: '',
    description: '',
    batchRemark: '',
    passingPercentage: 40,
    scheduleStart: '',
    scheduleEnd: '',
    // accessCode: '',
    accessPassword: '',
    durationMinutes: 60,
    allowNegativeMarks: false,
    negativeMarkPercentage: 0,
  })

  const [validationErrors, setValidationErrors] = useState({})

  useEffect(() => {
    if (isEditMode) {
      loadExamData()
    }
  }, [examId, isEditMode])

  const loadExamData = async () => {
    try {
      setLoading(true)
      const response = await teacherService.getExamById(examId)
      const exam = response.data
      
      // Format dates for input fields
      const formatDatetime = (dateString) => {
        const date = new Date(dateString)
        return date.toISOString().slice(0, 16)
      }

      setFormData({
        title: exam.title || '',
        description: exam.description || '',
        batchRemark: exam.batchRemark || '',
        passingPercentage: exam.passingPercentage || 40,
        scheduleStart: exam.scheduleStart ? formatDatetime(exam.scheduleStart) : '',
        scheduleEnd: exam.scheduleEnd ? formatDatetime(exam.scheduleEnd) : '',
        // accessCode: exam.accessCode || '',
        accessPassword: exam.accessPassword || '',
        durationMinutes: exam.durationMinutes || 60,
        allowNegativeMarks: exam.allowNegativeMarks || false,
        negativeMarkPercentage: exam.negativeMarkPercentage || 0,
      })
    } catch (err) {
      setError('Failed to load exam data')
    } finally {
      setLoading(false)
    }
  }

  const validateForm = () => {
    const errors = {}

    if (!formData.title.trim()) {
      errors.title = 'Exam title is required'
    }

    if (!formData.scheduleStart) {
      errors.scheduleStart = 'Start date/time is required'
    }

    if (!formData.scheduleEnd) {
      errors.scheduleEnd = 'End date/time is required'
    }

    if (formData.scheduleStart && formData.scheduleEnd) {
      const startDate = new Date(formData.scheduleStart)
      const endDate = new Date(formData.scheduleEnd)
      
      if (startDate >= endDate) {
        errors.scheduleEnd = 'End date must be after start date'
      }

      // Validate duration doesn't exceed schedule window
      if (formData.durationMinutes > 0) {
        const scheduleWindowMinutes = (endDate - startDate) / (1000 * 60) // Convert milliseconds to minutes
        if (formData.durationMinutes > scheduleWindowMinutes) {
          errors.durationMinutes = `Duration (${formData.durationMinutes} min) cannot exceed time window between start and end (${Math.floor(scheduleWindowMinutes)} min)`
        }
      }
    }

    const passingPercentage = Number(formData.passingPercentage);
    if (passingPercentage < 0 || passingPercentage > 100) {
      errors.passingPercentage = 'Passing percentage must be between 0 and 100'
    }

    if (formData.durationMinutes <= 0) {
      errors.durationMinutes = 'Duration must be greater than 0 minutes'
    }

    // if (!formData.accessCode.trim()) {
    //   errors.accessCode = 'Access code is required'
    // } else if (formData.accessCode.length < 4) {
    //   errors.accessCode = 'Access code must be at least 4 characters'
    // }

    if (!formData.accessPassword.trim()) {
      errors.accessPassword = 'Access password is required'
    } else if (formData.accessPassword.length < 4) {
      errors.accessPassword = 'Access password must be at least 4 characters'
    }

    setValidationErrors(errors)
    return Object.keys(errors).length === 0
  }

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }))
    if (validationErrors[name]) {
      setValidationErrors((prev) => ({
        ...prev,
        [name]: '',
      }))
    }
  }

  const handleSubmit = async (e) => {
    e.preventDefault()

    if (!validateForm()) {
      return
    }

    try {
      setSaving(true)
      setError('')
      setSuccess('')

      // Convert datetime-local values to UTC ISO format
      const dataToSubmit = {
        ...formData,
        scheduleStart: formData.scheduleStart ? new Date(formData.scheduleStart).toISOString() : '',
        scheduleEnd: formData.scheduleEnd ? new Date(formData.scheduleEnd).toISOString() : '',
      }

      if (isEditMode) {
        await teacherService.updateExam(examId, dataToSubmit)
        setSuccess('Exam updated successfully!')
        setTimeout(() => navigate(`/teacher/exam/${examId}`), 1500)
      } else {
        const response = await teacherService.createExam(dataToSubmit)
        setSuccess('Exam created successfully!')
        setTimeout(() => navigate(`/teacher/exam/${response.data.examID}`), 1500)
      }
    } catch (err) {
      setError(err.message || 'Failed to save exam')
    } finally {
      setSaving(false)
    }
  }

  const handleCancel = () => {
    if (isEditMode) {
      navigate(`/teacher/exam/${examId}`)
    } else {
      navigate('/teacher/dashboard')
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
        <FormContainer>
          <PageTitle>{isEditMode ? 'Edit Exam' : 'Create New Exam'}</PageTitle>

          {error && <ErrorMessage>{error}</ErrorMessage>}
          {success && <SuccessMessage>{success}</SuccessMessage>}

          <form onSubmit={handleSubmit}>
            {/* Exam Title */}
            <FormGroup>
              <Label htmlFor="title">Exam Title *</Label>
              <Input
                id="title"
                type="text"
                name="title"
                value={formData.title}
                onChange={handleInputChange}
                placeholder="e.g., Final Exam - Mathematics"
                maxLength={200}
              />
              {validationErrors.title && (
                <Helper style={{ color: '#ef4444' }}>{validationErrors.title}</Helper>
              )}
            </FormGroup>

            {/* Description */}
            <FormGroup>
              <Label htmlFor="description">Description</Label>
              <TextArea
                id="description"
                name="description"
                value={formData.description}
                onChange={handleInputChange}
                placeholder="Enter exam description or instructions..."
              />
              <Helper>Optional: Provide instructions or additional information for students</Helper>
            </FormGroup>

            {/* Batch Remark */}
            <FormGroup>
              <Label htmlFor="batchRemark">Batch/Class Remark</Label>
              <Input
                id="batchRemark"
                type="text"
                name="batchRemark"
                value={formData.batchRemark}
                onChange={handleInputChange}
                placeholder="e.g., Class 10-A, Batch 2024"
                maxLength={100}
              />
              <Helper>Optional: Add a remark to identify the batch or class for this exam</Helper>
            </FormGroup>

            {/* Passing Percentage Configuration */}
            <FormGroup>
              <Label htmlFor="passingPercentage">Passing Percentage (%) *</Label>
              <Input
                id="passingPercentage"
                type="number"
                name="passingPercentage"
                value={formData.passingPercentage}
                onChange={handleInputChange}
                min="0"
                max="100"
                step="0.01"
              />
              {validationErrors.passingPercentage && (
                <Helper style={{ color: '#ef4444' }}>{validationErrors.passingPercentage}</Helper>
              )}
              <Helper>Set the minimum percentage students need to pass (0-100%). Total marks will be calculated from question marks.</Helper>
            </FormGroup>

            {/* Schedule */}
            <TwoColumnGrid>
              <FormGroup>
                <Label htmlFor="scheduleStart">Start Date/Time *</Label>
                <Input
                  id="scheduleStart"
                  type="datetime-local"
                  name="scheduleStart"
                  value={formData.scheduleStart}
                  onChange={handleInputChange}
                />
                {validationErrors.scheduleStart && (
                  <Helper style={{ color: '#ef4444' }}>{validationErrors.scheduleStart}</Helper>
                )}
              </FormGroup>

              <FormGroup>
                <Label htmlFor="scheduleEnd">End Date/Time *</Label>
                <Input
                  id="scheduleEnd"
                  type="datetime-local"
                  name="scheduleEnd"
                  value={formData.scheduleEnd}
                  onChange={handleInputChange}
                />
                {validationErrors.scheduleEnd && (
                  <Helper style={{ color: '#ef4444' }}>{validationErrors.scheduleEnd}</Helper>
                )}
              </FormGroup>
            </TwoColumnGrid>

            {/* Access Control */}
            <TwoColumnGrid>
              {/* <FormGroup>
                <Label htmlFor="accessCode">Access Code *</Label>
                <Input
                  id="accessCode"
                  type="text"
                  name="accessCode"
                  value={formData.accessCode}
                  onChange={handleInputChange}
                  placeholder="e.g., EXAM2024"
                  maxLength={50}
                />
                {validationErrors.accessCode && (
                  <Helper style={{ color: '#ef4444' }}>{validationErrors.accessCode}</Helper>
                )}
                <Helper>Students need this code to access the exam</Helper>
              </FormGroup> */}

              <FormGroup>
                <Label htmlFor="accessPassword">Access Password *</Label>
                <Input
                  id="accessPassword"
                  type="password"
                  name="accessPassword"
                  value={formData.accessPassword}
                  onChange={handleInputChange}
                  placeholder="e.g., SecurePass123"
                  maxLength={50}
                />
                {validationErrors.accessPassword && (
                  <Helper style={{ color: '#ef4444' }}>{validationErrors.accessPassword}</Helper>
                )}
                <Helper>Students need this password along with the access code</Helper>
              </FormGroup>
            </TwoColumnGrid>

            {/* Duration */}
            <FormGroup>
              <Label htmlFor="durationMinutes">Exam Duration (minutes) *</Label>
              <Input
                id="durationMinutes"
                type="number"
                name="durationMinutes"
                value={formData.durationMinutes}
                onChange={handleInputChange}
                min="1"
                max="480"
              />
              {validationErrors.durationMinutes && (
                <Helper style={{ color: '#ef4444' }}>{validationErrors.durationMinutes}</Helper>
              )}
              <Helper>Maximum time students have to complete the exam</Helper>
            </FormGroup>

            {/* Negative Marks */}
            <FormGroup>
              <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer' }}>
                <input
                  type="checkbox"
                  name="allowNegativeMarks"
                  checked={formData.allowNegativeMarks}
                  onChange={handleInputChange}
                />
                <span>Allow Negative Marks</span>
              </label>
              <Helper>Check this if you want to penalize wrong answers</Helper>
            </FormGroup>

            {formData.allowNegativeMarks && (
              <FormGroup>
                <Label htmlFor="negativeMarkPercentage">Negative Mark Percentage</Label>
                <Input
                  id="negativeMarkPercentage"
                  type="number"
                  name="negativeMarkPercentage"
                  value={formData.negativeMarkPercentage}
                  onChange={handleInputChange}
                  min="0"
                  max="100"
                />
                <Helper>Percentage of marks to deduct for each wrong answer</Helper>
              </FormGroup>
            )}

            {/* Buttons */}
            <ButtonGroup>
              <CancelButton type="button" onClick={handleCancel}>
                Cancel
              </CancelButton>
              <SubmitButton type="submit" disabled={saving}>
                {saving ? 'Saving...' : isEditMode ? 'Update Exam' : 'Create Exam'}
              </SubmitButton>
            </ButtonGroup>
          </form>
        </FormContainer>
      </MainContent>
    </Container>
  )
}
