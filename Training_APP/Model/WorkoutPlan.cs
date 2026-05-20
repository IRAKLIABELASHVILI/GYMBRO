using System;

namespace Training_APP.Model
{
    public class WorkoutPlan
    {
        public int Id { get; set; }
        public string PlanText { get; set; } = "";
        public string Location { get; set; } = "";
        public string Goal { get; set; } = "";
        public int DaysPerWeek { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
}
