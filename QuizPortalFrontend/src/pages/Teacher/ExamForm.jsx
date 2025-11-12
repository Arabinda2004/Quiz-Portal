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
  TwoColumnGrid,
  ButtonGroup,
  PrimaryButton,
  SecondaryButton,
  FormError,
  FormSuccess,
  COLORS,
} from '../../styles/TeacherStyles'

const StepIndicator = styled.div`
  display: flex;
  gap: 12px;
  margin-bottom: 32px;
  justify-content: space-between;
  max-width: 400px;
`

const Step = styled.div`
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  flex: 1;

  .step-number {
    width: 36px;
    height: 36px;
    border-radius: 50%;
    background-color: ${(props) => (props.$active ? COLORS.primary : props.$completed ? COLORS.success : COLORS.border)};
    color: white;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: 600;
    font-size: 14px;
  }

  .step-label {
    font-size: 12px;
    font-weight: 600;
    color: ${(props) => (props.$active || props.$completed ? COLORS.primary : COLORS.textMuted)};
    text-align: center;
  }
`

const InfoBox = styled.div`
  background-color: rgba(30, 64, 175, 0.05);
  border-left: 4px solid ${COLORS.primary};
  border-radius: 8px;
  padding: 14px 16px;
  margin-bottom: 20px;
  font-size: 13px;
  color: ${COLORS.text};
  line-height: 1.6;
`

const Helper = styled.p`
  color: ${COLORS.textSecondary};
  font-size: 13px;
  margin-top: 6px;
`

export default function ExamForm() {
  const navigate = useNavigate()
  const { examId } = useParams()
  const { user } = useAuth()
  const isEditMode = Boolean(examId)

  const [loading, setLoading] = useState(isEditMode)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [currentStep, setCurrentStep] = useState(1)

  const [formData, setFormData] = useState({
    title: '',
    description: '',
    batchRemark: '',
    passingPercentage: 40,
    scheduleStart: '',
    scheduleEnd: '',
    durationMinutes: 60,
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
        // accessPassword: exam.accessPassword || '',
        durationMinutes: exam.durationMinutes || 60,
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

    // if (!formData.accessPassword.trim()) {
    //   errors.accessPassword = 'Access password is required'
    // } else if (formData.accessPassword.length < 4) {
    //   errors.accessPassword = 'Access password must be at least 4 characters'
    // }

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
      navigate('/login')
    } catch (err) {
      console.error('Logout failed:', err)
    }
  }

  if (loading) {
    return (
      <TeacherLayout pageTitle={isEditMode ? 'Edit Exam' : 'Create Exam'}>
        <FormContainer>
          <div style={{ textAlign: 'center', padding: '40px' }}>
            <div style={{ fontSize: '20px' }}>Loading...</div>
          </div>
        </FormContainer>
      </TeacherLayout>
    )
  }

  return (
    <TeacherLayout pageTitle={isEditMode ? 'Edit Exam' : 'Create New Exam'}>
      <FormContainer maxWidth="800px">
        {/* Step Indicator */}
        {!isEditMode && (
          <StepIndicator>
            <Step $active={currentStep === 1} $completed={currentStep > 1}>
              <div className="step-number">1</div>
              <div className="step-label">Basic Info</div>
            </Step>
            <Step $active={currentStep === 2} $completed={currentStep > 2}>
              <div className="step-number">2</div>
              <div className="step-label">Schedule</div>
            </Step>
            <Step $active={currentStep === 3} $completed={currentStep > 3}>
              <div className="step-number">3</div>
              <div className="step-label">Settings</div>
            </Step>
          </StepIndicator>
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

        <form onSubmit={handleSubmit}>
          {/* Step 1: Basic Information */}
          {(isEditMode || currentStep === 1) && (
            <FormSection>
              <FormSectionTitle>
                Basic Exam Information
              </FormSectionTitle>

              <InfoBox>
                Provide the title, description, and batch information for your exam.
              </InfoBox>

              <FormGroup>
                <Label htmlFor="title">
                  Exam Title <span style={{ color: COLORS.danger }}>*</span>
                </Label>
                <Input
                  id="title"
                  type="text"
                  name="title"
                  value={formData.title}
                  onChange={handleInputChange}
                  placeholder="e.g., Final Exam - Mathematics 2024"
                  maxLength={200}
                  $error={!!validationErrors.title}
                />
                {validationErrors.title && <FormError>{validationErrors.title}</FormError>}
              </FormGroup>

              <FormGroup>
                <Label htmlFor="description">Description</Label>
                <TextArea
                  id="description"
                  name="description"
                  value={formData.description}
                  onChange={handleInputChange}
                  placeholder="Enter exam description, instructions, or special notes for students..."
                />
                <Helper>Optional: Provide any specific instructions or details students should know</Helper>
              </FormGroup>

              <FormGroup>
                <Label htmlFor="batchRemark">Batch/Class Remark</Label>
                <Input
                  id="batchRemark"
                  type="text"
                  name="batchRemark"
                  value={formData.batchRemark}
                  onChange={handleInputChange}
                  placeholder="e.g., Class 10-A, Batch 2024, Section B"
                  maxLength={100}
                />
                <Helper>Optional: Add a remark to identify the batch or class</Helper>
              </FormGroup>

              {!isEditMode && currentStep === 1 && (
                <ButtonGroup>
                  <SecondaryButton type="button" onClick={handleCancel}>
                    Cancel
                  </SecondaryButton>
                  <PrimaryButton type="button" onClick={() => setCurrentStep(2)}>
                    Next →
                  </PrimaryButton>
                </ButtonGroup>
              )}
            </FormSection>
          )}

          {/* Step 2: Schedule */}
          {(isEditMode || currentStep === 2) && (
            <FormSection>
              <FormSectionTitle>
                Exam Schedule
              </FormSectionTitle>

              <InfoBox>
                Set when your exam will be available to students. Students can only attempt during this time window.
              </InfoBox>

              <TwoColumnGrid>
                <FormGroup>
                  <Label htmlFor="scheduleStart">
                    Start Date &amp; Time <span style={{ color: COLORS.danger }}>*</span>
                  </Label>
                  <Input
                    id="scheduleStart"
                    type="datetime-local"
                    name="scheduleStart"
                    value={formData.scheduleStart ? formData.scheduleStart.slice(0, 16) : ''}
                    onChange={handleInputChange}
                    $error={!!validationErrors.scheduleStart}
                  />
                  {validationErrors.scheduleStart && (
                    <FormError>{validationErrors.scheduleStart}</FormError>
                  )}
                </FormGroup>

                <FormGroup>
                  <Label htmlFor="scheduleEnd">
                    End Date &amp; Time <span style={{ color: COLORS.danger }}>*</span>
                  </Label>
                  <Input
                    id="scheduleEnd"
                    type="datetime-local"
                    name="scheduleEnd"
                    value={formData.scheduleEnd}
                    onChange={handleInputChange}
                    $error={!!validationErrors.scheduleEnd}
                  />
                  {validationErrors.scheduleEnd && (
                    <FormError>{validationErrors.scheduleEnd}</FormError>
                  )}
                </FormGroup>
              </TwoColumnGrid>

              <FormGroup>
                <Label htmlFor="durationMinutes">
                  Exam Duration (minutes) <span style={{ color: COLORS.danger }}>*</span>
                </Label>
                <Input
                  id="durationMinutes"
                  type="number"
                  name="durationMinutes"
                  value={formData.durationMinutes}
                  onChange={handleInputChange}
                  min="1"
                  max="480"
                  $error={!!validationErrors.durationMinutes}
                />
                {validationErrors.durationMinutes && (
                  <FormError>{validationErrors.durationMinutes}</FormError>
                )}
                <Helper>Time allowed for each student to complete the exam (1-480 minutes)</Helper>
              </FormGroup>

              {!isEditMode && currentStep === 2 && (
                <ButtonGroup>
                  <SecondaryButton type="button" onClick={() => setCurrentStep(1)}>
                    ← Back
                  </SecondaryButton>
                  <PrimaryButton type="button" onClick={() => setCurrentStep(3)}>
                    Next →
                  </PrimaryButton>
                </ButtonGroup>
              )}
            </FormSection>
          )}

          {/* Step 3: Grading Settings */}
          {(isEditMode || currentStep === 3) && (
            <FormSection>
              <FormSectionTitle>
                Grading Settings
              </FormSectionTitle>

              <InfoBox>
                Configure how students will be graded. The passing percentage determines how many marks are needed to pass.
              </InfoBox>

              <FormGroup>
                <Label htmlFor="passingPercentage">
                  Passing Percentage (%) <span style={{ color: COLORS.danger }}>*</span>
                </Label>
                <Input
                  id="passingPercentage"
                  type="number"
                  name="passingPercentage"
                  value={formData.passingPercentage}
                  onChange={handleInputChange}
                  min="0"
                  max="100"
                  step="0.01"
                  $error={!!validationErrors.passingPercentage}
                />
                {validationErrors.passingPercentage && (
                  <FormError>{validationErrors.passingPercentage}</FormError>
                )}
                <Helper>
                  Set the minimum percentage students need to achieve (0-100%). This is calculated from total marks of all questions.
                </Helper>
              </FormGroup>

              <ButtonGroup>
                {!isEditMode && (
                  <SecondaryButton type="button" onClick={() => setCurrentStep(2)}>
                    ← Back
                  </SecondaryButton>
                )}
                <SecondaryButton type="button" onClick={handleCancel}>
                  Cancel
                </SecondaryButton>
                <PrimaryButton type="submit" disabled={saving}>
                  {saving ? 'Saving...' : isEditMode ? 'Update Exam' : 'Create Exam'}
                </PrimaryButton>
              </ButtonGroup>
            </FormSection>
          )}
        </form>
      </FormContainer>
    </TeacherLayout>
  )
}
