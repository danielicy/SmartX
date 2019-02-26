using System;
using System.Collections.Generic;
using System.Text;

namespace DataModels.Dtos
{
    public class FollowerDto
    {
        public int UserId { get; set; }
        public int FollowedId { get; set; }
    }
}
