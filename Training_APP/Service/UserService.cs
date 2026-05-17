using Microsoft.EntityFrameworkCore;
using Training_APP.Data;
using Training_APP.Model;

namespace Training_APP.Service
{
    public class UserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        // პირველი მომხმარებლის ჩატვირთვა
        public async Task<User?> GetUserAsync()
        {
            return await _db.Users.FirstOrDefaultAsync();
        }

        // მომხმარებლის შენახვა / განახლება
        public async Task SaveUserAsync(User user)
        {
            var existing = await _db.Users.FirstOrDefaultAsync();
            if (existing == null)
            {
                _db.Users.Add(user);
            }
            else
            {
                existing.Name = user.Name;
                existing.Age = user.Age;
                existing.WeightKg = user.WeightKg;
                existing.HeightCm = user.HeightCm;
                existing.Gender = user.Gender;
                existing.ActivityLevel = user.ActivityLevel;
                existing.Goal = user.Goal;
            }
            await _db.SaveChangesAsync();
        }
    }
}