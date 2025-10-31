import styled from 'styled-components'

// Main Container
export const ExamContainer = styled.div`
  min-height: 100vh;
  background-color: #f9fafb;
  display: flex;
  flex-direction: column;
`

// Header with title and timer
export const ExamHeader = styled.div`
  background-color: white;
  border-bottom: 1px solid #e5e7eb;
  padding: 16px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
`

export const ExamTitle = styled.h1`
  font-size: 20px;
  font-weight: 700;
  color: #111827;
  margin: 0;
`

export const TimerSection = styled.div`
  display: flex;
  align-items: center;
  gap: 12px;
`

export const Timer = styled.div`
  background-color: ${props => (props.isLowTime ? '#fee' : '#f0f9ff')};
  border: 2px solid ${props => (props.isLowTime ? '#dc2626' : '#3b82f6')};
  border-radius: 8px;
  padding: 12px 20px;
  font-size: 16px;
  font-weight: 700;
  color: ${props => (props.isLowTime ? '#991b1b' : '#1e40af')};
  min-width: 140px;
  text-align: center;
`

export const SubmitButton = styled.button`
  background-color: #10b981;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
  transition: background-color 0.2s;

  &:hover {
    background-color: #059669;
  }

  &:disabled {
    background-color: #9ca3af;
    cursor: not-allowed;
  }
`

// Main content area
export const ExamContent = styled.div`
  display: flex;
  flex: 1;
  gap: 0;
  padding: 24px;
  max-width: none;
`

// Left panel - Questions navigator
export const LeftPanel = styled.div`
  width: 280px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  padding: 20px;
  overflow-y: auto;
  max-height: calc(100vh - 150px);

  @media (max-width: 1024px) {
    width: 200px;
  }

  @media (max-width: 768px) {
    display: none;
  }
`

export const SectionTitle = styled.h3`
  font-size: 14px;
  font-weight: 700;
  color: #1f2937;
  margin-bottom: 12px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
`

export const QuestionsGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 8px;
`

export const QuestionButton = styled.button`
  aspect-ratio: 1;
  border: 2px solid ${props => {
    if (props.isActive) return '#1e40af'
    if (props.status === 'answered') return '#10b981'
    if (props.status === 'skipped') return '#f59e0b'
    return '#e5e7eb'
  }};
  background-color: ${props => {
    if (props.isActive) return '#dbeafe'
    if (props.status === 'answered') return '#ecfdf5'
    if (props.status === 'skipped') return '#fef3c7'
    return 'white'
  }};
  color: ${props => {
    if (props.isActive) return '#1e40af'
    if (props.status === 'answered') return '#065f46'
    if (props.status === 'skipped') return '#92400e'
    return '#6b7280'
  }};
  border-radius: 6px;
  cursor: pointer;
  font-size: 12px;
  font-weight: 600;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;

  &:hover {
    border-color: #1e40af;
    background-color: #dbeafe;
  }
`

export const LegendContainer = styled.div`
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid #e5e7eb;
  display: flex;
  flex-direction: column;
  gap: 8px;
  font-size: 12px;
`

export const LegendItem = styled.div`
  display: flex;
  align-items: center;
  gap: 8px;
`

export const LegendBox = styled.div`
  width: 16px;
  height: 16px;
  border: 2px solid ${props => props.color};
  background-color: ${props => props.bgColor};
  border-radius: 4px;
`

// Center panel - Question and answer
export const CenterPanel = styled.div`
  flex: 1;
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  padding: 24px;
  margin-left: 24px;
  overflow-y: auto;
  max-height: calc(100vh - 150px);

  @media (max-width: 768px) {
    margin-left: 0;
  }
`

export const QuestionNumber = styled.p`
  color: #6b7280;
  font-size: 13px;
  margin-bottom: 8px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
`

export const QuestionText = styled.h2`
  font-size: 18px;
  font-weight: 600;
  color: #111827;
  margin-bottom: 24px;
  line-height: 1.6;
`

export const OptionsContainer = styled.div`
  display: flex;
  flex-direction: column;
  gap: 12px;
`

export const OptionLabel = styled.label`
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 14px 16px;
  border: 2px solid #e5e7eb;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
  background-color: white;

  &:hover {
    background-color: #f9fafb;
    border-color: #1e40af;
  }

  ${props =>
    props.isSelected &&
    `
    background-color: #dbeafe;
    border-color: #1e40af;
  `}
`

export const RadioInput = styled.input`
  margin-top: 3px;
  cursor: pointer;
`

export const OptionText = styled.span`
  font-size: 14px;
  color: #1f2937;
  flex: 1;
  line-height: 1.5;
`

// Text area for descriptive answers
export const AnswerTextArea = styled.textarea`
  width: 100%;
  min-height: 200px;
  padding: 12px;
  border: 2px solid #e5e7eb;
  border-radius: 6px;
  font-size: 14px;
  font-family: inherit;
  resize: vertical;
  transition: all 0.2s;

  &:focus {
    outline: none;
    border-color: #1e40af;
    box-shadow: 0 0 0 3px rgba(30, 64, 175, 0.1);
  }
`

// Bottom controls
export const ControlsContainer = styled.div`
  display: flex;
  gap: 12px;
  margin-top: 24px;
  padding-top: 24px;
  border-top: 1px solid #e5e7eb;
`

export const PreviousButton = styled.button`
  flex: 1;
  background-color: #f3f4f6;
  color: #1f2937;
  border: 1px solid #d1d5db;
  padding: 12px 24px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
  transition: all 0.2s;

  &:hover {
    background-color: #e5e7eb;
  }

  &:disabled {
    background-color: #f9fafb;
    color: #9ca3af;
    cursor: not-allowed;
    border-color: #e5e7eb;
  }
`

export const ClearButton = styled.button`
  flex: 1;
  background-color: #fef3c7;
  color: #92400e;
  border: 1px solid #fcd34d;
  padding: 12px 24px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
  transition: all 0.2s;

  &:hover {
    background-color: #fde68a;
  }
`

export const SaveNextButton = styled.button`
  flex: 1;
  background-color: #10b981;
  color: white;
  border: none;
  padding: 12px 24px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
  transition: background-color 0.2s;

  &:hover {
    background-color: #059669;
  }

  &:disabled {
    background-color: #9ca3af;
    cursor: not-allowed;
  }
`

// Loading and message states
export const LoadingContainer = styled.div`
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 400px;
  color: #6b7280;
`

export const ErrorMessage = styled.div`
  background-color: #fee;
  color: #991b1b;
  padding: 16px;
  border-radius: 6px;
  margin-bottom: 16px;
  border-left: 4px solid #991b1b;
`

export const SuccessMessage = styled.div`
  background-color: #ecfdf5;
  color: #065f46;
  padding: 16px;
  border-radius: 6px;
  margin-bottom: 16px;
  border-left: 4px solid #065f46;
`

// Modal for submission confirmation
export const SubmissionModal = styled.div`
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

export const SubmissionModalContent = styled.div`
  background: white;
  border-radius: 8px;
  padding: 32px;
  max-width: 420px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
`

export const ModalTitle = styled.h2`
  font-size: 20px;
  font-weight: 700;
  color: #111827;
  margin-bottom: 12px;
`

export const ModalText = styled.p`
  color: #6b7280;
  font-size: 14px;
  margin-bottom: 24px;
  line-height: 1.6;
`

export const ModalStats = styled.div`
  background-color: #f9fafb;
  border-radius: 6px;
  padding: 16px;
  margin-bottom: 24px;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
`

export const StatItem = styled.div`
  display: flex;
  flex-direction: column;
  align-items: center;
`

export const StatLabel = styled.p`
  font-size: 12px;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 4px;
`

export const StatValue = styled.p`
  font-size: 18px;
  font-weight: 700;
  color: #1f2937;
`

export const ModalButtons = styled.div`
  display: flex;
  gap: 12px;
`

export const CancelButton = styled.button`
  flex: 1;
  background-color: #f3f4f6;
  color: #1f2937;
  border: 1px solid #d1d5db;
  padding: 12px 24px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
  transition: all 0.2s;

  &:hover {
    background-color: #e5e7eb;
  }
`

export const ConfirmButton = styled.button`
  flex: 1;
  background-color: #dc2626;
  color: white;
  border: none;
  padding: 12px 24px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
  transition: background-color 0.2s;

  &:hover {
    background-color: #b91c1c;
  }
`
