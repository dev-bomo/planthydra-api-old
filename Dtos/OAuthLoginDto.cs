using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace api.Dtos
{
    /// <summary>
    /// OAuth login DTO
    /// </summary>
    public class OAuthLoginDto
    {
        /// <summary>
        /// The user
        /// </summary>
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// The password
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// The grant type
        /// </summary>
        [FromForm(Name = "grant_type")]
        [Required]
        public string GrantType { get; set; }
    }
}