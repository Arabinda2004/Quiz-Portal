import { useState, useEffect } from 'react'
import styled from 'styled-components'
import { teacherService } from '../services/api'

const ModalOverlay = styled.div`
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
  padding: 1rem;
`

const ModalContent = styled.div`
  background: white;
  border-radius: 8px;
  max-width: 900px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
`

const ModalHeader = styled.div`
  padding: 2rem;
  border-bottom: 1px solid #e5e7eb;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background-color: #f9fafb;
`

const ModalTitle = styled.h2`
  margin: 0;
  font-size: 1.5rem;
  color: #1f2937;
`

const CloseButton = styled.button`
  background: none;
  border: none;
  font-size: 1.5rem;
  color: #6b7280;
  cursor: pointer;
  padding: 0;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;

  &:hover {
    color: #1f2937;
  }
`

const ModalBody = styled.div`
  padding: 2rem;
`

const Section = styled.div`
  margin-bottom: 2rem;

  &:last-child {
    margin-bottom: 0;
  }
`

const SectionTitle = styled.h3`
  font-size: 1rem;
  font-weight: 600;
  color: #374151;
  margin: 0 0 1rem 0;
  padding-bottom: 0.75rem;
  border-bottom: 2px solid #e5e7eb;
`

const InfoGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
`

const InfoItem = styled.div`
  background-color: #f9fafb;
  padding: 1rem;
  border-radius: 4px;
  border-left: 3px solid #3b82f6;
`

const InfoLabel = styled.div`
  font-size: 0.875rem;
  color: #6b7280;
  font-weight: 600;
  margin-bottom: 0.25rem;
`

const InfoValue = styled.div`
  font-size: 1rem;
  color: #1f2937;
  font-weight: 500;
`

const QuestionBox = styled.div`
  background-color: #f0f9ff;
  padding: 1.5rem;
  border-radius: 4px;
  border-left: 4px solid #3b82f6;
  margin-bottom: 1.5rem;
`

const QuestionText = styled.p`
  margin: 0 0 0.75rem 0;
  font-size: 1rem;
  font-weight: 600;
  color: #1f2937;
`

const StudentAnswerBox = styled.div`
  background-color: #fef3c7;
  padding: 1.5rem;
  border-radius: 4px;
  border-left: 4px solid #f59e0b;
  margin-bottom: 1.5rem;
`

const StudentAnswerTitle = styled.p`
  margin: 0 0 0.75rem 0;
  font-size: 0.95rem;
  font-weight: 600;
  color: #92400e;
`

const StudentAnswerText = styled.p`
  margin: 0;
  color: #78350f;
  line-height: 1.6;
  word-wrap: break-word;
  white-space: pre-wrap;
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

  &:disabled {
    background-color: #f3f4f6;
    color: #9ca3af;
  }
`

const TextArea = styled.textarea`
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

  &:disabled {
    background-color: #f3f4f6;
    color: #9ca3af;
  }
`

const CheckboxLabel = styled.label`
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.95rem;
  color: #374151;
  cursor: pointer;

  input {
    cursor: pointer;
  }
`

const ButtonGroup = styled.div`
  display: flex;
  gap: 1rem;
  margin-top: 2rem;
  padding-top: 2rem;
  border-top: 1px solid #e5e7eb;
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

  &:hover:not(:disabled) {
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

const DangerButton = styled(Button)`
  background-color: #ef4444;
  color: white;

  &:hover:not(:disabled) {
    background-color: #dc2626;
  }

  &:disabled {
    background-color: #9ca3af;
    cursor: not-allowed;
  }
`

const StatusBadge = styled.span`
  display: inline-block;
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  background-color: ${(props) => {
    switch (props.$status) {
      case 'Graded':
        return '#d1fae5'
      case 'Pending':
        return '#fef3c7'
      case 'Regraded':
        return '#dbeafe'
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
      case 'Regraded':
        return '#1e40af'
      default:
        return '#374151'
    }
  }};
`

const HistoryBox = styled.div`
  background-color: #f9fafb;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 0.5rem;

  &:last-child {
    margin-bottom: 0;
  }
`

const HistoryItem = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 0.75rem;
  margin-bottom: 0.75rem;
  border-bottom: 1px solid #e5e7eb;

  &:last-child {
    border-bottom: none;
    padding-bottom: 0;
    margin-bottom: 0;
  }
`

const HistoryLabel = styled.span`
  font-weight: 600;
  color: #374151;
  font-size: 0.95rem;
`

const HistoryValue = styled.span`
  color: #6b7280;
  font-size: 0.95rem;
`

export default function GradingResponseDetail({ responseId, onClose, onGraded, isOpen }) {
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [response, setResponse] = useState(null)
  const [gradingHistory, setGradingHistory] = useState([])
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  // Form state
  const [marksObtained, setMarksObtained] = useState('')
  const [feedback, setFeedback] = useState('')
  const [comment, setComment] = useState('')
  const [isPartialCredit, setIsPartialCredit] = useState(false)
  const [isRegrading, setIsRegrading] = useState(false)
  const [regradingReason, setRegradingReason] = useState('')

  useEffect(() => {
    if (isOpen && responseId) {
      loadResponseData()
    }
  }, [responseId, isOpen])

  const loadResponseData = async () => {
    try {
      setLoading(true)
      setError('')

      // Load response details
      console.log('Loading response for ID:', responseId)
      const responseData = await teacherService.getResponseForGrading(responseId)
      console.log('Full response data received:', responseData)
      
      // Handle both response.data and response.data.data structures
      const responseInfo = responseData.data ? responseData.data : responseData
      console.log('Extracted response info:', responseInfo)
      setResponse(responseInfo)

      // Load grading history
      try {
        const historyData = await teacherService.getGradingHistory(responseId)
        console.log('Loaded history data:', historyData)
        
        // Handle history data structure
        const historyInfo = historyData.data ? historyData.data : historyData
        setGradingHistory(Array.isArray(historyInfo) ? historyInfo : [])
      } catch (histErr) {
        console.warn('Error loading history (non-blocking):', histErr)
        setGradingHistory([])
      }

      // Pre-fill form if already graded
      if (responseInfo && responseInfo.isGraded) {
        setMarksObtained(responseInfo.currentMarksObtained || responseInfo.marksObtained || '')
        setFeedback(responseInfo.feedback || '')
      }
    } catch (err) {
      console.error('Error loading response:', err)
      console.error('Error details:', {
        message: err.message,
        response: err.response,
        status: err.status,
      })
      setError(err.message || 'Failed to load response details')
    } finally {
      setLoading(false)
    }
  }

  const validateForm = () => {
    if (marksObtained === '' || marksObtained === undefined || marksObtained === null) {
      setError('Please enter marks obtained')
      return false
    }

    const marks = parseFloat(marksObtained)
    if (isNaN(marks) || marks < 0) {
      setError('Marks must be a valid number')
      return false
    }

    if (response && marks > response.maxMarks) {
      setError(`Marks cannot exceed ${response.maxMarks}`)
      return false
    }

    if (isRegrading && !regradingReason.trim()) {
      setError('Please provide a reason for regrading')
      return false
    }

    return true
  }

  const handleSubmitGrade = async () => {
    if (!validateForm()) return

    try {
      setSaving(true)
      setError('')
      setSuccess('')

      const gradingData = {
        marksObtained: parseFloat(marksObtained),
        feedback: feedback.trim(),
        comment: comment.trim(),
        isPartialCredit: isPartialCredit,
      }

      console.log('Submitting grade with data:', gradingData)
      await teacherService.gradeSingleResponse(responseId, gradingData)
      console.log('Grade submitted successfully')
      setSuccess('Response graded successfully!')
      setTimeout(() => {
        onGraded?.()
        onClose()
      }, 1500)
    } catch (err) {
      console.error('Grading error:', err)
      console.error('Error details:', {
        message: err.message,
        response: err.response?.data,
        status: err.response?.status,
      })
      // Provide more specific error message
      if (typeof err === 'string') {
        setError(err)
      } else if (err.message) {
        setError(err.message)
      } else if (err.response?.data?.message) {
        setError(err.response.data.message)
      } else {
        setError('Failed to grade response. Please check the console for details.')
      }
    } finally {
      setSaving(false)
    }
  }

  const handleRegrade = async () => {
    if (!validateForm()) return

    try {
      setSaving(true)
      setError('')
      setSuccess('')

      const regradingData = {
        reason: regradingReason.trim(),
        newMarksObtained: parseFloat(marksObtained),
        newFeedback: feedback.trim(),
        comment: comment.trim(),
      }

      await teacherService.regradeResponse(responseId, regradingData)
      setSuccess('Response regraded successfully!')
      setTimeout(() => {
        onGraded?.()
        onClose()
      }, 1500)
    } catch (err) {
      console.error('Regrading error:', err)
      setError(err.message || 'Failed to regrade response')
    } finally {
      setSaving(false)
    }
  }

  if (!isOpen) return null

  if (loading) {
    return (
      <ModalOverlay onClick={onClose}>
        <ModalContent onClick={(e) => e.stopPropagation()}>
          <ModalHeader>
            <ModalTitle>Loading Response...</ModalTitle>
            <CloseButton onClick={onClose}>✕</CloseButton>
          </ModalHeader>
          <ModalBody>
            <div style={{ textAlign: 'center', padding: '2rem', color: '#6b7280' }}>
              Loading response details...
            </div>
          </ModalBody>
        </ModalContent>
      </ModalOverlay>
    )
  }

  if (!response) {
    return (
      <ModalOverlay onClick={onClose}>
        <ModalContent onClick={(e) => e.stopPropagation()}>
          <ModalHeader>
            <ModalTitle>Error</ModalTitle>
            <CloseButton onClick={onClose}>✕</CloseButton>
          </ModalHeader>
          <ModalBody>
            <div style={{ textAlign: 'center', padding: '2rem', color: '#dc2626' }}>
              {error || 'Failed to load response'}
            </div>
          </ModalBody>
        </ModalContent>
      </ModalOverlay>
    )
  }

  return (
    <ModalOverlay onClick={onClose}>
      <ModalContent onClick={(e) => e.stopPropagation()}>
        <ModalHeader>
          <ModalTitle>Grade Response</ModalTitle>
          <CloseButton onClick={onClose}>✕</CloseButton>
        </ModalHeader>

        <ModalBody>
          {error && (
            <div
              style={{
                backgroundColor: '#fee',
                border: '1px solid #fca5a5',
                color: '#991b1b',
                padding: '1rem',
                borderRadius: '4px',
                marginBottom: '1rem',
              }}
            >
              {error}
            </div>
          )}

          {success && (
            <div
              style={{
                backgroundColor: '#ecfdf5',
                border: '1px solid #86efac',
                color: '#065f46',
                padding: '1rem',
                borderRadius: '4px',
                marginBottom: '1rem',
              }}
            >
              {success}
            </div>
          )}

          {/* Student Info */}
          <Section>
            <SectionTitle>Student Information</SectionTitle>
            <InfoGrid>
              <InfoItem>
                <InfoLabel>Student Name</InfoLabel>
                <InfoValue>{response?.studentName}</InfoValue>
              </InfoItem>
              <InfoItem>
                <InfoLabel>Student Email</InfoLabel>
                <InfoValue>{response?.studentEmail}</InfoValue>
              </InfoItem>
              <InfoItem>
                <InfoLabel>Question Type</InfoLabel>
                <InfoValue>{response?.questionType}</InfoValue>
              </InfoItem>
              <InfoItem>
                <InfoLabel>Status</InfoLabel>
                <InfoValue>
                  <StatusBadge $status={response?.isGraded ? 'Graded' : 'Pending'}>
                    {response?.isGraded ? 'Graded' : 'Pending'}
                  </StatusBadge>
                </InfoValue>
              </InfoItem>
            </InfoGrid>
          </Section>

          {/* Question and Answer */}
          <Section>
            <SectionTitle>Question & Response</SectionTitle>
            <QuestionBox>
              <QuestionText>{response?.questionText}</QuestionText>
              <div style={{ fontSize: '0.875rem', color: '#6b7280' }}>
                Max Marks: <strong>{response?.maxMarks}</strong>
              </div>
            </QuestionBox>

            <StudentAnswerBox>
              <StudentAnswerTitle>Student's Answer:</StudentAnswerTitle>
              <StudentAnswerText>{response?.studentAnswer}</StudentAnswerText>
            </StudentAnswerBox>
          </Section>

          {/* Grading History */}
          {gradingHistory && gradingHistory.length > 0 && (
            <Section>
              <SectionTitle>Grading History</SectionTitle>
              {gradingHistory.map((record, idx) => (
                <HistoryBox key={idx}>
                  <HistoryItem>
                    <HistoryLabel>Graded By:</HistoryLabel>
                    <HistoryValue>{record.gradedByTeacher}</HistoryValue>
                  </HistoryItem>
                  <HistoryItem>
                    <HistoryLabel>Marks:</HistoryLabel>
                    <HistoryValue>
                      {record.marksObtained} / {response.maxMarks}
                    </HistoryValue>
                  </HistoryItem>
                  <HistoryItem>
                    <HistoryLabel>Graded At:</HistoryLabel>
                    <HistoryValue>{new Date(record.gradedAt).toLocaleString()}</HistoryValue>
                  </HistoryItem>
                  {record.feedback && (
                    <HistoryItem>
                      <HistoryLabel>Feedback:</HistoryLabel>
                      <HistoryValue>{record.feedback}</HistoryValue>
                    </HistoryItem>
                  )}
                </HistoryBox>
              ))}
            </Section>
          )}

          {/* Grading Form */}
          <Section>
            <SectionTitle>{response.isGraded ? 'Update Grade' : 'Submit Grade'}</SectionTitle>

            <FormGroup>
              <Label htmlFor="marks">
                Marks Obtained (Max: {response.maxMarks}) *
              </Label>
              <Input
                id="marks"
                type="number"
                min="0"
                max={response.maxMarks}
                step="0.5"
                value={marksObtained}
                onChange={(e) => setMarksObtained(e.target.value)}
                placeholder="Enter marks"
                disabled={saving}
              />
            </FormGroup>

            <FormGroup>
              <Label htmlFor="feedback">Feedback for Student</Label>
              <TextArea
                id="feedback"
                value={feedback}
                onChange={(e) => setFeedback(e.target.value)}
                placeholder="Provide constructive feedback to help the student improve"
                disabled={saving}
              />
            </FormGroup>

            <FormGroup>
              <Label htmlFor="comment">Internal Comment (for reference)</Label>
              <TextArea
                id="comment"
                value={comment}
                onChange={(e) => setComment(e.target.value)}
                placeholder="Internal notes (not shown to student)"
                disabled={saving}
              />
            </FormGroup>

            <FormGroup>
              <CheckboxLabel>
                <input
                  type="checkbox"
                  checked={isPartialCredit}
                  onChange={(e) => setIsPartialCredit(e.target.checked)}
                  disabled={saving}
                />
                Mark as Partial Credit
              </CheckboxLabel>
            </FormGroup>

            {response.isGraded && (
              <FormGroup>
                <CheckboxLabel>
                  <input
                    type="checkbox"
                    checked={isRegrading}
                    onChange={(e) => setIsRegrading(e.target.checked)}
                    disabled={saving}
                  />
                  This is a regrading
                </CheckboxLabel>
              </FormGroup>
            )}

            {isRegrading && (
              <FormGroup>
                <Label htmlFor="reason">Reason for Regrading *</Label>
                <TextArea
                  id="reason"
                  value={regradingReason}
                  onChange={(e) => setRegradingReason(e.target.value)}
                  placeholder="Explain why this response is being regraded"
                  disabled={saving}
                />
              </FormGroup>
            )}

            <ButtonGroup>
              <SecondaryButton onClick={onClose} disabled={saving}>
                Cancel
              </SecondaryButton>
              {response.isGraded && !isRegrading && (
                <PrimaryButton onClick={handleSubmitGrade} disabled={saving}>
                  {saving ? 'Saving...' : 'Update Grade'}
                </PrimaryButton>
              )}
              {!response.isGraded && !isRegrading && (
                <PrimaryButton onClick={handleSubmitGrade} disabled={saving}>
                  {saving ? 'Saving...' : 'Submit Grade'}
                </PrimaryButton>
              )}
              {isRegrading && (
                <DangerButton onClick={handleRegrade} disabled={saving}>
                  {saving ? 'Regrading...' : 'Regrade Response'}
                </DangerButton>
              )}
            </ButtonGroup>
          </Section>
        </ModalBody>
      </ModalContent>
    </ModalOverlay>
  )
}
