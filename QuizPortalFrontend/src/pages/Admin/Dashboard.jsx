import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { authService, adminService } from '../../services/api'
import {
  AdminContainer,
  Sidebar,
  SidebarBrand,
  SidebarMenu,
  SidebarLink,
  MainContent,
  PageHeader,
  Card,
  StatGrid,
  StatCard,
  DataTable,
  Alert,
  StatusBadge,
} from '../../styles/AdminStyles'

export default function AdminDashboard() {
  const navigate = useNavigate()
  const { user, logout } = useAuth()
  const [users, setUsers] = useState([])
  const [exams, setExams] = useState([])
  const [stats, setStats] = useState({
    totalUsers: 0,
    totalTeachers: 0,
    totalStudents: 0,
    totalAdmins: 0,
    totalExams: 0,
  })
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [activeMenu, setActiveMenu] = useState('overview')
  const [sidebarOpen, setSidebarOpen] = useState(true)

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
      const admins = usersData?.filter(u => u.role === 'Admin').length || 0

      setStats({
        totalUsers: usersData?.length || 0,
        totalTeachers: teachers,
        totalStudents: students,
        totalAdmins: admins,
        totalExams: examsData.data?.length || 0,
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

  const menuItems = [
    { id: 'overview', label: 'Dashboard', icon: '📊' },
    { id: 'users', label: 'User Management', icon: '👥' },
    { id: 'exams', label: 'Exam Management', icon: '📝' },
    { id: 'reports', label: 'Reports', icon: '📈' },
    // { id: 'settings', label: 'Settings', icon: '⚙️' },
    // { id: 'audit', label: 'Audit Logs', icon: '🔍' },
  ]

  const navigateTo = id => {
    setActiveMenu(id)
    switch (id) {
      case 'users':
        navigate('/admin/users')
        break
      case 'exams':
        navigate('/admin/exams')
        break
      case 'reports':
        navigate('/admin/reports')
        break
      case 'settings':
        navigate('/admin/settings')
        break
      case 'audit':
        navigate('/admin/audit-logs')
        break
      default:
        break
    }
  }

  if (loading) {
    return (
      <AdminContainer>
        <MainContent style={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <Card style={{ textAlign: 'center', padding: '60px 20px' }}>
            <p>Loading Dashboard...</p>
          </Card>
        </MainContent>
      </AdminContainer>
    )
  }

  return (
    <AdminContainer>
      <Sidebar className={sidebarOpen ? 'open' : ''}>
        <SidebarBrand>🎓 Quiz Portal</SidebarBrand>
        <SidebarMenu>
          {menuItems.map(item => (
            <li key={item.id}>
              <SidebarLink
                active={activeMenu === item.id}
                onClick={() => navigateTo(item.id)}
                className={activeMenu === item.id ? 'active' : ''}
              >
                <span style={{ fontSize: '18px' }}>{item.icon}</span>
                <span>{item.label}</span>
              </SidebarLink>
            </li>
          ))}
        </SidebarMenu>
        <div style={{ marginTop: 'auto', paddingTop: '20px', borderTop: '1px solid rgba(255, 255, 255, 0.2)' }}>
          <p style={{ fontSize: '12px', opacity: 0.8, margin: '0 0 12px 0' }}>Logged in as:</p>
          <p style={{ fontSize: '14px', fontWeight: '600', margin: '0 0 16px 0' }}>{user?.fullName}</p>
          <button
            onClick={handleLogout}
            style={{
              width: '100%',
              padding: '10px',
              background: 'rgba(255, 255, 255, 0.1)',
              color: 'white',
              border: '1px solid rgba(255, 255, 255, 0.2)',
              borderRadius: '6px',
              cursor: 'pointer',
              fontSize: '14px',
              fontWeight: '500',
            }}
          >
            Logout
          </button>
        </div>
      </Sidebar>

      <MainContent>
        {error && (
          <Alert type="error">
            {error}
            <button onClick={() => setError('')}>×</button>
          </Alert>
        )}

        <PageHeader>
          <div>
            <h1>Dashboard</h1>
            <p>Welcome to the Admin Dashboard</p>
          </div>
        </PageHeader>

        <StatGrid>
          <StatCard>
            <h3>Total Users</h3>
            <p className="value">{stats.totalUsers}</p>
          </StatCard>
          <StatCard>
            <h3>Administrators</h3>
            <p className="value">{stats.totalAdmins}</p>
          </StatCard>
          <StatCard>
            <h3>Teachers</h3>
            <p className="value">{stats.totalTeachers}</p>
          </StatCard>
          <StatCard>
            <h3>Students</h3>
            <p className="value">{stats.totalStudents}</p>
          </StatCard>
          <StatCard>
            <h3>Total Exams</h3>
            <p className="value">{stats.totalExams}</p>
          </StatCard>
        </StatGrid>

        <Card>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
            <h2 style={{ margin: 0 }}>Recent Users</h2>
          </div>

          {users.length === 0 ? (
            <p style={{ color: '#6b7280', textAlign: 'center', padding: '20px' }}>No users found</p>
          ) : (
            <div style={{ overflowX: 'auto' }}>
              <DataTable>
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Email</th>
                    <th>Role</th>
                    <th>Created At</th>
                  </tr>
                </thead>
                <tbody>
                  {users.slice(0, 10).map(userItem => (
                    <tr key={userItem.userID}>
                      <td>
                        <strong>{userItem.fullName}</strong>
                      </td>
                      <td>{userItem.email}</td>
                      <td>
                        <StatusBadge status={userItem.role}>{userItem.role}</StatusBadge>
                      </td>
                      <td>{new Date(userItem.createdAt).toLocaleDateString()}</td>
                    </tr>
                  ))}
                </tbody>
              </DataTable>
            </div>
          )}
        </Card>

        <Card>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
            <h2 style={{ margin: 0 }}>Recent Exams</h2>
          </div>

          {exams.length === 0 ? (
            <p style={{ color: '#6b7280', textAlign: 'center', padding: '20px' }}>No exams found</p>
          ) : (
            <div style={{ overflowX: 'auto' }}>
              <DataTable>
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
                  {exams.slice(0, 10).map(exam => (
                    <tr key={exam.examID}>
                      <td>
                        <strong>{exam.title}</strong>
                      </td>
                      <td>{exam.createdByName || 'N/A'}</td>
                      <td>{exam.totalMarks}</td>
                      <td>{exam.durationMinutes}</td>
                      <td>{new Date(exam.scheduleStart).toLocaleDateString()}</td>
                    </tr>
                  ))}
                </tbody>
              </DataTable>
            </div>
          )}
        </Card>
      </MainContent>
    </AdminContainer>
  )
}
