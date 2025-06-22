namespace TechQuestions.DTOs
{
    public class JobTitleDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Image { get; set; }
        public int CategoryId { get; set; }
        public string Time { get; set; }
        public bool IsDeleted { get; set; }
    }
}
