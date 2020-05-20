using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using api.Dtos;
using api.Models;
using api.Services;
using api.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace api.Controllers
{

    /// <summary>
    /// THIS IS DEPRECATED. WILL DELETE SOON
    /// </summary>
    [Route("ws")]
    public class WsController : Controller
    {
        private IDispatcher _dispatcher;
        private WebSocket _webSocket;
        private string _token;
        private readonly Db _context;

        private readonly IPushNotificationService _pushNotificationService;

        private readonly ILogger<WsController> _logger;

        /// <summary>
        /// The websockets controller. Used to communicate with the ESP8266
        /// This functionality is obsolete as we transitioned to socket.IO
        /// </summary>
        /// <param name="dispatcher">The event dispatcher</param>
        /// <param name="logger">The logger</param>
        /// <param name="context">The DB context</param>
        /// <param name="pushNotificationService">The push notif service</param>
        public WsController(IDispatcher dispatcher,
        ILogger<WsController> logger,
        Db context,
        IPushNotificationService pushNotificationService)
        {
            this._dispatcher = dispatcher;
            this._dispatcher.WaterNowEvent += waterNow;
            this._dispatcher.CancelWateringEvent += cancelWatering;
            this._logger = logger;
            this._context = context;
            this._pushNotificationService = pushNotificationService;
        }

        // TODO: this is very bad, sending the token in the URL like this
        [HttpGet]
        public async Task GetAsync([FromQuery(Name = "tk")] string token)
        {
            this._logger.LogInformation("WsControllerGetAsyncCallBeingMade");

            this._token = token;
            if (this.HttpContext.WebSockets.IsWebSocketRequest)
            {
                // get the user with this token and set the IsDeviceOnline to true
                DeviceToken devTok = this._context.DeviceTokens
                    .Include(dt => dt.User).FirstOrDefault(dt => dt.Token == token);
                this._context.SaveChanges();
                this._logger.LogInformation("WsControllerGetAsync" + this._token + "::" + token);
                var buffer = new byte[1024 * 4];
                this._webSocket = await this.HttpContext.WebSockets.AcceptWebSocketAsync();
                this._logger.LogInformation("WsControllerGetAsyncProtocol" + this._token + "::" + this.HttpContext.WebSockets.WebSocketRequestedProtocols);
                try
                {
                    devTok.IsDeviceOnline = true;
                    devTok.DeviceEvents.Add(new DeviceEvent(DateTime.UtcNow, true));
                    while (this._webSocket != null && this._webSocket.State == WebSocketState.Open)
                    {
                        WebSocketReceiveResult response = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (response.MessageType == WebSocketMessageType.Close)
                            break;
                    }
                    this._pushNotificationService.BroadcastDeviceOfflineNotification(devTok.User, devTok.Token);
                    devTok.IsDeviceOnline = false;
                    devTok.DeviceEvents.Add(new DeviceEvent(DateTime.UtcNow, false));
                    this._context.SaveChanges();
                    await _webSocket.CloseAsync(WebSocketCloseStatus.Empty, "Closed_by_client", CancellationToken.None);
                }
                catch (WebSocketException ex)
                {
                    this._logger.LogError("WsControllerGetAsync:Error::" + ex.Message);
                    switch (ex.WebSocketErrorCode)
                    {
                        case WebSocketError.ConnectionClosedPrematurely:
                            this._pushNotificationService.BroadcastDeviceOfflineNotification(devTok.User, devTok.Token);
                            devTok.IsDeviceOnline = false;
                            devTok.DeviceEvents.Add(new DeviceEvent(DateTime.UtcNow, false));
                            this._context.SaveChanges();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void waterNow(object sender, WaterNowArgs args)
        {
            this._logger.LogInformation("WsControllerWaterNowToken", "Token {0}", args.Token);
            if (this._token == args.Token && this._webSocket != null && this._webSocket.State == WebSocketState.Open)
            {
                this._logger.LogInformation("WsControllerWaterNowArgs", "WaterNow arguments: {0}:{1}", (int)args.WaterNow.level, args.WaterNow.duration);
                var bytes = System.Text.Encoding.UTF8.GetBytes(String.Format("W:{0}:{1}", (int)args.WaterNow.level, args.WaterNow.duration));

                this._webSocket.SendAsync(new System.ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        private void cancelWatering(object sender, CancelWateringArgs args)
        {
            this._logger.LogInformation("WsControllerCancelWateringToken", "Token {0}", args.Token);
            if (this._token == args.Token && this._webSocket != null && this._webSocket.State == WebSocketState.Open)
            {
                this._logger.LogInformation("WsControllerCancelWateringArgs", "CancelWatering arguments: {0}", (int)args.Level);
                var bytes = System.Text.Encoding.UTF8.GetBytes(String.Format("C:{0}", (int)args.Level));

                this._webSocket.SendAsync(new System.ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}