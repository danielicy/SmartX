using System;
using System.Collections.Generic;
using DataModels.Models.Tweets;
using DataModels.Models.UserManagment;
using Microsoft.EntityFrameworkCore;


namespace DataCore
{
    //[DbConfigurationType(typeof(MySqlConfiguration))]
    public class MyTweetContext : DbContext
    {

        public MyTweetContext()
        {

        }
        public MyTweetContext(DbContextOptions<MyTweetContext> options) : base(options)
        {
           
        }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Contacts>().HasKey(r => new { r.UserId, r.ContactId });

            modelBuilder.Entity<Contacts>()
            .HasOne(pt => pt.User)
            .WithMany(p => p.UserContacts)
            .HasForeignKey(pt => pt.UserId).OnDelete(DeleteBehavior.Restrict); ;

            modelBuilder.Entity<Contacts>()
            .HasOne(pt => pt.Contact)
            .WithMany(t => t.ContactUsers)
            .HasForeignKey(pt => pt.ContactId).OnDelete(DeleteBehavior.Restrict); ;

        }

        private void DoDo()
        {
            
        }

        private Boolean DoMore(int v)
        {
            return true;
        }

        private int DoSomething(Boolean v)
        {
            return 0;
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Tweet> Tweets { get; set; }
        public DbSet<Contacts> Follower { get; set; }

       
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
             
        }

        public void Commit()
        {
            this.SaveChanges();
        }

    }

}



//namespace EFGetStarted.AspNetCore.NewDb.Models
//{
//    public class BloggingContext : DbContext
//    {
//        public BloggingContext(DbContextOptions<BloggingContext> options)
//            : base(options)
//        {
//        }

//        public DbSet<Blog> Blogs { get; set; }
//        public DbSet<Post> Posts { get; set; }
//    }

//    public class Blog
//    {
//        public int BlogId { get; set; }
//        public string Url { get; set; }

//        public ICollection<Post> Posts { get; set; }
//    }

//    public class Post
//    {
//        public int PostId { get; set; }
//        public string Title { get; set; }
//        public string Content { get; set; }

//        public int BlogId { get; set; }
//        public Blog Blog { get; set; }
//    }
//}

