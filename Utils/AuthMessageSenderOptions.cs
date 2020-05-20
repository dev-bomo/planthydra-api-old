namespace api.Utils
{
    /// <summary>
    /// Options for the auth message sender
    /// </summary>
    public interface IAuthMessageSenderOptions
    {
        /// <summary>
        /// The sendgrid user
        /// </summary>
        /// <value></value>
        string SendGridUser { get; }

        /// <summary>
        /// The sendgrid key
        /// </summary>
        /// <value></value>
        string SendGridKey { get; }
    }
}