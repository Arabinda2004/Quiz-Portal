import { useNavigate } from 'react-router-dom'
import { useEffect } from 'react'
import styled from 'styled-components'
import { useAuth } from '../context/AuthContext'

const LandingPage = () => {
  const navigate = useNavigate()
  const { isAuthenticated, user, loading } = useAuth()

  // Redirect if already authenticated
  useEffect(() => {
    if (!loading && isAuthenticated && user) {
      if (user.role === 'Teacher') {
        navigate('/teacher/dashboard', { replace: true })
      } else if (user.role === 'Student') {
        navigate('/student/dashboard', { replace: true })
      } else if (user.role === 'Admin') {
        navigate('/admin/dashboard', { replace: true })
      }
    }
  }, [isAuthenticated, user, loading, navigate])

  // Show nothing while checking authentication
  if (loading) {
    return null
  }

  // Don't render landing page if authenticated
  if (isAuthenticated) {
    return null
  }

  return (
    <Container>
      {/* Header */}
      <Header>
        <Logo>
          <LogoText>ExamPortal</LogoText>
        </Logo>
        <Nav>
          <NavLink onClick={() => navigate('/login')}>Login</NavLink>
          <NavButton onClick={() => navigate('/register')}>Get Started</NavButton>
        </Nav>
      </Header>

      {/* Hero Section */}
      <HeroSection>
        <HeroContent>
          <HeroTitle>
            Modern Online Examination
            <Highlight> Platform</Highlight>
          </HeroTitle>
          <HeroSubtitle>
            Create, manage, and grade exams seamlessly. A comprehensive solution for educators and students.
          </HeroSubtitle>
          <HeroButtons>
            <PrimaryButton onClick={() => navigate('/register')}>
              Start Free Trial
            </PrimaryButton>
            <SecondaryButton onClick={() => navigate('/login')}>
              Sign In
            </SecondaryButton>
          </HeroButtons>
        </HeroContent>
      </HeroSection>

      {/* Roles Section */}
      <RolesSection>
        <SectionTitle>Built For Everyone</SectionTitle>
        <SectionSubtitle>
          Tailored experiences for different user roles
        </SectionSubtitle>
        <RolesGrid>
          <RoleCard>
            <RoleIconWrapper color="#1E40AF">
              <RoleIconText>T</RoleIconText>
            </RoleIconWrapper>
            <RoleTitle>For Teachers</RoleTitle>
            <RoleFeatures>
              <RoleFeature>Create and manage exams</RoleFeature>
              <RoleFeature>Add multiple question types</RoleFeature>
              <RoleFeature>Grade student responses</RoleFeature>
              <RoleFeature>Publish and analyze results</RoleFeature>
              <RoleFeature>Track student performance</RoleFeature>
            </RoleFeatures>
          </RoleCard>
          <RoleCard>
            <RoleIconWrapper color="#3B82F6">
              <RoleIconText>S</RoleIconText>
            </RoleIconWrapper>
            <RoleTitle>For Students</RoleTitle>
            <RoleFeatures>
              <RoleFeature>Browse available exams</RoleFeature>
              <RoleFeature>Take exams online</RoleFeature>
              <RoleFeature>Instant MCQ feedback</RoleFeature>
              <RoleFeature>View detailed results</RoleFeature>
              <RoleFeature>Track your progress</RoleFeature>
            </RoleFeatures>
          </RoleCard>
          <RoleCard>
            <RoleIconWrapper color="#60A5FA">
              <RoleIconText>A</RoleIconText>
            </RoleIconWrapper>
            <RoleTitle>For Admins</RoleTitle>
            <RoleFeatures>
              <RoleFeature>Manage users and roles</RoleFeature>
              <RoleFeature>Monitor system activity</RoleFeature>
              <RoleFeature>Access audit logs</RoleFeature>
              <RoleFeature>Configure platform settings</RoleFeature>
              <RoleFeature>Generate reports</RoleFeature>
            </RoleFeatures>
          </RoleCard>
        </RolesGrid>
      </RolesSection>

      {/* CTA Section */}
      <CTASection>
        <CTAContent>
          <CTATitle>Ready to Get Started?</CTATitle>
          <CTASubtitle>
            Join thousands of educators and students using ExamPortal
          </CTASubtitle>
          <CTAButton onClick={() => navigate('/register')}>
            Create Your Account
          </CTAButton>
        </CTAContent>
      </CTASection>

      {/* Footer */}
      <Footer>
        <FooterContent>
          <FooterSection>
            <FooterLogo>
              <LogoText>ExamPortal</LogoText>
            </FooterLogo>
            <FooterText>
              Modern examination platform for the digital age.
            </FooterText>
          </FooterSection>
          <FooterSection>
            <FooterTitle>Product</FooterTitle>
            <FooterLink>Features</FooterLink>
            <FooterLink>Pricing</FooterLink>
            <FooterLink>Security</FooterLink>
          </FooterSection>
          <FooterSection>
            <FooterTitle>Company</FooterTitle>
            <FooterLink>About</FooterLink>
            <FooterLink>Contact</FooterLink>
            <FooterLink>Support</FooterLink>
          </FooterSection>
          <FooterSection>
            <FooterTitle>Legal</FooterTitle>
            <FooterLink>Privacy</FooterLink>
            <FooterLink>Terms</FooterLink>
            <FooterLink>Cookies</FooterLink>
          </FooterSection>
        </FooterContent>
        <FooterBottom>
          <FooterCopyright>
            © 2025 ExamPortal. All rights reserved.
          </FooterCopyright>
        </FooterBottom>
      </Footer>
    </Container>
  )
}

// Styled Components
const Container = styled.div`
  min-height: 100vh;
  background: #ffffff;
`

const Header = styled.header`
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem 5%;
  background: #ffffff;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  position: sticky;
  top: 0;
  z-index: 1000;
`

const Logo = styled.div`
  display: flex;
  align-items: center;
  gap: 0.75rem;
  cursor: pointer;
`

const LogoText = styled.span`
  font-size: 1.5rem;
  font-weight: 700;
  color: #1f2937;
`

const Nav = styled.nav`
  display: flex;
  align-items: center;
  gap: 1.5rem;
`

const NavLink = styled.button`
  background: none;
  border: none;
  color: #4b5563;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: color 0.3s;

  &:hover {
    color: #1E40AF;
  }
`

const NavButton = styled.button`
  padding: 0.625rem 1.5rem;
  background: #1E40AF;
  color: white;
  border: none;
  border-radius: 0.5rem;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.3s;

  &:hover {
    background: #1E3A8A;
  }
`

const HeroSection = styled.section`
  padding: 6rem 5% 8rem;
  background: #1E40AF;
  text-align: center;
`

const HeroContent = styled.div`
  max-width: 1200px;
  margin: 0 auto;
`

const HeroTitle = styled.h1`
  font-size: 4rem;
  font-weight: 800;
  color: #ffffff;
  margin-bottom: 1.5rem;
  line-height: 1.2;

  @media (max-width: 768px) {
    font-size: 2.5rem;
  }
`

const Highlight = styled.span`
  color: #60A5FA;
`

const HeroSubtitle = styled.p`
  font-size: 1.25rem;
  color: #DBEAFE;
  margin-bottom: 3rem;
  max-width: 700px;
  margin-left: auto;
  margin-right: auto;
  line-height: 1.6;
`

const HeroButtons = styled.div`
  display: flex;
  gap: 1rem;
  justify-content: center;
  flex-wrap: wrap;
`

const PrimaryButton = styled.button`
  padding: 1rem 2.5rem;
  background: #ffffff;
  color: #1E40AF;
  border: none;
  border-radius: 0.75rem;
  font-size: 1.125rem;
  font-weight: 600;
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 10px 20px rgba(0, 0, 0, 0.2);
  }
`

const SecondaryButton = styled.button`
  padding: 1rem 2.5rem;
  background: transparent;
  color: #ffffff;
  border: 2px solid #ffffff;
  border-radius: 0.75rem;
  font-size: 1.125rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.3s;

  &:hover {
    background: rgba(255, 255, 255, 0.1);
  }
`

const SectionTitle = styled.h2`
  font-size: 3rem;
  font-weight: 800;
  color: #1f2937;
  text-align: center;
  margin-bottom: 1rem;
`

const SectionSubtitle = styled.p`
  font-size: 1.25rem;
  color: #6b7280;
  text-align: center;
  margin-bottom: 4rem;
`

const RolesSection = styled.section`
  padding: 6rem 5%;
  background: #ffffff;
`

const RolesGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 2.5rem;
  max-width: 1200px;
  margin: 0 auto;
`

const RoleCard = styled.div`
  background: #F9FAFB;
  padding: 2.5rem;
  border-radius: 1rem;
  text-align: center;
  transition: transform 0.3s;

  &:hover {
    transform: translateY(-5px);
  }
`

const RoleIconWrapper = styled.div`
  width: 80px;
  height: 80px;
  background: ${props => props.color};
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 1.5rem;
`

const RoleIconText = styled.span`
  font-size: 2.5rem;
  font-weight: 700;
  color: #ffffff;
`

const RoleTitle = styled.h3`
  font-size: 1.75rem;
  font-weight: 700;
  color: #1f2937;
  margin-bottom: 1.5rem;
`

const RoleFeatures = styled.ul`
  list-style: none;
  padding: 0;
`

const RoleFeature = styled.li`
  font-size: 1rem;
  color: #6b7280;
  padding: 0.75rem 0;
  border-bottom: 1px solid #e5e7eb;

  &:last-child {
    border-bottom: none;
  }

  &:before {
    content: "✓ ";
    color: #1E40AF;
    font-weight: bold;
    margin-right: 0.5rem;
  }
`

const CTASection = styled.section`
  padding: 6rem 5%;
  background: #1f2937;
  text-align: center;
`

const CTAContent = styled.div`
  max-width: 800px;
  margin: 0 auto;
`

const CTATitle = styled.h2`
  font-size: 3rem;
  font-weight: 800;
  color: #ffffff;
  margin-bottom: 1rem;
`

const CTASubtitle = styled.p`
  font-size: 1.25rem;
  color: #D1D5DB;
  margin-bottom: 2.5rem;
`

const CTAButton = styled.button`
  padding: 1.25rem 3rem;
  background: #1E40AF;
  color: #ffffff;
  border: none;
  border-radius: 0.75rem;
  font-size: 1.125rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.3s, transform 0.2s;

  &:hover {
    background: #1E3A8A;
    transform: translateY(-2px);
  }
`

const Footer = styled.footer`
  background: #111827;
  color: #D1D5DB;
  padding: 4rem 5% 2rem;
`

const FooterContent = styled.div`
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 1fr;
  gap: 3rem;
  max-width: 1200px;
  margin: 0 auto 3rem;

  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }
`

const FooterSection = styled.div`
  display: flex;
  flex-direction: column;
  gap: 1rem;
`

const FooterLogo = styled.div`
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 0.5rem;
`

const FooterTitle = styled.h4`
  font-size: 1rem;
  font-weight: 600;
  color: #ffffff;
  margin-bottom: 0.5rem;
`

const FooterText = styled.p`
  font-size: 0.875rem;
  color: #9CA3AF;
  line-height: 1.6;
`

const FooterLink = styled.a`
  font-size: 0.875rem;
  color: #9CA3AF;
  cursor: pointer;
  transition: color 0.3s;

  &:hover {
    color: #ffffff;
  }
`

const FooterBottom = styled.div`
  border-top: 1px solid #374151;
  padding-top: 2rem;
  text-align: center;
`

const FooterCopyright = styled.p`
  font-size: 0.875rem;
  color: #9CA3AF;
`

export default LandingPage
