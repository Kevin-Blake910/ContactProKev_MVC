using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using RequiredAttribute = Microsoft.Build.Framework.RequiredAttribute;

namespace ContactProKev_MVC.Models
{
    public class Category
    {
        //Primary Key
        public int Id { get; set; }

        //Foreign Key
        [Required]
        public string? AppUserID { get; set; }

        
        [Required]
        [Display(Name = "Category Name")]
        public string? Name { get; set; }

        //Navigation Properties
        public virtual AppUser? AppUser { get; set; }

        public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();
        
    }
}
