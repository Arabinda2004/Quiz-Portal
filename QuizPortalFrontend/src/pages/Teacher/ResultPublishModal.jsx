import { useState, useEffect } from 'react'
import styled from 'styled-components'
import { resultService, teacherService } from '../../services/api'

const Overlay = styled.div`
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
`

const Modal = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
  width: 90%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
  z-index: 1001;
`

const ModalHeader = styled.div`
  padding: 2rem;
  border-bottom: 1px solid #e5e7eb;
  display: flex;
  justify-content: space-between;
  align-items: center;
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
  cursor: pointer;
  color: #6b7280;

  &:hover {
    color: #1f2937;
  }
`

const ModalBody = styled.div`
  padding: 2rem;
`

const SectionTitle = styled.h3`
  margin: 0 0 1rem 0;
  font-size: 1rem;
  font-weight: 600;
  color: #374151;
`

const ProgressSection = styled.div`
  background-color: #f9fafb;
  padding: 1.5rem;
  border-radius: 6px;
  margin-bottom: 2rem;
`

const ProgressRow = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;

  &:last-child {
    margin-bottom: 0;
  }
`

const ProgressLabel = styled.span`
  color: #6b7280;
  font-size: 0.9rem;
`

const ProgressValue = styled.span`
  color: #1f2937;
  font-weight: 600;
  font-size: 0.95rem;
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

const FormSection = styled.div`
  margin-bottom: 2rem;
`

const FormGroup = styled.div`
  margin-bottom: 1.5rem;

  &:last-child {
    margin-bottom: 0;
  }
`

const Label = styled.label`
  display: block;
  margin-bottom: 0.5rem;
  font-size: 0.95rem;
  font-weight: 500;
  color: #374151;
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
`

const InfoBox = styled.div`
  background-color: #eff6ff;
  border-left: 4px solid #3b82f6;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
`

const InfoText = styled.p`
  margin: 0;
  color: #1e40af;
  font-size: 0.95rem;
  line-height: 1.4;
  font-weight: 500;
`

const HelpText = styled.p`
  margin: 0.5rem 0 0 0;
  font-size: 0.85rem;
  color: #6b7280;
`

const WarningBox = styled.div`
  background-color: #fef3c7;
  border-left: 4px solid #f59e0b;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
`

const WarningTitle = styled.h4`
  margin: 0 0 0.5rem 0;
  color: #92400e;
  font-size: 0.95rem;
  font-weight: 600;
`

const WarningText = styled.p`
  margin: 0;
  color: #78350f;
  font-size: 0.9rem;
  line-height: 1.4;
`

const ModalFooter = styled.div`
  padding: 1.5rem 2rem;
  border-top: 1px solid #e5e7eb;
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
`

const Button = styled.button`
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  font-size: 0.95rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;

  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
`

const CancelButton = styled(Button)`
  background-color: #f3f4f6;
  color: #374151;

  &:hover:not(:disabled) {
    background-color: #e5e7eb;
  }
`

const PublishButton = styled(Button)`
  background-color: #3b82f6;
  color: white;

  &:hover:not(:disabled) {
    background-color: #2563eb;
  }
`

const ErrorMessage = styled.div`
  background-color: #fee;
  border: 1px solid #dc2626;
  color: #991b1b;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
  font-size: 0.95rem;
`

const SuccessMessage = styled.div`
  background-color: #dcfce7;
  border: 1px solid #16a34a;
  color: #15803d;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
  font-size: 0.95rem;
`

const LoadingSpinner = styled.div`
  display: inline-block;
  width: 16px;
  height: 16px;
  border: 2px solid #f3f3f3;
  border-top: 2px solid #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;

  @keyframes spin {
    0% {
      transform: rotate(0deg);
    }
    100% {
      transform: rotate(360deg);
    }
  }
`

export default function ResultPublishModal({ examId, examTitle, onClose, onPublishSuccess }) {
  const [gradingProgress, setGradingProgress] = useState(null)
  const [publicationStatus, setPublicationStatus] = useState(null)
  const [examData, setExamData] = useState(null)
  const [publicationNotes, setPublicationNotes] = useState('')
  const [loading, setLoading] = useState(true)
  const [publishing, setPublishing] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState(false)

  useEffect(() => {
    loadData()
  }, [examId])

  const loadData = async () => {
    try {
      setLoading(true)
      setError('')
      const [progressData, statusData, examResponse] = await Promise.all([
        resultService.getGradingProgress(examId),
        resultService.getPublicationStatus(examId),
        teacherService.getExamById(examId),
      ])

      setGradingProgress(progressData)
      setPublicationStatus(statusData)
      setExamData(examResponse.data)
    } catch (err) {
      setError(err.message || 'Failed to load data')
    } finally {
      setLoading(false)
    }
  }

  // Get passing percentage from exam data
  const getPassingPercentage = () => {
    if (!examData || !examData.passingPercentage) {
      return 0
    }
    return examData.passingPercentage.toFixed(2)
  }

  const handlePublish = async () => {
    try {
      setPublishing(true)
      setError('')

      // Validate grading progress
      if (gradingProgress && gradingProgress.gradingProgress < 100) {
        setError(
          'All responses must be graded before publishing results. Current progress: ' +
            Math.round(gradingProgress.gradingProgress) +
            '%'
        )
        setPublishing(false)
        return
      }

      // Use the passing percentage from exam data
      const passingPercentage = examData?.passingPercentage || 40

      const response = await resultService.publishExam(examId, {
        passingPercentage: parseFloat(passingPercentage),
        publicationNotes,
      })

      if (response.success) {
        setSuccess(true)
        setTimeout(() => {
          if (onPublishSuccess) {
            onPublishSuccess()
          }
          onClose()
        }, 1500)
      } else {
        setError(response.message || 'Failed to publish results')
      }
    } catch (err) {
      setError(err.message || 'An error occurred while publishing results')
    } finally {
      setPublishing(false)
    }
  }

  if (loading) {
    return (
      <Overlay onClick={onClose}>
        <Modal onClick={(e) => e.stopPropagation()}>
          <ModalHeader>
            <ModalTitle>Publish Results</ModalTitle>
            <CloseButton onClick={onClose}>×</CloseButton>
          </ModalHeader>
          <ModalBody style={{ textAlign: 'center', padding: '3rem 2rem' }}>
            <LoadingSpinner /> Loading...
          </ModalBody>
        </Modal>
      </Overlay>
    )
  }

  return (
    <Overlay onClick={onClose}>
      <Modal onClick={(e) => e.stopPropagation()}>
        <ModalHeader>
          <ModalTitle>Publish Results</ModalTitle>
          <CloseButton onClick={onClose}>×</CloseButton>
        </ModalHeader>
        <ModalBody>
          {error && <ErrorMessage>{error}</ErrorMessage>}
          {success && (
            <SuccessMessage>✓ Results published successfully! Students can now view their results.</SuccessMessage>
          )}

          {publicationStatus?.isPublished && (
            <WarningBox>
              <WarningTitle>Results Already Published</WarningTitle>
              <WarningText>
                These results were published on {publicationStatus.publishedAt}. Publishing again will update all
                student results.
              </WarningText>
            </WarningBox>
          )}

          {/* Grading Progress Section */}
          <ProgressSection>
            <SectionTitle>Grading Progress</SectionTitle>
            <ProgressRow>
              <ProgressLabel>Total Students</ProgressLabel>
              <ProgressValue>{gradingProgress?.totalResponses || 0}</ProgressValue>
            </ProgressRow>
            <ProgressRow>
              <ProgressLabel>Graded Responses</ProgressLabel>
              <ProgressValue>{gradingProgress?.totalResponses - gradingProgress?.totalPending || 0}</ProgressValue>
            </ProgressRow>
            <ProgressRow>
              <ProgressLabel>Pending Responses</ProgressLabel>
              <ProgressValue>{gradingProgress?.totalPending || 0}</ProgressValue>
            </ProgressRow>
            <ProgressRow>
              <ProgressLabel>Progress</ProgressLabel>
            </ProgressRow>
            <ProgressBar>
              <ProgressFill percentage={gradingProgress?.gradingProgress || 0} />
            </ProgressBar>
            <ProgressValue style={{ display: 'block', marginTop: '0.5rem' }}>
              {Math.round((gradingProgress?.totalResponses - gradingProgress?.totalPending) / gradingProgress?.totalResponses * 100) || 0}% Complete
            </ProgressValue>
          </ProgressSection>

          {/* Exam Information */}
          <InfoBox>
            <InfoText>
              📊 Passing Percentage: <strong>{getPassingPercentage()}%</strong> (Total Marks: {examData?.totalMarks || 0}, Passing Marks: {examData?.passingMarks?.toFixed(2) || 0})
            </InfoText>
          </InfoBox>

          {/* Publication Settings */}
          <FormSection>
            <SectionTitle>Publication Settings</SectionTitle>

            <FormGroup>
              <Label htmlFor="publicationNotes">Publication Notes (Optional)</Label>
              <TextArea
                id="publicationNotes"
                value={publicationNotes}
                onChange={(e) => setPublicationNotes(e.target.value)}
                disabled={publishing}
                placeholder="Add any notes or announcements for students regarding these results..."
              />
              <HelpText>These notes will be visible to students when viewing their results</HelpText>
            </FormGroup>
          </FormSection>

          {/* Important Notice */}
          {gradingProgress?.pendingStudents === 0 && (
            <WarningBox>
              <WarningTitle>Ready to Publish</WarningTitle>
              <WarningText>
                All responses have been graded. Once published, students will be able to view their results
                immediately.
              </WarningText>
            </WarningBox>
          )}
        </ModalBody>
        <ModalFooter>
          <CancelButton onClick={onClose} disabled={publishing}>
            Cancel
          </CancelButton>
          <PublishButton
            onClick={handlePublish}
            disabled={publishing || (gradingProgress?.pendingStudents || 0) > 0}
          >
            {publishing ? <><LoadingSpinner /> Publishing...</> : 'Publish Results'}
          </PublishButton>
        </ModalFooter>
      </Modal>
    </Overlay>
  )
}
