namespace TechQuestions.Entites
{
    public class JobTitle : MainEntity 
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Image {  get; set; }
        public string Category { get; set; }
        public int    CategoryId { get; set; }
    }
}
