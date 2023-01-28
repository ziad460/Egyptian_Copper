using System.ComponentModel.DataAnnotations;

namespace CopperFactory.ViewModels
{
    public class EditRoleViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Role Name is required")]
        public string Name { get; set; }
    }
}
