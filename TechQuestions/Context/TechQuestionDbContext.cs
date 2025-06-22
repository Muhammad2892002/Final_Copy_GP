using Microsoft.EntityFrameworkCore;
using TechQuestions.Entites;

namespace TechQuestions.Context
{
    public class TechQuestionDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<JobTitle> JobTitles { get; set; }
        public DbSet<Testimoial> Testimoials { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }
        public DbSet<Question> Questions { get; set; }
        public TechQuestionDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var entity = modelBuilder.Entity(entityType.ClrType);

                // Primary Key
                entity.HasKey("Id");
                entity.Property<int>("Id").UseIdentityColumn();
                //Creation Date 
                // IsDeleted
                if (entityType.FindProperty("CreationDate") != null)
                {
                    entity.Property<DateTime>("CreationDate")
                        .HasDefaultValueSql("getdate()");
                }
                // IsDeleted
                if (entityType.FindProperty("IsDeleted") != null)
                {
                    entity.Property<bool>("IsDeleted")
                        .HasDefaultValue(false);
                }
            }
            modelBuilder.Entity<JobTitle>().HasMany<Question>().WithOne().HasForeignKey
                (x => x.JobTitleId);
            modelBuilder.Entity<User>().HasMany<Exam>().WithOne().HasForeignKey
               (x => x.CustomerId);
            modelBuilder.Entity<User>().HasMany<Testimoial>().WithOne().HasForeignKey
               (x => x.CustomerId);
            modelBuilder.Entity<Question>().HasMany<ExamQuestion>().WithOne().HasForeignKey
               (x => x.QuestionId);
            modelBuilder.Entity<Exam>().HasMany<ExamQuestion>().WithOne().HasForeignKey
               (x => x.ExamId);
            
            
        }
    }
}
