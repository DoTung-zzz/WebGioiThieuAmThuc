using System;
using System.Collections.Generic;

namespace WebGioiThieuAmThuc.Models;

public partial class Specialty
{
    public int SpecialtyId { get; set; }

    public int RegionId { get; set; }

    public string Name { get; set; } = null!;

    public string? ShortDescription { get; set; }

    public string? FullDescription { get; set; }

    public string? ImageUrl { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Status { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual Region Region { get; set; } = null!;
}
