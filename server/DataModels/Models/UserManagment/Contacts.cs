using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModels.Models.UserManagment
{ 
    [Table("contacts")]
    public class Contacts
    {       
        [Key, Column("userid")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [Column("contactid")]
        public int ContactId { get; set; }
        public virtual User Contact { get; set; }
    }
}
