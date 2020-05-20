using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos;
using api.Helpers;
using api.Models;
using api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace api.Controllers
{
    /// <summary>
    /// Account controller
    /// </summary>
    [Route("api/[controller]/[action]")]
    public class AccountController : Controller
    {
        /// <summary>
        /// The database context
        /// </summary>
        public Db Context { get; private set; }

        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IEmailSender _emailSender;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IFacebookAuthOptions _facebookOptions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="roleManager"></param>
        /// <param name="dbContext"></param>
        /// <param name="tokenGenerator"></param>
        /// <param name="emailSender"></param>
        /// <param name="clientFactory"></param>
        /// <param name="facebookOptions"></param>
        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            Db dbContext,
            ITokenGenerator tokenGenerator,
            IEmailSender emailSender,
            IHttpClientFactory clientFactory,
            IFacebookAuthOptions facebookOptions
            )
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this.Context = dbContext;
            this._tokenGenerator = tokenGenerator;
            this._emailSender = emailSender;
            this._clientFactory = clientFactory;
            this._facebookOptions = facebookOptions;
            this._roleManager = roleManager;
        }

        /// <summary>
        /// Login a user via email and password but through the swagger endpoint
        /// </summary>
        /// <param name="model">The body of the request</param>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<object> SwaggerLogin([FromForm] OAuthLoginDto model)
        {
            if (model.GrantType != "password")
            {
                return new BadRequestResult();
            }
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
            if (result.Succeeded)
            {
                var appUser = _userManager.Users.Include(u => u.DeviceTokens).Include(i => i.RefreshTokens).SingleOrDefault(r => r.Email == model.Username);
                return await GetSuccessfulOAuthLoginResponse(appUser);
            }
            throw new ApplicationException("INVALID_LOGIN_ATTEMPT");
        }

        /// <summary>
        /// Login a user via email and password
        /// </summary>
        /// <param name="model">The body of the request</param>
        [HttpPost]
        public async Task<object> Login([FromBody] LoginDto model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                var appUser = _userManager.Users.Include(u => u.DeviceTokens).Include(i => i.RefreshTokens).SingleOrDefault(r => r.Email == model.Email);
                // generate a refreshtoken and add it to the list of refresh tokens on the client

                return await GetSuccessfulLoginResponse(appUser);
            }

            return Unauthorized();
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Register([FromBody] RegisterDto model)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return Ok(await this.GetSuccessfulLoginResponse(user, true));
            }

            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Refresh a token with the current refresh token
        /// </summary>
        /// <param name="refreshToken"></param>
        [HttpPost("refreshtoken")]
        public async Task<ActionResult> RefreshToken([FromBody] string refreshToken)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            ClaimsPrincipal cp = this._tokenGenerator.GetPrincipalFromToken(refreshToken);
            if (cp != null)
            {
                string id = Misc.GetIdFromClaimsPrincipal(cp);
                // TODO: are these AppUsers maybe? What's the difference?
                var user = await this.Context.Users.Include(u => u.RefreshTokens).FirstAsync(u => u.Id == id);

                if (user.HasValidRefreshToken(refreshToken))
                {
                    var jwtToken = this._tokenGenerator.GenerateJwtToken(user);
                    var newRefreshToken = this._tokenGenerator.GetRefreshToken();
                    user.RemoveRefreshToken(refreshToken); // delete the token we've exchanged
                    user.AddRefreshToken(refreshToken); // add the new one
                    await this.Context.SaveChangesAsync();
                    return new ContentResult { ContentType = Constants.ApplicationJsonContentType, StatusCode = (int)HttpStatusCode.OK, Content = newRefreshToken };
                }
            }

            return BadRequest();
        }


        /// <summary>
        /// Trigger a "forgot password" flow
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto fpd)
        {
            var user = await _userManager.FindByEmailAsync(fpd.Email);
            // if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            if (user == null)
            {
                return Ok();
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailSender.SendEmailAsync(
                    fpd.Email,
                    "Reset Password",
                    $"Use this reset code to reset your password: " + code);
            return Ok();
        }


        /// <summary>
        /// Reset a password
        /// </summary>
        /// <param name="resetPass">The payload of the request</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto resetPass)
        {
            var user = await _userManager.FindByEmailAsync(resetPass.Email);
            if (user == null)
            {
                //don't reveal that the user doesn't exist
                return Ok();
            }
            var result = await _userManager.ResetPasswordAsync(user, resetPass.ResetPasswordCode, resetPass.NewPassword);
            return Ok();
        }

        /// <summary>
        /// Login via facebook
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        public async Task<IActionResult> Facebook([FromBody] FacebookLoginDto model)
        {
            HttpClient client = this._clientFactory.CreateClient("Facebook");
            var fbAppId = this._facebookOptions.FbAppId;
            var fbAppSecret = this._facebookOptions.FbSecret;
            // 1.generate an app access token
            var appAccessTokenResponse = await client.GetStringAsync($"https://graph.facebook.com/oauth/access_token?client_id={fbAppId}&client_secret={fbAppSecret}&grant_type=client_credentials");
            var appAccessToken = JsonConvert.DeserializeObject<FacebookAppAccessToken>(appAccessTokenResponse);
            // 2. validate the user access token
            var userAccessTokenValidationResponse = await client.GetStringAsync($"https://graph.facebook.com/debug_token?input_token={model.Token}&access_token={appAccessToken.AccessToken}");
            var userAccessTokenValidation = JsonConvert.DeserializeObject<FacebookUserAccessTokenValidation>(userAccessTokenValidationResponse);

            if (!userAccessTokenValidation.Data.IsValid)
            {
                return BadRequest("Invalid facebook auth token");
            }

            // 3. we've got a valid token so we can request user data from fb
            var userInfoResponse = await client.GetStringAsync($"https://graph.facebook.com/v2.8/me?fields=id,email,first_name,last_name,name,picture&access_token={model.Token}");
            var userInfo = JsonConvert.DeserializeObject<FacebookUserData>(userInfoResponse);

            // 4. ready to create the local user account (if necessary) and jwt
            var user = await _userManager.FindByEmailAsync(userInfo.Email);

            if (user == null)
            {
                var appUser = new User
                {
                    Email = userInfo.Email,
                    UserName = userInfo.Email
                };

                var result = await _userManager.CreateAsync(appUser, _tokenGenerator.GetOAuthPassword());

                if (!result.Succeeded) return new BadRequestObjectResult(result);
                return Ok(await this.GetSuccessfulLoginResponse(appUser, true));
            }
            else
            {
                var appUser = _userManager.Users.Include(u => u.DeviceTokens).Include(i => i.RefreshTokens).SingleOrDefault(r => r.Email == userInfo.Email);
                if (appUser == null)
                {
                    return BadRequest("Could not log in");
                }
                return Ok(await this.GetSuccessfulLoginResponse(appUser, false));
            }
        }

        /// <summary>
        /// Deletes a user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = Context.Users.Include(u => u.RefreshTokens).Include(u => u.DeviceTokens).SingleOrDefault(u => u.Id == id.ToString());
            if (user == null)
            {
                return BadRequest("Could not find user");
            }
            user.ClearDeviceTokens();
            user.ClearRefreshTokens();
            Context.Users.Remove(user);
            await Context.SaveChangesAsync();
            return Ok("Successful deletion");
        }


        /// <summary>
        /// Logs out the currently logged in user
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Logout()
        {
            var usr = await this.Context.Users
                .Include(u => u.DeviceTokens)
                .Include(u => u.WateringSchedules)
                .FirstOrDefaultAsync(u => u.Id == Misc.GetIdFromClaimsPrincipal(User));
            if (usr == null)
            {
                return NotFound("Could not find such a user in our system");
            }
            Context.RemoveRange(Context.RefreshTokens.Where(r => r.UserId == usr.Id));
            await Context.SaveChangesAsync();
            await _signInManager.SignOutAsync();

            return Ok();
        }

        /// <summary>
        /// Adds an expo push token to the user
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SetExpoPushToken([FromBody] ExpoPushTokenDto dto)
        {
            User usr = await this.Context.Users.Include(u => u.ExpoPushTokens)
            .FirstOrDefaultAsync(u => u.Id == Misc.GetIdFromClaimsPrincipal(User));
            if (usr == null)
            {
                return NotFound("Could not find user");
            }

            usr.AddExpoPushToken(dto.token);
            await Context.SaveChangesAsync();

            return Ok();
        }

        #region Private methods

        private async Task<OAuthLoginResponseDto> GetSuccessfulOAuthLoginResponse(User user)
        {
            string refTok = this._tokenGenerator.GetRefreshToken();
            user.AddRefreshToken(refTok);
            await this.Context.SaveChangesAsync();

            return new OAuthLoginResponseDto
            {
                AccessToken = this._tokenGenerator.GenerateJwtToken(user),
                RefreshToken = refTok
            };
        }

        private async Task<LoginResponseDto> GetSuccessfulLoginResponse(User user, bool register = false)
        {
            string devTok;
            if (register)
            {
                devTok = this._tokenGenerator.GetDeviceToken();
                user.AddDeviceToken(devTok);
            }
            else
            {
                user.ClearRefreshTokens();
                if (user.DeviceTokens.FirstOrDefault() == null)
                {
                    user.AddDeviceToken(this._tokenGenerator.GetDeviceToken());
                    await Context.SaveChangesAsync();
                }
                devTok = user.DeviceTokens.FirstOrDefault().Token;
            }

            string refTok = this._tokenGenerator.GetRefreshToken();
            user.AddRefreshToken(refTok);

            await this.Context.SaveChangesAsync();

            return new LoginResponseDto
            {
                DeviceToken = devTok,
                AccessToken = this._tokenGenerator.GenerateJwtToken(user),
                RefreshToken = refTok,
                Email = user.Email
            };
        }
    }

    #endregion
}