import { useState } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { authService } from '../services/api'
import styled from 'styled-components'

const LayoutContainer = styled.div`
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  background-color: #F5F7FA;
`

const NavBar = styled.nav`
  background-color: #2c3e50;
  color: white;
  padding: 0;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
  display: flex;
  align-items: center;
  position: sticky;
  top: 0;
  z-index: 1000;
  min-height: 60px;
`

const NavContent = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  padding: 0 24px;
  gap: 24px;
`

const NavLogo = styled.div`
  font-size: 18px;
  font-weight: 700;
  white-space: nowrap;
  color: white;
  letter-spacing: -0.3px;
  cursor: pointer;
  
  &:hover {
    opacity: 0.8;
  }
`

const MenuSection = styled.div`
  display: flex;
  align-items: center;
  gap: 4px;
  flex: 1;
`

const MenuItem = styled.button`
  padding: 8px 16px;
  background: ${(props) => (props.$active ? 'rgba(52, 152, 219, 0.2)' : 'transparent')};
  border: none;
  color: ${(props) => (props.$active ? '#ffffff' : 'rgba(255, 255, 255, 0.7)')};
  cursor: pointer;
  border-radius: 4px;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.25s ease;
  font-size: 13px;
  font-weight: 500;
  border-bottom: ${(props) => (props.$active ? '2px solid #3498db' : '2px solid transparent')};
  white-space: nowrap;

  &:hover {
    background: rgba(52, 152, 219, 0.12);
    color: white;
  }

  span:first-child {
    font-size: 14px;
    flex-shrink: 0;
    color: ${(props) => (props.$active ? '#3498db' : 'rgba(255, 255, 255, 0.6)')};
  }

  span:last-child {
    white-space: nowrap;
  }
`

const NavRight = styled.div`
  display: flex;
  align-items: center;
  gap: 16px;
  margin-left: auto;
`

const UserInfo = styled.div`
  display: flex;
  align-items: center;
  gap: 12px;
  padding-right: 16px;
  border-right: 1px solid rgba(255, 255, 255, 0.2);
`

const UserName = styled.span`
  font-size: 13px;
  font-weight: 500;
  color: rgba(255, 255, 255, 0.9);
`

const UserRole = styled.span`
  font-size: 11px;
  color: rgba(255, 255, 255, 0.6);
`

const LogoutBtn = styled.button`
  padding: 6px 12px;
  background: rgba(204, 0, 0, 0.15);
  border: 1px solid rgba(204, 0, 0, 0.25);
  color: #FF6B6B;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  font-weight: 600;
  transition: all 0.2s ease;
  white-space: nowrap;

  &:hover {
    background: rgba(204, 0, 0, 0.2);
    border-color: rgba(204, 0, 0, 0.35);
  }
`

const MainContent = styled.main`
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
`

const PageHeader = styled.div`
  background: white;
  padding: 16px 24px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 20px;
  min-height: 60px;

  h1 {
    font-size: 22px;
    font-weight: 600;
    color: #1F2937;
    margin: 0;
    letter-spacing: -0.3px;
  }

  @media (max-width: 768px) {
    h1 {
      font-size: 18px;
    }
  }
`

const ContentArea = styled.div`
  padding: 24px;
  overflow-y: auto;
  flex: 1;

  @media (max-width: 768px) {
    padding: 16px;
  }
`

const MobileMenuButton = styled.button`
  display: none;
  background: none;
  border: none;
  color: white;
  cursor: pointer;
  font-size: 20px;
  padding: 8px;

  @media (max-width: 768px) {
    display: flex;
    align-items: center;
    justify-content: center;
  }
`

const MobileMenuContainer = styled.div`
  display: none;
  flex-direction: column;
  gap: 4px;
  max-height: ${(props) => (props.$isOpen ? '300px' : '0')};
  overflow: hidden;
  transition: max-height 0.3s ease;
  background: rgba(0, 0, 0, 0.1);
  border-top: 1px solid rgba(255, 255, 255, 0.1);

  @media (max-width: 768px) {
    display: flex;
    padding: 8px 0;
  }
`

const MobileMenuItem = styled.button`
  width: 100%;
  padding: 10px 24px;
  background: transparent;
  border: none;
  color: ${(props) => (props.$active ? '#3498db' : 'rgba(255, 255, 255, 0.7)')};
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.25s ease;
  font-size: 13px;
  font-weight: 500;
  text-align: left;

  &:hover {
    background: rgba(52, 152, 219, 0.12);
    color: white;
  }

  span:first-child {
    font-size: 14px;
  }
`

export default function TeacherLayout({ children, pageTitle = 'Dashboard' }) {
  const navigate = useNavigate()
  const location = useLocation()
  const { user, logout } = useAuth()
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)

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

  const toggleMobileMenu = () => {
    setMobileMenuOpen(!mobileMenuOpen)
  }

  const menuItems = [
    {
      label: 'Dashboard',
      path: '/teacher/dashboard',
      icon: '⊞',
    },
    {
      label: 'My Exams',
      path: '/teacher/exams',
      icon: '≡',
    },
    {
      label: 'Create Exam',
      path: '/teacher/create-exam',
      icon: '+',
    },
    {
      label: 'Results',
      path: '/teacher/results',
      icon: '∞',
    },
    {
      label: 'Grading',
      path: '/teacher/grading',
      icon: '✓',
    },
  ]

  const isActive = (path) => {
    if (path === '/teacher/dashboard') {
      return location.pathname === path
    }
    return location.pathname.startsWith(path) && path !== '/teacher/dashboard'
  }

  return (
    <LayoutContainer>
      <NavBar>
        <NavContent>
          <NavLogo onClick={() => navigate('/teacher/dashboard')}>📝 Quiz Portal</NavLogo>
          
          <MenuSection>
            {menuItems.map((item) => (
              <MenuItem
                key={item.path}
                $active={isActive(item.path)}
                onClick={() => {
                  navigate(item.path)
                  setMobileMenuOpen(false)
                }}
              >
                <span>{item.icon}</span>
                <span>{item.label}</span>
              </MenuItem>
            ))}
          </MenuSection>

          <NavRight>
            <UserInfo>
              <div>
                <UserName>{user?.fullName || 'User'}</UserName>
                <UserRole>Teacher</UserRole>
              </div>
            </UserInfo>
            <LogoutBtn onClick={handleLogout}>Logout</LogoutBtn>
          </NavRight>

          <MobileMenuButton onClick={toggleMobileMenu}>☰</MobileMenuButton>
        </NavContent>
      </NavBar>

      <MobileMenuContainer $isOpen={mobileMenuOpen}>
        {menuItems.map((item) => (
          <MobileMenuItem
            key={item.path}
            $active={isActive(item.path)}
            onClick={() => {
              navigate(item.path)
              setMobileMenuOpen(false)
            }}
          >
            <span>{item.icon}</span>
            <span>{item.label}</span>
          </MobileMenuItem>
        ))}
      </MobileMenuContainer>

      <MainContent>
        <PageHeader>
          <h1>{pageTitle}</h1>
          <div style={{ fontSize: '13px', color: '#6B7280', whiteSpace: 'nowrap' }}>
            Welcome back, <strong>{user?.fullName}</strong>
          </div>
        </PageHeader>
        <ContentArea>{children}</ContentArea>
      </MainContent>
    </LayoutContainer>
  )
}
