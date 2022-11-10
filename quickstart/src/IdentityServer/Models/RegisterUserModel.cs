using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models
{
    public class RegisterUserModel : IValidatableObject
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression("^([0-9]{10})$", ErrorMessage = "Invalid Mobile Number")]
        public string PhoneNumber { get; set; }

        [DataType(DataType.Password)]
        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,15}$", ErrorMessage = "Password must have at least one digit and one capital letter with total lenght not less than 8")]
        public string Password { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult>();
        }
    }
}
