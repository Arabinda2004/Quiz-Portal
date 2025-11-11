import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { adminService } from '../../services/api'
import {
  PageHeader,
  DataTable,
  Card,
  StatusBadge,
  EmptyState,
  LoadingSpinner,
  Alert,
  SearchBar,
} from '../../styles/AdminStyles'

export default function UserManagement() {
  const navigate = useNavigate()
  const { user } = useAuth()
  const [users, setUsers] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [searchTerm, setSearchTerm] = useState('')
  const [filterRole, setFilterRole] = useState('All')

  useEffect(() => {
    loadUsers()
  }, [])

  const loadUsers = async () => {
    try {
      setLoading(true)
      setError('')
      const data = await adminService.getAllUsers()
      setUsers(data || [])
    } catch (err) {
      setError(err.message || 'Failed to load users')
    } finally {
      setLoading(false)
    }
  }



  // Filter users
  const filteredUsers = users.filter(userItem => {
    const matchesSearch =
      userItem.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      userItem.email.toLowerCase().includes(searchTerm.toLowerCase())
    const matchesRole = filterRole === 'All' || userItem.role === filterRole
    return matchesSearch && matchesRole
  })

  if (loading) {
    return (
      <Card style={{ textAlign: 'center', padding: '60px 20px' }}>
        <LoadingSpinner />
        <p style={{ marginTop: '20px' }}>Loading users...</p>
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
      {success && (
        <Alert type="success">
          {success}
          <button onClick={() => setSuccess('')}>×</button>
        </Alert>
      )}

      <PageHeader>
        <div>
          <h1>User Management</h1>
          <p>View all system users and their roles</p>
        </div>
      </PageHeader>

      <Card>
        <SearchBar>
          <input
            type="text"
            placeholder="Search by name or email..."
            value={searchTerm}
            onChange={e => setSearchTerm(e.target.value)}
          />
          <select
            value={filterRole}
            onChange={e => setFilterRole(e.target.value)}
            style={{ padding: '10px', borderRadius: '6px', border: '1px solid #d1d5db' }}
          >
            <option value="All">All Roles</option>
            <option value="Admin">Admin</option>
            <option value="Teacher">Teacher</option>
            <option value="Student">Student</option>
          </select>
        </SearchBar>

        {filteredUsers.length === 0 ? (
          <EmptyState>
            <h3>No users found</h3>
            <p>{searchTerm || filterRole !== 'All' ? 'Try adjusting your filters' : 'No users available'}</p>
          </EmptyState>
        ) : (
          <DataTable>
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Role</th>
                <th>Created</th>
              </tr>
            </thead>
            <tbody>
              {filteredUsers.map(userItem => (
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
        )}
      </Card>
    </>
  )
}
