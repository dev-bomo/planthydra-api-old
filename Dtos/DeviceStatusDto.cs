using System;

namespace api.Dtos
{
    public class DeviceStatusDto
    {
        public string deviceToken { get; set; }
        public bool isOnline { get; set; }

        public DateTime? changeOfStatus { get; set; }
    }
}