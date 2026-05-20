using Microsoft.EntityFrameworkCore;
using Training_APP.Data;
using Training_APP.Model;

namespace Training_APP.Service
{
    public class UserService
    {
        public async Task<User?> GetUserAsync()
        {
            using var db = new AppDbContext();
            return await db.Users.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task SaveUserAsync(User user)
        {
            user.RecalculateGoals();

            using var db = new AppDbContext();
            var existing = await db.Users.FirstOrDefaultAsync();

            if (existing == null)
            {
                db.Users.Add(user);
            }
            else
            {
                // Preserve the DB primary key, copy everything else
                user.Id = existing.Id;
                db.Entry(existing).CurrentValues.SetValues(user);
            }

            await db.SaveChangesAsync();
        }
    }
}
