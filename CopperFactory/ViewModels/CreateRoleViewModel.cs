using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CopperFactory.ViewModels
{
    public class CreateRoleViewModel
    {
        [Required]
        [Display(Name = "Role")]
        public string RoleName { get; set; }
    }
}
