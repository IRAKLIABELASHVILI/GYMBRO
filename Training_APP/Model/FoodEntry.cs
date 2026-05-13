using System;
using System.Collections.Generic;
using System.Text;

namespace Training_APP.Model
{
    public class FoodEntry
    {
        public int Id { get; set; }
        public string FoodName { get; set; }
        public string MealType { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
        public string ImagePath { get; set; }

        // კალორიები
        public int Calories { get; set; }

        // მაკრო (გრამები)
        public double Protein { get; set; }
        public double Carbohydrates { get; set; }
        public double Fats { get; set; }
        public double Fiber { get; set; }
        public double Sugar { get; set; }

        // მიკრო
        public double Sodium { get; set; }
        public double Calcium { get; set; }
        public double Iron { get; set; }
        public double VitaminC { get; set; }
        public double VitaminD { get; set; }

        // წყალი
        public double Water { get; set; }
    }
}
