namespace TechQuestions.Entites
{
    public class MainEntity
    {
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
