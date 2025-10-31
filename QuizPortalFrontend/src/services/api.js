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
      console.log("++++++++++++")
      console.log(response.data)
      return response.data
    } catch (error) {
      console.log("Error occured!")
      throw error.response?.data || error.message
    }
  },

  getQuestionResponses: async (examId, questionId, page = 1, pageSize = 10) => {
    try {
      const response = await api.get(`/teacher/exams/${examId}/responses/questions/${questionId}/all`, {
        params: { page, pageSize },
      })
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Statistics
  getExamStatistics: async (examId) => {
    try {
      const response = await api.get(`/teacher/exams/${examId}/responses/statistics`)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  getQuestionStatistics: async (examId, questionId) => {
    try {
      const response = await api.get(`/teacher/exams/${examId}/responses/questions/${questionId}/statistics`)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

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
      // console.log("~~~~~~~~~~~~~~~~~~~")
      // console.log(response.data)
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

  getStudentGradedResponses: async (examId, studentId) => {
    try {
      const response = await api.get(`/teacher/grading/exams/${examId}/students/${studentId}/graded-responses`)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

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
  validateAccess: async (accessCode, accessPassword) => {
    try {
      const response = await api.post('/exams/validate-access', {
        accessCode,
        accessPassword,
      })
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Get exam with all questions
  getExamWithQuestions: async (examId) => {
    try {
      const response = await api.get(`/exams/${examId}/student-view`)
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

  // Create user
  createUser: async (data) => {
    try {
      const response = await api.post('/admin/users', data)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Update user
  updateUser: async (userId, data) => {
    try {
      const response = await api.put(`/admin/users/${userId}`, data)
      return response.data
    } catch (error) {
      throw error.response?.data || error.message
    }
  },

  // Delete user
  deleteUser: async (userId) => {
    try {
      const response = await api.delete(`/admin/users/${userId}`)
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
}

export default api
