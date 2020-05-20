using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    /// <summary>
    /// The product client from a marketing perspective
    /// </summary>
    public class ProductClient
    {
        /// <summary>
        /// The id
        /// </summary>
        /// <value></value>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The name the customer should be addressed by
        /// </summary>
        /// <value></value>
        public string Name { get; set; }

        /// <summary>
        /// The email
        /// </summary>
        /// <value></value>
        public string Email { get; set; }

        /// <summary>
        /// Whether or not they are subscribed to the newsletter
        /// </summary>
        /// <value></value>
        public bool IsSubscribed { get; set; }
    }
}