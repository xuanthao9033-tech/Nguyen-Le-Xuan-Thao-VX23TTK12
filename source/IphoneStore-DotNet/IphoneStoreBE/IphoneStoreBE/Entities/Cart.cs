using IphoneStoreBE.Entities;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Cart")]   // 🟢 KHÔNG có "s"
public partial class Cart
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public int ProductId { get; set; }
    public int UserId { get; set; }

    public virtual Product? Product { get; set; }
    public virtual User? User { get; set; }
}
