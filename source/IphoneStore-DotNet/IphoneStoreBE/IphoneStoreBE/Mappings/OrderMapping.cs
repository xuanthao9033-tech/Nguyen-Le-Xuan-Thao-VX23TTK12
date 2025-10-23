using IphoneStoreBE.Context;
using IphoneStoreBE.Entities;
using IphoneStoreBE.VModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IphoneStoreBE.Mappings
{
    public static class OrderExtensions
    {
        // 🟢 Tạo OrderAddress từ OrderCreateVModel
        public static OrderAddress CreateOrderAddress(this OrderCreateVModel model, int userId)
        {
            return new OrderAddress
            {
                UserId = userId,
                Recipient = model.Recipient,
                PhoneNumber = model.PhoneNumber,
                AddressDetailRecipient = model.AddressDetailRecipient,
                City = model.City,
                District = model.District,
                Ward = model.Ward,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = $"User_{userId}",
                IsActive = true
            };
        }

        // 🟡 Tạo Order từ OrderCreateVModel
        public static Order ToEntity(this OrderCreateVModel model, int userId, decimal total, string orderCode, int orderAddressId)
        {
            return new Order
            {
                UserId = userId,
                OrderCode = orderCode,
                OrderDate = DateTime.UtcNow,
                Total = total,
                ShippingPrice = model.ShippingPrice,
                PaymentMethod = model.PaymentMethod,
                OrderStatus = "Chờ xác nhận",
                OrderAddId = orderAddressId,
                CreatedBy = $"User_{userId}",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
        }

        // 🔵 Cập nhật trạng thái đơn hàng
        public static void UpdateEntity(this Order order, OrderUpdateVModel model)
        {
            order.OrderStatus = model.OrderStatus;
            order.UpdatedBy = "Admin";
            order.UpdatedDate = DateTime.UtcNow;
        }

        // 🟣 Convert entity sang GetVModel
        public static OrderGetVModel ToGetVModel(this Order order, IphoneStoreContext context)
        {
            var address = context.OrderAddresses.FirstOrDefault(a => a.Id == (order.OrderAddId ?? 0));

            var details = context.OrderDetails
                .Where(d => d.OrderId == order.Id)
                .Select(d => new OrderDetailGetVModel
                {
                    ProductId = d.ProductId ?? 0,
                    ProductName = context.Products
                        .Where(p => p.Id == d.ProductId)
                        .Select(p => p.ProductName)
                        .FirstOrDefault() ?? "Sản phẩm không xác định",
                    Quantity = d.Quantity,
                    Price = d.Price
                }).ToList();

            return new OrderGetVModel
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                OrderDate = order.OrderDate,
                Total = order.Total ?? 0m,
                ShippingPrice = order.ShippingPrice,
                PaymentMethod = order.PaymentMethod,
                OrderStatus = order.OrderStatus,
                OrderAddress = address != null ? new OrderAddressGetVModel
                {
                    Recipient = address.Recipient,
                    PhoneNumber = address.PhoneNumber,
                    AddressDetailRecipient = address.AddressDetailRecipient,
                    City = address.City,
                    District = address.District,
                    Ward = address.Ward
                } : null,
                OrderDetails = details
            };
        }
    }
}
