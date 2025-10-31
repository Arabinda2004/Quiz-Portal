import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { authService, adminService } from '../../services/api'
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
  DangerButton,
} from '../../styles/DashboardStyles'
import { ErrorMessage } from '../../styles/SharedStyles'

export default function AdminDashboard() {
  const navigate = useNavigate()
  const { user, logout } = useAuth()
  const [users, setUsers] = useState([])
  const [exams, setExams] = useState([])
  const [stats, setStats] = useState({
    totalUsers: 0,
    totalTeachers: 0,
    totalStudents: 0,
    totalExams: 0,
  })
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [activeTab, setActiveTab] = useState('users')

  useEffect(() => {
    loadDashboardData()
  }, [])

  const loadDashboardData = async () => {
    try {
      setLoading(true)
      
      // Load users
      const usersData = await adminService.getAllUsers()
      setUsers(usersData || [])

      // Load exams
      const examsData = await adminService.getAllExams()
      setExams(examsData.data || [])

      // Calculate stats
      const teachers = usersData?.filter(u => u.role === 'Teacher').length || 0
      const students = usersData?.filter(u => u.role === 'Student').length || 0

      setStats({
        totalUsers: usersData?.length || 0,
        totalTeachers: teachers,
        totalStudents: students,
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

  const handleCreateUser = () => {
    navigate('/admin/create-user')
  }

  const handleEditUser = (userId) => {
    navigate(`/admin/edit-user/${userId}`)
  }

  const handleDeleteUser = async (userId, userName) => {
    if (!window.confirm(`Are you sure you want to delete ${userName}?`)) {
      return
    }

    try {
      await adminService.deleteUser(userId)
      loadDashboardData()
    } catch (err) {
      setError('Failed to delete user')
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
          <h1>Admin Dashboard</h1>
          <p>Manage users, exams, and system settings</p>
        </WelcomeSection>

        {error && (
          <Card style={{ backgroundColor: '#fee', borderLeft: '4px solid #991b1b' }}>
            {error}
          </Card>
        )}

        <Grid>
          <StatCard>
            <StatLabel>Total Users</StatLabel>
            <StatValue>{stats.totalUsers}</StatValue>
          </StatCard>
          <StatCard>
            <StatLabel>Teachers</StatLabel>
            <StatValue>{stats.totalTeachers}</StatValue>
          </StatCard>
          <StatCard>
            <StatLabel>Students</StatLabel>
            <StatValue>{stats.totalStudents}</StatValue>
          </StatCard>
          <StatCard>
            <StatLabel>Total Exams</StatLabel>
            <StatValue>{stats.totalExams}</StatValue>
          </StatCard>
        </Grid>

        <Card>
          <div style={{ display: 'flex', gap: '10px', marginBottom: '20px', borderBottom: '1px solid #e5e7eb', paddingBottom: '16px' }}>
            <button
              onClick={() => setActiveTab('users')}
              style={{
                padding: '8px 16px',
                border: 'none',
                background: activeTab === 'users' ? '#1e40af' : '#f3f4f6',
                color: activeTab === 'users' ? 'white' : '#1f2937',
                borderRadius: '4px',
                cursor: 'pointer',
                fontWeight: '600',
                fontSize: '14px',
              }}
            >
              Users
            </button>
            <button
              onClick={() => setActiveTab('exams')}
              style={{
                padding: '8px 16px',
                border: 'none',
                background: activeTab === 'exams' ? '#1e40af' : '#f3f4f6',
                color: activeTab === 'exams' ? 'white' : '#1f2937',
                borderRadius: '4px',
                cursor: 'pointer',
                fontWeight: '600',
                fontSize: '14px',
              }}
            >
              Exams
            </button>
          </div>

          {activeTab === 'users' && (
            <>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
                <PageTitle style={{ margin: 0 }}>Users Management</PageTitle>
                <ActionButton onClick={handleCreateUser}>Add New User</ActionButton>
              </div>

              {users.length === 0 ? (
                <p style={{ color: '#6b7280', textAlign: 'center', padding: '20px' }}>No users found</p>
              ) : (
                <Table>
                  <thead>
                    <tr>
                      <th>Name</th>
                      <th>Email</th>
                      <th>Role</th>
                      <th>Created At</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {users.map((userItem) => (
                      <tr key={userItem.userID}>
                        <td>{userItem.fullName}</td>
                        <td>{userItem.email}</td>
                        <td>
                          <span style={{
                            display: 'inline-block',
                            padding: '4px 8px',
                            borderRadius: '4px',
                            fontSize: '12px',
                            fontWeight: '600',
                            backgroundColor: userItem.role === 'Teacher' ? '#dbeafe' : userItem.role === 'Admin' ? '#fecaca' : '#d1fae5',
                            color: userItem.role === 'Teacher' ? '#0c4a6e' : userItem.role === 'Admin' ? '#7c2d12' : '#065f46',
                          }}>
                            {userItem.role}
                          </span>
                        </td>
                        <td>{new Date(userItem.createdAt).toLocaleDateString()}</td>
                        <td>
                          <ActionButton onClick={() => handleEditUser(userItem.userID)}>Edit</ActionButton>
                          <DangerButton onClick={() => handleDeleteUser(userItem.userID, userItem.fullName)}>Delete</DangerButton>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              )}
            </>
          )}

          {activeTab === 'exams' && (
            <>
              <PageTitle style={{ marginTop: 0 }}>Exams</PageTitle>

              {exams.length === 0 ? (
                <p style={{ color: '#6b7280', textAlign: 'center', padding: '20px' }}>No exams found</p>
              ) : (
                <Table>
                  <thead>
                    <tr>
                      <th>Title</th>
                      <th>Creator</th>
                      <th>Total Marks</th>
                      <th>Duration (mins)</th>
                      <th>Schedule Start</th>
                    </tr>
                  </thead>
                  <tbody>
                    {exams.map((exam) => (
                      <tr key={exam.examID}>
                        <td>{exam.title}</td>
                        <td>{exam.createdByName || 'N/A'}</td>
                        <td>{exam.totalMarks}</td>
                        <td>{exam.durationMinutes}</td>
                        <td>{new Date(exam.scheduleStart).toLocaleDateString()}</td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              )}
            </>
          )}
        </Card>
      </MainContent>
    </DashboardContainer>
  )
}
