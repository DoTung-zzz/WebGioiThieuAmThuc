using Microsoft.EntityFrameworkCore;
using WebGioiThieuAmThuc.Data;
using WebGioiThieuAmThuc.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(); // Add Session service
builder.Services.AddHttpContextAccessor(); // Add HttpContextAccessor

// 🔥 Đăng ký DbContext theo connection string trong appsettings.json
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDbContext")));

var app = builder.Build();

// Seed Regions data if not exists
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    
    // Ensure database is created
    context.Database.EnsureCreated();
    
    // Seed Regions: Bắc (1), Trung (2), Nam (3)
    if (!context.Regions.Any())
    {
        // Use SET IDENTITY_INSERT to allow inserting specific IDs
        context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Regions ON");
        
        context.Regions.AddRange(
            new Region { RegionId = 1, RegionName = "Miền Bắc", Description = "Vùng miền Bắc Việt Nam" },
            new Region { RegionId = 2, RegionName = "Miền Trung", Description = "Vùng miền Trung Việt Nam" },
            new Region { RegionId = 3, RegionName = "Miền Nam", Description = "Vùng miền Nam Việt Nam" }
        );
        context.SaveChanges();
        
        context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Regions OFF");
    }
    else
    {
        // Check and update region names if needed (don't change IDs to avoid FK issues)
        var bac = context.Regions.FirstOrDefault(r => r.RegionId == 1);
        if (bac != null && bac.RegionName != "Miền Bắc")
        {
            bac.RegionName = "Miền Bắc";
            bac.Description = "Vùng miền Bắc Việt Nam";
        }
        else if (bac == null)
        {
            // Only add if ID 1 doesn't exist
            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Regions ON");
            context.Regions.Add(new Region { RegionId = 1, RegionName = "Miền Bắc", Description = "Vùng miền Bắc Việt Nam" });
            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Regions OFF");
        }
        
        var trung = context.Regions.FirstOrDefault(r => r.RegionId == 2);
        if (trung != null && trung.RegionName != "Miền Trung")
        {
            trung.RegionName = "Miền Trung";
            trung.Description = "Vùng miền Trung Việt Nam";
        }
        else if (trung == null)
        {
            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Regions ON");
            context.Regions.Add(new Region { RegionId = 2, RegionName = "Miền Trung", Description = "Vùng miền Trung Việt Nam" });
            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Regions OFF");
        }
        
        var nam = context.Regions.FirstOrDefault(r => r.RegionId == 3);
        if (nam != null && nam.RegionName != "Miền Nam")
        {
            nam.RegionName = "Miền Nam";
            nam.Description = "Vùng miền Nam Việt Nam";
        }
        else if (nam == null)
        {
            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Regions ON");
            context.Regions.Add(new Region { RegionId = 3, RegionName = "Miền Nam", Description = "Vùng miền Nam Việt Nam" });
            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Regions OFF");
        }
        
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession(); // Enable Session middleware
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
