using System;
using api.Dtos;

namespace api.Models
{
    public class WateringScheduleItem
    {
        public Guid Id { get; set; }
        public Level Level { get; set; }
        public string StartTime { get; set; }
        public int Duration { get; set; }
        public int NotifiyBeforeMinutes { get; set; }

        //A string representation of the days of the week when it should water. Ex '024' means Su, Tu, Th
        public string WateringDays { get; set; }
        public ScheduledRun LastRun { get; set; }
        public User User { get; set; }

        public bool IsEnabled { get; set; }
    }
}