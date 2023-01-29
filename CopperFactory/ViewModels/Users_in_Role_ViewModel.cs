using System.ComponentModel.DataAnnotations;

namespace CopperFactory.ViewModels
{
    public class Users_in_Role_ViewModel
    {
        [Required(ErrorMessage = "This field is required")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string Username { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string FactoryName { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string Role { get; set; }
    }
}
