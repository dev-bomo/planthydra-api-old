using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace api.Controllers
{
    /// <summary>
    /// A very temporary DTO for sensor data
    /// </summary>
    public class CurrentSensorDataDto
    {
        /// <summary>
        /// The data from the light sensor
        /// </summary>
        /// <value></value>
        public SensorData lightDataPoint { get; set; }

        /// <summary>
        /// The data from the temperature sensor
        /// </summary>
        /// <value></value>
        public SensorData tempDataPoint { get; set; }

        /// <summary>
        /// The data from the humidity sensor
        /// </summary>
        /// <value></value>
        public SensorData humidityDataPoint { get; set; }
    }

    /// <summary>
    /// Controller used to interact with the data produced by the sensors
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SensorDataController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;

        private readonly ILogger<SensorDataController> _logger;

        private readonly Db _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        public SensorDataController(IDispatcher dispatcher, ILogger<SensorDataController> logger, Db context)
        {
            this._dispatcher = dispatcher;
            this._logger = logger;
            this._context = context;
        }

        /// <summary>
        /// Gets the current sensor info from the device. This is done in real time
        /// </summary>
        /// <param name="deviceToken"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("getCurrentSensorInfo")]
        public ActionResult<CurrentSensorDataDto> GetCurrentSensorInfo([FromBody] string deviceToken)
        {
            // emit a message on the dispatcher and get the data from the device
            // in the mean time just send back some mock data

            DeviceToken dt = this._context.DeviceTokens.FirstOrDefault(d => d.Token == deviceToken);

            if (this.IsValidDeviceToken(dt) == false)
            {
                return BadRequest();
            }

            return new CurrentSensorDataDto
            {
                humidityDataPoint = new SensorData
                {
                    Device = dt,
                    EventTime = DateTime.Now,
                    Id = 1,
                    Value = 200
                },
                lightDataPoint = new SensorData
                {
                    Device = dt,
                    EventTime = DateTime.Now,
                    Id = 1,
                    Value = 800
                },
                tempDataPoint = new SensorData
                {
                    Device = dt,
                    EventTime = DateTime.Now,
                    Id = 1,
                    Value = 19
                }
            };
        }

        /// <summary>
        /// Gets the last 7 day's worth of hourly light data
        /// </summary>
        /// <param name="deviceToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getHourlyLightHistory")]
        public ActionResult<List<SensorData>> GetHourlyLightHistory([FromBody] string deviceToken)
        {
            DeviceToken dt = this._context.DeviceTokens.FirstOrDefault(d => d.Token == deviceToken);

            if (this.IsValidDeviceToken(dt) == false)
            {
                return BadRequest();
            }


            return new List<SensorData>(){
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-2),
                    Id = 1,
                    Value = 200
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-3),
                    Id = 2,
                    Value = 210
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-4),
                    Id = 3,
                    Value = 305
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-5),
                    Id = 4,
                    Value = 310
                },
                                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-6),
                    Id = 1,
                    Value = 200
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-7),
                    Id = 2,
                    Value = 210
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-8),
                    Id = 3,
                    Value = 305
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-9),
                    Id = 4,
                    Value = 310
                },
                                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-10),
                    Id = 1,
                    Value = 200
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-11),
                    Id = 2,
                    Value = 210
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-12),
                    Id = 3,
                    Value = 305
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-13),
                    Id = 4,
                    Value = 310
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-14),
                    Id = 1,
                    Value = 200
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-15),
                    Id = 2,
                    Value = 210
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-16),
                    Id = 3,
                    Value = 305
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-17),
                    Id = 4,
                    Value = 310
                },
                                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-18),
                    Id = 1,
                    Value = 200
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-19),
                    Id = 2,
                    Value = 210
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-20),
                    Id = 3,
                    Value = 305
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-21),
                    Id = 4,
                    Value = 310
                }
            };
        }

        /// <summary>
        /// Get the last 7 months of aggregate light data
        /// </summary>
        /// <param name="deviceToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getLongTermLightHistory")]
        public ActionResult<List<SensorData>> GetLongTermLightHistory([FromBody] string deviceToken)
        {
            DeviceToken dt = this._context.DeviceTokens.FirstOrDefault(d => d.Token == deviceToken);

            if (this.IsValidDeviceToken(dt) == false)
            {
                return BadRequest();
            }

            return new List<SensorData>(){
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddDays(-7),
                    Id = 1,
                    IsAggregate = true,
                    Value = 190
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddDays(-8),
                    Id = 2,
                    IsAggregate = true,
                    Value = 210
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddDays(-9),
                    Id = 3,
                    IsAggregate = true,
                    Value = 250
                }
            };
        }

        /// <summary>
        /// Get the last 7 day's worth of hourly temperature data
        /// </summary>
        /// <param name="deviceToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getHourlyTempHistory")]
        public ActionResult<List<SensorData>> GetHourlyTempHistory([FromBody] string deviceToken)
        {
            DeviceToken dt = this._context.DeviceTokens.FirstOrDefault(d => d.Token == deviceToken);

            if (this.IsValidDeviceToken(dt) == false)
            {
                return BadRequest();
            }

            return new List<SensorData>(){
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-2),
                    Id = 1,
                    Value = 20
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-3),
                    Id = 2,
                    Value = 21
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-4),
                    Id = 3,
                    Value = 20
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-5),
                    Id = 4,
                    Value = 30
                }
            };
        }

        /// <summary>
        /// Get the last 7 months of aggregate temperature data
        /// /// </summary>
        /// <param name="deviceToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getLongTermTempHistory")]
        public ActionResult<List<SensorData>> GetLongTermTempHistory([FromBody] string deviceToken)
        {
            DeviceToken dt = this._context.DeviceTokens.FirstOrDefault(d => d.Token == deviceToken);

            if (this.IsValidDeviceToken(dt) == false)
            {
                return BadRequest();
            }

            return new List<SensorData>(){
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddDays(-7),
                    Id = 1,
                    IsAggregate = true,
                    Value = 19
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddDays(-8),
                    Id = 2,
                    IsAggregate = true,
                    Value = 21
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddDays(-9),
                    Id = 3,
                    IsAggregate = true,
                    Value = 25
                }
            };
        }

        /// <summary>
        /// Get the last 7 day's worth of hourly humidity data
        /// </summary>
        /// <param name="deviceToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getHourlyHumidityHistory")]
        public ActionResult<List<SensorData>> GetHourlyHumidityHistory([FromBody] string deviceToken)
        {
            DeviceToken dt = this._context.DeviceTokens.FirstOrDefault(d => d.Token == deviceToken);

            if (this.IsValidDeviceToken(dt) == false)
            {
                return BadRequest();
            }

            return new List<SensorData>(){
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-2),
                    Id = 1,
                    Value = 20
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-3),
                    Id = 2,
                    Value = 24
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-4),
                    Id = 3,
                    Value = 22
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddHours(-5),
                    Id = 4,
                    Value = 34
                }
            };
        }

        /// <summary>
        /// Get the last 7 months of aggregate humidity data
        /// /// </summary>
        /// <param name="deviceToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getLongTermHumidityHistory")]
        public ActionResult<List<SensorData>> GetLongTermHumidityHistory([FromBody] string deviceToken)
        {
            DeviceToken dt = this._context.DeviceTokens.FirstOrDefault(d => d.Token == deviceToken);

            if (this.IsValidDeviceToken(dt) == false)
            {
                return BadRequest();
            }

            return new List<SensorData>(){
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddDays(-7),
                    Id = 1,
                    IsAggregate = true,
                    Value = 20
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddDays(-8),
                    Id = 2,
                    IsAggregate = true,
                    Value = 24
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddDays(-9),
                    Id = 3,
                    IsAggregate = true,
                    Value = 22
                },
                new SensorData{
                    Device = dt,
                    EventTime = DateTime.Now.AddDays(-11),
                    Id = 4,
                    IsAggregate = true,
                    Value = 34
                }
            };
        }

        private bool IsValidDeviceToken(DeviceToken dt)
        {
            if (dt == null)
            {
                //TODO: Don't forget to log stuff
                return false;
            }

            User usr = this._context.Users
                .FirstOrDefault(u => u.Id == Misc.GetIdFromClaimsPrincipal(User));

            if (usr.Id != dt.UserId)
            {
                return false; // should this be a bad request? Should this be a not found?
            }

            return true;
        }
    }
}