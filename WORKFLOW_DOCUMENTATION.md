# Online Exam Portal - Complete Workflow Documentation

## Overview
This document outlines the complete workflow from creating an exam to publishing results in the Online Exam Portal system. The workflow involves three main actors: **Teachers**, **Students**, and the **System**.

---

## 1. EXAM CREATION PHASE

### 1.1 Teacher Creates Exam
**Role:** Teacher  
**Endpoint:** `POST /api/exams`  
**DTOs Used:** `CreateExamDTO`

#### Steps:
1. Teacher authenticates and receives JWT token with role "Teacher"
2. Teacher submits exam details:
   - **Title**: Name of the exam
   - **Description**: Exam details (optional)
   - **DurationMinutes**: Time allowed to complete exam
   - **ScheduleStart**: When exam becomes available
   - **ScheduleEnd**: When exam stops accepting submissions
   - **PassingPercentage**: Percentage needed to pass (0-100, default 40%)
   - **AccessCode**: Unique code for students to access exam
   - **AccessPassword**: Password for students to validate access
   - **BatchRemark**: Optional note about exam batch

#### Database Changes:
- Creates new `Exam` record with:
  - `ExamID` (auto-generated)
  - `CreatedBy` = Teacher ID
  - `CreatedAt` = Current timestamp
  - `AccessCode` (must be unique)
  - Status initialized

#### Computed Properties:
- `TotalMarks` = Sum of all question marks (calculated from Questions collection)
- `PassingMarks` = (TotalMarks × PassingPercentage) / 100

#### Example Response:
```json
{
  "message": "Exam created successfully",
  "data": {
    "examID": 1,
    "title": "Mathematics Final Exam",
    "durationMinutes": 120,
    "scheduleStart": "2025-11-10T10:00:00Z",
    "scheduleEnd": "2025-11-10T12:00:00Z",
    "passingPercentage": 40,
    "accessCode": "MATH2025",
    "createdBy": 5
  }
}
```

---

### 1.2 Teacher Adds Questions to Exam
**Role:** Teacher  
**Endpoint:** `POST /api/exams/{examId}/questions`  
**DTOs Used:** `CreateQuestionDTO`

#### Steps:
1. Teacher (who created the exam) adds questions one by one
2. For each question, provide:
   - **QuestionText**: The question content (supports text, can include HTML)
   - **QuestionType**: MCQ, SAQ, or Subjective
   - **Marks**: Points awarded for correct answer

#### Question Types:

##### a) MCQ (Multiple Choice Question)
- **Type:** `QuestionType.MCQ` (value: 0)
- **Behavior:** Auto-graded by system
- **Options:** Multiple `QuestionOption` records with:
  - **OptionText**: The option content
  - **IsCorrect**: Boolean flag (only ONE should be true)

##### b) SAQ (Short Answer Question)
- **Type:** `QuestionType.SAQ` (value: 1)
- **Behavior:** Manual grading required by teacher
- **Options:** Can have options or reference answer

##### c) Subjective
- **Type:** `QuestionType.Subjective` (value: 2)
- **Behavior:** Manual grading required by teacher
- **Options:** No options (free-form answer)

#### Database Structure for Questions:

```
Exam (1 → Many)
  ├── Questions (1 → Many)
  │   ├── QuestionID (PK)
  │   ├── ExamID (FK)
  │   ├── CreatedBy (FK to User - Teacher)
  │   ├── QuestionText
  │   ├── QuestionType (enum)
  │   ├── Marks (decimal)
  │   ├── CreatedAt
  │   └── Options (1 → Many)
  │       ├── QuestionOptionID (PK)
  │       ├── QuestionID (FK)
  │       ├── OptionText
  │       ├── IsCorrect (boolean)
  │       └── CreatedAt
```

#### Example Response:
```json
{
  "message": "Question created successfully",
  "data": {
    "questionID": 10,
    "examID": 1,
    "questionText": "What is 2 + 2?",
    "questionType": 0,
    "marks": 5,
    "options": [
      { "optionID": 1, "optionText": "3", "isCorrect": false },
      { "optionID": 2, "optionText": "4", "isCorrect": true },
      { "optionID": 3, "optionText": "5", "isCorrect": false }
    ]
  }
}
```

#### Total Marks Calculation:
As questions are added, the exam's **TotalMarks** is automatically calculated:
```
TotalMarks = SUM(All Question Marks)
PassingMarks = TotalMarks × PassingPercentage / 100
```

---

## 2. EXAM PUBLICATION & STUDENT ACCESS PHASE

### 2.1 Student Validates Access to Exam
**Role:** Any user (anonymous or authenticated)  
**Endpoint:** `POST /api/exams/validate-access`  
**DTOs Used:** `AccessExamDTO`

#### Purpose:
- Verify student has correct access credentials before taking exam
- Check if exam is currently within scheduled time window
- Prevent students from retaking exams

#### Steps:
1. Student provides:
   - **AccessCode**: Unique exam code
   - **AccessPassword**: Access password

2. System verifies:
   - Exam exists and is active (now between ScheduleStart and ScheduleEnd)
   - Access credentials are correct
   - If authenticated, check if student already completed this exam

#### Business Logic:
```
IF now < ScheduleStart OR now > ScheduleEnd
    RETURN "Exam is not currently available"

IF authenticated student already has Result with Status="Completed" or "Graded"
    RETURN "You have already submitted this exam. You cannot access it again."

RETURN "Access granted" + Exam details
```

#### Example Response (Success):
```json
{
  "canAttempt": true,
  "message": "Access granted",
  "data": {
    "examID": 1,
    "title": "Mathematics Final Exam",
    "durationMinutes": 120,
    "scheduledEnd": "2025-11-10T12:00:00Z"
  }
}
```

### 2.2 Student Views & Takes Exam
**Role:** Student (authenticated)  
**Endpoint:** `GET /api/exams/{id}/student-view`

#### Steps:
1. Student authenticates with JWT token
2. System verifies:
   - Exam is within schedule window
   - Student hasn't already submitted this exam
   - Exam hasn't been published yet

3. Returns exam with all questions and options:
   - Question text
   - Question type
   - All MCQ options (WITHOUT marking correct answer)
   - Marks for each question
   - Total marks and passing marks

#### Example Response:
```json
{
  "data": {
    "examID": 1,
    "title": "Mathematics Final Exam",
    "durationMinutes": 120,
    "scheduleStart": "2025-11-10T10:00:00Z",
    "scheduleEnd": "2025-11-10T12:00:00Z",
    "totalMarks": 100,
    "passingMarks": 40,
    "questions": [
      {
        "questionID": 10,
        "questionText": "What is 2 + 2?",
        "questionType": 0,
        "marks": 5,
        "options": [
          { "optionID": 1, "optionText": "3" },
          { "optionID": 2, "optionText": "4" },
          { "optionID": 3, "optionText": "5" }
        ]
      }
    ]
  }
}
```

---

## 3. EXAM SUBMISSION PHASE

### 3.1 Student Submits Answers
**Role:** Student (authenticated)  
**Endpoint:** `POST /api/exams/{examId}/responses`  
**DTOs Used:** `CreateStudentResponseDTO`

#### Steps:
1. For each question, student submits:
   - **QuestionID**: The question being answered
   - **AnswerText**: For subjective/SAQ questions (free text)
   - **SelectedOptionID**: For MCQ (ID of selected option)

2. System creates `StudentResponse` record with:
   - `ResponseID` (auto-generated)
   - `ExamID`
   - `QuestionID`
   - `StudentID`
   - `AnswerText` (question text response)
   - `IsCorrect` (null for subjective/SAQ, true/false for MCQ if auto-evaluated)
   - `MarksObtained` (0 initially for subjective, auto-calculated for MCQ)
   - `SubmittedAt` (current timestamp)

#### Auto-Grading for MCQ:
- System immediately checks if selected option is correct
- If correct: `IsCorrect = true`, `MarksObtained = Question.Marks`
- If incorrect: `IsCorrect = false`, `MarksObtained = 0`

#### Grading Status for Subjective/SAQ:
- `IsCorrect = null` (awaiting teacher grading)
- `MarksObtained = 0` (initially)

#### Example Request:
```json
{
  "questionID": 10,
  "answerText": "The capital of France",
  "selectedOptionID": 2
}
```

#### Example Response:
```json
{
  "message": "Answer submitted successfully",
  "data": {
    "responseID": 105,
    "examID": 1,
    "questionID": 10,
    "studentID": 20,
    "marksObtained": 5,
    "isCorrect": true,
    "submittedAt": "2025-11-10T10:15:30Z"
  }
}
```

### 3.2 Constraint: Submit Before Exam Ends
- Student can only submit responses while:
  - Current time is within `Exam.ScheduleStart` to `Exam.ScheduleEnd`
  - Exam has NOT been published yet (`Result.IsPublished = false`)

---

## 4. GRADING PHASE

### 4.1 Teacher Reviews Pending Responses
**Role:** Teacher (who created the exam)  
**Endpoints:**
- `GET /api/teacher/grading/exams/{examId}/pending` - All pending responses
- `GET /api/teacher/grading/exams/{examId}/questions/{questionId}/pending` - By question
- `GET /api/teacher/grading/exams/{examId}/students/{studentId}/pending` - By student

#### Data Returned:
For each pending response:
- Student name and ID
- Question text
- Student's answer
- Question marks
- Option to grade with marks

### 4.2 Teacher Grades Subjective Responses
**Role:** Teacher  
**Endpoint:** `POST /api/teacher/grading/grade-response`

#### Steps:
1. Teacher views a pending response (subjective/SAQ type)
2. Teacher assigns marks and optional feedback/comment
3. System creates `GradingRecord` with:
   - `GradingID` (auto-generated)
   - `ResponseID` (reference to StudentResponse)
   - `QuestionID`
   - `StudentID`
   - `GradedByTeacherID` = Logged-in teacher
   - `MarksObtained` (0 to Question.Marks)
   - `Feedback` (optional)
   - `Comment` (optional notes)
   - `IsPartialCredit` (boolean, if partial marking)
   - `Status` = "Graded"
   - `GradedAt` (current timestamp)

4. System updates `StudentResponse`:
   - `IsCorrect` = true (if full marks) or false (if partial/zero)
   - `MarksObtained` = Assigned marks

#### Example Response:
```json
{
  "message": "Response graded successfully",
  "data": {
    "gradingID": 50,
    "responseID": 105,
    "studentID": 20,
    "marksObtained": 4,
    "feedback": "Good explanation",
    "isPartialCredit": true,
    "gradedAt": "2025-11-10T14:30:00Z"
  }
}
```

### 4.3 Auto-Grading of MCQ Responses
- MCQ responses are auto-graded when submitted (not in this phase)
- However, teacher can review MCQ responses and mark them as reviewed

### 4.4 GradingRecord Model Structure
```
GradingRecord (Manual Grading Audit Trail)
├── GradingID (PK)
├── ResponseID (FK to StudentResponse)
├── QuestionID (FK to Question)
├── StudentID (FK to User)
├── GradedByTeacherID (FK to User - Teacher)
├── MarksObtained (decimal)
├── Feedback (text, optional)
├── Comment (text, optional)
├── IsPartialCredit (boolean)
├── Status (enum: Graded, Pending, Regraded)
├── RegradeFrom (int, reference to previous GradingRecord)
├── RegradeReason (text, optional)
├── GradedAt (datetime)
├── RegradeAt (datetime, optional)
```

---

## 5. RESULT CALCULATION & PUBLICATION PHASE

### 5.1 Check Grading Progress
**Role:** Teacher  
**Endpoint:** `GET /api/results/publication-status/{examId}`

#### Returns:
- Total student responses
- Graded responses count
- Pending responses count
- Percentage of grading completion
- Can-publish status (true if all graded)

#### Example Response:
```json
{
  "isPublished": false,
  "examID": 1,
  "examTitle": "Mathematics Final Exam",
  "gradingProgress": {
    "totalResponses": 150,
    "gradedResponses": 150,
    "pendingResponses": 0,
    "completionPercentage": 100,
    "canPublish": true
  }
}
```

### 5.2 Teacher Publishes Results
**Role:** Teacher (who created the exam)  
**Endpoint:** `POST /api/results/publish`  
**DTOs Used:** `PublishExamRequestDTO`

#### Prerequisites:
- ALL student responses must be graded
- Exam must exist and belong to teacher
- Exam not already published

#### Parameters:
- **examId**: ID of exam to publish
- **passingPercentage**: Percentage needed to pass (0-100, optional, defaults to exam's PassingPercentage)
- **notes**: Optional publication notes

#### System Process (within transaction):

##### Step 1: Verification
```
✓ Verify teacher owns exam
✓ Check exam not already published
✓ Check all responses are graded
✓ Get list of all students who submitted responses
```

##### Step 2: Auto-Grade MCQ (if any pending)
```
FOR each StudentResponse WHERE QuestionType = MCQ AND IsCorrect IS NULL
    IF SelectedOption.IsCorrect = true
        StudentResponse.IsCorrect = true
        StudentResponse.MarksObtained = Question.Marks
    ELSE
        StudentResponse.IsCorrect = false
        StudentResponse.MarksObtained = 0
```

##### Step 3: Calculate Results for Each Student
```
FOR each student who submitted responses:
    
    totalMarks = SUM(StudentResponse.MarksObtained for all responses)
    
    examTotalMarks = SUM(Question.Marks for all questions)
    
    percentage = (totalMarks / examTotalMarks) * 100
    
    isPassed = totalMarks >= (examTotalMarks * passingPercentage / 100)
    
    rank = COUNT(OTHER students WITH totalMarks > this student's totalMarks) + 1
```

##### Step 4: Create/Update Result Records
```
FOR each student:
    result = Find or Create Result(examId, studentId)
    
    result.TotalMarks = totalMarks
    result.Percentage = percentage
    result.Rank = rank
    result.Status = "Graded"
    result.IsPublished = true
    result.PublishedAt = NOW
    result.EvaluatedBy = teacherId
    result.EvaluatedAt = NOW
    
    Save result
```

##### Step 5: Create ExamPublication Record
```
examinationPublication = Create ExamPublication
├── ExamID = examId
├── Status = "Published"
├── TotalStudents = count of students
├── GradedStudents = count of graded students
├── PassingPercentage = passingPercentage provided
├── PublishedBy = teacherId
├── PublishedAt = NOW
├── PublicationNotes = notes
└── CreatedAt = NOW
```

#### Result Model Structure:
```
Result (Final Score & Publication)
├── ResultID (PK)
├── ExamID (FK)
├── StudentID (FK)
├── TotalMarks (decimal) - Student's marks obtained
├── Rank (int) - Position among all students
├── Percentage (decimal) - Student's percentage score
├── Status (enum: Pending, Completed, Graded)
├── IsPublished (boolean) - True after publishing
├── EvaluatedBy (FK to User - Teacher)
├── EvaluatedAt (datetime)
├── CreatedAt (datetime)
├── UpdatedAt (datetime, optional)
└── PublishedAt (datetime) - When results became visible
```

#### Example Response:
```json
{
  "success": true,
  "message": "Exam published successfully. 150 results are now visible to students.",
  "examID": 1,
  "examTitle": "Mathematics Final Exam",
  "totalStudents": 150,
  "gradedStudents": 150,
  "passingPercentage": 40,
  "resultsPublished": 150,
  "publishedAt": "2025-11-10T16:00:00Z",
  "publishedBy": 5
}
```

### 5.3 ExamPublication Record
```
ExamPublication (Publication Audit Trail)
├── PublicationID (PK)
├── ExamID (FK, unique)
├── Status (enum: NotPublished, Published, Rejected)
├── TotalStudents (int)
├── GradedStudents (int)
├── PassingPercentage (decimal)
├── PublishedBy (FK to User)
├── PublishedAt (datetime)
├── PublicationNotes (text, optional)
├── CreatedAt (datetime)
└── UpdatedAt (datetime, optional)
```

---

## 6. RESULT VIEWING PHASE

### 6.1 Student Views Published Results
**Role:** Student (authenticated)  
**Endpoint:** `GET /api/results/my-results`

#### Behavior:
- Only returns results that are PUBLISHED (`IsPublished = true` AND `Status = "Graded"`)
- Shows:
  - Exam title
  - Marks obtained
  - Total marks
  - Percentage
  - Rank
  - Pass/Fail status
  - Submission timestamp

#### Example Response:
```json
{
  "success": true,
  "page": 1,
  "pageSize": 10,
  "totalCount": 5,
  "data": [
    {
      "resultID": 1,
      "examID": 1,
      "examName": "Mathematics Final Exam",
      "studentID": 20,
      "studentName": "John Doe",
      "totalMarks": 75,
      "examTotalMarks": 100,
      "rank": 3,
      "percentage": 75,
      "passingPercentage": 40,
      "status": "Passed",
      "isPublished": true,
      "evaluatedAt": "2025-11-10T16:00:00Z",
      "publishedAt": "2025-11-10T16:00:00Z",
      "submittedAt": "2025-11-10T12:00:00Z",
      "createdAt": "2025-11-10T16:00:00Z"
    }
  ]
}
```

### 6.2 Student Views All Completed Exams
**Role:** Student (authenticated)  
**Endpoint:** `GET /api/results/my-completed-exams`

#### Behavior:
- Returns ALL completed exams (published or not)
- For unpublished results, masks sensitive data (scores hidden)
- Shows exam attempt status

---

## 7. COMPLETE DATA FLOW DIAGRAM

```
┌─────────────────────────────────────────────────────────────────┐
│                      EXAM CREATION PHASE                         │
├─────────────────────────────────────────────────────────────────┤
│ 1. Teacher creates Exam                                          │
│ 2. Teacher adds Questions (MCQ, SAQ, Subjective)               │
│ 3. Teacher adds Options for MCQ questions                       │
│ 4. System calculates TotalMarks & PassingMarks                 │
└─────────────────────┬───────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────────┐
│                   EXAM ACCESS PHASE                              │
├─────────────────────────────────────────────────────────────────┤
│ 1. Student validates access with code & password               │
│ 2. System checks exam schedule (ScheduleStart → ScheduleEnd)   │
│ 3. System checks if student already submitted                  │
│ 4. Student views exam questions                                 │
└─────────────────────┬───────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────────┐
│                   EXAM SUBMISSION PHASE                          │
├─────────────────────────────────────────────────────────────────┤
│ 1. Student submits answer to each question                      │
│ 2. System auto-grades MCQ immediately:                          │
│    - Correct → IsCorrect=true, MarksObtained=Marks             │
│    - Incorrect → IsCorrect=false, MarksObtained=0              │
│ 3. Subjective/SAQ responses marked as pending                   │
│ 4. StudentResponse records created                              │
└─────────────────────┬───────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────────┐
│                    GRADING PHASE                                 │
├─────────────────────────────────────────────────────────────────┤
│ 1. Teacher reviews pending responses (Subjective/SAQ)           │
│ 2. Teacher assigns marks & feedback                             │
│ 3. System creates GradingRecord                                 │
│ 4. System updates StudentResponse with marks                    │
│ 5. All responses reach "Graded" status                          │
└─────────────────────┬───────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────────┐
│              RESULT CALCULATION & PUBLICATION                    │
├─────────────────────────────────────────────────────────────────┤
│ 1. Teacher verifies all responses are graded                    │
│ 2. System calculates for each student:                          │
│    - TotalMarks = SUM(StudentResponse.MarksObtained)           │
│    - Percentage = (TotalMarks / ExamTotalMarks) * 100          │
│    - Rank = Position among all students                         │
│    - Status = "Graded" & IsPublished = true                    │
│ 3. System creates Result record for each student                │
│ 4. System creates ExamPublication record                        │
│ 5. Results become visible to students                           │
└─────────────────────┬───────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────────┐
│                   RESULT VIEWING PHASE                           │
├─────────────────────────────────────────────────────────────────┤
│ 1. Student views published results                              │
│ 2. Shows: Marks, Rank, Percentage, Pass/Fail status            │
│ 3. Student can compare with class performance                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 8. KEY CONSTRAINTS & BUSINESS RULES

### 8.1 Exam Creation Rules
- ✓ Only Teachers can create exams
- ✓ Access code must be unique across system
- ✓ Passing percentage must be 0-100
- ✓ ScheduleStart must be before ScheduleEnd
- ✓ Questions can be added until exam closes

### 8.2 Student Submission Rules
- ✓ Students can only submit during schedule window (ScheduleStart → ScheduleEnd)
- ✓ Students cannot retake exam once submitted (Result status = "Completed" or "Graded")
- ✓ Students cannot submit after exam published
- ✓ Each question can only be answered once per student per exam

### 8.3 Grading Rules
- ✓ Only the creating teacher can grade exam responses
- ✓ Admin can also grade on behalf
- ✓ MCQ auto-graded immediately on submission
- ✓ Subjective/SAQ require manual teacher grading
- ✓ Teacher must grade ALL responses before publication

### 8.4 Publication Rules
- ✓ Only creating teacher can publish results
- ✓ All responses must be graded first
- ✓ Passing percentage can be adjusted at publication time
- ✓ Exam can only be published once (unless unpublished first)
- ✓ Once published, students can see their results

### 8.5 Data Integrity
- ✓ Cascade delete: Deleting exam deletes all related questions, responses, results
- ✓ Unique constraint on (ExamID, QuestionID, StudentID) for StudentResponse
- ✓ Unique constraint on ExamID for ExamPublication
- ✓ Unique constraint on AccessCode for Exam

---

## 9. OPTIONAL FEATURES

### 9.1 Unpublish Exam Results
**Endpoint:** `POST /api/results/unpublish`

#### Purpose:
- Teacher can unpublish exam results if there was an error
- Results become hidden from students again

#### Changes:
- Sets `IsPublished = false` for all Result records
- Updates ExamPublication status back to "NotPublished"
- Records unpublish reason in audit trail

### 9.2 Re-grading
- Teacher can re-grade a response
- System tracks original grading in `GradingRecord.RegradeFrom`
- Maintains audit trail of grade changes

### 9.3 Question Pool & Randomization
- Teachers can create question banks/pools
- System can randomize question order per student

---

## 10. SUMMARY TABLE

| Phase | Actor | Key Action | Database Changes | Status |
|-------|-------|-----------|-------------------|--------|
| 1. Creation | Teacher | Create exam & questions | Exam, Question, QuestionOption created | Ready |
| 2. Access | Student | Validate access credentials | None | Checking only |
| 3. Submission | Student | Submit answers | StudentResponse created, MCQ auto-graded | In Progress |
| 4. Grading | Teacher | Review & grade subjective | GradingRecord created, StudentResponse updated | Pending |
| 5. Publication | Teacher | Publish results | Result created, ExamPublication created | Published |
| 6. Viewing | Student | View published results | None | Read only |

---

## 11. API ENDPOINT SUMMARY

### Teacher Endpoints
```
POST   /api/exams                              - Create exam
GET    /api/exams                              - Get my exams
GET    /api/exams/{id}                         - Get exam details
PUT    /api/exams/{id}                         - Update exam
DELETE /api/exams/{id}                         - Delete exam

POST   /api/exams/{examId}/questions           - Create question
GET    /api/exams/{examId}/questions           - Get all questions
GET    /api/exams/{examId}/questions/{id}      - Get question
PUT    /api/exams/{examId}/questions/{id}      - Update question
DELETE /api/exams/{examId}/questions/{id}      - Delete question

GET    /api/teacher/grading/exams/{examId}/pending                           - Pending responses
GET    /api/teacher/grading/exams/{examId}/questions/{questionId}/pending    - By question
GET    /api/teacher/grading/exams/{examId}/students/{studentId}/pending     - By student
POST   /api/teacher/grading/grade-response                                   - Grade response

GET    /api/teacher/exams/{examId}/responses/students                        - Student attempts
GET    /api/teacher/exams/{examId}/responses/students/{studentId}            - Student responses

POST   /api/results/publish                    - Publish exam results
POST   /api/results/unpublish                  - Unpublish exam results
GET    /api/results/publication-status/{examId} - Check publication status
```

### Student Endpoints
```
POST   /api/exams/validate-access              - Validate exam access
GET    /api/exams/{id}/student-view            - View exam questions
POST   /api/exams/{examId}/responses           - Submit answer
GET    /api/exams/{examId}/responses           - Get my responses
GET    /api/results/my-results                 - View published results
GET    /api/results/my-completed-exams         - View completed exams
```

---

## END OF WORKFLOW DOCUMENTATION
