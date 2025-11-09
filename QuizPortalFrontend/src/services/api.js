import axios from 'axios'

const API_BASE_URL = 'http://localhost:5242/api'

const api = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true,
})

// Auth APIs
export const authService = {
  register: async (data) => {
    try {
      const response = await api.post('/auth/register', data)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  login: async (email, password) => {
    try {
      const response = await api.post('/auth/login', { email, password })
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  logout: async () => {
    try {
      const response = await api.post('/auth/logout')
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },
}

// User APIs
export const userService = {
  getProfile: async () => {
    try {
      const response = await api.get('/users/profile')
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  updateProfile: async (data) => {
    try {
      const response = await api.put('/users/profile', data)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  changePassword: async (currentPassword, newPassword) => {
    try {
      const response = await api.put('/users/change-password', {
        currentPassword,
        newPassword,
      })
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },
}

// Teacher APIs
export const teacherService = {
  // Exam Management
  createExam: async (data) => {
    try {
      // console.log("Here")
      const response = await api.post('/exams', data)
      console.log(response.data) // ! 
      return response.data
    } catch (error) {
      console.log("Error occured!")
      throw error.response?.data || error.message
    }
  },

  getExams: async () => {
    try {
      const response = await api.get('/exams')
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  getExamById: async (examId) => {
    try {
      const response = await api.get(`/exams/${examId}`)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  updateExam: async (examId, data) => {
    try {
      const response = await api.put(`/exams/${examId}`, data)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  deleteExam: async (examId) => {
    try {
      const response = await api.delete(`/exams/${examId}`)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Question Management
  createQuestion: async (examId, data) => {
    try {
      const response = await api.post(`/exams/${examId}/questions`, data)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  getQuestions: async (examId) => {
    try {
      const response = await api.get(`/exams/${examId}/questions`)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  getQuestionById: async (examId, questionId) => {
    try {
      const response = await api.get(`/exams/${examId}/questions/${questionId}`)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  updateQuestion: async (examId, questionId, data) => {
    try {
      const response = await api.put(`/exams/${examId}/questions/${questionId}`, data)
      // console.log(response.data)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  deleteQuestion: async (examId, questionId) => {
    try {
      const response = await api.delete(`/exams/${examId}/questions/${questionId}`)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Student Responses & Attempts (View all responses)
  getStudentAttempts: async (examId, page = 1, pageSize = 10) => {
    try {
      const response = await api.get(`/teacher/exams/${examId}/responses/students`, {
        params: { page, pageSize },
      })
      console.log("========================")
      console.log(response.data)
      console.log("========================")
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Get all responses submitted by a student (for viewing/reviewing)
  getStudentResponses: async (examId, studentId) => {
    try {
      const response = await api.get(`/teacher/exams/${examId}/responses/students/${studentId}`)
      // console.log("++++++++++++")
      console.log(response.data)
      return response.data
    } catch (error) {
      console.log("Error occured!")
      throw error.response?.data || error.message
    }
  },

  // getQuestionResponses: async (examId, questionId, page = 1, pageSize = 10) => {
  //   try {
  //     const response = await api.get(`/teacher/exams/${examId}/responses/questions/${questionId}/all`, {
  //       params: { page, pageSize },
  //     })
  //     return response.data
  //   } catch (error) {
  //     throw error.response?.data || error.message
  //   }
  // },

  // Statistics
  getExamStatistics: async (examId) => {
    try {
      const response = await api.get(`/teacher/exams/${examId}/responses/statistics`)
      console.log(`Exam Statistics Response for examId ${examId}:`, response.data)
      return response.data
    } catch (error) {
      console.error(`Error fetching exam statistics for examId ${examId}:`, error)
      throw error.response?.data || error.message
    }
  },

  // getQuestionStatistics: async (examId, questionId) => {
  //   try {
  //     const response = await api.get(`/teacher/exams/${examId}/responses/questions/${questionId}/statistics`)
  //     console.log(`Question Statistics Response for questionId ${questionId}:`, response.data)
  //     return response.data
  //   } catch (error) {
  //     console.error(`Error fetching question statistics:`, error)
  //     throw error.response?.data || error.message
  //   }
  // },

  // Grading Methods (for grading pending responses)
  getPendingResponses: async (examId, page = 1, pageSize = 10) => {
    try {
      const response = await api.get(`/teacher/grading/exams/${examId}/pending`, {
        params: { page, pageSize },
      })
      console.log("+++++++++++")
      console.log(response.data)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  getPendingResponsesByQuestion: async (examId, questionId, page = 1, pageSize = 10) => {
    try {
      const response = await api.get(`/teacher/grading/exams/${examId}/questions/${questionId}/pending`, {
        params: { page, pageSize },
      })
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Get pending responses for a specific student that need grading
  getPendingResponsesByStudent: async (examId, studentId, page = 1, pageSize = 10) => {
    try {
      const response = await api.get(`/teacher/grading/exams/${examId}/students/${studentId}/pending`, {
        params: { page, pageSize },
      })
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  getResponseForGrading: async (responseId) => {
    try {
      const response = await api.get(`/teacher/grading/responses/${responseId}`)
      console.log("Response data: ", response)
      return response.data
    } catch (error) {
      console.log("Error occured!")
      throw error.response?.data || error.message
    }
  },

  gradeSingleResponse: async (responseId, data) => {
    try {
      const response = await api.post(`/teacher/grading/responses/${responseId}/grade`, data)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  gradeBatchResponses: async (data) => {
    try {
      const response = await api.post(`/teacher/grading/batch-grade`, data)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  getGradingHistory: async (responseId) => {
    try {
      const response = await api.get(`/teacher/grading/responses/${responseId}/history`)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  getGradingStatistics: async (examId) => {
    try {
      const response = await api.get(`/teacher/grading/exams/${examId}/statistics`)
      console.log("Response: ")
      console.log(response.data)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // getStudentGradedResponses: async (examId, studentId) => {
  //   try {
  //     const response = await api.get(`/teacher/grading/exams/${examId}/students/${studentId}/graded-responses`)
  //     return response.data
  //   } catch (error) {
  //     throw error.response?.data || error.message
  //   }
  // },

  regradeResponse: async (responseId, data) => {
    try {
      const response = await api.post(`/teacher/grading/responses/${responseId}/regrade`, data)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },
}

// Student APIs
export const studentService = {
  // Validate exam access
  validateAccess: async (accessCode) => {
    try {
      const response = await api.post('/exams/validate-access', {
        accessCode
      })
      console.log("Access data: ")
      console.log(response.data)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Get exam with all questions
  getExamWithQuestions: async (examId) => {
    try {
      const response = await api.get(`/exams/${examId}/student-view`)
      console.log("Student view response: ")
      console.log(response.data)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Submit answer
  submitAnswer: async (examId, data) => {
    try {
      console.log(data)
      console.log("=============")
      const response = await api.post(`/exams/${examId}/responses`, data)
      console.log(response.data)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Finalize exam submission
  finalizeExamSubmission: async (examId) => {
    try {
      const response = await api.post(`/exams/${examId}/responses/submit`)
      return response.data
    } catch (error) {
      console.error('API Error in finalizeExamSubmission:', error)
      const errorData = error.response?.data
      throw errorData?.message || errorData || error.message || 'Failed to finalize exam submission'
    }
  },

  // Get exam responses
  getExamResponses: async (examId) => {
    try {
      const response = await api.get(`/exams/${examId}/responses`)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Get submission status
  getSubmissionStatus: async (examId) => {
    try {
      const response = await api.get(`/exams/${examId}/responses/status`)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },
}

// Admin APIs
export const adminService = {
  // Get all users
  getAllUsers: async () => {
    try {
      const response = await api.get('/admin/users')
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Get user by ID
  getUserById: async (userId) => {
    try {
      const response = await api.get(`/admin/users/${userId}`)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Get all exams
  getAllExams: async () => {
    try {
      const response = await api.get('/exams/all')
      console.log("Getting all exams..")
      console.log(response.data)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Get exam by ID (admin-accessible endpoint)
  getExamById: async (examId) => {
    try {
      const response = await api.get(`/admin/exams/${examId}`)
      console.log(`Getting exam ${examId} by ID...`)
      console.log(response.data)
      return response.data
    } catch (error) {
      console.error(`Error getting exam ${examId}:`, error)
      throw error.response?.data || error.message
    }
  },

  // Get exam statistics (admin-accessible endpoint)
  getExamStatistics: async (examId) => {
    try {
      const response = await api.get(`/admin/exams/${examId}/responses/statistics`)
      console.log(`Getting exam statistics for examId ${examId}:`, response.data)
      return response.data
    } catch (error) {
      console.error(`Error getting exam statistics for ${examId}:`, error)
      throw error.response?.data || error.message
    }
  },

  // Get all student attempts for an exam (admin-accessible endpoint)
  getStudentAttempts: async (examId) => {
    try {
      const response = await api.get(`/admin/exams/${examId}/responses/students`)
      console.log(`Getting student attempts for examId ${examId}:`, response.data)
      return response.data
    } catch (error) {
      console.error(`Error getting student attempts for ${examId}:`, error)
      throw error.response?.data || error.message
    }
  },

  // Get exam questions (admin-accessible endpoint)
  getExamQuestions: async (examId) => {
    try {
      const response = await api.get(`/admin/exams/${examId}/questions`)
      console.log(`Getting exam questions for examId ${examId}:`, response.data)
      return response.data
    } catch (error) {
      console.error(`Error getting exam questions for ${examId}:`, error)
      throw error.response?.data || error.message
    }
  },
}

// Result Publishing APIs
export const resultService = {
  // Get publication status for an exam
  getPublicationStatus: async (examId) => {
    try {
      const response = await api.get(`/results/exams/${examId}/publication-status`)
      console.log("Publication Status Response:", response.data)
      // Backend returns { success: true, data: { ... } }
      return response.data.data || response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Get grading progress for an exam
  getGradingProgress: async (examId) => {
    try {
      const response = await api.get(`/teacher/grading/exams/${examId}/pending`, {
        params: { page: 1, pageSize: 10 }
      })
      console.log("Grading Progress Response:", response.data)
      
      // Transform the response to match expected structure
      const data = response.data.data
      if (data) {
        // Count unique students from responses
        const uniqueStudents = new Set(data.responses?.map(r => r.studentId) || [])
        const totalStudents = uniqueStudents.size
        
        // Count students who have all questions graded
        const studentGradingStatus = {}
        data.responses?.forEach(r => {
          if (!studentGradingStatus[r.studentId]) {
            studentGradingStatus[r.studentId] = {
              pending: 0,
              graded: 0
            }
          }
          if (r.isGraded) {
            studentGradingStatus[r.studentId].graded++
          } else {
            studentGradingStatus[r.studentId].pending++
          }
        })
        
        // Count students who have all questions graded (no pending questions)
        const gradedStudents = Object.values(studentGradingStatus).filter(
          status => status.pending === 0
        ).length
        
        const pendingStudents = totalStudents - gradedStudents
        const gradingProgress = totalStudents > 0 ? (gradedStudents / totalStudents) * 100 : 0
        
        return {
          totalStudents,
          gradedStudents,
          pendingStudents,
          gradingProgress: Math.round(gradingProgress * 100) / 100,
          totalResponses: data.totalResponses,
          totalPending: data.totalPending
        }
      }
      
      return {
        totalStudents: 0,
        gradedStudents: 0,
        pendingStudents: 0,
        gradingProgress: 0
      }
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Get all results for an exam
  getExamResults: async (examId, page = 1, pageSize = 10) => {
    try {
      const response = await api.get(`/results/exams/${examId}/all-results`, {
        params: { page, pageSize },
      })
      console.log("Exam result: ")
      console.log(response.data)
      console.log("All Results Response:", response.data)
      return response.data.data || []
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // // Get result statistics for an exam
  // getResultStatistics: async (examId) => {
  //   try {
  //     const response = await api.get(`/results/exams/${examId}/statistics`)
  //     console.log("Statistics Response:", response.data)
  //     return response.data.data
  //   } catch (error) {
  //     throw error.response?.data || error.message
  //   }
  // },

  // // Get result summary for an exam
  // getResultSummary: async (examId) => {
  //   try {
  //     const response = await api.get(`/results/exams/${examId}/summary`)
  //     console.log("Summary Response:", response.data)
  //     return response.data.data
  //   } catch (error) {
  //     throw error.response?.data || error.message
  //   }
  // },

  // // Get pass/fail breakdown for an exam
  // getPassFailBreakdown: async (examId) => {
  //   try {
  //     const response = await api.get(`/results/exams/${examId}/pass-fail`)
  //     console.log("Pass/Fail Response:", response.data)
  //     return response.data.data
  //   } catch (error) {
  //     throw error.response?.data || error.message
  //   }
  // },

  // Publish exam results
  publishExam: async (examId, data) => {
    try {
      const response = await api.post(`/results/exams/${examId}/publish`, data)
      console.log("Publish Response:", response.data)
      return response.data.data || response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Unpublish exam results
  unpublishExam: async (examId, reason) => {
    try {
      const response = await api.post(`/results/exams/${examId}/unpublish`, { reason })
      console.log("Unpublish Response:", response.data)
      return response.data.data || response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Get published results for logged-in student
  getPublishedResults: async () => {
    try {
      const response = await api.get(`/results/published`)
      console.log("Published Results Response:", response.data)
      return response.data.data || []
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Get specific exam result for logged-in student
  // getMyExamResult: async (examId) => {
  //   try {
  //     const response = await api.get(`/results/exams/${examId}`)
  //     console.log("My Exam Result Response:", response.data)
  //     return response.data.data
  //   } catch (error) {
  //     throw error.response?.data || error.message
  //   }
  // },

  // Get exam result details for logged-in student
  getExamResultDetails: async (examId) => {
    try {
      const response = await api.get(`/results/exams/${examId}/details`)
      console.log("Exam Result Details Response:", response.data)
      return response.data.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // // Get all student results (for student dashboard)
  // getMyResults: async (page = 1, pageSize = 10) => {
  //   try {
  //     const response = await api.get(`/results/my-results`, {
  //       params: { page, pageSize },
  //     })
  //     console.log("My Results Response:", response.data.data)
  //     return response.data.data || []
  //   } catch (error) {
  //     throw error.response?.data || error.message
  //   }
  // },

  getMyCompletedExams: async (page = 1, pageSize = 100) => {
    try {
      const response = await api.get(`/results/my-completed-exams`, {
        params: { page, pageSize },
      })
      return response.data.data || []
    } catch (error) {
      throw error.response?.data || error.message
    }
  },
}

export default api
