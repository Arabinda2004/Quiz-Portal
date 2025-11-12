import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { teacherService } from '../../services/api'
import TeacherLayout from '../../components/TeacherLayout'
import styled from 'styled-components'
import {
  COLORS,
} from '../../styles/TeacherStyles'

const SectionTitle = styled.h2`
  font-size: 22px;
  font-weight: 600;
  color: ${COLORS.text};
  margin: 0 0 20px 0;
  padding-bottom: 12px;
  border-bottom: 2px solid ${COLORS.border};
`

const ExamCardGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
  margin-top: 20px;
`

const ExamCard = styled.div`
  background: white;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
  border-left: 4px solid ${(props) => {
    switch (props.$status) {
      case 'Ended':
        return '#10b981'
      case 'Active':
        return '#f59e0b'
      case 'Upcoming':
        return '#3b82f6'
      default:
        return '#d1d5db'
    }
  }};
  transition: all 0.3s ease;
  cursor: pointer;

  &:hover {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.12);
    transform: translateY(-4px);
  }

  .header {
    display: flex;
    justify-content: space-between;
    align-items: start;
    gap: 12px;
    margin-bottom: 16px;

    .title {
      margin: 0;
      font-size: 16px;
      font-weight: 600;
      color: ${COLORS.text};
      flex: 1;
    }

    .status-badge {
      padding: 4px 12px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: 600;
      white-space: nowrap;
      background: ${(props) => {
        switch (props.$status) {
          case 'Ended':
            return '#d1fae5'
          case 'Active':
            return '#fef3c7'
          case 'Upcoming':
            return '#dbeafe'
          default:
            return '#f3f4f6'
        }
      }};
      color: ${(props) => {
        switch (props.$status) {
          case 'Ended':
            return '#065f46'
          case 'Active':
            return '#92400e'
          case 'Upcoming':
            return '#1e40af'
          default:
            return '#4b5563'
        }
      }};
    }
  }

  .info-row {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;
    margin-bottom: 12px;

    .info-item {
      font-size: 13px;
      color: ${COLORS.textSecondary};

      strong {
        color: ${COLORS.text};
        font-weight: 600;
      }
    }
  }

  .schedule-info {
    font-size: 12px;
    color: ${COLORS.textSecondary};
    background: #f9fafb;
    padding: 8px 12px;
    border-radius: 6px;
    margin-bottom: 16px;
  }

  .actions {
    display: flex;
    gap: 8px;
    flex-wrap: wrap;
  }
`

const ActionButton = styled.button`
  flex: 1;
  min-width: 120px;
  padding: 10px 16px;
  border: none;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;

  &:hover {
    transform: translateY(-2px);
  }

  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
`

const PrimaryButton = styled(ActionButton)`
  background: ${COLORS.primary};
  color: white;

  &:hover:not(:disabled) {
    background: #2563eb;
    box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
  }
`

const SecondaryButton = styled(ActionButton)`
  background: ${COLORS.border};
  color: ${COLORS.text};

  &:hover:not(:disabled) {
    background: #e5e7eb;
  }
`

const DangerButton = styled(ActionButton)`
  background: #fee2e2;
  color: #991b1b;

  &:hover:not(:disabled) {
    background: #fecaca;
  }
`

const NoExamsMessage = styled.div`
  text-align: center;
  padding: 60px 20px;
  background: white;
  border-radius: 12px;

  .icon {
    font-size: 48px;
    margin-bottom: 16px;
  }

  h3 {
    font-size: 18px;
    font-weight: 600;
    color: ${COLORS.text};
    margin-bottom: 8px;
  }

  p {
    color: ${COLORS.textSecondary};
    margin-bottom: 20px;
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

const FilterContainer = styled.div`
  display: flex;
  gap: 12px;
  margin-bottom: 24px;
  flex-wrap: wrap;
  align-items: center;

  select {
    padding: 8px 12px;
    border: 1px solid ${COLORS.border};
    border-radius: 6px;
    font-size: 14px;
    background: white;
    cursor: pointer;
    color: ${COLORS.text};

    &:focus {
      outline: none;
      border-color: ${COLORS.primary};
      box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
    }
  }
`

export default function Results() {
  const navigate = useNavigate()
  const [exams, setExams] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [statusFilter, setStatusFilter] = useState('ended')

  useEffect(() => {
    loadExams()
  }, [])

  const loadExams = async () => {
    try {
      setLoading(true)
      const examsData = await teacherService.getExams()
      const examsList = examsData.data || []
      setExams(examsList)
      setError('')
    } catch (err) {
      setError(err.message || 'Failed to load exams')
    } finally {
      setLoading(false)
    }
  }

  const getExamStatus = (exam) => {
    const now = new Date()
    const start = new Date(exam.scheduleStart)
    const end = new Date(exam.scheduleEnd)

    if (now < start) return 'Upcoming'
    if (now > end) return 'Ended'
    return 'Active'
  }

  const filteredExams = exams.filter((exam) => {
    if (statusFilter === 'all') return true
    return getExamStatus(exam) === statusFilter
  })

  const handleViewResults = (examId) => {
    navigate(`/teacher/exam/${examId}/results`)
  }

  if (loading) {
    return (
      <TeacherLayout pageTitle="Results">
        <LoadingSpinner>
          <div className="spinner" />
        </LoadingSpinner>
      </TeacherLayout>
    )
  }

  return (
    <TeacherLayout pageTitle="Results & Publishing">
      <SectionTitle>Results & Publishing</SectionTitle>

      <FilterContainer>
        <span style={{ fontWeight: 500, color: COLORS.text }}>Filter by Status:</span>
        <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
          <option value="all">All Exams</option>
          <option value="Ended">Ended Exams (Ready for Results)</option>
          <option value="Active">Active Exams (In Progress)</option>
          <option value="Upcoming">Upcoming Exams</option>
        </select>
      </FilterContainer>

      {error && (
        <div
          style={{
            background: '#fee',
            border: '1px solid #fcc',
            color: '#c33',
            padding: '12px 16px',
            borderRadius: '6px',
            marginBottom: '20px',
          }}
        >
          {error}
        </div>
      )}

      {filteredExams.length === 0 ? (
        <NoExamsMessage>
          <div className="icon">📊</div>
          <h3>No Exams Found</h3>
          <p>
            {statusFilter === 'all'
              ? 'You have no exams yet.'
              : `You have no ${statusFilter.toLowerCase()} exams.`}
          </p>
        </NoExamsMessage>
      ) : (
        <ExamCardGrid>
          {filteredExams.map((exam) => {
            const status = getExamStatus(exam)
            const canPublishResults = status === 'Ended'

            return (
              <ExamCard key={exam.examID} $status={status}>
                <div className="header">
                  <h3 className="title">{exam.title}</h3>
                  <span className="status-badge">{status}</span>
                </div>

                <div className="info-row">
                  <div className="info-item">
                    <strong>Questions:</strong> {exam.totalQuestions || 0}
                  </div>
                  <div className="info-item">
                    <strong>Total Marks:</strong> {exam.totalMarks}
                  </div>
                </div>

                <div className="info-row">
                  <div className="info-item">
                    <strong>Pass %:</strong> {exam.passingPercentage?.toFixed(1)}%
                  </div>
                  <div className="info-item">
                    <strong>Pass Marks:</strong> {exam.passingMarks?.toFixed(1)}
                  </div>
                </div>

                <div className="schedule-info">
                  📅 {new Date(exam.scheduleStart).toLocaleDateString()} -{' '}
                  {new Date(exam.scheduleEnd).toLocaleDateString()}
                </div>

                <div className="actions">
                  <PrimaryButton
                    onClick={() => handleViewResults(exam.examID)}
                    disabled={!canPublishResults}
                    title={
                      !canPublishResults
                        ? 'Results can only be published after exam ends'
                        : 'View and publish results'
                    }
                  >
                    📊 View Results
                  </PrimaryButton>
                  <SecondaryButton
                    onClick={() => navigate(`/teacher/exam/${exam.examID}`)}
                    title="View exam details and questions"
                  >
                    📋 View Details
                  </SecondaryButton>
                </div>
              </ExamCard>
            )
          })}
        </ExamCardGrid>
      )}
    </TeacherLayout>
  )
}
