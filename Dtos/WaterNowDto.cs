namespace api.Dtos
{
    public enum Level
    {
        Upper = 1,
        Lower = 2
    }

    public class WaterNowDto
    {
        public Level level { get; set; }

        public int duration { get; set; }
    }

    public class WaterNowUserDto
    {
        public WaterNowDto waterNow { get; set; }
        public UserDto user { get; set; }
    }
}