import { useState, useEffect } from 'react'
import { adminService } from '../../services/api'
import {
  PageHeader,
  Card,
  StatGrid,
  StatCard,
  DataTable,
  EmptyState,
  LoadingSpinner,
  Alert,
  Tabs,
  Tab,
  TabContent,
} from '../../styles/AdminStyles'

export default function SystemReports() {
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
  const [activeTab, setActiveTab] = useState('overview')

  useEffect(() => {
    loadReports()
  }, [])

  const loadReports = async () => {
    try {
      setLoading(true)
      setError('')

      const [usersData, examsData] = await Promise.all([
        adminService.getAllUsers(),
        adminService.getAllExams().catch(() => ({ data: [] })),
      ])

      setUsers(usersData || [])
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
      setError(err.message || 'Failed to load reports')
    } finally {
      setLoading(false)
    }
  }

  const getExamStats = () => {
    if (exams.length === 0) return { published: 0, notPublished: 0, avgDuration: 0 }

    const published = exams.filter(e => e.isPublished).length
    const notPublished = exams.length - published
    const avgDuration = exams.reduce((sum, e) => sum + (e.durationMinutes || 0), 0) / exams.length

    return { published, notPublished, avgDuration: Math.round(avgDuration) }
  }

  const getUserByRole = role => {
    return users.filter(u => u.role === role)
  }

  if (loading) {
    return (
      <Card style={{ textAlign: 'center', padding: '60px 20px' }}>
        <LoadingSpinner />
        <p style={{ marginTop: '20px' }}>Loading reports...</p>
      </Card>
    )
  }

  const examStats = getExamStats()

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
          <h1>System Reports</h1>
          <p>System statistics and analytics</p>
        </div>
      </PageHeader>

      <Tabs>
        <Tab active={activeTab === 'overview'} onClick={() => setActiveTab('overview')}>
          Overview
        </Tab>
        <Tab active={activeTab === 'users'} onClick={() => setActiveTab('users')}>
          User Details
        </Tab>
        <Tab active={activeTab === 'exams'} onClick={() => setActiveTab('exams')}>
          Exam Details
        </Tab>
      </Tabs>

      <TabContent active={activeTab === 'overview'}>
        <Card>
          <h2 style={{ marginTop: 0 }}>System Overview</h2>

          <h3 style={{ marginTop: '30px', marginBottom: '15px', color: '#1f2937' }}>User Statistics</h3>
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
          </StatGrid>

          <h3 style={{ marginTop: '30px', marginBottom: '15px', color: '#1f2937' }}>Exam Statistics</h3>
          <StatGrid>
            <StatCard>
              <h3>Total Exams</h3>
              <p className="value">{stats.totalExams}</p>
            </StatCard>
            <StatCard>
              <h3>Published Exams</h3>
              <p className="value">{examStats.published}</p>
            </StatCard>
            <StatCard>
              <h3>Draft Exams</h3>
              <p className="value">{examStats.notPublished}</p>
            </StatCard>
            <StatCard>
              <h3>Avg. Duration</h3>
              <p className="value">{examStats.avgDuration}</p>
              <p className="subtitle">minutes</p>
            </StatCard>
          </StatGrid>
        </Card>
      </TabContent>

      <TabContent active={activeTab === 'users'}>
        <Card>
          <h2 style={{ marginTop: 0 }}>User Details</h2>

          {['Admin', 'Teacher', 'Student'].map(role => {
            const roleUsers = getUserByRole(role)
            return (
              <div key={role} style={{ marginTop: '30px' }}>
                <h3 style={{ marginBottom: '15px', color: '#1f2937' }}>
                  {role}s ({roleUsers.length})
                </h3>

                {roleUsers.length === 0 ? (
                  <EmptyState style={{ padding: '20px' }}>
                    <p>No {role.toLowerCase()}s found</p>
                  </EmptyState>
                ) : (
                  <div style={{ overflowX: 'auto' }}>
                    <DataTable>
                      <thead>
                        <tr>
                          <th>Name</th>
                          <th>Email</th>
                          <th>Created At</th>
                        </tr>
                      </thead>
                      <tbody>
                        {roleUsers.map(user => (
                          <tr key={user.userID}>
                            <td>
                              <strong>{user.fullName}</strong>
                            </td>
                            <td>{user.email}</td>
                            <td>{new Date(user.createdAt).toLocaleDateString()}</td>
                          </tr>
                        ))}
                      </tbody>
                    </DataTable>
                  </div>
                )}
              </div>
            )
          })}
        </Card>
      </TabContent>

      <TabContent active={activeTab === 'exams'}>
        <Card>
          <h2 style={{ marginTop: 0 }}>Exam Details</h2>

          {exams.length === 0 ? (
            <EmptyState>
              <h3>No exams found</h3>
            </EmptyState>
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
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {exams.map(exam => (
                    <tr key={exam.examID}>
                      <td>
                        <strong>{exam.title}</strong>
                      </td>
                      <td>{exam.createdByName || 'Unknown'}</td>
                      <td>{exam.totalMarks}</td>
                      <td>{exam.durationMinutes}</td>
                      <td>{new Date(exam.scheduleStart).toLocaleDateString()}</td>
                      <td>{exam.isPublished ? 'Published' : 'Draft'}</td>
                    </tr>
                  ))}
                </tbody>
              </DataTable>
            </div>
          )}
        </Card>
      </TabContent>
    </>
  )
}
