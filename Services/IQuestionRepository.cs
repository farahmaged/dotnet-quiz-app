using QuizApplicationMVC.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizApplicationMVC.Services
{
    public interface IQuestionRepository
    {
        Task<List<Questions>> GetQuestionsByQuizIdAsync(int quizId);
        Task<Questions> GetQuestionByIdAsync(int id);
        Task AddQuestionAsync(Questions question, int quizId);
        Task<Questions> UpdateQuestionAsync(Questions updatedQuestion);
        Task<Quiz?> GetQuizByIdAsync(int id);
        Task<bool> DeleteQuestionAsync(Questions question);
    }
}
