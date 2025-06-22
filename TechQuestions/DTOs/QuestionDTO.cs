namespace TechQuestions.DTOs
{
    public class QuestionDTO
    {
        public int Id { get; set; }
        public string? MultiMediaPath { get; set; }
        public string JobTitle { get; set; }
        public string Body { get; set; }
        public string Type { get; set; }
        public string Level { get; set; }
        public string Time { get; set; }
        public bool IsDeleted { get; set; }
    }
}
