namespace IphoneStoreFE.Models
{
    public class CreateOrderViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? Note { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public List<int> CartIds { get; set; } = new List<int>();
    }
}
