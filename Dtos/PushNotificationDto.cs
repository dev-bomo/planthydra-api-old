namespace api.Dtos
{
    /// <summary>
    /// The DTO used for expo push notifications
    /// </summary>
    public class PushNotificationDto
    {
        /// <summary>
        /// The title of the notification
        /// </summary>
        /// <value></value>
        public string title {get;set;}

        /// <summary>
        /// The body of the notification
        /// </summary>
        /// <value></value>
        public string body {get;set;}

        /// <summary>
        /// The sound the notification makes on the user device.await This should be an enum at some point
        /// </summary>
        /// <value></value>
        public string sound {get;set;}
    }
}