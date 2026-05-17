using System.Windows;
using Training_APP.Data;
using Training_APP.Service;

namespace Training_APP
{
    public partial class App : Application
    {
        // ყველა სერვისი გლობალურია — ნებისმიერი View-იდან ხელმისაწვდომი
        public static AppDbContext Database { get; private set; }
        public static UserService UserService { get; private set; }
        public static NutritionService NutritionService { get; private set; }
        public static WorkoutService WorkoutService { get; private set; }
        public static CloudService CloudService { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. DB კავშირი
            Database = new AppDbContext();
            Database.Database.EnsureCreated();

            // 2. სერვისები — ყველას ერთი და იგივე DB გადაეცემა
            UserService = new UserService(Database);
            NutritionService = new NutritionService(Database);
            WorkoutService = new WorkoutService(Database);

            // 3. Claude AI — აქ ჩასვი შენი გასაღები
            CloudService = new CloudService("sk-ant-...");
        }
    }
}