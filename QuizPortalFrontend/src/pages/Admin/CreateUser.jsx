import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { adminService } from '../../services/api'
import {
  PageHeader,
  Card,
  FormGroup,
  FormRow,
  PrimaryButton,
  SecondaryButton,
  Alert,
  LoadingSpinner,
} from '../../styles/AdminStyles'

export default function CreateUser() {
  const navigate = useNavigate()
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    role: 'Student',
    password: '',
  })
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const handleChange = e => {
    const { name, value } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: value,
    }))
  }

  const handleSubmit = async e => {
    e.preventDefault()
    setError('')
    setSuccess('')

    // Validate
    if (!formData.fullName.trim()) {
      setError('Full name is required')
      return
    }
    if (!formData.email.trim()) {
      setError('Email is required')
      return
    }
    if (!formData.role) {
      setError('Role is required')
      return
    }

    try {
      setLoading(true)
      const userData = {
        fullName: formData.fullName,
        email: formData.email,
        role: formData.role,
        password: formData.password || null,
      }
      await adminService.createUser(userData)
      setSuccess('User created successfully!')
      setTimeout(() => {
        navigate('/admin/users')
      }, 1500)
    } catch (err) {
      setError(err.message || 'Failed to create user')
    } finally {
      setLoading(false)
    }
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
          <h1>Create New User</h1>
          <p>Add a new user to the system</p>
        </div>
      </PageHeader>

      <Card style={{ maxWidth: '500px' }}>
        <form onSubmit={handleSubmit}>
          <FormGroup>
            <label htmlFor="fullName">Full Name *</label>
            <input
              type="text"
              id="fullName"
              name="fullName"
              value={formData.fullName}
              onChange={handleChange}
              placeholder="Enter full name"
              disabled={loading}
              required
            />
          </FormGroup>

          <FormGroup>
            <label htmlFor="email">Email Address *</label>
            <input
              type="email"
              id="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              placeholder="Enter email address"
              disabled={loading}
              required
            />
          </FormGroup>

          <FormGroup>
            <label htmlFor="role">Role *</label>
            <select
              id="role"
              name="role"
              value={formData.role}
              onChange={handleChange}
              disabled={loading}
              required
            >
              <option value="Student">Student</option>
              <option value="Teacher">Teacher</option>
              <option value="Admin">Admin</option>
            </select>
          </FormGroup>

          <FormGroup>
            <label htmlFor="password">Password (Optional)</label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              placeholder="Leave empty to generate default password"
              disabled={loading}
            />
            <small style={{ color: '#6b7280', marginTop: '6px', display: 'block' }}>
              If left empty, a default password will be generated and the user will be prompted to change it on first login.
            </small>
          </FormGroup>

          <div style={{ display: 'flex', gap: '12px', marginTop: '30px' }}>
            <PrimaryButton type="submit" disabled={loading}>
              {loading ? (
                <>
                  <LoadingSpinner />
                  Creating...
                </>
              ) : (
                'Create User'
              )}
            </PrimaryButton>
            <SecondaryButton type="button" onClick={() => navigate('/admin/users')} disabled={loading}>
              Cancel
            </SecondaryButton>
          </div>
        </form>
      </Card>
    </>
  )
}
