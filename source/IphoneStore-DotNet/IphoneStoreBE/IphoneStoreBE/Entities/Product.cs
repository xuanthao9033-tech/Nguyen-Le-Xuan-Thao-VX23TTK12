using System;
using System.Collections.Generic;

namespace IphoneStoreBE.Entities;

public partial class Product
{
    public int Id { get; set; }

    public string ProductName { get; set; } = null!;

    public string Sku { get; set; } = null!;

    public decimal Price { get; set; }

    public string? SpecificationDescription { get; set; }

    public string? Warranty { get; set; }

    public string? ProductType { get; set; }

    public string? Color { get; set; }

    public string? Capacity { get; set; }
    public string? ImageUrl { get; set; }


    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public int? CategoryId { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
