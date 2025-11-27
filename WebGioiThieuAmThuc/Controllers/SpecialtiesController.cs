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
    public class SpecialtiesController : Controller
    {
        private readonly MyDbContext _context;

        public SpecialtiesController(MyDbContext context)
        {
            _context = context;
        }

        // GET: Specialties
        public async Task<IActionResult> Index(string searchString, int? regionId)
        {
            var specialties = from s in _context.Specialties.Include(s => s.Region).Include(s => s.CreatedByNavigation)
                              select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                specialties = specialties.Where(s => s.Name.Contains(searchString) || s.ShortDescription.Contains(searchString) || s.Region.RegionName.Contains(searchString));
            }

            if (regionId.HasValue)
            {
                specialties = specialties.Where(s => s.RegionId == regionId.Value);
            }

            ViewData["Regions"] = await _context.Regions.ToListAsync();
            ViewData["CurrentRegion"] = regionId;

            return View(await specialties.ToListAsync());
        }

        // GET: Specialties/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var specialty = await _context.Specialties
                .Include(s => s.Region)
                .Include(s => s.CreatedByNavigation)
                .Include(s => s.Ratings).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.SpecialtyId == id);
            if (specialty == null)
            {
                return NotFound();
            }

            return View(specialty);
        }

        // GET: Specialties/Create
        public IActionResult Create()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            ViewData["RegionId"] = new SelectList(_context.Regions, "RegionId", "RegionName");
            return View();
        }

        // POST: Specialties/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SpecialtyId,RegionId,Name,ShortDescription,FullDescription,ImageUrl")] Specialty specialty, IFormFile? imageFile)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/specialties", fileName);
                    
                    // Ensure directory exists
                    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/specialties"));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    specialty.ImageUrl = "/images/specialties/" + fileName;
                }

                specialty.CreatedBy = int.Parse(userId);
                specialty.CreatedAt = DateTime.Now;
                specialty.Status = "pending"; // Default status

                _context.Add(specialty);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RegionId"] = new SelectList(_context.Regions, "RegionId", "RegionName", specialty.RegionId);
            return View(specialty);
        }

        // GET: Specialties/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty == null)
            {
                return NotFound();
            }

            // Check permission
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("Role"); // Assuming we store Role in session, need to update Login to store it.
            
            // For now, let's re-fetch user role if not in session or just rely on logic
            // But wait, Login logic in UsersController didn't store Role. I need to update UsersController later.
            // For now, I'll assume I will fix Login.
            
            if (userId == null) return RedirectToAction("Login", "Users");
            
            // Allow if owner or admin (we'll implement admin check properly later, for now just owner check)
            if (specialty.CreatedBy != int.Parse(userId))
            {
                 // If not owner, check if admin (will implement later)
                 // For now return Unauthorized or Redirect
                 return RedirectToAction("Index");
            }

            ViewData["RegionId"] = new SelectList(_context.Regions, "RegionId", "RegionName", specialty.RegionId);
            return View(specialty);
        }

        // POST: Specialties/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SpecialtyId,RegionId,Name,ShortDescription,FullDescription,ImageUrl,CreatedBy,CreatedAt,Status")] Specialty specialty)
        {
            if (id != specialty.SpecialtyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(specialty);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpecialtyExists(specialty.SpecialtyId))
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
            ViewData["RegionId"] = new SelectList(_context.Regions, "RegionId", "RegionName", specialty.RegionId);
            return View(specialty);
        }

        // GET: Specialties/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var specialty = await _context.Specialties
                .Include(s => s.Region)
                .Include(s => s.CreatedByNavigation)
                .FirstOrDefaultAsync(m => m.SpecialtyId == id);
            if (specialty == null)
            {
                return NotFound();
            }

            return View(specialty);
        }

        // POST: Specialties/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty != null)
            {
                _context.Specialties.Remove(specialty);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SpecialtyExists(int id)
        {
            return _context.Specialties.Any(e => e.SpecialtyId == id);
        }

        // POST: Specialties/AddReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int specialtyId, int rating, string comment)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var review = new Rating
            {
                SpecialtyId = specialtyId,
                UserId = int.Parse(userId),
                Rating1 = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _context.Ratings.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = specialtyId });
        }
        // POST: Specialties/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            // Check if admin (simple check for now, should be robust role check)
            // Assuming we will fix Login to store Role properly or re-fetch here
             var user = await _context.Users.FindAsync(int.Parse(userId));
             if (user == null || user.Role != "admin")
             {
                 return Unauthorized();
             }

            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty == null)
            {
                return NotFound();
            }

            specialty.Status = "approved";
            _context.Update(specialty);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Specialties/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
             var userId = HttpContext.Session.GetString("UserId");
             var user = await _context.Users.FindAsync(int.Parse(userId));
             if (user == null || user.Role != "admin")
             {
                 return Unauthorized();
             }

            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty == null)
            {
                return NotFound();
            }

            specialty.Status = "rejected";
            _context.Update(specialty);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
