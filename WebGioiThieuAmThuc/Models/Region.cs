using System;
using System.Collections.Generic;

namespace WebGioiThieuAmThuc.Models;

public partial class Region
{
    public int RegionId { get; set; }

    public string RegionName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Specialty> Specialties { get; set; } = new List<Specialty>();
}
