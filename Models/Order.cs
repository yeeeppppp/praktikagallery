using System;
using System.Collections.Generic;
using System.Linq;

namespace ArtGalleryStore.Models
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }

    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Total { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public string Artist { get; set; } = string.Empty;
        public string Medium { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        
        // Ссылка на исходный продукт (может быть null если продукт удален)
        public Product? Product { get; set; }
        
        public decimal Total => ProductPrice * Quantity;
    }
}