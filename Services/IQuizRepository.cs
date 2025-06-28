using QuizApplicationMVC.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizApplicationMVC.Services
{
    public interface IQuizRepository
    {
        Task<List<QuizUserHistory>> GetQuizHistoryByUserIdAsync(int userId);
        Task<List<Quiz>> GetQuizzesWithQuestionsAsync();
        Task RemoveQuizzesAsync(IEnumerable<Quiz> quizzes);
        Task<List<Quiz>> GetQuizzesByUserIdAsync(int userId);
        Task<Quiz> GetQuizDetailsAsync(int? id);
        Task<Quiz> CreateQuizAsync(Quiz quiz, int userId);
        Task<Quiz> GetQuizByIdAsync(int id);
        Task UpdateQuizAsync(Quiz quiz);
        Task DeleteQuizWithHistoryAsync(int id);
        bool QuizExists(int id);
    }
}
