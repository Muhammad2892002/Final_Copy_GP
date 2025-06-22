namespace TechQuestions.DTOs
{
    public class CommentDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public string Date { get; set; }
        public string Image { get; set; }
        public bool IsDeleted { get; set; }
    }
}
