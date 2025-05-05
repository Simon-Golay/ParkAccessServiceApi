namespace ParkAccessServiceApi.Class
{
    public class History
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }

        public History(DateTime date, string description)
        {
            Date = date;
            Description = description;
        }
    }
}
