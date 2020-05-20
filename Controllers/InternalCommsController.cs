using System;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.Services;
using api.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace api.Controllers
{

    /// <summary>
    /// The dto used to send the device token between internal services
    /// </summary>
    public class DeviceTokenDto
    {
        /// <summary>
        /// The device token
        /// </summary>
        /// <value>It's a string</value>
        public string deviceToken { get; set; }
    }

    /// <summary>
    /// ontroller used for internal comms between own web services. This has no security but the host should be 
    /// configured such that it only accepts calls from the other microservices' ips
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class InternalCommsController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;

        private readonly ILogger<InternalCommsController> _logger;

        private readonly Db _context;

        private readonly IPushNotificationService _pushNotificationService;

        /// <summary>
        /// Controller used for internal comms between own web services. This has no security but the host should be 
        /// configured such that it only accepts calls from the other microservices' ips
        /// </summary>
        /// <param name="dispatcher">The Dispatcher</param>
        /// <param name="logger">The logger</param>
        /// <param name="context">The Db context</param>
        /// <param name="pushNotificationService">The push notifications service</param>
        public InternalCommsController(
            IDispatcher dispatcher,
            ILogger<InternalCommsController> logger,
            Db context,
            IPushNotificationService pushNotificationService)
        {
            this._dispatcher = dispatcher;
            this._logger = logger;
            this._context = context;
            this._pushNotificationService = pushNotificationService;
        }

        /// <summary>
        /// The deviceOffline operation. It sets the device as offline in the DB and send a push notification to the app
        /// </summary>
        /// <param name="devTok">The device token that identifies the device</param>
        /// <returns>Http status</returns>
        [Route("deviceOffline")]
        [HttpPost]
        public async Task<ActionResult> DeviceOffline([FromBody] DeviceTokenDto devTok)
        {
            if (devTok == null || string.IsNullOrWhiteSpace(devTok.deviceToken))
            {
                this._logger.LogError("InternalCommsController_DevOff_NoToken");
                return BadRequest();
            }

            DeviceToken dt = this._context.DeviceTokens.FirstOrDefault(d => d.Token == devTok.deviceToken);

            if (dt == null)
            {
                this._logger.LogError("InternalCommsController_DevOff_NoSuchDevice");
                return StatusCode(500, "No User for this device token");
            }

            dt.IsDeviceOnline = false;
            dt.DeviceEvents.Add(new DeviceEvent(DateTime.UtcNow, false));
            await this._context.SaveChangesAsync();

            User usr = this._context.Users.FirstOrDefault(u => u.Id == dt.UserId);

            if (usr == null)
            {
                this._logger.LogError("InternalCommsController_DevOff_NoUserForDeviceToken");
                return StatusCode(500, "No User for this device token");
            }

            this._pushNotificationService.BroadcastDeviceOfflineNotification(usr, dt.Token);
            return Ok();
        }

        /// <summary>
        /// The DeviceOnline operation.null It sets the device as online in the db and sends a push notification to the app
        /// </summary>
        /// <param name="devTok">The device token that identifies the device</param>
        /// <returns>Http status</returns>
        [Route("deviceOnline")]
        [HttpPost]
        public async Task<ActionResult> DeviceOnline([FromBody] DeviceTokenDto devTok)
        {
            if (devTok == null || string.IsNullOrWhiteSpace(devTok.deviceToken))
            {
                this._logger.LogError("InternalCommsController_DevOn_NoToken");
                return BadRequest();
            }

            DeviceToken dt = this._context.DeviceTokens.FirstOrDefault(d => d.Token == devTok.deviceToken);

            if (dt == null)
            {
                this._logger.LogError("InternalCommsController_DevOff_NoSuchDevice");
                return StatusCode(500);
            }

            dt.IsDeviceOnline = true;
            dt.DeviceEvents.Add(new DeviceEvent(DateTime.UtcNow, true));
            await this._context.SaveChangesAsync();
            return Ok();
        }

    }
}