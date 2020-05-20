using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using api.Models;
using api.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace api.Services
{
    public class HourMinuteTime
    {
        public int Hour { get; set; }
        public int Minute { get; set; }
    }

    public class ScheduledWateringService : BackgroundService
    {
        private readonly Db _dbContext;
        private readonly ILogger<ScheduledWateringService> _logger;

        private readonly IDispatcher _dispatcher;

        public ScheduledWateringService(Db context, ILogger<ScheduledWateringService> logger, IDispatcher dispatcher)
        {
            this._dbContext = context;
            this._logger = logger;
            this._dispatcher = dispatcher;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                List<WateringScheduleItem> scheduleItems = this.GetWateringSchedulesToRun();
                DateTime now = DateTime.UtcNow;
                this._logger.LogInformation("ScheduledWateringItemCount", "{0} waterings at {1}", scheduleItems.Count, now);
                // Go through each item and add an event on the dispatcher
                // log each item in a meaningful way
                scheduleItems.ForEach((item) =>
                {
                    DeviceToken token = item.User.DeviceTokens.FirstOrDefault();
                    if (token != null)
                    {
                        this._dispatcher.WaterNow(this, new WaterNowArgs { Token = token.Token, WaterNow = new Dtos.WaterNowDto { level = item.Level, duration = item.Duration } });
                        item.LastRun = new ScheduledRun { Level = item.Level, Duration = item.Duration, StartTime = now, User = item.User, Id = Guid.NewGuid() };
                        this._dbContext.SaveChanges();
                        this._logger.LogInformation("ScheduledWatering", "{0} at {1}", item.StartTime, now);
                    }
                    else
                    {
                        this._logger.LogError("ScheduledWatering_NoToken", "User: {0}", item.User.Id);
                        // TODO: should the user be informed of this? maybe not right now
                    }

                });

                await Task.Delay(1000 * 10, stoppingToken);
            }
        }

        private List<WateringScheduleItem> GetWateringSchedulesToRun()
        {
            // all items that have the difference between the last run time and the current time greater than the notifybeforetime
            // and that have the startTime hour and minute within the last 10 minutes
            List<WateringScheduleItem> items =
                this._dbContext.WateringSchedules
                    .Include(ws => ws.User).ThenInclude(us => us.DeviceTokens)
                    .Where(item => item.IsEnabled && this.HasNotBeenTriggered(item) && this.IsWithinTimeWindow(item))
                    .ToList();
            return items;
        }

        private bool IsWithinTimeWindow(WateringScheduleItem item)
        {
            DateTime now = DateTime.UtcNow;
            HourMinuteTime time = this.ExtractHour(item.StartTime);

            char dayOfWeekNumber = Char.Parse(((int)now.DayOfWeek).ToString());

            if (item.WateringDays.Contains(dayOfWeekNumber))
            {
                if (item.LastRun == null || now - item.LastRun.StartTime > new TimeSpan(0, 10, 0))
                {
                    DateTime transposedTime = now;
                    TimeSpan ts = new TimeSpan(time.Hour, time.Minute, 0);
                    transposedTime = transposedTime.Date + ts;

                    bool isWithinTimeWindow =
                        (now - transposedTime >= new TimeSpan(0) && now - transposedTime < new TimeSpan(0, 1, 0));

                    if (isWithinTimeWindow)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool HasNotBeenTriggered(WateringScheduleItem item)
        {
            // TODO: these operations may be slow when dealing with a lot of schedules. Find a better way to store them
            int notificationInterval = item.NotifiyBeforeMinutes;
            DateTime now = DateTime.UtcNow;
            ScheduledRun lastRun = item.LastRun;

            if (lastRun != null && (now - lastRun.StartTime).TotalMinutes > notificationInterval || lastRun == null)
            {
                return true;
            }

            return false;
        }

        private HourMinuteTime ExtractHour(string startTime)
        {
            string[] shards = startTime.Split(':');
            return new HourMinuteTime { Hour = Int32.Parse(shards[0]), Minute = Int32.Parse(shards[1]) };
        }
    }
}