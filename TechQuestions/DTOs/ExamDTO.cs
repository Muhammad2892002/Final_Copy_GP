namespace TechQuestions.DTOs
{
    public class ExamDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ExamDate { get; set; }
        public string ExamDeadLine { get; set; }
        public string ExamLevel { get; set; }
        public int QuestionCountInEasyLevel { get; set; }
        public int QuestionCountInHardLevel { get; set; }
        public int QuestionCountInIntermidateLevel { get; set; }
        public string Customer { get; set; }
        public string JobTitle { get; set; }
        public string? Score { get; set; }
    }
}
