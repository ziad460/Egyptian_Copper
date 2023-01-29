using System.ComponentModel.DataAnnotations;

namespace CopperFactory.ViewModels
{
    public class ForgetPassVM
    {
        [Required(ErrorMessage = "This field is required")]
        [EmailAddress(ErrorMessage = "The Email is not a valid e-mail address")]
        public string Email { get; set; }
    }
}
