namespace Feast.Models
{
    public class UpdateMealTimesModel
    {
        public TimeSpan BreakfastTime { get; set; }
        public TimeSpan LunchTime { get; set; }
        public TimeSpan DinnerTime { get; set; }
    }
}