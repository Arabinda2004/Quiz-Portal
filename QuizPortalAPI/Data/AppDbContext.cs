using Microsoft.EntityFrameworkCore;
using QuizPortalAPI.Models;

namespace QuizPortalAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<StudentResponse> StudentResponses { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<GradingRecord> GradingRecords { get; set; }
        public DbSet<ExamPublication> ExamPublications { get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_User_Email");

            // Exam configuration
            modelBuilder.Entity<Exam>()
                .HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Cascade);

            // Exam to Questions relationship
            modelBuilder.Entity<Exam>()
                .HasMany(e => e.Questions)
                .WithOne(q => q.Exam)
                .HasForeignKey(q => q.ExamID)
                .OnDelete(DeleteBehavior.Cascade);

            // AccessCode unique constraint on Exams
            modelBuilder.Entity<Exam>()
                .HasIndex(e => e.AccessCode)
                .IsUnique()
                .HasDatabaseName("IX_Exam_AccessCode");

            // Schedule validation (optional but good practice)
            modelBuilder.Entity<Exam>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Exam)
                .WithMany(e => e.Questions)
                .HasForeignKey(q => q.ExamID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.CreatedByUser)
                .WithMany()
                .HasForeignKey(q => q.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Question>()
                .HasIndex(q => q.ExamID);

            // QuestionOption configuration 
            modelBuilder.Entity<QuestionOption>()
                .HasOne(qo => qo.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(qo => qo.QuestionID)
                .OnDelete(DeleteBehavior.Cascade);

            // Default values
            modelBuilder.Entity<Question>()
                .Property(q => q.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<QuestionOption>()
                .Property(qo => qo.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // StudentResponse configuration 
            modelBuilder.Entity<StudentResponse>()
                .HasOne(sr => sr.Exam)
                .WithMany()
                .HasForeignKey(sr => sr.ExamID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentResponse>()
                .HasOne(sr => sr.Question)
                .WithMany()
                .HasForeignKey(sr => sr.QuestionID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentResponse>()
                .HasOne(sr => sr.Student)
                .WithMany()
                .HasForeignKey(sr => sr.StudentID)
                .OnDelete(DeleteBehavior.Cascade);

            // Result configuration 
            modelBuilder.Entity<Result>()
                .HasOne(r => r.Exam)
                .WithMany()
                .HasForeignKey(r => r.ExamID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Result>()
                .HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Result>()
                .HasOne(r => r.EvaluatorUser)
                .WithMany()
                .HasForeignKey(r => r.EvaluatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // Default values 
            modelBuilder.Entity<StudentResponse>()
                .Property(sr => sr.SubmittedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Result>()
                .Property(r => r.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // GradingRecord configuration 
            modelBuilder.Entity<GradingRecord>()
                .HasOne(gr => gr.StudentResponse)
                .WithMany()
                .HasForeignKey(gr => gr.ResponseID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GradingRecord>()
                .HasOne(gr => gr.Question)
                .WithMany()
                .HasForeignKey(gr => gr.QuestionID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GradingRecord>()
                .HasOne(gr => gr.Student)
                .WithMany()
                .HasForeignKey(gr => gr.StudentID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GradingRecord>()
                .HasOne(gr => gr.GradedByTeacher)
                .WithMany()
                .HasForeignKey(gr => gr.GradedByTeacherID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GradingRecord>()
                .Property(gr => gr.GradedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // ExamPublication configuration 
            modelBuilder.Entity<ExamPublication>()
                .HasOne(ep => ep.Exam)
                .WithMany()
                .HasForeignKey(ep => ep.ExamID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExamPublication>()
                .HasOne(ep => ep.PublishedByUser)
                .WithMany()
                .HasForeignKey(ep => ep.PublishedBy)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ExamPublication>()
                .HasIndex(ep => ep.ExamID)
                .IsUnique()
                .HasDatabaseName("IX_ExamPublication_ExamID");

            modelBuilder.Entity<ExamPublication>()
                .Property(ep => ep.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}