using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebGioiThieuAmThuc.Models;

namespace WebGioiThieuAmThuc.Data;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<Region> Regions { get; set; }

    public virtual DbSet<Specialty> Specialties { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=TUNG2K4;Database=DacSanVungMienDB;User Id=sa;Password=123456;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.FavoriteId).HasName("PK__Favorite__46ACF4CBC5790764");

            entity.HasIndex(e => e.UserId, "IX_Favorites_User");

            entity.Property(e => e.FavoriteId).HasColumnName("favorite_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.SpecialtyId).HasColumnName("specialty_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Specialty).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.SpecialtyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Favorites_Specialties");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Favorites_Users");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.RatingId).HasName("PK__Ratings__D35B278B0D6589A6");

            entity.HasIndex(e => e.SpecialtyId, "IX_Ratings_Specialty");

            entity.Property(e => e.RatingId).HasColumnName("rating_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Rating1).HasColumnName("rating");
            entity.Property(e => e.SpecialtyId).HasColumnName("specialty_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Specialty).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.SpecialtyId)
                .HasConstraintName("FK_Ratings_Specialties");

            entity.HasOne(d => d.User).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Ratings_Users");
        });

        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.RegionId).HasName("PK__Regions__01146BAE6E3C8104");

            entity.Property(e => e.RegionId).HasColumnName("region_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.RegionName)
                .HasMaxLength(100)
                .HasColumnName("region_name");
        });

        modelBuilder.Entity<Specialty>(entity =>
        {
            entity.HasKey(e => e.SpecialtyId).HasName("PK__Specialt__B90D8D12B226448A");

            entity.HasIndex(e => e.RegionId, "IX_Specialties_Region");

            entity.Property(e => e.SpecialtyId).HasColumnName("specialty_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.FullDescription).HasColumnName("full_description");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("image_url");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.RegionId).HasColumnName("region_id");
            entity.Property(e => e.ShortDescription)
                .HasMaxLength(500)
                .HasColumnName("short_description");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("approved")
                .HasColumnName("status");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Specialties)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Specialties_Users");

            entity.HasOne(d => d.Region).WithMany(p => p.Specialties)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Specialties_Regions");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370F0BC353E0");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E6164E0CEB30B").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__Users__F3DBC572A191AE1E").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(100)
                .HasColumnName("fullname");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("member")
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasDefaultValue(true)
                .HasColumnName("status");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
