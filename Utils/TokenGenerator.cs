using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace api.Utils
{
    /// <summary>
    /// Generates tokens for users
    /// </summary>
    public interface ITokenGenerator
    {
        /// <summary>
        /// Gets the refresh token
        /// </summary>
        /// <param name="size"></param>
        string GetRefreshToken(int size = 32);

        /// <summary>
        /// Gets the device token
        /// </summary>
        /// <returns></returns>
        string GetDeviceToken();
        /// <summary>
        /// Gets the oauth password
        /// </summary>
        /// <returns></returns>
        string GetOAuthPassword();
        /// <summary>
        /// Generates a jwt token
        /// </summary>
        /// <param name="user">The user</param>
        /// <returns></returns>
        string GenerateJwtToken(User user);
        /// <summary>
        /// Get the claims principal from the user
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        ClaimsPrincipal GetPrincipalFromToken(string token);
    }

    /// <summary>
    /// Generates tokens for users
    /// </summary>
    public class TokenGenerator : ITokenGenerator
    {
        private JwtSecurityTokenHandler _tokenHandler;
        private readonly ILogger<TokenGenerator> _logger;

        private readonly Db _context;
        private readonly IJwtOptions _options;

        /// <summary>
        /// Generates tokens for users
        /// </summary>
        /// <param name="logger">A logger</param>
        /// <param name="context">The DB</param>
        /// <param name="options">JWT config options</param>
        public TokenGenerator(ILogger<TokenGenerator> logger, Db context, IJwtOptions options)
        {
            this._tokenHandler = new JwtSecurityTokenHandler();
            this._logger = logger;
            this._options = options;
            this._context = context;
        }

        /// <summary>
        /// Get the refresh token
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public string GetRefreshToken(int size = 32)
        {
            var randomNumber = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                // + is not handled well by the query string so we're replacing it with 9
                String encodedRandom = Convert.ToBase64String(randomNumber);
                encodedRandom = encodedRandom.Replace('+', '9');
                return encodedRandom;
            }
        }

        /// <summary>
        /// Get the device token
        /// </summary>
        /// <returns></returns>
        public string GetDeviceToken()
        {
            return this.GetRefreshToken(10);
        }

        // TODO: This is NOT GOOD. Need to generate a password that for sure does not fail the constraints
        /// <summary>
        /// Get the oauth password
        /// </summary>
        /// <returns></returns>
        public string GetOAuthPassword()
        {
            return this.GetRefreshToken(8);
        }

        /// <summary>
        /// Create the jwt token for the user
        /// </summary>
        /// <param name="user">The <see cref="User"/> instance</param>
        public string GenerateJwtToken(User user)
        {
            var identity = GenerateClaimsIdentity(user.Id, user.UserName);
            var roles = _context.UserRoles.Where(r => r.UserId == user.Id);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                identity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Rol),
                identity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Id)
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, _context.Roles.Single(r => r.Id == role.RoleId).Name));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._options.JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(this._options.JwtExpireDays);

            var token = new JwtSecurityToken(
                this._options.JwtIssuer,
                this._options.JwtIssuer,
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Gets the claim principal from the refresh token
        /// </summary>
        /// <param name="token"></param>
        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            return this.ValidateToken(token, new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._options.JwtKey)),
                ValidateLifetime = false // we check expired tokens here
            });
        }

        private ClaimsIdentity GenerateClaimsIdentity(string id, string userName)
        {
            return new ClaimsIdentity(new GenericIdentity(userName, "Token"), new[]
            {
                new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.Id, id),
                new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.Rol, Helpers.Constants.Strings.JwtClaims.ApiAccess)
            });
        }

        private ClaimsPrincipal ValidateToken(string token, TokenValidationParameters tokenValidationParameters)
        {
            var principal = this._tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                this._logger.LogError("TokenGenerator_InvalidToken", "User: {0}", Misc.GetIdFromClaimsPrincipal(principal));
                throw new SecurityTokenException("Invalid token");
            }


            return principal;
        }
    }
}