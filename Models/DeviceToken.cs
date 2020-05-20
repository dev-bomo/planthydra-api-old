using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class DeviceToken
    {
        // TODO: add a way for the user to invalidate the generated tokens and set new ones for the devices
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public string Token { get; private set; }
        public bool? IsDeviceOnline { get; set; }
        public string UserId { get; private set; }

        public User User { get; set; }

        public List<DeviceEvent> DeviceEvents { get; set; }

        public DeviceToken(string token, string userId)
        {
            Token = token;
            UserId = userId;
            DeviceEvents = new List<DeviceEvent>();
            DeviceEvents.Add(new DeviceEvent(DateTime.UtcNow, false));
        }
    }
}
