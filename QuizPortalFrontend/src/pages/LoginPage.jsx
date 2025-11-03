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
  Button,
  LinkText,
  ErrorMessage,
  LoadingSpinner,
} from '../styles/SharedStyles'

export default function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
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

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    try {
      const response = await authService.login(email, password)

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
        setError(response.message || 'Login failed')
      }
    } catch (err) {
      setError(err.message || 'An error occurred during login')
    } finally {
      setLoading(false)
    }
  }

  return (
    <AuthContainer>
      <BackButton onClick={() => navigate('/')}>
        ← Back to Home
      </BackButton>
      <AuthCard>
        <Title>Quiz Portal</Title>
        <Subtitle>Sign in to your account</Subtitle>

        {error && <ErrorMessage>{error}</ErrorMessage>}

        <form onSubmit={handleSubmit}>
          <FormGroup>
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@example.com"
              required
              disabled={loading}
            />
          </FormGroup>

          <FormGroup>
            <Label htmlFor="password">Password</Label>
            <Input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter your password"
              required
              disabled={loading}
            />
          </FormGroup>

          <Button type="submit" disabled={loading}>
            {loading ? <LoadingSpinner /> : 'Sign In'}
          </Button>
        </form>

        <LinkText>
          Don't have an account? <Link to="/register">Create one</Link>
        </LinkText>
      </AuthCard>
    </AuthContainer>
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
