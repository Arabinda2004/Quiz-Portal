import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { adminService } from '../../services/api'
import {
  PageHeader,
  Card,
  FormGroup,
  PrimaryButton,
  SecondaryButton,
  Alert,
  LoadingSpinner,
} from '../../styles/AdminStyles'

export default function EditUser() {
  const navigate = useNavigate()
  const { userId } = useParams()
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    role: 'Student',
  })
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  useEffect(() => {
    loadUser()
  }, [userId])

  const loadUser = async () => {
    try {
      setLoading(true)
      setError('')
      const user = await adminService.getUserById(userId)
      if (user) {
        setFormData({
          fullName: user.fullName,
          email: user.email,
          role: user.role,
        })
      }
    } catch (err) {
      setError(err.message || 'Failed to load user')
    } finally {
      setLoading(false)
    }
  }

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
      setSaving(true)
      await adminService.updateUser(userId, formData)
      setSuccess('User updated successfully!')
      setTimeout(() => {
        navigate('/admin/users')
      }, 1500)
    } catch (err) {
      setError(err.message || 'Failed to update user')
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return (
      <Card style={{ textAlign: 'center', padding: '60px 20px' }}>
        <LoadingSpinner />
        <p style={{ marginTop: '20px' }}>Loading user...</p>
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
          <h1>Edit User</h1>
          <p>Update user information and role</p>
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
              disabled={saving}
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
              disabled={saving}
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
              disabled={saving}
              required
            >
              <option value="Student">Student</option>
              <option value="Teacher">Teacher</option>
              <option value="Admin">Admin</option>
            </select>
          </FormGroup>

          <div style={{ display: 'flex', gap: '12px', marginTop: '30px' }}>
            <PrimaryButton type="submit" disabled={saving}>
              {saving ? (
                <>
                  <LoadingSpinner />
                  Saving...
                </>
              ) : (
                'Save Changes'
              )}
            </PrimaryButton>
            <SecondaryButton type="button" onClick={() => navigate('/admin/users')} disabled={saving}>
              Cancel
            </SecondaryButton>
          </div>
        </form>
      </Card>
    </>
  )
}
