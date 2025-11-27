using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebGioiThieuAmThuc.Data;
using WebGioiThieuAmThuc.Models;

namespace WebGioiThieuAmThuc.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly MyDbContext _context;

        public FavoritesController(MyDbContext context)
        {
            _context = context;
        }

        // GET: Favorites
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var favorites = await _context.Favorites
                .Include(f => f.Specialty)
                    .ThenInclude(s => s.Region)
                .Include(f => f.Specialty)
                    .ThenInclude(s => s.Ratings)
                .Where(f => f.UserId == int.Parse(userId))
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(favorites);
        }

        // POST: Favorites/Add
        [HttpPost]
        public async Task<IActionResult> Add(int specialtyId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var userIdInt = int.Parse(userId);

            // Check if already favorited
            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userIdInt && f.SpecialtyId == specialtyId);

            if (existing != null)
            {
                return Json(new { success = false, message = "Đã có trong danh sách yêu thích" });
            }

            var favorite = new Favorite
            {
                UserId = userIdInt,
                SpecialtyId = specialtyId,
                CreatedAt = DateTime.Now
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã thêm vào yêu thích" });
        }

        // POST: Favorites/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int specialtyId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var userIdInt = int.Parse(userId);

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userIdInt && f.SpecialtyId == specialtyId);

            if (favorite == null)
            {
                return Json(new { success = false, message = "Không tìm thấy" });
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa khỏi yêu thích" });
        }

        // GET: Check if favorited
        [HttpGet]
        public async Task<IActionResult> Check(int specialtyId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return Json(new { isFavorited = false });
            }

            var userIdInt = int.Parse(userId);
            var isFavorited = await _context.Favorites
                .AnyAsync(f => f.UserId == userIdInt && f.SpecialtyId == specialtyId);

            return Json(new { isFavorited });
        }
    }
}
