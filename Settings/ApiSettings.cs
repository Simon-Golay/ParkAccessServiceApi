namespace ParkAccessServiceApi.Settings
{
    public class ApiSettings
    {
        public string ApiKey { get; set; } = "";
        public string BaseUrl { get; set; } = "";
        public int IntervalToGetEventsFromCalendarInSeconds { get; set; } = 1;
        public int IntervalToControllIncomingEventInSeconds { get; set; } = 1;
        public int TolerenceToDetectEventInSeconds { get; set; } = 20;
    }
}
