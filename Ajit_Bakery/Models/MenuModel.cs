using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Ajit_Bakery.Models
{
    public class MenuModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MenuId { get; set; }
        public int? ParentMenuId { get; set; }
        public string? icon { get; set; }
        public string? Title { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
    }
}
