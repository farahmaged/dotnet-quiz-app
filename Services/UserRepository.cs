using Microsoft.EntityFrameworkCore;
using QuizApplicationMVC.Data;
using QuizApplicationMVC.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizApplicationMVC.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Users>> GetAllUsersAsync()
        {
            return await _context.Users.Include(u => u.quizzes).ToListAsync();
        }

        public async Task<Users> GetUserByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Users> AuthenticateUserAsync(string email, string password)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
        }

        public async Task AddUserAsync(Users user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(Users user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
