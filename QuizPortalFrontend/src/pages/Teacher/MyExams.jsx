import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { teacherService } from '../../services/api'
import TeacherLayout from '../../components/TeacherLayout'
import styled from 'styled-components'
import {
  ExamCard,
  Alert,
  TableContainer,
  Table,
  StatusBadge,
  PrimaryButton,
  SecondaryButton,
  DangerButton,
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

const ExamsList = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 20px;
  margin-top: 20px;
`

const ExamTableContainer = styled.div`
  margin-top: 20px;
`

const ActionButtons = styled.div`
  display: flex;
  gap: 8px;
  flex-wrap: wrap;

  button {
    padding: 8px 12px;
    font-size: 12px;
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

const FilterButton = styled.button`
  padding: 8px 16px;
  background: ${(props) => (props.$active ? COLORS.primary : COLORS.border)};
  color: ${(props) => (props.$active ? 'white' : COLORS.text)};
  border: 1px solid ${COLORS.border};
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.2s ease;

  &:hover {
    background: ${COLORS.primary};
    color: white;
  }
`

export default function MyExams() {
  const navigate = useNavigate()
  const { user } = useAuth()
  const [exams, setExams] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [statusFilter, setStatusFilter] = useState('all')

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

  const handleCreateExam = () => {
    navigate('/teacher/create-exam')
  }

  const handleEditExam = (examId) => {
    navigate(`/teacher/edit-exam/${examId}`)
  }

  const handleViewExam = (examId) => {
    navigate(`/teacher/exam/${examId}`)
  }

  const handleGradeExam = (examId) => {
    navigate(`/teacher/exam/${examId}/grade-by-student`)
  }

  const handleDeleteExam = async (examId) => {
    if (!window.confirm('Are you sure you want to delete this exam? This action cannot be undone.')) {
      return
    }

    try {
      await teacherService.deleteExam(examId)
      loadExams()
    } catch (err) {
      setError('Failed to delete exam')
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

  if (loading) {
    return (
      <TeacherLayout pageTitle="My Exams">
        <LoadingSpinner>
          <div className="spinner" />
        </LoadingSpinner>
      </TeacherLayout>
    )
  }

  return (
    <TeacherLayout pageTitle="My Exams">
      {error && (
        <Alert type="error">
          <div className="icon">⚠</div>
          <div className="content">
            <p>{error}</p>
          </div>
        </Alert>
      )}

      <SectionTitle>My Exams</SectionTitle>

      <FilterContainer>
        <span style={{ fontWeight: 500, color: COLORS.text }}>Filter by Status:</span>
        <FilterButton $active={statusFilter === 'all'} onClick={() => setStatusFilter('all')}>
          All Exams ({exams.length})
        </FilterButton>
        <FilterButton
          $active={statusFilter === 'Upcoming'}
          onClick={() => setStatusFilter('Upcoming')}
        >
          Upcoming ({exams.filter((e) => getExamStatus(e) === 'Upcoming').length})
        </FilterButton>
        <FilterButton
          $active={statusFilter === 'Active'}
          onClick={() => setStatusFilter('Active')}
        >
          Active ({exams.filter((e) => getExamStatus(e) === 'Active').length})
        </FilterButton>
        <FilterButton
          $active={statusFilter === 'Ended'}
          onClick={() => setStatusFilter('Ended')}
        >
          Ended ({exams.filter((e) => getExamStatus(e) === 'Ended').length})
        </FilterButton>
        <PrimaryButton onClick={handleCreateExam} style={{ marginLeft: 'auto' }}>
          + Create New Exam
        </PrimaryButton>
      </FilterContainer>

      {filteredExams.length === 0 ? (
        <NoExamsMessage>
          <div className="icon">■</div>
          <h3>{statusFilter === 'all' ? 'No Exams Yet' : `No ${statusFilter} Exams`}</h3>
          <p>
            {statusFilter === 'all'
              ? "You haven't created any exams yet. Get started by creating your first exam."
              : `There are no ${statusFilter.toLowerCase()} exams at the moment.`}
          </p>
          {statusFilter === 'all' && (
            <PrimaryButton onClick={handleCreateExam}>Create First Exam</PrimaryButton>
          )}
        </NoExamsMessage>
      ) : (
        <>
          {/* Card View for larger screens */}
          <ExamsList>
            {filteredExams.map((exam) => {
              const status = getExamStatus(exam)
              const isUpcoming = status === 'Upcoming'

              return (
                <ExamCard key={exam.examID} status={status}>
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

                  <div className="actions">
                    <SecondaryButton onClick={() => handleViewExam(exam.examID)}>
                      View
                    </SecondaryButton>
                    {isUpcoming && (
                      <>
                        <SecondaryButton onClick={() => handleEditExam(exam.examID)}>
                          Edit
                        </SecondaryButton>
                        <DangerButton onClick={() => handleDeleteExam(exam.examID)}>
                          Delete
                        </DangerButton>
                      </>
                    )}
                    {status === 'Ended' && (
                      <PrimaryButton onClick={() => handleGradeExam(exam.examID)}>
                        Grade
                      </PrimaryButton>
                    )}
                  </div>
                </ExamCard>
              )
            })}
          </ExamsList>

          {/* Table View for medium screens */}
          <ExamTableContainer style={{ display: 'none' }}>
            <TableContainer>
              <Table>
                <thead>
                  <tr>
                    <th>Title</th>
                    <th>Questions</th>
                    <th>Total Marks</th>
                    <th>Pass %</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredExams.map((exam) => {
                    const status = getExamStatus(exam)
                    const isUpcoming = status === 'Upcoming'

                    return (
                      <tr key={exam.examID}>
                        <td>{exam.title}</td>
                        <td>{exam.totalQuestions || 0}</td>
                        <td>{exam.totalMarks}</td>
                        <td>{exam.passingPercentage?.toFixed(1)}%</td>
                        <td>
                          <StatusBadge status={status}>{status}</StatusBadge>
                        </td>
                        <td>
                          <ActionButtons>
                            <SecondaryButton onClick={() => handleViewExam(exam.examID)}>
                              View
                            </SecondaryButton>
                            {isUpcoming && (
                              <>
                                <SecondaryButton onClick={() => handleEditExam(exam.examID)}>
                                  Edit
                                </SecondaryButton>
                                <DangerButton onClick={() => handleDeleteExam(exam.examID)}>
                                  Delete
                                </DangerButton>
                              </>
                            )}
                            {status === 'Ended' && (
                              <PrimaryButton onClick={() => handleGradeExam(exam.examID)}>
                                Grade
                              </PrimaryButton>
                            )}
                          </ActionButtons>
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </Table>
            </TableContainer>
          </ExamTableContainer>
        </>
      )}
    </TeacherLayout>
  )
}
