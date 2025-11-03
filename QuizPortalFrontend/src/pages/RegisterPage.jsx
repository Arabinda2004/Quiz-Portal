import { useState, useEffect } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import styled from 'styled-components'
import { useAuth } from '../context/AuthContext'
import { authService } from '../services/api'
import {
  AuthContainer,
  AuthCard,
  Title,
  Subtitle,
  FormGroup,
  Label,
  Input,
  Select,
  Button,
  LinkText,
  ErrorMessage,
  LoadingSpinner,
} from '../styles/SharedStyles'

export default function RegisterPage() {
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    password: '',
    confirmPassword: '',
    role: 'Student',
  })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const navigate = useNavigate()
  const { login, isAuthenticated, user } = useAuth()

  // Redirect if already authenticated
  useEffect(() => {
    if (isAuthenticated && user) {
      if (user.role === 'Teacher') {
        navigate('/teacher/dashboard', { replace: true })
      } else if (user.role === 'Student') {
        navigate('/student/dashboard', { replace: true })
      } else if (user.role === 'Admin') {
        navigate('/admin/dashboard', { replace: true })
      }
    }
  }, [isAuthenticated, user, navigate])

  const handleChange = (e) => {
    const { name, value } = e.target
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }))
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')

    // Validation
    if (formData.password !== formData.confirmPassword) {
      setError('Passwords do not match')
      return
    }

    if (formData.password.length < 6) {
      setError('Password must be at least 6 characters')
      return
    }

    setLoading(true)

    try {
      const response = await authService.register(formData)

      if (response.success) {
        login(response.user)
        
        // Redirect based on role
        if (response.user.role === 'Teacher') {
          navigate('/teacher/dashboard')
        } else if (response.user.role === 'Student') {
          navigate('/student/dashboard')
        } else if (response.user.role === 'Admin') {
          navigate('/admin/dashboard')
        }
      } else {
        setError(response.message || 'Registration failed')
      }
    } catch (err) {
      setError(err.message || 'An error occurred during registration')
    } finally {
      setLoading(false)
    }
  }

  return (
    <AuthContainerCompact>
      <BackButton onClick={() => navigate('/')}>
        ← Back to Home
      </BackButton>
      <AuthCardCompact>
        <TitleCompact>Create Account</TitleCompact>
        <SubtitleCompact>Join the Quiz Portal</SubtitleCompact>

        {error && <ErrorMessageCompact>{error}</ErrorMessageCompact>}

        <form onSubmit={handleSubmit}>
          <FormGroupCompact>
            <LabelCompact htmlFor="fullName">Full Name</LabelCompact>
            <InputCompact
              id="fullName"
              type="text"
              name="fullName"
              value={formData.fullName}
              onChange={handleChange}
              placeholder="Your full name"
              required
              disabled={loading}
            />
          </FormGroupCompact>

          <FormGroupCompact>
            <LabelCompact htmlFor="email">Email</LabelCompact>
            <InputCompact
              id="email"
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              placeholder="you@example.com"
              required
              disabled={loading}
            />
          </FormGroupCompact>

          <FormGroupCompact>
            <LabelCompact htmlFor="role">Role</LabelCompact>
            <SelectCompact
              id="role"
              name="role"
              value={formData.role}
              onChange={handleChange}
              disabled={loading}
            >
              <option value="Student">Student</option>
              <option value="Teacher">Teacher</option>
            </SelectCompact>
          </FormGroupCompact>

          <FormGroupCompact>
            <LabelCompact htmlFor="password">Password</LabelCompact>
            <InputCompact
              id="password"
              type="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              placeholder="Enter password (min 6 characters)"
              required
              disabled={loading}
            />
          </FormGroupCompact>

          <FormGroupCompact>
            <LabelCompact htmlFor="confirmPassword">Confirm Password</LabelCompact>
            <InputCompact
              id="confirmPassword"
              type="password"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleChange}
              placeholder="Confirm password"
              required
              disabled={loading}
            />
          </FormGroupCompact>

          <ButtonCompact type="submit" disabled={loading}>
            {loading ? <LoadingSpinner /> : 'Create Account'}
          </ButtonCompact>
        </form>

        <LinkTextCompact>
          Already have an account? <Link to="/login">Sign in</Link>
        </LinkTextCompact>
      </AuthCardCompact>
    </AuthContainerCompact>
  )
}

const BackButton = styled.button`
  position: absolute;
  top: 2rem;
  left: 2rem;
  padding: 0.75rem 1.5rem;
  background: transparent;
  color: #1E40AF;
  border: 2px solid #1E40AF;
  border-radius: 0.5rem;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  z-index: 10;

  &:hover {
    background: #1E40AF;
    color: white;
    transform: translateX(-5px);
  }

  @media (max-width: 768px) {
    top: 1rem;
    left: 1rem;
    padding: 0.5rem 1rem;
    font-size: 0.875rem;
  }
`

// Compact styled components for single-page fit
const AuthContainerCompact = styled.div`
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  background: linear-gradient(135deg, #1e3a8a 0%, #1e40af 100%);
  padding: 15px;
  overflow: hidden;
`

const AuthCardCompact = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
  padding: 24px;
  width: 100%;
  max-width: 400px;
  max-height: 95vh;
  overflow-y: auto;

  @media (max-width: 768px) {
    max-width: 350px;
    padding: 16px;
  }
`

const TitleCompact = styled.h1`
  font-size: 22px;
  font-weight: 700;
  color: #111827;
  margin-bottom: 4px;
  text-align: center;
`

const SubtitleCompact = styled.p`
  text-align: center;
  color: #6b7280;
  font-size: 12px;
  margin-bottom: 12px;
`

const FormGroupCompact = styled.div`
  margin-bottom: 12px;
`

const LabelCompact = styled.label`
  display: block;
  margin-bottom: 4px;
  color: #1f2937;
  font-weight: 500;
  font-size: 12px;
`

const InputCompact = styled.input`
  width: 100%;
  padding: 8px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 13px;
  transition: all 0.2s;

  &:focus {
    outline: none;
    border-color: #1e40af;
    box-shadow: 0 0 0 3px rgba(30, 64, 175, 0.1);
  }
`

const SelectCompact = styled.select`
  width: 100%;
  padding: 8px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 13px;
  background-color: white;
  transition: all 0.2s;

  &:focus {
    outline: none;
    border-color: #1e40af;
    box-shadow: 0 0 0 3px rgba(30, 64, 175, 0.1);
  }
`

const ButtonCompact = styled.button`
  width: 100%;
  padding: 10px;
  background-color: #1e40af;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.2s;
  margin-top: 4px;

  &:hover {
    background-color: #1e3a8a;
  }

  &:disabled {
    background-color: #9ca3af;
    cursor: not-allowed;
  }
`

const ErrorMessageCompact = styled.div`
  background-color: #fee;
  color: #991b1b;
  padding: 8px;
  border-radius: 6px;
  margin-bottom: 12px;
  font-size: 12px;
  border-left: 4px solid #991b1b;
`

const LinkTextCompact = styled.p`
  text-align: center;
  margin-top: 10px;
  color: #6b7280;
  font-size: 12px;

  a {
    color: #1e40af;
    text-decoration: none;
    font-weight: 600;

    &:hover {
      text-decoration: underline;
    }
  }
`
