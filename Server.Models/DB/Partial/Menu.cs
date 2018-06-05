namespace Server.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    partial class Menu
    {
        [NotMapped]
        public List<Menu> Childs { get; set; } = new List<Menu>(); 
    }
}
