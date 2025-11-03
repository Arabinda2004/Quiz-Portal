import styled from 'styled-components'

export const AdminContainer = styled.div`
  display: flex;
  min-height: 100vh;
  background-color: #f9fafb;
`

export const Sidebar = styled.div`
  width: 250px;
  background: linear-gradient(135deg, #1e40af 0%, #1e3a8a 100%);
  color: white;
  padding: 20px;
  box-shadow: 2px 0 8px rgba(0, 0, 0, 0.1);
  position: fixed;
  height: 100vh;
  overflow-y: auto;

  @media (max-width: 768px) {
    position: absolute;
    width: 200px;
    z-index: 1000;
    transform: translateX(-100%);
    transition: transform 0.3s ease;

    &.open {
      transform: translateX(0);
    }
  }
`

export const SidebarBrand = styled.div`
  font-size: 20px;
  font-weight: bold;
  margin-bottom: 30px;
  padding-bottom: 15px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.2);
`

export const SidebarMenu = styled.ul`
  list-style: none;
  padding: 0;
  margin: 0;

  li {
    margin-bottom: 10px;
  }
`

export const SidebarLink = styled.button`
  width: 100%;
  background: ${props => (props.active ? 'rgba(255, 255, 255, 0.2)' : 'transparent')};
  color: white;
  border: none;
  padding: 12px 16px;
  border-radius: 6px;
  cursor: pointer;
  text-align: left;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 10px;

  &:hover {
    background: rgba(255, 255, 255, 0.15);
    transform: translateX(4px);
  }

  &.active {
    background: rgba(255, 255, 255, 0.25);
    border-left: 3px solid white;
    padding-left: 13px;
  }
`

export const MainContent = styled.div`
  margin-left: 250px;
  flex: 1;
  padding: 30px;
  overflow-y: auto;

  @media (max-width: 768px) {
    margin-left: 0;
    padding: 20px;
  }
`

export const PageHeader = styled.div`
  background: white;
  padding: 20px;
  border-radius: 8px;
  margin-bottom: 30px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  display: flex;
  justify-content: space-between;
  align-items: center;

  h1 {
    margin: 0;
    color: #1f2937;
    font-size: 28px;
  }

  p {
    color: #6b7280;
    margin: 5px 0 0 0;
  }
`

export const HeaderActions = styled.div`
  display: flex;
  gap: 12px;
  align-items: center;
`

export const DataTable = styled.table`
  width: 100%;
  border-collapse: collapse;
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);

  thead {
    background-color: #f3f4f6;
    border-bottom: 2px solid #e5e7eb;
  }

  th {
    padding: 16px;
    text-align: left;
    font-weight: 600;
    color: #1f2937;
    font-size: 14px;
  }

  td {
    padding: 16px;
    border-bottom: 1px solid #e5e7eb;
    color: #4b5563;
    font-size: 14px;
  }

  tbody tr {
    transition: background-color 0.2s ease;

    &:hover {
      background-color: #f9fafb;
    }

    &:last-child td {
      border-bottom: none;
    }
  }
`

export const Card = styled.div`
  background: white;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  margin-bottom: 20px;
`

export const FormGroup = styled.div`
  margin-bottom: 20px;

  label {
    display: block;
    margin-bottom: 8px;
    font-weight: 600;
    color: #1f2937;
    font-size: 14px;
  }

  input,
  select,
  textarea {
    width: 100%;
    padding: 10px 12px;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    font-size: 14px;
    font-family: inherit;
    transition: all 0.3s ease;

    &:focus {
      outline: none;
      border-color: #1e40af;
      box-shadow: 0 0 0 3px rgba(30, 64, 175, 0.1);
    }

    &:disabled {
      background-color: #f3f4f6;
      cursor: not-allowed;
    }
  }

  textarea {
    resize: vertical;
    min-height: 100px;
  }
`

export const FormRow = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 20px;
  margin-bottom: 20px;
`

export const Button = styled.button`
  padding: 10px 20px;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: inline-flex;
  align-items: center;
  gap: 8px;

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
`

export const PrimaryButton = styled(Button)`
  background: linear-gradient(135deg, #1e40af 0%, #1e3a8a 100%);
  color: white;

  &:hover:not(:disabled) {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(30, 64, 175, 0.3);
  }
`

export const SecondaryButton = styled(Button)`
  background: #f3f4f6;
  color: #1f2937;
  border: 1px solid #d1d5db;

  &:hover:not(:disabled) {
    background: #e5e7eb;
  }
`

export const DangerButton = styled(Button)`
  background: #ef4444;
  color: white;

  &:hover:not(:disabled) {
    background: #dc2626;
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(239, 68, 68, 0.3);
  }
`

export const SuccessButton = styled(Button)`
  background: #10b981;
  color: white;

  &:hover:not(:disabled) {
    background: #059669;
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(16, 185, 129, 0.3);
  }
`

export const WarningButton = styled(Button)`
  background: #f59e0b;
  color: white;

  &:hover:not(:disabled) {
    background: #d97706;
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(245, 158, 11, 0.3);
  }
`

export const ActionCell = styled.div`
  display: flex;
  gap: 8px;
  flex-wrap: wrap;

  button {
    padding: 6px 12px;
    font-size: 12px;
  }
`

export const StatusBadge = styled.span`
  display: inline-block;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 600;
  background-color: ${props => {
    switch (props.status?.toLowerCase()) {
      case 'active':
        return '#d1fae5'
      case 'inactive':
        return '#fee2e2'
      case 'pending':
        return '#fef3c7'
      case 'admin':
        return '#fecaca'
      case 'teacher':
        return '#dbeafe'
      case 'student':
        return '#d1fae5'
      default:
        return '#f3f4f6'
    }
  }};
  color: ${props => {
    switch (props.status?.toLowerCase()) {
      case 'active':
        return '#065f46'
      case 'inactive':
        return '#7c2d12'
      case 'pending':
        return '#78350f'
      case 'admin':
        return '#7c2d12'
      case 'teacher':
        return '#0c4a6e'
      case 'student':
        return '#065f46'
      default:
        return '#374151'
    }
  }};
`

export const EmptyState = styled.div`
  text-align: center;
  padding: 60px 20px;
  color: #6b7280;

  svg {
    width: 64px;
    height: 64px;
    margin-bottom: 20px;
    opacity: 0.5;
  }

  h3 {
    font-size: 18px;
    margin: 20px 0 10px 0;
    color: #1f2937;
  }

  p {
    font-size: 14px;
    margin: 0;
  }
`

export const Modal = styled.div`
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
`

export const ModalContent = styled.div`
  background: white;
  border-radius: 8px;
  padding: 30px;
  max-width: 500px;
  width: 90%;
  box-shadow: 0 20px 25px rgba(0, 0, 0, 0.15);

  h2 {
    margin: 0 0 20px 0;
    color: #1f2937;
  }

  p {
    color: #6b7280;
    margin: 0 0 20px 0;
  }
`

export const ModalActions = styled.div`
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  margin-top: 24px;
`

export const Tabs = styled.div`
  display: flex;
  gap: 0;
  border-bottom: 2px solid #e5e7eb;
  margin-bottom: 20px;
  background: white;
  border-radius: 8px 8px 0 0;
`

export const Tab = styled.button`
  padding: 14px 20px;
  border: none;
  background: transparent;
  color: ${props => (props.active ? '#1e40af' : '#6b7280')};
  border-bottom: ${props => (props.active ? '3px solid #1e40af' : 'none')};
  cursor: pointer;
  font-weight: 600;
  font-size: 14px;
  transition: all 0.3s ease;
  white-space: nowrap;

  &:hover {
    color: #1e40af;
  }
`

export const TabContent = styled.div`
  display: ${props => (props.active ? 'block' : 'none')};
`

export const SearchBar = styled.div`
  display: flex;
  gap: 12px;
  margin-bottom: 20px;
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);

  input {
    flex: 1;
    padding: 10px 16px;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    font-size: 14px;

    &:focus {
      outline: none;
      border-color: #1e40af;
      box-shadow: 0 0 0 3px rgba(30, 64, 175, 0.1);
    }
  }
`

export const Pagination = styled.div`
  display: flex;
  gap: 8px;
  justify-content: center;
  margin-top: 20px;
  align-items: center;

  button {
    padding: 8px 12px;
    border: 1px solid #d1d5db;
    background: white;
    border-radius: 6px;
    cursor: pointer;
    font-size: 14px;
    transition: all 0.3s ease;

    &:hover:not(:disabled) {
      background: #1e40af;
      color: white;
      border-color: #1e40af;
    }

    &:disabled {
      cursor: not-allowed;
      opacity: 0.5;
    }
  }

  span {
    color: #6b7280;
    font-size: 14px;
  }
`

export const StatGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
`

export const StatCard = styled.div`
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  border-left: 4px solid #1e40af;

  h3 {
    margin: 0 0 10px 0;
    color: #6b7280;
    font-size: 14px;
    font-weight: 600;
  }

  .value {
    font-size: 32px;
    font-weight: bold;
    color: #1f2937;
    margin: 0;
  }

  .subtitle {
    font-size: 12px;
    color: #9ca3af;
    margin-top: 8px;
  }
`

export const LoadingSpinner = styled.div`
  display: inline-block;
  width: 20px;
  height: 20px;
  border: 3px solid #f3f4f6;
  border-top: 3px solid #1e40af;
  border-radius: 50%;
  animation: spin 1s linear infinite;

  @keyframes spin {
    0% {
      transform: rotate(0deg);
    }
    100% {
      transform: rotate(360deg);
    }
  }
`

export const Alert = styled.div`
  padding: 16px;
  border-radius: 6px;
  margin-bottom: 20px;
  display: flex;
  align-items: center;
  gap: 12px;
  background-color: ${props => {
    switch (props.type) {
      case 'success':
        return '#d1fae5'
      case 'error':
        return '#fee2e2'
      case 'warning':
        return '#fef3c7'
      case 'info':
        return '#dbeafe'
      default:
        return '#f3f4f6'
    }
  }};
  color: ${props => {
    switch (props.type) {
      case 'success':
        return '#065f46'
      case 'error':
        return '#7c2d12'
      case 'warning':
        return '#78350f'
      case 'info':
        return '#0c4a6e'
      default:
        return '#374151'
    }
  }};
  border-left: 4px solid ${props => {
    switch (props.type) {
      case 'success':
        return '#10b981'
      case 'error':
        return '#ef4444'
      case 'warning':
        return '#f59e0b'
      case 'info':
        return '#3b82f6'
      default:
        return '#9ca3af'
    }
  }};

  button {
    margin-left: auto;
    background: none;
    border: none;
    color: inherit;
    cursor: pointer;
    font-size: 18px;
    padding: 0;
  }
`
