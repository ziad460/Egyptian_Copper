using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CopperFactory.ViewModels
{
    public class CreateRoleViewModel
    {
        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "Role")]
        public string RoleName { get; set; }
    }
}
