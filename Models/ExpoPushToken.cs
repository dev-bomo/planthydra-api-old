using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class ExpoPushToken
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        public string Token { get; set; }

        public bool IsActive { get; set; }

        public User User { get; set; }

        public ExpoPushToken(string token, bool isActive)
        {
            this.Token = token;
            this.IsActive = isActive;
        }
    }
}

