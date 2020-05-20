using System;
using api.Dtos;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace api.Models
{

    public class ScheduledRun
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public Level Level { get; set; }

        [JsonIgnore]
        public User User { get; set; }
    }
}