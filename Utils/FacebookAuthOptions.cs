namespace api.Utils
{
    /// <summary>
    /// Fb auth options
    /// </summary>
    public interface IFacebookAuthOptions
    {
        /// <summary>
        /// app id
        /// </summary>
        /// <value></value>
        string FbAppId { get; }
        /// <summary>
        /// app secret
        /// </summary>
        /// <value></value>
        string FbSecret { get; }
    }
}