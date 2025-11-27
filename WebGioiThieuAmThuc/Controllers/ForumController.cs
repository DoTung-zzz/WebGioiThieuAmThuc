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
            // Get all specialties (posts) ordered by newest first
            var posts = await _context.Specialties
                .Include(s => s.Region)
                .Include(s => s.CreatedByNavigation)
                .Include(s => s.Ratings)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(posts);
        }
    }
}
