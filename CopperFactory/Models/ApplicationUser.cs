using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CopperFactory.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string SeconedName { get; set; }
        public int FactoryID { get; set; }

        [ForeignKey("FactoryID")]
        public virtual Factory? Factory { get; set; }
    }
}
