using System;
using api.Models;

namespace api.Dtos
{
    public class WateringHistoryDto
    {
        public Guid id { get; set; }
        public WaterNowDto waterNow { get; set; }
        public DateTime startTime { get; set; }

        public WateringHistoryDto()
        {

        }

        public WateringHistoryDto(ScheduledRun run)
        {
            this.id = run.Id;
            this.waterNow = new WaterNowDto { duration = run.Duration, level = run.Level };
            this.startTime = run.StartTime;
        }
    }
}