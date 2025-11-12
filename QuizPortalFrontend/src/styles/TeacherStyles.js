import styled from 'styled-components'

// Professional Color Theme
export const COLORS = {
  primary: '#0052CC',           // Professional Blue
  primaryDark: '#003D99',       // Darker Blue
  primaryLight: '#0066FF',      // Light Blue
  secondary: '#1B6F3F',         // Professional Green
  danger: '#CC0000',            // Professional Red
  warning: '#E67E22',           // Professional Orange
  success: '#27AE60',           // Professional Green
  info: '#2E86AB',              // Professional Teal
  light: '#FAFBFC',             // Off-white
  background: '#F5F7FA',        // Light neutral gray
  border: '#D1D5DB',            // Neutral border
  text: '#1F2937',              // Dark gray text
  textSecondary: '#6B7280',     // Medium gray text
  textMuted: '#9CA3AF',         // Light gray text
  surface: '#FFFFFF',           // Pure white
}

// Dashboard Components
export const DashboardGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 20px;
  margin-bottom: 32px;
`

export const StatCard = styled.div`
  background: ${COLORS.surface};
  border: 1px solid ${COLORS.border};
  border-left: 4px solid ${(props) => props.accentColor || COLORS.primary};
  padding: 24px;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
  transition: all 0.2s ease;

  &:hover {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.12);
    border-left-color: ${(props) => props.accentColor || COLORS.primaryDark};
  }

  h3 {
    font-size: 12px;
    font-weight: 600;
    margin: 0 0 16px 0;
    color: ${COLORS.textMuted};
    text-transform: uppercase;
    letter-spacing: 0.8px;
  }

  .value {
    font-size: 32px;
    font-weight: 700;
    margin-bottom: 12px;
    color: ${COLORS.text};
    line-height: 1;
  }

  .subtitle {
    font-size: 13px;
    color: ${COLORS.textSecondary};
    line-height: 1.4;
  }
`

export const QuickActionCard = styled.div`
  background: ${COLORS.surface};
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
  border: 1px solid ${COLORS.border};
  transition: all 0.2s ease;
  cursor: pointer;

  &:hover {
    border-color: ${COLORS.primary};
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.12);
  }

  .icon {
    font-size: 28px;
    margin-bottom: 12px;
    width: 40px;
    height: 40px;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: ${COLORS.background};
    border-radius: 6px;
    color: ${COLORS.primary};
  }

  h4 {
    font-size: 15px;
    font-weight: 600;
    color: ${COLORS.text};
    margin: 0 0 8px 0;
  }

  p {
    font-size: 13px;
    color: ${COLORS.textSecondary};
    margin: 0;
    line-height: 1.4;
  }
`

export const ExamCard = styled.div`
  background: ${COLORS.surface};
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
  border: 1px solid ${COLORS.border};
  border-top: 3px solid ${(props) => {
    switch (props.status) {
      case 'Active':
        return COLORS.success
      case 'Upcoming':
        return COLORS.warning
      case 'Ended':
        return COLORS.danger
      default:
        return COLORS.primary
    }
  }};
  transition: all 0.2s ease;

  &:hover {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.12);
  }

  .header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 16px;
  }

  .title {
    font-size: 16px;
    font-weight: 600;
    color: ${COLORS.text};
    margin: 0;
  }

  .status-badge {
    display: inline-block;
    padding: 6px 12px;
    border-radius: 4px;
    font-size: 11px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    background-color: ${(props) => {
      switch (props.status) {
        case 'Active':
          return '#D4EDDA'
        case 'Upcoming':
          return '#FFF3CD'
        case 'Ended':
          return '#F8D7DA'
        default:
          return '#D1ECF1'
      }
    }};
    color: ${(props) => {
      switch (props.status) {
        case 'Active':
          return '#155724'
        case 'Upcoming':
          return '#856404'
        case 'Ended':
          return '#721C24'
        default:
          return '#0C5460'
      }
    }};
  }

  .info-row {
    display: flex;
    gap: 16px;
    margin-bottom: 12px;
    font-size: 13px;
    color: ${COLORS.textSecondary};

    .info-item {
      display: flex;
      gap: 4px;

      strong {
        color: ${COLORS.text};
        font-weight: 600;
      }
    }
  }

  .actions {
    display: flex;
    gap: 8px;
    margin-top: 16px;
    padding-top: 16px;
    border-top: 1px solid ${COLORS.border};

    button {
      flex: 1;
      font-size: 12px;
      padding: 8px 12px;
    }
  }
`

// Form Components
export const FormContainer = styled.div`
  background: ${COLORS.surface};
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
  padding: 32px;
  max-width: ${(props) => props.maxWidth || '900px'};
  margin: 0 auto;
`

export const FormSection = styled.div`
  margin-bottom: 32px;

  &:last-child {
    margin-bottom: 0;
  }
`

export const FormSectionTitle = styled.h2`
  font-size: 16px;
  font-weight: 600;
  color: ${COLORS.text};
  margin: 0 0 20px 0;
  padding-bottom: 12px;
  border-bottom: 1px solid ${COLORS.border};
  display: flex;
  align-items: center;
  gap: 12px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  font-size: 14px;

  .icon {
    font-size: 18px;
    color: ${COLORS.primary};
  }
`

export const FormGroup = styled.div`
  margin-bottom: 20px;
`

export const Label = styled.label`
  display: block;
  font-size: 14px;
  font-weight: 600;
  color: ${COLORS.text};
  margin-bottom: 8px;

  .required {
    color: ${COLORS.danger};
  }

  .helper-text {
    display: block;
    font-size: 12px;
    font-weight: 400;
    color: ${COLORS.textSecondary};
    margin-top: 2px;
  }
`

export const Input = styled.input`
  width: 100%;
  padding: 12px 14px;
  border: 1px solid ${COLORS.border};
  border-radius: 6px;
  font-size: 14px;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  transition: all 0.2s ease;
  background-color: ${COLORS.surface};

  &:focus {
    outline: none;
    border-color: ${COLORS.primary};
    box-shadow: 0 0 0 3px rgba(0, 82, 204, 0.1);
  }

  &:disabled {
    background-color: ${COLORS.background};
    color: ${COLORS.textMuted};
    cursor: not-allowed;
  }

  &::placeholder {
    color: ${COLORS.textMuted};
  }

  ${(props) => props.$error && `
    border-color: ${COLORS.danger};
    background-color: rgba(204, 0, 0, 0.02);
  `}
`

export const TextArea = styled.textarea`
  width: 100%;
  padding: 12px 14px;
  border: 1px solid ${COLORS.border};
  border-radius: 6px;
  font-size: 14px;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  resize: vertical;
  min-height: 120px;
  transition: all 0.2s ease;

  &:focus {
    outline: none;
    border-color: ${COLORS.primary};
    box-shadow: 0 0 0 3px rgba(0, 82, 204, 0.1);
  }

  &:disabled {
    background-color: ${COLORS.background};
    color: ${COLORS.textMuted};
    cursor: not-allowed;
  }

  &::placeholder {
    color: ${COLORS.textMuted};
  }

  ${(props) => props.$error && `
    border-color: ${COLORS.danger};
    background-color: rgba(204, 0, 0, 0.02);
  `}
`

export const Select = styled.select`
  width: 100%;
  padding: 12px 14px;
  border: 1px solid ${COLORS.border};
  border-radius: 6px;
  font-size: 14px;
  font-family: inherit;
  background-color: ${COLORS.surface};
  cursor: pointer;
  transition: all 0.2s ease;

  &:focus {
    outline: none;
    border-color: ${COLORS.primary};
    box-shadow: 0 0 0 3px rgba(0, 82, 204, 0.1);
  }

  &:disabled {
    background-color: ${COLORS.background};
    color: ${COLORS.textMuted};
    cursor: not-allowed;
  }

  ${(props) => props.$error && `
    border-color: ${COLORS.danger};
  `}
`

export const FormError = styled.div`
  display: flex;
  align-items: flex-start;
  gap: 8px;
  color: ${COLORS.danger};
  font-size: 13px;
  margin-top: 6px;
  padding: 8px 12px;
  background-color: rgba(220, 38, 38, 0.05);
  border-radius: 6px;
  border-left: 3px solid ${COLORS.danger};
`

export const FormSuccess = styled.div`
  display: flex;
  align-items: flex-start;
  gap: 8px;
  color: ${COLORS.success};
  font-size: 13px;
  padding: 12px 14px;
  background-color: rgba(16, 185, 129, 0.05);
  border-radius: 6px;
  border-left: 3px solid ${COLORS.success};
  margin-bottom: 20px;
`

export const TwoColumnGrid = styled.div`
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;

  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }
`

export const ThreeColumnGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;

  @media (max-width: 1024px) {
    grid-template-columns: 1fr 1fr;
  }

  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }
`

// Button Styles
export const Button = styled.button`
  padding: 12px 24px;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  white-space: nowrap;

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }

  &:active:not(:disabled) {
    transform: scale(0.98);
  }
`

export const PrimaryButton = styled(Button)`
  background-color: ${COLORS.primary};
  color: white;
  box-shadow: 0 2px 4px rgba(0, 82, 204, 0.15);

  &:hover:not(:disabled) {
    background-color: ${COLORS.primaryDark};
    box-shadow: 0 4px 8px rgba(0, 82, 204, 0.25);
  }
`

export const SecondaryButton = styled(Button)`
  background-color: ${COLORS.background};
  color: ${COLORS.text};
  border: 1px solid ${COLORS.border};
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);

  &:hover:not(:disabled) {
    background-color: ${COLORS.border};
    border-color: ${COLORS.textMuted};
  }
`

export const SuccessButton = styled(Button)`
  background-color: ${COLORS.success};
  color: white;
  box-shadow: 0 2px 4px rgba(39, 174, 96, 0.15);

  &:hover:not(:disabled) {
    background-color: #1E8449;
    box-shadow: 0 4px 8px rgba(39, 174, 96, 0.25);
  }
`

export const DangerButton = styled(Button)`
  background-color: ${COLORS.danger};
  color: white;
  box-shadow: 0 2px 4px rgba(204, 0, 0, 0.15);

  &:hover:not(:disabled) {
    background-color: #990000;
    box-shadow: 0 4px 8px rgba(204, 0, 0, 0.25);
  }
`

export const WarningButton = styled(Button)`
  background-color: ${COLORS.warning};
  color: white;
  box-shadow: 0 2px 4px rgba(230, 126, 34, 0.15);

  &:hover:not(:disabled) {
    background-color: #D35400;
    box-shadow: 0 4px 8px rgba(230, 126, 34, 0.25);
  }
`

export const OutlineButton = styled(Button)`
  background-color: transparent;
  color: ${COLORS.primary};
  border: 2px solid ${COLORS.primary};

  &:hover:not(:disabled) {
    background-color: rgba(0, 82, 204, 0.05);
  }
`

// Button Group
export const ButtonGroup = styled.div`
  display: flex;
  gap: 12px;
  justify-content: ${(props) => props.justify || 'flex-end'};
  margin-top: 32px;
  padding-top: 20px;
  border-top: 1px solid ${COLORS.border};

  @media (max-width: 768px) {
    flex-direction: column;
    gap: 10px;

    button {
      width: 100%;
    }
  }
`

// Card Components
export const Card = styled.div`
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  overflow: hidden;
  margin-bottom: 20px;
  transition: all 0.3s ease;

  &:hover {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  }
`

export const CardHeader = styled.div`
  padding: 20px;
  background-color: ${(props) => props.background || 'white'};
  border-bottom: 1px solid ${COLORS.border};
  display: flex;
  justify-content: space-between;
  align-items: center;

  h3 {
    font-size: 16px;
    font-weight: 600;
    color: ${COLORS.text};
    margin: 0;
  }
`

export const CardContent = styled.div`
  padding: 20px;
`

export const CardFooter = styled.div`
  padding: 16px 20px;
  background-color: ${COLORS.background};
  border-top: 1px solid ${COLORS.border};
  display: flex;
  justify-content: ${(props) => props.justify || 'flex-end'};
  gap: 12px;
`

// Table Components
export const TableContainer = styled.div`
  width: 100%;
  overflow-x: auto;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);

  &::-webkit-scrollbar {
    height: 6px;
  }

  &::-webkit-scrollbar-track {
    background: ${COLORS.background};
  }

  &::-webkit-scrollbar-thumb {
    background: ${COLORS.border};
    border-radius: 3px;

    &:hover {
      background: ${COLORS.textMuted};
    }
  }
`

export const Table = styled.table`
  width: 100%;
  border-collapse: collapse;
  background: white;

  thead {
    background-color: ${COLORS.background};
    border-bottom: 2px solid ${COLORS.border};
  }

  th {
    padding: 14px 16px;
    text-align: left;
    font-weight: 600;
    color: ${COLORS.text};
    font-size: 13px;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }

  td {
    padding: 14px 16px;
    border-bottom: 1px solid ${COLORS.border};
    font-size: 14px;
    color: ${COLORS.textSecondary};
  }

  tbody tr {
    transition: background-color 0.2s ease;

    &:hover {
      background-color: ${COLORS.light};
    }
  }
`

export const StatusBadge = styled.span`
  display: inline-block;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  background-color: ${(props) => {
    switch (props.status) {
      case 'Active':
        return '#dcfce7'
      case 'Upcoming':
        return '#fef3c7'
      case 'Ended':
        return '#fee2e2'
      case 'Draft':
        return '#f3f4f6'
      case 'Published':
        return '#dbeafe'
      default:
        return '#f1f5f9'
    }
  }};
  color: ${(props) => {
    switch (props.status) {
      case 'Active':
        return '#166534'
      case 'Upcoming':
        return '#92400e'
      case 'Ended':
        return '#991b1b'
      case 'Draft':
        return '#4b5563'
      case 'Published':
        return '#0c4a6e'
      default:
        return '#64748b'
    }
  }};
`

// Alert/Message Components
export const Alert = styled.div`
  padding: 14px 16px;
  border-radius: 6px;
  border-left: 4px solid;
  margin-bottom: 20px;
  display: flex;
  gap: 12px;
  align-items: flex-start;

  .icon {
    font-size: 16px;
    flex-shrink: 0;
    margin-top: 1px;
  }

  .content {
    flex: 1;
  }

  p {
    margin: 0;
    font-size: 14px;
    line-height: 1.5;
  }

  ${(props) => {
    switch (props.type) {
      case 'error':
        return `
          background-color: rgba(204, 0, 0, 0.05);
          border-color: ${COLORS.danger};
          color: #660000;
        `
      case 'success':
        return `
          background-color: rgba(27, 111, 63, 0.05);
          border-color: ${COLORS.success};
          color: #0D3B1F;
        `
      case 'warning':
        return `
          background-color: rgba(230, 126, 34, 0.05);
          border-color: ${COLORS.warning};
          color: #663300;
        `
      case 'info':
        return `
          background-color: rgba(46, 134, 171, 0.05);
          border-color: ${COLORS.info};
          color: #1B3E54;
        `
      default:
        return `
          background-color: rgba(0, 82, 204, 0.05);
          border-color: ${COLORS.primary};
          color: #003366;
        `
    }
  }}
`

// Modal/Dialog Components
export const Modal = styled.div`
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: ${(props) => (props.$isOpen ? 'flex' : 'none')};
  align-items: center;
  justify-content: center;
  z-index: 2000;
  padding: 20px;
`

export const ModalContent = styled.div`
  background: white;
  border-radius: 12px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  max-width: ${(props) => props.maxWidth || '500px'};
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  animation: slideUp 0.3s ease;

  @keyframes slideUp {
    from {
      opacity: 0;
      transform: translateY(20px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }
`

// Loading Spinner
export const Spinner = styled.div`
  display: inline-block;
  width: 20px;
  height: 20px;
  border: 3px solid ${COLORS.border};
  border-top-color: ${COLORS.primary};
  border-radius: 50%;
  animation: spin 0.6s linear infinite;

  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }
`

// Empty State
export const EmptyState = styled.div`
  text-align: center;
  padding: 60px 20px;
  color: ${COLORS.textSecondary};

  .icon {
    font-size: 64px;
    margin-bottom: 16px;
    opacity: 0.5;
  }

  h3 {
    font-size: 18px;
    font-weight: 600;
    color: ${COLORS.text};
    margin-bottom: 8px;
  }

  p {
    font-size: 14px;
    margin-bottom: 20px;
  }
`

// Breadcrumb
export const Breadcrumb = styled.div`
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 20px;
  font-size: 14px;

  a {
    color: ${COLORS.primary};
    text-decoration: none;
    cursor: pointer;

    &:hover {
      text-decoration: underline;
    }
  }

  span {
    color: ${COLORS.textMuted};
  }
`

// Badge
export const Badge = styled.span`
  display: inline-block;
  padding: ${(props) => props.size === 'small' ? '4px 8px' : '6px 12px'};
  border-radius: 12px;
  font-size: ${(props) => props.size === 'small' ? '11px' : '12px'};
  font-weight: 600;
  background-color: ${(props) => {
    switch (props.variant) {
      case 'primary':
        return 'rgba(30, 64, 175, 0.1)'
      case 'success':
        return 'rgba(16, 185, 129, 0.1)'
      case 'danger':
        return 'rgba(220, 38, 38, 0.1)'
      case 'warning':
        return 'rgba(245, 158, 11, 0.1)'
      default:
        return 'rgba(107, 114, 128, 0.1)'
    }
  }};
  color: ${(props) => {
    switch (props.variant) {
      case 'primary':
        return COLORS.primary
      case 'success':
        return COLORS.success
      case 'danger':
        return COLORS.danger
      case 'warning':
        return COLORS.warning
      default:
        return COLORS.textSecondary
    }
  }};
`
