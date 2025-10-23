using System;
using System.Collections.Generic;

namespace IphoneStoreBE.Entities;

public partial class ProductImage
{
    public int Id { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? AltText { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public int? ProductId { get; set; }

    public virtual Product? Product { get; set; }
}
