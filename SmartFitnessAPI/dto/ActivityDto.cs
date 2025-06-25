namespace SmartFitnessApi.Models
{
    public class ActivityDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public TimeSpan Duration { get; set; }
        public int CaloriesBurned { get; set; }
        public DateTime Date { get; set; }
    }
}