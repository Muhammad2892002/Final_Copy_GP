namespace TechQuestions.Entites
{
    public class Question : MainEntity
    {
        public int JobTitleId { get; set; }
        public string? MultiMediaPath { get; set; }
        public string Body { get; set; }
        public string Type { get; set; }
        public string Level { get; set; }
        public string? Answer1 { get; set; }
        public string? Answer2 { get; set; }
        public string? Answer3 { get; set; }
        public string? Answer4 { get; set; }
        public string CorrectAnswer { get; set; }
    }
}
