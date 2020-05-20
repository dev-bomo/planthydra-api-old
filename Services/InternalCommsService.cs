using System;
using System.Net;
using api.Dtos;
using api.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace api.Services
{

    internal class WaterNowDto
    {
        public string deviceToken { get; set; }
        public int duration { get; set; }
        public Level level { get; set; }
    }

    internal class CancelWateringDto
    {
        public string deviceToken { get; set; }

        public Level level { get; set; }
    }

    public interface IInternalCommsService
    {
    }

    public class InternalCommsService : IInternalCommsService
    {

        private readonly ILogger<InternalCommsService> _logger;

        private readonly IDispatcher _dispatcher;

        private readonly string _nodeServiceUrl = "http://localhost:3000";

        public InternalCommsService(IDispatcher dispatcher, ILogger<InternalCommsService> logger)
        {

            this._dispatcher = dispatcher;
            this._logger = logger;

            this._dispatcher.WaterNowEvent += WaterNow;
            this._dispatcher.CancelWateringEvent += CancelWatering;
        }

        private void WaterNow(object sender, WaterNowArgs e)
        {
            if (String.IsNullOrWhiteSpace(e.Token))
            {
                this._logger.LogError("InternalCommsService_WaterNow_NoToken");
                return;
            }

            using (WebClient client = new WebClient())
            {
                client.Headers.Add("accept", "application/json");
                client.Headers.Add("accept-encoding", "gzip, deflate");
                client.Headers.Add("Content-Type", "application/json");

                string path = "/waterNow";

                try
                {
                    string response = client.UploadString(this._nodeServiceUrl + path, JsonConvert.SerializeObject(new WaterNowDto
                    {
                        deviceToken = e.Token,
                        duration = e.WaterNow.duration,
                        level = e.WaterNow.level
                    }));
                }
                catch (Exception ex)
                {
                    this._logger.LogError("InternalCommsService_WaterNow_Exception", ex);
                }
            }
        }

        private void CancelWatering(object sender, CancelWateringArgs e)
        {
            if (String.IsNullOrWhiteSpace(e.Token))
            {
                this._logger.LogError("InternalCommsService_CancelWatering_NoToken");
                return;
            }

            using (WebClient client = new WebClient())
            {
                client.Headers.Add("accept", "application/json");
                client.Headers.Add("accept-encoding", "gzip, deflate");
                client.Headers.Add("Content-Type", "application/json");

                string path = "/cancelWatering";

                try
                {
                    string response = client.UploadString(this._nodeServiceUrl + path, JsonConvert.SerializeObject(new CancelWateringDto
                    {
                        deviceToken = e.Token,
                        level = e.Level
                    }));
                }
                catch (Exception ex)
                {
                    this._logger.LogError("InternalCommsService_CancelWatering_Exception", ex);
                }
            }
        }
    }
}