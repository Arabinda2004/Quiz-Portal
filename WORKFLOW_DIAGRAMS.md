# Online Exam Portal - Visual Workflow Diagrams

## 1. High-Level System Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                    ONLINE EXAM PORTAL SYSTEM                     │
└──────────────────────────────────────────────────────────────────┘

┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│    FRONTEND     │     │  BACKEND API    │     │   DATABASE      │
├─────────────────┤     ├─────────────────┤     ├─────────────────┤
│  Student UI     │◄───►│  Controllers    │◄───►│  Exams          │
│  Teacher UI     │     │  Services       │     │  Questions      │
│  Admin UI       │     │  Middleware     │     │  Responses      │
└─────────────────┘     │  Auth           │     │  Results        │
                        └─────────────────┘     │  GradingRecords │
                                                └─────────────────┘
```

---

## 2. Entity Relationship Diagram (ERD)

```
┌─────────────────────────────────────┐
│            USER                      │
├─────────────────────────────────────┤
│ UserID (PK)                         │
│ Email                               │
│ FullName                            │
│ Role (Teacher, Student, Admin)      │
│ CreatedAt                           │
└─────────────────────────────────────┘
           △                   △
           │                   │ (CreatedBy)
           │                   │
           │(CreatedByUser)    │(EvaluatorUser)
           │                   │
┌──────────┴──────────┐    ┌────────┴──────────────┐
│                     │    │                       │
│                     │    │                       │
┌──────────────────────────────┐      ┌─────────────────────────┐
│      EXAM                     │      │      RESULT             │
├──────────────────────────────┤      ├─────────────────────────┤
│ ExamID (PK)                  │      │ ResultID (PK)           │
│ Title                        │      │ ExamID (FK)             │
│ Description                  │      │ StudentID (FK)          │
│ CreatedBy (FK)               │      │ TotalMarks              │
│ ScheduleStart / ScheduleEnd  │      │ Rank                    │
│ AccessCode (UNIQUE)          │      │ Percentage              │
│ AccessPassword               │      │ Status (Graded/Pending) │
│ PassingPercentage            │      │ IsPublished             │
│ DurationMinutes              │      │ EvaluatedBy (FK)        │
│ CreatedAt                    │      │ PublishedAt             │
└──────────┬───────────────────┘      └─────────────────────────┘
           │ (1:Many)
           │
           ▼
┌──────────────────────────────────────────┐
│        QUESTION                          │
├──────────────────────────────────────────┤
│ QuestionID (PK)                          │
│ ExamID (FK)                              │
│ CreatedBy (FK)                           │
│ QuestionText                             │
│ QuestionType (MCQ, SAQ, Subjective)      │
│ Marks                                    │
│ CreatedAt                                │
└──────────┬───────────────────────────────┘
           │ (1:Many)
           │
           ▼
┌──────────────────────────────────────────┐
│     QUESTIONOPTION                       │
├──────────────────────────────────────────┤
│ QuestionOptionID (PK)                    │
│ QuestionID (FK)                          │
│ OptionText                               │
│ IsCorrect (boolean)                      │
│ CreatedAt                                │
└──────────────────────────────────────────┘

    ┌─────────────────────────────────────┐
    │    STUDENTRESPONSE                  │
    ├─────────────────────────────────────┤
    │ ResponseID (PK)                     │
    │ ExamID (FK)                         │
    │ QuestionID (FK)                     │
    │ StudentID (FK)                      │
    │ AnswerText                          │
    │ IsCorrect (MCQ only)                │
    │ MarksObtained                       │
    │ SubmittedAt                         │
    │ (UNIQUE on ExamID, QuestionID,      │
    │          StudentID)                 │
    └──────────┬──────────────────────────┘
               │ (1:1)
               │
               ▼
    ┌─────────────────────────────────────┐
    │    GRADINGRECORD                    │
    ├─────────────────────────────────────┤
    │ GradingID (PK)                      │
    │ ResponseID (FK)                     │
    │ QuestionID (FK)                     │
    │ StudentID (FK)                      │
    │ GradedByTeacherID (FK)              │
    │ MarksObtained                       │
    │ Feedback / Comment                  │
    │ IsPartialCredit                     │
    │ Status (Graded, Regraded)           │
    │ GradedAt                            │
    └─────────────────────────────────────┘

    ┌─────────────────────────────────────┐
    │   EXAMPUBLICATION                   │
    ├─────────────────────────────────────┤
    │ PublicationID (PK)                  │
    │ ExamID (FK, UNIQUE)                 │
    │ Status (Published, NotPublished)    │
    │ TotalStudents                       │
    │ GradedStudents                      │
    │ PassingPercentage                   │
    │ PublishedBy (FK)                    │
    │ PublishedAt                         │
    │ PublicationNotes                    │
    │ CreatedAt                           │
    └─────────────────────────────────────┘
```

---

## 3. Complete Workflow State Machine

```
                           ┌──────────────────┐
                           │  EXAM CREATION   │
                           │  (Teacher)       │
                           └────────┬─────────┘
                                    │
                    ┌───────────────┼───────────────┐
                    │               │               │
            ┌───────▼────────┐      │      ┌────────▼────────┐
            │ Add Questions  │      │      │ Set Exam Config │
            │ (MCQ, SAQ,     │      │      │ • Schedule      │
            │  Subjective)   │      │      │ • Access Code   │
            │ Add Options    │      │      │ • Passing %     │
            └───────┬────────┘      │      └─────────────────┘
                    │               │
                    └───────────────┼───────────────┐
                                    │               │
                            ┌───────▼──────────────▼────────┐
                            │   EXAM READY FOR STUDENTS     │
                            │   (Within Schedule Window)     │
                            └───────────┬────────────────────┘
                                        │
            ┌───────────────────────────┼───────────────────────────┐
            │                           │                           │
    ┌───────▼─────────┐        ┌───────▼─────────┐        ┌────────▼────────┐
    │ STUDENT VIEWS   │        │  STUDENT TAKES  │        │   SCHEDULE      │
    │ EXAM QUESTIONS  │        │  EXAM           │        │   ENDED         │
    │ (Read Only)     │        │ (Submitand)     │        │ (Exam Closed)   │
    └─────────────────┘        └───────┬─────────┘        └─────────────────┘
                                       │
                    ┌──────────────────┼──────────────────┐
                    │                  │                  │
            ┌───────▼───────────┐   ┌──▼───────┐   ┌─────▼───────────┐
            │  MCQ SUBMITTED    │   │  SAQ/    │   │   SUBJECTIVE    │
            │  (Auto-Graded)    │   │SUBJECTIVE│   │  (Pending Grade)│
            │  ✓ MarksObtained  │   │(Pending) │   │   ✓ AnswerText  │
            │    recorded       │   │          │   │   ✗ IsCorrect   │
            └───────────────────┘   └──┬───────┘   └─────┬───────────┘
                    │                  │                  │
                    └──────────────────┼──────────────────┘
                                       │
                            ┌──────────▼──────────┐
                            │  TEACHER GRADES     │
                            │  PENDING RESPONSES  │
                            │  (Only SAQ/Subject) │
                            └──────────┬──────────┘
                                       │
                    ┌──────────────────┼──────────────────┐
                    │                  │                  │
            ┌───────▼───────────┐   ┌──▼───────────┐   ┌─▼──────────────┐
            │  GRADING RECORD   │   │   UPDATE     │   │   ALL RESPONSES│
            │  CREATED          │   │   RESPONSE   │   │   NOW GRADED   │
            │  ✓ MarksObtained  │   │   RECORD     │   │   ✓ 100%       │
            │  ✓ Feedback       │   │              │   └────────┬───────┘
            │  ✓ Status=Graded  │   └──────────────┘            │
            └───────────────────┘                               │
                    │                                           │
                    └───────────────────────┬───────────────────┘
                                            │
                        ┌───────────────────▼────────────────┐
                        │  ALL RESPONSES GRADED              │
                        │  Ready for Publication             │
                        └───────────────────┬────────────────┘
                                            │
                        ┌───────────────────▼────────────────┐
                        │  TEACHER PUBLISHES RESULTS         │
                        │  • Calculate ranks                 │
                        │  • Create Result records           │
                        │  • Create ExamPublication record   │
                        │  • Set IsPublished = true          │
                        └───────────────────┬────────────────┘
                                            │
                    ┌───────────────────────┼───────────────────┐
                    │                       │                   │
            ┌───────▼──────────┐   ┌───────▼─────────────┐   ┌─▼──────────┐
            │  RESULT RECORD   │   │ EXAMPUBLICATION    │   │  VISIBLE   │
            │  CREATED         │   │ CREATED            │   │  TO        │
            │  ✓ TotalMarks    │   │ Status="Published" │   │  STUDENTS  │
            │  ✓ Rank          │   │                    │   └────────────┘
            │  ✓ Percentage    │   │ ✓ Passing %        │
            │  ✓ IsPublished   │   │ ✓ Publish date     │
            │                  │   │                    │
            └──────────────────┘   └────────────────────┘
                    │                       │
                    └───────────────────────┼──────────┐
                                            │          │
                        ┌───────────────────▼────┐    │
                        │  STUDENT VIEWS RESULTS │    │
                        │ • Marks obtained       │    │
                        │ • Rank                 │    │
                        │ • Percentage           │    │
                        │ • Pass/Fail            │    │
                        └────────────────────────┘    │
                                                      │
                        ┌─────────────────────────┐   │
                        │ OPTIONAL: UNPUBLISH     │◄──┘
                        │ (Teacher can unpublish) │
                        │ Results hidden again    │
                        └─────────────────────────┘
```

---

## 4. Question Type Processing Flow

```
┌─────────────────────────────────────────────────────────┐
│           TEACHER CREATES QUESTION                      │
└────────────────────┬────────────────────────────────────┘
                     │
         ┌───────────┴───────────┐
         │                       │
    ┌────▼─────────┐    ┌───────▼──────────┐
    │ Question     │    │   Question Type  │
    │ Details      │    │   Selection      │
    └────┬─────────┘    └───────┬──────────┘
         │                      │
         ▼                      ▼
    ┌────────────────────────────────────────────┐
    │  MCQ (Type = 0)    SAQ (Type = 1)  SUBJ(2) │
    │  Multiple Choice   Short Answer   Subj.    │
    └─┬──────────────┬─────────────┬─────────────┘
      │              │             │
      │              │             │
    ┌─▼──────────────────┐   ┌─────▼───────────────────┐
    │ ADD OPTIONS        │   │ NO OPTIONS (or generic) │
    │ • Option 1: "Text" │   │ (Free form answer)      │
    │ • Option 2: "Text" │   └────┬────────────────────┘
    │ • Option 3: "Text" │        │
    │ • Option 4: "Text" │        │
    │ Mark ONE as correct│        │
    └────┬───────────────┘        │
         │                        │
         ▼                        ▼
    ┌──────────────────┐   ┌───────────────────┐
    │ STUDENT SUBMITS  │   │ STUDENT SUBMITS   │
    │ ANSWER           │   │ ANSWER TEXT       │
    │ (Select Option)  │   │ (Type Response)   │
    └────┬─────────────┘   └────┬──────────────┘
         │                      │
    ┌────▼──────────────────────▼──────────────┐
    │ SYSTEM EVALUATION                        │
    └────┬──────────────────────┬──────────────┘
         │                      │
    ┌────▼──────────────┐  ┌────▼──────────────┐
    │ AUTO-GRADED       │  │ AWAITING GRADING  │
    │ Selected Option   │  │ Marked as Pending │
    │ = Correct Option? │  │ Teacher must      │
    │                   │  │ assign marks      │
    │ YES → Marks=Full  │  │                   │
    │ NO → Marks=0      │  │                   │
    │                   │  │                   │
    │ IsCorrect=true/   │  │ IsCorrect=null    │
    │          false    │  │ MarksObtained=0   │
    └───────────────────┘  └────┬──────────────┘
            │                   │
            │               ┌───▼──────────────────┐
            │               │ TEACHER REVIEWS &    │
            │               │ GRADES               │
            │               │ • Assign marks       │
            │               │ • Add feedback       │
            │               │ • Create grading rec │
            │               │                      │
            │               │ IsCorrect=t/f        │
            │               │ MarksObtained=value  │
            │               └───┬──────────────────┘
            │                   │
            └───────────────────┼──────────────┐
                                │              │
                    ┌───────────▼──────────┐   │
                    │  RESPONSE GRADED     │   │
                    │  Ready for Result    │   │
                    └──────────────────────┘   │
                                               │
                        ┌──────────────────────┘
                        │ (Repeat for all questions)
                        │
                        ▼
                    ┌──────────────────────┐
                    │ ALL RESPONSES        │
                    │ GRADED               │
                    │ Ready for Publishing │
                    └──────────────────────┘
```

---

## 5. Result Calculation & Publishing Flow

```
┌────────────────────────────────────────────────┐
│  ALL STUDENT RESPONSES GRADED                  │
│  Teacher clicks "Publish Results"              │
└─────────────────────┬──────────────────────────┘
                      │
        ┌─────────────▼──────────────┐
        │ BEGIN TRANSACTION          │
        │ (Atomic operation)         │
        └─────────────┬──────────────┘
                      │
    ┌─────────────────┼────────────────┐
    │                 │                │
┌───▼───────┐   ┌────▼────────────┐   ┌──▼──────────────┐
│ VERIFY     │   │ AUTO-GRADE MCQ  │   │ GET STUDENTS    │
│ • Exam     │   │ IF NEEDED       │   │ WHO SUBMITTED   │
│   exists   │   │ (if any pending)│   │ (ALL StudentID) │
│ • Teacher  │   │                 │   │                 │
│   owns it  │   │ Set IsCorrect   │   └────┬────────────┘
│ • Not yet  │   │ Set MarksObtain │        │
│   published│   │                 │        │
└───┬────────┘   └─────────────────┘        │
    │                                       │
    └───────────────┬───────────────────────┘
                    │
        ┌───────────▼────────────────┐
        │ FOR EACH STUDENT:          │
        └───────┬────────────────────┘
                │
    ┌───────────▼────────────────────────────┐
    │ CALCULATE RESULTS                      │
    │                                        │
    │ totalMarks = SUM(                      │
    │   StudentResponse.MarksObtained        │
    │   for all this student's responses)    │
    │                                        │
    │ examTotalMarks = SUM(                  │
    │   Question.Marks for all questions)    │
    │                                        │
    │ percentage = (totalMarks /             │
    │   examTotalMarks) * 100                │
    │                                        │
    │ passingMarks = (examTotalMarks *       │
    │   passingPercentage) / 100             │
    │                                        │
    │ isPassed = totalMarks >= passingMarks  │
    │                                        │
    │ rank = COUNT(students with higher      │
    │        totalMarks) + 1                 │
    └───────┬────────────────────────────────┘
            │
    ┌───────▼──────────────────────────┐
    │ CREATE/UPDATE RESULT RECORD       │
    │                                  │
    │ Result {                          │
    │   ExamID: 1                       │
    │   StudentID: 20                   │
    │   TotalMarks: 75                  │
    │   Percentage: 75.0                │
    │   Rank: 3                         │
    │   Status: "Graded"                │
    │   IsPublished: true               │
    │   PublishedAt: NOW                │
    │   EvaluatedBy: teacherId          │
    │   EvaluatedAt: NOW                │
    │ }                                 │
    │                                  │
    │ INSERT or UPDATE to Results table│
    └───────┬──────────────────────────┘
            │
        ┌───▼───────────────────────────┐
        │ [Repeat for all students]     │
        │ (BULK INSERT/UPDATE)          │
        └───┬───────────────────────────┘
            │
    ┌───────▼──────────────────────────┐
    │ CREATE EXAMPUBLICATION RECORD     │
    │                                  │
    │ ExamPublication {                │
    │   ExamID: 1                      │
    │   Status: "Published"            │
    │   TotalStudents: 150             │
    │   GradedStudents: 150            │
    │   PassingPercentage: 40          │
    │   PublishedBy: teacherId         │
    │   PublishedAt: NOW               │
    │   PublicationNotes: "..."        │
    │   CreatedAt: NOW                 │
    │ }                                │
    │                                  │
    │ INSERT into ExamPublications     │
    └───────┬──────────────────────────┘
            │
    ┌───────▼──────────────────────┐
    │ SAVE ALL CHANGES              │
    │ (SaveChangesAsync)            │
    └───────┬──────────────────────┘
            │
    ┌───────▼──────────────────────┐
    │ COMMIT TRANSACTION            │
    │ (All-or-Nothing)              │
    └───────┬──────────────────────┘
            │
        SUCCESS?
       /        \
      /          \
    YES          NO
     │            │
     │      ┌─────▼───────────┐
     │      │ ROLLBACK ALL    │
     │      │ CHANGES         │
     │      │ THROW ERROR     │
     │      └─────────────────┘
     │
    ┌▼──────────────────────────┐
    │ RESULTS NOW VISIBLE       │
    │ TO STUDENTS               │
    │                           │
    │ Students can:             │
    │ • View their scores       │
    │ • See rank                │
    │ • See passing status      │
    │ • Compare percentages     │
    └───────────────────────────┘
```

---

## 6. Data Access Control Flow

```
┌────────────────────────────────┐
│  INCOMING API REQUEST           │
└──────────────┬─────────────────┘
               │
       ┌───────▼────────┐
       │ JWT VALIDATION │
       │ Extract UserId │
       │ Extract Role   │
       └───────┬────────┘
               │
         ┌─────▼──────────────────┐
         │  EXTRACT USER CONTEXT  │
         └─────┬──────────────────┘
               │
      ┌────────┴──────────┬──────────────┬──────────────┐
      │                   │              │              │
    TEACHER            STUDENT         ADMIN        UNKNOWN
      │                   │              │              │
      ▼                   ▼              ▼              ▼
   ┌────────┐          ┌──────┐      ┌──────┐      ┌────────┐
   │Can:    │          │Can:  │      │Can:  │      │ REJECT │
   │• Create│          │• View│      │• View│      │ 401    │
   │  exam  │          │ own  │      │all   │      └────────┘
   │• Create│          │ exams│      │exams │
   │  ques- │          │• Take│      │• View│
   │  tions │          │exam  │      │all   │
   │• Grade │          │• View│      │users │
   │• View  │          │own   │      │• View│
   │  own   │          │ resp │      │all   │
   │  resp  │          │• View│      │ resp │
   │• Publish│         │own   │      │• Admin│
   │ results│         │results│     │grading│
   │        │          │      │      │      │
   └─┬──────┘          └──┬───┘      └──┬───┘
     │                    │            │
     └─────────┬──────────┴────────────┘
               │
         ┌─────▼──────────────────┐
         │FETCH FROM DATABASE     │
         │(with ownership checks) │
         └─────┬──────────────────┘
               │
         ┌─────▼──────────────────┐
         │ VERIFY OWNERSHIP       │
         │ (if applicable)        │
         └─────┬──────────────────┘
               │
          ✓/✗?
         /    \
        /      \
       ✓        ✗
       │        │
       │   ┌────▼──────┐
       │   │ 403 FORBID│
       │   └───────────┘
       │
  ┌────▼──────────────┐
  │ RETURN DATA       │
  │ (with masks if    │
  │  unpublished)     │
  └───────────────────┘
```

---

## 7. Sequence Diagram: Complete Exam Workflow

```
Teacher          Student           System           Database
  │                │                 │                │
  │─── Create ─────────────────────>  │                │
  │     Exam       │                  │─────────────>  │
  │                │                  │     INSERT     │
  │                │                  │              Create Exam
  │                │                  │<─────────────  │
  │<─────────────────────────────────│                │
  │   Exam Created  │                 │                │
  │                 │                 │                │
  │─── Add ─────────────────────────> │                │
  │  Questions      │                 │─────────────>  │
  │                 │                 │    INSERT x5   │
  │                 │                 │              Create Questions
  │                 │                 │<─────────────  │
  │<─────────────────────────────────│                │
  │  Questions Added│                 │                │
  │                 │                 │                │
  │                 │ ┌────────────────────────────────┐
  │                 │ │ Exam Schedule Active           │
  │                 │ │ (ScheduleStart < NOW < End)    │
  │                 │ └────────────────────────────────┘
  │                 │                 │                │
  │                 │─ Validate ────> │                │
  │                 │ AccessCode      │─────SELECT──>  │
  │                 │ AccessPassword  │    FROM Exam   │
  │                 │                 │              Get Exam
  │                 │                 │<───────────── │
  │                 │<─────────────────────────────────│
  │                 │  Access Granted │                │
  │                 │                 │                │
  │                 │─ Get ──────────>│                │
  │                 │ Exam Questions  │─────SELECT──>  │
  │                 │                 │    Questions   │
  │                 │                 │      +Options  │
  │                 │                 │              Get Questions
  │                 │                 │<───────────── │
  │                 │<─────────────────────────────────│
  │                 │ Questions Loaded│                │
  │                 │                 │                │
  │                 │─ Submit ──────> │                │
  │                 │ Answer Q1       │─────INSERT──>  │
  │                 │ (Option 2)      │ StudentResponse│
  │                 │                 │              Create Response
  │                 │                 │    (Auto-Grade)│
  │                 │                 │              ✓ Correct!
  │                 │                 │<───────────── │
  │                 │<─────────────────────────────────│
  │                 │ Response Saved  │                │
  │                 │ Marks = 5       │                │
  │                 │                 │                │
  │                 │[...repeat for other questions...]│
  │                 │                 │                │
  │                 │─ Submit ──────> │                │
  │                 │ Answer Q3       │─────INSERT──>  │
  │                 │ (Subjective)    │ StudentResponse│
  │                 │                 │              Create Response
  │                 │                 │    (Pending)   │
  │                 │                 │              ✗ Awaiting Grade
  │                 │                 │<───────────── │
  │                 │<─────────────────────────────────│
  │                 │ Response Saved  │                │
  │                 │ Marks = 0 (TBD) │                │
  │                 │                 │                │
  │                 │ ┌───────────────────────────────┐
  │                 │ │ Exam Completed               │
  │                 │ │ Schedule Ended               │
  │                 │ └───────────────────────────────┘
  │                 │                 │                │
  │─ Grade ──────────────────────────>│                │
  │ Responses       │                 │─────SELECT──>  │
  │                 │                 │ Pending Resp   │
  │                 │                 │              Get Responses
  │                 │                 │<───────────── │
  │<─────────────────────────────────│                │
  │ Subjective Resp │                 │                │
  │ for review      │                 │                │
  │                 │                 │                │
  │─ Grade Q3 ────────────────────────>│                │
  │ Marks: 4        │                 │─────INSERT──>  │
  │ Feedback: "Good"│                 │ GradingRecord  │
  │                 │                 │                │
  │                 │                 │─────UPDATE──>  │
  │                 │                 │ StudentResp    │
  │                 │                 │  Marks = 4     │
  │                 │                 │              Create Grading
  │                 │                 │              Update Response
  │                 │                 │<───────────── │
  │<─────────────────────────────────│                │
  │ Graded          │                 │                │
  │                 │                 │                │
  │─ Publish ──────────────────────────>│                │
  │ Results         │                 │                │
  │                 │                 │─────SELECT──>  │
  │                 │                 │ All Responses  │
  │                 │                 │              Get All Responses
  │                 │                 │<───────────── │
  │                 │                 │                │
  │                 │                 │ Calculate:    │
  │                 │                 │ • TotalMarks   │
  │                 │                 │ • Percentage   │
  │                 │                 │ • Rank         │
  │                 │                 │                │
  │                 │                 │─────INSERT──>  │
  │                 │                 │ Results        │
  │                 │                 │                │
  │                 │                 │─────INSERT──>  │
  │                 │                 │ ExamPublication│
  │                 │                 │              Create Results
  │                 │                 │              Create Publication
  │                 │                 │<───────────── │
  │<─────────────────────────────────│                │
  │ Published       │                 │                │
  │                 │                 │                │
  │                 │─ View ────────> │                │
  │                 │ My Results      │─────SELECT──>  │
  │                 │                 │ Results        │
  │                 │                 │ WHERE Published│
  │                 │                 │              Get Published
  │                 │                 │<───────────── │
  │                 │<─────────────────────────────────│
  │                 │ Results         │                │
  │                 │ Rank: 3         │                │
  │                 │ Marks: 77/100   │                │
  │                 │ Passed          │                │
```

---

## END OF VISUAL DIAGRAMS
