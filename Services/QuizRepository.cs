using Microsoft.EntityFrameworkCore;
using QuizApplicationMVC.Data;
using QuizApplicationMVC.Models;

namespace QuizApplicationMVC.Services
{
    public class QuizRepository : IQuizRepository
    {
        private readonly ApplicationDbContext _context;

        public QuizRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<QuizUserHistory>> GetQuizHistoryByUserIdAsync(int userId)
        {
            return await _context.QuizUserHistory
                .Include(qu => qu.Quiz)
                    .ThenInclude(quiz => quiz.Questions)
                .Where(qu => qu.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Quiz>> GetQuizzesWithQuestionsAsync()
        {
            var quizzes = await _context.Quiz.Include(q => q.Questions).ToListAsync();
            return quizzes.Where(q => q.Questions.Any()).ToList();
        }

        public async Task RemoveQuizzesAsync(IEnumerable<Quiz> quizzes)
        {
            if (quizzes.Any())
            {
                _context.Quiz.RemoveRange(quizzes);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Quiz>> GetQuizzesByUserIdAsync(int userId)
        {
            return await _context.Quiz
                .Where(q => q.UserId == userId)
                .Include(q => q.Questions)
                .ToListAsync();
        }

        public async Task<Quiz> GetQuizDetailsAsync(int? id)
        {
            return await _context.Quiz
                .Include(q => q.User)
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Quiz> CreateQuizAsync(Quiz quiz, int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                quiz.User = user;
                quiz.Questions = null;

                await _context.AddAsync(quiz);
                await _context.SaveChangesAsync();

                return quiz;
            }

            return null;
        }

        public async Task<Quiz> GetQuizByIdAsync(int id)
        {
            return await _context.Quiz
                .Include(q => q.User)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task UpdateQuizAsync(Quiz quiz)
        {
            _context.Update(quiz);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuizAsync(int id)
        {
            var quiz = await _context.Quiz.FindAsync(id);
            if (quiz != null)
            {
                _context.Quiz.Remove(quiz);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteQuizWithHistoryAsync(int id)
        {
            var quiz = await _context.Quiz
                .Include(q => q.Questions)
                .Include(q => q.quizUserHistory)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz != null)
            {
                if (quiz.quizUserHistory != null && quiz.quizUserHistory.Any())
                {
                    _context.QuizUserHistory.RemoveRange(quiz.quizUserHistory);
                }

                if (quiz.Questions != null && quiz.Questions.Any())
                {
                    _context.Questions.RemoveRange(quiz.Questions);
                }

                _context.Quiz.Remove(quiz);
                await _context.SaveChangesAsync();
            }
        }

        public bool QuizExists(int id)
        {
            return (_context.Quiz?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
