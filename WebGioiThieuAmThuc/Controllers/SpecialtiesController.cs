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
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("Role");
            
            var specialties = from s in _context.Specialties.Include(s => s.Region).Include(s => s.CreatedByNavigation)
                              select s;

            // Admin can see all, members see only approved + their own pending/rejected
            if (userRole != "admin")
            {
                if (userId != null)
                {
                    var userIdInt = int.Parse(userId);
                    specialties = specialties.Where(s => s.Status == "approved" || s.CreatedBy == userIdInt);
                }
                else
                {
                    // Not logged in - only show approved
                    specialties = specialties.Where(s => s.Status == "approved");
                }
            }

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

            return View(await specialties.OrderByDescending(s => s.CreatedAt).ToListAsync());
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
        public async Task<IActionResult> Create()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Users");
            }

            // Get distinct regions ordered by Name to ensure no duplicates
            // Fix: Fetch all first, then group in memory to avoid EF Core translation error
            var allRegions = await _context.Regions.ToListAsync();
            var regions = allRegions
                .GroupBy(r => r.RegionName)
                .Select(g => g.First())
                .OrderBy(r => r.RegionName)
                .ToList();
            
            ViewData["RegionId"] = new SelectList(regions, "RegionId", "RegionName");
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

            // Remove Region from validation as it's a navigation property populated by EF
            ModelState.Remove("Region");

            if (ModelState.IsValid)
            {
                // Handle image upload - priority: file upload > URL
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ImageUrl", "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif, webp)");
                        ViewData["RegionId"] = new SelectList(_context.Regions, "RegionId", "RegionName", specialty.RegionId);
                        return View(specialty);
                    }

                    // Generate unique filename to avoid conflicts
                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "specialties");
                    
                    // Ensure directory exists
                    Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    
                    specialty.ImageUrl = "/images/specialties/" + uniqueFileName;
                }
                // If no file upload but URL is provided, use URL
                else if (!string.IsNullOrWhiteSpace(specialty.ImageUrl))
                {
                    // URL is already set, keep it
                }
                // If neither file nor URL, ImageUrl remains null

                specialty.CreatedBy = int.Parse(userId);
                specialty.CreatedAt = DateTime.Now;
                specialty.Status = "pending"; // Default status - needs admin approval

                _context.Add(specialty);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Get distinct regions ordered by Name to ensure no duplicates
            // Fix: Fetch all first, then group in memory
            var allRegions = await _context.Regions.ToListAsync();
            var regions = allRegions
                .GroupBy(r => r.RegionName)
                .Select(g => g.First())
                .OrderBy(r => r.RegionName)
                .ToList();
            
            ViewData["RegionId"] = new SelectList(regions, "RegionId", "RegionName", specialty.RegionId);
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
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var userRole = HttpContext.Session.GetString("Role");
            // Allow if owner or admin
            if (specialty.CreatedBy != int.Parse(userId) && userRole != "admin")
            {
                return Unauthorized();
            }

            // Get distinct regions ordered by Name to ensure no duplicates
            // Fix: Fetch all first, then group in memory
            var allRegions = await _context.Regions.ToListAsync();
            var regions = allRegions
                .GroupBy(r => r.RegionName)
                .Select(g => g.First())
                .OrderBy(r => r.RegionName)
                .ToList();
            
            ViewData["RegionId"] = new SelectList(regions, "RegionId", "RegionName", specialty.RegionId);
            return View(specialty);
        }

        // POST: Specialties/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SpecialtyId,RegionId,Name,ShortDescription,FullDescription,ImageUrl,CreatedBy,CreatedAt,Status")] Specialty specialty, IFormFile? imageFile)
        {
            if (id != specialty.SpecialtyId)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            // Check permission
            var existingSpecialty = await _context.Specialties.FindAsync(id);
            if (existingSpecialty == null)
            {
                return NotFound();
            }

            var userRole = HttpContext.Session.GetString("Role");
            if (existingSpecialty.CreatedBy != int.Parse(userId) && userRole != "admin")
            {
                return Unauthorized();
            }

            // Remove Region from validation
            ModelState.Remove("Region");

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Validate file type
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("ImageUrl", "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif, webp)");
                            ViewData["RegionId"] = new SelectList(_context.Regions, "RegionId", "RegionName", specialty.RegionId);
                            return View(specialty);
                        }

                        // Delete old image if it's a local file
                        if (!string.IsNullOrEmpty(existingSpecialty.ImageUrl) && existingSpecialty.ImageUrl.StartsWith("/images/"))
                        {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingSpecialty.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Generate unique filename
                        var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "specialties");
                        Directory.CreateDirectory(uploadsFolder);

                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        
                        specialty.ImageUrl = "/images/specialties/" + uniqueFileName;
                    }
                    // If no new file but URL is provided, use URL
                    else if (!string.IsNullOrWhiteSpace(specialty.ImageUrl))
                    {
                        // If changing from local file to URL, delete old file
                        if (!string.IsNullOrEmpty(existingSpecialty.ImageUrl) && existingSpecialty.ImageUrl.StartsWith("/images/") && !specialty.ImageUrl.StartsWith("/images/"))
                        {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingSpecialty.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                        // URL is already set, keep it
                    }

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
            // Get distinct regions ordered by Name to ensure no duplicates
            var regions = _context.Regions
                .GroupBy(r => r.RegionName)
                .Select(g => g.First())
                .OrderBy(r => r.RegionName)
                .ToList();
            
            ViewData["RegionId"] = new SelectList(regions, "RegionId", "RegionName", specialty.RegionId);
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
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }
            
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

            return RedirectToAction("PendingPosts");
        }

        // POST: Specialties/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
             var userId = HttpContext.Session.GetString("UserId");
             if (userId == null)
             {
                 return RedirectToAction("Login", "Users");
             }
             
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

            return RedirectToAction("PendingPosts");
        }

        // GET: Specialties/PendingPosts - Admin review page
        public async Task<IActionResult> PendingPosts()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null || user.Role != "admin")
            {
                return Unauthorized();
            }

            var pendingSpecialties = await _context.Specialties
                .Include(s => s.Region)
                .Include(s => s.CreatedByNavigation)
                .Where(s => s.Status == "pending")
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(pendingSpecialties);
        }
    }
}
