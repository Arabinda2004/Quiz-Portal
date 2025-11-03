import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { adminService } from '../../services/api'
import {
  PageHeader,
  HeaderActions,
  DataTable,
  Card,
  PrimaryButton,
  DangerButton,
  SecondaryButton,
  ActionCell,
  StatusBadge,
  EmptyState,
  LoadingSpinner,
  Alert,
  SearchBar,
  Pagination,
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
  const [currentPage, setCurrentPage] = useState(1)
  const [itemsPerPage] = useState(10)

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

  const handleDeleteUser = async (userId, userName) => {
    if (!window.confirm(`Are you sure you want to delete "${userName}"? This action cannot be undone.`)) {
      return
    }

    try {
      setError('')
      setSuccess('')
      await adminService.deleteUser(userId)
      setSuccess(`User "${userName}" deleted successfully`)
      loadUsers()
      setTimeout(() => setSuccess(''), 3000)
    } catch (err) {
      setError(err.message || 'Failed to delete user')
    }
  }

  const handleEditUser = (userId) => {
    navigate(`/admin/edit-user/${userId}`)
  }

  const handleCreateUser = () => {
    navigate('/admin/create-user')
  }

  // Filter users
  const filteredUsers = users.filter(userItem => {
    const matchesSearch =
      userItem.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      userItem.email.toLowerCase().includes(searchTerm.toLowerCase())
    const matchesRole = filterRole === 'All' || userItem.role === filterRole
    return matchesSearch && matchesRole
  })

  // Paginate
  const totalPages = Math.ceil(filteredUsers.length / itemsPerPage)
  const startIndex = (currentPage - 1) * itemsPerPage
  const endIndex = startIndex + itemsPerPage
  const paginatedUsers = filteredUsers.slice(startIndex, endIndex)

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
          <p>Manage all system users and their roles</p>
        </div>
        <HeaderActions>
          <PrimaryButton onClick={handleCreateUser}>+ Add New User</PrimaryButton>
        </HeaderActions>
      </PageHeader>

      <Card>
        <SearchBar>
          <input
            type="text"
            placeholder="Search by name or email..."
            value={searchTerm}
            onChange={e => {
              setSearchTerm(e.target.value)
              setCurrentPage(1)
            }}
          />
          <select
            value={filterRole}
            onChange={e => {
              setFilterRole(e.target.value)
              setCurrentPage(1)
            }}
            style={{ padding: '10px', borderRadius: '6px', border: '1px solid #d1d5db' }}
          >
            <option value="All">All Roles</option>
            <option value="Admin">Admin</option>
            <option value="Teacher">Teacher</option>
            <option value="Student">Student</option>
          </select>
        </SearchBar>

        {paginatedUsers.length === 0 ? (
          <EmptyState>
            <h3>No users found</h3>
            <p>{searchTerm || filterRole !== 'All' ? 'Try adjusting your filters' : 'Click "Add New User" to get started'}</p>
          </EmptyState>
        ) : (
          <>
            <DataTable>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Role</th>
                  <th>Created</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {paginatedUsers.map(userItem => (
                  <tr key={userItem.userID}>
                    <td>
                      <strong>{userItem.fullName}</strong>
                    </td>
                    <td>{userItem.email}</td>
                    <td>
                      <StatusBadge status={userItem.role}>{userItem.role}</StatusBadge>
                    </td>
                    <td>{new Date(userItem.createdAt).toLocaleDateString()}</td>
                    <td>
                      <ActionCell>
                        <SecondaryButton onClick={() => handleEditUser(userItem.userID)}>Edit</SecondaryButton>
                        <DangerButton onClick={() => handleDeleteUser(userItem.userID, userItem.fullName)}>
                          Delete
                        </DangerButton>
                      </ActionCell>
                    </td>
                  </tr>
                ))}
              </tbody>
            </DataTable>

            {totalPages > 1 && (
              <Pagination>
                <button onClick={() => setCurrentPage(1)} disabled={currentPage === 1}>
                  First
                </button>
                <button onClick={() => setCurrentPage(Math.max(1, currentPage - 1))} disabled={currentPage === 1}>
                  Previous
                </button>
                <span>
                  Page {currentPage} of {totalPages}
                </span>
                <button
                  onClick={() => setCurrentPage(Math.min(totalPages, currentPage + 1))}
                  disabled={currentPage === totalPages}
                >
                  Next
                </button>
                <button onClick={() => setCurrentPage(totalPages)} disabled={currentPage === totalPages}>
                  Last
                </button>
              </Pagination>
            )}
          </>
        )}
      </Card>
    </>
  )
}
