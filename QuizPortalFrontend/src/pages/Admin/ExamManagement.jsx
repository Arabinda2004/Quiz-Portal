import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { adminService } from '../../services/api'
import {
  PageHeader,
  DataTable,
  Card,
  SecondaryButton,
  StatusBadge,
  EmptyState,
  LoadingSpinner,
  Alert,
  SearchBar,
  ActionCell,
} from '../../styles/AdminStyles'

export default function ExamManagement() {
  const navigate = useNavigate()
  const [exams, setExams] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [searchTerm, setSearchTerm] = useState('')

  useEffect(() => {
    loadExams()
  }, [])

  const loadExams = async () => {
    try {
      setLoading(true)
      setError('')
      const response = await adminService.getAllExams()
      setExams(response.data || [])
    } catch (err) {
      setError(err.message || 'Failed to load exams')
    } finally {
      setLoading(false)
    }
  }

  const handleViewExam = examId => {
    navigate(`/admin/exam/${examId}`)
  }

  // Filter exams
  const filteredExams = exams.filter(exam =>
    exam.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
    exam.createdByName?.toLowerCase().includes(searchTerm.toLowerCase())
  )

  if (loading) {
    return (
      <Card style={{ textAlign: 'center', padding: '60px 20px' }}>
        <LoadingSpinner />
        <p style={{ marginTop: '20px' }}>Loading exams...</p>
      </Card>
    )
  }

  return (
    <>
      {error && (
        <Alert type="error">
          {error}
          <button onClick={() => setError('')}>×</button>
        </Alert>
      )}

      <PageHeader>
        <div>
          <h1>Exam Management</h1>
          <p>View all exams in the system</p>
        </div>
      </PageHeader>

      <Card>
        <SearchBar>
          <input
            type="text"
            placeholder="Search by exam title or creator..."
            value={searchTerm}
            onChange={e => setSearchTerm(e.target.value)}
          />
        </SearchBar>

        {filteredExams.length === 0 ? (
          <EmptyState>
            <h3>No exams found</h3>
            <p>{searchTerm ? 'Try adjusting your search' : 'No exams have been created yet'}</p>
          </EmptyState>
        ) : (
          <DataTable>
            <thead>
              <tr>
                <th>Title</th>
                <th>Creator</th>
                <th>Total Marks</th>
                <th>Duration (mins)</th>
                <th>Start Date</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filteredExams.map(exam => {
                const startDate = new Date(exam.scheduleStart)
                const now = new Date()
                let status = 'Not Started'
                if (startDate <= now) {
                  status = exam.status
                }

                return (
                  <tr key={exam.examID}>
                    <td>
                      <strong>{exam.title}</strong>
                    </td>
                    <td>{exam.createdByName || 'Unknown'}</td>
                    <td>{exam.totalMarks}</td>
                    <td>{exam.durationMinutes}</td>
                    <td>{startDate.toLocaleDateString()}</td>
                    <td>
                      <StatusBadge status={status}>{status}</StatusBadge>
                    </td>
                    <td>
                      <ActionCell>
                        <SecondaryButton onClick={() => handleViewExam(exam.examID)}>View</SecondaryButton>
                      </ActionCell>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </DataTable>
        )}
      </Card>
    </>
  )
}
