using DataCore;
using DataModels.Models.Tweets;
using DataModels.Models.UserManagment;
using Microsoft.EntityFrameworkCore;
using MyTweetAPI.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace MyTweetAPI.Services
{
    public class TweetService : ITweetService
    {
        private MyTweetContext _context;

        public TweetService(MyTweetContext context)
        {
            _context = context;
        }

        public Tweet Create(Tweet tweet, string param)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public void Follow(Contacts follower)
        {
            _context.Follower.Add(follower);
            _context.SaveChanges();
        }

        public IEnumerable<Tweet> GetAll()
        {
            return _context.Tweets.Include("User");
        }

        public Tweet GetById(int id)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Gets followed users tweets (The tweeting user, date added, content), ordered by date
        /// </summary>
        /// <param name="id">follower id</param>
        /// <returns></returns>
        public IEnumerable<Tweet> GetFollowedtweets(int id)
        {
            var followed = _context.Follower.Where(follower => follower.UserId == id);
            return _context.Tweets.
                Where(userid => followed.Any(f => f.ContactId == userid.UserId))
                .OrderBy(date => date.CreatedDate).Include("User");
        }

        /// <summary>
        /// Gets own users tweets
        /// </summary>
        /// <param name="id">loged in user Id</param>
        /// <returns></returns>
        public IEnumerable<Tweet> GetMyTweets(int id)
        {
            return _context.Tweets.Where(userid => userid.UserId == id).OrderBy(date => date.CreatedDate);
        }

        /// <summary>
        /// Creates a new Tweet
        /// </summary>
        /// <param name="tweet"></param>
        /// <returns></returns>
        public Tweet Tweet(Tweet tweet)
        {
            tweet.User = _context.Users.Where(user => user.Id.Equals(tweet.UserId)).FirstOrDefault();

            tweet.CreatedDate = DateTime.Now;

            _context.Tweets.Add(tweet);
            _context.SaveChanges();

            return tweet;
        }

        public void UnFollow(Contacts follower)
        {
            var itemToRemove = _context.Follower.Where(followed => followed.ContactId == follower.UserId).FirstOrDefault();

            if (follower != null)
            {
                _context.Follower.Remove(itemToRemove);
                _context.SaveChanges();
            }
        }

        public void Update(Tweet model, string param = null)
        {
            throw new NotImplementedException();
        }


    }
}
