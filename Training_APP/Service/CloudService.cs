using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Training_APP.Model;

namespace Training_APP.Service
{
    public class CloudService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        private const string API_URL = "https://api.anthropic.com/v1/messages";
        private const string MODEL = "claude-sonnet-4-6";

        // ================================
        // კონსტრუქტორი
        // ================================
        public CloudService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        }

        // ================================
        // 🥗 საჭმლის ფოტოს ანალიზი
        // ================================
        public async Task<(string foodName, int estimatedGrams)> AnalyzeFoodImageAsync(string imagePath)
        {
            // Detect media type — Claude API supports jpeg, png, gif, webp
            // BMP is not supported so we convert it to PNG in memory
            string ext = Path.GetExtension(imagePath).ToLowerInvariant();
            string mediaType;
            byte[] imageBytes;

            if (ext == ".bmp")
            {
                // Convert BMP → PNG using System.Drawing
                using var bmp = new System.Drawing.Bitmap(imagePath);
                using var ms  = new System.IO.MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                imageBytes = ms.ToArray();
                mediaType  = "image/png";
            }
            else
            {
                imageBytes = await File.ReadAllBytesAsync(imagePath);
                mediaType  = ext == ".png" ? "image/png" : "image/jpeg";
            }

            string base64Image = Convert.ToBase64String(imageBytes);

            string prompt = @"What food is in this image?
Return ONLY raw JSON, no markdown:
{
  ""foodName"": ""exact food name"",
  ""estimatedGrams"": 0
}";

            string raw = await SendImageRequestAsync(base64Image, prompt, mediaType);
            var json = JObject.Parse(raw);

            string foodName = json["foodName"]?.ToString() ?? "Unknown Food";
            int grams = json["estimatedGrams"]?.Value<int>() ?? 100;

            return (foodName, grams);
        }

        // ================================
        // ⚖️ გრამებით კვების გამოთვლა
        // ================================
        public async Task<FoodEntry> AnalyzeFoodWithGramsAsync(string foodName, int grams, string mealType)
        {
            string prompt = $@"Calculate exact nutrition for {grams}g of {foodName}.
Return ONLY raw JSON, no markdown:
{{
  ""foodName"": ""{foodName}"",
  ""calories"": 0,
  ""protein"": 0.0,
  ""carbohydrates"": 0.0,
  ""fats"": 0.0,
  ""fiber"": 0.0,
  ""sugar"": 0.0,
  ""sodium"": 0.0,
  ""calcium"": 0.0,
  ""iron"": 0.0,
  ""vitaminC"": 0.0,
  ""vitaminD"": 0.0,
  ""water"": 0.0
}}";

            string raw = await SendRequestAsync(prompt);
            var nutrition = JObject.Parse(raw);

            return new FoodEntry
            {
                FoodName = nutrition["foodName"]?.ToString() ?? foodName,
                MealType = mealType,
                Date = DateTime.Today,
                Calories = nutrition["calories"]?.Value<int>() ?? 0,
                Protein = nutrition["protein"]?.Value<double>() ?? 0,
                Carbohydrates = nutrition["carbohydrates"]?.Value<double>() ?? 0,
                Fats = nutrition["fats"]?.Value<double>() ?? 0,
                Fiber = nutrition["fiber"]?.Value<double>() ?? 0,
                Sugar = nutrition["sugar"]?.Value<double>() ?? 0,
                Sodium = nutrition["sodium"]?.Value<double>() ?? 0,
                Calcium = nutrition["calcium"]?.Value<double>() ?? 0,
                Iron = nutrition["iron"]?.Value<double>() ?? 0,
                VitaminC = nutrition["vitaminC"]?.Value<double>() ?? 0,
                VitaminD = nutrition["vitaminD"]?.Value<double>() ?? 0,
                Water = nutrition["water"]?.Value<double>() ?? 0
            };
        }

        // ================================
        // 💬 ნუტრიციის კითხვა
        // ================================
        public async Task<string> AskNutritionQuestionAsync(string question, DailyLog dailyLog)
        {
            string logSummary = $@"Today's nutrition summary:
- Calories: {dailyLog.TotalCalories} / {dailyLog.CalorieGoal} kcal
- Protein: {dailyLog.TotalProtein}g / {dailyLog.ProteinGoal}g
- Carbs: {dailyLog.TotalCarbs}g / {dailyLog.CarbsGoal}g
- Fats: {dailyLog.TotalFats}g / {dailyLog.FatsGoal}g
- Water: {dailyLog.TotalWater}ml / {dailyLog.WaterGoal}ml
- Calories burned: {dailyLog.TotalCaloriesBurned} kcal
- Net calories: {dailyLog.NetCalories} kcal
Foods eaten: {string.Join(", ", dailyLog.FoodEntries.Select(f => f.FoodName))}
Workouts: {string.Join(", ", dailyLog.WorkoutEntries.Select(w => w.WorkoutName))}";

            string prompt = $"You are a helpful nutrition and fitness coach. Here is the user's data:\n{logSummary}\n\nUser question: {question}\n\nGive a short, friendly, practical answer.";

            return await SendRequestAsync(prompt);
        }

        // ================================
        // 💪 ვარჯიშის გეგმის გენერაცია
        // ================================
        public async Task<string> GenerateWorkoutPlanAsync(
            string location, string goal, string equipment, int daysPerWeek, User profile)
        {
            string equipmentLine = string.IsNullOrWhiteSpace(equipment)
                ? "bodyweight only (no equipment)"
                : equipment;

            string prompt = $@"You are a professional personal trainer. Create a detailed {daysPerWeek}-day workout plan.

User profile:
- Location: {location}
- Goals: {goal}
- Available equipment: {equipmentLine}
- Weight: {profile.WeightKg} kg  |  Height: {profile.HeightCm} cm
- Gender: {profile.Gender}  |  Age: {profile.Age}
- Experience level: {profile.ActivityLevel}

For each day include:
- Day name and focus (e.g. Day 1 — Upper Body)
- Exercise list with sets × reps (or duration for cardio)
- Estimated session duration
- Brief tip or note for that day

Make exercises match the available equipment exactly. Do not suggest equipment the user doesn't have.
If multiple goals are selected, balance the plan to address all of them.";

            return await SendRequestAsync(prompt);
        }

        // ================================
        // 🏋️ ვარჯიშის კითხვა
        // ================================
        public async Task<string> AskWorkoutQuestionAsync(string question, string workoutPlan)
        {
            string prompt = $@"You are a professional fitness coach.
The user has the following workout plan:
{workoutPlan}

Answer the user's question. Be specific, practical, and concise.
User's question: {question}";

            return await SendRequestAsync(prompt);
        }

        // ================================
        // 🔥 კალორიების გამოთვლა
        // ================================
        public async Task<WorkoutEntry> EstimateCaloriesBurnedAsync(string workoutName, int duration, string intensity, User profile)
        {
            string prompt = $@"You are a fitness expert. Estimate calories burned.

User: {profile.WeightKg}kg, {profile.Age}yo, {profile.Gender}, level: {profile.ActivityLevel}
Workout: {workoutName}, {duration} minutes, intensity: {intensity}

Return ONLY raw JSON, no markdown, no backticks:
{{
  ""workoutName"": ""{workoutName}"",
  ""durationMinutes"": {duration},
  ""intensityLevel"": ""{intensity}"",
  ""caloriesBurned"": 0,
  ""notes"": """"
}}";

            try
            {
                string raw = await SendRequestAsync(prompt);
                var json = JObject.Parse(raw);

                return new WorkoutEntry
                {
                    WorkoutName = json["workoutName"]?.ToString() ?? workoutName,
                    DurationMinutes = json["durationMinutes"]?.Value<int>() ?? duration,
                    Intensity = json["intensityLevel"]?.ToString() ?? intensity,
                    CaloriesBurned = json["caloriesBurned"]?.Value<int>() ?? 0,
                    Notes = json["notes"]?.ToString() ?? "",
                    Date = DateTime.Today
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"კალორიების გამოთვლა ვერ მოხდა: {ex.Message}");
            }
        }

        // ================================
        // 🔒 პირადი მეთოდები (API კავშირი)
        // ================================
        private async Task<string> SendRequestAsync(string prompt)
        {
            var requestBody = new
            {
                model = MODEL,
                max_tokens = 1024,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            return await ExecutePostAsync(requestBody);
        }

        private async Task<string> SendImageRequestAsync(string base64Image, string prompt,
                                                          string mediaType = "image/jpeg")
        {
            var requestBody = new
            {
                model = MODEL,
                max_tokens = 1024,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "image",
                                source = new
                                {
                                    type = "base64",
                                    media_type = mediaType,
                                    data = base64Image
                                }
                            },
                            new
                            {
                                type = "text",
                                text = prompt
                            }
                        }
                    }
                }
            };

            return await ExecutePostAsync(requestBody);
        }

        // Strips ```json ... ``` or ``` ... ``` markdown fences Claude sometimes adds
        private static string StripMarkdown(string text)
        {
            text = text.Trim();
            if (text.StartsWith("```"))
            {
                int firstNewline = text.IndexOf('\n');
                if (firstNewline >= 0) text = text[(firstNewline + 1)..];
                if (text.EndsWith("```")) text = text[..^3];
            }
            return text.Trim();
        }

        private async Task<string> ExecutePostAsync(object payload)
        {
            string json = JsonConvert.SerializeObject(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(API_URL, content);

            string responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API Error: {responseString}");

            var responseJson = JObject.Parse(responseString);
            string text = responseJson["content"]?[0]?["text"]?.ToString()
                          ?? throw new Exception("Claude returned an empty response.");
            return StripMarkdown(text);
        }
    }
}