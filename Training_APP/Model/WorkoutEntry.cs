using System;
using System.Collections.Generic;
using System.Text;

namespace Training_APP.Model
{
    public class WorkoutEntry
    {
        public int Id { get; set; }
        public string WorkoutName { get; set; } = "";
        public string? WorkoutType { get; set; }
        public int DurationMinutes { get; set; }
        public int CaloriesBurned { get; set; }
        public string? Intensity { get; set; }
        public string? Notes { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;


    }
}
