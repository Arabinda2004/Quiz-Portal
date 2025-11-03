import { useState } from 'react'
import {
  PageHeader,
  Card,
  FormGroup,
  FormRow,
  PrimaryButton,
  SecondaryButton,
  Alert,
  Tabs,
  Tab,
  TabContent,
  StatCard,
} from '../../styles/AdminStyles'

export default function SystemSettings() {
  const [generalSettings, setGeneralSettings] = useState({
    systemName: 'Quiz Portal',
    supportEmail: 'support@quizportal.com',
    maintenanceMode: false,
    maxUploadSize: 10,
  })

  const [examSettings, setExamSettings] = useState({
    defaultDuration: 60,
    allowLateSubmission: true,
    lateSubmissionMinutes: 5,
    shuffleQuestions: false,
    showCorrectAnswers: true,
  })

  const [securitySettings, setSecuritySettings] = useState({
    enableTwoFactor: false,
    passwordMinLength: 8,
    passwordExpireDays: 90,
    sessionTimeout: 30,
  })

  const [activeTab, setActiveTab] = useState('general')
  const [success, setSuccess] = useState('')
  const [saving, setSaving] = useState(false)

  const handleGeneralChange = e => {
    const { name, value, type, checked } = e.target
    setGeneralSettings(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }))
  }

  const handleExamChange = e => {
    const { name, value, type, checked } = e.target
    setExamSettings(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }))
  }

  const handleSecurityChange = e => {
    const { name, value, type, checked } = e.target
    setSecuritySettings(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }))
  }

  const handleSave = async section => {
    setSaving(true)
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 500))
    setSuccess(`${section} settings saved successfully!`)
    setSaving(false)
    setTimeout(() => setSuccess(''), 3000)
  }

  return (
    <>
      {success && (
        <Alert type="success">
          {success}
          <button onClick={() => setSuccess('')}>×</button>
        </Alert>
      )}

      <PageHeader>
        <div>
          <h1>System Settings</h1>
          <p>Configure system-wide settings and preferences</p>
        </div>
      </PageHeader>

      <Tabs>
        <Tab active={activeTab === 'general'} onClick={() => setActiveTab('general')}>
          General
        </Tab>
        <Tab active={activeTab === 'exam'} onClick={() => setActiveTab('exam')}>
          Exam Settings
        </Tab>
        <Tab active={activeTab === 'security'} onClick={() => setActiveTab('security')}>
          Security
        </Tab>
      </Tabs>

      <TabContent active={activeTab === 'general'}>
        <Card>
          <h2 style={{ marginTop: 0 }}>General Settings</h2>

          <FormGroup>
            <label htmlFor="systemName">System Name</label>
            <input
              type="text"
              id="systemName"
              name="systemName"
              value={generalSettings.systemName}
              onChange={handleGeneralChange}
              disabled={saving}
            />
          </FormGroup>

          <FormGroup>
            <label htmlFor="supportEmail">Support Email</label>
            <input
              type="email"
              id="supportEmail"
              name="supportEmail"
              value={generalSettings.supportEmail}
              onChange={handleGeneralChange}
              disabled={saving}
            />
          </FormGroup>

          <FormGroup>
            <label htmlFor="maxUploadSize">Maximum Upload Size (MB)</label>
            <input
              type="number"
              id="maxUploadSize"
              name="maxUploadSize"
              value={generalSettings.maxUploadSize}
              onChange={handleGeneralChange}
              min="1"
              disabled={saving}
            />
          </FormGroup>

          <FormGroup>
            <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginTop: '10px' }}>
              <input
                type="checkbox"
                id="maintenanceMode"
                name="maintenanceMode"
                checked={generalSettings.maintenanceMode}
                onChange={handleGeneralChange}
                disabled={saving}
                style={{ width: 'auto' }}
              />
              <label htmlFor="maintenanceMode" style={{ marginBottom: 0 }}>
                Enable Maintenance Mode
              </label>
            </div>
            <small style={{ color: '#6b7280', marginTop: '6px', display: 'block' }}>
              When enabled, only admins can access the system
            </small>
          </FormGroup>

          <div style={{ display: 'flex', gap: '12px', marginTop: '30px' }}>
            <PrimaryButton onClick={() => handleSave('General')} disabled={saving}>
              Save Settings
            </PrimaryButton>
          </div>
        </Card>
      </TabContent>

      <TabContent active={activeTab === 'exam'}>
        <Card>
          <h2 style={{ marginTop: 0 }}>Exam Settings</h2>

          <FormGroup>
            <label htmlFor="defaultDuration">Default Exam Duration (minutes)</label>
            <input
              type="number"
              id="defaultDuration"
              name="defaultDuration"
              value={examSettings.defaultDuration}
              onChange={handleExamChange}
              min="1"
              disabled={saving}
            />
          </FormGroup>

          <FormGroup>
            <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginTop: '10px' }}>
              <input
                type="checkbox"
                id="allowLateSubmission"
                name="allowLateSubmission"
                checked={examSettings.allowLateSubmission}
                onChange={handleExamChange}
                disabled={saving}
                style={{ width: 'auto' }}
              />
              <label htmlFor="allowLateSubmission" style={{ marginBottom: 0 }}>
                Allow Late Submission
              </label>
            </div>
          </FormGroup>

          {examSettings.allowLateSubmission && (
            <FormGroup>
              <label htmlFor="lateSubmissionMinutes">Grace Period (minutes)</label>
              <input
                type="number"
                id="lateSubmissionMinutes"
                name="lateSubmissionMinutes"
                value={examSettings.lateSubmissionMinutes}
                onChange={handleExamChange}
                min="0"
                disabled={saving}
              />
            </FormGroup>
          )}

          <FormGroup>
            <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginTop: '10px' }}>
              <input
                type="checkbox"
                id="shuffleQuestions"
                name="shuffleQuestions"
                checked={examSettings.shuffleQuestions}
                onChange={handleExamChange}
                disabled={saving}
                style={{ width: 'auto' }}
              />
              <label htmlFor="shuffleQuestions" style={{ marginBottom: 0 }}>
                Shuffle Questions
              </label>
            </div>
            <small style={{ color: '#6b7280', marginTop: '6px', display: 'block' }}>
              Questions will appear in random order for each student
            </small>
          </FormGroup>

          <FormGroup>
            <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginTop: '10px' }}>
              <input
                type="checkbox"
                id="showCorrectAnswers"
                name="showCorrectAnswers"
                checked={examSettings.showCorrectAnswers}
                onChange={handleExamChange}
                disabled={saving}
                style={{ width: 'auto' }}
              />
              <label htmlFor="showCorrectAnswers" style={{ marginBottom: 0 }}>
                Show Correct Answers to Students
              </label>
            </div>
          </FormGroup>

          <div style={{ display: 'flex', gap: '12px', marginTop: '30px' }}>
            <PrimaryButton onClick={() => handleSave('Exam')} disabled={saving}>
              Save Settings
            </PrimaryButton>
          </div>
        </Card>
      </TabContent>

      <TabContent active={activeTab === 'security'}>
        <Card>
          <h2 style={{ marginTop: 0 }}>Security Settings</h2>

          <FormGroup>
            <label htmlFor="passwordMinLength">Minimum Password Length</label>
            <input
              type="number"
              id="passwordMinLength"
              name="passwordMinLength"
              value={securitySettings.passwordMinLength}
              onChange={handleSecurityChange}
              min="6"
              disabled={saving}
            />
          </FormGroup>

          <FormGroup>
            <label htmlFor="passwordExpireDays">Password Expiration (days)</label>
            <input
              type="number"
              id="passwordExpireDays"
              name="passwordExpireDays"
              value={securitySettings.passwordExpireDays}
              onChange={handleSecurityChange}
              min="0"
              disabled={saving}
            />
            <small style={{ color: '#6b7280', marginTop: '6px', display: 'block' }}>
              Set to 0 to disable password expiration
            </small>
          </FormGroup>

          <FormGroup>
            <label htmlFor="sessionTimeout">Session Timeout (minutes)</label>
            <input
              type="number"
              id="sessionTimeout"
              name="sessionTimeout"
              value={securitySettings.sessionTimeout}
              onChange={handleSecurityChange}
              min="5"
              disabled={saving}
            />
          </FormGroup>

          <FormGroup>
            <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginTop: '10px' }}>
              <input
                type="checkbox"
                id="enableTwoFactor"
                name="enableTwoFactor"
                checked={securitySettings.enableTwoFactor}
                onChange={handleSecurityChange}
                disabled={saving}
                style={{ width: 'auto' }}
              />
              <label htmlFor="enableTwoFactor" style={{ marginBottom: 0 }}>
                Enable Two-Factor Authentication
              </label>
            </div>
          </FormGroup>

          <div style={{ display: 'flex', gap: '12px', marginTop: '30px' }}>
            <PrimaryButton onClick={() => handleSave('Security')} disabled={saving}>
              Save Settings
            </PrimaryButton>
          </div>
        </Card>
      </TabContent>
    </>
  )
}
