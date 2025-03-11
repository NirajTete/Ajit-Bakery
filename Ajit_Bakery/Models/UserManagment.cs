using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class UserManagment
    {

        public int Id { get; set; }
        public string UserName { get; set; }
        public string PageName { get; set; }
        public string Role { get; set; }
        [NotMapped]
        public List<string>? selectedpages { get; set; }
    }
}
