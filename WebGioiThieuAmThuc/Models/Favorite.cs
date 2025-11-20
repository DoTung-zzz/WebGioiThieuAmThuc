using System;
using System.Collections.Generic;

namespace WebGioiThieuAmThuc.Models;

public partial class Favorite
{
    public int FavoriteId { get; set; }

    public int UserId { get; set; }

    public int SpecialtyId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Specialty Specialty { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
