using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class DeviceEvent
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        public DeviceToken Device { get; set; }
        public DateTime EventDate { get; set; }
        public bool IsOnline { get; set; }

        public DeviceEvent(DateTime eventDate, bool isOnline)
        {
            EventDate = eventDate;
            IsOnline = isOnline;
        }
    }
}