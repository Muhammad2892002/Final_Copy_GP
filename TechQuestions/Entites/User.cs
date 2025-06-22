namespace TechQuestions.Entites
{
    public class User : MainEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Image { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string CountryCode { get; set; }
        public string Nationality { get; set; }
        public string UserType { get; set; }
    }
}
