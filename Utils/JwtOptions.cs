namespace api.Utils
{
    /// <summary>
    /// Jwt options
    /// </summary>
    public interface IJwtOptions
    {
        /// <summary>
        /// jwt key
        /// </summary>
        /// <value></value>
        string JwtKey { get; }
        /// <summary>
        /// jwt issuer
        /// </summary>
        /// <value></value>
        string JwtIssuer { get; }
        /// <summary>
        /// jwt expires
        /// </summary>
        /// <value></value>
        int JwtExpireDays { get; }
    }
}