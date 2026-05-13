using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using Training_APP.Model;

namespace Training_APP.Service
{
    public class CloudService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string API_URL = "https://api.anthropic.com/v1/messages";
        private const string MODEL = "claude-opus-4-5";

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



        private async Task<string> SendRequestAsync(string prompt)
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
                content = prompt
            }
        }
            };
            string json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(API_URL, content);

            string responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API Error: {responseString}");

            var responseJson = JObject.Parse(responseString);
            return responseJson["content"][0]["text"].ToString();
        }

        private async Task<string> SendImageRequestAsync(string base64Image, string prompt)
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
                            media_type = "image/jpeg",
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

            string json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(API_URL, content);

            string responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API Error: {responseString}");

            var responseJson = JObject.Parse(responseString);
            return responseJson["content"][0]["text"].ToString();
        }

        public async Task<FoodEntry> AnalyzeFoodImageAsync(string imagePath, string mealType)
        {
            try
            {
                // ნაბიჯი 1: ფოტო → bytes → base64
                byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
                string base64Image = Convert.ToBase64String(imageBytes);

                // ნაბიჯი 2: გაგზავნა დამხმარე მეთოდით
                string textContent = await SendImageRequestAsync(base64Image, @"
Analyze this food image and return ONLY raw JSON, no markdown, no backticks:
{
  ""foodName"": ""სახელი"",
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
}");

                // ნაბიჯი 3: JSON დამუშავება
                var nutrition = JObject.Parse(textContent);

                // ნაბიჯი 4: FoodEntry დაბრუნება
                return new FoodEntry
                {
                    FoodName = nutrition["foodName"]?.ToString() ?? "Unknown Food",
                    MealType = mealType,
                    Date = DateTime.Today,
                    ImagePath = imagePath,
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
            catch (FileNotFoundException)
            {
                throw new Exception("ფოტო ვერ მოიძებნა!");
            }
            catch (HttpRequestException)
            {
                throw new Exception("ინტერნეტ კავშირი შეწყდა!");
            }
            catch (Exception ex)
            {
                throw new Exception($"შეცდომა: {ex.Message}");
            }
        }

        // ================================
        // 💬 ნუტრიციის კითხვა
        // ================================
        public async Task<string> AskNutritionQuestionAsync(string question, DailyLog dailyLog)
        {
            try
            {
                string logSummary = $@"
                           Today's nutrition summary:
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
            catch (HttpRequestException)
            {
                throw new Exception("ინტერნეტ კავშირი შეწყდა!");
            }
            catch (Exception ex)
            {
                throw new Exception($"შეცდომა: {ex.Message}");
            }
        }

        // ================================
        // 💪 ვარჯიშის გეგმის გენერაცია
        // ================================
        public async Task<string> GenerateWorkoutPlanAsync(string location, string goal, int daysPerWeek, User profile)
        {
            string prompt = $@"
               შენ ხარ პერსონალური ტრენერი.
               მომხმარებლის მონაცემები:
               - სად ვარჯიშობს: {location}
               - მიზანი: {goal}
               - კვირაში რამდენი დღე: {daysPerWeek}
               - წონა: {profile.WeightKg}კგ
               - სიმაღლე: {profile.HeightCm}სმ
               - სქესი: {profile.Gender}
               - ასაკი: {profile.Age}
               - გამოცდილება: {profile.ActivityLevel}
               - მიზანი: {profile.Goal}
               
               შექმენი {daysPerWeek}-დღიანი კვირის ვარჯიშის გეგმა.
               თითოეული დღისთვის დაწერე:
               - დღის სახელი
               - ვარჯიშების სია (სახელი, სეტი, გამეორება)
               - სავარაუდო ხანგრძლივობა
               ";


            return await SendRequestAsync(prompt);
        }

        // ================================
        // 🏋️ ვარჯიშის კითხვა
        // ================================
        public async Task<string> AskWorkoutQuestionAsync(string question, string workoutPlan)
        {




        }

        // ================================
        // 🔥 კალორიების გამოთვლა
        // ================================
        public async Task<WorkoutEntry> EstimateCaloriesBurnedAsync(string workoutName, int duration, string intensity, User profile)
        {

        }
    }
}