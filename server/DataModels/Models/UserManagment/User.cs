using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModels.Models.UserManagment
{
    [Table("users")]
    public class User
    {       

        [Key, Column("userid")]
        public int Id { get; set; }

        [Column("user_name")]
        public string UserName { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; }

        [Column("last_name")]
        public string LastName { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [NotMapped]
        public string Password { get; set; }

        [Column("hashed_password")]
        public byte[] HashedPassword { get; set; }

        [Column("password_salt")]
        public byte[] PasswordSalt { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }


        [Column("information")]
        public string Information { get; set; }


        [Column("user_picture")]
        public byte[] UserPicture { get; set; }

        [Column("UserStatus")]      
        public int UserStatus { get; set; }

        [Column("role_id")]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }

        public List<Contacts> UserContacts { get; set; }
        public List<Contacts> ContactUsers { get; set; }

    }
}
