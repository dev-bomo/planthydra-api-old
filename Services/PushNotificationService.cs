using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using api.Dtos;
using api.Models;
using Microsoft.Extensions.Logging;

namespace api.Services
{

    /// <summary>
    /// Service for push notifications
    /// </summary>
    public interface IPushNotificationService
    {
        /// <summary>
        /// Given a user it sends push notifications to all active devices
        /// </summary>
        /// <param name="user">The user</param>
        /// <param name="deviceToken">The notification</param>
        void BroadcastDeviceOfflineNotification(User user, string deviceToken);
    }

    /// <summary>
    /// Service for push notifications
    /// </summary>
    public class PushNotificationService : IPushNotificationService
    {

        private readonly Db _context;

        private readonly ILogger<IPushNotificationService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">DbContext is injected</param>
        /// <param name="logger">The logger</param>
        public PushNotificationService(Db context, ILogger<IPushNotificationService> logger)
        {
            this._context = context;
            this._logger = logger;
        }

        /// <summary>
        /// Send a CIP offline notification to all user devices
        /// </summary>
        /// <param name="user">The user</param>
        /// <param name="deviceToken">The CIP token</param>
        public void BroadcastDeviceOfflineNotification(User user, string deviceToken)
        {
            List<ExpoPushToken> activeTokens = _context.ExpoPushTokens
               .Where(ept => ept.User.Id == user.Id && ept.IsActive == true).ToList();

            if (activeTokens.Count < 1)
            {
                this._logger.LogWarning("NotifyUserDevices", "User {0} has no active devices", user.Id);
                return;
            }

            using (WebClient client = new WebClient())
            {
                client.Headers.Add("accept", "application/json");
                client.Headers.Add("accept-encoding", "gzip, deflate");
                client.Headers.Add("Content-Type", "application/json");

                string response = null;

                foreach (ExpoPushToken ept in activeTokens)
                {
                    dynamic body = new
                    {
                        to = ept.Token,
                        title = "Device offline",
                        body = "The device with token=" + deviceToken + "went offline",
                        sound = "default"
                    };

                    response = client.UploadString("https://exp.host/--/api/v2/push/send", JsonExtensions.SerializeToJson(body));
                    // TODO: handle the response, check push receipts 
                }
            }
        }
    }
}