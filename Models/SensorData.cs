using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace api.Models
{
    /// <summary>
    /// Object containing a data point from a device mounted sensor
    /// </summary>
    public class SensorData
    {
        /// <summary>
        /// The id
        /// </summary>
        /// <value></value>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// The time the data point was captured
        /// </summary>
        /// <value></value>
        public DateTime EventTime { get; set; }

        /// <summary>
        /// The value of the data point in its metric
        /// </summary>
        /// <value></value>
        public float Value { get; set; }

        /// <summary>
        /// True if this data point is an aggregate, like a daily average.
        /// False if not specified
        /// </summary>
        /// <value></value>
        public bool? IsAggregate { get; set; }

        /// <summary>
        /// The device the data point was captured on
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public DeviceToken Device { get; set; }
    }
}