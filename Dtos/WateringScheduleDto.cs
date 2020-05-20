using api.Models;

namespace api.Dtos
{
    public class WateringScheduleDto
    {
        public WaterNowDto waterNow { get; set; }
        public string startTime { get; set; }
        public int notifyBeforeMinutes { get; set; }
        public string wateringDays { get; set; }

        public bool isEnabled { get; set; }

        public WateringScheduleDto()
        {

        }

        public WateringScheduleDto(WateringScheduleItem item)
        {
            this.waterNow = new WaterNowDto { duration = item.Duration, level = item.Level };
            this.startTime = item.StartTime;
            this.notifyBeforeMinutes = item.NotifiyBeforeMinutes;
            this.wateringDays = item.WateringDays;
            this.isEnabled = item.IsEnabled;
        }
    }

    public class WateringScheduleUserDto
    {
        public WateringScheduleDto wateringSchedule { get; set; }
        public UserDto user { get; set; }
    }
}