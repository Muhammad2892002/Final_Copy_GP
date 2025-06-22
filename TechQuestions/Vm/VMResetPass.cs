using System.ComponentModel.DataAnnotations;

namespace TechQuestions.Vm
{
    public class VMResetPass
    {
        [Required]
        public string Email { get; set; }

        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$_@#!%*?&])[A-Za-z\d$_@#!%*?&]{8,}$",
      ErrorMessage = "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character.")]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

    }
}
