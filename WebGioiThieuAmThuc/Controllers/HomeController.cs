using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebGioiThieuAmThuc.Data;
using WebGioiThieuAmThuc.Models;

namespace WebGioiThieuAmThuc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MyDbContext _context;

        public HomeController(ILogger<HomeController> logger, MyDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get featured specialties (only approved, top 6 by rating or newest)
            var featuredSpecialties = await _context.Specialties
                .Include(s => s.Region)
                .Include(s => s.Ratings)
                .Where(s => s.Status == "approved")
                .OrderByDescending(s => s.CreatedAt)
                .Take(6)
                .ToListAsync();

            ViewData["FeaturedSpecialties"] = featuredSpecialties;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
