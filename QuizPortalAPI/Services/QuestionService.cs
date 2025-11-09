using QuizPortalAPI.DAL.ExamRepo;
using QuizPortalAPI.DAL.QuestionOptionRepo;
using QuizPortalAPI.DAL.QuestionRepo;
using QuizPortalAPI.DTOs.Question;
using QuizPortalAPI.Models;

namespace QuizPortalAPI.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly ILogger<QuestionService> _logger;
        private readonly IExamRepository _examRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IQuestionOptionRepository _questionOptionRepository;

        public QuestionService(ILogger<QuestionService> logger, IExamRepository examRepository, IQuestionRepository questionRepository, IQuestionOptionRepository questionOptionRepository)
        {
            _logger = logger;
            _examRepository = examRepository;
            _questionRepository = questionRepository;
            _questionOptionRepository = questionOptionRepository;
        }

        /// <summary>
        /// Teacher creates a new question for an exam
        /// </summary>
        public async Task<QuestionResponseDTO> CreateQuestionAsync(int examId, int teacherId, CreateQuestionDTO createQuestionDTO)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createQuestionDTO.QuestionText))
                    throw new InvalidOperationException("Question text is required");

                var exam = await _examRepository.FindExamByIdAsync(examId);
                if (exam == null)
                    throw new InvalidOperationException("Exam not found");

                if (exam.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to add question to exam {examId} they don't own");
                    throw new UnauthorizedAccessException("You can only add questions to your own exams");
                }

                var now = DateTime.UtcNow;
                if (now >= exam.ScheduleStart)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to add question to exam {examId} that has already started");
                    throw new InvalidOperationException("Questions can only be added to upcoming exams. Once an exam starts, no new questions can be added.");
                }

                if (createQuestionDTO.Marks <= 0)
                    throw new InvalidOperationException("Marks must be greater than 0");

                if (createQuestionDTO.QuestionType == QuestionType.MCQ)
                {
                    if (createQuestionDTO.Options == null || createQuestionDTO.Options.Count != 4)
                        throw new InvalidOperationException("MCQ questions must have exactly 4 options");

                    var correctOptionsCount = createQuestionDTO.Options.Count(o => o.IsCorrect);
                    if (correctOptionsCount != 1)
                        throw new InvalidOperationException("MCQ questions must have exactly 1 correct option");
                }
                else if (createQuestionDTO.Options != null && createQuestionDTO.Options.Count > 0)
                {
                    throw new InvalidOperationException($"Only MCQ questions can have options");
                }

                var question = new Question
                {
                    ExamID = examId,
                    CreatedBy = teacherId,
                    QuestionText = createQuestionDTO.QuestionText,
                    QuestionType = createQuestionDTO.QuestionType,
                    Marks = createQuestionDTO.Marks,
                    CreatedAt = DateTime.UtcNow
                };

                if (createQuestionDTO.QuestionType == QuestionType.MCQ && createQuestionDTO.Options != null)
                {
                    question.Options = createQuestionDTO.Options.Select(opt => new QuestionOption
                    {
                        OptionText = opt.OptionText,
                        IsCorrect = opt.IsCorrect,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();
                }

                await _questionRepository.AddAsync(question);

                _logger.LogInformation($"Question {question.QuestionID} created for exam {examId} by teacher {teacherId}");
                return MapToQuestionResponseDTO(question);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized access: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating question: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get question details by ID
        /// </summary>
        public async Task<QuestionResponseDTO?> GetQuestionByIdAsync(int questionId)
        {
            try
            {
                var question = await _questionRepository.GetQuestionsWithOptionsbyIdAsync(questionId);

                if (question == null)
                {
                    _logger.LogWarning($"Question {questionId} not found");
                    return null;
                }

                return MapToQuestionResponseDTO(question);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving question {questionId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all questions for an exam
        /// </summary>
        public async Task<IEnumerable<QuestionListDTO>> GetExamQuestionsAsync(int examId)
        {
            try
            {
                var questions = await _questionRepository.GetQuestionsWithOptionsForAnExamByIdAsync(examId);

                _logger.LogInformation($"Retrieved {questions.Count} questions for exam {examId}");
                return questions.Select(q => MapToQuestionListDTO(q)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving exam questions: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Teacher updates a question
        /// </summary>
        public async Task<QuestionResponseDTO?> UpdateQuestionAsync(int questionId, int teacherId, UpdateQuestionDTO updateQuestionDTO)
        {
            try
            {
                if (updateQuestionDTO == null)
                    throw new ArgumentNullException(nameof(updateQuestionDTO));

                // Get question with options
                var question = await _questionRepository.GetQuestionsWithOptionsbyIdAsync(questionId);

                if (question == null)
                {
                    _logger.LogWarning($"Question {questionId} not found");
                    return null;
                }

                // Verify ownership
                if (question.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to update question {questionId} they don't own");
                    throw new UnauthorizedAccessException("You can only update your own questions");
                }

                var exam = await _examRepository.FindExamByIdAsync(question.ExamID);
                if (exam != null)
                {
                    var now = DateTime.UtcNow;
                    if (now >= exam.ScheduleStart)
                    {
                        _logger.LogWarning($"Teacher {teacherId} attempted to update question in exam {exam.ExamID} that has already started");
                        throw new InvalidOperationException("Questions can only be updated in upcoming exams. Once an exam starts, questions cannot be modified.");
                    }
                }

                // Update text if provided
                if (!string.IsNullOrWhiteSpace(updateQuestionDTO.QuestionText))
                {
                    if (updateQuestionDTO.QuestionText.Length < 10 || updateQuestionDTO.QuestionText.Length > 5000)
                        throw new InvalidOperationException("Question text must be between 10 and 5000 characters");
                    question.QuestionText = updateQuestionDTO.QuestionText;
                }

                // Update marks if provided
                if (updateQuestionDTO.Marks.HasValue)
                {
                    if (updateQuestionDTO.Marks <= 0)
                        throw new InvalidOperationException("Marks must be greater than 0");
                    
                    // Note: No validation for total marks limit since TotalMarks is now dynamically calculated from questions
                    
                    question.Marks = updateQuestionDTO.Marks.Value;
                }

                // Update options if provided
                if (updateQuestionDTO.Options != null && updateQuestionDTO.Options.Count > 0)
                {
                    if (question.QuestionType != QuestionType.MCQ)
                        throw new InvalidOperationException("Only MCQ questions can have options");


                    var currentOptions = question.Options ?? new List<QuestionOption>();

                    // Remove options that are not in the update list
                    var optionIdsToKeep = updateQuestionDTO.Options
                        .Where(o => o.OptionID.HasValue)
                        .Select(o => o.OptionID!.Value)
                        .ToList();

                    var optionsToRemove = currentOptions
                        .Where(o => !optionIdsToKeep.Contains(o.OptionID))
                        .ToList();

                    foreach (var option in optionsToRemove)
                    {
                        await _questionOptionRepository.RemoveAsync(option);
                    }

                    // Update existing options
                    foreach (var updateOption in updateQuestionDTO.Options.Where(o => o.OptionID.HasValue))
                    {
                        var existingOption = currentOptions.FirstOrDefault(o => o.OptionID == updateOption.OptionID);
                        if (existingOption != null)
                        {
                            if (!string.IsNullOrWhiteSpace(updateOption.OptionText))
                                existingOption.OptionText = updateOption.OptionText;
                            if (updateOption.IsCorrect.HasValue)
                                existingOption.IsCorrect = updateOption.IsCorrect.Value;
                        }
                    }

                    // Add new options
                    foreach (var newOption in updateQuestionDTO.Options.Where(o => !o.OptionID.HasValue))
                    {
                        // FIX: Initialize Options list if null
                        if (question.Options == null)
                            question.Options = new List<QuestionOption>();

                        question.Options.Add(new QuestionOption
                        {
                            OptionText = newOption.OptionText ?? string.Empty,
                            IsCorrect = newOption.IsCorrect ?? false,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    // NEW: Validate exactly 4 options for MCQ
                    if (question.QuestionType == QuestionType.MCQ)
                    {
                        var totalOptions = question.Options?.Count ?? 0;
                        if (totalOptions != 4)
                            throw new InvalidOperationException("MCQ questions must have exactly 4 options");
                    }

                    // Validate exactly 1 correct option
                    var correctCount = (question.Options ?? new List<QuestionOption>()).Count(o => o.IsCorrect);
                    if (correctCount != 1)
                        throw new InvalidOperationException("MCQ questions must have exactly 1 correct option");
                }

                // Save changes
                await _questionRepository.UpdateAsync(question);

                _logger.LogInformation($"Question {questionId} updated by teacher {teacherId}");
                return MapToQuestionResponseDTO(question);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError($"Validation error: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized access: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating question {questionId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Teacher deletes a question
        /// </summary>
        public async Task<bool> DeleteQuestionAsync(int questionId, int teacherId)
        {
            try
            {
                var question = await _questionRepository.FindQuestionWithIdAsync(questionId);
                if (question == null)
                {
                    _logger.LogWarning($"Question {questionId} not found");
                    return false;
                }

                // Verify ownership
                if (question.CreatedBy != teacherId)
                {
                    _logger.LogWarning($"Teacher {teacherId} attempted to delete question {questionId} they don't own");
                    throw new UnauthorizedAccessException("You can only delete your own questions");
                }

                // Check if exam has started (can only delete questions from upcoming exams)
                var exam = await _examRepository.FindExamByIdAsync(question.ExamID);
                if (exam != null)
                {
                    var now = DateTime.UtcNow;
                    if (now >= exam.ScheduleStart)
                    {
                        _logger.LogWarning($"Teacher {teacherId} attempted to delete question from exam {exam.ExamID} that has already started");
                        throw new InvalidOperationException("Questions can only be deleted from upcoming exams. Once an exam starts, questions cannot be removed.");
                    }
                }

                await _questionRepository.DeleteAsync(question);

                _logger.LogInformation($"Question {questionId} deleted by teacher {teacherId}");
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized deletion: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting question {questionId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if teacher owns the question
        /// </summary>
        public async Task<bool> IsTeacherQuestionOwnerAsync(int questionId, int teacherId)
        {
            try
            {
                var question = await _questionRepository.FindQuestionWithIdAsync(questionId);
                return question != null && question.CreatedBy == teacherId;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking question ownership: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if teacher owns the exam
        /// </summary>
        public async Task<bool> IsExamOwnerAsync(int examId, int teacherId)
        {
            try
            {
                var exam = await _examRepository.FindExamByIdAsync(examId);
                return exam != null && exam.CreatedBy == teacherId;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking exam ownership: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get total question count for an exam
        /// </summary>
        public async Task<int> GetExamQuestionCountAsync(int examId)
        {
            try
            {
                return await _questionRepository.GetExamQuestionCountByExamIdAsync(examId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting question count: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Get total marks for an exam
        /// </summary>
        public async Task<decimal> GetExamTotalMarksAsync(int examId)
        {
            try
            {
                var totalMarks = await _examRepository.GetExamTotalMarksByExamIdAsync(examId);

                return totalMarks;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting exam total marks: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// NEW: Check if exam is completed
        /// </summary>
        public async Task<bool> IsExamCompletedAsync(int examId)
        {
            try
            {
                var exam = await _examRepository.FindExamByIdAsync(examId);
                if (exam == null)
                    return false;

                var now = DateTime.UtcNow;
                return now > exam.ScheduleEnd;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking exam completion: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Map Question entity to QuestionResponseDTO
        /// </summary>
        private QuestionResponseDTO MapToQuestionResponseDTO(Question question)
        {
            return new QuestionResponseDTO
            {
                QuestionID = question.QuestionID,
                ExamID = question.ExamID,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType,
                Marks = question.Marks,
                CreatedAt = question.CreatedAt,
                Options = question.Options?.Select(o => new QuestionOptionResponseDTO
                {
                    OptionID = o.OptionID,
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect
                }).ToList() ?? new()
            };
        }

        /// <summary>
        /// Map Question entity to QuestionListDTO
        /// </summary>
        private QuestionListDTO MapToQuestionListDTO(Question question)
        {
            return new QuestionListDTO
            {
                QuestionID = question.QuestionID,
                ExamID = question.ExamID,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType,
                Marks = question.Marks,
                OptionCount = question.Options?.Count ?? 0,
                CreatedAt = question.CreatedAt
            };
        }
    }
}