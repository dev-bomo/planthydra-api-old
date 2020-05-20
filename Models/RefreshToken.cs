using System;
using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; private set; }
        public DateTime Expires { get; private set; }
        public string UserId { get; private set; }
        public bool Active => DateTime.UtcNow <= Expires;

        public RefreshToken(string token, DateTime expires, string userId)
        {
            Token = token;
            Expires = expires;
            UserId = userId;
        }
    }
}
