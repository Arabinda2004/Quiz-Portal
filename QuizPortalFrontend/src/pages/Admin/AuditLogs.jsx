import { useState, useEffect } from 'react'
import {
  PageHeader,
  DataTable,
  Card,
  EmptyState,
  LoadingSpinner,
  SearchBar,
  StatusBadge,
  Pagination,
} from '../../styles/AdminStyles'

export default function AuditLogs() {
  const [logs, setLogs] = useState([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [filterAction, setFilterAction] = useState('All')
  const [currentPage, setCurrentPage] = useState(1)
  const [itemsPerPage] = useState(15)

  useEffect(() => {
    loadLogs()
  }, [])

  const loadLogs = async () => {
    try {
      setLoading(true)
      // Simulate loading audit logs - in production, fetch from API
      const mockLogs = [
        {
          id: 1,
          userId: 1,
          userName: 'Admin User',
          action: 'CREATE_USER',
          resource: 'User',
          resourceId: 5,
          description: 'Created new user: teacher@example.com',
          timestamp: new Date(Date.now() - 3600000),
          status: 'SUCCESS',
        },
        {
          id: 2,
          userId: 1,
          userName: 'Admin User',
          action: 'UPDATE_USER',
          resource: 'User',
          resourceId: 3,
          description: 'Updated user role to Admin',
          timestamp: new Date(Date.now() - 7200000),
          status: 'SUCCESS',
        },
        {
          id: 3,
          userId: 2,
          userName: 'Teacher 1',
          action: 'CREATE_EXAM',
          resource: 'Exam',
          resourceId: 10,
          description: 'Created new exam: Mathematics 101',
          timestamp: new Date(Date.now() - 10800000),
          status: 'SUCCESS',
        },
        {
          id: 4,
          userId: 1,
          userName: 'Admin User',
          action: 'DELETE_USER',
          resource: 'User',
          resourceId: 8,
          description: 'Deleted inactive user account',
          timestamp: new Date(Date.now() - 86400000),
          status: 'SUCCESS',
        },
        {
          id: 5,
          userId: 3,
          userName: 'Student 1',
          action: 'SUBMIT_EXAM',
          resource: 'Exam Submission',
          resourceId: 15,
          description: 'Submitted exam: Physics Final',
          timestamp: new Date(Date.now() - 172800000),
          status: 'SUCCESS',
        },
      ]
      setLogs(mockLogs)
    } catch (err) {
      console.error('Failed to load audit logs:', err)
    } finally {
      setLoading(false)
    }
  }

  const getActionColor = action => {
    if (action.startsWith('CREATE')) return '#10b981'
    if (action.startsWith('UPDATE')) return '#f59e0b'
    if (action.startsWith('DELETE')) return '#ef4444'
    if (action.startsWith('SUBMIT')) return '#3b82f6'
    return '#6b7280'
  }

  const filteredLogs = logs.filter(log => {
    const matchesSearch =
      log.userName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      log.description.toLowerCase().includes(searchTerm.toLowerCase())
    const matchesAction = filterAction === 'All' || log.action.startsWith(filterAction)
    return matchesSearch && matchesAction
  })

  // Paginate
  const totalPages = Math.ceil(filteredLogs.length / itemsPerPage)
  const startIndex = (currentPage - 1) * itemsPerPage
  const endIndex = startIndex + itemsPerPage
  const paginatedLogs = filteredLogs.slice(startIndex, endIndex)

  if (loading) {
    return (
      <Card style={{ textAlign: 'center', padding: '60px 20px' }}>
        <LoadingSpinner />
        <p style={{ marginTop: '20px' }}>Loading audit logs...</p>
      </Card>
    )
  }

  return (
    <>
      <PageHeader>
        <div>
          <h1>Audit Logs</h1>
          <p>System activity and user actions tracking</p>
        </div>
      </PageHeader>

      <Card>
        <SearchBar>
          <input
            type="text"
            placeholder="Search by user or action..."
            value={searchTerm}
            onChange={e => {
              setSearchTerm(e.target.value)
              setCurrentPage(1)
            }}
          />
          <select
            value={filterAction}
            onChange={e => {
              setFilterAction(e.target.value)
              setCurrentPage(1)
            }}
            style={{ padding: '10px', borderRadius: '6px', border: '1px solid #d1d5db', minWidth: '150px' }}
          >
            <option value="All">All Actions</option>
            <option value="CREATE">Create Actions</option>
            <option value="UPDATE">Update Actions</option>
            <option value="DELETE">Delete Actions</option>
            <option value="SUBMIT">Submit Actions</option>
          </select>
        </SearchBar>

        {paginatedLogs.length === 0 ? (
          <EmptyState>
            <h3>No logs found</h3>
            <p>{searchTerm || filterAction !== 'All' ? 'Try adjusting your filters' : 'No activity recorded yet'}</p>
          </EmptyState>
        ) : (
          <>
            <div style={{ overflowX: 'auto' }}>
              <DataTable>
                <thead>
                  <tr>
                    <th>Timestamp</th>
                    <th>User</th>
                    <th>Action</th>
                    <th>Resource</th>
                    <th>Description</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {paginatedLogs.map(log => (
                    <tr key={log.id}>
                      <td>
                        <small>{log.timestamp.toLocaleString()}</small>
                      </td>
                      <td>
                        <strong>{log.userName}</strong>
                      </td>
                      <td>
                        <span
                          style={{
                            display: 'inline-block',
                            padding: '4px 8px',
                            borderRadius: '4px',
                            fontSize: '12px',
                            fontWeight: '600',
                            backgroundColor: getActionColor(log.action) + '20',
                            color: getActionColor(log.action),
                          }}
                        >
                          {log.action}
                        </span>
                      </td>
                      <td>{log.resource}</td>
                      <td>{log.description}</td>
                      <td>
                        <StatusBadge status={log.status}>{log.status}</StatusBadge>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </DataTable>
            </div>

            {totalPages > 1 && (
              <Pagination>
                <button onClick={() => setCurrentPage(1)} disabled={currentPage === 1}>
                  First
                </button>
                <button onClick={() => setCurrentPage(Math.max(1, currentPage - 1))} disabled={currentPage === 1}>
                  Previous
                </button>
                <span>
                  Page {currentPage} of {totalPages}
                </span>
                <button
                  onClick={() => setCurrentPage(Math.min(totalPages, currentPage + 1))}
                  disabled={currentPage === totalPages}
                >
                  Next
                </button>
                <button onClick={() => setCurrentPage(totalPages)} disabled={currentPage === totalPages}>
                  Last
                </button>
              </Pagination>
            )}
          </>
        )}
      </Card>
    </>
  )
}
