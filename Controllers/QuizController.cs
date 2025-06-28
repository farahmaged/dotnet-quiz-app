using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuizApplicationMVC.Data;
using QuizApplicationMVC.Models;
using QuizApplicationMVC.Services;

namespace QuizApplicationMVC.Controllers
{
    public class QuizController : Controller
    {
        private readonly IQuizRepository _quizRepository;
        private readonly ApplicationDbContext _context;

        public QuizController(IQuizRepository quizRepository, ApplicationDbContext context)
        {
            _quizRepository = quizRepository;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            if (TempData.ContainsKey("QuizEvaluated") && (bool)TempData["QuizEvaluated"])
            {
                TempData.Remove("QuizEvaluated");
            }

            var quizzesWithQuestions = await _quizRepository.GetQuizzesWithQuestionsAsync();
            var quizzesToRemove = quizzesWithQuestions.Where(q => !q.Questions.Any()).ToList();
            await _quizRepository.RemoveQuizzesAsync(quizzesToRemove);

            return View(quizzesWithQuestions);
        }

        public async Task<IActionResult> MyQuizes()
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            int userId = HttpContext.Session.GetInt32("Id") ?? 0;

            if (TempData.ContainsKey("QuizEvaluated") && (bool)TempData["QuizEvaluated"])
            {
                TempData.Remove("QuizEvaluated");
            }

            var quizzes = await _quizRepository.GetQuizzesByUserIdAsync(userId);

            foreach (var quiz in quizzes)
            {
                Console.WriteLine(quiz.UserId);
            }

            return quizzes != null ? View(quizzes) : Problem("No quizzes found for the user or the entity set is null.");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _quizRepository.GetQuizDetailsAsync(id);
            Console.WriteLine("quiz.User");
            Console.WriteLine(quiz?.User);

            if (quiz == null)
            {
                return NotFound();
            }

            Console.WriteLine($"Number of Questions: {quiz.Questions.Count}");
            return View(quiz);
        }

        [HttpPost, ActionName("Evaluate")]
        [ValidateAntiForgeryToken]
        public IActionResult Evaluate(List<QuizQuestion> quizQuestions)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            if (TempData.ContainsKey("QuizEvaluated") && (bool)TempData["QuizEvaluated"])
            {
                return RedirectToAction("Index", "Home");
            }

            if (quizQuestions == null || !quizQuestions.Any())
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                int quizId = -1;
                int correctAnswers = 0;

                foreach (var newQuestion in quizQuestions)
                {
                    var originalQuestion = _context.QuizQuestion.FirstOrDefault(q => q.Id == newQuestion.Id);
                    Console.WriteLine("newQuestion.Id");
                    Console.WriteLine(newQuestion.Id);

                    if (originalQuestion != null)
                    {
                        originalQuestion.SelectedOption = newQuestion.SelectedOption;
                        _context.SaveChanges();
                        Console.WriteLine(originalQuestion.SelectedOption);

                        if (newQuestion.SelectedOption == originalQuestion.CorrectOption)
                        {
                            correctAnswers++;
                        }

                        quizId = originalQuestion.QuizId;
                    }
                    else
                    {
                        Console.WriteLine("not found asjb");
                    }
                }

                TempData["QuizEvaluated"] = true;
                TempData["CorrectAnswersCount"] = correctAnswers;

                int userId = (int)HttpContext.Session.GetInt32("Id");
                Console.WriteLine("userId");
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                Console.WriteLine(quizId);
                var quiz = _context.Quiz.FirstOrDefault(q => q.Id == quizId);

                var quizUserHistory = new QuizUserHistory
                {
                    UserId = userId,
                    QuizId = quizId,
                    Score = correctAnswers,
                    DateTaken = DateTime.Now,
                    User = user,
                    Quiz = quiz
                };

                Console.WriteLine("Calculated");
                _context.QuizUserHistory.Add(quizUserHistory);
                _context.SaveChanges();

                return View("EvaluationResult", quizQuestions);
            }
            else
            {
                foreach (var key in ModelState.Keys)
                {
                    var modelStateEntry = ModelState[key];
                    foreach (var error in modelStateEntry.Errors)
                    {
                        var errorMessage = error.ErrorMessage;
                        Console.WriteLine($"Validation Error for {key}: {errorMessage}");
                    }
                }

                return View("Take", quizQuestions);
            }
        }

        public IActionResult EvaluationResult()
        {
            return View();
        }

        public async Task<IActionResult> Take(int? id)
        {
            if (id == null || _context.Quiz == null)
            {
                return NotFound();
            }

            if (TempData.ContainsKey("QuizEvaluated") && (bool)TempData["QuizEvaluated"])
            {
                TempData.Remove("QuizEvaluated");
                return RedirectToAction("Index", "Home");
            }

            var questions = await _context.Questions
                .Where(q => q.QuizId == id)
                .ToListAsync();

            Console.WriteLine("Questions count: " + questions.Count);

            if (questions == null || !questions.Any())
            {
                return NotFound();
            }

            var quizQuestions = questions.Select(q => new QuizQuestion
            {
                QuestionName = q.QuestionsName,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                SelectedOption = "",
                CorrectOption = q.CorrectOption,
                QuizId = (int)id
            }).ToList();

            Console.WriteLine(quizQuestions);

            foreach (var question in quizQuestions)
            {
                _context.Add(question);
            }

            await _context.SaveChangesAsync();

            var quiz = await _quizRepository.GetQuizByIdAsync((int)id);
            if (quiz == null)
            {
                return NotFound();
            }

            ViewBag.Duration = quiz.Duration;
            return View(quizQuestions);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Duration")] Quiz quiz)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var userId = (int)HttpContext.Session.GetInt32("Id");

            if (ModelState.IsValid)
            {
                var createdQuiz = await _quizRepository.CreateQuizAsync(quiz, userId);
                if (createdQuiz != null)
                {
                    HttpContext.Session.SetInt32("QuizId", createdQuiz.Id);
                    return RedirectToAction("Create", "Questions", null);
                }
                else
                {
                    Console.WriteLine("User not found for the provided ID.");
                }
            }
            else
            {
                Console.WriteLine("Validation Errors:");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
            }

            Console.WriteLine("Returning to Quiz creation view due to validation errors.");
            return View(quiz);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _quizRepository.GetQuizByIdAsync((int)id);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Duration")] Quiz quiz)
        {
            if (id != quiz.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingQuiz = await _quizRepository.GetQuizByIdAsync(id);
                    if (existingQuiz == null)
                    {
                        return NotFound();
                    }

                    existingQuiz.Title = quiz.Title;
                    existingQuiz.Description = quiz.Description;
                    existingQuiz.Duration = quiz.Duration;

                    await _quizRepository.UpdateQuizAsync(existingQuiz);
                    return RedirectToAction(nameof(MyQuizes));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (_quizRepository.QuizExists(quiz.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(quiz);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _quizRepository.GetQuizByIdAsync(id.Value);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!_quizRepository.QuizExists(id))
            {
                return NotFound();
            }

            await _quizRepository.DeleteQuizWithHistoryAsync(id);
            return RedirectToAction(nameof(MyQuizes));
        }
    }
}
