using DataModels.Models.UserManagment;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModels.Models.Tweets
{
    [Table("tweets")]
    public class Tweet
    {        

        [Key, Column("tweetid")]
        public int Id { get; set; }
        
        [Column("userid")]
        public int UserId { get; set; }
        
        [NotMapped]
        public string UserName { get { return User.UserName; } }
               
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [Column("tweet_content")]
        public string Content { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }


    }
}
