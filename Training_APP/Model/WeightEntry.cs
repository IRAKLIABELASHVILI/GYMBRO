using System;

namespace Training_APP.Model
{
    public class WeightEntry
    {
        public int Id { get; set; }
        public double WeightKg { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
        public string? Note { get; set; }
    }
}
