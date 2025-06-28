using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuizApplicationMVC.Data;
using QuizApplicationMVC.Models;
using Microsoft.AspNetCore.Http;
using QuizApplicationMVC.Services;

namespace QuizApplicationMVC.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly ApplicationDbContext _context;

        public UsersController(IUserRepository userRepository, IQuizRepository quizRepository, ApplicationDbContext context)
        {
            _userRepository = userRepository;
            _quizRepository = quizRepository;
            _context = context;
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home", null);
        }

        // GET: Users
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

            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                return users != null ? View(users) : Problem("Entity set 'ApplicationDBContext.Users' is null.");
            }
            catch (Exception ex)
            {
                return Problem("An error occurred while fetching users: " + ex.Message);
            }
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userRepository.GetUserByIdAsync((int)id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Email,Password")] Users users)
        {
            Console.WriteLine("Successfully Logged In");

            var userData = await _userRepository.AuthenticateUserAsync(users.Email, users.Password);

            if (userData == null)
            {
                ModelState.AddModelError("", "User not found. Please check your email and password.");
                return View();
            }

            HttpContext.Session.SetInt32("Id", userData.Id);
            Console.WriteLine(HttpContext.Session.GetInt32("Id"));

            return RedirectToAction("Index", "Home", null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,Password")] Users users)
        {
            if (ModelState.IsValid)
            {
                await _userRepository.AddUserAsync(users);
                return RedirectToAction("Login", "Users");
            }
            return View(users);
        }

        public async Task<IActionResult> QuizHistory()
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var userId = HttpContext.Session.GetInt32("Id");
            var quizHistory = await _quizRepository.GetQuizHistoryByUserIdAsync(userId.Value);
            quizHistory.Reverse();
            return View(quizHistory);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var users = await _userRepository.GetUserByIdAsync(id.Value);
            if (users == null)
            {
                return NotFound();
            }

            return View(users);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Password")] Users users)
        {
            if (id != users.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _userRepository.UpdateUserAsync(users);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_userRepository.UserExists(users.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = users.Id });
            }
            return View(users);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var users = await _userRepository.GetUserByIdAsync(id);
            if (users == null)
            {
                return NotFound();
            }

            return View(users);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return Problem("Entity set 'ApplicationDBContext.Users' is null.");
            }

            var quizzes = await _quizRepository.GetQuizzesByUserIdAsync(id);
            foreach (var quiz in quizzes)
            {
                await _quizRepository.DeleteQuizWithHistoryAsync(quiz.Id);
            }

            var userHistories = await _context.QuizUserHistory
                .Where(uh => uh.UserId == id)
                .ToListAsync();

            if (userHistories != null)
            {
                _context.QuizUserHistory.RemoveRange(userHistories);
                await _context.SaveChangesAsync();
            }

            await _userRepository.DeleteUserAsync(user.Id);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Users");
        }
    }
}
