using System;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Hosting;

namespace api.Utils
{

    /// <summary>
    /// Gets app secrets based on the environment
    /// </summary>
    public interface ISecretsVault : IFacebookAuthOptions, IAuthMessageSenderOptions, IJwtOptions
    {
        /// <summary>
        /// The connection string
        /// </summary>
        /// <value></value>
        string DbConnectionString { get; }
    }

    internal class SecretsVault : ISecretsVault
    {
        private readonly string _fbAppId = "FbAppId";
        private readonly string _fbSecret = "FbSecret";
        private readonly string _dbConnectionString = "ConnectionString";
        private readonly string _sendGridUser = "SendGridUser";
        private readonly string _sendGridKey = "SendGridKey";
        private readonly string _jwtKey = "JwtKey";
        private readonly string _jwtIssuer = "JwtIssuer";
        private readonly string _jwtExpireDays = "JwtExpireDays";

        private readonly string _vaultUrl = "https://planthydra-vault.vault.azure.net/";
        private readonly bool _devMode;
        private readonly SecretClient _secretsClient;

        public string FbAppId
        {
            get => _devMode
                ? Environment.GetEnvironmentVariable(_fbAppId)
                : _secretsClient.GetSecret(_fbAppId).Value.Value;
        }
        public string FbSecret
        {
            get => _devMode
                ? Environment.GetEnvironmentVariable(_fbSecret)
                : _secretsClient.GetSecret(_fbSecret).Value.Value;
        }
        public string DbConnectionString
        {
            get => _devMode
                ? Environment.GetEnvironmentVariable(_dbConnectionString)
                : _secretsClient.GetSecret(_dbConnectionString).Value.Value;
        }
        public string SendGridUser
        {
            get => _devMode
                ? Environment.GetEnvironmentVariable(_sendGridUser)
                : _secretsClient.GetSecret(_sendGridUser).Value.Value;
        }
        public string SendGridKey
        {
            get => _devMode
                ? Environment.GetEnvironmentVariable(_sendGridKey)
                : _secretsClient.GetSecret(_sendGridKey).Value.Value;
        }
        public string JwtKey
        {
            get => _devMode
                ? Environment.GetEnvironmentVariable(_jwtKey)
                : _secretsClient.GetSecret(_jwtKey).Value.Value;
        }
        public string JwtIssuer
        {
            get => _devMode
                ? Environment.GetEnvironmentVariable(_jwtIssuer)
                : _secretsClient.GetSecret(_jwtIssuer).Value.Value;
        }

        public int JwtExpireDays
        {
            get => _devMode
                ? Int32.Parse(Environment.GetEnvironmentVariable(_jwtExpireDays))
                : 30;
        }

        public SecretsVault(IHostingEnvironment env)
        {
            if (env.IsDevelopment() == true)
            {
                _devMode = true;
            }
            else
            {
                _devMode = false;
                SecretClientOptions options = new SecretClientOptions()
                {
                    Retry =
                    {
                        Delay= TimeSpan.FromSeconds(2),
                        MaxDelay = TimeSpan.FromSeconds(16),
                        MaxRetries = 5,
                        Mode = RetryMode.Exponential
                    }
                };

                _secretsClient = new SecretClient(
                    new Uri(_vaultUrl),
                    new DefaultAzureCredential(),
                    options);
            }
        }
    }
}