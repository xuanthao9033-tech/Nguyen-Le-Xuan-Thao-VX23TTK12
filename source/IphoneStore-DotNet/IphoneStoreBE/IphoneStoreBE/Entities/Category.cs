﻿using System;
using System.Collections.Generic;

namespace IphoneStoreBE.Entities;

public partial class Category
{
    public int Id { get; set; }

    public string? CategoryName { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
