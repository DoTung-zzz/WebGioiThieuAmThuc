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
    public class ForumController : Controller
    {
        private readonly MyDbContext _context;

        public ForumController(MyDbContext context)
        {
            _context = context;
        }

        // GET: Forum
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("Role");
            
            // Get all specialties (posts) ordered by newest first
            var posts = _context.Specialties
                .Include(s => s.Region)
                .Include(s => s.CreatedByNavigation)
                .Include(s => s.Ratings)
                .AsQueryable();

            // Admin can see all, members see only approved + their own pending/rejected
            if (userRole != "admin")
            {
                if (userId != null)
                {
                    var userIdInt = int.Parse(userId);
                    posts = posts.Where(s => s.Status == "approved" || s.CreatedBy == userIdInt);
                }
                else
                {
                    // Not logged in - only show approved
                    posts = posts.Where(s => s.Status == "approved");
                }
            }

            return View(await posts.OrderByDescending(s => s.CreatedAt).ToListAsync());
        }
    }
}
