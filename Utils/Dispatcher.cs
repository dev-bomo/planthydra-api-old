using System;
using api.Dtos;
using api.Models;
using Microsoft.Extensions.Logging;

namespace api.Utils
{
    /// <summary>
    /// Dispatcher handling the messaging between components
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// Water now event
        /// </summary>
        event EventHandler<WaterNowArgs> WaterNowEvent;
        /// <summary>
        /// Cancel watering event
        /// </summary>
        event EventHandler<CancelWateringArgs> CancelWateringEvent;

        /// <summary>
        /// water now handler
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        void WaterNow(object source, WaterNowArgs args);

        /// <summary>
        /// cancel watering handler
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        void CancelWatering(object source, CancelWateringArgs args);
    }

    /// <summary>
    /// Water now event args
    /// </summary>
    public class WaterNowArgs : EventArgs
    {
        /// <summary>
        /// The dto containing the parameters for the watering
        /// </summary>
        /// <value></value>
        public WaterNowDto WaterNow { get; set; }

        /// <summary>
        /// The device token
        /// </summary>
        /// <value></value>
        public string Token { get; set; }
    }

    /// <summary>
    /// The cancel watering event args
    /// </summary>
    public class CancelWateringArgs : EventArgs
    {
        /// <summary>
        /// Options
        /// </summary>
        /// <value></value>
        public Level Level { get; set; }
        /// <summary>
        /// The device token
        /// </summary>
        /// <value></value>
        public string Token { get; set; }
    }

    /// <summary>
    /// Dispatcher handling the messaging between components
    /// </summary>
    public class Dispatcher : IDispatcher
    {
        /// <summary>
        /// Water now event
        /// </summary>
        public event EventHandler<WaterNowArgs> WaterNowEvent;

        /// <summary>
        /// Cancel watering event
        /// </summary>
        public event EventHandler<CancelWateringArgs> CancelWateringEvent;

        private readonly ILogger<Dispatcher> _logger;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        public Dispatcher(ILogger<Dispatcher> logger)
        {
            this._logger = logger;
        }

        /// <summary>
        /// water now handler
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        public void WaterNow(object source, WaterNowArgs args)
        {
            if (this.WaterNowEvent != null)
            {
                this.WaterNowEvent(source, args);
            }
            else
            {
                this._logger.LogInformation("Dispatcher_WaterNow_NoDevice");
            }
        }

        /// <summary>
        /// cancel watering handler
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        public void CancelWatering(object source, CancelWateringArgs args)
        {
            if (this.CancelWateringEvent != null)
            {
                this.CancelWateringEvent(source, args);
            }
            else
            {
                this._logger.LogInformation("Dispatcher_CancelWatering_NoDevice");
            }
        }
    }
}