using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CopperFactory.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "Last Name")]
        public string SeconedName { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [EmailAddress(ErrorMessage = "The Email is not a valid e-mail address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [DataType(DataType.Password , ErrorMessage = "Passwords must be at least 6 characters and one non alphanumeric character and at least one digit ('0'-'9') and at least one uppercase ('A'-'Z').")]
        public string Password { get; set; }

        [Required]
        public int FactoryID { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password",
            ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string RoleName { get; set; }


    }
}
