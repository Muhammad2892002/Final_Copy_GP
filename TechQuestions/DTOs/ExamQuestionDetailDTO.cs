using TechQuestions.Entites;

namespace TechQuestions.DTOs
{
    public class ExamQuestionDetailDTO
    {
        public Question? Question { get; set; }
        public Exam ?Exam { get; set; }
        public User ?User { get; set; }  
        public ExamQuestion? ExamQuestion { get; set; }
    }
}
