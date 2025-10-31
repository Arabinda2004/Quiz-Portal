import styled from 'styled-components'

export const DashboardContainer = styled.div`
  min-height: 100vh;
  background-color: #f9fafb;
`

export const NavBar = styled.nav`
  background-color: #1f2937;
  color: white;
  padding: 16px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
`

export const NavLeft = styled.div`
  display: flex;
  align-items: center;
  gap: 24px;
`

export const Logo = styled.h1`
  font-size: 20px;
  font-weight: 700;
  color: white;
`

export const NavMenu = styled.div`
  display: flex;
  gap: 20px;
  align-items: center;
`

export const NavLink = styled.span`
  color: #d1d5db;
  cursor: pointer;
  font-size: 14px;
  transition: color 0.2s;

  &:hover {
    color: white;
  }
`

export const UserInfo = styled.div`
  display: flex;
  align-items: center;
  gap: 12px;
  color: #d1d5db;
  font-size: 14px;
`

export const LogoutButton = styled.button`
  background-color: #dc2626;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
  transition: background-color 0.2s;

  &:hover {
    background-color: #b91c1c;
  }
`

export const MainContent = styled.div`
  padding: 32px 24px;
  max-width: 1200px;
  margin: 0 auto;
`

export const PageTitle = styled.h2`
  font-size: 24px;
  font-weight: 700;
  color: #111827;
  margin-bottom: 24px;
`

export const Card = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  padding: 24px;
  margin-bottom: 24px;
`

export const Grid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 20px;
  margin-bottom: 24px;
`

export const StatCard = styled.div`
  background: white;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  border-left: 4px solid #1e40af;
`

export const StatLabel = styled.p`
  color: #6b7280;
  font-size: 14px;
  margin-bottom: 8px;
`

export const StatValue = styled.p`
  font-size: 28px;
  font-weight: 700;
  color: #1f2937;
`

export const Table = styled.table`
  width: 100%;
  border-collapse: collapse;

  th {
    background-color: #f3f4f6;
    padding: 12px;
    text-align: left;
    font-weight: 600;
    color: #1f2937;
    border-bottom: 1px solid #e5e7eb;
    font-size: 14px;
  }

  td {
    padding: 12px;
    border-bottom: 1px solid #e5e7eb;
    font-size: 14px;
    color: #4b5563;
  }

  tr:hover {
    background-color: #f9fafb;
  }
`

export const ActionButton = styled.button`
  background-color: #1e40af;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 13px;
  font-weight: 600;
  margin-right: 8px;
  transition: background-color 0.2s;

  &:hover {
    background-color: #1e3a8a;
  }

  &:disabled {
    background-color: #9ca3af;
    cursor: not-allowed;
  }
`

export const DangerButton = styled(ActionButton)`
  background-color: #dc2626;

  &:hover {
    background-color: #b91c1c;
  }
`

export const WelcomeSection = styled.div`
  background: linear-gradient(135deg, #1e3a8a 0%, #1e40af 100%);
  color: white;
  padding: 32px 24px;
  border-radius: 8px;
  margin-bottom: 32px;

  h1 {
    font-size: 28px;
    font-weight: 700;
    margin-bottom: 8px;
  }

  p {
    font-size: 16px;
    opacity: 0.9;
  }
`

export const FormContainer = styled.form`
  background: white;
  border-radius: 8px;
  padding: 24px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  max-width: 600px;
`

export const FormButton = styled.button`
  background-color: #1e40af;
  color: white;
  border: none;
  padding: 12px 24px;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.2s;
  margin-right: 12px;

  &:hover {
    background-color: #1e3a8a;
  }

  &:disabled {
    background-color: #9ca3af;
    cursor: not-allowed;
  }
`
