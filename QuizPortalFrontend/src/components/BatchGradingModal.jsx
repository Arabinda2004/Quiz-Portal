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
  max-width: 1000px;
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
`

const SectionTitle = styled.h3`
  font-size: 1rem;
  font-weight: 600;
  color: #374151;
  margin: 0 0 1rem 0;
  padding-bottom: 0.75rem;
  border-bottom: 2px solid #e5e7eb;
`

const Table = styled.table`
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 1rem;

  thead {
    background-color: #f3f4f6;
    border-bottom: 1px solid #e5e7eb;
  }

  th {
    padding: 1rem;
    text-align: left;
    font-weight: 600;
    color: #374151;
    font-size: 0.95rem;
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

const Input = styled.input`
  padding: 0.5rem;
  border: 1px solid #d1d5db;
  border-radius: 4px;
  font-size: 0.9rem;
  width: 100%;

  &:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 2px rgba(59, 130, 246, 0.1);
  }
`

const TextArea = styled.textarea`
  padding: 0.5rem;
  border: 1px solid #d1d5db;
  border-radius: 4px;
  font-size: 0.9rem;
  width: 100%;
  min-height: 60px;
  resize: vertical;
  font-family: inherit;

  &:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 2px rgba(59, 130, 246, 0.1);
  }
`

const InfoBox = styled.div`
  padding: 1rem;
  background-color: #eff6ff;
  border-left: 4px solid #3b82f6;
  border-radius: 4px;
  margin-bottom: 1.5rem;
  color: #1e40af;
  font-size: 0.95rem;
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

const ErrorMessage = styled.div`
  background-color: #fee;
  border: 1px solid #fca5a5;
  color: #991b1b;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1rem;
`

const SuccessMessage = styled.div`
  background-color: #ecfdf5;
  border: 1px solid #86efac;
  color: #065f46;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1rem;
`

export default function BatchGradingModal({ isOpen, onClose, responses, examId, questionId, onGraded }) {
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [grades, setGrades] = useState({})

  useEffect(() => {
    if (isOpen && responses) {
      // Initialize grades with empty values
      const initialGrades = {}
      responses.forEach((response) => {
        initialGrades[response.responseId] = {
          marksObtained: '',
          feedback: '',
          comment: '',
        }
      })
      setGrades(initialGrades)
    }
  }, [isOpen, responses])

  const handleGradeChange = (responseId, field, value) => {
    setGrades((prev) => ({
      ...prev,
      [responseId]: {
        ...prev[responseId],
        [field]: value,
      },
    }))
  }

  const validateGrades = () => {
    const gradedCount = Object.values(grades).filter((g) => g.marksObtained !== '').length
    if (gradedCount === 0) {
      setError('Please enter marks for at least one response')
      return false
    }

    // Validate individual marks
    for (const [responseId, gradeData] of Object.entries(grades)) {
      if (gradeData.marksObtained !== '') {
        const marks = parseFloat(gradeData.marksObtained)
        if (isNaN(marks) || marks < 0) {
          setError(`Invalid marks for response ${responseId}`)
          return false
        }

        const response = responses.find((r) => r.responseId === responseId)
        if (response && marks > response.maxMarks) {
          setError(`Marks for response ${responseId} cannot exceed ${response.maxMarks}`)
          return false
        }
      }
    }

    return true
  }

  const handleSubmitBatchGrades = async () => {
    if (!validateGrades()) return

    try {
      setSaving(true)
      setError('')
      setSuccess('')

      // Prepare batch grading data
      const responsesToGrade = responses
        .filter((response) => grades[response.responseId]?.marksObtained !== '')
        .map((response) => ({
          responseId: response.responseId,
          marksObtained: parseFloat(grades[response.responseId].marksObtained),
          feedback: grades[response.responseId].feedback || '',
          comment: grades[response.responseId].comment || '',
        }))

      const batchData = {
        examId: examId,
        questionId: questionId,
        responses: responsesToGrade,
      }

      await teacherService.gradeBatchResponses(batchData)
      setSuccess(`Successfully graded ${responsesToGrade.length} responses!`)
      setTimeout(() => {
        onGraded?.()
        onClose()
      }, 1500)
    } catch (err) {
      console.error('Batch grading error:', err)
      setError(err.message || 'Failed to grade responses')
    } finally {
      setSaving(false)
    }
  }

  if (!isOpen) return null

  return (
    <ModalOverlay onClick={onClose}>
      <ModalContent onClick={(e) => e.stopPropagation()}>
        <ModalHeader>
          <ModalTitle>Batch Grade Responses</ModalTitle>
          <CloseButton onClick={onClose}>✕</CloseButton>
        </ModalHeader>

        <ModalBody>
          {error && <ErrorMessage>{error}</ErrorMessage>}
          {success && <SuccessMessage>{success}</SuccessMessage>}

          <InfoBox>
                    ℹ️ Enter marks for the responses you want to grade. Leave blank to skip. You can grade responses for multiple students at once.
          </InfoBox>

          {responses && responses.length > 0 && (
            <Section>
              <SectionTitle>Grade {responses.length} Response(s)</SectionTitle>
              <div style={{ overflowX: 'auto' }}>
                <Table>
                  <thead>
                    <tr>
                      <th>Student</th>
                      <th>Question Preview</th>
                      <th>Marks (Max: {responses[0]?.maxMarks})</th>
                      <th>Feedback</th>
                    </tr>
                  </thead>
                  <tbody>
                    {responses.map((response) => (
                      <tr key={response.responseId}>
                        <td>
                          <strong>{response.studentName}</strong>
                          <div style={{ fontSize: '0.85rem', color: '#6b7280' }}>
                            {response.studentEmail}
                          </div>
                        </td>
                        <td>
                          <div style={{ fontSize: '0.9rem', maxWidth: '300px' }}>
                            {response.questionText?.substring(0, 60)}...
                          </div>
                        </td>
                        <td>
                          <Input
                            type="number"
                            min="0"
                            max={response.maxMarks}
                            step="0.5"
                            placeholder="Marks"
                            value={grades[response.responseId]?.marksObtained || ''}
                            onChange={(e) =>
                              handleGradeChange(response.responseId, 'marksObtained', e.target.value)
                            }
                            disabled={saving}
                          />
                        </td>
                        <td>
                          <TextArea
                            placeholder="Feedback"
                            value={grades[response.responseId]?.feedback || ''}
                            onChange={(e) =>
                              handleGradeChange(response.responseId, 'feedback', e.target.value)
                            }
                            disabled={saving}
                            style={{ minHeight: '40px' }}
                          />
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </div>
            </Section>
          )}

          <ButtonGroup>
            <SecondaryButton onClick={onClose} disabled={saving}>
              Cancel
            </SecondaryButton>
            <PrimaryButton onClick={handleSubmitBatchGrades} disabled={saving}>
              {saving ? 'Saving...' : 'Save All Grades'}
            </PrimaryButton>
          </ButtonGroup>
        </ModalBody>
      </ModalContent>
    </ModalOverlay>
  )
}
