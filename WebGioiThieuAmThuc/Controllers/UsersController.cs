using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebGioiThieuAmThuc.Data;
using WebGioiThieuAmThuc.Models;

namespace WebGioiThieuAmThuc.Controllers
{
    public class UsersController : Controller
    {
        private readonly MyDbContext _context;

        public UsersController(MyDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
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

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Username,PasswordHash,Fullname,Email,Phone,Role,CreatedAt,Status")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Username,PasswordHash,Fullname,Email,Phone,Role,CreatedAt,Status")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

        // GET: Users/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Username,PasswordHash,Fullname,Email,Phone")] User user)
        {
            if (ModelState.IsValid)
            {
                if (UserExists(user.Username)) // Check if username exists
                {
                     ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                     return View(user);
                }

                // Check if email exists (hash it first to compare)
                string hashedEmail = ComputeSha256Hash(user.Email);
                if (_context.Users.Any(u => u.Email == hashedEmail))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(user);
                }

                user.CreatedAt = DateTime.Now;
                user.Status = true;
                user.Role = "member"; // Default role
                
                // Hash sensitive data
                user.PasswordHash = ComputeSha256Hash(user.PasswordHash);
                user.Email = hashedEmail;

                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Login));
            }
            return View(user);
        }

        private bool UserExists(string username)
        {
            return _context.Users.Any(e => e.Username == username);
        }

        // GET: Users/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (ModelState.IsValid)
            {
                string hashedPassword = ComputeSha256Hash(password);
                // Find user by username first
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (user != null)
                {
                    bool isValid = false;
                    // 1. Check if password matches hash
                    if (user.PasswordHash == hashedPassword)
                    {
                        isValid = true;
                    }
                    // 2. Check if password matches plain text (Legacy support)
                    else if (user.PasswordHash == password)
                    {
                        isValid = true;
                        // Auto-migrate to hash
                        user.PasswordHash = hashedPassword;
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                    }

                    if (isValid)
                    {
                    // Store user info in Session
                    HttpContext.Session.SetString("UserId", user.UserId.ToString());
                    HttpContext.Session.SetString("Username", user.Username);
                    if (!string.IsNullOrEmpty(user.Fullname))
                    {
                         HttpContext.Session.SetString("Fullname", user.Fullname);
                    }
                    if (!string.IsNullOrEmpty(user.Role))
                    {
                        HttpContext.Session.SetString("Role", user.Role);
                    }

                    return RedirectToAction("Index", "Home");
                }
                }
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
            }
            return View();
        }

        public IActionResult Logout()
        {
            // Clear session
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        // GET: Users/Profile
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile([Bind("UserId,Username,Fullname,Email,Phone")] User user)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null || user.UserId != int.Parse(userId))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(user.UserId);
                    if (existingUser == null) return NotFound();

                    existingUser.Fullname = user.Fullname;
                    if (existingUser.Email != user.Email)
                    {
                         string hashedNewEmail = ComputeSha256Hash(user.Email);
                         if (_context.Users.Any(u => u.Email == hashedNewEmail && u.UserId != user.UserId))
                         {
                             ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                             return View(user);
                         }
                         existingUser.Email = hashedNewEmail;
                    }
                    
                    existingUser.Phone = user.Phone;

                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Profile));
            }
            return View(user);
        }

        // GET: Users/ChangePassword
        public IActionResult ChangePassword()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        // POST: Users/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu mới và xác nhận không khớp.";
                return View();
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            string hashedCurrentPassword = ComputeSha256Hash(currentPassword);

            if (user == null || user.PasswordHash != hashedCurrentPassword)
            {
                ViewBag.Error = "Mật khẩu hiện tại không đúng.";
                return View();
            }

            user.PasswordHash = ComputeSha256Hash(newPassword);
            _context.Update(user);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Đổi mật khẩu thành công!";
            return View();
        }

        // GET: Users/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Users/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string username, string email, string newPassword)
        {
            string hashedEmail = ComputeSha256Hash(email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.Email == hashedEmail);
            
            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc email không đúng.";
                return View();
            }

            user.PasswordHash = ComputeSha256Hash(newPassword);
            _context.Update(user);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Đặt lại mật khẩu thành công!";
            return View();
        }

        // Helper method for SHA256 hashing
        private static string ComputeSha256Hash(string rawData)
        {
            using (System.Security.Cryptography.SHA256 sha256Hash = System.Security.Cryptography.SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
