using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuizApplicationMVC.Data;
using QuizApplicationMVC.Models;
using QuizApplicationMVC.Services;

namespace QuizApplicationMVC.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly IQuestionRepository _questionRepository;

        public QuestionsController(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }

        public async Task<IActionResult> Index(int quizId)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            HttpContext.Session.SetInt32("QuizId", quizId);
            int? currentQuizId = HttpContext.Session.GetInt32("QuizId");

            if (currentQuizId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var questions = await _questionRepository.GetQuestionsByQuizIdAsync(currentQuizId.Value);
            if (questions == null || !questions.Any())
            {
                return NotFound();
            }

            return View(questions);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _questionRepository.GetQuestionByIdAsync(id.Value);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            if (HttpContext.Session.GetInt32("QuizId") == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("QuestionsId,QuestionsName,OptionA,OptionB,OptionC,OptionD,CorrectOption")] Questions questions)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            if (HttpContext.Session.GetInt32("QuizId") == null)
            {
                return RedirectToAction("Index", "Home");
            }

            string COption = questions.CorrectOption;
            switch (questions.CorrectOption)
            {
                case "A":
                    COption = questions.OptionA;
                    break;
                case "B":
                    COption = questions.OptionB;
                    break;
                case "C":
                    COption = questions.OptionC;
                    break;
                case "D":
                    COption = questions.OptionD;
                    break;
            }

            questions.CorrectOption = COption;
            questions.QuizId = (int)HttpContext.Session.GetInt32("QuizId");

            if (ModelState.IsValid)
            {
                await _questionRepository.AddQuestionAsync(questions, questions.QuizId);
                return RedirectToAction("Index", "Questions", new { quizId = questions.QuizId });
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

            return View(questions);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questions = await _questionRepository.GetQuestionByIdAsync(id.Value);
            if (questions == null)
            {
                return NotFound();
            }

            return View(questions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,QuestionsName,OptionA,OptionB,OptionC,OptionD,CorrectOption")] Questions updatedQuestion)
        {
            if (id != updatedQuestion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var updateResult = await _questionRepository.UpdateQuestionAsync(updatedQuestion);
                if (updateResult != null)
                {
                    return RedirectToAction("Index", new { quizId = updateResult.QuizId });
                }
                else
                {
                    return NotFound();
                }
            }

            return View(updatedQuestion);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questions = await _questionRepository.GetQuestionByIdAsync(id.Value);
            if (questions == null)
            {
                return NotFound();
            }

            return View(questions);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await _questionRepository.GetQuestionByIdAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            int quizId = (int)HttpContext.Session.GetInt32("QuizId");
            var quiz = await _questionRepository.GetQuizByIdAsync(quizId);

            if (quiz != null)
            {
                quiz.Questions.Remove(question);
            }

            await _questionRepository.DeleteQuestionAsync(question);

            if (quiz?.Questions.Count == 0)
            {
                return RedirectToAction("MyQuizes", "Quiz", new { id = quizId });
            }

            return RedirectToAction("Index", new { quizId });
        }
    }
}
