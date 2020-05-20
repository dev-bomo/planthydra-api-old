namespace api.Dtos
{
    /// <summary>
    /// Client data when they register
    /// </summary>
    public class ProductClientDto
    {
        /// <summary>
        /// The name they should be addressed by
        /// </summary>
        /// <value></value>
        public string name { get; set; }

        /// <summary>
        /// The email
        /// </summary>
        /// <value></value>
        public string email { get; set; }

        /// <summary>
        /// Whether they are registered to get newsletters
        /// </summary>
        /// <value></value>
        public bool isSubscribed { get; set; }

    }
}