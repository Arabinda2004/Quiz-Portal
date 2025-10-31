import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { authService, teacherService } from '../../services/api'
import {
  DashboardContainer,
  NavBar,
  NavLeft,
  Logo,
  NavMenu,
  UserInfo,
  LogoutButton,
  MainContent,
  PageTitle,
  WelcomeSection,
  Grid,
  StatCard,
  StatLabel,
  StatValue,
  Card,
  Table,
  ActionButton,
} from '../../styles/DashboardStyles'

export default function TeacherDashboard() {
  const navigate = useNavigate()
  const { user, logout } = useAuth()
  const [exams, setExams] = useState([])
  const [stats, setStats] = useState({
    totalExams: 0,
  })
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    loadDashboardData()
  }, [])

  const loadDashboardData = async () => {
    try {
      setLoading(true)
      const examsData = await teacherService.getExams()
      setExams(examsData.data || [])

      setStats({
        totalExams: examsData.count || 0,
      })
    } catch (err) {
      setError(err.message || 'Failed to load dashboard')
    } finally {
      setLoading(false)
    }
  }

  const handleLogout = async () => {
    try {
      await authService.logout()
      logout()
      navigate('/login')
    } catch (err) {
      console.error('Logout failed:', err)
      logout()
      navigate('/login')
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
    if (!window.confirm('Are you sure you want to delete this exam?')) {
      return
    }

    try {
      await teacherService.deleteExam(examId)
      loadDashboardData()
    } catch (err) {
      setError('Failed to delete exam')
    }
  }

  if (loading) {
    return (
      <DashboardContainer>
        <NavBar>
          <NavLeft>
            <Logo>Quiz Portal</Logo>
          </NavLeft>
        </NavBar>
        <MainContent>
          <PageTitle>Loading...</PageTitle>
        </MainContent>
      </DashboardContainer>
    )
  }

  return (
    <DashboardContainer>
      <NavBar>
        <NavLeft>
          <Logo>Quiz Portal</Logo>
        </NavLeft>
        <NavMenu>
          <UserInfo>
            <span>{user?.fullName} ({user?.role})</span>
          </UserInfo>
          <LogoutButton onClick={handleLogout}>Logout</LogoutButton>
        </NavMenu>
      </NavBar>

      <MainContent>
        <WelcomeSection>
          <h1>Welcome, {user?.fullName}</h1>
          <p>Manage your exams and monitor student progress</p>
        </WelcomeSection>

        {error && (
          <Card style={{ backgroundColor: '#fee', borderLeft: '4px solid #991b1b' }}>
            {error}
          </Card>
        )}

        <Grid>
          <StatCard>
            <StatLabel>Total Exams</StatLabel>
            <StatValue>{stats.totalExams}</StatValue>
          </StatCard>
        </Grid>

        <Card>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
            <PageTitle style={{ margin: 0 }}>My Exams</PageTitle>
            <ActionButton onClick={handleCreateExam}>Create New Exam</ActionButton>
          </div>

          {exams.length === 0 ? (
            <p style={{ color: '#6b7280', textAlign: 'center', padding: '20px' }}>
              No exams created yet. <button onClick={handleCreateExam} style={{ background: 'none', color: '#1e40af', border: 'none', cursor: 'pointer', textDecoration: 'underline' }}>Create one now</button>
            </p>
          ) : (
            <Table>
              <thead>
                <tr>
                  <th>Exam Title</th>
                  <th>Questions</th>
                  <th>Total Marks</th>
                  <th>Passing Marks</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {exams.map((exam) => {
                  const now = new Date()
                  const start = new Date(exam.scheduleStart)
                  const end = new Date(exam.scheduleEnd)
                  let status = 'Upcoming'

                  if (now < start) {
                    status = 'Upcoming'
                  } else if (now > end) {
                    status = 'Ended'
                  } else {
                    status = 'Active'
                  }

                  const isUpcoming = status === 'Upcoming'

                  return (
                    <tr key={exam.examID}>
                      <td>{exam.title}</td>
                      <td>{exam.totalQuestions || 0}</td>
                      <td>{exam.totalMarks}</td>
                      <td>{exam.passingMarks}</td>
                      <td>
                        <span style={{
                          display: 'inline-block',
                          padding: '4px 8px',
                          borderRadius: '4px',
                          fontSize: '12px',
                          fontWeight: '600',
                          backgroundColor: status === 'Active' ? '#ecfdf5' : status === 'Upcoming' ? '#fef3c7' : '#fee',
                          color: status === 'Active' ? '#065f46' : status === 'Upcoming' ? '#92400e' : '#991b1b',
                        }}>
                          {status}
                        </span>
                      </td>
                      <td>
                        <ActionButton onClick={() => handleViewExam(exam.examID)}>View</ActionButton>
                        {isUpcoming && (
                          <>
                            <ActionButton onClick={() => handleEditExam(exam.examID)}>Edit</ActionButton>
                            <ActionButton
                              onClick={() => handleDeleteExam(exam.examID)}
                              style={{ backgroundColor: '#dc2626' }}
                              onMouseEnter={(e) => (e.target.style.backgroundColor = '#b91c1c')}
                              onMouseLeave={(e) => (e.target.style.backgroundColor = '#dc2626')}
                            >
                              Delete
                            </ActionButton>
                          </>
                        )}
                        {status === 'Ended' && (
                          <ActionButton onClick={() => handleGradeExam(exam.examID)}>Grade</ActionButton>
                        )}
                        {status === 'Active' && (
                          <ActionButton 
                            disabled 
                            title="Grading is available only after the exam ends"
                            style={{ backgroundColor: '#d1d5db', cursor: 'not-allowed', opacity: 0.5 }}
                          >
                            Grade (Not Available)
                          </ActionButton>
                        )}
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </Table>
          )}
        </Card>
      </MainContent>
    </DashboardContainer>
  )
}
