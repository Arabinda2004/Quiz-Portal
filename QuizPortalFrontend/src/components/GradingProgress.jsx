import styled from 'styled-components'

const Container = styled.div`
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  padding: 2rem;
  margin-bottom: 1.5rem;
`

const Title = styled.h2`
  margin: 0 0 1.5rem 0;
  font-size: 1.25rem;
  color: #1f2937;
  display: flex;
  align-items: center;
  gap: 0.5rem;
`

const StatsGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
`

const StatCard = styled.div`
  background: linear-gradient(135deg, ${(props) => props.$from} 0%, ${(props) => props.$to} 100%);
  color: white;
  padding: 1.5rem;
  border-radius: 8px;
  text-align: center;
`

const StatLabel = styled.div`
  font-size: 0.875rem;
  opacity: 0.9;
  margin-bottom: 0.5rem;
`

const StatValue = styled.div`
  font-size: 2rem;
  font-weight: bold;
`

const StatSubtitle = styled.div`
  font-size: 0.75rem;
  opacity: 0.85;
  margin-top: 0.5rem;
`

const ProgressBar = styled.div`
  width: 100%;
  height: 8px;
  background-color: #e5e7eb;
  border-radius: 4px;
  overflow: hidden;
  margin-bottom: 0.5rem;
`

const ProgressFill = styled.div`
  height: 100%;
  background-color: #3b82f6;
  width: ${(props) => props.percentage}%;
  transition: width 0.3s ease;
`

const ProgressLabel = styled.div`
  display: flex;
  justify-content: space-between;
  font-size: 0.875rem;
  color: #6b7280;
  margin-bottom: 1rem;
`

const QuestionStatsTable = styled.table`
  width: 100%;
  border-collapse: collapse;
  margin-top: 1rem;

  thead {
    background-color: #f3f4f6;
    border-bottom: 1px solid #e5e7eb;
  }

  th {
    padding: 0.875rem;
    text-align: left;
    font-weight: 600;
    color: #374151;
    font-size: 0.875rem;
  }

  td {
    padding: 0.875rem;
    border-bottom: 1px solid #e5e7eb;
    color: #1f2937;
    font-size: 0.9rem;
  }

  tbody tr:hover {
    background-color: #f9fafb;
  }
`

const Badge = styled.span`
  display: inline-block;
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  background-color: ${(props) => {
    switch (props.$status) {
      case 'complete':
        return '#d1fae5'
      case 'pending':
        return '#fef3c7'
      case 'partial':
        return '#dbeafe'
      default:
        return '#e5e7eb'
    }
  }};
  color: ${(props) => {
    switch (props.$status) {
      case 'complete':
        return '#065f46'
      case 'pending':
        return '#92400e'
      case 'partial':
        return '#1e40af'
      default:
        return '#374151'
    }
  }};
`

export default function GradingProgress({ stats }) {
  console.log("Stats: ")
  console.log(stats)
  if (!stats) {
    return (
      <Container>
        <Title>📊 Grading Progress</Title>
        <div style={{ color: '#6b7280', textAlign: 'center' }}>
          No statistics available
        </div>
      </Container>
    )
  }

  const gradingPercentage = stats.totalResponses > 0
    ? Math.round((stats.gradedResponses / stats.totalResponses) * 100)
    : 0

  const getStatusBadge = (question) => {
    if (question.pendingResponses === 0) {
      return 'complete'
    } else if (question.gradedResponses > 0) {
      return 'partial'
    }
    return 'pending'
  }

  return (
    <Container>
      <Title>📊 Grading Progress</Title>

      <StatsGrid>
        <StatCard $from="#667eea" $to="#764ba2">
          <StatLabel>Total Responses</StatLabel>
          <StatValue>{stats.totalResponses}</StatValue>
        </StatCard>

        <StatCard $from="#f093fb" $to="#f5576c">
          <StatLabel>Graded</StatLabel>
          <StatValue>{stats.gradedResponses}</StatValue>
          <StatSubtitle>✓ Completed</StatSubtitle>
        </StatCard>

        <StatCard $from="#4facfe" $to="#00f2fe">
          <StatLabel>Pending</StatLabel>
          <StatValue>{stats.pendingResponses}</StatValue>
          <StatSubtitle>⏱️ Remaining</StatSubtitle>
        </StatCard>

        <StatCard $from="#43e97b" $to="#38f9d7">
          <StatLabel>Progress</StatLabel>
          <StatValue>{gradingPercentage}%</StatValue>
          <StatSubtitle>Complete</StatSubtitle>
        </StatCard>
      </StatsGrid>

      <div>
        <ProgressLabel>
          <span>Overall Grading Progress</span>
          <span>
            {stats.gradedResponses} / {stats.totalResponses}
          </span>
        </ProgressLabel>
        <ProgressBar>
          <ProgressFill percentage={gradingPercentage} />
        </ProgressBar>
      </div>

      {stats.questionStats && stats.questionStats.length > 0 && (
        <div style={{ marginTop: '2rem' }}>
          <h3 style={{ margin: '0 0 1rem 0', fontSize: '1rem', color: '#374151' }}>
            Question-wise Grading Status
          </h3>
          <QuestionStatsTable>
            <thead>
              <tr>
                <th>Question</th>
                <th>Type</th>
                <th>Total</th>
                <th>Graded</th>
                <th>Pending</th>
                <th>Avg Marks</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {stats.questionStats.map((q) => {
                const questionGradingPercent = q.totalResponses > 0
                  ? Math.round((q.gradedResponses / q.totalResponses) * 100)
                  : 0

                return (
                  <tr key={q.questionID}>
                    <td style={{ maxWidth: '300px' }}>
                      <div style={{ overflow: 'hidden', textOverflow: 'ellipsis' }}>
                        {q.questionText?.substring(0, 50)}...
                      </div>
                    </td>
                    <td>{q.questionType}</td>
                    <td style={{ fontWeight: 600 }}>{q.totalResponses}</td>
                    <td style={{ color: '#16a34a', fontWeight: 600 }}>
                      {q.gradedResponses}
                    </td>
                    <td style={{ color: '#dc2626', fontWeight: 600 }}>
                      {q.pendingResponses}
                    </td>
                    <td>
                      {q.averageMarks.toFixed(1)} / {q.maxMarks}
                    </td>
                    <td>
                      <Badge $status={getStatusBadge(q)}>
                        {questionGradingPercent}%
                      </Badge>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </QuestionStatsTable>
        </div>
      )}
    </Container>
  )
}
