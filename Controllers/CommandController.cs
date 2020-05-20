using System;
using System.Collections.Generic;
using System.Linq;
using api.Dtos;
using api.Models;
using api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace api.Controllers
{
    /// <summary>
    /// Controller used to command the device
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CommandController : ControllerBase
    {

        private readonly IDispatcher _dispatcher;
        private readonly Db _context;

        private readonly UserManager<User> _userManager;

        private readonly ILogger<CommandController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dispatcher">The dispatcher used to communicate internally</param>
        /// <param name="context">DB context</param>
        /// <param name="userManager">The user manager</param>
        /// <param name="logger">The logger</param>
        public CommandController(IDispatcher dispatcher, Db context, UserManager<User> userManager, ILogger<CommandController> logger)
        {
            this._dispatcher = dispatcher;
            this._context = context;
            this._userManager = userManager;
            this._logger = logger;
        }

        /// <summary>
        /// Operation used to command the device to begin watering
        /// </summary>
        /// <param name="dto"></param>
        [Route("waterNow")]
        [HttpPost]
        public void WaterNow([FromBody] WaterNowDto dto)
        {
            this._logger.LogInformation("CommandControllerWaterNow");
            if (dto == null)
            {
                throw new Exception("Could not parse the object");
            }

            User usr = this._context.Users
                .Include(u => u.DeviceTokens)
                .Include(u => u.WateringSchedules)
                .FirstOrDefault(u => u.Id == Misc.GetIdFromClaimsPrincipal(User));

            if (usr != null)
            {
                string deviceToken = usr.DeviceTokens.Count > 0 ? usr.DeviceTokens[0].Token : null;
                if (deviceToken == null)
                {
                    this._logger.LogError("Command_NoToken", "User: {0}", usr.Id);
                    return;
                    // TODO: return some error to the customer so they know it's bad
                }

                this._dispatcher.WaterNow(this, new WaterNowArgs { WaterNow = dto, Token = deviceToken });

                ScheduledRun newRun = new ScheduledRun { Id = Guid.NewGuid(), Duration = dto.duration, Level = dto.level, StartTime = DateTime.Now };
                usr.ScheduledRuns.Add(newRun);
                WateringScheduleItem wateringSchedule = usr.WateringSchedules.FirstOrDefault(ws => ws.Level == dto.level);
                if (wateringSchedule != null)
                {
                    wateringSchedule.LastRun = newRun;
                }

                this._context.SaveChanges();
            }
            else
            {
                // TODO: the user doesn't exist. There's something wrong with Identity.
                // tell the user to try again, then log out/log in, then email us if none of this works
            }

        }

        /// <summary>
        /// Operation used to command the device to cancel the current watering process
        /// </summary>
        /// <param name="level"></param>
        [Route("cancelWatering")]
        [HttpPost]
        public void CancelWatering([FromBody] Level level)
        {
            this._logger.LogInformation("CommandControllerCancelWatering");

            User usr = this._context.Users
            .Include(u => u.DeviceTokens)
            .Include(u => u.WateringSchedules)
            .FirstOrDefault(u => u.Id == Misc.GetIdFromClaimsPrincipal(User));

            if (usr != null)
            {
                string deviceToken = usr.DeviceTokens.Count > 0 ? usr.DeviceTokens[0].Token : null;
                if (deviceToken == null)
                {
                    this._logger.LogError("Command_NoToken", "User: {0}", usr.Id);
                    return;
                    // TODO: return some error to the customer so they know it's bad
                }

                this._dispatcher.CancelWatering(this, new CancelWateringArgs { Level = level, Token = deviceToken });

                WateringScheduleItem wateringSchedule = usr.WateringSchedules.FirstOrDefault(ws => ws.Level == level);
                if (wateringSchedule != null)
                {
                    if (wateringSchedule.LastRun != null)
                    {
                        DateTime endTime = wateringSchedule.LastRun.StartTime.AddSeconds(wateringSchedule.LastRun.Duration);
                        if (endTime > DateTime.UtcNow)
                        {
                            wateringSchedule.LastRun.Duration = (int)(endTime - DateTime.UtcNow).TotalSeconds;
                        }
                    }
                    this._context.SaveChanges();
                }
            }
            else
            {
                // TODO: the user doesn't exist. There's something wrong with Identity.
                // tell the user to try again, then log out/log in, then email us if none of this works
            }

        }

        /// <summary>
        /// Get the history of all the waterings for the user/device
        /// </summary>
        /// <returns></returns>
        [Route("getRunHistory")]
        [HttpGet]
        public List<WateringHistoryDto> GetRunHistory()
        {
            User user = this._context.Users.Include(u => u.ScheduledRuns).FirstOrDefault(u => u.Id == Misc.GetIdFromClaimsPrincipal(User));

            List<WateringHistoryDto> historyItems;
            historyItems = new List<WateringHistoryDto>();
            user.ScheduledRuns.ForEach((scheduledRun) => { historyItems.Add(new WateringHistoryDto(scheduledRun)); });

            return historyItems;
        }

        /// <summary>
        /// Updates a schedule with new values
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Route("updateSchedule")]
        [HttpPost]
        public bool UpdateSchedule([FromBody] WateringScheduleDto dto)
        {
            if (dto == null)
            {
                throw new Exception("Could not parse object");
            }

            WateringScheduleItem tempItem = new WateringScheduleItem
            {
                Id = Guid.NewGuid(),
                Duration = dto.waterNow.duration,
                Level = dto.waterNow.level,
                WateringDays = dto.wateringDays,
                StartTime = dto.startTime,
                NotifiyBeforeMinutes = dto.notifyBeforeMinutes,
                IsEnabled = dto.isEnabled
            };

            User usr = this._context.Users.Include(u => u.WateringSchedules).FirstOrDefault(u => u.Id == Misc.GetIdFromClaimsPrincipal(User));

            WateringScheduleItem schedule = usr.WateringSchedules.FirstOrDefault(w => w.Level == dto.waterNow.level);

            if (schedule == null)
            {
                // this is the first time the schedule is set. 
                usr.WateringSchedules.Add(tempItem);
            }
            else
            {
                schedule.Duration = tempItem.Duration;
                schedule.WateringDays = tempItem.WateringDays;
                schedule.StartTime = tempItem.StartTime;
                schedule.NotifiyBeforeMinutes = tempItem.NotifiyBeforeMinutes;
                schedule.IsEnabled = tempItem.IsEnabled;
            }

            this._context.SaveChanges();

            return true;
        }

        /// <summary>
        /// Gets all the schedules for the device
        /// </summary>
        /// <returns></returns>
        [Route("getSchedules")]
        [HttpGet]
        public List<WateringScheduleDto> GetSchedules()
        {
            User user = this._context.Users.Include(u => u.WateringSchedules).FirstOrDefault(u => u.Id == Misc.GetIdFromClaimsPrincipal(User));

            List<WateringScheduleDto> schedules = new List<WateringScheduleDto>();
            if (user != null)
            {
                user.WateringSchedules.ForEach((schedule) => { schedules.Add(new WateringScheduleDto(schedule)); });
            }


            return schedules;
        }

        /// <summary>
        /// Enables or disables a schedule
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        [Route("toggleSchedule")]
        [HttpPost]
        public bool ToggleSchedule([FromBody] Level level)
        {
            User user = this._context.Users.Include(u => u.WateringSchedules).FirstOrDefault(u => u.Id == Misc.GetIdFromClaimsPrincipal(User));

            WateringScheduleItem schedule = user.WateringSchedules.FirstOrDefault((sc) => { return sc.Level == level; });
            if (schedule != null)
            {
                schedule.IsEnabled = !schedule.IsEnabled;
                this._context.SaveChanges();
                return schedule.IsEnabled;
            }

            throw new Exception("Trying to update a schedule that doesn't exist");
        }

        /// <summary>
        /// Gets the online/offline status of the device
        /// </summary>
        /// <param name="deviceToken"></param>
        /// <returns></returns>
        [Route("getDeviceStatus")]
        [HttpPost]
        public DeviceStatusDto GetDeviceStatus([FromBody] string deviceToken)
        {
            // TODO SECURITY ISSUE: Make sure the user for the device and the currently logged in user are the same.
            User user = this._context.Users.Include(u => u.DeviceTokens).FirstOrDefault(u => u.Id == Misc.GetIdFromClaimsPrincipal(User));

            if (user.Email == "appletest@meridiatech.com")
            {
                return new DeviceStatusDto
                {
                    deviceToken = "7onpybsPelfV3Q==",
                    isOnline = true,
                    changeOfStatus = DateTime.Now
                };
            }

            DeviceToken device = user.DeviceTokens.FirstOrDefault((dv) => { return dv.Token == deviceToken; });
            if (device != null)
            {
                bool isOnline = device.IsDeviceOnline ?? false;
                DeviceEvent lastChangeOfStatus = device.DeviceEvents.LastOrDefault();
                DateTime? changeOfStatus;
                if (lastChangeOfStatus == null)
                {
                    changeOfStatus = null;
                }
                else
                {
                    changeOfStatus = lastChangeOfStatus.EventDate;
                }
                return new DeviceStatusDto
                {
                    deviceToken = device.Token,
                    isOnline = isOnline,
                    changeOfStatus = changeOfStatus
                };
            }

            this._logger.LogError("DeviceStatus_NoDevice", "User: {0}, device Token: {1}", user.Id, device.Token);
            throw new Exception("No device with this deviceId found for user");
        }

        /// <summary>
        /// Gets the online/offline history for this device
        /// </summary>
        /// <param name="deviceToken"></param>
        /// <returns></returns>
        [Route("getAllHistoryForDevice")]
        [HttpPost]
        public List<DeviceStatusDto> GetAllHistoryForDevice([FromBody] string deviceToken)
        {
            // TODO SECURITY ISSUE: Make sure the user for the device and the currently logged in user are the same.
            User user = this._context.Users.Include(u => u.DeviceTokens).ThenInclude(dt => dt.DeviceEvents).FirstOrDefault(u => u.Id == Misc.GetIdFromClaimsPrincipal(User));

            DeviceToken device = user.DeviceTokens.FirstOrDefault((dv) => { return dv.Token == deviceToken; });
            if (device != null)
            {
                List<DeviceStatusDto> statuses = new List<DeviceStatusDto>();
                foreach (DeviceEvent de in device.DeviceEvents)
                {
                    if (de.Device == null)
                    {
                        continue;
                    }
                    statuses.Add(new DeviceStatusDto { changeOfStatus = de.EventDate, deviceToken = device.Token, isOnline = de.IsOnline });
                }

                return statuses.OrderByDescending(status => status.changeOfStatus).ToList();
            }

            this._logger.LogError("DeviceStatus_NoDevice", "User: {0}, device Token: {1}", user.Id, device.Token);
            throw new Exception("No device with this deviceId found for user");
        }

        /// <summary>
        /// Gets the news for this user. This is deprecated
        /// </summary>
        /// <returns></returns>
        [Route("getNews")]
        [HttpGet]
        public string GetNews()
        {
            List<NewsItemDto> newsItems = new List<NewsItemDto>();

            newsItems.Add(new NewsItemDto { contentHtml = "When to water", title = "When to water", synopsis = "For all those wannabe gardeners, or those who are time-poor, find out which plants are made for you with our Top 10 Low Maintenance Plants guide", coverImageUrl = "https://www.sciencenews.org/sites/default/files/2018/09/main/articles/092918_book_plants_feat_free.jpg" });
            newsItems.Add(new NewsItemDto { contentHtml = "Pests. Really?", title = "Pests. Really?", synopsis = "The Pothos plant is the perfect first indoor plant. It has an air-purifying quality that absorbs and strips toxins from materials such as carpet, so if you allergic to dust mites, this is your go to!", coverImageUrl = "https://images-na.ssl-images-amazon.com/images/I/71Bl8JbkPEL._SL1000_.jpg" });
            newsItems.Add(new NewsItemDto { contentHtml = "The Aloe plant", title = "The Aloe plant", synopsis = "The aloe plant has multiple uses, including being a nice touch in your home. This succulent is best known for its medicinal properties to remedy skin condition such as sunburn, psoriasis and rashes.", coverImageUrl = "https://amp.businessinsider.com/images/5a1d8f4d3dbef4ae078b893e-960-636.jpg" });

            return JsonConvert.SerializeObject(newsItems);
        }
    }
}
