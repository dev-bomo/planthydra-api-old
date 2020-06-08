
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using api.GraphQL;
using api.Helpers;
using api.Models;
using api.Services;
using api.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;

namespace api
{
    /// <summary>
    /// Startup class
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">root config</param>
        /// <param name="env">hosting env</param>
        public Startup(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        /// <summary>
        /// The root app configuration
        /// </summary>
        /// <value></value>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// The hosting environment
        /// </summary>
        public Microsoft.AspNetCore.Hosting.IHostingEnvironment HostingEnvironment { get; }

        /// <summary>
        /// Configures services, called by the runtime
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            ISecretsVault secretsVault = new SecretsVault(HostingEnvironment);
            services.AddDbContext<Db>(options => options.UseSqlite(secretsVault.DbConnectionString));
            services.AddIdentity<User, IdentityRole>(config =>
                {
                    config.SignIn.RequireConfirmedEmail = false;
                    config.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultPhoneProvider;
                })
                .AddEntityFrameworkStores<Db>()
                .AddRoles<IdentityRole>()
                .AddDefaultTokenProviders();

            // ===== Add Jwt Authentication ========
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = secretsVault.JwtIssuer,
                        ValidAudience = secretsVault.JwtIssuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretsVault.JwtKey)),
                        ClockSkew = TimeSpan.Zero // remove delay of token when expire
                    };
                }).AddFacebook(facebookOptions => // are these really needed?
                {
                    facebookOptions.AppId = secretsVault.FbAppId;
                    facebookOptions.AppSecret = secretsVault.FbSecret;
                }); ;

            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddTransient<IJwtOptions, SecretsVault>();
            services.AddTransient<IAuthMessageSenderOptions, SecretsVault>();
            services.AddTransient<IFacebookAuthOptions, SecretsVault>();
            services.AddSingleton<IDispatcher, Dispatcher>();
            services.AddSingleton<IHostedService, ScheduledWateringService>();
            services.AddSingleton<ITokenGenerator, TokenGenerator>();
            services.AddSingleton<IInternalCommsService, InternalCommsService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IPushNotificationService, PushNotificationService>();
            services.AddTransient<ISeedDatabaseService, SeedDatabaseService>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddHttpClient();
            services.AddGraphQLServices();


            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1.1", new Info
                {
                    Title = "Meridia API",
                    Version = "1.1"
                });
                c.DescribeAllEnumsAsStrings();
                //Set the comments path for the swagger json and ui.
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, "api.xml");
                c.IncludeXmlComments(xmlPath);

                c.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "password",
                    TokenUrl = Path.Combine(HostingEnvironment.WebRootPath, "/api/account/swaggerLogin")
                });
                c.OperationFilter<SecurityRequirementsOperationFilter>();
            });
        }

        /// <summary>
        /// Configure the app once the services are up and running
        /// </summary>
        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();

            {
                app.UseHsts();
            }

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };
            //app.UseApplicationInsights();
            app.UseWebSockets(webSocketOptions);
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseCors();
            app.UseMvc();
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1.1/swagger.json", "Meridia API V1.1");
                c.OAuthAppName("Meridia API");
                c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
            });

            // Get to swagger when getting /
            var rewriteOptions = new RewriteOptions();
            rewriteOptions.AddRedirect("^$", "swagger");
            app.UseRewriter(rewriteOptions);
            app.ConfigureGraphQL();
        }
    }
}
